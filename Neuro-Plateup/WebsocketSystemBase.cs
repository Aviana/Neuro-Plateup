using Kitchen;
using KitchenMods;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WebSocketSharp;
using Newtonsoft.Json;
using UnityEngine;

namespace Neuro_Plateup
{
    public abstract class WebsocketSystemBase : GenericSystemBase, IModInitializer, IModSystem
    {
        private struct WebSocketEntry
        {
            public string Url { get; }
            public string Name { get; }

            public WebSocketEntry(string url, string name)
            {
                Url = url;
                Name = name;
            }
        }

        private class WebSocketClient
        {
            public Thread Thread;
            public WebSocket Socket;
            public readonly BlockingCollection<string> OutgoingMessages = new BlockingCollection<string>(new ConcurrentQueue<string>());
        }

        private static Dictionary<int, WebSocketEntry> _clientConfigs;
        private static readonly ConcurrentDictionary<int, WebSocketClient> _clients = new ConcurrentDictionary<int, WebSocketClient>();
        private static readonly ConcurrentQueue<(int, string)> _mainThreadQueue = new ConcurrentQueue<(int, string)>();
        private volatile bool _isRunning = false;
        private static bool _isInitialized = false;

        private static bool IsValidWebSocketUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == "ws" || uriResult.Scheme == "wss");
        }

        private static Dictionary<int, WebSocketEntry> ReadWebSocketCsv(string filePath)
        {
            var result = new Dictionary<int, WebSocketEntry>();
            using (var reader = new StreamReader(filePath))
            {
                int lineNumber = 1;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    if (parts.Length != 2)
                    {
                        Debug.LogError($"[Config] Invalid CSV format at line {lineNumber}: \"{line}\"");
                        lineNumber++;
                        continue;
                    }

                    string url = parts[0].Trim();
                    if (!IsValidWebSocketUrl(url))
                    {
                        Debug.LogError($"[Config] Invalid ws url at line {lineNumber}: \"{line}\"");
                        lineNumber++;
                        continue;
                    }

                    string name = parts[1].Trim();
                    if (name.Length > 20 || name.Length < 1)
                    {
                        Debug.LogError($"[Config] Invalid profile name length at line {lineNumber}: \"{line}\"");
                        lineNumber++;
                        continue;
                    }

                    result[lineNumber] = new WebSocketEntry(url, name);
                    lineNumber++;
                }
            }
            return result;
        }

        public void PostActivate(Mod mod)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            _clientConfigs = ReadWebSocketCsv(System.Windows.Forms.Application.StartupPath + "\\Mods\\Neuro-Plateup\\bots.csv");

            _isRunning = true;

            foreach (var kvp in _clientConfigs)
            {
                int id = kvp.Key;
                string url = kvp.Value.Url;

                var wsClient = new WebSocketClient();
                _clients[id] = wsClient;

                var thread = new Thread(() => WebSocketWorker(id, url));
                thread.IsBackground = true;
                wsClient.Thread = thread;
                thread.Start();
            }
        }

        private void WebSocketWorker(int id, string url)
        {
            if (!_clients.TryGetValue(id, out var client))
                return;

            while (_isRunning)
            {
                WebSocket ws = null;
                try
                {
                    ws = new WebSocket(url);
                    client.Socket = ws;

                    ws.OnOpen += (_, __) =>
                    {
                        Debug.Log($"[WebSocket {id}] Connected");
                        _mainThreadQueue.Enqueue((id, "clear"));
                        EnqueueMessage(id, new NeuroAPI.Startup());
                    };

                    ws.OnMessage += (_, e) =>
                        _mainThreadQueue.Enqueue((id, e.Data));

                    ws.OnClose += (_, __) =>
                    {
                        Debug.Log($"[WebSocket {id}] Disconnected");
                        client.OutgoingMessages.Add(string.Empty);
                    };

                    ws.OnError += (_, e) =>
                    {
                        Debug.Log($"[WebSocket {id}] Error: {e.Message}");
                        client.OutgoingMessages.Add(string.Empty);
                    };

                    ws.Connect();

                    while (_isRunning)
                    {
                        string msg;
                        try
                        {
                            msg = client.OutgoingMessages.Take();
                        }
                        catch (InvalidOperationException)
                        {
                            break;
                        }

                        try
                        {
                            ws.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WebSocket {id}] Send error: {ex.Message}");
                            while (client.OutgoingMessages.TryTake(out _)) { }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log($"[WebSocket {id}] Exception: {ex.Message}");
                }

                if (_isRunning)
                {
                    Debug.Log($"[WebSocket {id}] Reconnecting in 2s...");
                    Thread.Sleep(2000);
                }
            }
        }

        public void EnqueueMessage(int id, object obj)
        {
            if (_clients.TryGetValue(id, out var client) && client.Socket.IsAlive)
            {
                string message = JsonConvert.SerializeObject(obj, Formatting.Indented);
                client.OutgoingMessages.Add(message);
            }
        }

        private void ShutdownClients()
        {
            _isRunning = false;

            foreach (var kvp in _clients)
            {
                var client = kvp.Value;

                try
                {
                    client.OutgoingMessages.CompleteAdding();
                    client.Socket?.Close();
                    client.Thread?.Join();
                }
                catch { }
            }
        }

        public void PreInject() { }
        public void PostInject() { }
        protected override void Initialise() => base.Initialise();

        protected abstract void OnAction(int id, string payload);
        protected abstract void ClearActions(int id);

        protected override void OnUpdate()
        {
            while (_mainThreadQueue.TryDequeue(out var obj))
            {
                if (obj.Item2 == "clear")
                    ClearActions(obj.Item1);
                else
                    OnAction(obj.Item1, obj.Item2);
            }
        }

        public Dictionary<int, string> GetClients()
        {
            var clients = new Dictionary<int, string>();
            foreach (var entry in _clientConfigs)
            {
                clients[entry.Key] = entry.Value.Name;
            }
            return clients;
        }
    }
}

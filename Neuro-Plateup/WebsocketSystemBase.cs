using Kitchen;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Windows.Forms;

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
            public ConcurrentQueue<string> OutgoingMessages = new ConcurrentQueue<string>();
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
                    }

                    string url = parts[0].Trim();
                    if (!IsValidWebSocketUrl(url))
                    {
                        Debug.LogError($"[Config] Invalid ws url at line {lineNumber}: \"{line}\"");
                    }
                    string name = parts[1].Trim();
                    if (name.Length > 20 || name.Length < 1)
                    {
                        Debug.LogError($"[Config] Invalid profile name lenght at line {lineNumber}: \"{line}\"");
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

            _clientConfigs = ReadWebSocketCsv(Application.StartupPath + "\\Mods\\Neuro-Plateup\\bots.csv");

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
            try
            {
                var ws = new WebSocket(url);

                ws.OnMessage += (sender, e) =>
                {
                    _mainThreadQueue.Enqueue((id, e.Data));
                };

                ws.OnOpen += (sender, e) =>
                {
                    Console.WriteLine($"[WebSocket {id}] Connected.");
                    EnqueueMessage(id, new NeuroAPI.Startup());
                };
                ws.OnClose += (sender, e) => Console.WriteLine($"[WebSocket {id}] Disconnected.");
                ws.OnError += (sender, e) => Console.WriteLine($"[WebSocket {id}] Error: {e.Message}");

                if (!_clients.TryGetValue(id, out var client))
                {
                    return;
                }
                client.Socket = ws;

                ws.Connect();

                while (_isRunning && ws.IsAlive)
                {
                    while (client.OutgoingMessages.TryDequeue(out var msg))
                    {
                        try
                        {
                            ws.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WebSocket {id}] Send error: {ex.Message}");
                        }
                    }

                    Thread.Sleep(100);
                }

                ws.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WebSocket {id}] Exception: {ex.Message}");
            }
        }

        public void EnqueueMessage(int id, object obj)
        {
            if (_clients.TryGetValue(id, out var client))
            {
                string message = JsonConvert.SerializeObject(obj, Formatting.Indented);
                client.OutgoingMessages.Enqueue(message);
            }
            else
            {
                Console.WriteLine($"[WebSocket {id}] No client found to send message.");
            }
        }

        private void ShutdownClients()
        {
            _isRunning = false;

            foreach (var kvp in _clients)
            {
                try
                {
                    kvp.Value.Socket?.Close();
                    kvp.Value.Thread?.Join();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Shutdown] Error stopping client {kvp.Key}: {ex.Message}");
                }
            }
        }

        public void PreInject()
        {

        }

        public void PostInject()
        {

        }

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected abstract void OnAction(int id, string payload);

        protected override void OnUpdate()
        {
            while (_mainThreadQueue.TryDequeue(out var item))
            {
                OnAction(item.Item1, item.Item2);
            }
        }

        public Dictionary<int, string> GetClients()
        {
            var clients = new Dictionary<int, string>();
            foreach (var entry in _clientConfigs)
            {
                if (_clients[entry.Key].Socket.IsAlive)
                {
                    clients[entry.Key] = entry.Value.Name;
                }
            }
            return clients;
        }
    }
}
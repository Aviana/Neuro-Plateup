using Kitchen;
using KitchenMods;
using Unity.Entities;
using Unity.Collections;
using Controllers;
using System.Collections.Generic;
using System.Reflection;

namespace Neuro_Plateup
{
    public class BotHandler : GenericSystemBase, IModSystem
    {
        private EntityQuery botQuery, playerQuery;
        bool isInitialized = false;
        private FakeInput input;
        private InputUpdateEvent evt;
        public static Dictionary<int, BotRole> currentRoles = new Dictionary<int, BotRole>();

        protected override void Initialise()
        {
            base.Initialise();

            botQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl)
                    ));
            playerQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPlayer)
                    ).None(
                        typeof(CBotControl)
                    ));
            input = new FakeInput();
            evt = new InputUpdateEvent
            {
                User = 0,
                State = new InputState
                {
                    Request = GameStateRequest.InstantJoin,
                    InteractAction = ButtonState.Pressed
                },
            };
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            Debug.Log("Started running!");

            // Provide access to the username
            var field = typeof(Players).GetField("PlayerData", BindingFlags.NonPublic | BindingFlags.Instance);
            var PlayerData = field.GetValue(Players.Main) as Dictionary<int, PlayerInfo>;

            var Bots = World.GetExistingSystem<WebsocketSystemBase>().GetClients();
            foreach (KeyValuePair<int, string> entry in Bots)
            {
                Debug.Log("Spawning " + entry.Value);
                evt.User = entry.Key;
                input.Send(evt);

                if (PlayerData.ContainsKey(entry.Key))
                {
                    var pInfo = PlayerData[entry.Key];
                    pInfo.Username = "Bot";
                    PlayerData[entry.Key] = pInfo;
                }

                var profileID = new ProfileIdentifier { _Value = entry.Value };
                if (!ProfileStore.Main.HasProfile(profileID))
                {
                    var profile = new PlayerProfile();
                    profile.Name = entry.Value;
                    profile.Outfit = PlayerOutfit.Party;
                    profile.RequiresTutorial = false;
                    ProfileStore.Main.SetProfile(profileID, profile);
                }
                Players.Main.SetActiveProfile(entry.Key, profileID);
            }
        }

        protected override void OnUpdate()
        {
            if (Has<CSceneFirstFrame>())
            {
                var Bots = World.GetExistingSystem<WebsocketSystemBase>().GetClients();
                var ActivePlayers = playerQuery.ToEntityArray(Allocator.Temp);
                foreach (var activePlayer in ActivePlayers)
                {
                    var ID = GetComponent<CPlayer>(activePlayer).ID;
                    if (Bots.ContainsKey(ID))
                    {
                        EntityManager.AddComponent<CBotControl>(activePlayer);
                        EntityManager.AddBuffer<CBotFeedback>(activePlayer);
                        if (currentRoles.ContainsKey(ID))
                        {
                            EntityManager.AddComponentData(activePlayer, new CBotRole(currentRoles[ID]));
                        }
                        else
                        {
                            EntityManager.AddComponentData(activePlayer, new CBotRole(BotRole.None));
                        }
                    }
                }
                ActivePlayers.Dispose();

                if (!isInitialized)
                {
                    isInitialized = true;

                    var ActiveBots = botQuery.ToEntityArray(Allocator.Temp);
                    foreach (var activeBot in ActiveBots)
                    {
                        var ID = GetComponent<CPlayer>(activeBot).ID;
                        Players.Main.ReloadLocalProfile(ID);
                    }
                    ActiveBots.Dispose();
                }
            }
        }
    }
}
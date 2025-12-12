using Kitchen;
using KitchenMods;
using Unity.Entities;
using Unity.Collections;
using Controllers;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Neuro_Plateup
{
    public class BotHandler : GenericSystemBase, IModSystem
    {
        private static EntityQuery botQuery, playerQuery, optionQuery, optionQuery2, optionQuery3;
        bool isInitialized = false;
        private FakeInput input;
        private InputUpdateEvent evt;
        public static Dictionary<int, BotRole> currentRoles = new Dictionary<int, BotRole>();

        private static GameSystemBase UpdateCustomerImpatience, UpdateQueuePatience, CreateCustomerSchedule;

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
            optionQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPlayer),
                        typeof(CPosition)
                    ));
            optionQuery2 = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPlayer),
                        typeof(CPosition)
                    ).None(
                        typeof(CBotControl)
                    ));
            optionQuery3 = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPlayer)
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

            UpdateCustomerImpatience = World.GetExistingSystem<UpdateCustomerImpatience>();
            UpdateQueuePatience = World.GetExistingSystem<UpdateQueuePatience>();
            CreateCustomerSchedule = World.GetExistingSystem<CreateCustomerSchedule>();
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
                    pInfo.Username = entry.Value + " (AI)";
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
                        EntityManager.AddBuffer<CBotItems>(activePlayer);
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

                    RefreshOptions();
                }
            }
        }

        public static void RefreshOptions()
        {
            var CustomerPatienceOption = AccessTools.Field(typeof(UpdateCustomerImpatience), "Players");
            if (NeuroPreferences.GetCustomerPatienceOption())
            {
                CustomerPatienceOption.SetValue(UpdateCustomerImpatience, optionQuery);
            }
            else
            {
                CustomerPatienceOption.SetValue(UpdateCustomerImpatience, optionQuery2);
            }

            var QueuePatienceOption = AccessTools.Field(typeof(UpdateQueuePatience), "Players");
            if (NeuroPreferences.GetQueuePatienceOption())
            {
                QueuePatienceOption.SetValue(UpdateQueuePatience, optionQuery3);
            }
            else
            {
                QueuePatienceOption.SetValue(UpdateQueuePatience, playerQuery);
            }

            var CustomerAmountOption = AccessTools.Field(typeof(CreateCustomerSchedule), "Players");
            if (NeuroPreferences.GetCustomerAmountOption())
            {
                CustomerAmountOption.SetValue(CreateCustomerSchedule, optionQuery3);
            }
            else
            {
                CustomerAmountOption.SetValue(CreateCustomerSchedule, playerQuery);
            }
        }
    }
}
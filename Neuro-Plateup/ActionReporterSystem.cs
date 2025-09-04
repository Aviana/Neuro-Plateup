using Kitchen;
using Kitchen.Layouts;
using KitchenData;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Neuro_Plateup
{
    public class ActionReporterSystem : WebsocketSystemBase
    {
        private EntityQuery BotQuery, IdleBotQuery, Feedback, Service, Fires, Messes, OrderQuery, HeldItems, Ingredients, BinsQuery, UnlockQuery, ProgressionQuery, PatienceQuery;

        private readonly Dictionary<int, List<string>> RegisteredActions = new Dictionary<int, List<string>> { };

        private bool isInitialized = true;

        private List<(EntityQuery query, GameState state)> gameStateQueries;

        protected override void Initialise()
        {
            base.Initialise();

            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl)
                    ));
            IdleBotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CBotRole)
                    ).None(
                        typeof(CBotAction),
                        typeof(CMoveTo),
                        typeof(CGrabAction),
                        typeof(CInteractAction)
                    ));
            Feedback = GetEntityQuery(typeof(CBotFeedback));
            Service = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CGroupReadyToOrder)
                    ));
            Fires = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CIsOnFire)
                    ));
            Messes = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CMess)
                    ));
            OrderQuery = GetEntityQuery(typeof(CDisplayedItem));
            HeldItems = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CItem),
                        typeof(CHeldBy)
                    ));
            Ingredients = GetEntityQuery(typeof(CAvailableIngredient));
            BinsQuery = GetEntityQuery(typeof(CApplianceBin));
            UnlockQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CLinkedView),
                        typeof(CUnlockSelectPopup),
                        typeof(CCapturedUserInput)
                    ));
            ProgressionQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CProgressionOption),
                        typeof(CProgressionOption.Selected),
                        typeof(CProgressionOption.Displayed)
                    ));
            PatienceQuery = GetEntityQuery(typeof(CPatience));

            gameStateQueries = new List<(EntityQuery, GameState)>
            {
                (GetEntityQuery(typeof(SClearScene)), GameState.None),
                (GetEntityQuery(new QueryHelper().Any(typeof(CGamePauseRequest), typeof(CCaptureInput)).None(typeof(CCapturePassthrough))), GameState.Paused),
                (GetEntityQuery(typeof(SGameOver)), GameState.GameOver),
                (GetEntityQuery(typeof(SFranchiseBuilderMarker)), GameState.FranchiseBuilder),
                (GetEntityQuery(typeof(SIsNightTime)), GameState.Night),
                (GetEntityQuery(typeof(SIsDayTime)), GameState.Day),
                (GetEntityQuery(typeof(SFranchiseMarker)), GameState.Franchise)
            };

            for (int i = 1; i <= 4; i++)
            {
                RegisteredActions.Add(i, new List<string>());
            }
        }

        protected override void OnAction(int id, string payload)
        {
            var action = new NeuroAPI.Answer();
            Dictionary<string, object> data = null;
            try
            {
                action = JsonConvert.DeserializeObject<NeuroAPI.Answer>(payload);
                if (action == null || action.command != "action")
                {
                    throw new Exception("Invalid command");
                }
                if (!ActionsRegistry.ACTIONS.Any(obj => obj.Name == action.data.name))
                {
                    throw new Exception("Unknown action");
                }

                if (!string.IsNullOrEmpty(action.data.Data))
                {
                    try
                    {
                        var jObject = JObject.Parse(action.data.Data);
                        data = jObject.ToObject<Dictionary<string, object>>();
                    }
                    catch (Exception)
                    {
                        throw new Exception("Malformed JSON");
                    }

                    var schema = SchemaFactory.GetSchema(action.data.name);
                    if (!SchemaFactory.ValidateAgainstSchema(data, schema))
                    {
                        throw new Exception("Invalid json");
                    }
                }
            }
            catch (Exception ex)
            {
                EnqueueMessage(
                    id,
                    new NeuroAPI.ActionResult
                    {
                        data = new NeuroAPI.ActionResultData
                        {
                            id = action.data.id,
                            message = ex.Message,
                            success = false
                        }
                    });
                return;
            }

            if (!RegisteredActions[id].Contains(action.data.name))
            {
                EnqueueMessage(
                    id,
                    new NeuroAPI.ActionResult
                    {
                        data = new NeuroAPI.ActionResultData
                        {
                            id = action.data.id,
                            message = "Invalid action.",
                            success = true
                        }
                    });
                return;
            }

            EnqueueMessage(
                id,
                new NeuroAPI.UnregisterActions
                {
                    data = new NeuroAPI.UnregisterActionsData
                    {
                        action_names = RegisteredActions[id].ToArray()
                    }
                });
            RegisteredActions[id].Clear();

            EnqueueMessage(
                id,
                new NeuroAPI.ActionResult
                {
                    data = new NeuroAPI.ActionResultData
                    {
                        id = action.data.id,
                        message = "",
                        success = true
                    }
                });

            if (action.data.name != "stop")
            {
                EnqueueMessage(
                    id,
                    new NeuroAPI.RegisterActions
                    {
                        data = new NeuroAPI.RegisterActionsData
                        {
                            actions = new NeuroAPI.Action[]
                            {
                                new NeuroAPI.Action
                                {
                                    name = "stop",
                                    description = GLOBALSTRINGS.ACTIONS["stop"].DESC.Replace("{{action}}", GetActionDescription(action.data.name)),
                                    schema = null
                                }
                            }
                        }
                    });
                RegisteredActions[id].Add("stop");
            }

            var Bots = BotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in Bots)
            {
                if (GetComponent<CPlayer>(bot).ID == id)
                {
                    if (HasComponent<CBotAction>(bot))
                    {
                        EntityManager.SetComponentData(bot, new CBotAction(action.data.name));
                    }
                    else
                    {
                        if (data == null)
                        {
                            EntityManager.AddComponentData(bot, new CBotAction(action.data.name));
                        }
                        else
                        {
                            EntityManager.AddComponentData(
                                bot,
                                new CBotAction(action.data.name, data.Values.First().ToString())
                                );
                        }
                    }
                    break;
                }
            }
            Bots.Dispose();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!ProgressionQuery.IsEmptyIgnoreFilter)
            {
                var OrderItems = ProgressionQuery.ToEntityArray(Allocator.Temp);
                foreach (var item in OrderItems)
                {
                    if (Require<CProgressionOption>(item, out var comp))
                    {
                        Debug.Log(comp.ID);
                        if (Data.TryGet<Dish>(comp.ID, out var dish))
                        {
                            foreach (var menuItem in dish.UnlocksMenuItems)
                            {
                                Debug.Log(menuItem.Item.ID);
                                
                            }
                        }
                    }
                }
                OrderItems.Dispose();
            }

            if (!isInitialized)
            {
                isInitialized = true;

                // NYI: This also needs to run when a dish is placed in the franchise mode & re-register the prepare dish action
                var IngredientUnlocks = Ingredients.ToComponentDataArray<CAvailableIngredient>(Allocator.Temp);
                var UnlockList = new HashSet<int>();
                foreach (var unlock in IngredientUnlocks)
                {
                    UnlockList.Add(unlock.Ingredient);
                }
                IngredientUnlocks.Dispose();

                foreach (var order in OrderNameRepository.Data)
                {
                    var flag = true;
                    foreach (var ingredient in order.Value.Items)
                    {
                        if (!UnlockList.Contains(ingredient))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        Debug.LogWarning("Adding Dish: " + order.Key);
                        SchemaFactory.Dishes.Add(order.Key);
                    }
                }
            }
            if (HasSingleton<CSceneFirstFrame>())
            {
                SchemaFactory.Dishes.Clear();
                SchemaFactory.Players.Clear();
                isInitialized = false;

                var field = typeof(Players).GetField("PlayerData", BindingFlags.NonPublic | BindingFlags.Instance);
                var PlayerData = field.GetValue(Players.Main) as Dictionary<int, PlayerInfo>;

                foreach (var pl in PlayerData)
                {
                    if (pl.Value.IsLocalUser && pl.Value.Username == "Bot")
                    {
                        SchemaFactory.Players.Add(pl.Value.Name, pl.Value.ID);
                    }
                    else
                    {
                        SchemaFactory.Players.Add(pl.Value.Username, pl.Value.ID);
                    }
                }

                var resetBots = BotQuery.ToEntityArray(Allocator.Temp);
                foreach (var bot in resetBots)
                {
                    EntityManager.RemoveComponent<CMoveTo>(bot);
                    EntityManager.RemoveComponent<CGrabAction>(bot);
                    EntityManager.RemoveComponent<CInteractAction>(bot);
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    EntityManager.RemoveComponent<CBotActionRunning>(bot);
                }
                resetBots.Dispose();
            }

            GameState State = GameState.None;
            foreach (var (query, mappedState) in gameStateQueries)
            {
                if (!query.IsEmptyIgnoreFilter)
                {
                    State = mappedState;
                    break;
                }
            }

            var Bots = Feedback.ToEntityArray(Allocator.Temp);
            foreach (var bot in Bots)
            {
                if (!RequireBuffer<CBotFeedback>(bot, out var Messages))
                    continue;

                var id = GetComponent<CPlayer>(bot).ID;
                foreach (var feedback in Messages)
                {
                    EnqueueMessage(
                        id,
                        new NeuroAPI.Context
                        {
                            data = new NeuroAPI.ContextData
                            {
                                message = feedback.Message.ToString(),
                                silent = feedback.IsSilent
                            }
                        }
                    );
                }
                Messages.Clear();
            }
            Bots.Dispose();

            Bots = IdleBotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in Bots)
            {
                var unregister = new List<string>();
                var register = new List<NeuroAPI.Action>();
                var id = GetComponent<CPlayer>(bot).ID;
                var role = GetComponent<CBotRole>(bot).Role;

                foreach (var action in ActionsRegistry.ACTIONS)
                {
                    var isAvailable =
                        (action.RequiredState & State) != 0 &&
                        (action.Roles & role) != 0 &&
                        action.IsAvailable(this, bot);
                    var isRegistered = RegisteredActions[id].Contains(action.Name);
                    if (isAvailable && !isRegistered)
                    {
                        register.Add(
                            new NeuroAPI.Action
                            {
                                name = action.Name,
                                description = GetActionDescription(action.Name),
                                schema = SchemaFactory.GetSchema(action.Name)
                            }
                        );
                        RegisteredActions[id].Add(action.Name);
                    }
                    else if (!isAvailable && isRegistered)
                    {
                        unregister.Add(action.Name);
                        RegisteredActions[id].Remove(action.Name);
                    }
                }
                if (register.Count > 0)
                {
                    EnqueueMessage(
                        id,
                        new NeuroAPI.RegisterActions
                        {
                            data = new NeuroAPI.RegisterActionsData
                            {
                                actions = register.ToArray()
                            }
                        }
                    );
                }
                if (unregister.Count > 0)
                {
                    EnqueueMessage(
                        id,
                        new NeuroAPI.UnregisterActions
                        {
                            data = new NeuroAPI.UnregisterActionsData
                            {
                                action_names = unregister.ToArray()
                            }
                        }
                    );
                }
            }
            Bots.Dispose();
        }

        private string GetActionDescription(string action)
        {
            if (GLOBALSTRINGS.ACTIONS.ContainsKey(action))
                return GLOBALSTRINGS.ACTIONS[action].DESC;

            var desc = "";
            if (!UnlockQuery.IsEmptyIgnoreFilter)
            {
                var buffer = GetBuffer<CUnlockSelectPopupOption>(UnlockQuery.GetSingletonEntity());
                int ID;
                if (action == "select_card_left")
                {
                    ID = buffer[0].ID;
                }
                else
                {
                    ID = buffer[1].ID;
                }
                if (GLOBALSTRINGS.UNLOCKS.ContainsKey(ID))
                {
                    desc = GLOBALSTRINGS.UNLOCKS[ID];
                }
                else if (Data.TryGet<Unlock>(ID, out var unlock))
                {
                    desc = unlock.Description;
                }
                else if (Data.TryGet<UnlockCard>(ID, out var card))
                {
                    desc = card.Description;
                }
            }
            
            return desc;
        }

        public bool IsBusy(Entity bot)
        {
            return HasComponent<CBotAction>(bot) && GetComponent<CBotAction>(bot).Action != "stop";
        }

        public bool HasPlayers(Entity bot)
        {
            return SchemaFactory.Players.Count > 0;
        }

        public bool HasCardSelection(Entity bot)
        {
            // NYI: Single card selection
            if (UnlockQuery.IsEmptyIgnoreFilter)
                return false;

            return true;
        }

        public bool AlwaysAvailable(Entity bot)
        {
            return true;
        }

        public bool AviableReceipes(Entity bot)
        {
            return SchemaFactory.Dishes.Count > 0;
        }

        public bool CanMoveToStart(Entity bot)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (pos.x <= -6 && pos.x >= -11 && pos.z <= -7 && pos.z >= -8)
                return false;
            return true;
        }

        public bool HasWaitingGuests(Entity bot)
        {
            return !Service.IsEmptyIgnoreFilter;
        }

        public bool HasFires(Entity bot)
        {
            // NYI: Maybe we check if those fires already have interactors / bots running to them
            return !Fires.IsEmptyIgnoreFilter;
        }

        public bool HasMess(Entity bot)
        {
            // NYI: Check only in your room?
            return !Messes.IsEmptyIgnoreFilter;
        }

        public bool HasServableFood(Entity bot)
        {
            var patienceList = new Dictionary<Vector3, float>();
            var Groups = PatienceQuery.ToEntityArray(Allocator.Temp);
            foreach (var group in Groups)
            {
                var comp = GetComponent<CPatience>(group);
                if (comp.Reason == PatienceReason.WaitForFood || comp.Reason == PatienceReason.GetFoodDelivered)
                {
                    patienceList.Add(GetComponent<CPosition>(group).Position, comp.RemainingTime);
                }
            }
            Groups.Dispose();

            var orderedFood = new List<ItemInfo>();
            var openOrders = OrderQuery.ToEntityArray(Allocator.Temp);
            foreach (var orderGroup in openOrders)
            {
                if (RequireBuffer<CDisplayedItem>(orderGroup, out var buffer))
                {
                    foreach (var order in buffer)
                    {
                        if (!patienceList.ContainsKey(order.TablePosition))
                            break;

                        if (order.ShowExtra)
                        {
                            if (CookingSystem.ServeProviders.ContainsKey(order.ExtraID))
                            {
                                openOrders.Dispose();
                                return true;
                            }
                            else
                            {
                                orderedFood.Add(new ItemInfo(order.ExtraID, new FixedListInt64 { order.ExtraID }));
                            }
                        }

                        if (order.IsComplete)
                                continue;

                        if (Require<CItem>(order.Item, out var comp))
                        {
                            if (CookingSystem.ServeProviders.ContainsKey(comp.ID))
                            {
                                openOrders.Dispose();
                                return true;
                            }

                            orderedFood.Add(new ItemInfo(comp.ID, comp.Items.ToFixedListInt64()));
                        }
                    }
                }
            }
            openOrders.Dispose();

            if (orderedFood.Count == 0)
                return false;

            // NYI: check the bots hands
            var flag = false;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                var holder = GetComponent<CHeldBy>(item).Holder;
                if (!HasComponent<CPlayer>(holder) && MoveToSystem.Hatches.Contains(GetComponent<CPosition>(holder).Position))
                {
                    if (orderedFood.Contains(GetComponent<CItem>(item)))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool HasUnsatisfiedOrders(Entity bot)
        {
            var patienceList = new Dictionary<Vector3, float>();
            var Groups = PatienceQuery.ToEntityArray(Allocator.Temp);
            foreach (var group in Groups)
            {
                var comp = GetComponent<CPatience>(group);
                if (comp.Reason == PatienceReason.WaitForFood || comp.Reason == PatienceReason.GetFoodDelivered)
                {
                    patienceList.Add(GetComponent<CPosition>(group).Position, comp.RemainingTime);
                }
            }
            Groups.Dispose();

            if (patienceList.Count < 1)
            {
                return false;
            }

            var orderSets = new Dictionary<float, OrderList>();
            var Orders = OrderQuery.ToEntityArray(Allocator.Temp);
            foreach (var orderGroup in Orders)
            {
                foreach (var order in GetBuffer<CDisplayedItem>(orderGroup))
                {
                    if (!patienceList.ContainsKey(order.TablePosition))
                        break;

                    var patience = patienceList[order.TablePosition];
                    var hasExtra = order.ShowExtra && !CookingSystem.ServeProviders.ContainsKey(order.ExtraID);

                    if (order.IsComplete && !hasExtra)
                        continue;

                    if (!orderSets.ContainsKey(patience))
                    {
                        orderSets[patience] = new OrderList(order.TablePosition);
                    }

                    if (hasExtra)
                        orderSets[patience].Add(new ItemInfo(order.ExtraID, new FixedListInt64 { order.ExtraID }));

                    if (order.IsComplete)
                        continue;

                    var comp = GetComponent<CItem>(order.Item);
                    orderSets[patience].Add(new ItemInfo(comp.ID, comp.Items.ToFixedListInt64()));
                }
            }
            Orders.Dispose();

            if (orderSets.Count < 1)
            {
                return false;
            }

            var orderSet = orderSets.OrderBy(kvp => kvp.Key).First().Value.Items;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var ent in Items)
            {
                var holder = GetComponent<CHeldBy>(ent).Holder;
                var tile = TileManager.GetTile(GetComponent<CPosition>(holder).Position);
                if (MoveToSystem.Hatches.Contains(tile.Position) || tile.Type != RoomType.Kitchen && HasComponent<CPlayer>(holder))
                {
                    var citem = GetComponent<CItem>(ent);
                    for (int i = orderSet.Count - 1; i >= 0; i--)
                    {
                        if (citem.Items.IsEquivalent(orderSet[i].Items))
                        {
                            orderSet.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return orderSet.Count > 0;
        }

        public bool IsBinFull(Entity bot)
        {
            var flag = false;
            var Bins = BinsQuery.ToComponentDataArray<CApplianceBin>(Allocator.Temp);
            foreach (var bin in Bins)
            {
                if (bin.CurrentAmount == bin.Capacity && bin.SelfEmptyTime == 0)
                {
                    flag = true;
                    break;
                }
            }
            Bins.Dispose();
            return flag;
        }

        public bool KitchenHasDirtyPlates(Entity bot)
        {
            if (Require<CBotRole>(bot, out var comp) && comp.Role == BotRole.Chef)
            {
                var Bots = BotQuery.ToComponentDataArray<CBotRole>(Allocator.Temp);
                foreach (var b in Bots)
                {
                    if (b.Role == BotRole.Dishwasher)
                    {
                        Bots.Dispose();
                        return false;
                    }
                }
                Bots.Dispose();
            }

            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                if (CookingSystem.DirtyPlates.Contains(GetComponent<CItem>(item).ID))
                {
                    var holder = GetComponent<CHeldBy>(item).Holder;
                    var pos = GetComponent<CPosition>(holder).Position;
                    var tile = TileManager.GetTile(pos);
                    if (!HasComponent<CPlayer>(holder) && (MoveToSystem.Hatches.Contains(pos) || tile.Type == RoomType.Kitchen))
                    {
                        return true;
                    }
                }
            }
            Items.Dispose();
            return false;
        }

        public bool HasDirtyPlates(Entity bot)
        {
            // NYI: Include returning Ketchup, Mustard and Soy Sauce
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                if (CookingSystem.DirtyPlates.Contains(GetComponent<CItem>(item).ID))
                {
                    var holder = GetComponent<CHeldBy>(item).Holder;
                    var pos = GetComponent<CPosition>(holder).Position;
                    var tile = TileManager.GetTile(pos);
                    if (tile.Type != RoomType.Kitchen && !HasComponent<CPlayer>(holder) && !MoveToSystem.Hatches.Contains(pos))
                    {
                        return true;
                    }
                }
            }
            Items.Dispose();
            return false;
        }
    }
}
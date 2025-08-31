using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using KitchenData;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Controllers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Neuro_Plateup
{
    public class ActionExecutor : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, ServiceQuery, OrderQuery, PatienceQuery, PlayerQuery, HeldItems, BinsQuery;
        private Dictionary<string, Action<Entity, string>> _commands;
        private Vector3 StartingPosition;
        private Dictionary<int, PlayerInfo> PlayerData;
        private readonly OrderNameRepository OrderNames = new OrderNameRepository();
        private CookingSystem cookingSystem;
        private FakeInput input;

        protected override void Initialise()
        {
            base.Initialise();

            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CBotAction)
                    ).None(
                        typeof(CBotWaiting)
                        ));
            ServiceQuery = GetEntityQuery(typeof(CGroupReadyToOrder));
            OrderQuery = GetEntityQuery(typeof(CDisplayedItem));
            PatienceQuery = GetEntityQuery(typeof(CPatience));
            PlayerQuery = GetEntityQuery(typeof(CPlayer));
            HeldItems = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CItem),
                        typeof(CHeldBy)
                    ));
            BinsQuery = GetEntityQuery(typeof(CApplianceBin));

            _commands = new Dictionary<string, Action<Entity, string>>
            {
                { "stop", CancelAction },
                { "go_to", GoToPlayer },
                { "select_card_left", MenuLeft },
                { "select_card_right", MenuRight },
                { "role_chef", BecomeChef },
                { "role_waiter", BecomeWaiter },
                { "start_game", StartGame },
                { "rename_restaurant", RenameRestaurant },
                { "service", Service },
                { "extinguish_fires", ExtinguishFires },
                { "clean_mess", CleanMess },
                { "serve", Serve },
                { "prepare_order", Cook },
                { "prepare_dish", Prepare },
                { "empty_bin", ClearBin },
                { "wash_plates", WashPlates },
                { "return_plates", ReturnPlates }
            };

            StartingPosition = new Vector3 { x = -8, y = 0, z = -8 };

            input = new FakeInput();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            var field = typeof(Players).GetField("PlayerData", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerData = field.GetValue(Players.Main) as Dictionary<int, PlayerInfo>;
            cookingSystem = World.GetExistingSystem<CookingSystem>();
        }

        protected override void OnUpdate()
        {
            if (Has<CSceneFirstFrame>())
            {
                // NYI: Make a list of available dishes???
            }

            var Bots = BotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in Bots)
            {
                var action = GetComponent<CBotAction>(bot);
                if (_commands.TryGetValue(action.Action.ToString(), out var method))
                    method(bot, action.Payload.ToString());
                else
                    Debug.LogError($"Unknown command: {action.Action}");
            }
            Bots.Dispose();
        }

        private bool GetNearest(ComponentType type, Vector3 origin, out Vector3 nearest)
        {
            nearest = new Vector3();
            float closestDistanceSqr = float.MaxValue;

            var query = GetEntityQuery(type);
            if (query.IsEmptyIgnoreFilter)
                return false;

            var entities = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                var pos = GetComponent<CPosition>(entity).Position;
                float distSqr = (pos - origin).sqrMagnitude;
                if (distSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distSqr;
                    nearest = pos;
                }
            }
            entities.Dispose();

            return true;
        }

        private void EmptyHands(Entity bot)
        {
            // NYI: return things like the sharp knife
            // NYI: Find a better way to empty the hands
            EntityManager.AddComponent<CReturnItem>(GetComponent<CItemHolder>(bot).HeldItem);
        }

        private void CancelAction(Entity bot, string payload)
        {
            if (HasComponentOfHeld<CItem>(bot))
            {
                if (!(HasComponent<CMoveTo>(bot) || HasComponent<CGrabAction>(bot)))
                {
                    EmptyHands(bot);
                }
            }
            EntityManager.RemoveComponent<CMoveTo>(bot);
            EntityManager.RemoveComponent<CInteractAction>(bot);
            EntityManager.RemoveComponent<CGrabAction>(bot);
            EntityManager.RemoveComponent<CBotActionRunning>(bot);
            EntityManager.RemoveComponent<CBotAction>(bot);
            EntityManager.RemoveComponent<CBotWaiting>(bot);
        }

        private void BecomeChef(Entity bot, string payload)
        {
            if (HasComponentOfHeld<CItem>(bot))
            {
                if (!(HasComponent<CMoveTo>(bot) || HasComponent<CGrabAction>(bot)))
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                BotHandler.currentRoles[GetComponent<CPlayer>(bot).ID] = BotRole.Chef;
                EntityManager.SetComponentData(bot, new CBotRole(BotRole.Chef));
                EntityManager.RemoveComponent<CBotAction>(bot);
                var ID = GetComponent<CPlayer>(bot).ID;
                var info = PlayerData[ID];
                // NYI: Switch costume
            }
        }

        private void BecomeWaiter(Entity bot, string payload)
        {
            if (HasComponentOfHeld<CItem>(bot))
            {
                if (!(HasComponent<CMoveTo>(bot) || HasComponent<CGrabAction>(bot)))
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                BotHandler.currentRoles[GetComponent<CPlayer>(bot).ID] = BotRole.Waiter;
                EntityManager.SetComponentData(bot, new CBotRole(BotRole.Waiter));
                EntityManager.RemoveComponent<CBotAction>(bot);
                // NYI: Switch costume
            }
        }

        private void StartGame(Entity bot, string payload)
        {
            var pos = GetComponent<CPosition>(bot).Position;
            if (!HasComponent<CMoveTo>(bot) && !pos.IsSameTile(StartingPosition))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(StartingPosition));
            }
            else if (!HasComponent<CMoveTo>(bot))
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
            }
        }

        private void RenameRestaurant(Entity bot, string payload)
        {
            // NYI: Duh
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void Service(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot))
                return;

            if (HasComponent<CBotActionRunning>(bot))
            {
                EntityManager.RemoveComponent<CBotActionRunning>(bot);
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
            }

            var Groups = ServiceQuery.ToEntityArray(Allocator.Temp);
            Entity target = new Entity();
            float RemainingTime = float.MaxValue;
            foreach (var group in Groups)
            {
                var patience = GetComponent<CPatience>(group);
                if (patience.RemainingTime < RemainingTime)
                {
                    RemainingTime = patience.RemainingTime;
                    target = group;
                }
            }
            Groups.Dispose();

            var pos = GetComponent<CPosition>(target).Position;
            EntityManager.AddComponent<CBotActionRunning>(bot);
            EntityManager.AddComponentData(bot, new CMoveTo(pos));
            EntityManager.AddComponentData(bot, new CInteractAction(pos, false));
        }

        private void ExtinguishFires(Entity bot, string payload)
        {
            // NYI: Fire Extinguisher
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot))
                return;
            if (GetNearest(typeof(CIsOnFire), GetComponent<CPosition>(bot).Position, out var nearest))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(nearest));
                EntityManager.AddComponentData(bot, new CInteractAction(nearest, true));
                return;
            }
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void CleanMess(Entity bot, string payload)
        {
            // NYI: Clean mess in your current room?
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void Serve(Entity bot, string payload)
        {
            // NYI: Trays
            if (HasComponent<CMoveTo>(bot) || HasComponent<CGrabAction>(bot))
                return;

            if (Has<CBotActionRunning>(bot))
            {
                EntityManager.RemoveComponent<CBotActionRunning>(bot);
                return;
            }

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

            var orderList = new Dictionary<float, OrderList>();
            var Orders = OrderQuery.ToEntityArray(Allocator.Temp);
            foreach (var orderGroup in Orders)
            {
                foreach (var order in GetBuffer<CDisplayedItem>(orderGroup))
                {
                    if (!patienceList.ContainsKey(order.TablePosition))
                        break;

                    var patience = patienceList[order.TablePosition];
                    if (!orderList.ContainsKey(patience))
                    {
                        orderList[patience] = new OrderList(GetComponent<CItem>(order.Item), order.TablePosition);
                    }
                    else
                    {
                        orderList[patience].Add(GetComponent<CItem>(order.Item));
                    }
                }
            }
            Orders.Dispose();

            if (orderList.Count < 1)
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
            }

            if (!HasComponentOfHeld<CItem>(bot))
            {
                var servables = new Dictionary<Vector3, CItem>();
                var Items = HeldItems.ToEntityArray(Allocator.Temp);
                foreach (var item in Items)
                {
                    var pos = GetComponent<CPosition>(GetComponent<CHeldBy>(item).Holder).Position;
                    if (MoveToSystem.Hatches.Contains(pos))
                    {
                        servables.Add(pos, GetComponent<CItem>(item));
                    }
                }
                Items.Dispose();

                if (servables.Count < 1)
                {
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }

                Vector3 target = new Vector3();
                float currentPatience = float.MaxValue;
                foreach (var servable in servables)
                {
                    foreach (var entry in orderList.OrderBy(kv => kv.Key))
                    {
                        if (entry.Key < currentPatience && entry.Value.Items.Contains(servable.Value))
                        {
                            currentPatience = entry.Key;
                            target = servable.Key;
                        }
                    }
                }

                if (currentPatience == float.MaxValue)
                {
                    // Dishes on hatch do not have a delivery target
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }
                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target));
            }
            else
            {
                GetComponentOfHeld<CItem>(bot, out var holding);
                Vector3 target = new Vector3();
                float currentPatience = float.MaxValue;

                foreach (var entry in orderList.OrderBy(kv => kv.Key))
                {
                    if (entry.Key < currentPatience && entry.Value.Items.Contains(holding))
                    {
                        currentPatience = entry.Key;
                        target = entry.Value.Position;
                    }
                }

                if (currentPatience == float.MaxValue)
                {
                    // Dish in hand does not have a delivery target
                    // NYI: put it back?
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }

                // We use this to stall as the result of our serving takes another frame to process
                EntityManager.AddComponent<CBotActionRunning>(bot);

                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target));
            }
        }

        private void Prepare(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

            if (!OrderNameRepository.TryGetValues(payload, out var itemList) || cookingSystem.Cook(bot, itemList))
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
            }
        }

        private void Cook(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

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
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
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
                    if (!orderSets.ContainsKey(patience))
                    {
                        orderSets[patience] = new OrderList(GetComponent<CItem>(order.Item), order.TablePosition);
                    }
                    else
                    {
                        orderSets[patience].Add(GetComponent<CItem>(order.Item));
                    }
                }
            }
            Orders.Dispose();

            if (orderSets.Count < 1)
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
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
                        if (orderSet[i].Items.IsEquivalent(citem.Items))
                        {
                            orderSet.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            if (orderSet.Count < 1)
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
            }

            // NYI: Check if there are ingredients in these orders that can be produced in multitasking (like steak) and produce those first 

            var finalItem = orderSet.First();
            cookingSystem.Cook(bot, finalItem.Items);
        }

        private void ClearBin(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                var pos = GetComponent<CPosition>(bot).Position;
                if (comp.ID == 895813906 && cookingSystem.GetNearestAppliance(pos, CookingSystem.CookingAppliances, out var target, out var ID, true))
                {
                    // burn the witch
                }
                else if (comp.ID == -1660145659 || comp.ID == 895813906)
                {
                    // normal garbage bag
                    if (!Require<CPosition>(GetSingletonEntity<CApplianceExternalBin>(), out var outsideBin))
                    {
                        Debug.LogError("Could not find roadside bin.");
                        EntityManager.RemoveComponent<CBotAction>(bot);
                        return;
                    }

                    EntityManager.AddComponentData(bot, new CMoveTo(outsideBin.Position));
                    EntityManager.AddComponentData(bot, new CGrabAction(outsideBin.Position));
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                var Bins = BinsQuery.ToEntityArray(Allocator.Temp);
                var flag = false;
                foreach (var bin in Bins)
                {
                    var binComp = GetComponent<CApplianceBin>(bin);
                    if (binComp.CurrentAmount == binComp.Capacity)
                    {
                        var binPos = GetComponent<CPosition>(bin).Position;
                        EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(binPos, false));
                        flag = true;
                        break;
                    }
                }
                Bins.Dispose();
                if (!flag)
                {
                    EntityManager.RemoveComponent<CBotAction>(bot);
                }
            }
        }

        private void WashPlates(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

            // Check & move to kitchen
            

            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 793377380)
                {
                    // clean plate
                    if (cookingSystem.GetNearestAppliance(pos, CookingSystem.Plates, out var platePos, out var ID, false, true))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1517992271)
                {
                    // dirty plate
                    if (cookingSystem.GetNearestDishWasher(pos, false, false, out var washerPos))
                    {
                        // NYI: needs testing
                        Debug.Log("Found a dish washer");
                        EntityManager.AddComponentData(bot, new CMoveTo(washerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(washerPos));
                    }
                    else if (cookingSystem.GetNearestAppliance(pos, CookingSystem.Sinks, out var sinkPos, out var ID, true))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(sinkPos));
                    }
                    else
                    {
                        // there is no free place to clean dishes
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -1955934157)
                {
                    // bone
                    if (!cookingSystem.GetNearestAppliance(pos, CookingSystem.Bins, out var binPos, out var ID, false, true))
                    {
                        binPos = GetComponent<CPosition>(GetEntity<CApplianceExternalBin>());
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(binPos));
                }
                else if (comp.ID == -1527669626)
                {
                    // plate with leftovers
                    if (!cookingSystem.GetNearestAppliance(pos, CookingSystem.Bins, out var binPos, out var ID, false, true))
                    {
                        binPos = GetComponent<CPosition>(GetEntity<CApplianceExternalBin>());
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(binPos, false));
                }
                else
                {
                    // we are holding something not related to cleaning dishes
                    EmptyHands(bot);
                }
            }
            else if (cookingSystem.GetNearestDishWasher(pos, true, false, out var washerPos))
            {
                // NYI: needs testing
                Debug.Log("Found full dishwasher");
                EntityManager.AddComponentData(bot, new CMoveTo(washerPos));
                EntityManager.AddComponentData(bot, new CInteractAction(washerPos, false));
            }
            else if (cookingSystem.FindNearestItem(new HashSet<int> { 793377380 }, pos, CookingSystem.Sinks, out var sinkPos))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                EntityManager.AddComponentData(bot, new CGrabAction(sinkPos));
            }
            else if (cookingSystem.FindNearestItem(new HashSet<int> { 1517992271 }, pos, CookingSystem.Sinks, out var sinkPos2))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(sinkPos2));
                EntityManager.AddComponentData(bot, new CInteractAction(sinkPos2, true));
            }
            else if (cookingSystem.FindNearestItem(new HashSet<int> { 348289471 }, pos, out var bonePlatePos, false, CookingSystem.KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(bonePlatePos));
                EntityManager.AddComponentData(bot, new CInteractAction(bonePlatePos, true));
            }
            else if (cookingSystem.FindNearestItem(new HashSet<int> { 1517992271, -1527669626 }, pos, out var dirtyPlatePos, false, CookingSystem.KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(dirtyPlatePos));
                EntityManager.AddComponentData(bot, new CGrabAction(dirtyPlatePos));
            }
            else
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
            }
        }

        private void ReturnPlates(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (CookingSystem.DirtyPlates.Contains(comp.ID))
                {
                    if (cookingSystem.GetBestDropOff(pos, out var hatchPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hatchPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hatchPos));
                        return;
                    }
                    else
                    {
                        // All hatches are blocked
                        EntityManager.RemoveComponent<CBotAction>(bot);
                        RequireBuffer<CBotFeedback>(bot, out var buffer);
                        buffer.Add(new CBotFeedback("Can't return more plates hatches are full.", true));
                        EmptyHands(bot);
                        return;
                    }
                }
                else
                {
                    // We are holding something that is not a dirty plate
                    EmptyHands(bot);
                    return;
                }
            }
            else if (cookingSystem.FindNearestItem(CookingSystem.DirtyPlates, pos, out var target, true, CookingSystem.NonKitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target));
            }
            else
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
            }
        }

        private void GoToPlayer(Entity bot, string payload)
        {
            if (!SchemaFactory.Players.TryGetValue(payload, out var ID))
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                Debug.LogError("Error player not in database.");
                return;
            }
            if (GetComponent<CPlayer>(bot).ID == ID)
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                RequireBuffer<CBotFeedback>(bot, out var buffer);
                buffer.Add(new CBotFeedback("Can't go to yourself, you are already there.", true));
                return;
            }
            var Players = PlayerQuery.ToEntityArray(Allocator.Temp);
            foreach (var pl in Players)
            {
                if (Require<CPlayer>(pl, out var comp) && comp.ID == ID)
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(GetComponent<CPosition>(pl).Position.Rounded()));
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    break;
                }
            }
            Players.Dispose();
        }

        private void MenuLeft(Entity bot, string payload)
        {
            var evt = new InputUpdateEvent();
            evt.User = GetComponent<CPlayer>(bot).ID;
            evt.State.MenuLeft = ButtonState.Pressed;
            input.Send(evt);
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void MenuRight(Entity bot, string payload)
        {
            var evt = new InputUpdateEvent();
            evt.User = GetComponent<CPlayer>(bot).ID;
            evt.State.MenuRight = ButtonState.Pressed;
            input.Send(evt);
            EntityManager.RemoveComponent<CBotAction>(bot);
        }
    }
}
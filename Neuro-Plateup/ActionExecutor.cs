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
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class ActionExecutor : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, ServiceQuery, OrderQuery, PatienceQuery, PlayerQuery, HeldItems, BinsQuery;
        private Dictionary<string, Action<Entity, string>> _commands;
        private Vector3 StartingPosition;
        private Dictionary<int, PlayerInfo> PlayerData;
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
                { "prepare_dishes", Prepare },
                { "empty_bin", ClearBin },
                { "wash_plates", WashPlates },
                { "return_plates", ReturnPlates },
                { "drop_item", DropItem }
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
            EntityManager.RemoveComponent<CBotOrders>(bot);
            EntityManager.RemoveComponent<CBotWatching>(bot);
            GetBuffer<CBotItems>(bot).Clear();
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
            // NYI: Clear condiment off of table if something else is requested (Ketchup / Mustard)

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

            var orderSets = new Dictionary<float, OrderList>();
            var Orders = OrderQuery.ToEntityArray(Allocator.Temp);
            foreach (var orderGroup in Orders)
            {
                foreach (var order in GetBuffer<CDisplayedItem>(orderGroup))
                {
                    if (!patienceList.ContainsKey(order.TablePosition))
                        break;

                    var patience = patienceList[order.TablePosition];

                    if (order.IsComplete && !order.ShowExtra)
                        continue;

                    if (!orderSets.ContainsKey(patience))
                    {
                        orderSets[patience] = new OrderList(order.TablePosition);
                    }

                    if (order.ShowExtra)
                        orderSets[patience].Add(new ItemInfo(order.ExtraID));

                    if (order.IsComplete)
                        continue;

                    var comp = GetComponent<CItem>(order.Item);
                    orderSets[patience].Add(new ItemInfo(comp));
                }
            }
            Orders.Dispose();

            if (orderSets.Count < 1)
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
                return;
            }

            if (GetComponentOfHeld<CItem>(bot, out var holding))
            {
                Vector3 target = new Vector3();
                float currentPatience = float.MaxValue;

                foreach (var entry in orderSets.OrderBy(kv => kv.Key))
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
                    EmptyHands(bot);
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }

                // We use this to stall as the result of our serving takes another frame to process
                EntityManager.AddComponent<CBotActionRunning>(bot);

                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target, GrabType.Drop, new ItemInfo(holding)));
            }
            else
            {
                var servables = new Dictionary<Vector3, ItemInfo>();
                var Items = HeldItems.ToEntityArray(Allocator.Temp);
                foreach (var item in Items)
                {
                    var pos = GetComponent<CPosition>(GetComponent<CHeldBy>(item).Holder).Position;
                    if (MoveToSystem.Hatches.Contains(pos))
                    {
                        servables.Add(pos, new ItemInfo(GetComponent<CItem>(item)));
                    }
                }
                Items.Dispose();

                foreach (var entry in CookingSystem.ServeProviders)
                {
                    servables.Add(entry.Value, new ItemInfo(entry.Key));
                }

                if (cookingSystem.FindNearestItem(bot, new ItemInfo(41735497), GetComponent<CPosition>(bot).Position.Rounded(), out var cakePos, true, CookingSystem.NonKitchenRoomTypes))
                {
                    servables.Add(cakePos, new ItemInfo(41735497));
                };

                if (servables.Count < 1)
                {
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }

                Vector3 foodPos = new Vector3();
                Vector3 tablePos = new Vector3();
                ItemInfo targetInfo = new ItemInfo();
                float currentPatience = float.MaxValue;
                foreach (var servable in servables)
                {
                    foreach (var entry in orderSets.OrderBy(kv => kv.Key))
                    {
                        if (entry.Key < currentPatience && entry.Value.Items.Contains(servable.Value))
                        {
                            currentPatience = entry.Key;
                            foodPos = servable.Key;
                            tablePos = entry.Value.Position;
                            targetInfo = servable.Value;
                        }
                    }
                }

                if (currentPatience == float.MaxValue)
                {
                    // Dishes on hatch do not have a delivery target
                    EntityManager.RemoveComponent<CBotAction>(bot);
                    return;
                }

                var ent = TileManager.GetPrimaryOccupant(tablePos);
                if (GetComponentOfHeld<CItem>(ent, out _))
                {
                    // There is something on our serve target
                    EntityManager.AddComponentData(bot, new CMoveTo(foodPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(foodPos, GrabType.Pickup, default, targetInfo));
                    return;
                }

                if (CookingSystem.ServeProviders.Values.Contains(foodPos))
                {
                    cookingSystem.ApplianceCapacity(ent, out var current, out var maximum);
                    if (current != maximum && current == 0 || targetInfo.ID == 41735497)
                    {
                        if (!cookingSystem.FindNearestItem(bot, targetInfo, GetComponent<CPosition>(bot).Position.Rounded(), out foodPos, false, CookingSystem.NonKitchenRoomTypes))
                        {
                            Debug.LogError("Could not find " + targetInfo.ID);
                        }
                    }
                    EntityManager.AddComponentData(bot, new CGrabAction(foodPos, GrabType.Dispense));
                }
                else
                {
                    EntityManager.AddComponentData(bot, new CGrabAction(foodPos, GrabType.Pickup, default, targetInfo));
                }
                EntityManager.AddComponentData(bot, new CMoveTo(foodPos));
            }
        }

        private void Prepare(Entity bot, string payload)
        {
            if (HasBuffer<CBotOrders>(bot))
                return;

            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void Cook(Entity bot, string payload)
        {
            if (HasBuffer<CBotOrders>(bot))
                return;

            if (Has<CBotActionRunning>(bot))
            {
                EntityManager.RemoveComponent<CBotActionRunning>(bot);
                EntityManager.RemoveComponent<CBotAction>(bot);
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
                    orderSets[patience].Add(new ItemInfo(comp));
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
                        if (citem == orderSet[i])
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

            var buffer = EntityManager.AddBuffer<CBotOrders>(bot);
            var tea = 0;
            var boards = 0;
            foreach (var order in orderSet)
            {
                // Teapot is shared by up to 4
                if (order.ID == -908710218)
                {
                    if (tea == 0)
                    {
                        buffer.Add(new CBotOrders(order.ID, order.Items));
                        tea++;
                    }
                    else if (tea == 3)
                    {
                        tea = 0;
                    }
                    else
                    {
                        tea++;
                    }
                    continue;
                }
                // Cheese Board is shared by up to 3
                else if (order.ID == 1639948793)
                {
                    if (boards == 0)
                    {
                        buffer.Add(new CBotOrders(order.ID, order.Items));
                        boards++;
                    }
                    else if (boards == 2)
                    {
                        boards = 0;
                    }
                    else
                    {
                        boards++;
                    }
                    continue;
                }
                buffer.Add(new CBotOrders(order.ID, order.Items));
            }
            EntityManager.AddComponent<CBotActionRunning>(bot);
        }

        private void ClearBin(Entity bot, string payload)
        {
            if (HasComponent<CMoveTo>(bot) || HasComponent<CInteractAction>(bot) || HasComponent<CGrabAction>(bot))
                return;

            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                var pos = GetComponent<CPosition>(bot).Position;
                if (comp.ID == 895813906 && cookingSystem.GetNearestAppliance(pos, CookingSystem.CookingAppliances, out var target, out _, true))
                {
                    cookingSystem.HobInteraction(bot, target, GrabType.Drop);
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
                    EntityManager.AddComponentData(bot, new CGrabAction(outsideBin.Position, GrabType.Fill, new ItemInfo(comp)));
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
                    if (cookingSystem.GetNearestAppliance(pos, CookingSystem.Plates, out var platePos, out var ID, null, CookingSystem.FillStateCheck.IsNotFull))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Fill, new ItemInfo(comp)));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -626784042)
                {
                    if (cookingSystem.GetNearestAppliance(pos, new HashSet<int> { 235423916 }, out var boardPos, out _, null, CookingSystem.FillStateCheck.IsNotFull))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(boardPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(boardPos, GrabType.Fill, new ItemInfo(comp)));
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
                        EntityManager.AddComponentData(bot, new CGrabAction(washerPos, GrabType.Fill, new ItemInfo(comp)));
                    }
                    else if (cookingSystem.GetNearestAppliance(pos, CookingSystem.Sinks, out var sinkPos, out var ID, true))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(sinkPos, GrabType.Drop, new ItemInfo(comp)));
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
                    if (!cookingSystem.GetNearestAppliance(pos, CookingSystem.Bins, out var binPos, out var ID, null, CookingSystem.FillStateCheck.IsNotFull))
                    {
                        binPos = GetComponent<CPosition>(GetEntity<CApplianceExternalBin>());
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(binPos, GrabType.Fill, new ItemInfo(comp)));
                }
                else if (comp.ID == -1527669626)
                {
                    // plate with leftovers
                    if (!cookingSystem.GetNearestAppliance(pos, CookingSystem.Bins, out var binPos, out var ID, null, CookingSystem.FillStateCheck.IsNotFull))
                    {
                        binPos = GetComponent<CPosition>(GetEntity<CApplianceExternalBin>());
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(binPos, GrabType.Fill, new ItemInfo(comp)));
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
            else if (cookingSystem.FindNearestItem(bot, new ItemInfo(793377380), pos, out var sinkPos, false, CookingSystem.Sinks))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                EntityManager.AddComponentData(bot, new CGrabAction(sinkPos, GrabType.Pickup, default, new ItemInfo(793377380)));
            }
            else if (cookingSystem.FindNearestItem(bot, new ItemInfo(1517992271), pos, out var sinkPos2, false, CookingSystem.Sinks))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(sinkPos2));
                EntityManager.AddComponentData(bot, new CInteractAction(sinkPos2, true));
            }
            else if (cookingSystem.FindNearestItem(bot, new ItemInfo(348289471), pos, out var bonePlatePos, false, CookingSystem.KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(bonePlatePos));
                EntityManager.AddComponentData(bot, new CInteractAction(bonePlatePos, true));
            }
            else if (cookingSystem.FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(1517992271), new ItemInfo(-1527669626), new ItemInfo(-626784042) }, pos, out var dirtyPlatePos, out var item, false, CookingSystem.KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(dirtyPlatePos));
                EntityManager.AddComponentData(bot, new CGrabAction(dirtyPlatePos, GrabType.Pickup, default, item));
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
                        EntityManager.AddComponentData(bot, new CGrabAction(hatchPos, GrabType.Drop, new ItemInfo(comp)));
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
                else if (CookingSystem.ServeProviders.ContainsKey(comp.ID))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(CookingSystem.ServeProviders[comp.ID]));
                    EntityManager.AddComponentData(bot, new CGrabAction(CookingSystem.ServeProviders[comp.ID], GrabType.Dispense));
                }
                else if (CookingSystem.Condiments.Contains(comp.ID))
                {
                    if (cookingSystem.GetBestDropOff(pos, out var hatchPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hatchPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hatchPos, GrabType.Drop, new ItemInfo(comp)));
                        return;
                    }
                    else
                    {
                        // All hatches are blocked
                        EntityManager.RemoveComponent<CBotAction>(bot);
                        RequireBuffer<CBotFeedback>(bot, out var buffer);
                        buffer.Add(new CBotFeedback("Can't return condiment hatches are full.", true));
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
            else if (cookingSystem.GetNextDirtyTablePosition(bot, out var target, out var plate))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target, GrabType.Pickup, default, new ItemInfo(plate)));
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
            evt.State.MenuLeft = ButtonState.Released;
            input.Send(evt);
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void MenuRight(Entity bot, string payload)
        {
            var evt = new InputUpdateEvent();
            evt.User = GetComponent<CPlayer>(bot).ID;
            evt.State.MenuRight = ButtonState.Pressed;
            input.Send(evt);
            evt.State.MenuRight = ButtonState.Released;
            input.Send(evt);
            EntityManager.RemoveComponent<CBotAction>(bot);
        }

        private void DropItem(Entity bot, string payload)
        {
            if (GetComponentOfHeld<CItem>(bot, out _))
            {
                EmptyHands(bot);
                return;
            }
            EntityManager.RemoveComponent<CBotAction>(bot);
        }
    }
}
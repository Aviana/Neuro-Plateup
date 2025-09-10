using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using KitchenData;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neuro_Plateup
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public class CookingSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, HeldItems, ResetQuery;

        private MoveToSystem moveTo;

        private static Dictionary<int, Action<Entity, List<ItemInfo>>> MealFunctions;

        public static HashSet<int> CookingAppliances, DishWashers, Sinks, Bins, Counters, WaterProviders, Plates, Tables, DirtyPlates, Trash, NoAppliances;

        public static Dictionary<int, Vector3> ServeProviders = new Dictionary<int, Vector3>();

        public static readonly HashSet<RoomType> AllRoomTypes = new HashSet<RoomType>
        {
            RoomType.Bedroom,
            RoomType.Contracts,
            RoomType.Dining,
            RoomType.FrontEntrance,
            RoomType.Garage,
            RoomType.Garden,
            RoomType.Kitchen,
            RoomType.Locations,
            RoomType.NoRoom,
            RoomType.Office,
            RoomType.SocialSpace,
            RoomType.Storage,
            RoomType.Trophies,
            RoomType.Unassigned,
            RoomType.Workshop
        };
        public static readonly HashSet<RoomType> NonKitchenRoomTypes = new HashSet<RoomType>
        {
            RoomType.Bedroom,
            RoomType.Contracts,
            RoomType.Dining,
            RoomType.FrontEntrance,
            RoomType.Garage,
            RoomType.Garden,
            RoomType.Locations,
            RoomType.NoRoom,
            RoomType.Office,
            RoomType.SocialSpace,
            RoomType.Storage,
            RoomType.Trophies,
            RoomType.Unassigned,
            RoomType.Workshop
        };
        public static readonly HashSet<RoomType> KitchenRoomTypes = new HashSet<RoomType>
        {
            RoomType.Kitchen
        };

        public enum FillStateCheck
        {
            Ignore,
            IsEmpty,
            IsNotEmpty,
            IsFull,
            IsNotFull,
        };

        protected override void Initialise()
        {
            base.Initialise();

            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CBotOrders)
                    ).None(
                        typeof(CMoveTo),
                        typeof(CGrabAction),
                        typeof(CInteractAction)
                    ));
            HeldItems = GetEntityQuery(
                new QueryHelper()
                    .All(
                        //typeof(CItemStorage),
                        typeof(CItem),
                        typeof(CHeldBy)
                    ));
            ResetQuery = GetEntityQuery(
                new QueryHelper()
                    .Any(
                        typeof(CSceneFirstFrame),
                        typeof(SIsDayFirstUpdate),
                        typeof(SIsNightFirstUpdate)
                    ));
            CookingAppliances = new HashSet<int> { 1154757341, -1448690107, 1266458729, 862493270, -441141351, 805530854, 944301512, -1311702572, -1068749602, 782648278, -1688921160 };
            DishWashers = new HashSet<int> { -214126192, -823922901 };
            Sinks = new HashSet<int> { 1083874952, 1467371088, -266993023, 540526865 };
            Bins = new HashSet<int> { 2127051779, -1632826946, -1855909480, 481495292, 1551609169, 620400448, 1159228054, 1492264331 }; // Infinite Bin: 1159228054
            Counters = new HashSet<int> { -1248669347, -1339944542, -1963699221 };
            WaterProviders = new HashSet<int> { 1467371088, 1083874952, -266993023 };
            Plates = new HashSet<int> { 540526865, 380220741, 1313469794 };
            Tables = new HashSet<int> { 209074140, -3721951, -34659638, -203679687, -2019409936 };
            DirtyPlates = new HashSet<int> { 1517992271, -1527669626, 348289471 };
            Trash = new HashSet<int> { 1075166571, -1724190260, -1960690485, -263299406, -1063655063, 936242560, 320607572, 958173724, 469714996, 1770849684, -1755371377, -1140210773, -1370587045, 390623838, -1427780146, -1176063723, -1628910037, -106588634 };
            NoAppliances = new HashSet<int>();

            MealFunctions = new Dictionary<int, Action<Entity, List<ItemInfo>>>
            {
                // NYI: starters and sides
                { -263257027, MandarinFunction },
                { 226055037, MandarinFunction },
                { -1307479546, IceCreamFunction },
                { 1173464355, SteakFunction },
                { 1067846341, SteakFunction },
                { -1034349623, SteakFunction },
                { -783008587, SteakFunction },
                { -1835015742, SaladFunction },
                { 599544171, AppleSaladFunction },
                { -2053442418, PotatoSaladFunction },
                { -1087205958, PizzaFunction },
                { -1938035042, DumplingsFunction },
                { -1293050650, CoffeeFunction }, // Coffee Cup
                { -249136431, CoffeeFunction }, // Affogato
                { 184647209, CoffeeFunction }, // Latte
                { -1388933833, CoffeeFunction }, // Iced Coffee
                { -908710218, TeaFunction },
                { -884392267, BurgerFunction },
                { 1792757441, TurkeyFunction },
                { -1934880099, NutRoastFunction },
                { 861630222, PieFunction },
                { 1366309564, CakeFunction },
                { 1900532137, SpaghettiFunction },
                { -1711635749, BologneseFunction },
                { -383718493, CheesySpaghettiFunction },
                { 82891941, LasagneFunction },
                { 536781335, FishFunction },
                { -1608542149, FishFunction },
                { 1011454010, FishFilletFunction },
                { 403539963, OysterFunction },
                { -491640227, SpinyFishFunction },
                { 1939124686, CrabCakeFunction },
                { 244927287, TacoFunction },
                { 1702578261, HotDogFunction },
                { 1754241573, BreakfastFunction },
                { -361808208, StirFryFunction },
                { 1639948793, CheeseBoardFunction },
                { 82666420, DessertPieFunction },
                { -126602470, DessertPieFunction },
                { 1842093636, DessertPieFunction },
                { -1075930689, GetFromProviderFunction },
                { -1114203942, GetFromProviderFunction },
                { 1190974918, GetFromProviderFunction },
                { 749675166, GetFromProviderFunction },
                { -1721929071, GetFromProviderFunction },
            };
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            moveTo = World.GetExistingSystem<MoveToSystem>();
        }

        protected override void OnUpdate()
        {
            if (!ResetQuery.IsEmptyIgnoreFilter)
            {
                ServeProviders.Clear();
                var pos = GetFrontDoor();
                if (GetNearestAppliance(pos, new HashSet<int> { 1377093570 }, out var cupPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[-1721929071] = cupPos;
                }
                if (GetNearestAppliance(pos, new HashSet<int> { -965827229 }, out var ketchupPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[-1075930689] = ketchupPos;
                }
                if (GetNearestAppliance(pos, new HashSet<int> { -117356585 }, out var mustardPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[-1114203942] = mustardPos;
                }
                if (GetNearestAppliance(pos, new HashSet<int> { -471813067 }, out var soyPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[1190974918] = soyPos;
                }
                if (GetNearestAppliance(pos, new HashSet<int> { 303858729 }, out var crackerPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[749675166] = crackerPos;
                }
            }
            var Bots = BotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in Bots)
            {
                if (HasComponent<CBotWaiting>(bot))
                {
                    var comp = GetComponent<CBotWaiting>(bot);
                    var ent = TileManager.GetPrimaryOccupant(comp.Position);
                    if (!HasComponentOfHeld<CItem>(ent) || GetComponentOfHeld<CItem>(ent, out var cheld) && cheld.ID == comp.itemID)
                    {
                        EntityManager.RemoveComponent<CBotWaiting>(bot);
                        return;
                    }
                    // We are waiting for an unclosed oven - close it.
                    if (HasComponent<CRequiresActivation>(ent) && HasComponent<CIsInactive>(ent))
                    {
                        EntityManager.AddComponentData(bot, new CInteractAction(comp.Position, false));
                    }
                }
                else
                {
                    // NYI: prevent switching type as we prepare the items
                    var buffer = GetBuffer<CBotOrders>(bot);
                    var list = new List<ItemInfo>();

                    foreach (var entry in buffer)
                    {
                        list.Add(new ItemInfo(entry.ID, entry.Items));
                    }

                    var groupedByFunction = list.GroupBy(s => MealFunctions[s.ID]);
                    var orderedByMethodName = groupedByFunction.OrderBy(g => g.Key.Method.Name);
                    var firstGroup = orderedByMethodName.First();

                    firstGroup.Key(bot, firstGroup.ToList());
                }
            }
            Bots.Dispose();
        }

        private void EmptyHands(Entity bot)
        {
            // NYI: return things like the sharp knife
            // NYI: Find a better way to empty the hands
            EntityManager.AddComponent<CReturnItem>(GetComponent<CItemHolder>(bot).HeldItem);
        }

        private bool ApplianceCapacity(Entity appliance, out int current, out int maximum)
        {
            if (Require<CApplianceBin>(appliance, out var compBin))
            {
                current = compBin.CurrentAmount;
                maximum = compBin.Capacity;
                return true;
            }
            else if (Require<CItemProvider>(appliance, out var compProvider))
            {
                current = compProvider.Available;
                maximum = compProvider.Maximum;
                return true;
            }
            // NYI?: prep stations
            // NYI?: preserving station
            current = 0;
            maximum = 0;
            return false;
        }

        public bool GetNearestAppliance(Vector3 sourceTile, HashSet<int> validAppliances, out Vector3 targetPos, out int targetID, bool? hasNoHeld = null, FillStateCheck? fillState = null, HashSet<RoomType> validRoomTypes = null)
        {
            fillState ??= FillStateCheck.Ignore;
            validRoomTypes ??= AllRoomTypes;
            targetPos = new Vector3();
            targetID = new int();

            var Appliances = GetEntityQuery(typeof(CAppliance)).ToEntityArray(Allocator.Temp);
            var flag = false;
            var currentSteps = int.MaxValue;
            foreach (var appliance in Appliances)
            {
                var ID = GetComponent<CAppliance>(appliance).ID;
                if (hasNoHeld == GetComponentOfHeld<CItem>(appliance, out _))
                {
                    continue;
                }
                if (fillState != FillStateCheck.Ignore && ApplianceCapacity(appliance, out var current, out var maximum))
                {
                    switch (fillState)
                    {
                        case FillStateCheck.IsEmpty:
                            if (current != 0)
                                continue;
                            break;
                        case FillStateCheck.IsFull:
                            if (current != maximum)
                                continue;
                            break;
                        case FillStateCheck.IsNotEmpty:
                            if (current == 0)
                                continue;
                            break;
                        case FillStateCheck.IsNotFull:
                            if (current == maximum)
                                continue;
                            break;
                        default:
                            break;
                    }
                }

                if (validAppliances.Contains(ID))
                {
                    var pos = GetComponent<CPosition>(appliance).Position;

                    if (!validRoomTypes.Contains(TileManager.GetTile(pos).Type))
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(sourceTile, pos, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            flag = true;
                            targetPos = pos;
                            targetID = ID;
                        }
                    }
                    else if (steps == 0)
                    {
                        flag = true;
                        targetPos = pos;
                        targetID = ID;
                        break;
                    }
                }
            }
            Appliances.Dispose();
            return flag;
        }

        public bool GetNearestDishWasher(Vector3 start, bool isFull, bool isClean, out Vector3 targetPos)
        {
            targetPos = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Appliances = GetEntityQuery(typeof(CAppliance)).ToEntityArray(Allocator.Temp);
            foreach (var appliance in Appliances)
            {
                if (Require<CAppliance>(appliance, out var comp) && DishWashers.Contains(comp.ID))
                {
                    if (Require<CItemProvider>(appliance, out var comp2))
                    {
                        if (isFull && comp2.Available != comp2.Maximum || isClean && comp2.ProvidedItem == 1517992271)
                        {
                            continue;
                        }
                        var pos = GetComponent<CPosition>(appliance).Position;
                        if (moveTo.GetWaypoint(start, pos, out var wp, out var steps))
                        {
                            if (steps < currentSteps)
                            {
                                currentSteps = steps;
                                flag = true;
                                targetPos = pos;
                            }
                        }
                    }
                }
            }
            Appliances.Dispose();
            return flag;
        }

        public bool FindNearestItem(ItemInfo item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;
            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && item == comp)
                {
                    if (!Require<CHeldBy>(i, out var comp2) || HasComponent<CPlayer>(comp2.Holder))
                    {
                        continue;
                    }

                    if (!Require<CPosition>(comp2.Holder, out var comp3))
                    {
                        continue;
                    }
                    var tile = TileManager.GetTile(comp3.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp3.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    else if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp3.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp3.Position;
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp3.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(ItemInfo item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;
            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && item == comp)
                {
                    if (!Require<CHeldBy>(i, out var comp2) || HasComponent<CPlayer>(comp2.Holder))
                    {
                        continue;
                    }

                    if (!Require<CAppliance>(comp2.Holder, out var comp3) || validAppliances.Count > 0 && !validAppliances.Contains(comp3.ID) || invalidAppliances.Contains(comp3.ID))
                    {
                        continue;
                    }

                    if (!Require<CPosition>(comp2.Holder, out var comp4))
                    {
                        continue;
                    }
                    if (!validRoomTypes.Contains(TileManager.GetTile(comp4.Position).Type))
                    {
                        continue;
                    }

                    var tile = TileManager.GetTile(comp4.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp4.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    else if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp4.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp4.Position;
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp4.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(Entity bot, HashSet<int> list, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            if (TryGetItemMemory(bot, list, out position))
                return true;

            validRoomTypes ??= AllRoomTypes;
            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                if (Require<CItem>(item, out var comp) && list.Contains(comp.ID))
                {
                    if (!Require<CHeldBy>(item, out var comp2) || HasComponent<CPlayer>(comp2.Holder))
                    {
                        continue;
                    }

                    if (!Require<CPosition>(comp2.Holder, out var comp3))
                    {
                        continue;
                    }
                    var tile = TileManager.GetTile(comp3.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp3.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    else if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp3.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp3.Position;
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp3.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(Entity bot, HashSet<int> list, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            if (TryGetItemMemory(bot, list, out position))
            {
                var ent = TileManager.GetPrimaryOccupant(position);
                if (validAppliances.Contains(GetComponent<CAppliance>(ent).ID))
                    return true;
            }

            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;
            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                if (Require<CItem>(item, out var comp) && list.Contains(comp.ID))
                {
                    if (!Require<CHeldBy>(item, out var comp2) || HasComponent<CPlayer>(comp2.Holder))
                    {
                        continue;
                    }

                    if (!Require<CAppliance>(comp2.Holder, out var comp3) || validAppliances.Count > 0 && !validAppliances.Contains(comp3.ID) || invalidAppliances.Contains(comp3.ID))
                    {
                        continue;
                    }

                    if (!Require<CPosition>(comp2.Holder, out var comp4))
                    {
                        continue;
                    }

                    var tile = TileManager.GetTile(comp4.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp4.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    else if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp4.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp4.Position;
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp4.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool GetBestDropOff(Vector3 startpos, out Vector3 target)
        {
            var flag = false;
            target = new Vector3 { };
            var currentSteps = int.MaxValue;
            foreach (var hatch in MoveToSystem.Hatches)
            {
                if (TileManager.GetTile(hatch[0]).Type == RoomType.Kitchen || TileManager.GetTile(hatch[1]).Type == RoomType.Kitchen)
                {
                    var block1 = moveTo.IsBlocked(hatch[0]);
                    var block2 = moveTo.IsBlocked(hatch[1]);
                    Entity ent;
                    if (block1 && !block2)
                    {
                        ent = TileManager.GetPrimaryOccupant(hatch[0]);
                    }
                    else if (!block1 && block2)
                    {
                        ent = TileManager.GetPrimaryOccupant(hatch[1]);
                    }
                    else
                    {
                        continue;
                    }
                    if (HasComponent<CItemHolder>(ent) && !HasComponentOfHeld<CItem>(ent))
                    {
                        var pos = GetComponent<CPosition>(ent).Position;
                        if (moveTo.GetWaypoint(startpos.Rounded(), pos, out var wp, out var steps) || steps == 0)
                        {
                            if (steps < currentSteps)
                            {
                                currentSteps = steps;
                                target = pos;
                                flag = true;
                            }
                        }
                    }
                }
            }
            return flag;
        }

        private void RemoveFromOrder(Entity bot, ItemInfo item)
        {
            if (HasBuffer<CBotOrders>(bot))
            {
                var buffer = GetBuffer<CBotOrders>(bot);
                for (var i = 0; i <= buffer.Length; i++)
                {
                    if (buffer[i].ID == item.ID && buffer[i].Items == item.Items)
                    {
                        buffer.RemoveAt(i);
                        if (buffer.Length == 0)
                        {
                            EntityManager.RemoveComponent<CBotOrders>(bot);
                        }
                        break;
                    }
                }
            }
        }

        private void ClearItemMemory(Entity bot)
        {
            GetBuffer<CBotItems>(bot).Clear();
        }

        private void RemoveItemMemory(Entity bot, int itemID)
        {
            var itemMemory = GetBuffer<CBotItems>(bot);
            for (var i = 0; i < itemMemory.Length; i++)
            {
                if (itemMemory[i].ID == itemID)
                {
                    itemMemory.RemoveAt(i);
                    return;
                }
            }
        }

        private void AddItemMemory(Entity bot, int itemID, Vector3 pos)
        {
            GetBuffer<CBotItems>(bot).Add(new CBotItems(itemID, pos));
        }

        private bool TryGetItemMemory(Entity bot, int itemID, out Vector3 pos)
        {
            pos = new Vector3();
            var itemMemory = GetBuffer<CBotItems>(bot);
            var flag = false;
            foreach (var entry in itemMemory)
            {
                if (entry.ID == itemID)
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp) && comp.ID == entry.ID)
                    {
                        pos = entry.Position;
                        return true;
                    }
                    else
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag)
                RemoveItemMemory(bot, itemID);

            return false;
        }

        private bool TryGetItemMemory(Entity bot, HashSet<int> itemIDs, out Vector3 pos)
        {
            pos = new Vector3();
            var itemMemory = GetBuffer<CBotItems>(bot);
            var flag = false;
            var markedForDeletion = new List<int>();
            foreach (var entry in itemMemory)
            {
                if (itemIDs.Contains(entry.ID))
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp) && comp.ID == entry.ID)
                    {
                        pos = entry.Position;
                        flag = true;
                        break;
                    }
                }
            }
            foreach (var i in markedForDeletion)
            {
                RemoveItemMemory(bot, i);
            }
            return flag;
        }

        private bool HobInteraction(Entity bot, Vector3 pos)
        {
            var ent = TileManager.GetPrimaryOccupant(pos);
            if (HasComponent<CRequiresActivation>(ent) && !HasComponent<CIsInactive>(ent))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(pos));
                EntityManager.AddComponentData(bot, new CInteractAction(pos, false));
                return false;
            }
            else
            {
                EntityManager.AddComponentData(bot, new CMoveTo(pos));
                EntityManager.AddComponentData(bot, new CGrabAction(pos));
                return true;
            }
        }

        private bool GetIcecream(Entity bot, int flavor)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (!GetNearestAppliance(pos, new HashSet<int> { -1533430406 }, out var target, out var ID))
                return false;

            var ent = TileManager.GetPrimaryOccupant(target);
            if (!Require<CVariableProvider>(ent, out var comp))
                return false;

            if (comp.Provide != flavor)
            {
                EntityManager.AddComponentData(bot, new CInteractAction(target, false));
            }
            else
            {
                EntityManager.AddComponentData(bot, new CGrabAction(target));
            }
            EntityManager.AddComponentData(bot, new CMoveTo(target));
            return true;
        }

        private void MandarinFunction(Entity bot, List<ItemInfo> orders)
        {
            var currentOrder = orders[0];
            foreach (var order in orders)
            {
                if (order.Items.Length == 4)
                {
                    currentOrder = order;
                    break;
                }
            }

            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (currentOrder == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos));
                        RemoveFromOrder(bot, currentOrder);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                        RemoveFromOrder(bot, currentOrder);
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1291848678)
                {
                    // Mandarin Raw
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        RemoveFromOrder(bot, currentOrder);
                        EmptyHands(bot);
                    }
                }
                else if (!comp.Items.Contains(448483396))
                {
                    EmptyHands(bot);
                }
                else if (FindNearestItem(bot, new HashSet<int> { 1291848678 }, pos, out var mandarinPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mandarinPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(mandarinPos, true));
                }
                else
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                        AddItemMemory(bot, comp.ID, counterPos);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new HashSet<int> { 1291848678 }, pos, out var fruitPos, false, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, new HashSet<int> { 448483396, -263257027, 226055037 }, pos, out var mandarinPos, true, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(mandarinPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(mandarinPos));
                        return;
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(fruitPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(fruitPos, true));
                }
                else
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { -1210117767 }, out var treePos, out _, null, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(treePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(treePos));
                    }
                    else
                    {
                        Debug.LogError("No Mandarin Provider found in Kitchen!");
                        RemoveFromOrder(bot, currentOrder);
                    }
                }
            }
        }

        private void IceCreamFunction(Entity bot, List<ItemInfo> orders)
        {
            var Items = orders[0].Items;
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID != -1307479546 && comp.ID != 186895094 && comp.ID != 1570518340 && comp.ID != 502129042)
                {
                    EmptyHands(bot);
                    return;
                }
                foreach (var flavor in comp.Items)
                {
                    Items.Remove(flavor);
                }
                if (Items.Length == 0)
                {
                    if (GetBestDropOff(GetComponent<CPosition>(bot).Position.Rounded(), out var pos))
                    {
                        EntityManager.AddComponentData(bot, new CGrabAction(pos));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                }
                else
                {
                    GetIcecream(bot, Items[0]);
                }
            }
            else
            {
                GetIcecream(bot, Items[0]);
            }
        }

        private void SteakFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void SaladFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void AppleSaladFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void PotatoSaladFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void PizzaFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void DumplingsFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void CoffeeFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0].ID == comp.ID)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                }
                else if (comp.ID == 329108931 && GetNearestAppliance(pos, new HashSet<int> { -557736569 }, out var appliancePos, out _, true, null, KitchenRoomTypes))
                {
                    // Milk in hand and empty steamer
                    EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                }
                else if (comp.ID == 364023067)
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { -1609758240 }, out var coffeePos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(coffeePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(coffeePos));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID != -1293050650)
                {
                    EmptyHands(bot);
                }
                else if (orders[0].ID == -249136431)
                {
                    // Affogato
                    GetIcecream(bot, 1570518340);
                }
                else if (orders[0].ID == 184647209)
                {
                    // Latte
                    if (GetNearestAppliance(pos, new HashSet<int> { -557736569 }, out var milkPos, out _, null, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(milkPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(milkPos, false));
                    }
                    else
                    {
                        Debug.LogError("No Milk Steamer found!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    // Iced Coffee
                    if (GetNearestAppliance(pos, new HashSet<int> { 801015432 }, out var icePos, out _, null, null, KitchenRoomTypes))
                    {
                        // NYI: What if there is no ice in the machine?
                        EntityManager.AddComponentData(bot, new CMoveTo(icePos));
                        EntityManager.AddComponentData(bot, new CInteractAction(icePos, false));
                    }
                    else
                    {
                        Debug.LogError("No Ice Dispenser found!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
            else
            {
                if (GetNearestAppliance(pos, new HashSet<int> { -1609758240 }, out var coffeePos, out _, true, null, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(coffeePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(coffeePos));
                }
                else if (GetNearestAppliance(pos, new HashSet<int> { -557736569 }, out _, out _, null, FillStateCheck.IsEmpty, KitchenRoomTypes))
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { 120342736 }, out var milkPos, out _, null, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(milkPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(milkPos));
                    }
                    else
                    {
                        Debug.LogError("No Milk found in Kitchen!");
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (FindNearestItem(bot, new HashSet<int> { -1293050650 }, pos, out var mugPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mugPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(mugPos));
                }
            }
        }

        private void TeaFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID != 707327422 && comp.ID != 712770280)
                {
                    EmptyHands(bot);
                    return;
                }
                if (!comp.Items.Contains(1657174953))
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (!comp.Items.Contains(574857689))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1598460622 }, out var bagPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(bagPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bagPos));
                }
                else
                {
                    if (GetBestDropOff(pos, out var hatchPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hatchPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hatchPos));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                }
            }
            else
            {
                GetNearestAppliance(pos, new HashSet<int> { -762638188 }, out var potPos, out _);
                EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                EntityManager.AddComponentData(bot, new CGrabAction(potPos));
            }
        }

        private void BurgerFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void TurkeyFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void NutRoastFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();

            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                switch (comp.ID)
                {
                    case -1934880099: // plated nut roast
                        {
                            if (GetBestDropOff(pos, out var hatchPos))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(hatchPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(hatchPos));
                                RemoveFromOrder(bot, orders[0]);
                                ClearItemMemory(bot); // just to be safe
                            }
                            else
                            {
                                Debug.LogError("No dropoff location free!");
                            }
                            break;
                        }
                    case -1945246136: // Nut Mixture - Baked
                        {
                            if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                            }
                            break;
                        }
                    case -201067776: // Onion Raw
                    case 609827370: // Nuts Raw
                        {
                            if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(counterPos));
                                AddItemMemory(bot, comp.ID, counterPos);
                            }
                            break;
                        }
                    case -2100850612: // Chopped Nuts
                        {
                            if (FindNearestItem(bot, new HashSet<int> { -1252408744 }, pos, out var appliancePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                                AddItemMemory(bot, -1515496760, appliancePos);
                            }
                            else
                            {
                                EmptyHands(bot);
                            }
                            RemoveItemMemory(bot, -1252408744);
                            break;
                        }
                    case -1252408744: // Chopped Onion
                        {
                            if (FindNearestItem(bot, new HashSet<int> { -2100850612 }, pos, out var appliancePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                                AddItemMemory(bot, -1515496760, appliancePos);
                            }
                            else
                            {
                                EmptyHands(bot);
                            }
                            RemoveItemMemory(bot, -2100850612);
                            break;
                        }
                    case -1515496760: // Nut Mixture
                        {
                            if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                            {
                                if (HobInteraction(bot, hobPos))
                                    EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1945246136));
                            }
                            else
                            {
                                Debug.LogError("No free cooking location!");
                            }
                            break;
                        }
                    case -1294491269: // Nut Mixture - Portion
                        {
                            if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(platePos));
                            }
                            else
                            {
                                Debug.LogError("No filled plate holder found!");
                            }
                            break;
                        }
                    default:
                        {
                            EmptyHands(bot);
                            break;
                        }
                }
            }
            else
            {
                if (FindNearestItem(bot, new HashSet<int> { -1945246136 }, pos, out var hobPos, false, CookingAppliances, NoAppliances, KitchenRoomTypes))
                {
                    HobInteraction(bot, hobPos);
                }
                else if (FindNearestItem(bot, new HashSet<int> { -1294491269 }, pos, out var slicePos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(slicePos));
                }
                else if (FindNearestItem(bot, new HashSet<int> { -1945246136 }, pos, out var roastPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(roastPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(roastPos, true));
                }
                else if (FindNearestItem(bot, new HashSet<int> { -1294491269, -1515496760 }, pos, out var targetPos, false, KitchenRoomTypes))
                {
                    // Nut Mixture
                    // Nut Mixture - Portion
                    EntityManager.AddComponentData(bot, new CMoveTo(targetPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(targetPos));
                    ClearItemMemory(bot);
                }
                else if (
                    FindNearestItem(bot, new HashSet<int> { -2100850612 }, pos, out _, false, KitchenRoomTypes) &&
                    FindNearestItem(bot, new HashSet<int> { -1252408744 }, pos, out var ingredientPos, false, KitchenRoomTypes)
                    )
                {
                    // Nuts - Chopped
                    // Onion - Chopped
                    EntityManager.AddComponentData(bot, new CMoveTo(ingredientPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(ingredientPos));
                    RemoveItemMemory(bot, -1252408744);
                }
                else if (FindNearestItem(bot, new HashSet<int> { -201067776 }, pos, out var onionPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(onionPos, true));
                    RemoveItemMemory(bot, -201067776);
                    AddItemMemory(bot, -1252408744, onionPos);
                }
                else if (FindNearestItem(bot, new HashSet<int> { 609827370 }, pos, out var nutsPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(nutsPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(nutsPos, true));
                    RemoveItemMemory(bot, 609827370);
                    AddItemMemory(bot, -2100850612, nutsPos);
                }
                else if (!TryGetItemMemory(bot, -2100850612, out _)) // Nuts chopped
                {
                    GetNearestAppliance(pos, new HashSet<int> { 1834063794 }, out var nutsSourcePos, out _, null, null, KitchenRoomTypes);
                    EntityManager.AddComponentData(bot, new CMoveTo(nutsSourcePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(nutsSourcePos));
                    AddItemMemory(bot, 609827370, nutsSourcePos);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionSourcePos, out _, null, null, KitchenRoomTypes);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionSourcePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionSourcePos));
                    AddItemMemory(bot, -201067776, onionSourcePos);
                }
            }
        }

        private void PieFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void CakeFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void SpaghettiFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void BologneseFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void CheesySpaghettiFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void LasagneFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void FishFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void FishFilletFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void OysterFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void SpinyFishFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void CrabCakeFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void TacoFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void HotDogFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void BreakfastFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void StirFryFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void CheeseBoardFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void DessertPieFunction(Entity bot, List<ItemInfo> orders)
        {

        }

        private void GetFromProviderFunction(Entity bot, List<ItemInfo> orders)
        {
            var itemInfo = Data.Get<Item>(orders[0].ID);
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] != comp)
                {
                    EmptyHands(bot);
                    return;
                }
                if (GetBestDropOff(pos, out var dropoff))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(dropoff));
                    EntityManager.AddComponentData(bot, new CGrabAction(dropoff));
                    RemoveFromOrder(bot, orders[0]);
                }
                else
                {
                    Debug.LogError("No dropoff location free!");
                }
            }
            else
            {
                if (GetNearestAppliance(pos, new HashSet<int> { itemInfo.DedicatedProvider.ID }, out var providerPos, out _, null, null, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos));
                }
                else
                {
                    Debug.LogError($"Error: Provider for {itemInfo.name} was not found in kitchen!");
                }
            }
        }
    }
}
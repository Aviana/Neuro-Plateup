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
        private EntityQuery BotQuery, HeldItems, ResetQuery, TableQuery, ApplianceQuery;

        private MoveToSystem moveTo;

        private static Dictionary<int, Action<Entity, List<ItemInfo>>> MealFunctions;

        public static HashSet<int> CookingAppliances, DishWashers, Sinks, Bins, Counters, WaterProviders, Plates, Tables, DirtyPlates, Trash, Condiments, NoAppliances;

        public static Dictionary<int, Vector3> ServeProviders = new Dictionary<int, Vector3>();

        public static bool HasCake;

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
            TableQuery = GetEntityQuery(typeof(CApplianceTable));
            ApplianceQuery = GetEntityQuery(typeof(CAppliance));

            CookingAppliances = new HashSet<int> { 1154757341, -1448690107, 1266458729, 862493270, -441141351, 805530854, 944301512, -1311702572, -1068749602, 782648278, -1688921160 };
            DishWashers = new HashSet<int> { -214126192, -823922901 };
            Sinks = new HashSet<int> { 1083874952, 1467371088, -266993023, 540526865 };
            Bins = new HashSet<int> { 2127051779, -1632826946, -1855909480, 481495292, 1551609169, 620400448, 1159228054, 1492264331 }; // Infinite Bin: 1159228054
            Counters = new HashSet<int> { -1573577293, -1248669347, -1339944542, -1963699221 };
            WaterProviders = new HashSet<int> { 1467371088, 1083874952, -266993023 };
            Plates = new HashSet<int> { 540526865, 380220741, 1313469794 };
            Tables = new HashSet<int> { 209074140, -3721951, -34659638, -203679687, -2019409936 };
            DirtyPlates = new HashSet<int> { 1517992271, -1527669626, 348289471, -626784042 };
            Trash = new HashSet<int> { 1075166571, -1724190260, -1960690485, -263299406, -1063655063, 936242560, 320607572, 958173724, 469714996, 1770849684, -1755371377, -1140210773, -1370587045, 390623838, -1427780146, -1176063723, -1628910037, -106588634 };
            Condiments = new HashSet<int> { -1075930689, -1114203942, 1190974918, 41735497 };
            NoAppliances = new HashSet<int>();

            MealFunctions = new Dictionary<int, Action<Entity, List<ItemInfo>>>
            {
                { 1384211889, BroccoliCheeseSoupFunction },
                { 226578993, PrepareBroccoliCheeseSoupFunction },
                { 409276704, CarrotSoupFunction },
                { -1582466042, PrepareCarrotSoupFunction },
                { 1684936685, MeatSoupFunction },
                { -1284423669, PrepareMeatSoupFunction },
                { 790436685, PumpkinSoupFunction },
                { 407468560, PreparePumpkinSoupFunction },
                { 894680043, TomatoSoupFunction },
                { 1752228187, PrepareTomatoSoupFunction },
                { 1503471951, BreadFunction },
                { -1867438686, LoafFunction },
                { 1018675021, PumpkinSeedFunction },
                { -263257027, MandarinFunction },
                { 226055037, MandarinFunction },
                { 2037858460, BambooFunction },
                { 2019756794, PrepareBambooFunction },
                { -1520921913, BroccoliFunction},
                { 98665743, PrepareBroccoliFunction},
                { -259844528, ChipsFunction },
                { -1640761177, CornFunction },
                { 107345299, MashedPotatoFunction },
                { -1341614392, PrepareMashedPotatoFunction },
                { -1086687302, OnionRingsFunction },
                { -939434748, RoastPotatoFunction },
                { -1307479546, IceCreamFunction },
                { 1173464355, SteakFunction }, // Thin Steak
                { 1067846341, SteakFunction }, // Thick Steak
                { -1034349623, SteakFunction }, // Steak
                { -783008587, SteakFunction }, // Boned Steak
                { -1835015742, SaladFunction },
                { 599544171, AppleSaladFunction },
                { -2053442418, PotatoSaladFunction },
                { -1087205958, PizzaFunction }, // Pizza Slice
                { -1196800934, PizzaFunction }, // Pizza Pie
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
                { 82666420, ApplePieFunction },
                { -126602470, PumpkinPieFunction },
                { 1842093636, CherryPieFunction },
                { -1075930689, GetFromProviderFunction }, // Ketchup
                { -1114203942, GetFromProviderFunction }, // Mustard
                { 1190974918, GetFromProviderFunction }, // Soy Souce
                { 749675166, GetFromProviderFunction }, // Christmas Cracker
                { -1721929071, GetFromProviderFunction }, // Tea Cup
                { 41735497, GetFromProviderFunction }, // Cake Stand
                { -849164789, GetFromProviderFunction }, // Sugar
                { 329108931, GetFromProviderFunction } // Milk
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

                HasCake = GetNearestAppliance(pos, new HashSet<int> { 143484231 }, out _, out _, null, null, NonKitchenRoomTypes);

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
                if (GetNearestAppliance(pos, new HashSet<int> { -2133205155 }, out var sugarPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[-849164789] = sugarPos;
                }
                if (GetNearestAppliance(pos, new HashSet<int> { 120342736 }, out var milkPos, out _, null, null, NonKitchenRoomTypes))
                {
                    ServeProviders[329108931] = milkPos;
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
                }
                else
                {
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

            if (GetComponentOfHeld<CItem>(bot, out var held))
            {
                if (ServeProviders.ContainsKey(held.ID))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(ServeProviders[held.ID]));
                    EntityManager.AddComponentData(bot, new CGrabAction(ServeProviders[held.ID], GrabType.Fill));
                    return;
                }
                var item = Data.Get<Item>(held.ID);
                if (!item.IsIndisposable)
                {
                    var pos = GetComponent<CPosition>(bot).Position.Rounded();
                    if (GetNearestAppliance(pos, Bins, out var binPos, out _, null, FillStateCheck.IsNotFull))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(binPos, GrabType.Fill));
                        return;
                    }
                }
                EntityManager.AddComponent<CReturnItem>(GetComponent<CItemHolder>(bot).HeldItem);
            }
        }

        public bool GetNextDirtyTablePosition(Entity bot, out Vector3 position, out CItem plate)
        {
            position = new Vector3();
            plate = default;
            var flag = false;
            var currentSteps = int.MaxValue;

            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            var Tables = TableQuery.ToEntityArray(Allocator.Temp);
            foreach (var t in Tables)
            {
                var tablePos = GetComponent<CPosition>(t).Position;
                if (RequireBuffer<CItemStored>(t, out var buffer) && buffer.Length > 0)
                {
                    foreach (var entry in buffer)
                    {
                        if (Require<CItem>(entry.StoredItem, out var comp) && DirtyPlates.Contains(comp.ID))
                        {
                            if (moveTo.GetWaypoint(pos, tablePos, out _, out var steps) && steps < currentSteps)
                            {
                                currentSteps = steps;
                                position = tablePos;
                                plate = comp;
                                flag = true;
                            }
                            else if (steps == 0)
                            {
                                position = tablePos;
                                Tables.Dispose();
                                plate = comp;
                                return true;
                            }
                            break;
                        }
                    }
                }
                else if (GetComponentOfHeld<CItem>(t, out var comp2) && DirtyPlates.Contains(comp2.ID))
                {
                    if (moveTo.GetWaypoint(pos, tablePos, out _, out var steps) && steps < currentSteps)
                    {
                        currentSteps = steps;
                        position = tablePos;
                        plate = comp2;
                        flag = true;
                    }
                    else if (steps == 0)
                    {
                        position = tablePos;
                        Tables.Dispose();
                        plate = comp2;
                        return true;
                    }
                }
            }
            Tables.Dispose();
            return flag;
        }

        public bool ApplianceCapacity(Entity appliance, out int current, out int maximum)
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
                var pos = GetComponent<CPosition>(appliance).Position;

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

                    if (!validRoomTypes.Contains(TileManager.GetTile(pos).Type) && !MoveToSystem.Hatches.Contains(pos))
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

        public bool GetBestHob(Vector3 start, out Vector3 hobPos, out bool isDanger, bool isLongCook = false)
        {
            isDanger = false;
            if (isLongCook && GetNearestAppliance(start, new HashSet<int> { -1311702572 }, out hobPos, out _, true, null, KitchenRoomTypes))
            {
                return true;
            }
            else if (GetNearestAppliance(start, new HashSet<int> { -1448690107, -1068749602 }, out hobPos, out _, true, null, KitchenRoomTypes))
            {
                isDanger = true;
                return true;
            }
            else if (GetNearestAppliance(start, new HashSet<int> { 862493270 }, out hobPos, out _, true, null, KitchenRoomTypes))
            {
                return true;
            }
            else if (GetNearestAppliance(start, CookingAppliances, out hobPos, out var ID, true, null, KitchenRoomTypes))
            {
                return true;
            }
            return false;
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

        public bool FindNearestItem(Entity bot, int item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, item, out position))
            {
                var tile = TileManager.GetTile(position);
                if (validRoomTypes.Contains(tile.Type))
                    return true;
            }

            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && item == comp.ID)
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

        public bool FindNearestItem(Entity bot, ItemInfo item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, item, out position))
            {
                var tile = TileManager.GetTile(position);
                if (validRoomTypes.Contains(tile.Type))
                    return true;
            }

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

        public bool FindNearestItem(Entity bot, int item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;
            
            if (TryGetItemMemory(bot, item, out position))
            {
                var tile = TileManager.GetTile(position);
                var ID = GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(position)).ID;
                if (validRoomTypes.Contains(tile.Type) && !invalidAppliances.Contains(ID) && (validAppliances.Count == 0 || validAppliances.Contains(ID)))
                    return true;
            }

            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && item == comp.ID)
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

                    var tile = TileManager.GetTile(comp4.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp4.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    if (!validRoomTypes.Contains(tile.Type) && !isHatch)
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

        public bool FindNearestItem(Entity bot, ItemInfo item, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, item, out position))
            {
                var tile = TileManager.GetTile(position);
                var ID = GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(position)).ID;
                if (validRoomTypes.Contains(tile.Type) && !invalidAppliances.Contains(ID) && (validAppliances.Count == 0 || validAppliances.Contains(ID)))
                    return true;
            }

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

                    var tile = TileManager.GetTile(comp4.Position);
                    var isHatch = MoveToSystem.Hatches.Contains(comp4.Position);

                    if (noHatches && isHatch)
                    {
                        continue;
                    }
                    if (!validRoomTypes.Contains(tile.Type) && !isHatch)
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

        public bool FindNearestItem(Entity bot, HashSet<int> list, Vector3 start, out Vector3 position, out ItemInfo item, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, list, out position, out item))
            {
                var tile = TileManager.GetTile(position);
                if (validRoomTypes.Contains(tile.Type))
                    return true;
            }

            position = new Vector3();
            item = default;
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && list.Contains(comp.ID))
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
                    if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp3.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp3.Position;
                            item = new ItemInfo(comp);
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp3.Position;
                        item = new ItemInfo(comp);
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(Entity bot, HashSet<ItemInfo> list, Vector3 start, out Vector3 position, out ItemInfo item, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, list, out position, out item))
            {
                var tile = TileManager.GetTile(position);
                if (validRoomTypes.Contains(tile.Type))
                    return true;
            }

            position = new Vector3();
            item = default;
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && list.Contains(comp))
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
                    if (!validRoomTypes.Contains(tile.Type) && !isHatch)
                    {
                        continue;
                    }

                    if (moveTo.GetWaypoint(start, comp3.Position, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = comp3.Position;
                            item = new ItemInfo(comp);
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp3.Position;
                        item = new ItemInfo(comp);
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(Entity bot, HashSet<int> list, Vector3 start, out Vector3 position, out ItemInfo item, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, list, out position, out item))
            {
                var tile = TileManager.GetTile(position);
                var ID = GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(position)).ID;
                if (validRoomTypes.Contains(tile.Type) && !invalidAppliances.Contains(ID) && (validAppliances.Count == 0 || validAppliances.Contains(ID)))
                    return true;
            }

            position = new Vector3();
            item = default;
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && list.Contains(comp.ID))
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
                            item = new ItemInfo(comp);
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp4.Position;
                        item = new ItemInfo(comp);
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(Entity bot, HashSet<ItemInfo> list, Vector3 start, out Vector3 position, out ItemInfo item, bool noHatches = true, HashSet<int> validAppliances = null, HashSet<int> invalidAppliances = null, HashSet<RoomType> validRoomTypes = null)
        {
            validAppliances ??= NoAppliances;
            invalidAppliances ??= NoAppliances;
            validRoomTypes ??= AllRoomTypes;

            if (TryGetItemMemory(bot, list, out position, out item))
            {
                var tile = TileManager.GetTile(position);
                var ID = GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(position)).ID;
                if (validRoomTypes.Contains(tile.Type) && !invalidAppliances.Contains(ID) && (validAppliances.Count == 0 || validAppliances.Contains(ID)))
                    return true;
            }

            position = new Vector3();
            item = default;
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var i in Items)
            {
                if (Require<CItem>(i, out var comp) && list.Contains(comp))
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
                            item = new ItemInfo(comp);
                            flag = true;
                        }
                    }
                    else if (steps == 0)
                    {
                        position = comp4.Position;
                        item = new ItemInfo(comp);
                        currentSteps = 0;
                        flag = true;
                        break;
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool GetBestStorage(Vector3 startpos, out Vector3 target)
        {
            var flag = false;
            target = new Vector3 { };
            var currentSteps = int.MaxValue;
            var currentStepsHatch = int.MaxValue;

            var Appliances = ApplianceQuery.ToEntityArray(Allocator.Temp);
            foreach (var a in Appliances)
            {
                if (GetComponentOfHeld<CItem>(a, out _))
                    continue;

                if (Require<CAppliance>(a, out var comp) && Counters.Contains(comp.ID))
                {
                    var pos = GetComponent<CPosition>(a).Position;
                    bool isKitchen = TileManager.GetTile(pos).Type == RoomType.Kitchen;
                    bool isHatch = MoveToSystem.Hatches.Contains(pos);

                    if (isKitchen && !isHatch)
                    {
                        if (moveTo.GetWaypoint(startpos.Rounded(), pos, out _, out var steps) || steps == 0)
                        {
                            if (steps < currentSteps)
                            {
                                currentSteps = steps;
                                target = pos;
                                flag = true;
                            }
                        }
                    }
                    else if (currentSteps == int.MaxValue && isHatch)
                    {
                        if (moveTo.GetWaypoint(startpos.Rounded(), pos, out _, out var steps) || steps == 0)
                        {
                            if (steps < currentStepsHatch)
                            {
                                currentStepsHatch = steps;
                                target = pos;
                                flag = true;
                            }
                        }
                    }
                }
            }
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

        private void RemoveItemMemory(Entity bot, ItemInfo item)
        {
            var itemMemory = GetBuffer<CBotItems>(bot);
            for (var i = 0; i < itemMemory.Length; i++)
            {
                if (itemMemory[i].Item == item)
                {
                    itemMemory.RemoveAt(i);
                    return;
                }
            }
        }

        private void UpdateItemMemory(Entity bot, int item, Vector3 pos, int ingredient)
        {
            var itemMemory = GetBuffer<CBotItems>(bot);
            foreach (var entry in itemMemory)
            {
                if (entry.Item == item && entry.Position == pos)
                {
                    entry.Item.Items.Add(ingredient);
                }
            }
        }

        private void AddItemMemory(Entity bot, ItemInfo item, Vector3 pos)
        {
            GetBuffer<CBotItems>(bot).Add(new CBotItems(item, pos));
        }

        private int GetNumItemsInMemory(Entity bot, ItemInfo item)
        {
            int num = 0;
            var itemMemory = GetBuffer<CBotItems>(bot);
            foreach (var entry in itemMemory)
            {
                if (entry.Item == item)
                    num ++;
            }
            return num;
        }

        private bool TryGetItemMemory(Entity bot, int item, out Vector3 pos)
        {
            pos = new Vector3();
            var itemMemory = GetBuffer<CBotItems>(bot);
            foreach (var entry in itemMemory)
            {
                if (entry.Item.ID == item)
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp) && entry.Item.ID == comp.ID)
                    {
                        pos = entry.Position;
                        return true;
                    }
                    else
                    {
                        RemoveItemMemory(bot, entry.Item);
                        break;
                    }
                }
            }

            return false;
        }

        private bool TryGetItemMemory(Entity bot, HashSet<int> items, out Vector3 pos, out ItemInfo item)
        {
            pos = new Vector3();
            item = default;
            var itemMemory = GetBuffer<CBotItems>(bot);
            var flag = false;
            var markedForDeletion = new List<ItemInfo>();
            foreach (var entry in itemMemory)
            {
                if (items.Contains(entry.Item.ID))
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp))
                    {
                        if (comp.ID == entry.Item.ID)
                        {
                            pos = entry.Position;
                            item = entry.Item;
                            flag = true;
                            break;
                        }
                        else
                        {
                            markedForDeletion.Add(entry.Item);
                        }
                    }
                }
            }
            foreach (var i in markedForDeletion)
            {
                RemoveItemMemory(bot, i);
            }
            return flag;
        }

        private bool TryGetItemMemory(Entity bot, ItemInfo item, out Vector3 pos)
        {
            pos = new Vector3();
            var itemMemory = GetBuffer<CBotItems>(bot);
            var flag = false;
            foreach (var entry in itemMemory)
            {
                if (entry.Item == item)
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp) && entry.Item == comp)
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
                RemoveItemMemory(bot, item);

            return false;
        }

        private bool TryGetItemMemory(Entity bot, HashSet<ItemInfo> items, out Vector3 pos, out ItemInfo item)
        {
            pos = new Vector3();
            item = default;
            var itemMemory = GetBuffer<CBotItems>(bot);
            var flag = false;
            var markedForDeletion = new List<ItemInfo>();
            foreach (var entry in itemMemory)
            {
                if (items.Contains(entry.Item))
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var comp))
                    {
                        if (comp == entry.Item)
                        {
                            pos = entry.Position;
                            item = entry.Item;
                            flag = true;
                            break;
                        }
                        else
                        {
                            markedForDeletion.Add(entry.Item);
                        }
                    }
                }
            }
            foreach (var i in markedForDeletion)
            {
                RemoveItemMemory(bot, i);
            }
            return flag;
        }

        private bool WatchedCheck(Entity bot)
        {
            if (RequireBuffer<CBotWatching>(bot, out var buffer))
            {
                Vector3 pos = new Vector3();
                bool flag = false;
                var markedForDeletion = new List<Vector3>();

                foreach (var entry in buffer)
                {
                    var ent = TileManager.GetPrimaryOccupant(entry.Position);
                    if (GetComponentOfHeld<CItem>(ent, out var held))
                    {
                        if (held.ID == entry.itemID)
                        {
                            pos = entry.Position;
                            flag = true;
                            markedForDeletion.Add(pos);
                            break;
                        }
                    }
                    else
                    {
                        markedForDeletion.Add(entry.Position);
                    }
                }
                if (markedForDeletion.Count > 0)
                {
                    if (flag && !HobInteraction(bot, pos, GrabType.Pickup))
                        return true;

                    for (var i = 0; i < buffer.Length; i++)
                    {
                        if (markedForDeletion.Contains(buffer[i].Position))
                        {
                            buffer.RemoveAt(i);
                        }
                    }
                    if (buffer.Length == 0)
                        EntityManager.RemoveComponent<CBotWatching>(bot);

                    return flag;
                }
            }
            return false;
        }

        public bool HobInteraction(Entity bot, Vector3 pos, GrabType grab)
        {
            var ent = TileManager.GetPrimaryOccupant(pos);
            ItemInfo compHeld = default;
            ItemInfo compAppl = default;

            if (GetComponentOfHeld<CItem>(bot, out var comp))
                compHeld = new ItemInfo(comp);

            if (GetComponentOfHeld<CItem>(ent, out comp))
                compAppl = new ItemInfo(comp);

            if (HasComponent<CRequiresActivation>(ent))
            {
                if (!HasComponent<CIsInactive>(ent))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pos));
                    EntityManager.AddComponentData(bot, new CInteractAction(pos, false));
                    return false;
                }
                else
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pos));
                    EntityManager.AddComponentData(bot, new CGrabAction(pos, grab, compHeld, compAppl));
                    if (grab == GrabType.Drop || grab == GrabType.CombineDrop)
                        EntityManager.AddComponentData(bot, new CInteractAction(pos, false));
                    return true;
                }
            }
            else
            {
                EntityManager.AddComponentData(bot, new CMoveTo(pos));
                EntityManager.AddComponentData(bot, new CGrabAction(pos, grab, compHeld, compAppl));
                return true;
            }
        }

        private bool GetIcecream(Entity bot, int flavor)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (!GetNearestAppliance(pos, new HashSet<int> { -1533430406 }, out var target, out var ID))
                return false;

            var ent = TileManager.GetPrimaryOccupant(target);
            if (!Require<CVariableProvider>(ent, out var provider))
                return false;

            if (provider.Provide != flavor)
            {
                EntityManager.AddComponentData(bot, new CInteractAction(target, false));
            }
            else
            {
                if (GetComponentOfHeld<CItem>(bot, out var comp))
                    EntityManager.AddComponentData(bot, new CGrabAction(target, GrabType.Dispense, new ItemInfo(comp)));
                else
                    EntityManager.AddComponentData(bot, new CGrabAction(target, GrabType.Dispense));
            }
            EntityManager.AddComponentData(bot, new CMoveTo(target));
            return true;
        }

        private void PrepareBroccoliCheeseSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 226578993)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var sinkPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(sinkPos, false));
                }
                else if (comp.ID == -719587509 || comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -69847810));
                        }
                    }
                }
                else if (comp.ID == -69847810)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -117339838 }, out var cheesePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(cheesePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(cheesePos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == -452280071)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(1030599135, -69847810, -755280170))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1573812073 }, out var broccoliPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(broccoliPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(broccoliPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(1030599135, -69847810, -755280170, -1774883004))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 226578993));
                        }
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, 226578993, pos, out var soupPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, soupPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                }
                else if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(brothPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(brothPos, GrabType.Pickup, default, new ItemInfo(-69847810)));
                }
                else
                {
                    if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.Log("No clean pot available");
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
        }

        private void BroccoliCheeseSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, 226578993, pos, out var soupPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(soupPos));
                EntityManager.AddComponentData(bot, new CInteractAction(soupPos, true));
                return;
            }
            PrepareBroccoliCheeseSoupFunction(bot, new List<ItemInfo> { });
        }

        private void PrepareCarrotSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1582466042)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var sinkPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(sinkPos, false));
                }
                else if (comp.ID == -719587509 || comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -69847810));
                        }
                    }
                }
                else if (comp.ID == -69847810)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -452101383 }, out var carrotPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(carrotPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == -1361723814)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1582466042));
                        }
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == -452280071)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
            }
            else
            {
                if (FindNearestItem(bot, -1582466042, pos, out var soupPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, soupPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                }
                else if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(brothPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(brothPos, GrabType.Pickup, default, new ItemInfo(-69847810)));
                }
                else
                {
                    if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.Log("No clean pot available");
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
        }

        private void CarrotSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, -1582466042, pos, out var soupPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(soupPos));
                EntityManager.AddComponentData(bot, new CInteractAction(soupPos, true));
                return;
            }
            PrepareCarrotSoupFunction(bot, new List<ItemInfo> { });
        }

        private void PrepareMeatSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1284423669)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var sinkPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(sinkPos, false));
                }
                else if (comp.ID == -719587509 || comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -69847810));
                        }
                    }
                }
                else if (comp.ID == -69847810)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -484165118 }, out var meatPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(meatPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == 1064697910)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1284423669));
                        }
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == -452280071)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
            }
            else
            {
                if (FindNearestItem(bot, -1284423669, pos, out var soupPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, soupPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                }
                else if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(brothPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(brothPos, GrabType.Pickup, default, new ItemInfo(-69847810)));
                }
                else
                {
                    if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.Log("No clean pot available");
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
        }

        private void MeatSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, -1284423669, pos, out var soupPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(soupPos));
                EntityManager.AddComponentData(bot, new CInteractAction(soupPos, true));
                return;
            }
            PrepareMeatSoupFunction(bot, new List<ItemInfo> { });
        }

        private void PreparePumpkinSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 407468560 || comp.ID == -165143951)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        if (comp.ID == -165143951)
                            AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0 && comp.ID == 407468560)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -711877651)
                {
                    if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, brothPos, GrabType.CombineDrop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(brothPos, 407468560));
                        }
                    }
                    else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(-719587509, 1657174953, 1859809622), new ItemInfo(1370203151, 1657174953, -201067776, -486398094) }, pos, out brothPos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CBotWaiting(brothPos, -69847810));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -1498186615)
                {
                    EmptyHands(bot);
                }
                else if (comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var sinkPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(sinkPos, false));
                }
                else if (comp.ID == -719587509 || comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            AddItemMemory(bot, new ItemInfo(comp), hobPos);
                        }
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == -452280071)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
            }
            else
            {
                if (FindNearestItem(bot, 407468560, pos, out var soupPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, soupPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(-719587509, 1657174953, 1859809622), new ItemInfo(1370203151, 1657174953, -201067776, -486398094), new ItemInfo(-69847810) }, pos, out _, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, -711877651, pos, out var slicePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(slicePos, GrabType.Pickup, default, new ItemInfo(-711877651)));
                        ClearItemMemory(bot);
                    }
                    else if (FindNearestItem(bot, 951737916, pos, out var hollowPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hollowPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(hollowPos, true));
                        RemoveItemMemory(bot, new ItemInfo(951737916));
                        AddItemMemory(bot, new ItemInfo(-711877651), hollowPos);
                    }
                    else if (TryGetItemMemory(bot, -165143951, out var pumpkinPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(pumpkinPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(pumpkinPos, true));
                        RemoveItemMemory(bot, new ItemInfo(-165143951));
                        AddItemMemory(bot, new ItemInfo(951737916), pumpkinPos);
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -1055654549 }, out var providerPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                }
                else if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                }
                else if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(brothPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(brothPos, GrabType.Pickup, default, new ItemInfo(-69847810)));
                }
                else
                {
                    if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.Log("No clean pot available");
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
        }

        private void PumpkinSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, 407468560, pos, out var soupPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(soupPos));
                EntityManager.AddComponentData(bot, new CInteractAction(soupPos, true));
                return;
            }
            PreparePumpkinSoupFunction(bot, new List<ItemInfo> { });
        }

        private void PrepareTomatoSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 1752228187 || comp.ID == 1242961771)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        if (comp.ID == 1242961771)
                            AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0 && comp.ID == 1752228187)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -1317168923)
                {
                    if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        HobInteraction(bot, brothPos, GrabType.CombineDrop);
                    }
                    else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(-719587509, 1657174953, 1859809622), new ItemInfo(1370203151, 1657174953, -201067776, -486398094) }, pos, out brothPos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CBotWaiting(brothPos, -69847810));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var sinkPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(sinkPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(sinkPos, false));
                }
                else if (comp.ID == -719587509 || comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            AddItemMemory(bot, new ItemInfo(comp), hobPos);
                        }
                    }
                }
                else if (comp == new ItemInfo(-1863787598, -1317168923, -69847810))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -712909563 }, out var tomatoPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(tomatoPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(-1863787598, -1317168923, -69847810, 1242961771))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, null, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 1752228187));
                        }
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == -452280071)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
            }
            else
            {
                if (FindNearestItem(bot, 1752228187, pos, out var soupPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, soupPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(-719587509, 1657174953, 1859809622), new ItemInfo(1370203151, 1657174953, -201067776, -486398094), new ItemInfo(-69847810) }, pos, out _, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, -1317168923, pos, out var soucePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(soucePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(soucePos, GrabType.Pickup, default, new ItemInfo(-1317168923)));
                        ClearItemMemory(bot);
                    }
                    else if (FindNearestItem(bot, -853757044, pos, out var slicePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                        EntityManager.AddComponentData(bot, new CInteractAction(slicePos, true));
                        RemoveItemMemory(bot, new ItemInfo(-853757044));
                        AddItemMemory(bot, new ItemInfo(-1317168923), slicePos);
                    }
                    else if (TryGetItemMemory(bot, 1242961771, out var tomatoPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(tomatoPos, true));
                        RemoveItemMemory(bot, new ItemInfo(1242961771));
                        AddItemMemory(bot, new ItemInfo(-853757044), tomatoPos);
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -712909563 }, out var providerPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-1863787598, -1317168923, -69847810), pos, out var uncookedPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, uncookedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                }
                else if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(brothPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(brothPos, GrabType.Pickup, default, new ItemInfo(-69847810)));
                }
                else
                {
                    if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.Log("No clean pot available");
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
        }

        private void TomatoSoupFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, 1752228187, pos, out var soupPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(soupPos));
                EntityManager.AddComponentData(bot, new CInteractAction(soupPos, true));
                return;
            }
            PrepareTomatoSoupFunction(bot, new List<ItemInfo> { });
        }

        private void LoafFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1867438686)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.ID == 1296980128)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1867438686));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, -1867438686, pos, out var loafPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, loafPos, GrabType.Pickup);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void BreadFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp == new ItemInfo(1503471951, -306959510, -306959510, -626784042))
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
                else if (comp.ID == -626784042 || comp == new ItemInfo(1503471951, -306959510, -626784042))
                {
                    if (FindNearestItem(bot, new ItemInfo(-1867438686), pos, out var loafPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(loafPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(loafPos, true));
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-306959510), pos, out var slicePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                        EntityManager.AddComponentData(bot, new CInteractAction(slicePos, false));
                    }
                    else
                    {
                        if (GetBestStorage(pos, out var counterPos) && comp.ID == 1503471951)
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(comp), counterPos);
                        }
                        else
                        {
                            EmptyHands(bot);
                        }
                    }
                    return;
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1867438686), pos, out _, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    if (TryGetItemMemory(bot, 1503471951, out var unfinishedPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(unfinishedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(unfinishedPos, GrabType.Pickup, default, new ItemInfo(1503471951)));
                        ClearItemMemory(bot);
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-626784042), pos, out var boardPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(boardPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(boardPos, GrabType.Pickup, default, new ItemInfo(-626784042)));
                    }
                    else if (GetNearestAppliance(pos, new HashSet<int> { 235423916 }, out var providerPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.LogWarning("No free boards!");
                        RemoveFromOrder(bot, orders[0]);
                    }
                    return;
                }
            }
            LoafFunction(bot, new List<ItemInfo> { });
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
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                        RemoveFromOrder(bot, currentOrder);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
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
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
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
                else if (FindNearestItem(bot, new ItemInfo(1291848678), pos, out var mandarinPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mandarinPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(mandarinPos, true));
                }
                else
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(1291848678), pos, out var fruitPos, false, KitchenRoomTypes))
                {
                    if (TryGetItemMemory(bot, new HashSet<int> { 448483396, -263257027, 226055037 }, out var mandarinPos, out var item))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(mandarinPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(mandarinPos, GrabType.Pickup, default, item));
                        ClearItemMemory(bot);
                        return;
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(fruitPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(fruitPos, true));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1210117767 }, out var treePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(treePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(treePos, GrabType.Dispense));
                }
            }
        }

        private void PumpkinSeedFunction(Entity bot, List<ItemInfo> orders)
        {
            // NYI: Handle the whole seed + pieces situation
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 1018675021)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -165143951)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(-165143951), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1498186615)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 1018675021));
                            AddItemMemory(bot, new ItemInfo(1018675021), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 951737916)
                {
                    if (GetNearestAppliance(pos, Bins, out var binPos, out _, null, FillStateCheck.IsNotFull))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(binPos, GrabType.Fill, new ItemInfo(comp)));
                    }
                    else
                    {
                        Require<CPosition>(GetSingletonEntity<CApplianceExternalBin>(), out var outsideBin);
                        EntityManager.AddComponentData(bot, new CMoveTo(outsideBin));
                        EntityManager.AddComponentData(bot, new CGrabAction(outsideBin, GrabType.Fill, new ItemInfo(comp)));
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(1018675021), pos, out var roastedPos, true, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, roastedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(1018675021), pos, out roastedPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(roastedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(roastedPos, GrabType.Pickup, default, new ItemInfo(1018675021)));
                }
                else if (FindNearestItem(bot, new ItemInfo(-1498186615), pos, out var seedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(seedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(seedPos, GrabType.Pickup, default, new ItemInfo(-1498186615)));
                }
                else if (TryGetItemMemory(bot, new ItemInfo(-165143951), out var pumpkinPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pumpkinPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(pumpkinPos, true));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1055654549 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void PrepareBambooFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 2019756794)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.Items.IsEquivalent(new FixedListInt64 { 1657174953, -486398094 }))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2092567672 }, out var bombooPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(bombooPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bombooPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(-1652763586, -486398094, 1657174953, -1635701703))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 2019756794));
                            AddItemMemory(bot, new ItemInfo(2019756794), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(2019756794), pos, out var potPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, potPos, GrabType.Pickup))
                    {
                        RemoveItemMemory(bot, new ItemInfo(2019756794));
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-486398094), pos, out var emptyPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(emptyPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(emptyPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty);
                    EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                }
            }
        }

        private void BambooFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 2037858460)
                {
                    // NYI: Pot cleanup?
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, new ItemInfo(2019756794), pos, out var potPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                EntityManager.AddComponentData(bot, new CInteractAction(potPos, true));
                return;
            }
            PrepareBambooFunction(bot, new List<ItemInfo> { });
        }

        private void PrepareBroccoliFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 98665743)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.Items.IsEquivalent(new FixedListInt64 { 1657174953, -486398094 }))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1573812073 }, out var broccoliPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(broccoliPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(broccoliPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(2141493703, -1774883004, 1657174953, -486398094))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 98665743));
                            AddItemMemory(bot, new ItemInfo(98665743), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(98665743), pos, out var potPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, potPos, GrabType.Pickup))
                    {
                        RemoveItemMemory(bot, new ItemInfo(98665743));
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-486398094), pos, out var emptyPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(emptyPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(emptyPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var stackPos, out _, null, FillStateCheck.IsNotEmpty);
                    EntityManager.AddComponentData(bot, new CMoveTo(stackPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(stackPos, GrabType.Dispense));
                }
            }
        }

        private void BroccoliFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1520921913)
                {
                    // NYI: Pot cleanup?
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else if (FindNearestItem(bot, new ItemInfo(98665743), pos, out var potPos, false, null, CookingAppliances, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                EntityManager.AddComponentData(bot, new CInteractAction(potPos, true));
                return;
            }
            PrepareBroccoliFunction(bot, new List<ItemInfo> { });
        }

        private void ChipsFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -259844528)
                {
                    // NYI: Pot cleanup?
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -1972529263)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(-1972529263), counterPos);
                    }
                    else
                    {
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 35611244)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -259844528));
                            AddItemMemory(bot, new ItemInfo(-259844528), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-259844528), pos, out var friesPos, true, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, friesPos, GrabType.Pickup))
                    {
                        ClearItemMemory(bot);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-259844528), pos, out friesPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(friesPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(friesPos, GrabType.Pickup, default, new ItemInfo(-259844528)));
                    ClearItemMemory(bot);
                }
                else if (FindNearestItem(bot, new ItemInfo(35611244), pos, out var slicedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(slicedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(slicedPos, GrabType.Pickup, default, new ItemInfo(35611244)));
                    ClearItemMemory(bot);
                }
                else if (TryGetItemMemory(bot, new ItemInfo(-1972529263), out var potatoPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(potatoPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(potatoPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(35611244), potatoPos);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 44541785 }, out var rawPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(rawPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(rawPos, GrabType.Dispense));
                }
            }
        }

        private void CornFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1640761177)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 529258958)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(529258958), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1075166571)
                {
                    if (GetNearestAppliance(pos, Bins, out var binPos, out _, null, FillStateCheck.IsNotFull))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(binPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(binPos, GrabType.Fill, new ItemInfo(comp)));
                    }
                    else
                    {
                        Require<CPosition>(GetSingletonEntity<CApplianceExternalBin>(), out var outsideBin);
                        EntityManager.AddComponentData(bot, new CMoveTo(outsideBin));
                        EntityManager.AddComponentData(bot, new CGrabAction(outsideBin, GrabType.Fill, new ItemInfo(comp)));
                    }
                }
                else if (comp.ID == -1854029532)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1640761177));
                            AddItemMemory(bot, new ItemInfo(-1640761177), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1640761177), pos, out var cookedPos, true, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, cookedPos, GrabType.Pickup))
                    {
                        ClearItemMemory(bot);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-1640761177), pos, out cookedPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(cookedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(cookedPos, GrabType.Pickup, default, new ItemInfo(-1640761177)));
                    ClearItemMemory(bot);
                }
                else if (TryGetItemMemory(bot, new ItemInfo(529258958), out var cornPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(cornPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(cornPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(-1854029532), cornPos);
                }
                else if (FindNearestItem(bot, new ItemInfo(-1854029532), pos, out var strippedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(strippedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(strippedPos, GrabType.Pickup, default, new ItemInfo(-1854029532)));
                    ClearItemMemory(bot);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 976574457 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void PrepareMashedPotatoFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == -1965870011)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.Items.IsEquivalent(new FixedListInt64 { 1657174953, -486398094 }))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 44541785 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(-735644169, -1972529263, 1657174953, -486398094))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1965870011));
                            AddItemMemory(bot, new ItemInfo(-1965870011), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        if (orders.Count > 0)
                            RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, -1965870011, pos, out var boiledPotPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, boiledPotPos, GrabType.Pickup))
                    {
                        RemoveItemMemory(bot, new ItemInfo(-1965870011));
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-1965870011), pos, out var boiledPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(boiledPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(boiledPos, true));
                    if (orders.Count > 0)
                        RemoveFromOrder(bot, orders[0]);
                }
                else if (FindNearestItem(bot, new ItemInfo(-486398094), pos, out var potPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var providerPos, out _, null, FillStateCheck.IsNotEmpty);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void MashedPotatoFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == 107345299)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                    return;
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1341614392), pos, out var mashedPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mashedPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(mashedPos, true));
                    return;
                }
            }
            PrepareMashedPotatoFunction(bot, new List<ItemInfo> { });
        }

        private void OnionRingsFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -1086687302)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1818895897)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1086687302));
                            AddItemMemory(bot, new ItemInfo(-1086687302), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1252408744)
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == -201067776)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(-201067776), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1086687302), pos, out var ringPos, true, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, ringPos, GrabType.Pickup))
                    {
                        ClearItemMemory(bot);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-1086687302), pos, out ringPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(ringPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(ringPos, GrabType.Pickup, default, new ItemInfo(-1086687302)));
                    ClearItemMemory(bot);
                }
                else if (TryGetItemMemory(bot, new ItemInfo(-201067776), out var onionPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(onionPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(-1252408744), onionPos);
                }
                else if (FindNearestItem(bot, new ItemInfo(-1252408744), pos, out var choppedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(choppedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(choppedPos, GrabType.Pickup, default, new ItemInfo(-1252408744)));
                    ClearItemMemory(bot);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void RoastPotatoFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.ID == -939434748)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -1972529263)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -939434748));
                            AddItemMemory(bot, new ItemInfo(-939434748), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-939434748), pos, out var potatoPos, true, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (HobInteraction(bot, potatoPos, GrabType.Pickup))
                    {
                        ClearItemMemory(bot);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-939434748), pos, out potatoPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(potatoPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(potatoPos, GrabType.Pickup, default, new ItemInfo(-939434748)));
                    ClearItemMemory(bot);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 44541785 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
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
                        EntityManager.AddComponentData(bot, new CGrabAction(pos, GrabType.Drop, new ItemInfo(comp)));
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
            int pizzaType;
            if (orders[0].Items.Contains(-336580972))
                pizzaType = -2093899333; // Mushroom
            else if (orders[0].Items.Contains(-1633089577))
                pizzaType = -1252408744; // Onion
            else
                pizzaType = 263830100; // Plain

            var baked = orders[0].Items;
            baked.Remove(793377380);

            var unbaked = new FixedListInt64 { 263830100, -48499881, -1317168923 };
            if (pizzaType != 263830100)
                unbaked.Add(pizzaType);

            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (comp.Items.Contains(793377380))
                    {
                        if (GetBestDropOff(pos, out var dropPos))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                        }
                        else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                        {
                            Debug.LogError("No hatch free, dropping on next free counter");
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        }
                        else
                        {
                            Debug.LogError("No dropoff location free!");
                            EmptyHands(bot);
                        }
                    }
                    else
                    {
                        if (GetBestStorage(pos, out var dropPos))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                        }
                        else
                        {
                            Debug.LogError("No dropoff location free!");
                            EmptyHands(bot);
                        }
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp == new ItemInfo(938942828, baked)) // Slice
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No plates found in kitchen!");
                    }
                }
                else if (comp == new ItemInfo(-1196800934, baked)) // baked pizza
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 445221203 && comp.Items.Contains(pizzaType)) // unbaked pizza
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1196800934));
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                    }
                }
                else if (comp.ID == 1296980128) // Dough
                {
                    FindNearestItem(bot, new ItemInfo(-1900989960), pos, out var oilPos, false, KitchenRoomTypes);
                    EntityManager.AddComponentData(bot, new CMoveTo(oilPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(oilPos, GrabType.Undefined, new ItemInfo(comp), new ItemInfo(-1900989960)));
                }
                else if (comp.ID == 1242961771 || comp.ID == -201067776 || comp.ID == 313161428 || comp.ID == -755280170) // Raw tomato, onion, mushroom, cheese
                {
                    if (GetNearestAppliance(pos, Counters, out var dropPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), dropPos);
                    }
                    else
                    {
                        Debug.LogError("No free counter!");
                    }
                }
                else if (comp.ID == -48499881) // crust
                {
                    if (TryGetItemMemory(bot, new ItemInfo(-1317168923), out var soucePos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(soucePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(soucePos, GrabType.CombineDrop));
                        RemoveItemMemory(bot, new ItemInfo(-1317168923));
                    }
                    else
                    {
                        if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(-48499881), counterPos);
                        }
                        else
                        {
                            Debug.LogError("No free counter!");
                        }
                    }
                }
                else if (comp.ID == 263830100) // grated cheese
                {
                    if (FindNearestItem(bot, new ItemInfo(445221203, -48499881, -1317168923), pos, out var soucedCrustPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(soucedCrustPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(soucedCrustPos, GrabType.CombineDrop));
                    }
                    else
                    {
                        Debug.LogError("No target for cheese!");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -1252408744 || comp.ID == -2093899333) // onion - chopped, mushroom - chopped
                {
                    if (FindNearestItem(bot, new ItemInfo(445221203, -48499881, -1317168923, 263830100), pos, out var soucedCrustPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(soucedCrustPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(soucedCrustPos, GrabType.CombineDrop));
                    }
                    else
                    {
                        Debug.LogError("No target for ingredient!");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1378842682) // flour
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _, null, null);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else
                {
                    EmptyHands(bot);
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1196800934, baked), pos, out var pizzaPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pizzaPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(pizzaPos, true));
                }
                else if (FindNearestItem(bot, new ItemInfo(938942828, baked), pos, out pizzaPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pizzaPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(pizzaPos, GrabType.Pickup, default, new ItemInfo(938942828, baked)));
                }
                else if (FindNearestItem(bot, new ItemInfo(-1196800934, baked), pos, out pizzaPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, pizzaPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(445221203, unbaked), pos, out pizzaPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pizzaPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(pizzaPos, GrabType.Pickup, default, new ItemInfo(445221203, unbaked)));
                }
                else if (pizzaType == -2093899333 && FindNearestItem(bot, new ItemInfo(445221203, 263830100, -48499881, -1317168923), pos, out _, false, KitchenRoomTypes))
                {
                    if (TryGetItemMemory(bot, new ItemInfo(313161428), out var rawShroomPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(rawShroomPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(rawShroomPos, true));
                        RemoveItemMemory(bot, new ItemInfo(313161428));
                        AddItemMemory(bot, new ItemInfo(-2093899333), rawShroomPos);
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-2093899333), pos, out var choppedShroomPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(choppedShroomPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(choppedShroomPos, GrabType.Pickup, default, new ItemInfo(-2093899333)));
                        RemoveItemMemory(bot, new ItemInfo(-2093899333));
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -1097889139 }, out var mushroomPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(mushroomPos, GrabType.Dispense));
                    }
                }
                else if (pizzaType == -1252408744 && FindNearestItem(bot, new ItemInfo(445221203, 263830100, -48499881, -1317168923), pos, out _, false, KitchenRoomTypes))
                {
                    if (TryGetItemMemory(bot, new ItemInfo(-201067776), out var rawOnionPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(rawOnionPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(rawOnionPos, true));
                        RemoveItemMemory(bot, new ItemInfo(-201067776));
                        AddItemMemory(bot, new ItemInfo(-1252408744), rawOnionPos);
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-1252408744), pos, out var choppedOnionPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(choppedOnionPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(choppedOnionPos, GrabType.Pickup, default, new ItemInfo(-1252408744)));
                        RemoveItemMemory(bot, new ItemInfo(-1252408744));
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense));
                    }
                }
                else
                {
                    if (!FindNearestItem(bot, new ItemInfo(445221203, -48499881, -1317168923), pos, out _, false, KitchenRoomTypes)) // crust with tomato souce
                    {
                        if (!FindNearestItem(bot, new ItemInfo(-1317168923), pos, out _, false, KitchenRoomTypes)) // tomato souce
                        {
                            if (!FindNearestItem(bot, new ItemInfo(-853757044), pos, out var slicedTomatoPos, false, KitchenRoomTypes)) // sliced tomato
                            {
                                if (!FindNearestItem(bot, new ItemInfo(1242961771), pos, out var rawTomatoPos, false, KitchenRoomTypes)) // raw tomato
                                {
                                    GetNearestAppliance(pos, new HashSet<int> { -712909563 }, out var tomatoProviderPos, out _);
                                    EntityManager.AddComponentData(bot, new CMoveTo(tomatoProviderPos));
                                    EntityManager.AddComponentData(bot, new CGrabAction(tomatoProviderPos, GrabType.Dispense));
                                }
                                else
                                {
                                    EntityManager.AddComponentData(bot, new CMoveTo(rawTomatoPos));
                                    EntityManager.AddComponentData(bot, new CInteractAction(rawTomatoPos, true));
                                    RemoveItemMemory(bot, new ItemInfo(1242961771));
                                    AddItemMemory(bot, new ItemInfo(-853757044), rawTomatoPos);
                                }
                            }
                            else
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(slicedTomatoPos));
                                EntityManager.AddComponentData(bot, new CInteractAction(slicedTomatoPos, true));
                                RemoveItemMemory(bot, new ItemInfo(-853757044));
                                AddItemMemory(bot, new ItemInfo(-1317168923), slicedTomatoPos);
                            }
                            return;
                        }
                        if (!FindNearestItem(bot, new ItemInfo(-48499881), pos, out var crustPos, false, KitchenRoomTypes))
                        {
                            GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var flourPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(flourPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(flourPos, GrabType.Dispense));
                        }
                        else
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(crustPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(crustPos, GrabType.Pickup, default, new ItemInfo(-48499881)));
                            RemoveItemMemory(bot, new ItemInfo(-48499881));
                        }
                    }
                    else
                    {
                        if (!FindNearestItem(bot, new ItemInfo(263830100), pos, out var gratedPos, false, KitchenRoomTypes))
                        {
                            if (FindNearestItem(bot, new ItemInfo(-755280170), pos, out var rawCheesePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(rawCheesePos));
                                EntityManager.AddComponentData(bot, new CInteractAction(rawCheesePos, true));
                                RemoveItemMemory(bot, new ItemInfo(-755280170));
                                AddItemMemory(bot, new ItemInfo(263830100), rawCheesePos);
                            }
                            else
                            {
                                GetNearestAppliance(pos, new HashSet<int> { -117339838 }, out var providerPos, out _);
                                EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                            }
                        }
                        else
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(gratedPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(gratedPos, GrabType.Pickup, default, new ItemInfo(263830100)));
                            RemoveItemMemory(bot, new ItemInfo(263830100));
                        }
                    }
                }
            }
        }

        private void DumplingsFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1296980128 || comp.ID == 1306214641 || comp.ID == -1944015682) // dough / meat / carrot
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, FillStateCheck.Ignore, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == -830135945) // carrot chopped
                {
                    if (FindNearestItem(bot, new ItemInfo(1296980128), pos, out var dumpPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dumpPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dumpPos, GrabType.CombineDrop));
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(1867434040, 1296980128, -830135945), dumpPos);
                    }
                    else
                    {
                        EmptyHands(bot);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1005005768) // meat chopped
                {
                    if (FindNearestItem(bot, new ItemInfo(1867434040, 1296980128, -830135945), pos, out var dumpPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dumpPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dumpPos, GrabType.CombineDrop));
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(1867434040, 1296980128, 1005005768, -830135945), dumpPos);
                    }
                    else
                    {
                        EmptyHands(bot);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1640282430)
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No plates found in kitchen!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1297982178) // seaweed
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1847818036));
                            AddItemMemory(bot, new ItemInfo(-1847818036), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == -1847818036) // seaweed cooked
                {
                    if (FindNearestItem(bot, new ItemInfo(-1938035042, 1640282430, 793377380), pos, out var platedPos, true, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platedPos, GrabType.CombineDrop));
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(-1938035042, 1640282430, 793377380, -1847818036), platedPos);
                    }
                }
                else if (comp.ID == 718093067) // kneaded dumplings
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 1640282430));
                            AddItemMemory(bot, new ItemInfo(1640282430), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp == new ItemInfo(-1938035042, 1640282430, 793377380))
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, FillStateCheck.Ignore, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(-1938035042, 1640282430, 793377380), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-1938035042, 1640282430, 793377380), pos, out _, true, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, new ItemInfo(-1847818036), pos, out var weedPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, weedPos, GrabType.Pickup))
                        {
                            RemoveItemMemory(bot, new ItemInfo(-1847818036));
                        }
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-1847818036), pos, out weedPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(weedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(weedPos, GrabType.Pickup, default, new ItemInfo(-1847818036)));
                        RemoveItemMemory(bot, new ItemInfo(-1847818036));
                    }
                    else
                    {
                        if (GetNearestAppliance(pos, new HashSet<int> { 595306349 }, out var providerPos, out _))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(1867434040, 1296980128, 1005005768, -830135945), pos, out var unprepPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(unprepPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(unprepPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(718093067), unprepPos);
                }
                else if (FindNearestItem(bot, new ItemInfo(1640282430), pos, out var cookedPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, cookedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(718093067), new ItemInfo(-1938035042, 1640282430, 793377380, -1847818036) }, pos, out var kneadedPos, out var item, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(kneadedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(kneadedPos, GrabType.Pickup, default, item));
                    ClearItemMemory(bot);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(1296980128), new ItemInfo(1867434040, 1296980128, 1005005768) }, pos, out _, out _, false, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, new ItemInfo(-830135945), pos, out var carrotPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(carrotPos, GrabType.Pickup, default, new ItemInfo(-830135945)));
                        RemoveItemMemory(bot, new ItemInfo(-830135945));
                    }
                    else if (TryGetItemMemory(bot, new ItemInfo(-1944015682), out carrotPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(carrotPos, true));
                        RemoveItemMemory(bot, new ItemInfo(-1944015682));
                        AddItemMemory(bot, new ItemInfo(-830135945), carrotPos);
                    }
                    else
                    {
                        // carrots
                        GetNearestAppliance(pos, new HashSet<int> { -452101383 }, out var providerPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(1867434040, 1296980128, -830135945), pos, out _, false, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, new ItemInfo(1005005768), pos, out var meatPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(meatPos, GrabType.Pickup, default, new ItemInfo(1005005768)));
                        RemoveItemMemory(bot, new ItemInfo(1005005768));
                    }
                    else if (TryGetItemMemory(bot, new ItemInfo(1306214641), out meatPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                        EntityManager.AddComponentData(bot, new CInteractAction(meatPos, true));
                        RemoveItemMemory(bot, new ItemInfo(1306214641));
                        AddItemMemory(bot, new ItemInfo(1005005768), meatPos);
                    }
                    else
                    {
                        // meat
                        GetNearestAppliance(pos, new HashSet<int> { -484165118 }, out var providerPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                }
                else
                {
                    // flour
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
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
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
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
                    EntityManager.AddComponentData(bot, new CGrabAction(appliancePos, GrabType.Fill));
                }
                else if (comp.ID == 364023067)
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { -1609758240 }, out var coffeePos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(coffeePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(coffeePos, GrabType.Drop, new ItemInfo(comp)));
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
                if (GetNearestAppliance(pos, new HashSet<int> { -1609758240 }, out var coffeePos, out _, true))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(coffeePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(coffeePos, GrabType.Dispense));
                }
                else if (GetNearestAppliance(pos, new HashSet<int> { -557736569 }, out _, out _, null, FillStateCheck.IsEmpty, KitchenRoomTypes))
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { 120342736 }, out var milkPos, out _))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(milkPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(milkPos, GrabType.Dispense));
                    }
                    else
                    {
                        Debug.LogError("No Milk found in Kitchen!");
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-1293050650), pos, out var mugPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mugPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(mugPos, GrabType.Pickup, default, new ItemInfo(-1293050650)));
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
                    EntityManager.AddComponentData(bot, new CGrabAction(bagPos, GrabType.Dispense));
                }
                else
                {
                    if (GetBestDropOff(pos, out var hatchPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hatchPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hatchPos, GrabType.Drop, new ItemInfo(comp)));
                        RemoveFromOrder(bot, orders[0]);
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
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
                EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Dispense));
            }
        }

        private void BurgerFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1755299639 || comp.ID == 1306214641 || comp.ID == -755280170 || comp.ID == -201067776 || comp.ID == 1242961771)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                }
                else if (comp.ID == 378690159)
                {
                    if (TryGetItemMemory(bot, 1005005768, out var mincePos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(mincePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(mincePos, GrabType.CombineDrop));
                        RemoveItemMemory(bot, new ItemInfo(1005005768));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1150879908)
                {
                    if (GetBestHob(pos, out var hobPos, out _))
                    {
                        HobInteraction(bot, hobPos, GrabType.Drop);
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -1756808590)
                {
                    if (FindNearestItem(bot, new ItemInfo(-884392267, 793377380, 687585830), pos, out var platedPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        HobInteraction(bot, platedPos, GrabType.CombineDrop);
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(-884392267), platedPos);
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 263830100 || comp.ID == -1252408744 || comp.ID == -853757044)
                {
                    if (TryGetItemMemory(bot, -884392267, out var platedPos))
                    {
                        HobInteraction(bot, platedPos, GrabType.CombineDrop);
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 793377380)
                {
                    if (FindNearestItem(bot, 687585830, pos, out var pattyPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, pattyPos, GrabType.CombineDrop))
                        {
                            ClearItemMemory(bot);
                            AddItemMemory(bot, new ItemInfo(-884392267, 793377380, 687585830), pattyPos);
                        }
                    }
                    else if (FindNearestItem(bot, 1150879908, pos, out pattyPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(pattyPos));
                        EntityManager.AddComponentData(bot, new CBotWaiting(pattyPos, 687585830));
                    }
                    else
                    {
                        EmptyHands(bot);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, orders[0], pos, out var platedPos, true, KitchenRoomTypes))
                {
                    HobInteraction(bot, platedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 1150879908, pos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense));
                    }
                    else if (FindNearestItem(bot, 793377380, pos, out var platePos2, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos2));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos2, GrabType.Pickup, default, new ItemInfo(793377380)));
                    }
                    else
                    {
                        Debug.LogError("No clean plates");
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (FindNearestItem(bot, new ItemInfo(-884392267, 793377380, 687585830), pos, out _, false, KitchenRoomTypes))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 759552160 }, out var bunPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(bunPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bunPos, GrabType.Dispense));
                }
                else if (TryGetItemMemory(bot, 1755299639, out var eggPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(eggPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(eggPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1755299639));
                    AddItemMemory(bot, new ItemInfo(378690159), eggPos);
                }
                else if (TryGetItemMemory(bot, 1306214641, out var meatPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(meatPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1306214641));
                    AddItemMemory(bot, new ItemInfo(1005005768), meatPos);
                }
                else if (TryGetItemMemory(bot, -755280170, out var cheesePos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(cheesePos));
                    EntityManager.AddComponentData(bot, new CInteractAction(cheesePos, true));
                    RemoveItemMemory(bot, new ItemInfo(-755280170));
                    AddItemMemory(bot, new ItemInfo(263830100), cheesePos);
                }
                else if (TryGetItemMemory(bot, -201067776, out var onionPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(onionPos, true));
                    RemoveItemMemory(bot, new ItemInfo(-201067776));
                    AddItemMemory(bot, new ItemInfo(-1252408744), onionPos);
                }
                else if (TryGetItemMemory(bot, 1242961771, out var tomatoPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(tomatoPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1242961771));
                    AddItemMemory(bot, new ItemInfo(-853757044), tomatoPos);
                }
                else if (FindNearestItem(bot, -884392267, pos, out var platePos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    GetComponentOfHeld<CItem>(TileManager.GetPrimaryOccupant(platePos), out var plated);
                    if (orders[0].Items.Contains(263830100) && !plated.Items.Contains(263830100))
                    {
                        if (FindNearestItem(bot, 263830100, pos, out var gratedPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(gratedPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(gratedPos, GrabType.Pickup, default, new ItemInfo(263830100)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -117339838 }, out var rawcheesePos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(rawcheesePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawcheesePos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(-1252408744) && !plated.Items.Contains(-1252408744))
                    {
                        if (FindNearestItem(bot, -1252408744, pos, out var rawonionPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(rawonionPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawonionPos, GrabType.Pickup, default, new ItemInfo(-1252408744)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var rawonionPos2, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(rawonionPos2));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawonionPos2, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(-853757044) && !plated.Items.Contains(-853757044))
                    {
                        if (FindNearestItem(bot, -853757044, pos, out var rawtomatoPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(rawtomatoPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawtomatoPos, GrabType.Pickup, default, new ItemInfo(-853757044)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -712909563 }, out var rawtomatoPos2, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(rawtomatoPos2));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawtomatoPos2, GrabType.Dispense));
                        }
                    }
                }
                else
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { 385684499 }, out var providerPos, out _))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                    else if (FindNearestItem(bot, 1150879908, pos, out var pattyPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(pattyPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(pattyPos, GrabType.Pickup, default, new ItemInfo(1150879908)));
                    }
                    else
                    {
                        if (FindNearestItem(bot, 1005005768, pos, out _, false, KitchenRoomTypes))
                        {
                            if (FindNearestItem(bot, 378690159, pos, out var crackedPos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(crackedPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(crackedPos, GrabType.Pickup, default, new ItemInfo(378690159)));
                            }
                            else
                            {
                                GetNearestAppliance(pos, new HashSet<int> { 961148621 }, out var raweggPos, out _);
                                EntityManager.AddComponentData(bot, new CMoveTo(raweggPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(raweggPos, GrabType.Dispense));
                            }
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -484165118 }, out var rawmeatPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(rawmeatPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(rawmeatPos, GrabType.Dispense));
                        }
                    }
                }
            }
        }

        private void TurkeyFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == -1568853395 || comp.ID == -1867438686 || comp.ID == 294281422)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No free space");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1252408744)
                {
                    if (FindNearestItem(bot, 235356204, pos, out var crumbPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(crumbPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(crumbPos, GrabType.CombineDrop));
                        RemoveItemMemory(bot, new ItemInfo(235356204));
                        AddItemMemory(bot, new ItemInfo(1427021177, -1252408744, 235356204), crumbPos);
                    }
                }
                else if (comp.ID == 1474921248 || comp.ID == -201067776 || comp.ID == 428559718 || comp.ID == -69847810)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free space");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -914826716)
                {
                    if (GetNumItemsInMemory(bot, new ItemInfo(1792757441, -914826716, 793377380)) >= orders.Count)
                    {
                        EmptyHands(bot);
                    }
                    else if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else if (FindNearestItem(bot, 793377380, pos, out platePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.CombineDrop));
                        AddItemMemory(bot, new ItemInfo(1792757441, 793377380, -914826716), platePos);
                    }
                    else
                    {
                        Debug.LogError("No clean plate");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == -1831502471)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1568853395));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp == new ItemInfo(1370203151, -486398094, -201067776) || comp.ID == 1859809622)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1370203151)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -69847810));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1696315132)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 294281422));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -777417645)
                {
                    if (SchemaFactory.Dishes.Contains("Turkey with Gravy"))
                    {
                        if (FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                        {
                            if (HobInteraction(bot, brothPos, GrabType.CombineDrop))
                            {
                                RemoveItemMemory(bot, new ItemInfo(-69847810));
                                EntityManager.AddComponentData(bot, new CBotWaiting(brothPos, 294281422));
                            }
                            return;
                        }
                    }
                    EmptyHands(bot);
                }
                else if (comp.ID == -306959510)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 428559718));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1296980128)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1867438686));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1427021177)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -352397598));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 163163953)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2133205155 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == -1788071646 || comp.ID == -352397598)
                {
                    if (FindNearestItem(bot, 1792757441, pos, out var platedPos, true, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platedPos, GrabType.CombineDrop));
                        UpdateItemMemory(bot, 1792757441, platedPos, comp.ID);
                    }
                    else
                    {
                        Debug.LogError("No turkey found");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1792757441)
                {
                    if (orders[0].Items.Contains(1168127977) && !comp.Items.Contains(1168127977) && FindNearestItem(bot, 294281422, pos, out var gravyPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(gravyPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(gravyPos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(-777417645), pos, out var bonePos, false, KitchenRoomTypes))
                {
                    if (SchemaFactory.Dishes.Contains("Turkey with Gravy"))
                    {
                        if (!FindNearestItem(bot, -69847810, pos, out var brothPos, false, KitchenRoomTypes))
                        {
                            if (FindNearestItem(bot, 1859809622, pos, out var depletedPos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(depletedPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(depletedPos, GrabType.Pickup, default, new ItemInfo(1859809622)));
                            }
                            else
                            {
                                GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out var potPos, out _, null, FillStateCheck.IsNotEmpty, KitchenRoomTypes);
                                EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Dispense));
                            }
                            return;
                        }
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(bonePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bonePos, GrabType.Pickup, default, new ItemInfo(-777417645)));
                }
                else if (FindNearestItem(bot, 1696315132, pos, out var gravyPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(gravyPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(gravyPos, GrabType.Pickup, default, new ItemInfo(1696315132)));
                }
                else if (FindNearestItem(bot, orders[0], pos, out var finishedPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(finishedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(finishedPos, GrabType.Pickup, default, orders[0]));
                }
                else if (FindNearestItem(bot, 428559718, pos, out var toastPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(toastPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(toastPos, true));
                    AddItemMemory(bot, new ItemInfo(235356204), toastPos);
                }
                else if (FindNearestItem(bot, new HashSet<int> { -1867438686, -352397598, 428559718, 294281422 }, pos, out var cookedPos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, cookedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(1427021177, -1252408744, 235356204), pos, out var stuffingPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(stuffingPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(stuffingPos, GrabType.Pickup, default, new ItemInfo(1427021177, -1252408744, 235356204)));
                }
                else if (TryGetItemMemory(bot, 163163953, out var choppedPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(choppedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(choppedPos, GrabType.Pickup, default, new ItemInfo(163163953)));
                    RemoveItemMemory(bot, new ItemInfo(163163953));
                }
                else if (TryGetItemMemory(bot, 1474921248, out var berryPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(berryPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(berryPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1474921248));
                    AddItemMemory(bot, new ItemInfo(163163953), berryPos);
                }
                else if (TryGetItemMemory(bot, -201067776, out var onionPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(onionPos, true));
                    RemoveItemMemory(bot, new ItemInfo(-201067776));
                    AddItemMemory(bot, new ItemInfo(-1252408744), onionPos);
                }
                else if (FindNearestItem(bot, 1792757441, pos, out var platedPos, true, KitchenRoomTypes))
                {
                    GetComponentOfHeld<CItem>(TileManager.GetPrimaryOccupant(platedPos), out var plated);
                    if (orders[0].Items.Contains(-352397598) && !plated.Items.Contains(-352397598))
                    {
                        // Stuffing
                        if (FindNearestItem(bot, 235356204, pos, out _, false, KitchenRoomTypes))
                        {
                            if (FindNearestItem(bot, -1252408744, pos, out var slicePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(slicePos, GrabType.Pickup, default, new ItemInfo(-1252408744)));
                                RemoveItemMemory(bot, new ItemInfo(-1252408744));
                            }
                            else
                            {
                                GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var providerPos, out _);
                                EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                            }
                        }
                        else
                        {
                            if (FindNearestItem(bot, -1867438686, pos, out var breadPos, false, null, CookingAppliances, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(breadPos));
                                EntityManager.AddComponentData(bot, new CInteractAction(breadPos, true));
                            }
                            else
                            {
                                GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var providerPos, out _);
                                EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                            }
                        }
                    }
                    else if (orders[0].Items.Contains(1168127977) && !plated.Items.Contains(1168127977))
                    {
                        // Gravy
                        if (FindNearestItem(bot, 294281422, pos, out var gravypotPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(platedPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(platedPos, GrabType.Pickup, default, new ItemInfo(294281422)));
                        }
                        else if (FindNearestItem(bot, -1568853395, pos, out var turkeyPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(turkeyPos));
                            EntityManager.AddComponentData(bot, new CInteractAction(turkeyPos, true));
                        }
                    }
                    else if (orders[0].Items.Contains(-1788071646) && !plated.Items.Contains(-1788071646))
                    {
                        if (FindNearestItem(bot, 163163953, pos, out berryPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(berryPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(berryPos, GrabType.Pickup, default, new ItemInfo(163163953)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { 735786885 }, out var providerPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                    else
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platedPos, GrabType.Pickup, default, new ItemInfo(plated)));
                        RemoveItemMemory(bot, new ItemInfo(plated));
                    }
                }
                else if (FindNearestItem(bot, -1568853395, pos, out var turkeyPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, turkeyPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, -1568853395, pos, out turkeyPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(turkeyPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(turkeyPos, true));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1506824829 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
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
                                EntityManager.AddComponentData(bot, new CGrabAction(hatchPos, GrabType.Drop, new ItemInfo(comp)));
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
                            if (GetBestStorage(pos, out var counterPos))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            }
                            break;
                        }
                    case -201067776: // Onion Raw
                    case 609827370: // Nuts Raw
                        {
                            if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                                AddItemMemory(bot, new ItemInfo(comp), counterPos);
                            }
                            break;
                        }
                    case -2100850612: // Chopped Nuts
                        {
                            if (FindNearestItem(bot, new ItemInfo(-1252408744), pos, out var appliancePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(appliancePos, GrabType.CombineDrop));
                                AddItemMemory(bot, new ItemInfo(-1515496760, -2100850612, -1252408744), appliancePos);
                            }
                            else
                            {
                                EmptyHands(bot);
                            }
                            RemoveItemMemory(bot, new ItemInfo(-1252408744));
                            break;
                        }
                    case -1252408744: // Chopped Onion
                        {
                            if (FindNearestItem(bot, new ItemInfo(-2100850612), pos, out var appliancePos, false, KitchenRoomTypes))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                                EntityManager.AddComponentData(bot, new CGrabAction(appliancePos, GrabType.CombineDrop));
                                AddItemMemory(bot, new ItemInfo(-1515496760, -2100850612, -1252408744), appliancePos);
                            }
                            else
                            {
                                EmptyHands(bot);
                            }
                            RemoveItemMemory(bot, new ItemInfo(-2100850612));
                            break;
                        }
                    case -1515496760: // Nut Mixture
                        {
                            if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                            {
                                if (HobInteraction(bot, hobPos, GrabType.Drop))
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
                                EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
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
                if (FindNearestItem(bot, new ItemInfo(-1945246136), pos, out var hobPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, hobPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(-1294491269), pos, out var slicePos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(slicePos, GrabType.Pickup, default, new ItemInfo(-1294491269)));
                }
                else if (FindNearestItem(bot, new ItemInfo(-1945246136), pos, out var roastPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(roastPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(roastPos, true));
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(-1294491269), new ItemInfo(-1515496760, -2100850612, -1252408744) }, pos, out var targetPos, out var item, false, KitchenRoomTypes))
                {
                    // Nut Mixture
                    // Nut Mixture - Portion
                    EntityManager.AddComponentData(bot, new CMoveTo(targetPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(targetPos, GrabType.Pickup, default, item));
                    ClearItemMemory(bot);
                }
                else if (
                    FindNearestItem(bot, new ItemInfo(-2100850612), pos, out _, false, KitchenRoomTypes) &&
                    FindNearestItem(bot, new ItemInfo(-1252408744), pos, out var ingredientPos, false, KitchenRoomTypes)
                    )
                {
                    // Nuts - Chopped
                    // Onion - Chopped
                    EntityManager.AddComponentData(bot, new CMoveTo(ingredientPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(ingredientPos, GrabType.Pickup, default, new ItemInfo(-1252408744)));
                    RemoveItemMemory(bot, new ItemInfo(-1252408744));
                }
                else if (FindNearestItem(bot, new ItemInfo(-201067776), pos, out var onionPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(onionPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(onionPos, true));
                    RemoveItemMemory(bot, new ItemInfo(-201067776));
                    AddItemMemory(bot, new ItemInfo(-1252408744), onionPos);
                }
                else if (FindNearestItem(bot, new ItemInfo(609827370), pos, out var nutsPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(nutsPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(nutsPos, true));
                    RemoveItemMemory(bot, new ItemInfo(609827370));
                    AddItemMemory(bot, new ItemInfo(-2100850612), nutsPos);
                }
                else if (!TryGetItemMemory(bot, new ItemInfo(-2100850612), out _)) // Nuts chopped
                {
                    GetNearestAppliance(pos, new HashSet<int> { 1834063794 }, out var nutsSourcePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(nutsSourcePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(nutsSourcePos, GrabType.Dispense));
                    AddItemMemory(bot, new ItemInfo(609827370), nutsSourcePos);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2042103798 }, out var onionSourcePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(onionSourcePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(onionSourcePos, GrabType.Dispense));
                    AddItemMemory(bot, new ItemInfo(-201067776), onionSourcePos);
                }
            }
        }

        private void PieFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1296980128)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(1296980128), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free counter");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1030798878 || comp.ID == 280553412 || comp.ID == -1612932608)
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else if (FindNearestItem(bot, 793377380, pos, out platePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.CombineDrop));
                        AddItemMemory(bot, new ItemInfo(861630222), platePos);
                    }
                }
                else if (orders[0].Items.Contains(1030798878))
                {
                    if (comp.Items.Contains(1306214641))
                    {
                        if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                        {
                            if (HobInteraction(bot, hobPos, GrabType.Drop))
                            {
                                EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 1030798878));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("No free hob");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -484165118 }, out var meatPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(meatPos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                }
                else if (orders[0].Items.Contains(280553412))
                {
                    if (comp.Items.Contains(313161428))
                    {
                        if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                        {
                            if (HobInteraction(bot, hobPos, GrabType.Drop))
                            {
                                EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 280553412));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("No free hob");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -1097889139 }, out var mushroomPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(mushroomPos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                }
                else
                {
                    if (comp.Items.Contains(-1944015682))
                    {
                        if (comp.Items.Contains(-1774883004))
                        {
                            if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                            {
                                if (HobInteraction(bot, hobPos, GrabType.Drop))
                                {
                                    EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1612932608));
                                }
                            }
                            else
                            {
                                Debug.LogWarning("No free hob");
                                EmptyHands(bot);
                                RemoveFromOrder(bot, orders[0]);
                            }
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -1573812073 }, out var broccoliPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(broccoliPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(broccoliPos, GrabType.Dispense, new ItemInfo(comp)));
                        }
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -452101383 }, out var carrotPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(carrotPos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                }
            }
            else
            {
                if (TryGetItemMemory(bot, 861630222, out var piePos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(piePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(piePos, GrabType.Pickup, default, new ItemInfo(861630222)));
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(1030798878), new ItemInfo(280553412), new ItemInfo(-1612932608) }, pos, out piePos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, piePos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, 164600160, pos, out var crustPos, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(crustPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(crustPos, GrabType.Pickup, default, new ItemInfo(164600160)));
                    ClearItemMemory(bot);
                }
                else if (FindNearestItem(bot, 1296980128, pos, out var doughPos, false, Counters, null, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(doughPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(doughPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(164600160), doughPos);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
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
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                }
                else if (comp.ID == 1702717896)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            if (!HasBuffer<CBotWatching>(bot))
                                EntityManager.AddBuffer<CBotWatching>(bot);

                            var buffer = EntityManager.GetBuffer<CBotWatching>(bot);
                            buffer.Add(new CBotWatching(hobPos, -248200024));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -248200024)
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1132411297 }, out var bunPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(bunPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bunPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == 1134979829)
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else if (FindNearestItem(bot, new ItemInfo(793377380), pos, out platePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.CombineDrop));
                        if (!MoveToSystem.Hatches.Contains(platePos))
                            AddItemMemory(bot, new ItemInfo(1702578261, -248200024, 756326364, 793377380), platePos);
                    }
                }

            }
            else
            {
                if (WatchedCheck(bot))
                    return;

                if (FindNearestItem(bot, new ItemInfo(1702578261, -248200024, 756326364, 793377380), pos, out var dogPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(dogPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(dogPos, GrabType.Pickup, default, new ItemInfo(1702578261, -248200024, 756326364, 793377380)));
                    return;
                }

                var numDogs = 0;
                if (RequireBuffer<CBotWatching>(bot, out var buffer))
                    numDogs = buffer.Length;

                if (numDogs < orders.Count && numDogs < 2 && GetNearestAppliance(pos, CookingAppliances, out var _, out _, true, null, KitchenRoomTypes))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 1799769627 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void BreakfastFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1296980128)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, -1867438686));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1921097327)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(hobPos, 1286433124));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -306959510 || comp.ID == 378690159)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        HobInteraction(bot, hobPos, GrabType.Drop);
                    }
                    else
                    {
                        Debug.LogError("No free hob");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1867438686 || comp.ID == 1286433124)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No free space");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1755299639 || comp.ID == 1242961771 || comp.ID == 313161428)
                {
                    if (GetBestStorage(pos, out var counterPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free space");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 793377380)
                {
                    if (FindNearestItem(bot, 428559718, pos, out var toastPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        HobInteraction(bot, toastPos, GrabType.CombineDrop);
                    }
                    else if (FindNearestItem(bot, -306959510, pos, out toastPos, false, CookingAppliances, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(toastPos));
                        EntityManager.AddComponentData(bot, new CBotWaiting(toastPos, 428559718));
                    }
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, new HashSet<int> { 1807525572 }, out var beanPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(beanPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(beanPos, false));
                }
                else if (comp.ID == 1324261001 || comp.ID == -853757044 || comp.ID == -2093899333)
                {
                    if (TryGetItemMemory(bot, 1754241573, out var unfinishedPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(unfinishedPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(unfinishedPos, GrabType.CombineDrop));
                        GetComponentOfHeld<CItem>(TileManager.GetPrimaryOccupant(unfinishedPos), out var item);
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(item, comp.ID), unfinishedPos);
                    }
                    else
                    {
                        Debug.LogError("Could not remember where i put the plate");
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 1754241573)
                {
                    if (orders[0].Items.Contains(-2138118944) && !comp.Items.Contains(-2138118944))
                    {
                        if (FindNearestItem(bot, 1286433124, pos, out var beanPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(beanPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(beanPos, GrabType.Dispense, new ItemInfo(comp)));
                        }
                        else if (GetBestStorage(pos, out var counterPos))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(comp), counterPos);
                        }
                        else
                        {
                            Debug.LogError("No free space");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                    else
                    {
                        if (GetBestStorage(pos, out var counterPos))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(comp), counterPos);
                        }
                        else
                        {
                            Debug.LogError("No free space");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                }
            }
            else
            {
                if (TryGetItemMemory(bot, 1755299639, out var eggPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(eggPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(eggPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1755299639));
                }
                else if (TryGetItemMemory(bot, 1242961771, out var tomatoPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(tomatoPos, true));
                    RemoveItemMemory(bot, new ItemInfo(1242961771));
                    AddItemMemory(bot, new ItemInfo(-853757044), tomatoPos);
                }
                else if (TryGetItemMemory(bot, 313161428, out var mushroomPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(mushroomPos, true));
                    RemoveItemMemory(bot, new ItemInfo(313161428));
                    AddItemMemory(bot, new ItemInfo(-2093899333), mushroomPos);
                }
                else if (FindNearestItem(bot, 1286433124, pos, out var beanPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, beanPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(1754241573, 793377380, 428559718), pos, out var platedPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, platedPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, orders[0], pos, out platedPos, true, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(platedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(platedPos, GrabType.Pickup, default, orders[0]));
                }
                else if (FindNearestItem(bot, 1754241573, pos, out var unfinishedPos, true, KitchenRoomTypes))
                {
                    GetComponentOfHeld<CItem>(TileManager.GetPrimaryOccupant(unfinishedPos), out comp);
                    if (orders[0].Items.Contains(-2138118944) && !comp.Items.Contains(-2138118944))
                    {
                        if (FindNearestItem(bot, 1286433124, pos, out _, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(unfinishedPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(unfinishedPos, GrabType.Pickup, default, new ItemInfo(comp)));
                            ClearItemMemory(bot);
                        }
                        else if (FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Pickup, default, new ItemInfo(-486398094)));
                        }
                        else if (GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out potPos, out _, null, FillStateCheck.IsNotEmpty))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(potPos, GrabType.Dispense));
                        }
                        else
                        {
                            Debug.LogError("No pot found");
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                    else if (orders[0].Items.Contains(1324261001) && !comp.Items.Contains(1324261001))
                    {
                        if (FindNearestItem(bot, 1324261001, pos, out var cookedPos, false, CookingAppliances, null, KitchenRoomTypes))
                        {
                            HobInteraction(bot, cookedPos, GrabType.Pickup);
                        }
                        else if (FindNearestItem(bot, 378690159, pos, out var crackedPos, false, CookingAppliances, null, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CBotWaiting(crackedPos, 1324261001));
                        }
                        else if (FindNearestItem(bot, 378690159, pos, out crackedPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(crackedPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(crackedPos, GrabType.Pickup, default, new ItemInfo(378690159)));
                            RemoveItemMemory(bot, new ItemInfo(378690159));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { 961148621 }, out eggPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(eggPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(eggPos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(-853757044) && !comp.Items.Contains(-853757044))
                    {
                        if (FindNearestItem(bot, -853757044, pos, out tomatoPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(tomatoPos, GrabType.Pickup, default, new ItemInfo(-853757044)));
                            RemoveItemMemory(bot, new ItemInfo(-853757044));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -712909563 }, out tomatoPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(tomatoPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(tomatoPos, GrabType.Dispense));
                        }
                    }
                    else
                    {
                        if (FindNearestItem(bot, -2093899333, pos, out mushroomPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(mushroomPos, GrabType.Pickup, default, new ItemInfo(-2093899333)));
                            RemoveItemMemory(bot, new ItemInfo(-2093899333));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -1097889139 }, out mushroomPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(mushroomPos, GrabType.Dispense));
                        }
                    }
                }
                else if (FindNearestItem(bot, -1867438686, pos, out var breadPos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, breadPos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, -306959510, pos, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense));
                    }
                    else if (FindNearestItem(bot, 793377380, pos, out platePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Pickup, default, new ItemInfo(793377380)));
                    }
                    else
                    {
                        Debug.LogError("No clean plate");
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (FindNearestItem(bot, -1867438686, pos, out breadPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(breadPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(breadPos, true));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var flourPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(flourPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(flourPos, GrabType.Dispense));
                }
            }
        }

        private void StirFryFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 793377380)
                {
                    if (!TryGetItemMemory(bot, 1475451665, out var wokPos))
                    {
                        if (!TryGetItemMemory(bot, 150639636, out wokPos))
                        {
                            Debug.Log("No currently cooking stir fry found");
                            ClearItemMemory(bot);
                            EmptyHands(bot);
                            return;
                        }
                    }
                    else
                    {
                        EntityManager.AddComponentData(bot, new CBotWaiting(wokPos, 150639636));
                        return;
                    }
                    HobInteraction(bot, wokPos, GrabType.Undefined);
                }
                else if (comp.ID == -486398094)
                {
                    GetNearestAppliance(pos, WaterProviders, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(providerPos, false));
                }
                else if (comp.Items.IsEquivalent(new FixedListInt64 { 1657174953, -486398094 }))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -2092567672 }, out var bambooPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(bambooPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(bambooPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == -1652763586 || comp.ID == -2135410839)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            AddItemMemory(bot, new ItemInfo(comp), hobPos);
                        }
                    }
                    else
                    {
                        Debug.LogError("No free cooking location!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == -1944015682 || comp.ID == -1774883004 || comp.ID == 313161428 || comp.ID == 1306214641 || comp.ID == 2019756794)
                {
                    if (GetNearestAppliance(pos, Counters, out var CounterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(CounterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(CounterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), CounterPos);
                    }
                    else
                    {
                        Debug.Log("No free counter");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp.ID == 1271508828)
                {
                    if (TryGetItemMemory(bot, -2135410839, out var wokPos))
                    {
                        if (!HobInteraction(bot, wokPos, GrabType.CombineDrop))
                            return;
                        ClearItemMemory(bot);
                        AddItemMemory(bot, new ItemInfo(1475451665), wokPos);
                        AddItemMemory(bot, new ItemInfo(150639636), wokPos);
                    }
                    else
                    {
                        Debug.Log("No wok in memory found");
                        ClearItemMemory(bot);
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == -830135945 || comp.ID == 748471091 || comp.ID == -2093899333 || comp.ID == 1005005768 || comp.ID == 2037858460)
                {
                    if (!TryGetItemMemory(bot, 1475451665, out var wokPos))
                    {
                        if (!TryGetItemMemory(bot, 150639636, out wokPos))
                        {
                            Debug.Log("No currently cooking stir fry found");
                            ClearItemMemory(bot);
                            EmptyHands(bot);
                            return;
                        }
                    }
                    else
                    {
                        EntityManager.AddComponentData(bot, new CBotWaiting(wokPos, 150639636));
                        return;
                    }
                    if (!HobInteraction(bot, wokPos, GrabType.CombineDrop))
                        return;
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(1475451665), wokPos);
                    AddItemMemory(bot, new ItemInfo(150639636), wokPos);
                }
            }
            else
            {
                if (orders[0].Items.Contains(880804869) && !FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(2019756794), new ItemInfo(-1652763586, -486398094, 1657174953, -1635701703) }, pos, out _, out _, false, null))
                {
                    GrabType grab = GrabType.Pickup;
                    var item = new ItemInfo(-486398094);
                    if (!FindNearestItem(bot, -486398094, pos, out var potPos, false, KitchenRoomTypes))
                    {
                        grab = GrabType.Dispense;
                        item = default;
                        if (!GetNearestAppliance(pos, new HashSet<int> { -957949759 }, out potPos, out _, null, FillStateCheck.IsNotEmpty))
                        {
                            Debug.Log("No pots found");
                            RemoveFromOrder(bot, orders[0]);
                            return;
                        }
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(potPos, grab, default, item));
                }
                else if (TryGetItemMemory(bot, 1306214641, out var meatPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(meatPos, true));
                    AddItemMemory(bot, new ItemInfo(1005005768), meatPos);
                }
                else if (TryGetItemMemory(bot, -1944015682, out var carrotPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(carrotPos, true));
                    AddItemMemory(bot, new ItemInfo(-830135945), carrotPos);
                }
                else if (TryGetItemMemory(bot, -1774883004, out var broccoliPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(broccoliPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(broccoliPos, true));
                    AddItemMemory(bot, new ItemInfo(748471091), broccoliPos);
                }
                else if (TryGetItemMemory(bot, 313161428, out var mushroomPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(mushroomPos, true));
                    AddItemMemory(bot, new ItemInfo(-2093899333), mushroomPos);
                }
                else if (TryGetItemMemory(bot, -2135410839, out var wokPos))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -1201769154 }, out var ricePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(ricePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(ricePos, GrabType.Dispense));
                }
                else if (TryGetItemMemory(bot, new HashSet<int> { 1475451665, 150639636 }, out wokPos, out _))
                {
                    GetComponentOfHeld<CItem>(TileManager.GetPrimaryOccupant(wokPos), out comp);
                    if (orders[0].Items.Contains(-1406021079) && !comp.Items.Contains(-1406021079) && !comp.Items.Contains(-830135945))
                    {
                        if (FindNearestItem(bot, -830135945, pos, out carrotPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(carrotPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(carrotPos, GrabType.Pickup, default, new ItemInfo(-830135945)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -452101383 }, out var providerPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(1453647256) && !comp.Items.Contains(1453647256) && !comp.Items.Contains(748471091))
                    {
                        if (FindNearestItem(bot, 748471091, pos, out broccoliPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(broccoliPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(broccoliPos, GrabType.Pickup, default, new ItemInfo(748471091)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -1573812073 }, out var providerPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(-336580972) && !comp.Items.Contains(-336580972) && !comp.Items.Contains(-2093899333))
                    {
                        if (FindNearestItem(bot, -2093899333, pos, out mushroomPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(mushroomPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(mushroomPos, GrabType.Pickup, default, new ItemInfo(-2093899333)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -1097889139 }, out var providerPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(-1018018897) && !comp.Items.Contains(-1018018897) && !comp.Items.Contains(1005005768))
                    {
                        if (FindNearestItem(bot, 1005005768, pos, out meatPos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(meatPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(meatPos, GrabType.Pickup, default, new ItemInfo(1005005768)));
                        }
                        else
                        {
                            GetNearestAppliance(pos, new HashSet<int> { -484165118 }, out var providerPos, out _);
                            EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                        }
                    }
                    else if (orders[0].Items.Contains(880804869) && !comp.Items.Contains(880804869) && !comp.Items.Contains(2037858460))
                    {
                        if (FindNearestItem(bot, 2019756794, pos, out var potPos, false, null))
                        {
                            if (CookingAppliances.Contains(GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(potPos)).ID))
                            {
                                Debug.Log("Picking up bamboo pot from hob");
                                HobInteraction(bot, potPos, GrabType.Pickup);
                                return;
                            }
                            EntityManager.AddComponentData(bot, new CMoveTo(potPos));
                            EntityManager.AddComponentData(bot, new CInteractAction(potPos, true));
                        }
                        else if (FindNearestItem(bot, new ItemInfo(-1652763586, -486398094, 1657174953, -1635701703), pos, out var bambooPos, false, KitchenRoomTypes))
                        {
                            if (!CookingAppliances.Contains(GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(bambooPos)).ID))
                            {
                                EntityManager.AddComponentData(bot, new CMoveTo(bambooPos));
                                EntityManager.AddComponentData(bot, new CGrabAction(bambooPos, GrabType.Pickup, default, new ItemInfo(-1652763586, -486398094, 1657174953, -1635701703)));
                            }
                            EntityManager.AddComponentData(bot, new CBotWaiting(bambooPos, 2019756794));
                        }
                        else
                        {
                            Debug.Log("No bamboo pot found");
                            ClearItemMemory(bot);
                            return;
                        }
                    }
                    else
                    {
                        if (GetNearestAppliance(pos, Plates, out var platePos, out _, null, FillStateCheck.IsNotEmpty, null))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Dispense));
                        }
                        else if (FindNearestItem(bot, 793377380, pos, out platePos, false, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(platePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(platePos, GrabType.Pickup, default, new ItemInfo(793377380)));
                        }
                        else
                        {
                            Debug.Log("No clean plate found");
                            ClearItemMemory(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                }
                else
                {
                    GrabType grab = GrabType.Pickup;
                    if (!FindNearestItem(bot, new ItemInfo(-2135410839), pos, out wokPos, false, KitchenRoomTypes))
                    {
                        grab = GrabType.Dispense;
                        if (!GetNearestAppliance(pos, new HashSet<int> { 314862254 }, out wokPos, out _, null, FillStateCheck.IsNotEmpty))
                        {
                            Debug.Log("No woks found");
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                    EntityManager.AddComponentData(bot, new CMoveTo(wokPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(wokPos, grab));
                }
            }
        }

        private void CheeseBoardFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 681117884)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, FillStateCheck.Ignore, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 252763172)
                {
                    if (GetNearestAppliance(pos, new HashSet<int> { 235423916 }, out var boardPos, out _, null, FillStateCheck.IsNotEmpty))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(boardPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(boardPos, GrabType.Dispense, new ItemInfo(comp)));
                    }
                    else if (FindNearestItem(bot, new ItemInfo(-626784042), pos, out boardPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(boardPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(boardPos, GrabType.Pickup, default, new ItemInfo(-626784042)));
                    }
                    else
                    {
                        Debug.LogError("No free serving boards!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                    }
                }
                else if (comp == new ItemInfo(1639948793, 252763172, -626784042))
                {
                    GetNearestAppliance(pos, new HashSet<int> { -117339838 }, out var cheesePos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(cheesePos));
                    EntityManager.AddComponentData(bot, new CGrabAction(cheesePos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { 1834063794 }, out var nutsPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(nutsPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(nutsPos, GrabType.Dispense));
                }
            }
            else
            {
                if (FindNearestItem(bot, new ItemInfo(252763172), pos, out var slicedPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(slicedPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(slicedPos, GrabType.Pickup, default, new ItemInfo(252763172)));
                    ClearItemMemory(bot);
                }
                else if (FindNearestItem(bot, new ItemInfo(681117884), pos, out var applePos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(applePos));
                    EntityManager.AddComponentData(bot, new CInteractAction(applePos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(252763172), applePos);
                }
                else
                {
                    GetNearestAppliance(pos, new HashSet<int> { -905438738 }, out var providerPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                }
            }
        }

        private void ApplePieFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1296980128 || comp.ID == 681117884)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1963815217)
                {
                    if (FindNearestItem(bot, 252763172, pos, out var applePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(applePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(applePos, GrabType.CombineDrop));
                        AddItemMemory(bot, new ItemInfo(-642148977, 252763172, 1963815217), applePos);
                    }
                    else
                    {
                        if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(1963815217), counterPos);
                        }
                        else
                        {
                            Debug.Log("No free counter");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                }
                else if (comp.ID == 252763172)
                {
                    if (FindNearestItem(bot, 1963815217, pos, out var hobPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hobPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hobPos, GrabType.CombineDrop));
                    }
                    else if (!FindNearestItem(bot, 164600160, pos, out _, false, KitchenRoomTypes))
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 164600160)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            if (!HasBuffer<CBotWatching>(bot))
                                EntityManager.AddBuffer<CBotWatching>(bot);

                            var buffer = EntityManager.GetBuffer<CBotWatching>(bot);
                            buffer.Add(new CBotWatching(hobPos, 82666420));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
            }
            else
            {
                if (WatchedCheck(bot))
                    return;

                var numPies = 0;
                if (RequireBuffer<CBotWatching>(bot, out var buffer))
                    numPies = buffer.Length;

                if (TryGetItemMemory(bot, 681117884, out var applePos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(applePos));
                    EntityManager.AddComponentData(bot, new CInteractAction(applePos, true));
                    RemoveItemMemory(bot, new ItemInfo(681117884));
                    AddItemMemory(bot, new ItemInfo(252763172), applePos);
                }
                else if (FindNearestItem(bot, new ItemInfo(1296980128), pos, out var doughPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(doughPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(doughPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(164600160), doughPos);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(164600160), new ItemInfo(1963815217) }, pos, out var crustPos, out var item, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(crustPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(crustPos, GrabType.Pickup, default, item));
                    ClearItemMemory(bot);
                }
                else if (numPies < orders.Count && numPies < 2 && GetNearestAppliance(pos, CookingAppliances, out var _, out _, true, null, KitchenRoomTypes))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var flourPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(flourPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(flourPos, GrabType.Dispense));
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(164600160), new ItemInfo(1963815217) }, pos, out _, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, 252763172, pos, out var slicePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(slicePos, GrabType.Pickup, default, new ItemInfo(252763172)));
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -905438738 }, out applePos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(applePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(applePos, GrabType.Dispense));
                    }
                }
            }
        }

        private void PumpkinPieFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1296980128 || comp.ID == -165143951)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1963815217)
                {
                    if (FindNearestItem(bot, 252763172, pos, out var applePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(applePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(applePos, GrabType.CombineDrop));
                        AddItemMemory(bot, new ItemInfo(-642148977, 252763172, 1963815217), applePos);
                    }
                    else
                    {
                        if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                            EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                            AddItemMemory(bot, new ItemInfo(1963815217), counterPos);
                        }
                        else
                        {
                            Debug.Log("No free counter");
                            EmptyHands(bot);
                            RemoveFromOrder(bot, orders[0]);
                        }
                    }
                }
                else if (comp.ID == -1498186615)
                {
                    EmptyHands(bot);
                }
                else if (comp.ID == -711877651)
                {
                    if (FindNearestItem(bot, 1963815217, pos, out var hobPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(hobPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(hobPos, GrabType.CombineDrop));
                    }
                    else if (!FindNearestItem(bot, 164600160, pos, out _, false, KitchenRoomTypes))
                    {
                        EmptyHands(bot);
                    }
                }
                else if (comp.ID == 164600160)
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            if (!HasBuffer<CBotWatching>(bot))
                                EntityManager.AddBuffer<CBotWatching>(bot);

                            var buffer = EntityManager.GetBuffer<CBotWatching>(bot);
                            buffer.Add(new CBotWatching(hobPos, -126602470));
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
            }
            else
            {
                if (WatchedCheck(bot))
                    return;

                var numPies = 0;
                if (RequireBuffer<CBotWatching>(bot, out var buffer))
                    numPies = buffer.Length;

                if (TryGetItemMemory(bot, -165143951, out var pumpkinPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pumpkinPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(pumpkinPos, true));
                    RemoveItemMemory(bot, new ItemInfo(-165143951));
                    AddItemMemory(bot, new ItemInfo(951737916), pumpkinPos);
                }
                else if (TryGetItemMemory(bot, 951737916, out pumpkinPos))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(pumpkinPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(pumpkinPos, true));
                    RemoveItemMemory(bot, new ItemInfo(951737916));
                    AddItemMemory(bot, new ItemInfo(-711877651), pumpkinPos);
                }
                else if (FindNearestItem(bot, new ItemInfo(1296980128), pos, out var doughPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(doughPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(doughPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(164600160), doughPos);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(164600160), new ItemInfo(1963815217) }, pos, out var crustPos, out var item, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(crustPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(crustPos, GrabType.Pickup, default, item));
                    ClearItemMemory(bot);
                }
                else if (numPies < orders.Count && numPies < 2 && GetNearestAppliance(pos, CookingAppliances, out var _, out _, true, null, KitchenRoomTypes))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var flourPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(flourPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(flourPos, GrabType.Dispense));
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(164600160), new ItemInfo(1963815217) }, pos, out _, out _, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    if (FindNearestItem(bot, -711877651, pos, out var slicePos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(slicePos));
                        EntityManager.AddComponentData(bot, new CGrabAction(slicePos, GrabType.Pickup, default, new ItemInfo(-711877651)));
                    }
                    else
                    {
                        GetNearestAppliance(pos, new HashSet<int> { -1055654549 }, out var providerPos, out _);
                        EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
                    }
                }
            }
        }

        private void CherryPieFunction(Entity bot, List<ItemInfo> orders)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (orders[0] == comp)
                {
                    if (GetBestDropOff(pos, out var dropPos))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(dropPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(dropPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true))
                    {
                        Debug.LogError("No hatch free, dropping on next free counter");
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                    }
                    else
                    {
                        Debug.LogError("No dropoff location free!");
                        EmptyHands(bot);
                    }
                    RemoveFromOrder(bot, orders[0]);
                    ClearItemMemory(bot);
                }
                else if (comp.ID == 1378842682)
                {
                    GetNearestAppliance(pos, WaterProviders, out var waterPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(waterPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(waterPos, false));
                }
                else if (comp.ID == 1296980128)
                {
                    if (GetNearestAppliance(pos, Counters, out var counterPos, out _, true, null, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(counterPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(counterPos, GrabType.Drop, new ItemInfo(comp)));
                        AddItemMemory(bot, new ItemInfo(comp.ID), counterPos);
                    }
                    else
                    {
                        Debug.LogError("No free Counter!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
                else if (comp.ID == 1963815217)
                {
                    GetNearestAppliance(pos, new HashSet<int> { 148543530 }, out var cherryPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(cherryPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(cherryPos, GrabType.Dispense, new ItemInfo(comp)));
                }
                else if (comp.ID == 164600160 || comp == new ItemInfo(-135657781, -2056677123, 1963815217))
                {
                    if (GetNearestAppliance(pos, CookingAppliances, out var hobPos, out _, true, null, KitchenRoomTypes))
                    {
                        if (HobInteraction(bot, hobPos, GrabType.Drop))
                        {
                            if (!HasBuffer<CBotWatching>(bot))
                                EntityManager.AddBuffer<CBotWatching>(bot);

                            var buffer = EntityManager.GetBuffer<CBotWatching>(bot);
                            if (comp.ID == 164600160)
                            {
                                buffer.Add(new CBotWatching(hobPos, 1963815217));
                            }
                            else
                            {
                                buffer.Add(new CBotWatching(hobPos, 1842093636));
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("No free hob!");
                        EmptyHands(bot);
                        RemoveFromOrder(bot, orders[0]);
                        ClearItemMemory(bot);
                    }
                }
            }
            else
            {
                if (WatchedCheck(bot))
                    return;
                
                var numPies = 0;
                if (RequireBuffer<CBotWatching>(bot, out var buffer))
                    numPies = buffer.Length;

                if (FindNearestItem(bot, new ItemInfo(1842093636), pos, out var piePos, false, CookingAppliances, null, KitchenRoomTypes))
                {
                    HobInteraction(bot, piePos, GrabType.Pickup);
                }
                else if (FindNearestItem(bot, new ItemInfo(1296980128), pos, out var doughPos, false, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(doughPos));
                    EntityManager.AddComponentData(bot, new CInteractAction(doughPos, true));
                    ClearItemMemory(bot);
                    AddItemMemory(bot, new ItemInfo(164600160), doughPos);
                }
                else if (FindNearestItem(bot, new HashSet<ItemInfo> { new ItemInfo(164600160), new ItemInfo(1963815217) }, pos, out var crustPos, out var item, false, null, CookingAppliances, KitchenRoomTypes))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(crustPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(crustPos, GrabType.Pickup, default, item));
                    ClearItemMemory(bot);
                }
                else if (numPies < orders.Count && numPies < 2 && GetNearestAppliance(pos, CookingAppliances, out var _, out _, true, null, KitchenRoomTypes))
                {
                    GetNearestAppliance(pos, new HashSet<int> { 925796718 }, out var flourPos, out _);
                    EntityManager.AddComponentData(bot, new CMoveTo(flourPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(flourPos, GrabType.Dispense));
                }
            }
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
                    EntityManager.AddComponentData(bot, new CGrabAction(dropoff, GrabType.Drop, new ItemInfo(comp)));
                    RemoveFromOrder(bot, orders[0]);
                }
                else
                {
                    Debug.LogError("No dropoff location free!");
                }
            }
            else
            {
                GetNearestAppliance(pos, new HashSet<int> { itemInfo.DedicatedProvider.ID }, out var providerPos, out _);
                EntityManager.AddComponentData(bot, new CMoveTo(providerPos));
                EntityManager.AddComponentData(bot, new CGrabAction(providerPos, GrabType.Dispense));
            }
        }
    }
}
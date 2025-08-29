using Kitchen;
using Kitchen.Layouts;
using KitchenMods;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using KitchenData;
using Photon.Realtime;
using System.Linq;

namespace Neuro_Plateup
{
    public class CookingSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, ServiceQuery, OrderQuery, HeldItems;

        private MoveToSystem moveTo;

        public static HashSet<int> CookingAppliances;
        public static HashSet<int> DishWashers;
        public static HashSet<int> Sinks;
        public static HashSet<int> Bins;
        public static HashSet<int> Counters;
        public static HashSet<int> WaterProviders;
        public static HashSet<int> Plates;
        public static HashSet<int> Tables;
        public static HashSet<int> DirtyPlates;

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

        private static Dictionary<int, ItemCreationProcess> ItemSources = new Dictionary<int, ItemCreationProcess>();

        protected override void Initialise()
        {
            base.Initialise();

            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CBotAction)
                    ));
            ServiceQuery = GetEntityQuery(typeof(CGroupReadyToOrder));
            OrderQuery = GetEntityQuery(typeof(CGroupAwaitingOrder));
            HeldItems = GetEntityQuery(
                new QueryHelper()
                    .All(
                        //typeof(CItemStorage),
                        typeof(CItem),
                        typeof(CHeldBy)
                    ));
            CookingAppliances = new HashSet<int> { 1154757341, -1448690107, 1266458729, 862493270, -441141351, 805530854, 944301512, -1311702572, -1068749602, 782648278, -1688921160 };
            DishWashers = new HashSet<int> { -214126192, -823922901 };
            Sinks = new HashSet<int> { 1083874952, 1467371088, -266993023, 540526865 };
            Bins = new HashSet<int> { 2127051779, -1632826946, -1855909480, 481495292, 1551609169, 620400448, 1159228054, 1492264331 }; // Infinite Bin: 1159228054
            Counters = new HashSet<int> { -1248669347, 1365340297, 1129858275, -1857890774 };
            WaterProviders = new HashSet<int> { 1467371088, 1083874952, -266993023 };
            Plates = new HashSet<int> { 540526865, 380220741, 1313469794 };
            Tables = new HashSet<int> { 209074140, -3721951, -34659638, -203679687, -2019409936 };
            DirtyPlates = new HashSet<int> { 1517992271, -1527669626, 348289471 };

            var ProcessAppliances = new Dictionary<int, HashSet<int>>
            {
                {
                    1972879238,
                    CookingAppliances
                },
                {
                    2087693779,
                    Counters
                },
                {
                    -1316622579,
                    new HashSet<int> { -1609758240, -349733673 }
                },
                {
                    -523839730,
                    Counters
                },
                {
                    620897674,
                    Sinks
                },
                {
                    -2048664109,
                    new HashSet<int>()
                },
                {
                    1393363605,
                    Counters
                }

            };

            foreach (var item in Data.Get<Item>())
            {
                foreach (var process in item.DerivedProcesses)
                {
                    if (process.Result is null || process.Result.ID == -1960690485 || process.Result.ID == 9768533)
                    {
                        // Filter Burned Food & Burned Fish
                        continue;
                    }
                    if (!ItemSources.ContainsKey(process.Result.ID))
                    {
                        ItemSources[process.Result.ID] = new ItemCreationProcess
                        {
                            ID = process.Process.ID,
                            itemIDs = new HashSet<int> { item.ID },
                            Appliances = ProcessAppliances[process.Process.ID]
                        };
                    }
                    else
                    {
                        ItemSources[process.Result.ID].itemIDs.Add(item.ID);
                    }
                }
                if (item.IsSplittable && item.SplitSubItem != null)
                {
                    if (!ItemSources.ContainsKey(item.SplitSubItem.ID))
                    {
                        ItemSources[item.SplitSubItem.ID] = new ItemCreationProcess
                        {
                            ID = 0,
                            itemIDs = new HashSet<int> { item.ID },
                            Appliances = Counters
                        };
                    }
                    else
                    {
                        ItemSources[item.SplitSubItem.ID].itemIDs.Add(item.ID);
                    }
                }
            }
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();

            moveTo = World.GetExistingSystem<MoveToSystem>();
        }

        protected override void OnUpdate()
        {
            if (Has<CSceneFirstFrame>())
            {
                // NYI: Do we need this?
            }
            var Waiting = GetEntityQuery(typeof(CBotWaiting)).ToEntityArray(Allocator.Temp);
            foreach (var bot in Waiting)
            {
                var comp = GetComponent<CBotWaiting>(bot);
                var ent = TileManager.GetPrimaryOccupant(comp.Position);
                if (!HasComponentOfHeld<CItem>(ent) || GetComponentOfHeld<CItem>(ent, out var cheld) && cheld.ID == comp.itemID)
                {
                    EntityManager.RemoveComponent<CBotWaiting>(bot);
                    continue;
                }
            }
            Waiting.Dispose();
        }

        // private bool IsProgressing(Vector3 position)
        // {
        //     var ProgressBars = GetEntityQuery(typeof(CProgressIndicator)).ToEntityArray(Allocator.Temp);
        //     foreach (var bar in ProgressBars)
        //     {
        //         var p = GetComponent<CPosition>(bar).Position;
        //         if (p == position)
        //         {
        //             bool flag;
        //             var comp = GetComponent<CProgressIndicator>(bar);
        //             if (comp.IsUnknownLength)
        //             {
        //                 flag = !comp.IsBad;
        //             }
        //             else
        //             {
        //                 flag = comp.CurrentChange > 0;
        //             }
        //             ProgressBars.Dispose();
        //             return flag;
        //         }
        //     }
        //     ProgressBars.Dispose();
        //     return false;
        // }

        // private bool GetNearest(ComponentType type, Vector3 origin, out Vector3 nearest)
        // {
        //     nearest = new Vector3();
        //     float closestDistanceSqr = float.MaxValue;

        //     var query = GetEntityQuery(type);
        //     if (query.IsEmptyIgnoreFilter)
        //         return false;

        //     var entities = query.ToEntityArray(Allocator.Temp);
        //     foreach (var entity in entities)
        //     {
        //         var pos = GetComponent<CPosition>(entity).Position;
        //         float distSqr = (pos - origin).sqrMagnitude;
        //         if (distSqr < closestDistanceSqr)
        //         {
        //             closestDistanceSqr = distSqr;
        //             nearest = pos;
        //         }
        //     }
        //     entities.Dispose();

        //     return true;
        // }

        private void EmptyHands(Entity bot)
        {
            // NYI: return things like the sharp knife
            // NYI: Find a better way to empty the hands
            EntityManager.AddComponent<CReturnItem>(GetComponent<CItemHolder>(bot).HeldItem);
        }

        private bool IsFullAppliance(Entity appliance)
        {
            if (Require<CApplianceBin>(appliance, out var compBin))
            {
                return compBin.CurrentAmount == compBin.Capacity;
            }
            else if (Require<CItemProvider>(appliance, out var compProvider))
            {
                return compProvider.Available == compProvider.Maximum;
            }
            // NYI?: prep stations
            // NYI?: preserving station
            return false;
        }

        public bool GetNearestAppliance(Vector3 sourceTile, HashSet<int> validAppliances, out Vector3 targetPos, out int targetID, bool onlyEmpty = false, bool notFull = false)
        {
            targetPos = new Vector3();
            targetID = new int();

            var Appliances = GetEntityQuery(typeof(CAppliance)).ToEntityArray(Allocator.Temp);
            var flag = false;
            var currentSteps = int.MaxValue;
            foreach (var appliance in Appliances)
            {
                var ID = GetComponent<CAppliance>(appliance).ID;
                if (onlyEmpty)
                {
                    if (!HasComponent<CItemHolder>(appliance))
                        continue;
                    var held = GetComponent<CItemHolder>(appliance).HeldItem;
                    if (HasComponent<CItem>(held))
                        continue;
                }
                if (notFull && IsFullAppliance(appliance))
                {
                    continue;
                }

                if (validAppliances.Contains(ID))
                {
                    var pos = GetComponent<CPosition>(appliance).Position;

                    if (TileManager.CanReach(sourceTile, pos))
                    {
                        flag = true;
                        targetPos = pos;
                        targetID = ID;
                        break;
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

        public bool FindNearestItem(HashSet<int> list, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
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
                    if (MoveToSystem.Hatches.Contains(comp3.Position))
                    {
                        if (noHatches)
                        {
                            continue;
                        }
                    }
                    else if (!validRoomTypes.Contains(tile.Type))
                    {
                        continue;
                    }

                    if (TileManager.CanReach(start, comp3.Position))
                    {
                        position = comp3.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
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
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(ItemList list, Vector3 start, out Vector3 position, bool noHatches = true, HashSet<RoomType> validRoomTypes = null)
        {
            validRoomTypes ??= AllRoomTypes;
            position = new Vector3();
            var flag = false;
            var currentSteps = int.MaxValue;
            var Items = HeldItems.ToEntityArray(Allocator.Temp);
            foreach (var item in Items)
            {
                var comp = GetComponent<CItem>(item);
                if (comp.Items.IsEquivalent(list))
                {
                    var holder = GetComponent<CHeldBy>(item).Holder;
                    if (HasComponent<CPlayer>(holder))
                        continue;

                    var pos = GetComponent<CPosition>(holder).Position;
                    var tile = TileManager.GetTile(pos);
                    if (MoveToSystem.Hatches.Contains(pos))
                    {
                        if (noHatches)
                        {
                            continue;
                        }
                    }
                    else if (!validRoomTypes.Contains(tile.Type))
                    {
                        continue;
                    }

                    if (TileManager.CanReach(start, pos))
                    {
                        position = pos;
                        currentSteps = 0;
                        flag = true;
                        break;
                    }

                    if (moveTo.GetWaypoint(start, pos, out var wp, out var steps))
                    {
                        if (steps < currentSteps)
                        {
                            currentSteps = steps;
                            position = pos;
                            flag = true;
                        }
                    }
                }
            }
            Items.Dispose();
            return flag;
        }

        public bool FindNearestItem(HashSet<int> list, Vector3 start, HashSet<int> validAppliances, out Vector3 position, HashSet<RoomType> validRoomTypes = null)
        {
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

                    if (!Require<CAppliance>(comp2.Holder, out var comp3) || !validAppliances.Contains(comp3.ID))
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

                    if (TileManager.CanReach(start, comp4.Position))
                    {
                        position = comp4.Position;
                        currentSteps = 0;
                        flag = true;
                        break;
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
                        if (moveTo.GetWaypoint(startpos, pos, out var wp, out var steps) || steps == 0)
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

        private bool Assemble(Entity bot, int ItemID, ItemList list, ItemList subItems)
        {
            // Debug.Log("Assembly:");
            // Debug.Log("Components we have assembled:");
            // foreach (var i in subItems)
            // {
            //     Debug.Log(i);
            // }
            // Debug.Log("Components we need to assemble:");
            // foreach (var i in list)
            // {
            //     Debug.Log(i);
            // }
            var pos = GetComponent<CPosition>(bot).Position;
            var Items = list.AsArray();
            if (GetComponentOfHeld<CItem>(bot, out var held))
            {
                if (subItems.Count > 0 && !held.Items.IsEquivalent(subItems))
                {
                    EmptyHands(bot);
                }
                else
                {
                    Data.TryGet<Item>(Items[0], out var itemData);
                    if (itemData.DedicatedProvider != null && GetNearestAppliance(pos, new HashSet<int> { itemData.DedicatedProvider.ID }, out var provider, out var applianceID))
                    {
                        if (applianceID == -1533430406)
                        {
                            return GetIcecream(bot, Items[0]);
                        }
                        else
                        {
                            // Write a method that checks for plates
                            EntityManager.AddComponentData(bot, new CMoveTo(provider));
                            EntityManager.AddComponentData(bot, new CGrabAction(provider));
                        }
                    }
                    else if (FindNearestItem(list, pos, out var itemPos, false, KitchenRoomTypes))
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(itemPos));
                        EntityManager.AddComponentData(bot, new CGrabAction(itemPos));
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (subItems.Count > 0 && FindNearestItem(subItems, pos, out var target, false, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(target));
                EntityManager.AddComponentData(bot, new CGrabAction(target));
            }
            else
            {
                Data.TryGet<Item>(Items[0], out var itemData);
                if (itemData.DedicatedProvider != null && GetNearestAppliance(pos, new HashSet<int> { itemData.DedicatedProvider.ID }, out var provider, out var applianceID))
                {
                    if (applianceID == -1533430406)
                    {
                        return GetIcecream(bot, Items[0]);
                    }
                    else
                    {
                        // Write a method that checks for plates
                        EntityManager.AddComponentData(bot, new CMoveTo(provider));
                        EntityManager.AddComponentData(bot, new CGrabAction(provider));
                    }
                }
                else if (FindNearestItem(new HashSet<int>(Items), pos, out var itemPos, false, KitchenRoomTypes))
                {
                    // NYI: This does not respect the order of assembly probably
                    EntityManager.AddComponentData(bot, new CMoveTo(itemPos));
                    EntityManager.AddComponentData(bot, new CGrabAction(itemPos));
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryToPrepare(Entity bot, int itemID, int targetItem = 0)
        {
            // Return value signals if an action is needed
            // It should be false if we assign no action
            var pos = GetComponent<CPosition>(bot).Position;
            if (GetComponentOfHeld<CItem>(bot, out var cheld) && cheld.ID == itemID)
            {
                EmptyHands(bot);
                return true;
            }
            if (ItemSources.TryGetValue(itemID, out var process))
            {
                if (FindNearestItem(process.itemIDs, pos, out var target, false, KitchenRoomTypes))
                {
                    var applianceID = GetComponent<CAppliance>(TileManager.GetPrimaryOccupant(target)).ID;
                    if (process.Appliances.Contains(applianceID))
                    {
                        // The thing is on the right appliance just process it
                        EntityManager.AddComponentData(bot, new CMoveTo(target));
                        EntityManager.AddComponentData(bot, new CInteractAction(target, true));
                    }
                    else
                    {
                        EntityManager.AddComponentData(bot, new CMoveTo(target));
                        EntityManager.AddComponentData(bot, new CGrabAction(target));
                    }
                    return true;
                }
                if (process.itemIDs.Contains(cheld.ID))
                {
                    // We are holding the thing find the correct appliance and put it on it
                    if (GetNearestAppliance(pos, process.Appliances, out var appliancePos, out var _, true))
                    {
                        if (process.ID != 1972879238)
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                        }
                        else
                        {
                            // NYI: Ovens and microwaves
                            // return UseHob(bot, appliancePos, applianceID);
                            EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                            EntityManager.AddComponentData(bot, new CBotWaiting(appliancePos, itemID));
                        }
                    }
                    else
                    {
                        Debug.LogError("Oh noes there was no free appliance to process " + cheld.ID);
                    }
                    return true;
                }
                foreach (var sourceItemID in process.itemIDs)
                {
                    var item = Data.Get<Item>(sourceItemID);
                    if (item.DedicatedProvider != null)
                    {
                        // Find corresponding provider and use it
                        if (GetNearestAppliance(pos, new HashSet<int> { item.DedicatedProvider.ID }, out var appliancePos, out var _))
                        {
                            EntityManager.AddComponentData(bot, new CMoveTo(appliancePos));
                            EntityManager.AddComponentData(bot, new CGrabAction(appliancePos));
                            return true;
                        }
                        else
                        {
                            // NYI: Edge case Fresh patties
                        }
                    }
                }
                return TryToPrepare(bot, process.itemIDs.First(), itemID);
            }
            else
            {
                Debug.LogError($"No process for item {itemID} found");
            }
            return false;
        }

        private bool IsPresentOrHasProvider(Entity bot, int item)
        {
            var pos = GetComponent<CPosition>(bot).Position;
            if (GetComponentOfHeld<CItem>(bot, out var cheld) && cheld.ID == item)
            {
                return true;
            }
            if (!FindNearestItem(new HashSet<int> { item }, pos, out var target, false, KitchenRoomTypes))
            {
                return Data.TryGet<Item>(item, out var itemData) && itemData.DedicatedProvider != null;
            }
            return true;
        }

        private bool FindItemAssembly(Entity bot, ItemList Items, out ItemList subItems)
        {
            var list = Items.AsArray();
            subItems = new ItemList();
            var ents = HeldItems.ToEntityArray(Allocator.Temp);
            var currentLength = 0;
            foreach (var ent in ents)
            {
                var holder = GetComponent<CHeldBy>(ent).Holder;
                if (HasComponent<CPlayer>(holder) && holder != bot)
                    continue;

                var tile = TileManager.GetTile(GetComponent<CPosition>(holder).Position);
                if (tile.Type != RoomType.Kitchen || MoveToSystem.Hatches.Contains(tile.Position))
                    continue;

                var comp = GetComponent<CItem>(ent);
                var flag = true;
                if (comp.Items.Count <= list.Length)
                {
                    foreach (var item in comp.Items)
                    {
                        if (!list.Contains(item))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                else
                {
                    flag = false;
                }
                if (flag && comp.Items.Count > currentLength)
                {
                    subItems = comp.Items;
                }
            }
            ents.Dispose();
            return subItems.Count > 0;
        }

        public bool AvailabilityCheck(Entity bot, int ItemID, ItemList list)
        {
            ItemList Items = list;
            if (FindItemAssembly(bot, list, out var subItems))
            {
                foreach (var subItem in subItems)
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        if (Items[i] == subItem)
                        {
                            Items.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            foreach (var item in Items)
            {
                if (!IsPresentOrHasProvider(bot, item))
                {
                    if (TryToPrepare(bot, item))
                    {
                        return true;
                    }
                }
            }

            if (!Assemble(bot, ItemID, Items, subItems))
                return false;

            return true;
        }

        public void Cook(Entity bot, int ItemID, ItemList Items)
        {
            var pos = GetComponent<CPosition>(bot).Position.Rounded();
            if (GetComponentOfHeld<CItem>(bot, out var comp))
            {
                if (comp.Items.IsEquivalent(Items) && GetBestDropOff(pos, out var target))
                {
                    EntityManager.AddComponentData(bot, new CMoveTo(target));
                    EntityManager.AddComponentData(bot, new CGrabAction(target));
                    return;
                }
            }
            else if (FindNearestItem(Items, pos, out var targetMeal, true, KitchenRoomTypes))
            {
                EntityManager.AddComponentData(bot, new CMoveTo(targetMeal));
                EntityManager.AddComponentData(bot, new CGrabAction(targetMeal));
                return;
            }
            if (!AvailabilityCheck(bot, ItemID, Items))
            {
                EntityManager.RemoveComponent<CBotAction>(bot);
            }
        }
    }
}
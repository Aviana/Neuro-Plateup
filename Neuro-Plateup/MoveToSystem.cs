using Kitchen;
using KitchenMods;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Controllers;
using System.Collections.Generic;
using Kitchen.Layouts;
using Kitchen.Layouts.Modules;
using System.Linq;

namespace Neuro_Plateup
{
    public class MoveToSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, InputCapturesQuery, LayoutQuery, LayoutResetQuery;
        private FakeInput input;
        public static TilePairContainer Doors, Hatches;
        public static HashSet<int> Whitelist;

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1),   // up-right
            new Vector2Int(-1, 1),  // up-left
            new Vector2Int(1, -1),  // down-right
            new Vector2Int(-1, -1)  // down-left
        };

        private static readonly Vector2Int[] DirectionsOrthagonal =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        // NYI: generate position markers for Kitchen & dining so the bots can move there with go_to actions
        // NYI: Think about positioning bots on certain actions / start of the day

        private static List<Vector3> FranchiseTable;

        protected override void Initialise()
        {
            base.Initialise();
            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CMoveTo)
                    ));
            InputCapturesQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPopup),
                        typeof(CCaptureInput)
                    ).None(
                        typeof(CCapturePassthrough)
                    ));
            LayoutQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(SLayout),
                        typeof(CLayoutFeature)
                    ));
            LayoutResetQuery = GetEntityQuery(
                new QueryHelper()
                    .Any(
                        typeof(CSceneFirstFrame),
                        typeof(SIsDayFirstUpdate),
                        typeof(SIsNightFirstUpdate)
                    ));
            Doors = new TilePairContainer();
            Hatches = new TilePairContainer();
            input = new FakeInput();

            FranchiseTable = new List<Vector3>();
            for (var x = 1; x > -4; x--)
            {
                for (var z = 1; z > -1; z--)
                {
                    FranchiseTable.Add(new Vector3(x, 0, z));
                }
            }

            Whitelist = new HashSet<int>
            {
                1553046198,  // Blueprint Letter
                1063254979,  // Blueprint
                1936421857,  // Parcel
                2041631136,  // Wallpaper Applicator
                1732122842,  // Flooring Applicator
                -1723340146, // Robot Buffer
                -2147057861, // Robot Mop
                2044081363,  // Levitation Line
                -1992638820, // Enchanted Broom
                -560953757,  // Ghost Scrubber
                689268680,   // Ghostly Rolling Pin
                1313278365,  // Ghostly Knife
                -1946127856, // Ghostly Clipboard
                1765889988   // Kitchen Floor Protector
            };
        }

        protected override void OnUpdate()
        {
            if (!LayoutResetQuery.IsEmptyIgnoreFilter)
            {
                Doors.Clear();
                Hatches.Clear();
            }
            if (!LayoutQuery.IsEmptyIgnoreFilter && Doors.Count == 0)
            {
                // NYI: Illusion Wall
                var layout = LayoutQuery.GetSingletonEntity();
                foreach (CLayoutFeature feature in EntityManager.GetBuffer<CLayoutFeature>(layout))
                {
                    if (feature.Type.IsDoor())
                    {
                        if (IsBlocked(feature.Tile1) || IsBlocked(feature.Tile2))
                        {
                            Hatches.Add(feature.Tile1, feature.Tile2);
                        }
                        else
                        {
                            Doors.Add(feature.Tile1, feature.Tile2);
                        }
                    }
                    else if (feature.Type == FeatureType.Hatch)
                    {
                        Hatches.Add(feature.Tile1, feature.Tile2);
                    }
                }

                var additionalHatches = new List<HashSet<Vector3>>();
                foreach (var hatch in Hatches)
                {
                    var Tile1 = TileManager.GetTile(hatch[0]);
                    var Tile2 = TileManager.GetTile(hatch[1]);
                    foreach (var target in GetSides(hatch[0], hatch[1]))
                    {
                        var TileTarget = TileManager.GetTile(target);
                        if (Tile1.RoomID != TileTarget.RoomID)
                        {
                            additionalHatches.Add(new HashSet<Vector3> { target, hatch[0] });
                        }
                    }
                    foreach (var target in GetSides(hatch[1], hatch[0]))
                    {
                        var TileTarget = TileManager.GetTile(target);
                        if (Tile2.RoomID != TileTarget.RoomID)
                        {
                            additionalHatches.Add(new HashSet<Vector3> { target, hatch[1] });
                        }
                    }
                }
                foreach (var h in additionalHatches)
                {
                    Hatches.Add(h);
                }
            }

            if (!InputCapturesQuery.IsEmptyIgnoreFilter || BotQuery.IsEmptyIgnoreFilter)
                return;

            var BotEntities = BotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in BotEntities)
            {
                var evt = new InputUpdateEvent();
                evt.User = GetComponent<CPlayer>(bot).ID;
                Vector3 playerPos = GetComponent<CPosition>(bot).Position.Rounded();
                Vector3 destination = GetComponent<CMoveTo>(bot).Position;

                // NYI: Do not do full pathfinding on every frame
                // NYI: Handling getting stuck
                if (GetWaypoint(playerPos, destination, out Vector3 wp, out int steps))
                {
                    evt.State.Movement = new Vector2(wp.x - playerPos.x, wp.z - playerPos.z).normalized;
                    input.Send(evt);
                }
                else
                {
                    EntityManager.RemoveComponent<CMoveTo>(bot);
                }
            }
            BotEntities.Dispose();
        }

        public bool GetWaypoint(Vector3 playerPos, Vector3 destination, out Vector3 wp, out int steps)
        {
            steps = 0;
            wp = playerPos;
            if (playerPos == destination)
            {
                return false;
            }

            Queue<Vector3> frontier = new Queue<Vector3>();
            Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

            frontier.Enqueue(playerPos);
            cameFrom[playerPos] = playerPos;

            bool found = false;
            int safetycounter = 0;
            while (frontier.Count > 0 && !found)
            {
                if (safetycounter > 1000)
                {
                    Debug.LogError("Infinite loop detected!");
                    return false;
                }
                safetycounter++;
                Vector3 current = frontier.Dequeue();

                foreach (var dir in Directions)
                {
                    Vector3 next = current + new Vector3(dir.x, 0, dir.y);

                    if (cameFrom.ContainsKey(next))
                        continue;

                    if (!RoomCheck(current, next, destination) || !CanReach(current, next) && next != destination)
                        continue;

                    frontier.Enqueue(next);
                    cameFrom[next] = current;

                    if (next == destination)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!cameFrom.ContainsKey(destination))
            {
                Debug.LogError("Pathfinding failed due to obstacle!");
                steps = -1;
                return false;
            }

            Vector3 step = destination;
            if (IsBlocked(destination))
            {
                step = cameFrom[destination];
                if (step == playerPos)
                    return false;
            }

            while (cameFrom[step] != playerPos)
            {
                step = cameFrom[step];
                steps++;
            }

            wp = step;
            return true;
        }

        public bool RoomCheck(Vector3 from, Vector3 to, Vector3 destination)
        {
            var room1 = TileManager.GetTile(from).RoomID;
            var room2 = TileManager.GetTile(to).RoomID;

            if (room1 != room2)
            {
                if (Doors.Contains(from) && Doors.Contains(to) || to == destination && IsBlocked(destination) && Hatches.Contains(from, to))
                    return true;
                else
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanReach(Vector3 from, Vector3 to)
        {
            // Man i hate that stupid table
            if (HasSingleton<SFranchiseMarker>() && FranchiseTable.Contains(to))
                return false;

            var RoomID = TileManager.GetTile(from).RoomID;

            // On vertical steps the adjacent tiles must also be free
            if (from.x != to.x && from.z != to.z)
            {
                Vector3 neighbor1 = new Vector3 { x = from.x, z = from.z + to.z - from.z, y = from.y };
                Vector3 neighbor2 = new Vector3 { x = from.x + to.x - from.x, z = from.z, y = from.y };
                var tile1 = TileManager.GetTile(neighbor1);
                var tile2 = TileManager.GetTile(neighbor2);

                if (tile1.RoomID != RoomID || tile2.RoomID != RoomID)
                    return false;
                if (IsBlocked(neighbor1) || IsBlocked(neighbor2))
                    return false;
            }

            return !IsBlocked(to);
        }
        public bool IsBlocked(Vector3 vec)
        {
            // Man i hate that stupid table
            if (HasSingleton<SFranchiseMarker>() && FranchiseTable.Contains(vec))
                return true;

            var ent = TileManager.GetPrimaryOccupant(vec);

            if (Require<CApplianceChair>(ent, out var comp) && comp.IsInUse)
                return true;

            if (Require<CAppliance>(ent, out var comp2) && Whitelist.Contains(comp2.ID))
                return true;

            if (HasComponent<CAppliance>(ent) && !HasComponent<CApplianceChair>(ent) && !HasComponent<CSlowPlayer>(ent) && !HasComponent<CDoesNotOccupy>(ent) && !HasComponent<CAllowMobilePathing>(ent))
            {
                return true;
            }
            return false;
        }

        private List<Vector3> GetSides(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;

            Vector3 left  = new Vector3(-direction.z, 0, direction.x);
            Vector3 right = -left;

            return new List<Vector3>
            {
                to + left,
                to + right
            };
        }
    }
}
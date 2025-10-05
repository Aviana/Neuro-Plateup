using Kitchen;
using KitchenMods;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Controllers;
using System.Collections.Generic;

namespace Neuro_Plateup
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ActingSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, InputCapturesQuery, LookingQuery, InteractionQuery, AttemptQuery;
        private FakeInput input;
        private static readonly Dictionary<int, float> barInfo = new Dictionary<int, float>();

        protected override void Initialise()
        {
            base.Initialise();
            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CInteractAction)
                    ).None(
                        typeof(CMoveTo),
                        typeof(CGrabAction)
                    ));
            InputCapturesQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPopup),
                        typeof(CCaptureInput)
                    ).None(
                        typeof(CCapturePassthrough)
                    ));
            LookingQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBeingLookedAt)
                    ));
            InteractionQuery = GetEntityQuery(typeof(CProgressIndicator));
            AttemptQuery = GetEntityQuery(typeof(CAttemptingInteraction));
            input = new FakeInput();

            for (var i = 1; i <= 4; i++)
            {
                barInfo.Add(i, 0f);
            }
        }

        protected override void OnUpdate()
        {
            if (!InputCapturesQuery.IsEmptyIgnoreFilter || BotQuery.IsEmptyIgnoreFilter)
                return;

            var BotEntities = BotQuery.ToEntityArray(Allocator.Temp);
            foreach (var bot in BotEntities)
            {
                // NYI: Fix items changing during travel time
                var evt = new InputUpdateEvent();
                var ID = GetComponent<CPlayer>(bot).ID;
                evt.User = ID;
                var comp = GetComponent<CInteractAction>(bot);
                var currentProgress = GetProgress(comp.Position);
                var appliance = TileManager.GetPrimaryOccupant(comp.Position);
                var pos = GetComponent<CPosition>(bot).Position;

                if ((!comp.HasProgress || currentProgress < barInfo[ID] && barInfo[ID] > 0.05f) && Require<CBeingActedOnBy>(appliance, out var buffer))
                {
                    var flag = false;
                    foreach (var entry in buffer)
                    {
                        if (entry.Interactor == bot)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        EntityManager.RemoveComponent<CInteractAction>(bot);
                        barInfo[ID] = 0f;
                        evt.State.InteractAction = ButtonState.Released;
                        input.Send(evt);
                        continue;
                    }
                }

                if (Require<CAttemptingInteraction>(bot, out var compAttempt) && compAttempt.Target == appliance)
                {
                    if (compAttempt.Type == InteractionType.Look)
                    {
                        evt.State.InteractAction = ButtonState.Pressed;
                        //evt.State.Movement = new Vector2(comp.Position.x - pos.x, comp.Position.z - pos.z);
                    }
                    else if (compAttempt.Type == InteractionType.Act)
                    {
                        evt.State.InteractAction = ButtonState.Held;
                    }
                }
                else
                {
                    var TileBot = TileManager.GetTile(pos);
                    var TileTarget = TileManager.GetTile(comp.Position);
                    var x = TileTarget.Position.x - TileBot.Position.x;
                    var z = TileTarget.Position.z - TileBot.Position.z;
                    if (TileBot.RoomID != TileTarget.RoomID && x != 0 && z != 0)
                    {
                        var horiz = new Vector3 { x = TileBot.Position.x + x, y = 0, z = TileBot.Position.z };
                        var vert = new Vector3 { x = TileBot.Position.x, y = 0, z = TileBot.Position.z + z };
                        if (TileManager.GetRoom(horiz) == TileTarget.RoomID && MoveToSystem.Hatches.Contains(horiz))
                        {
                            if (pos.x.ToString("F1") != (TileBot.Position.x + (0.1 * x)).ToString("F1"))
                            {
                                evt.State.Movement = new Vector2(horiz.x - pos.x, horiz.z - pos.z);
                            }
                            else
                            {
                                evt.State.Movement = new Vector2(comp.Position.x - pos.x, comp.Position.z - pos.z);
                                evt.State.InteractAction = ButtonState.Pressed;
                            }
                        }
                        else if (TileManager.GetRoom(vert) == TileTarget.RoomID && MoveToSystem.Hatches.Contains(vert))
                        {
                            if (pos.z.ToString("F1") != (TileBot.Position.z + (0.1 * z)).ToString("F1"))
                            {
                                evt.State.Movement = new Vector2(vert.x - pos.x, vert.z - pos.z);
                            }
                            else
                            {
                                evt.State.Movement = new Vector2(comp.Position.x - pos.x, comp.Position.z - pos.z);
                                evt.State.InteractAction = ButtonState.Pressed;
                            }
                        }
                        else
                        {
                            evt.State.Movement = new Vector2(comp.Position.x - pos.x, comp.Position.z - pos.z);
                        }
                    }
                    else
                    {
                        evt.State.Movement = new Vector2(comp.Position.x - pos.x, comp.Position.z - pos.z);
                    }
                }
                input.Send(evt);
                barInfo[ID] = currentProgress;
            }
            BotEntities.Dispose();
        }

        protected float GetProgress(Vector3 pos)
        {
            var fProgress = 0f;
            var Interactions = InteractionQuery.ToEntityArray(Allocator.Temp);
            foreach (var interaction in Interactions)
            {
                var p = GetComponent<CPosition>(interaction).Position;
                if (p == pos)
                {
                    fProgress = GetComponent<CProgressIndicator>(interaction).Progress;
                    break;
                }
            }
            Interactions.Dispose();
            return fProgress;
        }
    }
}
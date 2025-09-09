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
    public class GrabSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, InputCapturesQuery;
        private FakeInput input;
        private static readonly Dictionary<int, int> grabInfo = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } };

        protected override void Initialise()
        {
            base.Initialise();
            BotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl),
                        typeof(CGrabAction)
                    ).None(
                        typeof(CMoveTo),
                        typeof(CInteractAction)
                    ));
            InputCapturesQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CPopup),
                        typeof(CCaptureInput)
                    ).None(
                        typeof(CCapturePassthrough)
                    ));
            input = new FakeInput();
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
                var comp = GetComponent<CGrabAction>(bot);
                var appliance = TileManager.GetPrimaryOccupant(comp.Position);

                if (Require<CAttemptingInteraction>(bot, out var comp3) && comp3.Target == appliance)
                {
                    if (comp3.Type == InteractionType.Look)
                    {
                        if (grabInfo[ID] < 10)
                        {
                            grabInfo[ID]++;
                        }
                        else
                        {
                            grabInfo[ID] = 0;
                            evt.State.GrabAction = ButtonState.Pressed;
                            input.Send(evt);
                        }
                        continue;
                    }
                    else if (comp3.Type == InteractionType.Grab && comp3.Result == InteractionResult.Performed)
                    {
                        evt.State.GrabAction = ButtonState.Released;
                        input.Send(evt);
                        grabInfo[ID] = 0;
                        EntityManager.RemoveComponent<CGrabAction>(bot);
                        continue;
                    }
                }
                Vector3 posBot = GetComponent<CPosition>(bot).Position;
                evt.State.Movement = new Vector2(comp.Position.x - posBot.x, comp.Position.z - posBot.z);
                input.Send(evt);
            }
            BotEntities.Dispose();
        }
    }
}
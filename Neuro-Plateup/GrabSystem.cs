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
                var evt = new InputUpdateEvent();
                var ID = GetComponent<CPlayer>(bot).ID;
                evt.User = ID;
                var comp = GetComponent<CGrabAction>(bot);
                var appliance = TileManager.GetPrimaryOccupant(comp.Position);
                var hasHeld = GetComponentOfHeld<CItem>(appliance, out _) ^ GetComponentOfHeld<CItem>(bot, out _);

                if ((comp.Type == GrabType.Drop && !hasHeld || comp.Type == GrabType.Pickup) && !hasHeld)
                {
                    grabInfo[ID] = 0;
                    EntityManager.RemoveComponent<CGrabAction>(bot);
                    continue;
                }


                if (ApplianceCapacity(appliance, out var current, out var maximum))
                {
                    if (comp.Type == GrabType.Fill && current == maximum || comp.Type == GrabType.Dispense && current == 0)
                    {
                        grabInfo[ID] = 0;
                        EntityManager.RemoveComponent<CGrabAction>(bot);
                        continue;
                    }
                }

                if (Require<CAttemptingInteraction>(bot, out var comp3) && comp3.Target == appliance)
                {
                    if (comp3.Type == InteractionType.Look)
                    {
                        if (grabInfo[ID] < 20)
                        {
                            grabInfo[ID]++;
                        }
                        else
                        {
                            grabInfo[ID] = 0;
                            evt.State.GrabAction = ButtonState.Pressed;
                            input.Send(evt);
                        }
                    }
                    else if (comp3.Type == InteractionType.Grab && comp3.Result == InteractionResult.Performed)
                    {
                        evt.State.GrabAction = ButtonState.Released;
                        input.Send(evt);
                        grabInfo[ID] = 0;
                        EntityManager.RemoveComponent<CGrabAction>(bot);
                        
                    }
                    continue;
                }

                grabInfo[ID] = 0;
                Vector3 posBot = GetComponent<CPosition>(bot).Position;
                evt.State.Movement = new Vector2(comp.Position.x - posBot.x, comp.Position.z - posBot.z);
                input.Send(evt);
            }
            BotEntities.Dispose();
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
            current = 0;
            maximum = 0;
            return false;
        }
    }
}
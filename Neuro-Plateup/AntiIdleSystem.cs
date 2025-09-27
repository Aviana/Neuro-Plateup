using Kitchen;
using KitchenMods;
using Kitchen.Modules;
using Unity.Entities;
using Unity.Collections;
using Controllers;
using System.Reflection;
using System.Collections.Generic;

namespace Neuro_Plateup
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AntiIdleSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, IdleBotQuery, PopupQuery, UnlockQuery;
        private FakeInput input;

        private FieldInfo GenericField, EndOfDayField, StartDayField, EndPracticeField;

        protected override void Initialise()
        {
            base.Initialise();
            BotQuery = GetEntityQuery(typeof(CBotControl));
            IdleBotQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CBotControl)
                    ).None(
                        typeof(CMoveTo),
                        typeof(CGrabAction),
                        typeof(CInteractAction)
                    ));
            PopupQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CLinkedView),
                        typeof(CCaptureInput)
                    ).Any(
                        typeof(LeavePracticeMode.SLeavePracticeView),
                        typeof(CPopup),
                        typeof(SStartDayWarnings),
                        typeof(CUnlockSelectPopup)
                    ));
            UnlockQuery = GetEntityQuery(
                new QueryHelper()
                    .All(
                        typeof(CLinkedView),
                        typeof(CUnlockSelectPopup),
                        typeof(CCapturedUserInput)
                    ));
            input = new FakeInput();
            GenericField = typeof(GenericChoiceView).GetField("Consent", BindingFlags.NonPublic | BindingFlags.Instance);
            EndOfDayField = typeof(EndOfDayPopupView).GetField("Consent", BindingFlags.NonPublic | BindingFlags.Instance);
            StartDayField = typeof(StartDayWarningView).GetField("ConsentElement", BindingFlags.NonPublic | BindingFlags.Instance);
            EndPracticeField = typeof(EndPracticeView).GetField("Consents", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override void OnUpdate()
        {
            if (BotQuery.IsEmptyIgnoreFilter)
                return;

            if (!PopupQuery.IsEmptyIgnoreFilter)
            {
                IObjectView view = null;
                var Popups = PopupQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                foreach (var popup in Popups)
                {
                    // If there are multiple popups find the one that takes precedence
                    view = EntityViewManager.EntityViews[popup.Identifier];
                    if (view is GenericChoiceView || view is EndOfDayPopupView)
                    {
                        break;
                    }
                }
                Popups.Dispose();

                var Bots = BotQuery.ToEntityArray(Allocator.Temp);

                if (view is GenericChoiceView)
                {
                    var Consent = GenericField.GetValue(view) as ConsentElement;

                    foreach (var bot in Bots)
                    {
                        var evt = new InputUpdateEvent();
                        var ID = GetComponent<CPlayer>(bot).ID;
                        evt.User = ID;
                        if (!Consent.GetConsent(ID))
                        {
                            evt.State.MenuSelect = ButtonState.Pressed;
                            input.Send(evt);
                            evt.State.MenuSelect = ButtonState.Released;
                            input.Send(evt);
                        }
                        else
                        {
                            input.Send(evt);
                        }
                    }
                }
                else if (view is EndOfDayPopupView)
                {
                    var Consent = EndOfDayField.GetValue(view) as ConsentElement;

                    foreach (var bot in Bots)
                    {
                        var evt = new InputUpdateEvent();
                        var ID = GetComponent<CPlayer>(bot).ID;
                        evt.User = ID;
                        if (!Consent.GetConsent(ID))
                        {
                            evt.State.MenuSelect = ButtonState.Pressed;
                            input.Send(evt);
                            evt.State.MenuSelect = ButtonState.Released;
                            input.Send(evt);
                        }
                        else
                        {
                            input.Send(evt);
                        }
                    }
                }
                else if (view is StartDayWarningView)
                {
                    var Consent = StartDayField.GetValue(view) as ConsentElement;

                    foreach (var bot in Bots)
                    {
                        var evt = new InputUpdateEvent();
                        var ID = GetComponent<CPlayer>(bot).ID;
                        evt.User = ID;
                        if (!Consent.GetConsent(ID))
                        {
                            evt.State.SecondaryAction1 = ButtonState.Pressed;
                            input.Send(evt);
                            evt.State.SecondaryAction1 = ButtonState.Released;
                            input.Send(evt);
                        }
                        else if (!HasComponent<CMoveTo>(bot) && !HasComponent<CGrabAction>(bot) && !HasComponent<CInteractAction>(bot))
                        {
                            input.Send(evt);
                        }
                    }
                }
                else if (view is EndPracticeView)
                {
                    var Consents = EndPracticeField.GetValue(view) as HashSet<int>;
                    foreach (var bot in Bots)
                    {
                        var evt = new InputUpdateEvent();
                        var ID = GetComponent<CPlayer>(bot).ID;
                        evt.User = ID;
                        if (!Consents.Contains(ID))
                        {
                            evt.State.SecondaryAction1 = ButtonState.Pressed;
                            input.Send(evt);
                            evt.State.SecondaryAction1 = ButtonState.Released;
                            input.Send(evt);
                        }
                        else if (!HasComponent<CMoveTo>(bot) && !HasComponent<CGrabAction>(bot) && !HasComponent<CInteractAction>(bot))
                        {
                            input.Send(evt);
                        }
                    }
                }
                else if (view is UnlockSelectPopupView)
                {
                    var buffer = GetBuffer<CUnlockSelectPopupOption>(UnlockQuery.GetSingletonEntity());
                    var evt = new InputUpdateEvent();

                    if (buffer[0].ID == 0 || buffer[1].ID == 0)
                    {
                        evt.State.MenuLeft = ButtonState.Pressed;
                    }
                    foreach (var bot in Bots)
                    {
                        var ID = GetComponent<CPlayer>(bot).ID;
                        evt.User = ID;
                        input.Send(evt);
                    }
                }
                Bots.Dispose();
            }
            else
            {
                var BotEntities = IdleBotQuery.ToEntityArray(Allocator.Temp);
                foreach (var bot in BotEntities)
                {
                    var evt = new InputUpdateEvent();
                    evt.User = GetComponent<CPlayer>(bot).ID;
                    input.Send(evt);
                }
                BotEntities.Dispose();
            }
        }
    }
}
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
    public class AntiIdleSystem : GenericSystemBase, IModSystem
    {
        private EntityQuery BotQuery, IdleBotQuery, PopupQuery;
        private FakeInput input;

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
                        typeof(SStartDayWarnings)
                    ));
            input = new FakeInput();
        }

        protected override void OnUpdate()
        {
            if (BotQuery.IsEmptyIgnoreFilter)
                return;

            var released = new InputUpdateEvent();
            var pressed = new InputUpdateEvent();
            pressed.State.SecondaryAction1 = ButtonState.Pressed;
            pressed.State.MenuSelect = ButtonState.Pressed;

            if (!PopupQuery.IsEmptyIgnoreFilter)
            {
                IObjectView view = null;
                var Popups = PopupQuery.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                foreach (var popup in Popups)
                {
                    // If there are multiple popups find the one that takes precedence
                    view = EntityViewManager.EntityViews[popup.Identifier];
                    if (Popups.Length == 1 || view is GenericChoiceView || view is EndOfDayPopupView)
                    {
                        break;
                    }
                }
                Popups.Dispose();

                var Bots = BotQuery.ToEntityArray(Allocator.Temp);

                if (view is GenericChoiceView || view is EndOfDayPopupView)
                {
                    var ConsentField = view.GetType().GetField("Consent", BindingFlags.NonPublic | BindingFlags.Instance);
                    var Consent = ConsentField.GetValue(view) as ConsentElement;

                    foreach (var bot in Bots)
                    {
                        var ID = GetComponent<CPlayer>(bot).ID;
                        if (!Consent.GetConsent(ID))
                        {
                            pressed.User = ID;
                            input.Send(pressed);
                        }
                    }
                }
                else if (view is StartDayWarningView)
                {
                    var ConsentField = typeof(StartDayWarningView).GetField("ConsentElement", BindingFlags.NonPublic | BindingFlags.Instance);
                    var Consent = ConsentField.GetValue(view) as ConsentElement;

                    foreach (var bot in Bots)
                    {
                        var ID = GetComponent<CPlayer>(bot).ID;
                        if (!Consent.GetConsent(ID))
                        {
                            pressed.User = ID;
                            input.Send(pressed);
                        }
                    }
                }
                else if (view is EndPracticeView)
                {
                    var ConsentField = typeof(EndPracticeView).GetField("Consents", BindingFlags.NonPublic | BindingFlags.Instance);
                    var Consents = ConsentField.GetValue(view) as HashSet<int>;
                    foreach (var bot in Bots)
                    {
                        var ID = GetComponent<CPlayer>(bot).ID;
                        if (!Consents.Contains(ID))
                        {
                            pressed.User = ID;
                            input.Send(pressed);
                        }
                    }
                }
                Bots.Dispose();
            }
            else
            {
                var BotEntities = IdleBotQuery.ToEntityArray(Allocator.Temp);
                foreach (var bot in BotEntities)
                {
                    released.User = GetComponent<CPlayer>(bot).ID;
                    input.Send(released);
                }
                BotEntities.Dispose();
            }
        }
    }
}
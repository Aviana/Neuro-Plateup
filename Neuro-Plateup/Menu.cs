using UnityEngine;
using Kitchen.Modules;
using Kitchen;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;

namespace Neuro_Plateup {

    public class NeuroPreferencesMenu : Menu<MenuAction> {

        public NeuroPreferencesMenu(Transform container, ModuleList module_list) : base(container, module_list) { }

        public override void Setup(int player_id)
        {
            AddInfoText("Neuro PlateUp! Options");
            AddInfo("These options only work if the game is hosted by yourself!");
            AddInfo("The game adjusts the following options by player count. You can turn these off for the bots so they no longer affect these aspects.");

            New<SpacerElement>();

            AddLabel("Customer Patience");
            var optCustomerPatience = new Option<bool>(new List<bool> { true, false }, NeuroPreferences.GetCustomerPatienceOption(), new List<string> { "True", "False" });
            Add<bool>(optCustomerPatience).OnChanged += delegate (object _, bool value)
            {
                NeuroPreferences.SetCustomerPatienceOption(value);
                BotHandler.RefreshOptions();
            };
            AddLabel("Customer Queue Patience");
            var optQueuePatience = new Option<bool>(new List<bool> { true, false }, NeuroPreferences.GetQueuePatienceOption(), new List<string> { "True", "False" });
            Add<bool>(optQueuePatience).OnChanged += delegate (object _, bool value)
            {
                NeuroPreferences.SetQueuePatienceOption(value);
                BotHandler.RefreshOptions();
            };
            AddLabel("Customer Amount");
            var optCustomerAmount = new Option<bool>(new List<bool> { true, false }, NeuroPreferences.GetCustomerAmountOption(), new List<string> { "True", "False" });
            Add<bool>(optCustomerAmount).OnChanged += delegate (object _, bool value)
            {
                NeuroPreferences.SetCustomerAmountOption(value);
                BotHandler.RefreshOptions();
            };
            New<SpacerElement>();
            New<SpacerElement>();
            AddButton(Localisation["MENU_BACK_SETTINGS"], delegate { RequestPreviousMenu(); });
        }
    }

    [HarmonyPatch(typeof(MainMenu), "Setup")]
    class MainMenu_Patch {

        public static bool Prefix(MainMenu __instance) {
            MethodInfo addSubmenu = __instance.GetType().GetMethod("AddSubmenuButton", BindingFlags.NonPublic | BindingFlags.Instance);
            addSubmenu.Invoke(__instance, new object[] { Main.MOD_NAME, typeof(NeuroPreferencesMenu), false });
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerPauseView), "SetupMenus")]
    class PauseMenu_Patch {

        public static bool Prefix(PlayerPauseView __instance) {
            ModuleList moduleList = (ModuleList)__instance.GetType().GetField("ModuleList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            MethodInfo addMenu = __instance.GetType().GetMethod("AddMenu", BindingFlags.NonPublic | BindingFlags.Instance);
            addMenu.Invoke(__instance, new object[] { typeof(NeuroPreferencesMenu), new NeuroPreferencesMenu(__instance.ButtonContainer, moduleList) });
            return true;
        }
    }
}
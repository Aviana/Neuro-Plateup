using KitchenMods;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Neuro_Plateup {

    public class Main : IModInitializer {

        public const string MOD_ID = "avi.Neuro_PlateUp!";
        public const string MOD_NAME = "Neuro PlateUp!";
        public static readonly string MOD_VERSION = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.ToString();

        public void PostActivate(Mod mod) {
            Debug.Log($"v{MOD_VERSION} initialized");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MOD_ID);
        }

        public void PreInject()
        {
            NeuroPreferences.RegisterPreferences();
        }

        public void PostInject()
        {

        }
    }
}
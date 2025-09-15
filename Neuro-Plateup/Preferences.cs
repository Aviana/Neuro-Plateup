using Kitchen;

namespace Neuro_Plateup
{
    public class NeuroPreferences
    {
        public static readonly Pref CustomerPatience = new Pref(Main.MOD_ID, nameof(CustomerPatience));
        public static readonly Pref QueuePatience = new Pref(Main.MOD_ID, nameof(QueuePatience));
        public static readonly Pref CustomerAmount = new Pref(Main.MOD_ID, nameof(CustomerAmount));

        public static bool preferencesLoaded = false;

        public static void RegisterPreferences()
        {
            if (!preferencesLoaded)
            {
                preferencesLoaded = true;
                Preferences.AddPreference<bool>(new BoolPreference(CustomerPatience, true));
                Preferences.AddPreference<bool>(new BoolPreference(QueuePatience, true));
                Preferences.AddPreference<bool>(new BoolPreference(CustomerAmount, true));
                Preferences.Load();
            }
        }
        
        public static bool GetCustomerPatienceOption() {
            return Preferences.Get<bool>(CustomerPatience);
        }

        public static void SetCustomerPatienceOption(bool value) {
            Preferences.Set(CustomerPatience, value);
        }

        public static bool GetQueuePatienceOption() {
            return Preferences.Get<bool>(QueuePatience);
        }

        public static void SetQueuePatienceOption(bool value) {
            Preferences.Set(QueuePatience, value);
        }

        public static bool GetCustomerAmountOption() {
            return Preferences.Get<bool>(CustomerAmount);
        }

        public static void SetCustomerAmountOption(bool value) {
            Preferences.Set(CustomerAmount, value);
        }
    }
}
using System.Collections.Generic;

namespace Neuro_Plateup
{
    public class ActionStrings
    {
        public string SUCCESS = "";
        public string FAIL = "";
        public string DESC;
    }
    public static class GLOBALSTRINGS
    {
        public static Dictionary<string, ActionStrings> ACTIONS = new Dictionary<string, ActionStrings>
        {
            { "stop", new ActionStrings
                {
                    DESC = "Stop your current activity: {{action}}."
                }
            },
            { "go_to", new ActionStrings
                {
                    DESC = "Go to player."
                }
            },
            { "role_chef", new ActionStrings
                {
                    DESC = "Change your current role to chef."
                }
            },
            { "role_waiter", new ActionStrings
                {
                    DESC = "Change your current role to waiter."
                }
            },
            { "start_game", new ActionStrings
                {
                    DESC = "Move in position to start the game."
                }
            },
            { "rename_restaurant", new ActionStrings
                {
                    DESC = "Give the restaurant a new name by changing the sign on the front."
                }
            },
            { "service", new ActionStrings
                {
                    DESC = "Take food orders from the customers."
                }
            },
            { "extinguish_fires", new ActionStrings
                {
                    DESC = "Extinguish the fire in your kitchen."
                }
            },
            { "clean_mess", new ActionStrings
                {
                    DESC = "Clean up mess on the floor."
                }
            },
            { "serve", new ActionStrings
                {
                    DESC = "Serve food to the customers."
                }
            },
            { "prepare_order", new ActionStrings
                {
                    DESC = "Prepare all items for the customer group with the least patience."
                }
            },
            { "prepare_dishes", new ActionStrings
                {
                    DESC = "Prepare multiple items from the menu. Don't do multiple entries for one dish."
                }
            },
            { "empty_bin", new ActionStrings
                {
                    DESC = "Empty the bin."
                }
            },
            { "wash_plates", new ActionStrings
                {
                    DESC = "Wash the dirty plates."
                }
            },
            { "return_plates", new ActionStrings
                {
                    DESC = "Return dirty plates to the kitchen."
                }
            },
            { "drop_item", new ActionStrings
                {
                    DESC = "Get rid of the currently held item."
                }
            }
        };

        public static readonly Dictionary<int, string> UNLOCKS = new Dictionary<int, string>
        {
            {
                -1086090066, "Gain the 'affordable' theme"
            },
            {
                761584062, "Gain the 'cosy' theme"
            },
            {
                5738470, "Gain the 'exclusive' theme"
            },
            {
                898820513, "Gain the 'formal' theme"
            },
            {
                -1728887993, "Gain 250 coin"
            },
            {
                79859072, "Customers have less service and delivery patience at night"
            },
            {
                -2106646073, "Chance of groups with 25% less service, wait for food and 50% less delivery patience. Increases all queue patience by 25%"
            },
            {
                2028238661, "Couples can now come in groups of 4. Increases all queue patience by 25%"
            },
            {
                587399881, "Chance of groups with 25% less service, wait for food and 50% less delivery patience. They also have randomised thinking and eating times. Increases all queue patience by 25%"
            },
            {
                1958825350, "New card each day, bigger shops, cheaper blueprints. More customers, customers come in rushes"
            },
            {
                1113735761, "Adds cake options as a dessert"
            },
            {
                2055765569, "Increased chance of double orders. Increased side, dessert and starter orders"
            },
            {
                -2112255403, "+1 maximum group size. -1 minimum group size"
            },
            {
                -1183014556, "+2 maximum group size. +1 minimum group size"
            },
            {
                -1857686620, "Focus on serving customers as quickly as possible"
            },
            {
                1293847744, "Focus on making your customers feel welcome with a personal touch"
            },
            {
                -1323758054, "Focus on making customers happy even to get inside your restaurant"
            },
            {
                -1641333859, "Focus on high standards and expect the same from your customers"
            },
            {
                -947047181, "Earn a 2 coin bonus per customer served"
            },
            {
                -1096314451, "Start with 50 coin"
            }
        };
    }
}
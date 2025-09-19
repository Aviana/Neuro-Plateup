using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

namespace Neuro_Plateup
{
    public static class OrderNameRepository
    {
        public static readonly Dictionary<string, ItemInfo> Data = new Dictionary<string, ItemInfo>
        {
            { "Broccoli Cheese Soup", new ItemInfo(1384211889, new FixedListInt64 { 1384211889 }) },
            {"Carrot Soup", new ItemInfo(409276704, new FixedListInt64 { 409276704 }) },
            { "Meat Soup", new ItemInfo(1684936685, new FixedListInt64 { 1684936685 }) },
            { "Pumpkin Soup", new ItemInfo(790436685, new FixedListInt64 { 790436685 }) },
            { "Tomato Soup", new ItemInfo(894680043, new FixedListInt64 { 894680043 }) },
            { "Bread", new ItemInfo(1503471951, new FixedListInt64 { -626784042, -306959510, -306959510 }) },
            { "Christmas Crackers", new ItemInfo(749675166, new FixedListInt64 { 749675166 }) },
            { "2 Mandarin Slices", new ItemInfo(-263257027, new FixedListInt64 { 448483396, 448483396 }) },
            { "4 Mandarin Slices", new ItemInfo(226055037, new FixedListInt64 { 448483396, 448483396, 448483396, 448483396 }) },
            { "Pumpkin Seeds", new ItemInfo(1018675021, new FixedListInt64 { 1018675021 }) },
            { "Bamboo", new ItemInfo(2037858460, new FixedListInt64 { 2037858460 }) },
            { "Broccoli", new ItemInfo(-1520921913, new FixedListInt64 { -1520921913 }) },
            { "Chips", new ItemInfo(-259844528, new FixedListInt64 { -259844528 }) },
            { "Corn on the Cob", new ItemInfo(-1640761177, new FixedListInt64 { -1640761177 }) },
            { "Mashed Potato", new ItemInfo(107345299, new FixedListInt64 { 107345299 }) },
            { "Onion Rings", new ItemInfo(-1086687302, new FixedListInt64 { -1086687302 }) },
            { "Roast Potato", new ItemInfo(-939434748, new FixedListInt64 { -939434748 }) },
            { "Apple Pies", new ItemInfo(82666420, new FixedListInt64 { 82666420 }) },
            { "Cherry Pie", new ItemInfo(1842093636, new FixedListInt64 { 1842093636 }) },
            { "Pumpkin Pies", new ItemInfo(-126602470, new FixedListInt64 { -126602470 }) },
            { "Cheese Board", new ItemInfo(1639948793, new FixedListInt64 { -626784042, 252763172, -755280170, 609827370 }) },

            { "Ice Cream with 3x Vanilla", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 1570518340, 1570518340 }) },
            { "Ice Cream with 3x Chocolate", new ItemInfo(-1307479546, new FixedListInt64 { 502129042, 502129042, 502129042 }) },
            { "Ice Cream with 3x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 186895094, 186895094, 186895094 }) },

            { "Ice Cream with 2x Vanilla 1x Chocolate", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 1570518340, 502129042 }) },
            { "Ice Cream with 2x Vanilla 1x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 1570518340, 186895094 }) },

            { "Ice Cream with 2x Chocolate 1x Vanilla", new ItemInfo(-1307479546, new FixedListInt64 { 502129042, 502129042, 1570518340 }) },
            { "Ice Cream with 2x Chocolate 1x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 502129042, 502129042, 186895094 }) },

            { "Ice Cream with 2x Strawberry 1x Vanilla", new ItemInfo(-1307479546, new FixedListInt64 { 186895094, 186895094, 1570518340 }) },
            { "Ice Cream with 2x Strawberry 1x Chocolate", new ItemInfo(-1307479546, new FixedListInt64 { 186895094, 186895094, 502129042 }) },

            { "Ice Cream with 1x Strawberry 1x Chocolate 1x Vanilla", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 502129042, 186895094 }) },

            { "Ice Cream with 2x Vanilla", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 1570518340 }) },
            { "Ice Cream with 2x Chocolate", new ItemInfo(-1307479546, new FixedListInt64 { 502129042, 502129042 }) },
            { "Ice Cream with 2x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 186895094, 186895094 }) },

            { "Ice Cream with 1x Vanilla 1x Chocolate", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 502129042 }) },
            { "Ice Cream with 1x Vanilla 1x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 1570518340, 186895094 }) },

            { "Ice Cream with 1x Chocolate 1x Strawberry", new ItemInfo(-1307479546, new FixedListInt64 { 502129042, 186895094 }) },

            { "Steak - Rare", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380 }) },
            { "Steak - Medium", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380 }) },
            { "Steak - Well-done", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380 }) },

            { "Steak - Rare with Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333 }) },
            { "Steak - Medium with Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333 }) },
            { "Steak - Well-done with Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333 }) },

            { "Steak - Rare with Tomato", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -853757044 }) },
            { "Steak - Medium with Tomato", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -853757044 }) },
            { "Steak - Well-done with Tomato", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -853757044 }) },

            { "Steak - Rare with Tomato and Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044 }) },
            { "Steak - Medium with Tomato and Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044 }) },
            { "Steak - Well-done with Tomato and Mushroom", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044 }) },

            { "Steak - Rare with Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -285798592 }) },
            { "Steak - Medium with Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -285798592 }) },
            { "Steak - Well-done with Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -285798592 }) },

            { "Steak - Rare with Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333, -285798592 }) },
            { "Steak - Medium with Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333, -285798592 }) },
            { "Steak - Well-done with Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333, -285798592 }) },

            { "Steak - Rare with Tomato and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -853757044, -285798592 }) },
            { "Steak - Medium with Tomato and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -853757044, -285798592 }) },
            { "Steak - Well-done with Tomato and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -853757044, -285798592 }) },

            { "Steak - Rare with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Steak - Medium with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Steak - Well-done with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044, -285798592 }) },

            { "Steak - Rare with Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -1217105161 }) },
            { "Steak - Medium with Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -1217105161 }) },
            { "Steak - Well-done with Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -1217105161 }) },

            { "Steak - Rare with Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333, -1217105161 }) },
            { "Steak - Medium with Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333, -1217105161 }) },
            { "Steak - Well-done with Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333, -1217105161 }) },

            { "Steak - Rare with Tomato and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -853757044, -1217105161 }) },
            { "Steak - Medium with Tomato and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -853757044, -1217105161 }) },
            { "Steak - Well-done with Tomato and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -853757044, -1217105161 }) },

            { "Steak - Rare with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Steak - Medium with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Steak - Well-done with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-1034349623, new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044, -1217105161 }) },

            { "Bone-in Steak - Rare", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380 }) },
            { "Bone-in Steak - Medium", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380 }) },
            { "Bone-in Steak - Well-done", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380 }) },

            { "Bone-in Steak - Rare with Mushroom", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333 }) },
            { "Bone-in Steak - Medium with Mushroom", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333 }) },
            { "Bone-in Steak - Well-done with Mushroom", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333 }) },

            { "Bone-in Steak - Rare with Tomato", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -853757044 }) },
            { "Bone-in Steak - Medium with Tomato", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -853757044 }) },
            { "Bone-in Steak - Well-done with Tomato", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -853757044 }) },

            { "Bone-in Steak - Rare with Tomato and Mushroom", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044 }) },
            { "Bone-in Steak - Medium with Tomato and Mushroom", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044 }) },
            { "Bone-in Steak - Well-done with Tomato and Mushroom", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044 }) },

            { "Bone-in Steak - Rare with Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -285798592 }) },
            { "Bone-in Steak - Medium with Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -285798592 }) },
            { "Bone-in Steak - Well-done with Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -285798592 }) },

            { "Bone-in Steak - Rare with Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333, -285798592 }) },
            { "Bone-in Steak - Medium with Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333, -285798592 }) },
            { "Bone-in Steak - Well-done with Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333, -285798592 }) },

            { "Bone-in Steak - Rare with Tomato and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -853757044, -285798592 }) },
            { "Bone-in Steak - Medium with Tomato and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -853757044, -285798592 }) },
            { "Bone-in Steak - Well-done with Tomato and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -853757044, -285798592 }) },

            { "Bone-in Steak - Rare with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Bone-in Steak - Medium with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Bone-in Steak - Well-done with Tomato, Mushroom and Red Wine Jus", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044, -285798592 }) },

            { "Bone-in Steak - Rare with Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -1217105161 }) },
            { "Bone-in Steak - Medium with Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -1217105161 }) },
            { "Bone-in Steak - Well-done with Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -1217105161 }) },

            { "Bone-in Steak - Rare with Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333, -1217105161 }) },
            { "Bone-in Steak - Medium with Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333, -1217105161 }) },
            { "Bone-in Steak - Well-done with Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333, -1217105161 }) },

            { "Bone-in Steak - Rare with Tomato and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -853757044, -1217105161 }) },
            { "Bone-in Steak - Medium with Tomato and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -853757044, -1217105161 }) },
            { "Bone-in Steak - Well-done with Tomato and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -853757044, -1217105161 }) },

            { "Bone-in Steak - Rare with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Bone-in Steak - Medium with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Bone-in Steak - Well-done with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(-783008587, new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044, -1217105161 }) },

            { "Thick Steak - Rare", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380 }) },
            { "Thick Steak - Medium", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380 }) },
            { "Thick Steak - Well-done", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380 }) },

            { "Thick Steak - Rare with Mushroom", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333 }) },
            { "Thick Steak - Medium with Mushroom", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333 }) },
            { "Thick Steak - Well-done with Mushroom", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333 }) },

            { "Thick Steak - Rare with Tomato", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -853757044 }) },
            { "Thick Steak - Medium with Tomato", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -853757044 }) },
            { "Thick Steak - Well-done with Tomato", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -853757044 }) },

            { "Thick Steak - Rare with Tomato and Mushroom", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044 }) },
            { "Thick Steak - Medium with Tomato and Mushroom", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044 }) },
            { "Thick Steak - Well-done with Tomato and Mushroom", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044 }) },

            { "Thick Steak - Rare with Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -285798592 }) },
            { "Thick Steak - Medium with Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -285798592 }) },
            { "Thick Steak - Well-done with Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -285798592 }) },

            { "Thick Steak - Rare with Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333, -285798592 }) },
            { "Thick Steak - Medium with Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333, -285798592 }) },
            { "Thick Steak - Well-done with Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333, -285798592 }) },

            { "Thick Steak - Rare with Tomato and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -853757044, -285798592 }) },
            { "Thick Steak - Medium with Tomato and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -853757044, -285798592 }) },
            { "Thick Steak - Well-done with Tomato and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -853757044, -285798592 }) },

            { "Thick Steak - Rare with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Thick Steak - Medium with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Thick Steak - Well-done with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044, -285798592 }) },

            { "Thick Steak - Rare with Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -1217105161 }) },
            { "Thick Steak - Medium with Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -1217105161 }) },
            { "Thick Steak - Well-done with Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -1217105161 }) },

            { "Thick Steak - Rare with Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333, -1217105161 }) },
            { "Thick Steak - Medium with Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333, -1217105161 }) },
            { "Thick Steak - Well-done with Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333, -1217105161 }) },

            { "Thick Steak - Rare with Tomato and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -853757044, -1217105161 }) },
            { "Thick Steak - Medium with Tomato and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -853757044, -1217105161 }) },
            { "Thick Steak - Well-done with Tomato and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -853757044, -1217105161 }) },

            { "Thick Steak - Rare with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Thick Steak - Medium with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Thick Steak - Well-done with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1067846341, new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044, -1217105161 }) },

            { "Thin Steak - Rare", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380 }) },
            { "Thin Steak - Medium", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380 }) },
            { "Thin Steak - Well-done", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380 }) },

            { "Thin Steak - Rare with Mushroom", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333 }) },
            { "Thin Steak - Medium with Mushroom", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333 }) },
            { "Thin Steak - Well-done with Mushroom", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333 }) },

            { "Thin Steak - Rare with Tomato", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -853757044 }) },
            { "Thin Steak - Medium with Tomato", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -853757044 }) },
            { "Thin Steak - Well-done with Tomato", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -853757044 }) },

            { "Thin Steak - Rare with Tomato and Mushroom", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044 }) },
            { "Thin Steak - Medium with Tomato and Mushroom", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044 }) },
            { "Thin Steak - Well-done with Tomato and Mushroom", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044 }) },

            { "Thin Steak - Rare with Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -285798592 }) },
            { "Thin Steak - Medium with Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -285798592 }) },
            { "Thin Steak - Well-done with Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -285798592 }) },

            { "Thin Steak - Rare with Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333, -285798592 }) },
            { "Thin Steak - Medium with Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333, -285798592 }) },
            { "Thin Steak - Well-done with Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333, -285798592 }) },

            { "Thin Steak - Rare with Tomato and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1384211889 }) },
            { "Thin Steak - Medium with Tomato and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1384211889 }) },
            { "Thin Steak - Well-done with Tomato and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1384211889 }) },

            { "Thin Steak - Rare with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Thin Steak - Medium with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044, -285798592 }) },
            { "Thin Steak - Well-done with Tomato, Mushroom and Red Wine Jus", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044, -285798592 }) },

            { "Thin Steak - Rare with Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -1217105161 }) },
            { "Thin Steak - Medium with Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -1217105161 }) },
            { "Thin Steak - Well-done with Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -1217105161 }) },

            { "Thin Steak - Rare with Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333, -1217105161 }) },
            { "Thin Steak - Medium with Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333, -1217105161 }) },
            { "Thin Steak - Well-done with Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333, -1217105161 }) },

            { "Thin Steak - Rare with Tomato and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -853757044, -1217105161 }) },
            { "Thin Steak - Medium with Tomato and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -853757044, -1217105161 }) },
            { "Thin Steak - Well-done with Tomato and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -853757044, -1217105161 }) },

            { "Thin Steak - Rare with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Thin Steak - Medium with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044, -1217105161 }) },
            { "Thin Steak - Well-done with Tomato, Mushroom and Mushroom Sauce", new ItemInfo(1173464355, new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044, -1217105161 }) },

            { "Plain Salad", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776 }) },

            { "Salad with Olives", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, 892659864 }) },
            { "Salad with Onion", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, -1252408744 }) },
            { "Salad with Tomato", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, -853757044 }) },

            { "Salad with Olives and Onion", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, 892659864, -1252408744 }) },
            { "Salad with Olives and Tomato", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, 892659864, -853757044 }) },
            { "Salad with Tomato and Onion", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, -853757044, -1252408744 }) },

            { "Salad with Tomato, Onion and Olives", new ItemInfo(-1835015742, new FixedListInt64 { 793377380, -1397390776, -853757044, -1252408744, 892659864 }) },

            { "Apple Salad", new ItemInfo(599544171, new FixedListInt64 { 793377380, -1397390776, 252763172, 564003642, 609827370 }) },

            { "Potato Salad", new ItemInfo(-2053442418, new FixedListInt64 { 793377380, -1252408744, -1399719685, 564003642 }) },

            { "Plated Pizza", new ItemInfo(-1087205958, new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908 }) },
            { "Pizza", new ItemInfo(-1196800934, new FixedListInt64 { -1317168923, -48499881, -369505908 }) },
            { "Plated Mushroom Pizza", new ItemInfo(-1087205958, new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908, -336580972 }) },
            { "Mushroom Pizza", new ItemInfo(-1196800934, new FixedListInt64 { -1317168923, -48499881, -369505908, -336580972 }) },
            { "Plated Onion Pizza", new ItemInfo(-1087205958, new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908, -1633089577 }) },
            { "Onion Pizza", new ItemInfo(-1196800934, new FixedListInt64 { -1317168923, -48499881, -369505908, -1633089577 }) },

            { "Dumplings", new ItemInfo(-1938035042, new FixedListInt64 { 793377380, 1640282430 }) },
            { "Dumplings with Seaweed", new ItemInfo(-1938035042, new FixedListInt64 { 793377380, 1640282430, -1847818036 }) },

            { "Black Coffee", new ItemInfo(-1293050650, new FixedListInt64 { -1293050650 }) },

            { "Iced Coffee", new ItemInfo(-1388933833, new FixedListInt64 { -1293050650, -442824475 }) },

            { "Latte", new ItemInfo(184647209, new FixedListInt64 { -1313420767, -1293050650 }) },

            { "Tea", new ItemInfo(-908710218, new FixedListInt64 { 712770280, 1657174953, 574857689 }) },
            { "Tea Cup", new ItemInfo(-1721929071, new FixedListInt64 { -1721929071 }) },

            { "Affogato", new ItemInfo(-249136431, new FixedListInt64 { -1293050650, 1570518340 }) },

            { "Cake Stand", new ItemInfo(41735497, new FixedListInt64 { 41735497 }) },

            { "Burgers", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830 }) },

            { "Burgers with Tomato", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, -853757044 }) },
            { "Burgers with Onion", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, -1252408744 }) },
            { "Cheeseburgers", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100 }) },

            { "Cheeseburgers with Tomato", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -853757044 }) },
            { "Cheeseburgers with Onion", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -1252408744 }) },

            { "Cheeseburgers with Tomato and Onion", new ItemInfo(-884392267, new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -853757044, -1252408744 }) },

            { "Turkey", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716 }) },

            { "Turkey with Stuffing", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, -352397598 }) },
            { "Turkey with Gravy", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, 1168127977 }) },
            { "Turkey with Cranberry Sauce", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, -1788071646 }) },

            { "Turkey with Stuffing and Gravy", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, -352397598, 1168127977 }) },
            { "Turkey with Stuffing and Cranberry Sauce", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, -352397598, -1788071646 }) },
            { "Turkey with Gravy and Cranberry Sauce", new ItemInfo(1792757441, new FixedListInt64 { 793377380, -914826716, 1168127977, -1788071646 }) },

            { "Nut Roast", new ItemInfo(-1934880099, new FixedListInt64 { 793377380, -1294491269 }) },

            { "Meat Pie", new ItemInfo(861630222, new FixedListInt64 { 793377380, 1030798878 }) },

            { "Mushroom Pie", new ItemInfo(861630222, new FixedListInt64 { 793377380, 280553412 }) },

            { "Vegetable Pie", new ItemInfo(861630222, new FixedListInt64 { 793377380, -1612932608 }) },

            { "Chocolate Cake", new ItemInfo(-1303191076, new FixedListInt64 { -1303191076 }) },
            { "Lemon Cake", new ItemInfo(51761947, new FixedListInt64 { 51761947 }) },
            { "Coffee Cake", new ItemInfo(-19982058, new FixedListInt64 { -19982058 }) },

            { "Spaghetti", new ItemInfo(1900532137, new FixedListInt64 { 793377380, -1640088321, -1317168923 }) },

            { "Spaghetti Bolognese", new ItemInfo(-1711635749, new FixedListInt64 { 793377380, -1640088321, 778957913 }) },

            { "Cheesy Spaghetti", new ItemInfo(-383718493, new FixedListInt64 { 793377380, -1640088321, 1972097789, 263830100 }) },

            { "Lasagne", new ItemInfo(82891941, new FixedListInt64 { 793377380, 1385009029 }) },

            { "Blue Fish", new ItemInfo(536781335, new FixedListInt64 { 793377380, 454058921 }) },
            { "Pink Fish", new ItemInfo(-1608542149, new FixedListInt64 { 793377380, 411057095 }) },

            { "Crab Cake", new ItemInfo(1939124686, new FixedListInt64 { 793377380, -2007852530 }) },

            { "Fish Fillet", new ItemInfo(1011454010, new FixedListInt64 { 793377380, -505249062 }) },

            { "2 Oysters", new ItemInfo(403539963, new FixedListInt64 { 793377380, -920494794, -920494794 }) },
            { "3 Oysters", new ItemInfo(403539963, new FixedListInt64 { 793377380, -920494794, -920494794, -920494794 }) },

            { "Spiny Fish", new ItemInfo(-491640227, new FixedListInt64 { 793377380, 1247388187 }) },

            { "Tacos", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183 }) },

            { "Tacos with Cheese", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100 }) },
            { "Tacos with Onion", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1252408744 }) },
            { "Tacos with Lettuce", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1397390776 }) },
            { "Tacos with Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -853757044 }) },

            { "Tacos with Cheese and Onion", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744 }) },
            { "Tacos with Cheese and Lettuce", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100, -1397390776 }) },
            { "Tacos with Cheese and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100, -853757044 }) },

            { "Tacos with Onion and Lettuce", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1252408744, -1397390776 }) },
            { "Tacos with Onion and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1252408744, -853757044 }) },

            { "Tacos with Lettuce and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044 }) },

            { "Tacos with Cheese, Onion and Lettuce", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -1397390776 }) },
            { "Tacos with Cheese, Onion and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -853757044 }) },
            { "Tacos with Lettuce, Onion and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1397390776, -1252408744, -853757044 }) },
            { "Tacos with Lettuce, Cheese and Tomato", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1397390776, 263830100, -853757044 }) },

            { "Tacos with Lettuce, Tomato, Onion and Cheese", new ItemInfo(244927287, new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100 }) },

            { "Hot Dog", new ItemInfo(1702578261, new FixedListInt64 { 793377380, 756326364, -248200024 }) },
            { "Ketchup", new ItemInfo(-1075930689, new FixedListInt64 { -1075930689 }) },
            { "Mustard", new ItemInfo(-1114203942, new FixedListInt64 { -1114203942 }) },

            { "Toast", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718 }) },

            { "Beans on Toast", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -2138118944 }) },
            { "Egg on Toast", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001 }) },
            { "Toast with Tomato", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -853757044 }) },
            { "Toast with Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -2093899333 }) },

            { "Egg on Toast with Beans", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944 }) },
            { "Egg on Toast with Tomato", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -853757044 }) },
            { "Egg on Toast with Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -2093899333 }) },
            { "Beans on Toast with Tomato", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -2138118944, -853757044 }) },
            { "Beans on Toast with Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -2138118944, -2093899333 }) },
            { "Toast with Tomato and Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -853757044, -2093899333 }) },

            { "Egg on Toast with Beans and Tomato", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944, -853757044 }) },
            { "Egg on Toast with Beans and Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944, -2093899333 }) },
            { "Beans on Toast with Tomato and Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, -2138118944, -853757044, -2093899333 }) },
            { "Egg on Toast with Tomato and Mushroom", new ItemInfo(1384211889, new FixedListInt64 { 793377380, 428559718, 1324261001, -853757044, -2093899333 }) },

            { "Stir Fry with Carrot", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1406021079 }) },
            { "Stir Fry with Meat", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1018018897 }) },
            { "Stir Fry with Broccoli", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, 1453647256 }) },
            { "Stir Fry with Bamboo", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, 880804869 }) },
            { "Stir Fry with Mushroom", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -336580972 }) },

            { "Stir Fry with Carrot and Meat", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1406021079 }) },
            { "Stir Fry with Carrot and Broccoli", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1406021079 }) },
            { "Stir Fry with Carrot and Bamboo", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1406021079 }) },
            { "Stir Fry with Carrot and Mushroom", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1406021079 }) },

            { "Stir Fry with Meat and Broccoli", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1018018897 }) },
            { "Stir Fry with Meat and Bamboo", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1018018897 }) },
            { "Stir Fry with Meat and Mushroom", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, -1018018897 }) },

            { "Stir Fry with Broccoli and Bamboo", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, 1453647256 }) },
            { "Stir Fry with Broccoli and Mushroom", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, 1453647256 }) },

            { "Stir Fry with Bamboo and Mushroom", new ItemInfo(-361808208, new FixedListInt64 { 793377380, 1928939081, 880804869 }) },

            { "Soy Sauce", new ItemInfo(1190974918, new FixedListInt64 { 1190974918 }) },
        };

        public static bool TryGetValues(string name, out int id, out FixedListInt64 items)
        {
            bool flag = Data.TryGetValue(name, out var value);
            id = value.ID;
            items = value.Items;
            return flag;
        }

        public static bool TryGetValues(FixedListInt64 items, out int id, out string name)
        {
            var match = Data.FirstOrDefault(kvp => kvp.Value.Items.Equals(items));

            if (!string.IsNullOrEmpty(match.Key))
            {
                id = match.Value.ID;
                name = match.Key;
                return true;
            }

            id = default;
            name = default;
            return false;
        }
    }
}

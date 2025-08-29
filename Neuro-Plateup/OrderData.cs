using System;
using System.Collections.Generic;
using System.Linq;
using KitchenData;
using Unity.Collections;

namespace Neuro_Plateup
{
    public class OrderNameRepository
    {
        private sealed class IntMultisetKey : IEquatable<IntMultisetKey>
        {
            private readonly int[] normalizedValues;
            private readonly int hashCode;

            public IntMultisetKey(IEnumerable<int> values)
            {
                normalizedValues = values.OrderBy(v => v).ToArray();
                hashCode = ComputeHash(normalizedValues);
            }

            private int ComputeHash(int[] values)
            {
                unchecked
                {
                    int hash = 19;
                    foreach (int v in values)
                        hash = hash * 31 + v;
                    return hash;
                }
            }

            public override bool Equals(object obj) => Equals(obj as IntMultisetKey);

            public bool Equals(IntMultisetKey other)
            {
                if (other == null) return false;
                return normalizedValues.SequenceEqual(other.normalizedValues);
            }

            public override int GetHashCode() => hashCode;
        }

        private static readonly Dictionary<IntMultisetKey, string> forward = new Dictionary<IntMultisetKey, string>();
        private static readonly Dictionary<string, ItemList> reverse = new Dictionary<string, ItemList>();

        private static void Add(FixedListInt64 keyValues, string value)
        {
            var keyArray = keyValues.ToArray();
            var key = new IntMultisetKey(keyArray);

            forward[key] = value;
            reverse[value] = new ItemList(keyValues);
        }

        public static bool TryGetName(IEnumerable<int> keyValues, out string value)
        {
            return forward.TryGetValue(new IntMultisetKey(keyValues), out value);
        }

        public static bool TryGetValues(string value, out ItemList keyValues)
        {
            if (reverse.TryGetValue(value, out keyValues))
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<KeyValuePair<string, ItemList>> AllEntries => reverse;

        static OrderNameRepository()
        {
            // Broccoli Cheese Soup
            Add(new FixedListInt64 { 1384211889 }, "Broccoli Cheese Soup");
            // Carrot Soup
            Add(new FixedListInt64 { 409276704 }, "Carrot Soup");
            // Meat Soup
            Add(new FixedListInt64 { 1684936685 }, "Meat Soup");
            // Pumpkin Soup
            Add(new FixedListInt64 { 790436685 }, "Pumpkin Soup");
            // Tomato Soup
            Add(new FixedListInt64 { 894680043 }, "Tomato Soup");
            // Bread
            Add(new FixedListInt64 { -626784042, -306959510, -306959510 }, "Bread");
            // Christmas Crackers
            Add(new FixedListInt64 { 749675166 }, "Christmas Crackers");
            // Mandarin Starter
            Add(new FixedListInt64 { 448483396, 448483396 }, "2 Mandarin Slices");
            Add(new FixedListInt64 { 448483396, 448483396, 448483396, 448483396 }, "4 Mandarin Slices");
            // Pumpkin Seeds
            Add(new FixedListInt64 { 1018675021 }, "Pumpkin Seeds");
            // Bamboo
            Add(new FixedListInt64 { 880804869 }, "Bamboo");
            // Broccoli
            Add(new FixedListInt64 { -1520921913 }, "Broccoli");
            // Chips
            Add(new FixedListInt64 { -259844528 }, "Chips");
            // Corn on the Cob
            Add(new FixedListInt64 { -1640761177 }, "Corn on the Cob");
            // Mashed Potato
            Add(new FixedListInt64 { 107345299 }, "Mashed Potato");
            // Onion Rings
            Add(new FixedListInt64 { -1086687302 }, "Onion Rings");
            // Roast Potato
            Add(new FixedListInt64 { -939434748 }, "Roast Potato");
            // Apple Pies
            Add(new FixedListInt64 { 82666420 }, "Apple Pies");
            // Cherry Pie
            Add(new FixedListInt64 { 1842093636 }, "Cherry Pie");
            // Pumpkin Pies
            Add(new FixedListInt64 { -126602470 }, "Pumpkin Pies");
            // Cheese Board
            Add(new FixedListInt64 { -626784042, 252763172, -755280170, 609827370 }, "Cheese Board");
            // Ice Cream
            Add(new FixedListInt64 { 1570518340, 1570518340, 1570518340 }, "Ice Cream with 3x Vanilla");
            Add(new FixedListInt64 { 502129042, 502129042, 502129042 }, "Ice Cream with 3x Chocolate");
            Add(new FixedListInt64 { 186895094, 186895094, 186895094 }, "Ice Cream with 3x Strawberry");

            Add(new FixedListInt64 { 1570518340, 1570518340, 502129042 }, "Ice Cream with 2x Vanilla 1x Chocolate");
            Add(new FixedListInt64 { 1570518340, 1570518340, 186895094 }, "Ice Cream with 2x Vanilla 1x Strawberry");

            Add(new FixedListInt64 { 502129042, 502129042, 1570518340 }, "Ice Cream with 2x Chocolate 1x Vanilla");
            Add(new FixedListInt64 { 502129042, 502129042, 186895094 }, "Ice Cream with 2x Chocolate 1x Strawberry");

            Add(new FixedListInt64 { 186895094, 186895094, 1570518340 }, "Ice Cream with 2x Strawberry 1x Vanilla");
            Add(new FixedListInt64 { 186895094, 186895094, 502129042 }, "Ice Cream with 2x Strawberry 1x Chocolate");

            Add(new FixedListInt64 { 1570518340, 502129042, 186895094 }, "Ice Cream with 1x Strawberry 1x Chocolate 1x Vanilla");

            Add(new FixedListInt64 { 1570518340, 1570518340 }, "Ice Cream with 2x Vanilla");
            Add(new FixedListInt64 { 502129042, 502129042 }, "Ice Cream with 2x Chocolate");
            Add(new FixedListInt64 { 186895094, 186895094 }, "Ice Cream with 2x Strawberry");

            Add(new FixedListInt64 { 1570518340, 502129042 }, "Ice Cream with 1x Vanilla 1x Chocolate");
            Add(new FixedListInt64 { 1570518340, 186895094 }, "Ice Cream with 1x Vanilla 1x Strawberry");

            Add(new FixedListInt64 { 502129042, 186895094 }, "Ice Cream with 1x Chocolate 1x Strawberry");
            // Steak
            Add(new FixedListInt64 { 1936140106, 793377380 }, "Steak - Rare");
            Add(new FixedListInt64 { 744193417, 793377380 }, "Steak - Medium");
            Add(new FixedListInt64 { -1631681807, 793377380 }, "Steak - Well-done");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333 }, "Steak - Rare with Mushroom");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333 }, "Steak - Medium with Mushroom");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333 }, "Steak - Well-done with Mushroom");

            Add(new FixedListInt64 { 1936140106, 793377380, -853757044 }, "Steak - Rare with Tomato");
            Add(new FixedListInt64 { 744193417, 793377380, -853757044 }, "Steak - Medium with Tomato");
            Add(new FixedListInt64 { -1631681807, 793377380, -853757044 }, "Steak - Well-done with Tomato");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044 }, "Steak - Rare with Tomato and Mushroom");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044 }, "Steak - Medium with Tomato and Mushroom");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044 }, "Steak - Well-done with Tomato and Mushroom");

            Add(new FixedListInt64 { 1936140106, 793377380, -285798592 }, "Steak - Rare with Red Wine Jus");
            Add(new FixedListInt64 { 744193417, 793377380, -285798592 }, "Steak - Medium with Red Wine Jus");
            Add(new FixedListInt64 { -1631681807, 793377380, -285798592 }, "Steak - Well-done with Red Wine Jus");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333, -285798592 }, "Steak - Rare with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333, -285798592 }, "Steak - Medium with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333, -285798592 }, "Steak - Well-done with Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { 1936140106, 793377380, -853757044, -285798592 }, "Steak - Rare with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { 744193417, 793377380, -853757044, -285798592 }, "Steak - Medium with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { -1631681807, 793377380, -853757044, -285798592 }, "Steak - Well-done with Tomato and Red Wine Jus");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Rare with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Medium with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Well-done with Tomato, Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { 1936140106, 793377380, -285798592 }, "Steak - Rare with Mushroom Sauce");
            Add(new FixedListInt64 { 744193417, 793377380, -285798592 }, "Steak - Medium with Mushroom Sauce");
            Add(new FixedListInt64 { -1631681807, 793377380, -285798592 }, "Steak - Well-done with Mushroom Sauce");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333, -285798592 }, "Steak - Rare with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333, -285798592 }, "Steak - Medium with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333, -285798592 }, "Steak - Well-done with Mushroom and Mushroom Sauce");

            Add(new FixedListInt64 { 1936140106, 793377380, -853757044, -285798592 }, "Steak - Rare with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { 744193417, 793377380, -853757044, -285798592 }, "Steak - Medium with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { -1631681807, 793377380, -853757044, -285798592 }, "Steak - Well-done with Tomato and Mushroom Sauce");

            Add(new FixedListInt64 { 1936140106, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Rare with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 744193417, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Medium with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -1631681807, 793377380, -2093899333, -853757044, -285798592 }, "Steak - Well-done with Tomato, Mushroom and Mushroom Sauce");
            // Bone-in Steak
            Add(new FixedListInt64 { -260257840, 793377380 }, "Bone-in Steak - Rare");
            Add(new FixedListInt64 { 418682003, 793377380 }, "Bone-in Steak - Medium");
            Add(new FixedListInt64 { 153969149, 793377380 }, "Bone-in Steak - Well-done");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333 }, "Bone-in Steak - Rare with Mushroom");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333 }, "Bone-in Steak - Medium with Mushroom");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333 }, "Bone-in Steak - Well-done with Mushroom");

            Add(new FixedListInt64 { -260257840, 793377380, -853757044 }, "Bone-in Steak - Rare with Tomato");
            Add(new FixedListInt64 { 418682003, 793377380, -853757044 }, "Bone-in Steak - Medium with Tomato");
            Add(new FixedListInt64 { 153969149, 793377380, -853757044 }, "Bone-in Steak - Well-done with Tomato");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044 }, "Bone-in Steak - Rare with Tomato and Mushroom");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044 }, "Bone-in Steak - Medium with Tomato and Mushroom");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044 }, "Bone-in Steak - Well-done with Tomato and Mushroom");

            Add(new FixedListInt64 { -260257840, 793377380, -285798592 }, "Bone-in Steak - Rare with Red Wine Jus");
            Add(new FixedListInt64 { 418682003, 793377380, -285798592 }, "Bone-in Steak - Medium with Red Wine Jus");
            Add(new FixedListInt64 { 153969149, 793377380, -285798592 }, "Bone-in Steak - Well-done with Red Wine Jus");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Rare with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Medium with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Well-done with Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -260257840, 793377380, -853757044, -285798592 }, "Bone-in Steak - Rare with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { 418682003, 793377380, -853757044, -285798592 }, "Bone-in Steak - Medium with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { 153969149, 793377380, -853757044, -285798592 }, "Bone-in Steak - Well-done with Tomato and Red Wine Jus");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Rare with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Medium with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Well-done with Tomato, Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -260257840, 793377380, -285798592 }, "Bone-in Steak - Rare with Mushroom Sauce");
            Add(new FixedListInt64 { 418682003, 793377380, -285798592 }, "Bone-in Steak - Medium with Mushroom Sauce");
            Add(new FixedListInt64 { 153969149, 793377380, -285798592 }, "Bone-in Steak - Well-done with Mushroom Sauce");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Rare with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Medium with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333, -285798592 }, "Bone-in Steak - Well-done with Mushroom and Mushroom Sauce");

            Add(new FixedListInt64 { -260257840, 793377380, -853757044, -285798592 }, "Bone-in Steak - Rare with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { 418682003, 793377380, -853757044, -285798592 }, "Bone-in Steak - Medium with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { 153969149, 793377380, -853757044, -285798592 }, "Bone-in Steak - Well-done with Tomato and Mushroom Sauce");

            Add(new FixedListInt64 { -260257840, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Rare with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 418682003, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Medium with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 153969149, 793377380, -2093899333, -853757044, -285798592 }, "Bone-in Steak - Well-done with Tomato, Mushroom and Mushroom Sauce");
            // Thick Steak
            Add(new FixedListInt64 { -510353055, 793377380 }, "Thick Steak - Rare");
            Add(new FixedListInt64 { -283606362, 793377380 }, "Thick Steak - Medium");
            Add(new FixedListInt64 { 623804310, 793377380 }, "Thick Steak - Well-done");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333 }, "Thick Steak - Rare with Mushroom");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333 }, "Thick Steak - Medium with Mushroom");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333 }, "Thick Steak - Well-done with Mushroom");

            Add(new FixedListInt64 { -510353055, 793377380, -853757044 }, "Thick Steak - Rare with Tomato");
            Add(new FixedListInt64 { -283606362, 793377380, -853757044 }, "Thick Steak - Medium with Tomato");
            Add(new FixedListInt64 { 623804310, 793377380, -853757044 }, "Thick Steak - Well-done with Tomato");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044 }, "Thick Steak - Rare with Tomato and Mushroom");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044 }, "Thick Steak - Medium with Tomato and Mushroom");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044 }, "Thick Steak - Well-done with Tomato and Mushroom");

            Add(new FixedListInt64 { -510353055, 793377380, -285798592 }, "Thick Steak - Rare with Red Wine Jus");
            Add(new FixedListInt64 { -283606362, 793377380, -285798592 }, "Thick Steak - Medium with Red Wine Jus");
            Add(new FixedListInt64 { 623804310, 793377380, -285798592 }, "Thick Steak - Well-done with Red Wine Jus");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333, -285798592 }, "Thick Steak - Rare with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333, -285798592 }, "Thick Steak - Medium with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333, -285798592 }, "Thick Steak - Well-done with Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -510353055, 793377380, -853757044, -285798592 }, "Thick Steak - Rare with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { -283606362, 793377380, -853757044, -285798592 }, "Thick Steak - Medium with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { 623804310, 793377380, -853757044, -285798592 }, "Thick Steak - Well-done with Tomato and Red Wine Jus");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Rare with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Medium with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Well-done with Tomato, Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -510353055, 793377380, -285798592 }, "Thick Steak - Rare with Mushroom Sauce");
            Add(new FixedListInt64 { -283606362, 793377380, -285798592 }, "Thick Steak - Medium with Mushroom Sauce");
            Add(new FixedListInt64 { 623804310, 793377380, -285798592 }, "Thick Steak - Well-done with Mushroom Sauce");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333, -285798592 }, "Thick Steak - Rare with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333, -285798592 }, "Thick Steak - Medium with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333, -285798592 }, "Thick Steak - Well-done with Mushroom and Mushroom Sauce");

            Add(new FixedListInt64 { -510353055, 793377380, -853757044, -285798592 }, "Thick Steak - Rare with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { -283606362, 793377380, -853757044, -285798592 }, "Thick Steak - Medium with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { 623804310, 793377380, -853757044, -285798592 }, "Thick Steak - Well-done with Tomato and Mushroom Sauce");

            Add(new FixedListInt64 { -510353055, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Rare with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -283606362, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Medium with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 623804310, 793377380, -2093899333, -853757044, -285798592 }, "Thick Steak - Well-done with Tomato, Mushroom and Mushroom Sauce");
            // Thin Steak
            Add(new FixedListInt64 { -1720486713, 793377380 }, "Thin Steak - Rare");
            Add(new FixedListInt64 { 1645212811, 793377380 }, "Thin Steak - Medium");
            Add(new FixedListInt64 { -989359657, 793377380 }, "Thin Steak - Well-done");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333 }, "Thin Steak - Rare with Mushroom");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333 }, "Thin Steak - Medium with Mushroom");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333 }, "Thin Steak - Well-done with Mushroom");

            Add(new FixedListInt64 { -1720486713, 793377380, -853757044 }, "Thin Steak - Rare with Tomato");
            Add(new FixedListInt64 { 1645212811, 793377380, -853757044 }, "Thin Steak - Medium with Tomato");
            Add(new FixedListInt64 { -989359657, 793377380, -853757044 }, "Thin Steak - Well-done with Tomato");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044 }, "Thin Steak - Rare with Tomato and Mushroom");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044 }, "Thin Steak - Medium with Tomato and Mushroom");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044 }, "Thin Steak - Well-done with Tomato and Mushroom");

            Add(new FixedListInt64 { -1720486713, 793377380, -285798592 }, "Thin Steak - Rare with Red Wine Jus");
            Add(new FixedListInt64 { 1645212811, 793377380, -285798592 }, "Thin Steak - Medium with Red Wine Jus");
            Add(new FixedListInt64 { -989359657, 793377380, -285798592 }, "Thin Steak - Well-done with Red Wine Jus");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333, -285798592 }, "Thin Steak - Rare with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333, -285798592 }, "Thin Steak - Medium with Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333, -285798592 }, "Thin Steak - Well-done with Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -1720486713, 793377380, -853757044, -285798592 }, "Thin Steak - Rare with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { 1645212811, 793377380, -853757044, -285798592 }, "Thin Steak - Medium with Tomato and Red Wine Jus");
            Add(new FixedListInt64 { -989359657, 793377380, -853757044, -285798592 }, "Thin Steak - Well-done with Tomato and Red Wine Jus");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Rare with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Medium with Tomato, Mushroom and Red Wine Jus");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Well-done with Tomato, Mushroom and Red Wine Jus");

            Add(new FixedListInt64 { -1720486713, 793377380, -285798592 }, "Thin Steak - Rare with Mushroom Sauce");
            Add(new FixedListInt64 { 1645212811, 793377380, -285798592 }, "Thin Steak - Medium with Mushroom Sauce");
            Add(new FixedListInt64 { -989359657, 793377380, -285798592 }, "Thin Steak - Well-done with Mushroom Sauce");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333, -285798592 }, "Thin Steak - Rare with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333, -285798592 }, "Thin Steak - Medium with Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333, -285798592 }, "Thin Steak - Well-done with Mushroom and Mushroom Sauce");

            Add(new FixedListInt64 { -1720486713, 793377380, -853757044, -285798592 }, "Thin Steak - Rare with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { 1645212811, 793377380, -853757044, -285798592 }, "Thin Steak - Medium with Tomato and Mushroom Sauce");
            Add(new FixedListInt64 { -989359657, 793377380, -853757044, -285798592 }, "Thin Steak - Well-done with Tomato and Mushroom Sauce");

            Add(new FixedListInt64 { -1720486713, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Rare with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { 1645212811, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Medium with Tomato, Mushroom and Mushroom Sauce");
            Add(new FixedListInt64 { -989359657, 793377380, -2093899333, -853757044, -285798592 }, "Thin Steak - Well-done with Tomato, Mushroom and Mushroom Sauce");
            // Salad
            Add(new FixedListInt64 { 793377380, -1397390776 }, "Salad");

            Add(new FixedListInt64 { 793377380, -1397390776, 892659864 }, "Salad with Olives");
            Add(new FixedListInt64 { 793377380, -1397390776, -1252408744 }, "Salad with Onion");
            Add(new FixedListInt64 { 793377380, -1397390776, -853757044 }, "Salad with Tomato");

            Add(new FixedListInt64 { 793377380, -1397390776, 892659864, -1252408744 }, "Salad with Olives and Onion");
            Add(new FixedListInt64 { 793377380, -1397390776, 892659864, -853757044 }, "Salad with Olives and Tomato");
            Add(new FixedListInt64 { 793377380, -1397390776, -853757044, -1252408744 }, "Salad with Tomato and Onion");

            Add(new FixedListInt64 { 793377380, -1397390776, -853757044, -1252408744, 892659864 }, "Salad with Tomato, Onion and Olives");
            // Apple Salad
            Add(new FixedListInt64 { 793377380, -1397390776, 252763172, 564003642, 609827370 }, "Apple Salad");
            // Potato Salad
            Add(new FixedListInt64 { 793377380, -1252408744, -1399719685, 564003642 }, "Potato Salad");
            // Pizza
            Add(new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908 }, "Pizza");
            Add(new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908, -336580972 }, "Mushroom Pizza");
            Add(new FixedListInt64 { 793377380, -1317168923, -48499881, -369505908, -1633089577 }, "Onion Pizza");
            // Dumplings
            Add(new FixedListInt64 { 793377380, 1640282430 }, "Dumplings");
            Add(new FixedListInt64 { 793377380, 1640282430, -1847818036 }, "Dumplings with Seaweed");
            // Black Coffee
            Add(new FixedListInt64 { -1293050650 }, "Black Coffee");
            // Iced Coffee
            Add(new FixedListInt64 { -1293050650, -442824475 }, "Iced Coffee");
            // Latte
            Add(new FixedListInt64 { -1313420767, -1293050650 }, "Latte");
            // Tea
            Add(new FixedListInt64 { 712770280, 1657174953, 574857689, -1721929071 }, "Tea"); // NYI: SPECIAL CASE - All will order a pot(712770280, 1657174953, 574857689) but you need only 1 but cups for every one
            // Affogato
            Add(new FixedListInt64 { -1293050650, 1570518340 }, "Affogato");
            // Burgers
            Add(new FixedListInt64 { 793377380, -1756808590, 687585830 }, "Burgers");

            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, -853757044 }, "Burgers with Tomato");
            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, -1252408744 }, "Burgers with Onion");
            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100 }, "Cheeseburgers");

            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -853757044 }, "Cheeseburgers with Tomato");
            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -1252408744 }, "Cheeseburgers with Onion");

            Add(new FixedListInt64 { 793377380, -1756808590, 687585830, 263830100, -853757044, -1252408744 }, "Cheeseburgers with Tomato and Onion");
            // Turkey
            Add(new FixedListInt64 { 793377380, -914826716 }, "Turkey");

            Add(new FixedListInt64 { 793377380, -914826716, -352397598 }, "Turkey with Stuffing");
            Add(new FixedListInt64 { 793377380, -914826716, 1168127977 }, "Turkey with Gravy");
            Add(new FixedListInt64 { 793377380, -914826716, -1788071646 }, "Turkey with Cranberry Sauce");

            Add(new FixedListInt64 { 793377380, -914826716, -352397598, 1168127977 }, "Turkey with Stuffing and Gravy");
            Add(new FixedListInt64 { 793377380, -914826716, -352397598, -1788071646 }, "Turkey with Stuffing and Cranberry Sauce");
            Add(new FixedListInt64 { 793377380, -914826716, 1168127977, -1788071646 }, "Turkey with Gravy and Cranberry Sauce");
            // Nut Roast
            Add(new FixedListInt64 { 793377380, -1294491269 }, "Nut Roast");
            // Pies
            Add(new FixedListInt64 { 793377380, 1030798878 }, "Meat Pie");
            // Mushroom Pie
            Add(new FixedListInt64 { 793377380, 280553412 }, "Mushroom Pie");
            // Vegetable Pies
            Add(new FixedListInt64 { 793377380, -1612932608 }, "Vegetable Pie");
            // Cakes
            Add(new FixedListInt64 { -1303191076 }, "Chocolate Cake");
            Add(new FixedListInt64 { 51761947 }, "Lemon Cake");
            Add(new FixedListInt64 { -19982058 }, "Coffee Cake");
            // Spaghetti
            Add(new FixedListInt64 { 793377380, -1640088321, -1317168923 }, "Spaghetti");
            // Spaghetti Bolognese
            Add(new FixedListInt64 { 793377380, -1640088321, 778957913 }, "Spaghetti Bolognese");
            // Cheesy Spaghetti
            Add(new FixedListInt64 { 793377380, -1640088321, 1972097789, 263830100 }, "Cheesy Spaghetti");
            // Lasagne
            Add(new FixedListInt64 { 793377380, 1385009029 }, "Lasagne");
            // Fish
            Add(new FixedListInt64 { 793377380, 454058921 }, "Blue Fish");
            Add(new FixedListInt64 { 793377380, 411057095 }, "Pink Fish");
            // Crab Cake
            Add(new FixedListInt64 { 793377380, -2007852530 }, "Crab Cake");
            // Fish Fillet
            Add(new FixedListInt64 { 793377380, -505249062 }, "Fish Fillet");
            // Oysters
            Add(new FixedListInt64 { 793377380, -920494794, -920494794 }, "2 Oysters");
            Add(new FixedListInt64 { 793377380, -920494794, -920494794, -920494794 }, "3 Oysters");
            // Spiny Fish
            Add(new FixedListInt64 { 793377380, 1247388187 }, "Spiny Fish");
            // Tacos
            Add(new FixedListInt64 { 111245472, 177461183, 111245472, 177461183 }, "2 Tacos");
            Add(new FixedListInt64 { 111245472, 177461183, 111245472, 177461183, 111245472, 177461183 }, "3 Tacos");

            Add(new FixedListInt64 { 111245472, 177461183, 263830100, 111245472, 177461183, 263830100 }, "2 Tacos with Cheese");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, 111245472, 177461183, 263830100, 111245472, 177461183, 263830100 }, "3 Tacos with Cheese");
            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, 111245472, 177461183, -1252408744 }, "2 Tacos with Onion");
            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, 111245472, 177461183, -1252408744, 111245472, 177461183, -1252408744 }, "3 Tacos with Onion");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, 111245472, 177461183, -1397390776 }, "2 Tacos with Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, 111245472, 177461183, -1397390776, 111245472, 177461183, -1397390776 }, "3 Tacos with Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, -853757044, 111245472, 177461183, -853757044 }, "2 Tacos with Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -853757044, 111245472, 177461183, -853757044, 111245472, 177461183, -853757044 }, "3 Tacos with Tomato");

            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, 111245472, 177461183, 263830100, -1252408744 }, "2 Tacos with Cheese and Onion");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, 111245472, 177461183, 263830100, -1252408744, 111245472, 177461183, 263830100, -1252408744 }, "3 Tacos with Cheese and Onion");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1397390776, 111245472, 177461183, 263830100, -1397390776 }, "2 Tacos with Cheese and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1397390776, 111245472, 177461183, 263830100, -1397390776, 111245472, 177461183, 263830100, -1397390776 }, "3 Tacos with Cheese and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -853757044, 111245472, 177461183, 263830100, -853757044 }, "2 Tacos with Cheese and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -853757044, 111245472, 177461183, 263830100, -853757044, 111245472, 177461183, 263830100, -853757044 }, "3 Tacos with Cheese and Tomato");

            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, -1397390776, 111245472, 177461183, -1252408744, -1397390776 }, "2 Tacos with Onion and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, -1397390776, 111245472, 177461183, -1252408744, -1397390776, 111245472, 177461183, -1252408744, -1397390776 }, "3 Tacos with Onion and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, -853757044, 111245472, 177461183, -1252408744, -853757044 }, "2 Tacos with Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1252408744, -853757044, 111245472, 177461183, -1252408744, -853757044, 111245472, 177461183, -1252408744, -853757044 }, "3 Tacos with Onion and Tomato");

            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044, 111245472, 177461183, -1397390776, -853757044 }, "2 Tacos with Lettuce and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044, 111245472, 177461183, -1397390776, -853757044, 111245472, 177461183, -1397390776, -853757044 }, "3 Tacos with Lettuce and Tomato");
            
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -1397390776, 111245472, 177461183, 263830100, -1252408744, -1397390776 }, "2 Tacos with Cheese, Onion and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -1397390776, 111245472, 177461183, 263830100, -1252408744, -1397390776, 111245472, 177461183, 263830100, -1252408744, -1397390776 }, "3 Tacos with Cheese, Onion and Lettuce");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -853757044, 111245472, 177461183, 263830100, -1252408744, -853757044 }, "2 Tacos with Cheese, Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, 263830100, -1252408744, -853757044, 111245472, 177461183, 263830100, -1252408744, -853757044, 111245472, 177461183, 263830100, -1252408744, -853757044 }, "3 Tacos with Cheese, Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -1252408744, -853757044, 111245472, 177461183, -1397390776, -1252408744, -853757044 }, "2 Tacos with Lettuce, Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -1252408744, -853757044, 111245472, 177461183, -1397390776, -1252408744, -853757044, 111245472, 177461183, -1397390776, -1252408744, -853757044 }, "3 Tacos with Lettuce, Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, 263830100, -853757044, 111245472, 177461183, -1397390776, 263830100, -853757044 }, "2 Tacos with Lettuce, Onion and Tomato");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, 263830100, -853757044, 111245472, 177461183, -1397390776, 263830100, -853757044, 111245472, 177461183, -1397390776, 263830100, -853757044 }, "3 Tacos with Lettuce, Cheese and Tomato");

            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100, 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100 }, "2 Tacos with Lettuce, Tomato, Onion and Cheese");
            Add(new FixedListInt64 { 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100, 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100, 111245472, 177461183, -1397390776, -853757044, -1252408744, 263830100 }, "3 Tacos with Lettuce, Tomato, Onion and Cheese");
            // Hot Dogs
            Add(new FixedListInt64 { 793377380, 756326364, -248200024 }, "Hot Dog");
            Add(new FixedListInt64 { -1075930689 }, "Ketchup");
            Add(new FixedListInt64 { -1114203942 }, "Mustard");
            // Breakfast
            Add(new FixedListInt64 { 793377380, 428559718 }, "Toast");

            Add(new FixedListInt64 { 793377380, 428559718, -2138118944 }, "Beans on Toast");
            Add(new FixedListInt64 { 793377380, 428559718, 1324261001 }, "Egg on Toast");
            Add(new FixedListInt64 { 793377380, 428559718, -853757044 }, "Toast with Tomato");
            Add(new FixedListInt64 { 793377380, 428559718, -2093899333 }, "Toast with Mushroom");

            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944 }, "Egg on Toast with Beans");
            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -853757044 }, "Egg on Toast with Tomato");
            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -2093899333 }, "Egg on Toast with Mushroom");
            Add(new FixedListInt64 { 793377380, 428559718, -2138118944, -853757044 }, "Beans on Toast with Tomato");
            Add(new FixedListInt64 { 793377380, 428559718, -2138118944, -2093899333 }, "Beans on Toast with Mushroom");
            Add(new FixedListInt64 { 793377380, 428559718, -853757044, -2093899333 }, "Toast with Tomato and Mushroom");

            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944, -853757044 }, "Egg on Toast with Beans and Tomato");
            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -2138118944, -2093899333 }, "Egg on Toast with Beans and Mushroom");
            Add(new FixedListInt64 { 793377380, 428559718, -2138118944, -853757044, -2093899333 }, "Beans on Toast with Tomato and Mushroom");
            Add(new FixedListInt64 { 793377380, 428559718, 1324261001, -853757044, -2093899333 }, "Egg on Toast with Tomato and Mushroom");
            // Stir Fry
            Add(new FixedListInt64 { 793377380, 1928939081, -1406021079 }, "Stir Fry with Carrot");
            Add(new FixedListInt64 { 793377380, 1928939081, -1018018897 }, "Stir Fry with Meat");
            Add(new FixedListInt64 { 793377380, 1928939081, 1453647256 }, "Stir Fry with Broccoli");
            Add(new FixedListInt64 { 793377380, 1928939081, 880804869 }, "Stir Fry with Bamboo");
            Add(new FixedListInt64 { 793377380, 1928939081, -336580972 }, "Stir Fry with Mushroom");

            Add(new FixedListInt64 { 793377380, 1928939081, -1406021079 }, "Stir Fry with Carrot and Meat");
            Add(new FixedListInt64 { 793377380, 1928939081, -1406021079 }, "Stir Fry with Carrot and Broccoli");
            Add(new FixedListInt64 { 793377380, 1928939081, -1406021079 }, "Stir Fry with Carrot and Bamboo");
            Add(new FixedListInt64 { 793377380, 1928939081, -1406021079 }, "Stir Fry with Carrot and Mushroom");

            Add(new FixedListInt64 { 793377380, 1928939081, -1018018897 }, "Stir Fry with Meat and Broccoli");
            Add(new FixedListInt64 { 793377380, 1928939081, -1018018897 }, "Stir Fry with Meat and Bamboo");
            Add(new FixedListInt64 { 793377380, 1928939081, -1018018897 }, "Stir Fry with Meat and Mushroom");

            Add(new FixedListInt64 { 793377380, 1928939081, 1453647256 }, "Stir Fry with Broccoli and Bamboo");
            Add(new FixedListInt64 { 793377380, 1928939081, 1453647256 }, "Stir Fry with Broccoli and Mushroom");

            Add(new FixedListInt64 { 793377380, 1928939081, 880804869 }, "Stir Fry with Bamboo and Mushroom");

            Add(new FixedListInt64 { 1190974918 }, "Soy Sauce");
        }
    }
}

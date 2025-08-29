using KitchenMods;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using Kitchen;

namespace Neuro_Plateup
{
    public static class ReceipeDB
    {
        // -1778969928 (-884392267)
        // find appliance 385684499 -> grab
        // find appliance {hob} -> grab
        // find appliance {plates} -> grab
        // find appliance 759552160 -> grab
        // find uncooked pattie (1150879908) on {hob} -> wait
        // find appliance {hob} with 687585830 -> grab
        // If (-853757044) is in receipe -> get ingredient
        // find plated burger -> grab
        // find empty hatch -> grab
        public static readonly Dictionary<int, int[]> ComponentOrder = new Dictionary<int, int[]>
        {
            // Burgers
            { -884392267, new int[] { 687585830, -1756808590, 793377380, -853757044, -1252408744, 263830100 } },
            // Ice cream
            { -1307479546, new int[] { 502129042, 186895094, 1570518340 } }
        };
    }
}
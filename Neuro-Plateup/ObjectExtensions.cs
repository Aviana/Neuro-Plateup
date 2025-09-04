using System.Collections.Generic;
using Kitchen;
using KitchenData;
using Neuro_Plateup;
using Unity.Collections;

public static class ObjectExtensions
{
    public static FixedListInt64 ToFixedListInt64(this ItemList container)
    {
        var list = new FixedListInt64();
        foreach (var entry in container)
        {
            list.Add(entry);
        }
        return list;
    }

    public static bool Contains(this ItemList container, int value)
    {
        for (int i = 0; i < container.Count; i++)
        {
            if (container[i] == value)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsEquivalent(this ItemList container, FixedListInt64 list)
    {
        if (container.Count != list.Length)
            return false;

        var matched = new bool[list.Length];

        for (int i = 0; i < container.Count; i++)
        {
            bool found = false;

            for (int j = 0; j < list.Length; j++)
            {
                if (!matched[j] && container[i] == list[j])
                {
                    matched[j] = true;
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;
        }

        return true;
    }

    public static bool Contains(this List<ItemInfo> list, CItem item)
    {
        foreach (var entry in list)
        {
            if (entry.ID == item.ID && item.Items.IsEquivalent(entry.Items))
            {
                return true;
            }
        }
        return false;
    }
}
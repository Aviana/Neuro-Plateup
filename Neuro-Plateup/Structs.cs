using Kitchen;
using KitchenMods;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;

namespace Neuro_Plateup
{
    public struct CBotControl : IModComponent { }

    public struct CBotActionRunning : IModComponent { }

    public struct CBotWaiting : IModComponent
    {
        public Vector3 Position;
        public int itemID;
        public CBotWaiting(Vector3 position, int resultID)
        {
            Position = position;
            itemID = resultID;
        }
    }

    public struct CBotWatching : IBufferElementData
    {
        public Vector3 Position;
        public int itemID;
        public CBotWatching(Vector3 position, int resultID)
        {
            Position = position;
            itemID = resultID;
        }
    }

    public struct CBotItems : IBufferElementData
    {
        public ItemInfo Item;
        public Vector3 Position;
        public CBotItems(ItemInfo item, Vector3 position)
        {
            Item = item;
            Position = position;
        }
    }

    public struct CBotOrders : IBufferElementData
    {
        public int ID;
        public FixedListInt64 Items;
        public CBotOrders(int id, FixedListInt64 items)
        {
            ID = id;
            Items = items;
        }
    }

    public struct CBotFeedback : IBufferElementData
    {
        public FixedString64 Message;
        public bool IsSilent;
        public CBotFeedback(string message, bool isSilent = false)
        {
            Message = message;
            IsSilent = isSilent;
        }
    }

    public struct CBotAction : IComponentData
    {
        public FixedString64 Action;
        public FixedString64 Payload;
        public CBotAction(string action, string payload = "")
        {
            Action = action;
            Payload = payload;
        }
    }

    public struct CBotRole : IComponentData
    {
        public BotRole Role;
        public CBotRole(BotRole role)
        {
            Role = role;
        }
    }

    public struct CMoveTo : IComponentData
    {
        public Vector3 Position;

        public CMoveTo(int x, int y, int z)
        {
            Position = new Vector3 { x = x, y = y, z = z };
        }
        public CMoveTo(Vector3 position)
        {
            Position = position;
        }
    }

    public struct CGrabAction : IComponentData
    {
        public Vector3 Position;

        public GrabType Type;

        public ItemInfo HandItem;

        public ItemInfo ApplianceItem;

        public CGrabAction(int x, int y, int z, GrabType type, ItemInfo hand = default, ItemInfo appliance = default)
        {
            Position = new Vector3 { x = x, y = y, z = z };
            Type = type;
            HandItem = hand;
            ApplianceItem = appliance;
        }
        public CGrabAction(Vector3 position, GrabType type, ItemInfo hand = default, ItemInfo appliance = default)
        {
            Position = position;
            Type = type;
            HandItem = hand;
            ApplianceItem = appliance;
        }
    }

    public struct CInteractAction : IComponentData
    {
        public Vector3 Position;
        public bool HasProgress;

        public CInteractAction(int x, int y, int z, bool hasProgress)
        {
            Position = new Vector3 { x = x, y = y, z = z };
            HasProgress = hasProgress;
        }
        public CInteractAction(Vector3 position, bool hasProgress)
        {
            Position = position;
            HasProgress = hasProgress;
        }
    }

    public struct OrderList
    {
        public List<ItemInfo> Items;
        public Vector3 Position;

        public readonly void Add(ItemInfo item)
        {
            Items.Add(item);
        }

        public OrderList(Vector3 position)
        {
            Items = new List<ItemInfo>();
            Position = position;
        }

        public readonly IEnumerator<ItemInfo> GetValues() => Items.GetEnumerator();
    }

    public struct ItemInfo
    {
        public int ID;
        public FixedListInt64 Items;

        public ItemInfo(int id, FixedListInt64 list)
        {
            ID = id;
            Items = list;
        }

        public ItemInfo(int id)
        {
            ID = id;
            Items = new FixedListInt64 { id };
        }

        public ItemInfo(CItem item)
        {
            ID = item.ID;
            Items = new FixedListInt64();
            foreach (var i in item.Items)
                Items.Add(i);
        }

        public ItemInfo(int id, params int[] items)
        {
            ID = id;
            Items = new FixedListInt64();
            foreach (var item in items)
                Items.Add(item);
        }

        public ItemInfo(CItem item, int component)
        {
            ID = item.ID;
            Extra = 0;
            foreach (var side in OrderNameRepository.Sides)
            {
                if (item.Items.Contains(side))
                    Extra = side;
            }
            Items = new FixedListInt64();
            foreach (var i in item.Items)
            {
                if (i != Extra)
                    Items.Add(i);
            }
            Items.Add(component);
        }

        public static bool operator ==(ItemInfo a, CItem b)
        {
            if (a.ID != b.ID)
                return false;

            return b.Items.IsEquivalent(a.Items);
        }

        public static bool operator !=(ItemInfo a, CItem b)
        {
            return !(a == b);
        }

        public static bool operator ==(CItem a, ItemInfo b)
        {
            if (a.ID != b.ID)
                return false;

            return a.Items.IsEquivalent(b.Items);
        }

        public static bool operator !=(CItem a, ItemInfo b)
        {
            return !(a == b);
        }

        public static bool operator ==(ItemInfo a, ItemInfo b)
        {
            if (a.Items.Length != b.Items.Length || a.ID != b.ID)
                return false;

            var matched = new bool[b.Items.Length];

            for (int i = 0; i < a.Items.Length; i++)
            {
                bool found = false;

                for (int j = 0; j < b.Items.Length; j++)
                {
                    if (!matched[j] && a.Items[i] == b.Items[j])
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

        public static bool operator !=(ItemInfo a, ItemInfo b)
        {
            return !(a == b);
        }

        public readonly bool Equals(ItemInfo other)
        {
            return this == other;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ItemInfo other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            int hash = 17;

            FixedListInt64 sortedItems = Items;
            Sort(ref sortedItems);

            for (int i = 0; i < sortedItems.Length; i++)
            {
                hash = hash * 31 + sortedItems[i];
            }

            return hash;
        }

        private static void Sort(ref FixedListInt64 list)
        {
            for (int i = 1; i < list.Length; i++)
            {
                int key = list[i];
                int j = i - 1;

                while (j >= 0 && list[j] > key)
                {
                    list[j + 1] = list[j];
                    j--;
                }

                list[j + 1] = key;
            }
        }
    }
}
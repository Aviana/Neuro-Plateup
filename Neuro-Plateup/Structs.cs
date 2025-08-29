using KitchenMods;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using Kitchen;
using System.Collections;
using System.Linq;

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

        public CGrabAction(int x, int y, int z)
        {
            Position = new Vector3 { x = x, y = y, z = z };
        }
        public CGrabAction(Vector3 position)
        {
            Position = position;
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

    public readonly struct ItemComponentList
    {
        private readonly List<CItem> Data;

        public readonly void Add(CItem item)
        {
            Data.Add(item);
        }

        public readonly bool Contains(CItem a)
        {
            foreach (var item in Data)
            {
                if (item.ID == a.ID && item.Items.IsEquivalent(a.Items))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsEmpty => Data.Count == 0;

        public ItemComponentList(bool empty = true)
        {
            Data = new List<CItem>();
        }

        public ItemComponentList(CItem item)
        {
            Data = new List<CItem> { item };
        }

        public void Remove(CItem item)
        {
            Data.Remove(item);
        }

        public int Count => Data.Count;

        public CItem First() => Data.First();

        public CItem this[int index]
        {
            get => Data[index];
            set => Data[index] = value;
        }

        public void RemoveAt(int i) => Data.RemoveAt(i);
    }

    public struct OrderList
    {
        public ItemComponentList Items;
        public Vector3 Position;

        public readonly void Add(CItem item)
        {
            Items.Add(item);
        }

        public OrderList(CItem item, Vector3 position)
        {
            Items = new ItemComponentList(item);
            Position = position;
        }
    }

    public struct ItemCreationProcess
    {
        public int ID;
        public HashSet<int> itemIDs;
        public HashSet<int> Appliances;
    }
}
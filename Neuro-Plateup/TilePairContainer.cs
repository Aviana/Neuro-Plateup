using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Neuro_Plateup
{
    public class HashSetOfVector3Comparer : IEqualityComparer<HashSet<Vector3>>
    {
        public bool Equals(HashSet<Vector3> x, HashSet<Vector3> y)
        {
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<Vector3> obj)
        {
            int hash = 0;
            foreach (var v in obj)
            {
                hash ^= v.GetHashCode();
            }
            return hash;
        }
    }

    public class TilePairContainer : IEnumerable<Vector3[]>
    {
        private readonly HashSet<HashSet<Vector3>> entries = new HashSet<HashSet<Vector3>>(new HashSetOfVector3Comparer());
        private readonly Dictionary<Vector3, List<HashSet<Vector3>>> lookup = new Dictionary<Vector3, List<HashSet<Vector3>>>();

        public void Add(HashSet<Vector3> entry)
        {
            if (entry.Count != 2 || entries.Contains(entry))
                return;

            entries.Add(entry);

            foreach (var i in entry)
            {
                AddToLookup(i, entry);
            }
        }

        public void Add(Vector3 tile1, Vector3 tile2)
        {
            var entry = new HashSet<Vector3> { tile1, tile2 };
            Add(entry);
        }

        private void AddToLookup(Vector3 tile, HashSet<Vector3> entry)
        {
            if (!lookup.TryGetValue(tile, out var list))
            {
                list = new List<HashSet<Vector3>>();
                lookup[tile] = list;
            }

            list.Add(entry);
        }

        public bool Contains(Vector3 tile)
        {
            return lookup.ContainsKey(tile);
        }

        public bool Contains(Vector3 Tile1, Vector3 Tile2)
        {
            return entries.Contains(new HashSet<Vector3> { Tile1, Tile2 });
        }

        public bool TryGet(Vector3 tile, out List<HashSet<Vector3>> list)
        {
            return lookup.TryGetValue(tile, out list);
        }

        public void Clear()
        {
            entries.Clear();
            lookup.Clear();
        }

        public void Remove(HashSet<Vector3> entry)
        {
            if (entries.Remove(entry))
            {
                foreach (var t in entry)
                {
                    RemoveFromLookup(t, entry);
                }
            }
        }

        public void Remove(Vector3 tile)
        {
            if (!lookup.TryGetValue(tile, out var list))
                return;

            var toRemove = new List<HashSet<Vector3>>(list);

            foreach (var entry in toRemove)
            {
                Remove(entry);
            }
        }

        private void RemoveFromLookup(Vector3 tile, HashSet<Vector3> entry)
        {
            if (lookup.TryGetValue(tile, out var list))
            {
                list.Remove(entry);
                if (list.Count == 0)
                {
                    lookup.Remove(tile);
                }
            }
        }

        public IEnumerator<Vector3[]> GetEnumerator()
        {
            foreach (var entry in entries)
            {
                var arr = new Vector3[2];
                entry.CopyTo(arr);
                yield return arr;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => entries.Count;
    }
}
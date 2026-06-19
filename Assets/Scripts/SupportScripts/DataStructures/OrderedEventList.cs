using System;
using System.Collections.Generic;
 
namespace CustomDataStructures
{
    /// Functions like a standard C# multicast event, but additionally supports
    /// O(1) time for accessing and/or removing subscribers
    /// occasional O(n) compaction passes.
    ///
    /// Differences from a normal C# event:
    ///   - Add() returns an integer handle you can use to access or remove
    ///     that specific subscriber later via the indexer or RemoveAt().
    ///   - Invocation order matches subscription order (modulo removed slots).
    ///   - Removed slots are tombstoned (set to null) rather than shifted,
    ///     so existing handles stay valid until a compaction occurs.
    /// 
    /// typeparam name="T" Must be a delegate type, e.g. Action, EventHandler, Func
    public class OrderedEventList<T> where T : Delegate
    {
        // contain the delegates
        #nullable enable
        private readonly List<T?> _items = new();
        private int _liveCount;
 
        //Total slots, including emptied slots.
        public int Capacity => _items.Count;
 
        /// Number of live subscribers.
        public int Count => _liveCount;
 
        /// O(1) indexed access to a subscriber slot. Returns null if that
        /// slot was removed or the index is otherwise inactive.
        public T? this[int index] => _items[index];

        #region Operator Overloads
        // operator overloads to use += to add an event
        public static OrderedEventList<T> operator +(OrderedEventList<T> evt, T handler)
        {
            evt.Add(handler);
            return evt;
        }

        // operator overloads to use -= to remove an event in O(n)
        public static OrderedEventList<T> operator -(OrderedEventList<T> evt, T handler)
        {
            evt.Remove(handler);
            return evt;
        }

        // operator overloads to use -= to remove an event by indexin O(1)
        public static OrderedEventList<T> operator -(OrderedEventList<T> evt, int index)
        {
            evt.RemoveAt(index);
            return evt;
        }
        #endregion
 
        /// Subscribes a handler and returns its index handle.
        /// Use this handle with RemoveAt() or the indexer later.
        /// O(1) amortized (List&lt;T&gt;.Add is O(1) amortized).
        /// returns an index to access the subscriber
        public int Add(T handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));
 
            _items.Add(handler);
            _liveCount++;
            return _items.Count - 1;
        }
 
        /// Removes the subscriber at the given index. O(1).
        /// Safe to call on an already-removed or invalid index (no-op).
        public void RemoveAt(int index)
        {
            // basic checks
            if (index < 0 || index >= _items.Count) return;
            if (_items[index] is null) return;
 
            _items[index] = null;
            _liveCount--;
 
            // Compact null slots so that the list isn't full of open space
            // full clear if there's nothing left
            if (_liveCount < _items.Count / 2)
            {
                Compact(_liveCount == 0);
            }
        }
 
        /// Removes the first slot matching this exact delegate, mirroring
        /// the behavior of a normal event's -= operator. O(n) search
        public void Remove(T handler)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                // use null forgiving operator because alr check for null
                if (_items[i] is not null && _items[i]!.Equals(handler))
                {
                    RemoveAt(i);
                    return;
                }
            }
        }
 
        /// Invokes every live subscriber in subscription order.
        /// Skips null slots (obviously)
        public void Invoke(params object?[] args)
        {
            // maintain list size, just in case the list changes mid invoke 
            int items_count = _items.Count;
            // use invoke, since func, action, and allat can all use this
            for (int i = 0; i < items_count; i++)
            {
                _items[i]?.DynamicInvoke(args);
            }
        }
 
        /// Removes all null slots in the list in O(n). 
        /// Can be used to fully clear the list in O(1)
        public void Compact(bool full_clear = false)
        {
            if (full_clear)
            {
                _items.Clear();
            }
            else
            {
                _items.RemoveAll(d => d == null);
            }
        }
    }
}
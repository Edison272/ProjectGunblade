using Unity;
using System;
using System.Collections.Generic;
using CustomDataStructures;

namespace OrderedEvents
{
    /// <summary>
    /// Functions like a standard C# multicast event, but additionally supports
    /// O(1) indexed access and O(1) removal-by-index, at the cost of
    /// occasional O(n) compaction passes.
    ///
    /// Differences from a normal C# event:
    ///   - Add() returns an integer handle you can use to access or remove
    ///     that specific subscriber later via the indexer or RemoveAt().
    ///   - Invocation order matches subscription order (modulo removed slots).
    ///   - Removed slots are tombstoned (set to null) rather than shifted,
    ///     so existing handles stay valid until a compaction occurs.
    /// </summary>
    /// <typeparam name="T">Must be a delegate type, e.g. Action, EventHandler, Func&lt;...&gt;.</typeparam>
    public class OrderedEvent<T> where T : Delegate
    {
        #nullable enable
        private readonly List<T?> _items = new();
        private int _liveCount;

        /// <summary>Total slots, including tombstoned (removed) ones.</summary>
        public int Capacity => _items.Count;

        /// <summary>Number of live (non-removed) subscribers.</summary>
        public int Count => _liveCount;

        /// <summary>
        /// O(1) indexed access to a subscriber slot. Returns null if that
        /// slot was removed or the index is otherwise inactive.
        /// </summary>
        public T? this[int index] => _items[index];

        #region Operator Overloads
        // operator overloads to use += to add a handler
        public static OrderedEvent<T> operator +(OrderedEvent<T> evt, T handler)
        {
            evt ??= new OrderedEvent<T>();
            evt.Add(handler);
            return evt;
        }

        // operator overloads to use -= to remove a handler by delegate, O(n)
        public static OrderedEvent<T> operator -(OrderedEvent<T> evt, T handler)
        {
            evt.Remove(handler);
            return evt;
        }

        // operator overload to use -= to remove a handler by index, O(1)
        public static OrderedEvent<T> operator -(OrderedEvent<T> evt, int index)
        {
            evt.RemoveAt(index);
            return evt;
        }
        #endregion

        /// <summary>
        /// Subscribes a handler and returns its index handle.
        /// Use this handle with RemoveAt() or the indexer later.
        /// O(1) amortized (List&lt;T&gt;.Add is O(1) amortized).
        /// </summary>
        public int Add(T handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            _items.Add(handler);
            _liveCount++;
            return _items.Count - 1;
        }

        /// <summary>
        /// Removes the subscriber at the given index. O(1).
        /// Safe to call on an already-removed or invalid index (no-op).
        /// </summary>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            if (_items[index] is null) return;

            _items[index] = null;
            _liveCount--;

            // Optional: compact once dead slots dominate, so Capacity
            // doesn't grow unboundedly under heavy churn. This is the
            // O(n) cost, but it's amortized and infrequent. If nothing's
            // left alive, skip straight to a full Clear() instead of
            // scanning with RemoveAll's predicate.
            if (_liveCount < _items.Count / 2)
            {
                Compact(_liveCount == 0);
            }
        }

        /// <summary>
        /// Removes the first slot matching this exact delegate, mirroring
        /// the behavior of a normal event's -= operator. O(n) search,
        /// since unlike RemoveAt this doesn't have an index to go on.
        /// </summary>
        public void Remove(T handler)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] is not null && _items[i]!.Equals(handler))
                {
                    RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Invokes every live subscriber in subscription order.
        /// Mirrors how a normal multicast delegate invocation works,
        /// but skips tombstoned (removed) slots.
        /// </summary>
        public void Invoke(params object?[] args)
        {
            // Snapshot count to avoid issues if a handler adds new subscribers
            // mid-invocation (mirrors how real events snapshot their
            // invocation list before invoking).
            int snapshotLength = _items.Count;

            for (int i = 0; i < snapshotLength; i++)
            {
                _items[i]?.DynamicInvoke(args);
            }
        }

        /// <summary>
        /// Zero-arg overload, so this can be bridged into a plain
        /// Action-typed vanilla event (e.g. someEvent += hashedEvent.Invoke)
        /// without a CS0123 signature mismatch. Still routes through
        /// DynamicInvoke underneath — this fixes the compile-time shape
        /// mismatch, not the reflection cost. If you need a fast, direct
        /// call, use the non-generic Action-only UnorderedEvent instead.
        /// </summary>
        public void Invoke() => Invoke(Array.Empty<object?>());

        /// <summary>
        /// Compacts out tombstoned slots, shrinking Capacity back down to
        /// Count. This invalidates any previously returned index handles,
        /// since live items shift to fill the gaps. O(n).
        /// Pass full_clear=true when every slot is known to be dead, to
        /// skip straight to Clear() instead of scanning with RemoveAll.
        /// </summary>
        public void Compact(bool full_clear = false)
        {
            if (full_clear)
            {
                _items.Clear();
            }
            else
            {
                _items.RemoveAll(d => d is null);
            }
        }
    }
}
using System;
using System.Collections.Generic;

namespace CustomDataStructures
{
    /// <summary>
    /// Functions like a C# multicast event (scoped to zero-argument Action
    /// subscribers), but backed by a HashSet instead of an array/list,
    /// trading a bit of extra memory for O(1) add, remove, and
    /// contains-checks.
    ///
    /// Differences from a normal C# event:
    ///   - Add(), Remove(), and Contains() are all O(1), instead of the
    ///     O(n) copy-on-write behavior of a real event's invocation list.
    ///   - No subscription order guarantee. HashSet&lt;T&gt; enumeration order
    ///     is based on internal bucket layout, not insertion order, and can
    ///     change after the set resizes internally. If you need invocation
    ///     order to match subscription order, use OrderedEvent instead.
    ///   - No index-based access. There's no array position to hand out, so
    ///     unlike OrderedEvent, there are no integer handles and no indexer
    ///     — removal is always by delegate reference via Remove().
    ///   - Subscribers are invoked directly (no DynamicInvoke/reflection),
    ///     since the shape is locked to plain Action.
    ///
    /// Not generic: this only supports zero-argument Action subscribers.
    /// If you need to pass data to subscribers, a one-argument version
    /// (HashedEvent&lt;TArg&gt; wrapping Action&lt;TArg&gt;) would be a separate class.
    /// </summary>
    public class HashedEvent
    {
        private readonly HashSet<Action> _items = new();

        /// <summary>Number of currently subscribed handlers.</summary>
        public int Count => _items.Count;

        #region Operator Overloads
        // operator overloads to use += to add a handler
        public static HashedEvent operator +(HashedEvent evt, Action handler)
        {
            evt ??= new HashedEvent();
            evt.Add(handler);
            return evt;
        }

        // operator overloads to use -= to remove a handler in O(1)
        public static HashedEvent operator -(HashedEvent evt, Action handler)
        {
            evt.Remove(handler);
            return evt;
        }
        #endregion

        /// <summary>
        /// Subscribes a handler. O(1). Adding the same delegate twice is a
        /// no-op the second time, since HashSet&lt;T&gt; only stores unique values.
        /// </summary>
        public void Add(Action handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            _items.Add(handler);
        }

        /// <summary>
        /// Unsubscribes a handler, if present. O(1). Safe to call on a
        /// handler that was never added (no-op).
        /// </summary>
        public void Remove(Action handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            _items.Remove(handler);
        }

        /// <summary>
        /// Checks whether a handler is currently subscribed. O(1).
        /// </summary>
        public bool Contains(Action handler) => _items.Contains(handler);

        /// <summary>
        /// Invokes every subscribed handler. Order is NOT guaranteed to
        /// match subscription order — see class remarks above. Direct
        /// call, no reflection.
        /// </summary>
        public void Invoke()
        {
            foreach (Action item in _items)
            {
                item.Invoke();
            }
        }
    }
}
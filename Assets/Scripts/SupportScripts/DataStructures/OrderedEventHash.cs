using System;
using System.Collections.Generic;
 
namespace CustomDataStructures
{
    /// Functions like a standard C# multicast event, but uses a hashset
    /// sacrifice a lil extra memory for full O(1) time lookups
    ///
    /// Differences from a normal C# event:
    ///   - Add(), Remove(), and Contains() are all O(1), instead of the
    ///     O(n) copy-on-write behavior of a real event's invocation list.
    ///   - No subscription order guarantee. HashSet&lt;T&gt; enumeration order
    ///     is based on internal bucket layout, not insertion order, and can
    ///     change after the set resizes internally. If you need invocation
    ///     order to match subscription order, use OrderedEvent&lt;T&gt; instead.
    ///   - No index-based access. There's no array position to hand out, so
    ///     unlike OrderedEvent, there are no integer handles and no
    ///     indexer — removal is always by delegate reference via Remove().
    /// 
    /// typeparam name="T" Must be a delegate type, e.g. Action, EventHandler
    public class OrderedEventHash<T> where T : Delegate
    {
        private readonly HashSet<T> _items = new();
 
        // All delegates subscribed
        public int Count => _items.Count;

        #region Operator Overloads
        // operator overloads to use += to add an event
        public static OrderedEventHash<T> operator +(OrderedEventHash<T> evt, T handler)
        {
            evt.Add(handler);
            return evt;
        }

        // operator overloads to use -= to remove an event in O(n)
        public static OrderedEventHash<T> operator -(OrderedEventHash<T> evt, T handler)
        {
            evt.Remove(handler);
            return evt;
        }
        #endregion
 
        /// Subscribes a handler. O(1). No double adding functions!
        public void Add(T handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));
 
            _items.Add(handler);
        }
 
        /// Safety Unsubscribes a handler, if present. O(1).
        public void Remove(T handler)
        {
            if (handler is null) throw new ArgumentNullException(nameof(handler));
 
            _items.Remove(handler);
        }
 
        /// Invokes every live subscriber, but NOT GUARANTEED TO MATCH SUBSCRIPTION ORDER
        public void Invoke(params object?[] args)
        {
            // use invoke, since func, action, and allat can all use this
            foreach (T item in _items)
            {
                item?.DynamicInvoke(args);
            }
        }
    }
}
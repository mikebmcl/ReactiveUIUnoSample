using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReactiveUIUnoSample
{
    /// <summary>
    /// Manages subscribing to, unsubscribing from, and raising an event using a <see cref="WeakReference"/> to the subscriber to
    /// avoid keeping alive a subscriber that is otherwise unreferenced but did not unsubscribe from the event (presumably because it
    /// would have been impractical to do so in a clean way though it also covers situations where the program could have reasonably
    /// unsubscribed but didn't).
    /// </summary>
    internal class WeakEventHandlerEventManager
    {
        private readonly struct SubscriberInfo
        {
            public readonly WeakReference Subscriber;
            public readonly MethodInfo EventHandlerDelegate;
            public SubscriberInfo(WeakReference subscriber, MethodInfo eventHandlerDelegate)
            {
                Subscriber = subscriber;
                EventHandlerDelegate = eventHandlerDelegate;
            }
        }

        private readonly Dictionary<string, List<SubscriberInfo>> m_subscriptions = new Dictionary<string, List<SubscriberInfo>>();

        public void RaiseEvent(object sender, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"Bad value '{eventName ?? "(null)"}'", nameof(eventName));
            }
            if (m_subscriptions.TryGetValue(eventName, out List<SubscriberInfo> eventHandlerDelegateList))
            {
                foreach (SubscriberInfo item in eventHandlerDelegateList)
                {
                    object subscriber = item.Subscriber.Target;
                    if (subscriber != null)
                    {
                        item.EventHandlerDelegate.Invoke(subscriber, new object[] { sender, (object)EventArgs.Empty });
                    }
                }
                eventHandlerDelegateList.RemoveAll(ehdl => ehdl.Subscriber.Target == null);
            }
        }

        public void AddHandler(EventHandler eventHandler, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"Bad value '{eventName ?? "(null)"}'", nameof(eventName));
            }
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }
            if (m_subscriptions.ContainsKey(eventName))
            {
                m_subscriptions[eventName].Add(new SubscriberInfo(new WeakReference(eventHandler.Target), eventHandler.GetMethodInfo()));
            }
            else
            {
                m_subscriptions.Add(eventName, new List<SubscriberInfo>() { new SubscriberInfo(new WeakReference(eventHandler.Target), eventHandler.GetMethodInfo()) });
            }
        }

        public void RemoveHandler(EventHandler eventHandler, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException($"Bad value '{eventName ?? "(null)"}'", nameof(eventName));
            }
            if (eventHandler == null)
            {
                throw new ArgumentNullException(nameof(eventHandler));
            }
            if (!m_subscriptions.ContainsKey(eventName))
            {
                throw new KeyNotFoundException($"No event named {eventName} found to unsubscribe from.");
            }
            try
            {
                m_subscriptions[eventName].Remove(m_subscriptions[eventName].First(si => si.Subscriber?.Target == eventHandler.Target && si.EventHandlerDelegate.Name == eventHandler.GetMethodInfo().Name));
            }
            catch
            {
                // bury exception, we don't care that we couldn't find a match or that something exploded.
            }
        }
    }
}
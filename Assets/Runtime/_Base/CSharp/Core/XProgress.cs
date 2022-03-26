using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public delegate void XProgressHandler(string senderName, string eventName);
    public class XProgressEvent
    {
        public string SenderName;
        public string EventName;
        public XProgressHandler Handler;
        public bool Fired;
    }

    public class XProgressHolder
    {
        private SortedDictionary<int, List<XProgressEvent>> mEvents;

        public XProgressHolder()
        {
            this.mEvents = new SortedDictionary<int, List<XProgressEvent>>();
        }

        public void Attach(string senderName, float percent, string eventName, XProgressHandler handler)
        {
            if (percent < 0 || percent > 1)
                return;
            if (handler == null)
                return;
            int eventId = this.PercentToEventId(percent);
            if (!this.mEvents.ContainsKey(eventId))
            {
                this.mEvents[eventId] = new List<XProgressEvent>();
            }
            XProgressEvent evt = new XProgressEvent();
            evt.SenderName = senderName;
            evt.EventName = eventName;
            evt.Handler = handler;
            evt.Fired = false;
            this.mEvents[eventId].Add(evt);
        }

        public void Detach(string senderName, float percent, string eventName, XProgressHandler handler)
        {
            int eventId = this.PercentToEventId(percent);
            if (!this.mEvents.ContainsKey(eventId))
                return;
            foreach (XProgressEvent evt in this.mEvents[eventId])
            {
                if (evt.EventName == eventName && evt.Handler == handler)
                {
                    this.mEvents[eventId].Remove(evt);
                }
            }
            if (this.mEvents[eventId].Count == 0)
            {
                this.mEvents.Remove(eventId);
            }
        }

        public void Clear()
        {
            this.mEvents.Clear();
        }

        public void Fire(float percent)
        {
            int eventId = this.PercentToEventId(percent);
            foreach (int evtId in this.mEvents.Keys)
            {
                if (evtId <= eventId)
                {
                    foreach (XProgressEvent evt in this.mEvents[evtId])
                    {
                        if (!evt.Fired)
                        {
                            evt.Handler(evt.SenderName, evt.EventName);
                            evt.Fired = true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void Freese(float percent)
        {
            int eventId = this.PercentToEventId(percent);
            if (!this.mEvents.ContainsKey(eventId))
                return;
            foreach (int evtId in this.mEvents.Keys)
            {
                if (evtId <= eventId)
                {
                    foreach (XProgressEvent evt in this.mEvents[evtId])
                    {
                        evt.Fired = false;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public void FreeseAll()
        {
            foreach (int evtId in this.mEvents.Keys)
            {
                foreach (XProgressEvent evt in this.mEvents[evtId])
                {
                    evt.Fired = false;
                }
            }
        }

        public bool AllFired()
        {
            foreach (int evtId in this.mEvents.Keys)
            {
                foreach (XProgressEvent evt in this.mEvents[evtId])
                {
                    if (!evt.Fired)
                    {
                        return false;
                    }
                }
            }
            return true; ;
        }

        public bool AnyFired()
        {
            foreach (int evtId in this.mEvents.Keys)
            {
                foreach (XProgressEvent evt in this.mEvents[evtId])
                {
                    if (evt.Fired)
                    {
                        return true;
                    }
                }
            }
            return false; ;
        }

        public float EventIdToPercent(int eventId)
        {
            return eventId / 10000.0f;
        }

        public int PercentToEventId(float percent)
        {
            return (int)(percent * 10000);
        }
    }
}



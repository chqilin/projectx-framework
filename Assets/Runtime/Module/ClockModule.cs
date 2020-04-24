using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public class ClockModule : AppModule
    {
        public class Clock
        {
            public enum ClockStatus
            {
                Ready,
                Running,
                Done
            }

            public delegate void Handler(Clock clock);
            public Handler onTick;
            public float tickTime = 1.0f;

            private SerialNumber mSerialNumber = new ScopedUniqueSerialNumber(typeof(Clock).Name.GetHashCode());
            private ClockStatus mStatus = ClockStatus.Ready;
            private long mCurrCount = 0;
            private float mCurrTime = 0.0f;

            #region Properties
            public int id
            {
                get { return this.mSerialNumber; }
            }
            public ClockStatus status
            {
                get { return this.mStatus; }
            }
            public long currCount
            {
                get { return this.mCurrCount; }
            }
            public float currTime
            {
                get { return this.mCurrTime; }
            }
            #endregion

            #region Public Methods
            public void Start()
            {
                this.mStatus = ClockStatus.Running;
            }

            public void Stop()
            {
                this.mStatus = ClockStatus.Done;
            }

            public void Pause()
            {
                this.mStatus = ClockStatus.Ready;
            }

            public void Update(float elapse)
            {
                if (this.mStatus == ClockStatus.Running)
                {
                    this.mCurrTime += elapse;
                    if (this.mCurrTime >= this.tickTime)
                    {
                        this.mCurrCount += 1;
                        this.mCurrTime = 0.0f;
                        if (this.onTick != null)
                        {
                            this.onTick(this);
                        }
                    }
                }
            }
            #endregion
        };

        private Dictionary<int, Clock> mClocks = new Dictionary<int, Clock>();
        private List<Clock> mReadies = new List<Clock>();
        private List<Clock> mDeletes = new List<Clock>();

        #region Life Circle
        public override void Tick(float elapse)
        {
            // process new-clock in last frame
            foreach (Clock clock in this.mReadies)
            {
                this.mClocks.Add(clock.id, clock);
            }
            this.mReadies.Clear();

            // update all clocks.
            foreach (Clock clock in this.mClocks.Values)
            {
                switch (clock.status)
                {
                    case Clock.ClockStatus.Ready:
                        break;
                    case Clock.ClockStatus.Running:
                        clock.Update(elapse);
                        break;
                    case Clock.ClockStatus.Done:
                        this.mDeletes.Add(clock);
                        break;
                }
            }

            // process stopped-clock in this frame
            foreach (Clock clock in this.mDeletes)
            {
                this.mClocks.Remove(clock.id);
            }
            this.mDeletes.Clear();
        }
        #endregion

        #region Public Methods
        public Clock Get(int id)
        {
            Clock clock = null;
            this.mClocks.TryGetValue(id, out clock);
            return clock;
        }

        public Clock New(float time, Clock.Handler onTick)
        {
            if (time <= 0 || onTick == null)
                return null;
            Clock clock = new Clock();
            clock.tickTime = time;
            clock.onTick = onTick;
            this.mReadies.Add(clock);
            return clock;
        }

        public Clock New(float time, long count, Clock.Handler onTick)
        {
            if (time <= 0 || count <= 0 || onTick == null)
                return null;
            return this.New(time, clock =>
            {
                if (clock.currCount > count)
                {
                    clock.Stop();
                    return;
                }
                onTick(clock);
            });
        }
        
        public Clock Repeat(float time, Clock.Handler onTick)
        {
            Clock clock = this.New(time, onTick);
            if (clock != null)
            {
                clock.Start();
            }
            return clock;
        }

        public Clock Repeat(float time, long count, Clock.Handler onTick)
        {
            Clock clock = this.New(time, count, onTick);
            if (clock != null)
            {
                clock.Start();
            }
            return clock;
        }
        
        public void Cancel(int id)
        {
            Clock clock = this.Get(id);
            if (clock != null)
            {
                clock.Stop();
            }
        }

        public void CancelAll()
        {
            foreach (Clock clock in this.mClocks.Values)
            {
                clock.Stop();
            }
        }
        #endregion
    }
}

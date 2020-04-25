using System;
using System.Collections.Generic;

namespace ProjectX
{
    #region Automatic FSM
    public class AFSM
    {
        #region Internal Types
        public enum Status
        {
            None,
            Inited,
            Running,
            Completed,
            Ended
        }

        public abstract class State
        {
            public string name = "";
            public State last = null;
            public Dictionary<string, List<State>> next = new Dictionary<string, List<State>>();

            public Status status { get; private set; }
            public float time { get; private set; }
            public string signal { get; set; }

            #region Life Circle
            public virtual void Init()
            {
                this.time = 0;
                this.signal = "";
                this.status = Status.Inited;
            }

            public virtual void Start()
            {
                this.status = Status.Running;
            }

            public virtual void Update(float elapse)
            {
                this.time += elapse;
                this.status = Status.Running;
            }

            public virtual void Stop()
            {
                this.status = Status.Ended;
            }

            public virtual void Quit()
            {
                this.signal = "";
                this.status = Status.None;
            }

            public virtual void Reset()
            {
                this.time = 0;
                this.signal = "";
                this.status = Status.None;
            }
            #endregion

            #region Runtime Methods
            public void ReadyAllNextStates(string signal)
            {
                List<State> states = null;
                this.next.TryGetValue(signal, out states);
                if (states == null || states.Count == 0)
                    return;
                foreach (var state in states)
                {
                    if (state.status != Status.None)
                        continue;
                    state.Init();
                }
            }
            #endregion
        }
        #endregion

        private Dictionary<string, State> mStates = new Dictionary<string, State>();
        private State mEntry = null;
        private List<State> mExits = new List<State>();

        private Status mStatus = Status.None;

        #region Properties
        public Status status
        {
            get { return this.mStatus; }
        }
        #endregion

        #region Virtual Life Circle
        public virtual void Init()
        {
            foreach (State state in this.mStates.Values)
            {
                state.Reset();
            }

            this.mStatus = Status.Inited;
        }

        public virtual void Start()
        {
            if (this.mEntry != null)
            {
                this.mEntry.Init();
            }
            this.mStatus = Status.Running;
        }

        public virtual void Update(float elapse)
        {
            foreach (State state in this.mStates.Values)
            {
                if (string.IsNullOrEmpty(state.signal))
                    continue;
                state.ReadyAllNextStates(state.signal);
                state.signal = "";
            }

            foreach (State state in this.mStates.Values)
            {
                switch (state.status)
                {
                    case Status.Inited:
                        state.signal = "Start";
                        state.Start();
                        break;
                    case Status.Running:
                        state.Update(elapse);
                        break;
                    case Status.Completed:
                        state.Stop();
                        state.signal = "Stop";
                        break;
                }
            }

            if (this.IsAnyExitStateEnded() || this.IsAllStateEnded())
            {
                this.mStatus = Status.Completed;
            }
        }

        public virtual void Stop()
        {
            this.mStatus = Status.Ended;
        }

        public virtual void Quit()
        {
            foreach (State state in this.mStates.Values)
            {
                state.Reset();
            }

            this.mStatus = Status.None;
        }
        #endregion

        #region Runtime Methods
        public bool IsAnyExitStateEnded()
        {
            foreach (State state in this.mExits)
            {
                if (state.status == Status.Ended)
                    return true;
            }
            return false;
        }

        public bool IsAllStateEnded()
        {
            foreach (State state in this.mStates.Values)
            {
                if (state.status != Status.Ended)
                    return false;
            }
            return true;
        }
        #endregion

        #region Flow Methods
        public State this[string name]
        {
            get
            {
                State value = null;
                this.mStates.TryGetValue(name, out value);
                return value;
            }
            set
            {
                this.mStates[name] = value;
            }
        }

        public void Begin(string name)
        {
            this.AssertState(name, true);
            this.mEntry = this[name];
        }

        public void End(string name)
        {
            this.AssertState(name, true);
            State exit = this[name];
            this.mExits.Add(exit);
        }

        public void Connect(string name1, string signal, string name2)
        {
            this.AssertState(name1, true);
            this.AssertState(name2, true);

            State state1 = this[name1];
            State state2 = this[name2];

            // Connect
            {
                state2.last = state1;
                List<State> nexts = null;
                if (!state1.next.TryGetValue(signal, out nexts))
                {
                    nexts = new List<State>();
                    state1.next.Add(signal, nexts);
                }
                if (!nexts.Contains(state2))
                {
                    nexts.Add(state2);
                }
            }
        }

        public void Disconnect(string name1, string signal, string name2)
        {
            this.AssertState(name1, true);
            this.AssertState(name2, true);

            State state1 = this[name1];
            State state2 = this[name2];

            // Disconnect
            if (state2.last == state1)
            {
                state2.last = null;
                List<State> nexts = null;
                if (state1.next.TryGetValue(signal, out nexts))
                {
                    nexts.Remove(state2);
                }
            }
        }

        private void AssertState(string name, bool exists)
        {
            if (this.mStates.ContainsKey(name) != exists)
            {
                string message = exists ?
                    "The state named '" + name + "' is undefined in state-machine." :
                    "The state named '" + name + "' has already defined in state-machine.";
                throw new Exception(message);
            }
        }

        private void AssertConnection(State state1, State state2, bool exists)
        {
            bool found = state2.last != null;
            if (found != exists)
            {
                string message = exists ?
                    "The transition to '" + state2.name + "' is undefined in state-machine." :
                    "The transition to '" + state2.name + "' has already defined in state-machine.";
                throw new Exception(message);
            }
        }

        public void Clear()
        {
            this.mStates.Clear();
            this.mEntry = null;
            this.mExits.Clear();
            this.mStatus = Status.None;
        }
        #endregion
    }
    #endregion
}

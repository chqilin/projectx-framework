using System;
using System.Collections.Generic;

namespace ProjectX
{
    #region Automatic FSM
    public class CFSM
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

            #region Life Circle
            public virtual void Init(StateParam param)
            {
                param.time = 0;
                param.signal = "";
                param.status = Status.Inited;
            }

            public virtual void Start(StateParam param)
            {
                param.status = Status.Running;
            }

            public virtual void Update(StateParam param, float elapse)
            {
                param.time += elapse;
                param.status = Status.Running;
            }

            public virtual void Stop(StateParam param)
            {
                param.status = Status.Ended;
            }

            public virtual void Quit(StateParam param)
            {
                param.signal = "";
                param.status = Status.None;
            }
            #endregion

            #region Runtime Methods
            public void ReadyAllNextStates(Context context, string signal)
            {
                List<State> states = null;
                this.next.TryGetValue(signal, out states);
                if (states == null || states.Count == 0)
                    return;
                foreach (var state in states)
                {
                    var param = context[state];
                    if (param == null)
                        continue;
                    if (param.status != Status.None)
                        continue;
                    state.Init(param);
                }
            }
            #endregion
        }

        public class StateParam
        {
            public Status status = Status.None;
            public float time = 0;
            public string signal = "";
        }

        public class Context
        {
            public Status status = Status.None;

            public Dictionary<State, StateParam> stateParams = new Dictionary<State, StateParam>();

            public StateParam this[State state]
            {
                get
                {
                    StateParam value = null;
                    this.stateParams.TryGetValue(state, out value);
                    return value;
                }
                set
                {
                    this.stateParams[state] = value;
                }
            }
        }
        #endregion

        private Dictionary<string, State> mStates = new Dictionary<string, State>();
        private State mEntry = null;
        private List<State> mExits = new List<State>();

        #region Virtual Life Circle
        public virtual void Init(Context context)
        {
            foreach (State state in this.mStates.Values)
            {
                StateParam param = new StateParam();
                context[state] = param;
            }

            context.status = Status.Inited;
        }

        public virtual void Start(Context context)
        {
            if (this.mEntry == null)
                return;

            var param = context[this.mEntry];
            if (param == null)
                return;

            this.mEntry.Init(param);

            context.status = Status.Running;
        }

        public virtual void Update(Context context, float elapse)
        {
            foreach (State state in this.mStates.Values)
            {
                var param = context[state];
                if (param == null)
                    continue;
                if (string.IsNullOrEmpty(param.signal))
                    continue;
                state.ReadyAllNextStates(context, param.signal);
                param.signal = "";
            }

            foreach (State state in this.mStates.Values)
            {
                var param = context[state];
                if (param == null)
                    continue;

                switch (param.status)
                {
                    case Status.Inited:
                        param.signal = "Start";
                        state.Start(param);
                        break;
                    case Status.Running:
                        state.Update(param, elapse);
                        break;
                    case Status.Completed:
                        param.signal = "Stop";
                        state.Stop(param);
                        break;
                }
            }

            if (this.IsAnyExitStateEnded(context) || this.IsAllStateEnded(context))
            {
                context.status = Status.Completed;
            }
        }

        public virtual void Stop(Context context)
        {
            context.status = Status.Ended;
        }

        public virtual void Quit(Context context)
        {
            context.status = Status.None;
        }
        #endregion

        #region Runtime Methods
        public bool IsAnyExitStateEnded(Context context)
        {
            foreach (State state in this.mExits)
            {
                var param = context[state];
                if (param == null)
                    continue;
                if (param.status == Status.Ended)
                    return true;
            }
            return false;
        }

        public bool IsAllStateEnded(Context context)
        {
            foreach (State state in this.mStates.Values)
            {
                var param = context[state];
                if (param == null)
                    continue;
                if (param.status != Status.Ended)
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

        public void Clear()
        {
            this.mStates.Clear();
            this.mEntry = null;
            this.mExits.Clear();
        }
        
        public void AssertState(string name, bool exists)
        {
            if (this.mStates.ContainsKey(name) != exists)
            {
                string message = exists ?
                    "The state named '" + name + "' is undefined in state-machine." :
                    "The state named '" + name + "' has already defined in state-machine.";
                throw new Exception(message);
            }
        }

        public void AssertConnection(State state1, State state2, bool exists)
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
        #endregion
    }
    #endregion
}

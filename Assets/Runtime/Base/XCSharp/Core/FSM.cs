using System;
using System.Collections.Generic;

namespace ProjectX
{
    #region Simple FSM
    public class SimpleFSM
    {
        #region Internal Types
        public abstract class State
        {
            public SimpleFSM machine { get; private set; }
            public float time { get; private set; }

            public virtual void Init(SimpleFSM machine)
            {
                this.machine = machine;
                this.time = 0;
            }

            public virtual void Quit()
            {
                this.machine = null;
                this.time = 0;
            }

            public virtual void Update(float elapse)
            {
                this.time += elapse;
            }
        }
        #endregion

        public Action<State, State> onChangeState;

        public State last { get; private set; }
        public State current { get; private set; }
        public State next {get; private set;}

        #region Virtual Life Circle
        public virtual void Init()
        { }

        public virtual void Quit()
        {
            this.Clear();
        }

        public virtual void Update(float elapse)
        {
            if (this.next != null)
            {
                if (this.current != null)
                {
                    this.current.Quit();
                }

                this.current = this.next;
                this.next = null;
                XCSharp.InvokeAction(this.onChangeState, this.last, this.current);

                if (this.current != null)
                {
                    this.current.Init(this);
                }
            }
            else
            {
                if (this.current != null)
                {
                    this.current.Update(elapse);
                }
            }
        }
        #endregion

        #region Public Methods
        public void Start(State state)
        {
            this.next = state;
        }

        public void Clear()
        {
            this.last = null;
            this.current = null;
            this.next = null;
        }
        #endregion
    }
    #endregion

    #region Subjected Simple FSM
    public class SimpleFSM<Subject> where Subject : class
    {
        #region Internal Types
        public abstract class State
        {
            public SimpleFSM<Subject> machine {get; private set;}
            public Subject subject {get; private set;}
            public float time {get; private set;}

            public virtual void Init(SimpleFSM<Subject> machine)
            {
                this.machine = machine;
                this.subject = machine.subject;
                this.time = 0;
            }

            public virtual void Quit()
            { 
                this.machine = null;
                this.subject = null;
                this.time = 0;
            }

            public virtual void Update(float elapse)
            {
                this.time += elapse;
            }
        }
        #endregion

        public Action<State, State> onChangeState;

        public Subject subject { get; private set; }
        public State last { get; private set; }
        public State current { get; private set; }
        public State next { get; private set; }

        #region Virtual Life Circle
        public virtual void Init(Subject subject)
        {
            this.subject = subject;
        }

        public virtual void Quit()
        {
            this.Clear();
        }

        public virtual void Update(float elapse)
        {
            if (this.next != null)
            {
                if (this.current != null)
                {
                    this.current.Quit();
                }

                this.current = this.next;
                this.next = null;
                XCSharp.InvokeAction(this.onChangeState, this.last, this.current);

                if (this.current != null)
                {
                    this.current.Init(this);
                }
            }
            else
            {
                if (this.current != null)
                {
                    this.current.Update(elapse);
                }
            }
        }
        #endregion

        #region Public Methods
        public void Start(State state)
        {
            this.next = state;
        }

        public void Clear()
        {
            this.last = null;
            this.current = null;
            this.next = null;
        }
        #endregion
    }
    #endregion

    #region Automatic FSM
    public class AutoFSM
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

            #region Public 
            public void Complete()
            {
                this.status = Status.Completed;
            }

            public void ConnectTo(string signal, State state)
            {
                state.last = this;
                List<State> nexts = null;
                if (!this.next.TryGetValue(signal, out nexts))
                {
                    nexts = new List<State>();
                    this.next.Add(signal, nexts);
                }
                if (!nexts.Contains(state))
                {
                    nexts.Add(state);
                }
            }

            public void DisconnectFrom(string signal, State state)
            {
                if (state.last != this)
                    return;
                state.last = null;
                List<State> nexts = null;
                if (this.next.TryGetValue(signal, out nexts))
                {
                    nexts.Remove(state);
                }
            }

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

            if (this.IsExitStateEnded() || this.IsAllStateEnded())
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

        #region Public Methods
        public void SetState(string name, State state)
        {
            this.mStates[name] = state;
        }

        public State GetState(string name)
        {
            State state = null;
            this.mStates.TryGetValue(name, out state);
            return state;
        }

        public void Begin(string name)
        {
            this.AssertState(name, true);
            this.mEntry = this.GetState(name);
        }

        public void End(string name)
        {
            this.AssertState(name, true);
            State exit = this.GetState(name);
            this.mExits.Add(exit);
        }

        public void Connect(string name1, string signal, string name2)
        {
            this.AssertState(name1, true);
            this.AssertState(name2, true);

            State state1 = this.GetState(name1);
            State state2 = this.GetState(name2);

            state1.ConnectTo(signal, state2);
        }

        public void Disconnect(string name1, string signal, string name2)
        {
            this.AssertState(name1, true);
            this.AssertState(name2, true);

            State state1 = this.GetState(name1);
            State state2 = this.GetState(name2);

            state1.DisconnectFrom(signal, state2);
        }

        public void Clear()
        {
            this.mStates.Clear();
            this.mEntry = null;
            this.mExits.Clear();
            this.mStatus = Status.None;
        }
        #endregion

        #region Private Methods
        private bool IsExitStateEnded()
        {
            foreach (State state in this.mExits)
            {
                if (state.status == Status.Ended)
                    return true;
            }
            return false;
        }

        private bool IsAllStateEnded()
        {
            foreach (State state in this.mStates.Values)
            {
                if (state.status != Status.Ended)
                    return false;
            }
            return true;
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
        #endregion
    }
    #endregion
}

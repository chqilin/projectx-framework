using System;
using System.Collections.Generic;

namespace ProjectX
{
    #region Simple FSM
    public class FSM
    {
        #region Internal Types
        public abstract class State
        {
            public FSM machine { get; private set; }
            public float time { get; private set; }

            public virtual void Init(FSM machine)
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

        public virtual void Tick(float elapse)
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
    public class FSM<Subject> where Subject : class
    {
        #region Internal Types
        public abstract class State
        {
            public FSM<Subject> machine {get; private set;}
            public Subject subject {get; private set;}
            public float time {get; private set;}

            public virtual void Init(FSM<Subject> machine)
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

            public virtual void Tick(float elapse)
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

        public virtual void Tick(float elapse)
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
                    this.current.Tick(elapse);
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
}

using System;
using System.Collections;

namespace ProjectX
{
    public class XEvent
    {
        private Action mOnEvent = null;

        public void Invoke()
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent();
            }
        }

        public void Attach(Action handler)
        {
            this.mOnEvent += handler;
        }

        public void Detach(Action handler)
        {
            this.mOnEvent -= handler;
        }
    }

    public class XEvent<Arg>
    {
        private Action<Arg> mOnEvent = null;

        public void Invoke(Arg arg)
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent(arg);
            }
        }

        public void Attach(Action<Arg> handler)
        {
            this.mOnEvent += handler;
        }

        public void Detach(Action<Arg> handler)
        {
            this.mOnEvent -= handler;
        }
    }

    public class XEvent<Arg1, Arg2>
    {
        private Action<Arg1, Arg2> mOnEvent = null;

        public void Invoke(Arg1 arg1, Arg2 arg2)
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent(arg1, arg2);
            }
        }

        public void Attach(Action<Arg1, Arg2> handler)
        {
            this.mOnEvent += handler;
        }

        public void Detach(Action<Arg1, Arg2> handler)
        {
            this.mOnEvent -= handler;
        }
    }

    public class XEvent<Arg1, Arg2, Arg3>
    {
        private Action<Arg1, Arg2, Arg3> mOnEvent = null;

        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3)
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent(arg1, arg2, arg3);
            }
        }

        public void Attach(Action<Arg1, Arg2, Arg3> handler)
        {
            this.mOnEvent += handler;
        }

        public void Detach(Action<Arg1, Arg2, Arg3> handler)
        {
            this.mOnEvent -= handler;
        }
    }

    public class XEvent<Arg1, Arg2, Arg3, Arg4>
    {
        private Action<Arg1, Arg2, Arg3, Arg4> mOnEvent = null;

        public void Invoke(Arg1 arg1, Arg2 arg2, Arg3 arg3, Arg4 arg4)
        {
            if (this.mOnEvent != null)
            {
                this.mOnEvent(arg1, arg2, arg3, arg4);
            }
        }

        public void Attach(Action<Arg1, Arg2, Arg3, Arg4> handler)
        {
            this.mOnEvent += handler;
        }

        public void Detach(Action<Arg1, Arg2, Arg3, Arg4> handler)
        {
            this.mOnEvent -= handler;
        }
    }
}


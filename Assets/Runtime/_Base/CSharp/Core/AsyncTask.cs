using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public partial class AsyncTask
    {
        public abstract class AsyncState
        {
            public bool done { get; protected set; }

            public virtual void Run(float elapse)
            { }
        }

        public class WaitForSeconds : AsyncState
        {
            private float mTime = 0;
            private float mCumu = 0;

            public WaitForSeconds(float seconds)
            {
                this.mTime = seconds;
                this.mCumu = 0;
            }

            public override void Run(float elapse)
            {
                base.Run(elapse);
                if (this.done)
                    return;
                this.mCumu += elapse;
                if (this.mCumu >= this.mTime)
                {
                    this.done = true;
                }
            }
        }
    }

    public partial class AsyncTask
    {
        public bool cancel { get; set; }
        public bool done { get; private set; }
        public bool waiting { get; private set; }
        
        private IEnumerator mEnumrator = null;

        private AsyncTask(IEnumerator func)
        {
            this.mEnumrator = func;
        }

        private bool Exec(object value, float elapse)
        {
            if (value == null)
                return true;

            if (value is AsyncTask)
            {
                AsyncTask subtask = value as AsyncTask;
                this.waiting = !subtask.done;
                return true;
            }

            if (value is AsyncState)
            {
                AsyncState state = value as AsyncState;
                state.Run(elapse);
                this.waiting = !state.done;
                return true;
            }

            return false;
        }
    }

    public partial class AsyncTask
    {
        private static List<AsyncTask> msReadyTasks = new List<AsyncTask>();
        private static List<AsyncTask> msEndedTasks = new List<AsyncTask>();
        private static List<AsyncTask> msRunningTasks = new List<AsyncTask>();

        public static AsyncTask Start(IEnumerator func)
        {
            AsyncTask task = new AsyncTask(func);
            msReadyTasks.Add(task);
            return task;
        }

        public static void Stop(AsyncTask task)
        {
            task.done = true;
        }

        public static void Update(float elapse)
        {
            // Add new task to running-queue
            msRunningTasks.AddRange(msReadyTasks);
            msReadyTasks.Clear();

            foreach (AsyncTask task in msRunningTasks)
            {
                // stop canceled task.
                if (task.cancel)
                {
                    msEndedTasks.Add(task);
                    continue;
                }

                // stop finished task.
                if (task.done)
                {
                    msEndedTasks.Add(task);
                    continue;
                }

                // if not sleeping, move to next fragment.
                if (!task.waiting)
                {
                    if (!task.mEnumrator.MoveNext())
                    {
                        task.done = true;
                        continue;
                    }
                }

                // get current async-state and exec it.
                object value = task.mEnumrator.Current;
                if (!task.Exec(value, elapse))
                {
                    throw new System.Exception("Unexpected Async State.");
                }
            }

            // remove ended-task from running-queue.
            foreach (AsyncTask task in msEndedTasks)
            {
                msRunningTasks.Remove(task);
            }
            msEndedTasks.Clear();
        }
    }
}

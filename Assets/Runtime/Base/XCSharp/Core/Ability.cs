using System.Collections.Generic;

namespace ProjectX
{
    public class Ability
    {
        private List<object> mMuteces = new List<object>();//
        private List<object> mLocks = new List<object>();

        public bool canbelocked
        {
            get { return this.mMuteces.Count == 0; }
        }

        public bool locked
        {
            get { return this.mLocks.Count > 0; }
        }

        public void AddMutex(object owner)
        {
            this.mMuteces.Add(owner);
        }

        public void RemoveMutex(object owner = null)
        {
            if (owner == null)
                this.mMuteces.Clear();
            else
                this.mMuteces.RemoveAll(o => o == owner);
        }

        public bool AddLock(object owner)
        {
            if (!this.canbelocked)
                return false;
            this.mLocks.Add(owner);
            return true;
        }

        public void RemoveLock(object owner = null)
        {
            if (owner == null)
                this.mLocks.Clear();
            else
                this.mLocks.RemoveAll(o => o == owner);
        }
    }
}

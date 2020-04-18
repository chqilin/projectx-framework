using System.Collections;
using System.Collections.Generic;

namespace ProjectX
{
    public abstract class SerialNumber
    {
        protected int mValue;

        public SerialNumber()
        {
            this.mValue = 0;
        }
        ~SerialNumber()
        {
            this.mValue = 0;
        }

        public static implicit operator int(SerialNumber serialNumber)
        {
            return serialNumber.mValue;
        }
    }

    public class GlobalUniqueSerialNumber : SerialNumber
    {
        private static int msSerialNumber;

        static GlobalUniqueSerialNumber()
        {
            msSerialNumber = 0;
        }

        public GlobalUniqueSerialNumber()
        {
            if (msSerialNumber < int.MaxValue)
            {
                this.mValue = ++msSerialNumber;
            }
            else
            {
                throw new System.Exception("There's no more serial numbers.");
            }
        }
    }

    public class GlobalReusedSerialNumber : SerialNumber
    {
        private static int msSerialNumber;
        private static LinkedList<int> msFreeNumbers;

        static GlobalReusedSerialNumber()
        {
            msSerialNumber = 0;
            msFreeNumbers = new LinkedList<int>();
        }

        public GlobalReusedSerialNumber()
        {
            if (msFreeNumbers.Count > 0)
            {
                this.mValue = msFreeNumbers.First.Value;
                msFreeNumbers.RemoveFirst();
            }
            else
            {
                if (msSerialNumber < int.MaxValue)
                {
                    this.mValue = ++msSerialNumber;
                }
                else
                {
                    throw new System.Exception("There's no more serial numbers.");
                }
            }
        }
        ~GlobalReusedSerialNumber()
        {
            msFreeNumbers.AddLast(this.mValue);
            this.mValue = 0;
        }
    }

    public class ScopedUniqueSerialNumber : SerialNumber
    {
        class Scope
        {
            public int serialNumber = 0;
        }

        private static Dictionary<int, Scope> msSerialNumbers = new Dictionary<int, Scope>();

        public ScopedUniqueSerialNumber(int scope)
        {
            Scope s = null;
            msSerialNumbers.TryGetValue(scope, out s);
            if (s == null)
            {
                s = new Scope();
                msSerialNumbers[scope] = s;
            }

            if (s.serialNumber < int.MaxValue)
            {
                this.mValue = ++s.serialNumber;
            }
            else
            {
                throw new System.Exception("There's no more serial numbers.");
            }
        }
    }

    public class ScopedReusedSerialNumber : SerialNumber
    {
        class Scope
        {
            public int serialNumber = 0;
            public LinkedList<int> freeNumbers = new LinkedList<int>();
        }

        private static Dictionary<int, Scope> msSerialNumbers;

        private Scope mScope;

        static ScopedReusedSerialNumber()
        {
            msSerialNumbers = new Dictionary<int, Scope>();
        }

        public ScopedReusedSerialNumber(int scope)
        {
            this.mScope = null;
            msSerialNumbers.TryGetValue(scope, out this.mScope);
            if (this.mScope == null)
            {
                this.mScope = new Scope();
                msSerialNumbers[scope] = this.mScope;
            }

            if (this.mScope.freeNumbers.Count > 0)
            {
                this.mValue = this.mScope.freeNumbers.First.Value;
                this.mScope.freeNumbers.RemoveFirst();
            }
            else
            {
                if (this.mScope.serialNumber < int.MaxValue)
                {
                    this.mValue = ++this.mScope.serialNumber;
                }
                else
                {
                    throw new System.Exception("There's no more serial numbers.");
                }
            }
        }
        ~ScopedReusedSerialNumber()
        {
            this.mScope.freeNumbers.AddLast(this.mValue);
            this.mValue = 0;
        }
    }
}





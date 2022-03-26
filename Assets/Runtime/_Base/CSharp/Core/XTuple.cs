
namespace ProjectX
{
    public class XTuple<T1>
    {
        public T1 Value1;

        public XTuple()
        { }
        public XTuple(T1 v1)
        {
            this.Value1 = v1;
        }
    }

    public class XTuple<T1, T2>
    {
        public T1 Value1;
        public T2 Value2;

        public XTuple()
        { }
        public XTuple(T1 v1, T2 v2)
        {
            this.Value1 = v1;
            this.Value2 = v2;
        }
    }

    public class XTuple<T1, T2, T3>
    {
        public T1 Value1;
        public T2 Value2;
        public T3 Value3;

        public XTuple()
        { }
        public XTuple(T1 v1, T2 v2, T3 v3)
        {
            this.Value1 = v1;
            this.Value2 = v2;
            this.Value3 = v3;
        }
    }

    public class XTuple<T1, T2, T3, T4>
    {
        public T1 Value1;
        public T2 Value2;
        public T3 Value3;
        public T4 Value4;

        public XTuple()
        { }
        public XTuple(T1 v1, T2 v2, T3 v3, T4 v4)
        {
            this.Value1 = v1;
            this.Value2 = v2;
            this.Value3 = v3;
            this.Value4 = v4;
        }
    }

    public class XTuple<T1, T2, T3, T4, T5>
    {
        public T1 Value1;
        public T2 Value2;
        public T3 Value3;
        public T4 Value4;
        public T5 Value5;

        public XTuple()
        { }
        public XTuple(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
        {
            this.Value1 = v1;
            this.Value2 = v2;
            this.Value3 = v3;
            this.Value4 = v4;
            this.Value5 = v5;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ProjectX
{
    public static class AsyncOperationExtensions
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}

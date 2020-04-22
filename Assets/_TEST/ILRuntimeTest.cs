using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILRuntimeTest : MonoBehaviour
{
    ILRuntime.Runtime.Enviorment.AppDomain appdomain = null;

    void Start()
    {
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
    }
}

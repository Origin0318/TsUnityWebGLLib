using System.Collections;
using System.Collections.Generic;
using Puerts;
using UnityEngine;

namespace GT
{
    public class GTJSManagerInstance : MonoBehaviour
    {
        JsEnv jsEnv;

        void Awake()
        {
            jsEnv = GTJSManager.Instance.JsEnv;
        }

        void Update()
        {
            jsEnv.Tick();
        }
    }
}



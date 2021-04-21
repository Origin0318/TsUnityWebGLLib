using System;
using UnityEngine;
using Puerts;

public class ECSTest : MonoBehaviour
{
    static JsEnv jsEnv;

    void Start()
    {
        RunScript();
    }

     void RunScript()
    {
        if (jsEnv == null)
        {
            string debugPath = Application.dataPath + "/../TsProj/output/";
            jsEnv = new JsEnv(new DefaultLoader(debugPath));
            jsEnv.UsingAction<bool>();//toggle.onValueChanged用到
            //await jsEnv.WaitDebuggerAsync();
        }
       // await null;
        var init = jsEnv.Eval<Action<MonoBehaviour>>("const m = require('ECSTest'); ");

        if (init != null) init(this);

    }

    void Update()
    {
        jsEnv.Tick();
    }
}

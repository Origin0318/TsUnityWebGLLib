using System;
using UnityEngine;
using Puerts;

public class UIEvent : MonoBehaviour
{
    static JsEnv jsEnv;

    void Start()
    {
        RunScript();
    }

    async void RunScript()
    {
        if (jsEnv == null)
        {
            string debugPath = Application.dataPath + "/../TsProj/output/";
            jsEnv = new JsEnv(new DefaultLoader(debugPath), 8088);
            jsEnv.UsingAction<bool>();//toggle.onValueChanged用到
            await jsEnv.WaitDebuggerAsync();
        }

        var init = jsEnv.Eval<Action<MonoBehaviour>>("const m = require('UIEvent'); m.init;");

        if (init != null) init(this);

    }

    void Update()
    {
        jsEnv.Tick();
    }
}

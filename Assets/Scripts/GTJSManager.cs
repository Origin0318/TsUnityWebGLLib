using System.Collections;
using System.Collections.Generic;
using Puerts;
using UnityEngine;

namespace GT
{
    public class GTJSManager
    {
        private static GTJSManager mInstance;
        private JsEnv jsEnv;
        private GTJSLoader jsLoader;

        public JsEnv JsEnv
        {
            get => jsEnv;
        }

        public static GTJSManager Instance
        {
            get {
                if(mInstance == null)
                {
                    mInstance = new GTJSManager();
                    mInstance.Init();
                }
                return mInstance;
            }
        }

        public GTJSLoader JsLoader { get => jsLoader;  }

        public void Init()
        {
            if (JsEnv == null)
            {
                //string debugPath = System.IO.Path.Combine(Application.streamingAssetsPath, "TsProj");
                string debugPath = Application.dataPath + "Assets/" + "/TsProj/";
                jsLoader = new GTJSLoader(debugPath);
                jsEnv = new JsEnv(JsLoader);
            }
        }

        void Update()
        {
            jsEnv.Tick();
        }
    }
}



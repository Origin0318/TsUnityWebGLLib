using UnityEngine;
using Puerts;
using System;
using System.IO;

namespace GT
{
    public delegate void ModuleInit(GTJSBehaviour monoBehaviour);

    //只是演示纯用js实现MonoBehaviour逻辑的可能，
    //但从性能角度这并不是最佳实践，会导致过多的跨语言调用
    public class GTJSBehaviour : MonoBehaviour
    {
        public string Path;
        public string ModuleName;//可配置加载的js模块

        public Action JsAwake;
        public Action JsStart;
        public Action JsUpdate;
        public Action JsOnDestroy;

        void Awake()
        {
            string m = "const m = require('./" + Path + "" + "" + ModuleName + ".js'); m.init;";
            Debug.LogError(m);
            Debug.LogError(Directory.Exists(GTJSManager.Instance.JsLoader.debugRoot) );

            var init = GTJSManager.Instance.JsEnv.Eval<ModuleInit>(m);

            if (init != null) init(this);
            if (JsAwake != null) JsAwake();

        }

        void Start()
        {
            if (JsStart != null) JsStart();
        }

        void Update()
        {
            Debug.Log(Time.deltaTime);
            if (JsUpdate != null) JsUpdate();
        }

        void OnDestroy()
        {
            if (JsOnDestroy != null) JsOnDestroy();
            JsStart = null;
            JsUpdate = null;
            JsOnDestroy = null;
        }
    }
}
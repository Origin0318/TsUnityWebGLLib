using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class WebGLTest : MonoBehaviour
{
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string DLLNAME = "__Internal";
#else
    const string DLLNAME = "hyplugin";
#endif


    public Text txt;
    [DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern int AddNum(int a,int b);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestUI()
    {
        Debug.LogError("TestUI");
        txt.text = AddNum(2, 3).ToString();
    }
}

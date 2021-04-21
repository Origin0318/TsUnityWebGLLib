﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using System.Diagnostics;

namespace iBicha
{
	public class WebGLBuilder : PluginBuilderBase {

        public WebGLBuilder()
        {
            SetSupportedArchitectures(Architecture.AnyCPU);
        }

        public override bool IsAvailable
        {
            get
            {
                return Directory.Exists(GetEmscriptenLocation());
            }
        }
        
        public override void PreBuild (NativePlugin plugin, NativeBuildOptions buildOptions){
			base.PreBuild (plugin, buildOptions);

			if (buildOptions.BuildPlatform != BuildPlatform.WebGL) {
				throw new ArgumentException (string.Format(
					"BuildPlatform mismatch: expected:\"{0}\", current:\"{1}\"", BuildPlatform.WebGL, buildOptions.BuildPlatform));
			}

            ArchtectureCheck(buildOptions);

			//optimization level check

            if(EditorPlatform == RuntimePlatform.WindowsEditor)
            {
                if (!File.Exists(MinGW32MakeLocation))
                {
                    throw new ArgumentException("\"mingw32-make.exe\" not found. please check the settings.");
                }
            }
		}

		public override BackgroundProcess Build (NativePlugin plugin, NativeBuildOptions buildOptions)
		{
			StringBuilder cmakeArgs = GetBasePluginCMakeArgs (plugin);

            BuildType buildType;
            if (buildOptions.BuildType == BuildType.Default)
            {
                buildType = EditorUserBuildSettings.development ? BuildType.Debug : BuildType.Release;
            }
            else
            {
                buildType = buildOptions.BuildType;
            }
            AddCmakeArg(cmakeArgs, "CMAKE_BUILD_TYPE", buildType.ToString());

            AddCmakeArg(cmakeArgs, "WEBGL", "ON", "BOOL");
			cmakeArgs.AppendFormat ("-B{0} ", "WebGL");

			if (EditorPlatform == RuntimePlatform.WindowsEditor)
			{
				cmakeArgs.AppendFormat(string.Format("-G {0} ", "\"MinGW Makefiles\""));
				AddCmakeArg (cmakeArgs, "CMAKE_MAKE_PROGRAM", MinGW32MakeLocation, "FILEPATH");
			}
			else
			{
				cmakeArgs.AppendFormat(string.Format("-G {0} ", "\"Unix Makefiles\""));
			}

			//We need our own copy of the toolchain, because we need to pass --em-config to emcc.
			//args.Add(string.Format("-DCMAKE_TOOLCHAIN_FILE=\"{0}{1}\" ", GetEmscriptenLocation(), "/cmake/Modules/Platform/Emscripten.cmake"));
			AddCmakeArg (cmakeArgs, "CMAKE_TOOLCHAIN_FILE", CombineFullPath(plugin.buildFolder, "../CMake/Emscripten.cmake"), "FILEPATH");
			AddCmakeArg (cmakeArgs, "EMSCRIPTEN_ROOT_PATH", GetEmscriptenLocation(), "PATH");

			string emconfig = RefreshEmscriptenConfig(plugin.buildFolder);
			AddCmakeArg (cmakeArgs, "EM_CONFIG", emconfig, "FILEPATH");

			buildOptions.OutputDirectory = CombineFullPath (plugin.buildFolder, "WebGL");

			ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = CMakeHelper.CMakeLocation;
            startInfo.Arguments = cmakeArgs.ToString();
			startInfo.WorkingDirectory = plugin.buildFolder;

			BackgroundProcess backgroundProcess = new BackgroundProcess (startInfo);
			backgroundProcess.Name = string.Format ("Building \"{0}\" for {1}", plugin.Name, "WebGL");
			return backgroundProcess;

		}

		public override void PostBuild (NativePlugin plugin, NativeBuildOptions buildOptions)
		{
			base.PostBuild (plugin, buildOptions);

			string assetFile = CombinePath(
				AssetDatabase.GetAssetPath (plugin.pluginBinaryFolder),
				"WebGL", 
				string.Format("{0}.bc", plugin.Name));

			PluginImporter pluginImporter = PluginImporter.GetAtPath((assetFile)) as PluginImporter;
			if (pluginImporter != null) {
                SetPluginBaseInfo(plugin, buildOptions, pluginImporter);

                pluginImporter.SaveAndReimport ();
			}
		}

		private static string GetEmscriptenLocation()
		{
			return CombineFullPath(GetEditorLocation(), "PlaybackEngines/WebGLSupport/BuildTools/Emscripten");
		}

		public static string MinGW32MakeLocation
		{
            get
            {
                return EditorPrefs.GetString("MinGW32MakeLocation");
            }
            set
            {
                if (File.Exists(value))
                {
                    EditorPrefs.SetString("MinGW32MakeLocation", value);
                }
            }
        }


		private static string RefreshEmscriptenConfig(string buildFolder)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("LLVM_ROOT='{0}'\n", GetLLVMLocation());
			sb.Append("NODE_JS=['" + GetNodeLocation() + "','--stack_size=8192','--max-old-space-size=2048']\n");
			sb.AppendFormat("EMSCRIPTEN_ROOT='{0}'\n", GetEmscriptenLocation());
			sb.Append("SPIDERMONKEY_ENGINE=''\n");
			sb.Append("V8_ENGINE=''\n");
			sb.AppendFormat("BINARYEN_ROOT='{0}'\n", CombineFullPath(GetLLVMLocation(), "binaryen"));
			sb.Append("COMPILER_ENGINE=NODE_JS\n");
			sb.Append("JS_ENGINES=[NODE_JS]\n");
			sb.Append("JAVA=''");
			string path = CombineFullPath(buildFolder, "emscripten.config");
			File.WriteAllText(path, sb.ToString());
			return path;
		}

		private static string GetNodeLocation()
		{
			switch (EditorPlatform)
			{
			case RuntimePlatform.WindowsEditor:
				return CombineFullPath(GetToolsLocation(), "nodejs/node.exe");
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.LinuxEditor:
				return CombineFullPath(GetToolsLocation(), "nodejs/bin/node");
			default:
				throw new PlatformNotSupportedException("Unknown platform");
			}
		}

		private static string GetLLVMLocation()
		{
			switch (EditorPlatform)
			{
			case RuntimePlatform.WindowsEditor:
				return CombineFullPath(GetEmscriptenLocation(), "../Emscripten_FastComp_Win");
			case RuntimePlatform.OSXEditor:
				return CombineFullPath(GetEmscriptenLocation(), "../Emscripten_FastComp_Mac");
			case RuntimePlatform.LinuxEditor:
				return CombineFullPath(GetEmscriptenLocation(), "../Emscripten_FastComp_Linux");
			default:
				throw new PlatformNotSupportedException("Unknown platform");
			}
		}


	}
}

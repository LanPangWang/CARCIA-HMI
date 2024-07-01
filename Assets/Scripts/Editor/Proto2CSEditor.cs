using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

public class Proto2CSEditor : MonoBehaviour
{
    public string filename;

    // MenuItem 属性允许我们在 Unity 编辑器中添加一个菜单项
    [MenuItem("Tools/BasicsProto2CS")]
    public static void basicsProtoGenerate()
    {
        // 从 basics.proto 文件生成 C# 代码
        AllProto2CS("basics.proto");
    }

    [MenuItem("Tools/LocalizationProto2CS")]
    public static void LocalizationProtoGenerate()
    {
        // 从 localization.proto 文件生成 C# 代码
        AllProto2CS("localization.proto");
    }

    [MenuItem("Tools/PerceptionProto2CS")]
    public static void PerceptionProtoGenerate()
    {
        // 从 perception.proto 文件生成 C# 代码
        AllProto2CS("perception.proto");
    }

    [MenuItem("Tools/behavior_debugProto2CS")]
    public static void behavior_debugProtoGenerate()
    {
        // 从 behavior_debug.proto 文件生成 C# 代码
        AllProto2CS("behavior_debug.proto");
    }

    [MenuItem("Tools/planner_debugProto2CS")]
    public static void planner_debugProtoGenerate()
    {
        // 从 planner_debug.proto 文件生成 C# 代码
        AllProto2CS("planner_debug.proto");
    }

    [MenuItem("Tools/semantic_environmentProto2CS")]
    public static void semantic_environmentProtoGenerate()
    {
        // 从 semantic_environment.proto 文件生成 C# 代码
        AllProto2CS("semantic_environment.proto");
    }

    [MenuItem("Tools/simulation_worldProto2CS")]
    public static void simulation_worldProtoGenerate()
    {
        // 从 simulation_world.proto 文件生成 C# 代码
        AllProto2CS("simulation_world.proto");
    }

    // 该方法根据给定的 .proto 文件生成 C# 代码
    public static void AllProto2CS(string _FileName)
    {
        // 获取当前目录
        string rootDir = Environment.CurrentDirectory;

        // 组合路径以获取包含 .proto 文件的目录
        string protoDir = Path.Combine(rootDir, "Assets/Proto/xlab_proto/");
        string protoFile = _FileName;

        // 根据操作系统确定 protoc 可执行文件的路径
        string protoc;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            protoc = Path.Combine(rootDir, "Assets/Proto/protoc.exe");
        }
        else
        {
            protoc = Path.Combine(rootDir, "Assets/Proto/protoc").Replace("\\", "/");
        }


        // 设置生成的 C# 代码的输出路径
        string hotfixMessageCodePath = Path.Combine(rootDir, "Assets", "Scripts", "ProtoMessage/");

        // 构建 protoc 的参数字符串
        string argument2 = $"--csharp_out=\"{hotfixMessageCodePath}\" --proto_path=\"{protoDir}\" {protoFile}";

        // 运行 protoc 进程
        Run(protoc, argument2, workingDirectory: protoDir, waitExit: true);

        // 记录成功消息
        UnityEngine.Debug.Log("proto2cs succeed!");

        // 刷新 AssetDatabase 以在 Unity 编辑器中反映更改
        AssetDatabase.Refresh();
    }

    // 该方法运行一个带有给定可执行文件和参数的进程
    public static Process Run(string exe, string arguments, string workingDirectory = ".", bool waitExit = false)
    {
        UnityEngine.Debug.Log($"Executing command in directory: {workingDirectory}");
        UnityEngine.Debug.Log($"Command: {exe} {arguments}");
        try
        {
            bool redirectStandardOutput = true;
            bool redirectStandardError = true;
            bool useShellExecute = false;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                redirectStandardOutput = false;
                redirectStandardError = false;
                useShellExecute = true;
            }

            if (waitExit)
            {
                redirectStandardOutput = true;
                redirectStandardError = true;
                useShellExecute = false;
            }

            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = useShellExecute,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = redirectStandardOutput,
                RedirectStandardError = redirectStandardError,
            };

            Process process = Process.Start(info);

            if (waitExit)
            {
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!string.IsNullOrEmpty(output))
                {
                    UnityEngine.Debug.Log("Standard Output:");
                    UnityEngine.Debug.Log(output);
                }

                if (!string.IsNullOrEmpty(error))
                {
                    UnityEngine.Debug.Log("Standard Error:");
                    UnityEngine.Debug.LogError(error);
                }

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Process exited with code {process.ExitCode}");
                }
            }

            return process;
        }
        catch (Exception e)
        {
            throw new Exception($"Directory: {Path.GetFullPath(workingDirectory)}, Command: {exe} {arguments}", e);
        }
    }

}

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildLuaFileList : MonoBehaviour
{
    [MenuItem("Tools/Generate Lua File List")]
    public static void GenerateLuaFileList()
    {
        string srcDir = Path.Combine(Application.streamingAssetsPath, "HGF/xLua/xLuaDOTween");
        string outputPath = Path.Combine(srcDir, "lua_files.txt");

        if (!Directory.Exists(srcDir))
        {
            Debug.LogError("目录不存在: " + srcDir);
            return;
        }

        string[] files = Directory.GetFiles(srcDir, "*.lua", SearchOption.AllDirectories);
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            foreach (string file in files)
            {
                // 写入相对路径
                string relative = file.Replace(srcDir + Path.DirectorySeparatorChar, "");
                writer.WriteLine(relative);
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("已生成 lua_files.txt，共 " + files.Length + " 个文件");
    }
}
#endif
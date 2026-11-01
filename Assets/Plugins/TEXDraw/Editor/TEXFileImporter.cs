using UnityEngine;
#if UNITY_2019 || UNITY_2020_1
using UnityEditor.Experimental.AssetImporters;
#else
using UnityEditor.AssetImporters;
#endif
using System.IO;

[ScriptedImporter(1, "tex")]
public class TEXFileImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var text = new TextAsset(File.ReadAllText(ctx.assetPath));
        ctx.AddObjectToAsset("text", text);
        ctx.SetMainObject(text);
    }
}
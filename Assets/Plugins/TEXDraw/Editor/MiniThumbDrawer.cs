using UnityEditor;
using UnityEngine;

public class MiniThumbTextureDrawer : MaterialPropertyDrawer
{
    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        GUILayout.Space(2);
        Texture t = prop.textureValue;

        if (t)
        {
            var name = t.name;
            if (name == "Font Texture" && AssetDatabase.IsSubAsset(t))
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(AssetDatabase.GetAssetPath(t));
                if (font)
                {
                    name = font.name;
                }
            }
            prop.textureValue = editor.TexturePropertyMiniThumbnail(position, prop, label + " (" + name + ")", "RGBA Texture for " + label);
        }
        else
            prop.textureValue = editor.TexturePropertyMiniThumbnail(position, prop, label, "RGBA Texture for " + label);
    }
}

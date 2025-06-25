
using UnityEngine;

[ExecuteAlways]
public class TEXDrawRendererFactory : MonoBehaviour
{
    public void Start()
    {
        var tex = GetComponentInParent<TEXDraw>();
        if (tex)
        {
            var ren = gameObject.AddComponent<TEXDrawRenderer>();
            tex.RegisterRenderer(ren);
            ren.m_TEXDraw = tex;
            ren.raycastTarget = false;
            tex.SetVerticesDirty();
        }
        if (Application.IsPlaying(this))
        {
            Destroy(this);
        }
        else
        {
            DestroyImmediate(this);
        }
    }

}
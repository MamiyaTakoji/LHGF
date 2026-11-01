using TexDrawLib;

using UnityEngine;

// This component is invisible in scene, but plays a vital role rendering the fonts.
[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(RectTransform))]
public class TEXDraw3DRenderer : MonoBehaviour, ITexRenderer
{
    private const string assetID = "TEXDraw 3D Instance";
    public TEXDraw3D m_TEXDraw;
    public int m_FontMode = -1;
    public ITEXDraw TEXDraw => m_TEXDraw ? m_TEXDraw : (m_TEXDraw = GetComponentInParent<TEXDraw3D>());
    private Mesh workerMesh;
    private MaterialPropertyBlock m_block;
    private Texture2D whiteTex;

    public int FontMode
    {
        get => m_FontMode; set { m_FontMode = value; }
    }

    protected void OnEnable()
    {
        if (!workerMesh)
        {
            workerMesh = new Mesh();
            workerMesh.name = assetID;
            workerMesh.hideFlags = HideFlags.DontSave;
        }
        if (m_block == null)
            m_block = new MaterialPropertyBlock();
        if (!whiteTex)
            whiteTex = Texture2D.whiteTexture;
    }

    public void Repaint()
    {
        if (m_block == null)
        {
            OnEnable();
        }
        var renderer = GetComponent<MeshRenderer>();
        if (m_TEXDraw.material)
            renderer.material = m_TEXDraw.material;
        else
            renderer.material = mainMaterial;
        m_block.SetTexture("_MainTex", mainTexture);
        renderer.SetPropertyBlock(m_block);
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(workerMesh);
        else
#endif
            Destroy(workerMesh);
    }

    public Texture mainTexture
    {
        get
        {
#if TEXDRAW_TMP
            if (m_FontMode >= 1024)
            {
                var font = TEXDraw?.preference.fonts[m_FontMode % 1024] as TexFontSigned;
                return font ? font.Texture(m_FontMode / 1024) : whiteTex;
            }
#endif
            return (m_FontMode >= 0 ?
        TEXDraw?.preference.fonts[m_FontMode].Texture() : whiteTex);
        }
    }

    public Material mainMaterial => m_FontMode >= 0 ? TEXDraw?.preference.fonts[m_FontMode % 1024].Material() : TEXPreference.main.defaultMaterial;

    public void ForceRender()
    {
        Redraw();
    }

    public void Redraw()
    {
        if (m_FontMode == -1 || !m_TEXDraw)
        {
            workerMesh.Clear();
        }
        else if (rectTransform.rect.width <= TEXDraw.padding.horizontal || rectTransform.rect.height <= TEXDraw.padding.vertical)
        {
            workerMesh.Clear();
        }
        else
        {
            m_TEXDraw.orchestrator.rendererState
                .GetVertexForFont(m_FontMode)
                ?.FillMesh(workerMesh, false);
        }
        GetComponent<MeshFilter>().mesh = workerMesh;
    }

    private RectTransform _cRT;

    public RectTransform rectTransform => _cRT ? _cRT : (_cRT = GetComponent<RectTransform>());
}
using System.Collections;
using System.Linq;
using TexDrawLib;

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("TEXDraw/TEXInput Cursor")]
public class TEXInputCursor : MaskableGraphic
{
    public Color activeColor = new Color32(0x00, 0x77, 0xCC, 0x99);
    public Color groupColor = new Color32(0xCC, 0x77, 0x00, 0x99);
    public Color idleColor = new Color32(0x80, 0x80, 0x80, 0x00);
    public float cursorWidth = 2;
    public float cursorBlink = 0;

    private Coroutine blinkCoroutine = null;
    private float blinkStartTime = 0;
    private bool isBlinkTime = false;
    private bool isHot = false;
    private bool isActive = false;

    public TEXInput input;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
		vh.Clear();

        if (!input || !input.IsInteractable())
            return;

        if (!(isActive = input.hasFocus) && idleColor.a < 1e-4f)
            return;

        if (!isHot && isBlinkTime)
            return;

        var logger = input.logger;
        var param = input.tex.orchestrator;
        var start = input.selectionStart;
        var length = input.selectionLength;
        var links = param.rendererState.vertexLinks;
        var color = (Color32)(isActive ? activeColor : idleColor);
        var colorG = (Color32)(isActive ? groupColor : idleColor);
        var blocks = logger.GetBlockMatches(start, length).ToArray();

        if (links.Count == 0)
        {
            // no target to draw. guess it
            DrawQuad(vh, new Rect(param.rendererState.x - cursorWidth,
                param.rendererState.y - lineDepth, cursorWidth * 2,
                lineHeight), color);
        }
        else if (blocks.Length == 0 && logger.blocks.Count > 0)
        {
            var i = input.selectionStart;

            var b = logger.GetBlockClosest(i);

            if (b.index == -1)
            {
                // nothing found
                // var f = param.latestAtomCache.formulas[0];
                // Draw(vh, ExtractAreaOfLine(scale, f), input.selectionStart > 0);
            }
            //else if (b.lineSeparator >= 0 && b.start == i)
            //{
                // it's different
                // int s, e;
                // param.GetLineRange(b.lineSeparator, out s, out e);
                // var f = param.formulas[s + e - 1];
                // Draw(vh, ExtractAreaOfLine(scale, f), true);
            //}
            else if (b.length == 0 && b.start == i)
            {
                // placeholder
                DrawQuad(vh, links[b.index].area, color);
            }
            else if (i >= b.end)
            {
                if (b.group > 0)
                {
                    for (int j = b.index + 1; j < b.group; j++)
                    {
                        DrawQuad(vh, links[j].area, colorG);
                    }
                }
                Draw(vh, links[b.index].area, true);
            }
            else if (i >= b.start)
            {
                if (b.group > 0)
                {
                    for (int j = b.index + 1; j < b.group; j++)
                    {
                        DrawQuad(vh, links[j].area, colorG);
                    }
                }
                Draw(vh, links[b.index].area, false);
            }
        }
        else
        {
            // just simple block
            foreach (var b in blocks)
            {
                DrawQuad(vh, links[b.index].area, color);
            }
        }

    }

    // private Rect ExtractAreaOfLine(float scale, TexRenderer f)
    // {
    //     return new Rect(f.X * scale, (f.Y * scale - lineDepth), f.Width * scale, lineHeight);
    // }

    private void Draw(VertexHelper verts, Rect r, bool onTheRight)
    {
        DrawQuad(verts, new Rect(r.x - cursorWidth + (onTheRight ? r.width : 0),
            r.y, cursorWidth * 2, Mathf.Max(lineDepth + lineHeight, r.height)), isActive ? activeColor : idleColor);
    }

    private float lineHeight => input.tex.size * TEXPreference.main.configuration.Typeface.lineAscent * TEXPreference.main.configuration.Document.pixelsPerInch * (1 / 72.27f);

    private float lineDepth => input.tex.size * TEXPreference.main.configuration.Typeface.lineDescent * TEXPreference.main.configuration.Document.pixelsPerInch * (1 / 72.27f);

    public static void DrawQuad(VertexHelper vertex, Rect v, Color32 c)
    {
        var z = new Vector2();
        var s = vertex.currentVertCount;
        vertex.AddVert(new Vector3(v.xMin, v.yMin), c, z);
        vertex.AddVert(new Vector3(v.xMax, v.yMin), c, z);
        vertex.AddVert(new Vector3(v.xMax, v.yMax), c, z);
        vertex.AddVert(new Vector3(v.xMin, v.yMax), c, z);

        vertex.AddTriangle(s + 0, s + 1, s + 2);
        vertex.AddTriangle(s + 0, s + 2, s + 3);
    }

    public static Rect Union(Rect r1, Rect r2)
    {
        return Rect.MinMaxRect(Mathf.Min(r1.x, r2.x), Mathf.Min(r1.y, r2.y),
            Mathf.Max(r1.xMax, r2.xMax), Mathf.Max(r1.yMax, r2.yMax));
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        useLegacyMeshGeneration = false;
        if (cursorBlink > 0)
        {
            blinkCoroutine = StartCoroutine(CaretBlink());
        }
    }

    public bool hotState
    {
        get { return isHot; }
        set
        {
            isHot = value;
            blinkStartTime = Time.unscaledTime;
            isBlinkTime = false;
            if (blinkCoroutine == null)
                blinkCoroutine = StartCoroutine(CaretBlink());
        }
    }

    IEnumerator CaretBlink()
    {
        // Always ensure caret is initially visible since it can otherwise be confusing for a moment.
        isBlinkTime = false;
        blinkStartTime = Time.unscaledTime;
        yield return null;

        while (input.hasFocus && cursorBlink > 0)
        {
            // the caret should be ON if we are in the first half of the blink period
            bool blinkState = (Time.unscaledTime - blinkStartTime) % cursorBlink > cursorBlink / 2;
            if (isBlinkTime != blinkState)
            {
                isBlinkTime = blinkState;
                if (!isHot)
                    input.tex.SetTextDirty();
            }

            // Then wait again.
            yield return null;
        }
        blinkCoroutine = null;
    }

}


using System;
using UnityEngine;

namespace TexDrawLib
{
    public class TexOrchestrator
    {

        public TexModuleParser parser;
        public static TexModuleParser staticParser;
        public TexParserState parserState;
        public TexBoxingState boxingState;
        public TexRendererState rendererState;

        public Atom latestAtomCache;
        public Box latestBoxCache;

        public float signedCoeff = 0;
        public float initialSize = 16;
        public float fitScaling = 1;
        public int initialFont = -1;
        public Color32 initialColor = Color.white;
        public float pixelsPerInch = 96f;
        public Vector2 alignment = Vector2.zero;
        public Vector2Int autoSize = Vector2Int.zero;
        public Overflow overflowMode;

        public Rect canvasRect;
        public Rect clipRect;
        public Rect drawingRect;
        public TexRectOffset clipOffset;

        public Vector2 outputNativeCanvasSize;

        /// <summary>
        /// </summary>
        /// <param name="area"></param>
        /// <param name="scroll"></param>
        /// <param name="offset"></param>
        /// <returns>True if box size changed: Need to recalculate box</returns>
        public bool InputCanvasSize(Rect area, Rect scroll, TexRectOffset offset, Vector2Int auto, Overflow overflow)
        {
            drawingRect = area;
            area = offset.Shrink(area);

            clipRect = area;
            clipOffset = offset;

            if (scroll.width > area.width) area.width = scroll.width;
            if (scroll.height > area.height) area.height = scroll.height;

            area.x += scroll.x;
            area.y += scroll.y;

            // auto and scroll don't pair each other
            if (auto.x == 1)
            {
                area.width = 0;
                area.x = 0;
            }
            if (auto.y == 1)
            {
                area.y = 0;
                area.height = 0;
            }
            autoSize = auto;

            bool flag = false;
            if (canvasRect.size != area.size || overflowMode != overflow)
                flag = true;
            canvasRect = area;
            overflowMode = overflow;
            return flag;
        }

        public static void InvalidateParser()
        {
            if (staticParser == null)
            {
                return;
            }
            staticParser.Initialize();
        }

        public TexOrchestrator()
        {
            if (staticParser == null)
            {
                staticParser = new TexModuleParser();
            }
            parser = staticParser;
            parserState = new TexParserState();
            boxingState = new TexBoxingState();
            rendererState = new TexRendererState();
        }

        public void ResetParser()
        {
            latestAtomCache?.Flush();

            parserState.Reset();
            if (initialFont >= 0)
                parserState.Font.Reset(initialFont);
            parserState.Size.Reset(initialSize);
            parserState.Color.Reset(initialColor);
            parserState.Document.initialSize = initialSize;
            parserState.Paragraph.alignment = alignment.x;
            parserState.Document.pixelsPerInch = pixelsPerInch;
            parserState.parser = parser;
        }

        protected void ResetBoxing(float scale = 1)
        {
            latestBoxCache?.Flush();
            boxingState.Reset(canvasRect.size * scale, autoSize);
        }

        public void Parse(string input)
        {
            ResetParser();
            latestAtomCache = parser.Parse(input, parserState);
        }

        public void Box()
        {
            fitScaling = 1;
            ResetBoxing(1 / fitScaling);
            var clipSize = new Vector2(clipOffset.horizontal, clipOffset.vertical);
            var box = latestBoxCache = latestAtomCache.CreateBox(boxingState);
            outputNativeCanvasSize = box.getBoundingRect.size + clipSize;
            if (overflowMode == Overflow.DownScale)
            {
                if (autoSize.x == 0)
                    fitScaling = Mathf.Min(fitScaling, canvasRect.width / outputNativeCanvasSize.x);
                if (autoSize.y == 0)
                    fitScaling = Mathf.Min(fitScaling, canvasRect.height / outputNativeCanvasSize.y);
            }
            if (overflowMode == Overflow.BestFit)
            {
                while ((
                    (autoSize.x == 0 && outputNativeCanvasSize.y - clipSize.y > canvasRect.height / fitScaling + 1e-1) ||
                    (autoSize.y == 0 && outputNativeCanvasSize.x - clipSize.x > canvasRect.width / fitScaling + 1e-1)) &&
                    fitScaling > 0 && initialSize >= 1)
                {
                    fitScaling -= 1 / initialSize;
                    ResetBoxing(1 / fitScaling);
                    box = latestBoxCache = latestAtomCache.CreateBox(boxingState);
                    outputNativeCanvasSize = box.getBoundingRect.size + clipSize;
                }
            }
        }

        public void Render()
        {
            var box = latestBoxCache;
            var size = new Vector3(box.width, box.height, box.depth);
            rendererState.Initialize(canvasRect, size, alignment);
            rendererState.clipRect = clipRect;
            rendererState.scale = initialSize / TEXConfiguration.main.Document.nativeSize;
            rendererState.signedCoeff = signedCoeff;
            rendererState.clip = overflowMode == Overflow.Hidden;
            box.Draw(rendererState);
            if (fitScaling != 1)
            {
                var offset = new Vector3(
                    canvasRect.width * (1 - fitScaling) * -(0.5f - (overflowMode == Overflow.BestFit ? 0 : alignment.x)),
                    canvasRect.height * (1 - fitScaling) * -(0.5f - alignment.y));
                foreach (var vertex in rendererState.vertexes)
                    for (int i = 0; i < vertex.m_Positions.Count; i++)
                        vertex.m_Positions[i] = vertex.m_Positions[i] * fitScaling + offset;
            }
        }

        private static string TraceAtom(Atom atom, int depth)
        {
            var s = new string('\t', depth) + atom.ToString() + "\r\n";
            if (atom is LayoutAtom layout)
            {
                foreach (var item in layout.children)
                {
                    s += TraceAtom(item, depth + 1);
                }
            }
            else if (atom is BlockAtom block)
            {
                if (block.atom != null)
                    s += TraceAtom(block.atom, depth + 1);
            }
            else if (atom is AutoDelimitedGroupAtom delimiter)
            {
                if (delimiter.left != null)
                    s += TraceAtom(delimiter.left, depth + 1);
                if (delimiter.atom != null)
                    s += TraceAtom(delimiter.atom, depth + 1);
                if (delimiter.right != null)
                    s += TraceAtom(delimiter.right, depth + 1);
            }
            else if (atom is InlineAtom inline)
            {
                if (inline.atom != null)
                    s += TraceAtom(inline.atom, depth + 1);
            }
            else if (atom is InlineMacroAtom inlinem)
            {
                if (inlinem.atom != null)
                    s += TraceAtom(inlinem.atom, depth + 1);
            }
            else if (atom is ScriptsAtom scripts)
            {
                s += TraceAtom(scripts.atom, depth + 1);
                if (scripts.superscript != null)
                    s += TraceAtom(scripts.superscript, depth + 1);
                if (scripts.subscript != null)
                    s += TraceAtom(scripts.subscript, depth + 1);
            }
            else if (atom is FractionAtom fractions)
            {
                if (fractions.numerator != null)
                    s += TraceAtom(fractions.numerator, depth + 1);
                if (fractions.denominator != null)
                    s += TraceAtom(fractions.denominator, depth + 1);
            }
            else if (atom is BoxedAtom box)
            {
                s += TraceAtom(box.atom, depth + 1);
            }
            else if (atom is BoxedLinkAtom link)
            {
                s += TraceAtom(link.atom, depth + 1);
            }
            else if (atom is AccentedAtom accent)
            {
                s += TraceAtom(accent.accent, depth + 1);
                s += TraceAtom(accent.atom, depth + 1);
            }
            return s;
        }

        private static string TraceBox(Box box, int depth)
        {
            var s = new String('\t', depth) + box.ToString() + "\r\n";
            if (box is LayoutBox layout)
            {
                foreach (var item in layout.children)
                {
                    s += TraceBox(item, depth + 1);
                }
            }
            else if (box is LinkBox link)
            {
                s += TraceBox(link.box, depth + 1);
            }
            return s;
        }

        public string Trace()
        {
            string s = "";
            if (latestAtomCache != null)
            {
                s += TraceAtom(latestAtomCache, 0);
            }
            s += "\r\n";
            if (latestBoxCache != null)
            {
                s += TraceBox(latestBoxCache, 0);
            }
            return s;
        }
    }
}

using UnityEngine;
using static TexDrawLib.TexParserUtility;
// Atom representing scripts to attach to other atom.
namespace TexDrawLib
{
    public class ScriptsAtom : DisplayAtom
    {
        public override CharType Type => atom != null ? atom.Type : CharTypeInternal.Invalid;
        public override CharType LeftType => atom != null ? atom.LeftType : CharTypeInternal.Invalid;
        public override CharType RightType => atom != null ? atom.RightType : CharTypeInternal.Invalid;

        public static Atom RetrieveBaseScript(LayoutAtom atom)
        {
            if (atom is ParagraphAtom para && para.charBuildingBlock.Count > 0)
            {
                para.CleanupWord();
            }
            if (atom.children.Count > 0)
            {
                var last = atom.children.Count - 1;
                var a = atom.children[last];
                atom.children.RemoveAt(last);
                return a;
            }

            return null;
        }

        public static ScriptsAtom Get()
        {
            return ObjPool<ScriptsAtom>.Get();
        }

        public static ScriptsAtom Get(Atom atom)
        {
            var a = ObjPool<ScriptsAtom>.Get();
            a.atom = atom;
            return a;
        }

        public Atom atom;

        public Atom subscript, superscript;

        protected float supDrop, supCrampedDrop, marginH, subDrop, marginW, marginD, lineHeight, lineDepth, lineMedian;
        protected bool crampedMode;

        public override Box CreateBox(TexBoxingState state)
        {
            if (subscript == null && superscript == null)
                return (atom == null ? StrutBox.Empty : atom.CreateBox(state));

            Box baseBox = (atom == null ? StrutBox.Empty : atom.CreateBox(state));

            HorizontalBox resultBox = HorizontalBox.Get(baseBox);
            Box superscriptBox = null, subscriptBox = null;
            HorizontalBox superscriptContainerBox = null;
            HorizontalBox subscriptContainerBox = null;

            // Set delta value and preliminary shift-up and shift-down amounts depending on type of base atom.
            float shiftUp = Mathf.Max(baseBox.height - (lineHeight - lineMedian), lineMedian) - (crampedMode ? supCrampedDrop : supDrop);
            if (superscript != null)
            {
                // Create box for superscript atom.
                superscriptBox = superscript.CreateBox(state);
                superscriptContainerBox = HorizontalBox.Get(superscriptBox);

                // Add box for script space.
                superscriptContainerBox.Add(0, StrutBox.Get(marginW, 0, 0));

                shiftUp = Mathf.Max(shiftUp, marginH);
            }

            float shiftDown = Mathf.Max(baseBox.depth, lineDepth) - subDrop;

            if (subscript != null)
            {
                // Create box for subscript atom.
                subscriptBox = subscript.CreateBox(state);
                subscriptContainerBox = HorizontalBox.Get(subscriptBox);

                // Add box for script space.
                subscriptContainerBox.Add(0, StrutBox.Get(marginW, 0, 0));

                shiftDown = Mathf.Max(shiftDown, marginD);
            }

            // Check if only superscript is set.
            if (subscriptBox == null)
            {
                superscriptContainerBox.shift = -shiftUp;
                resultBox.Add(superscriptContainerBox);
                resultBox.height = shiftUp + superscriptBox.height;
                return resultBox;
            }

            // Check if only subscript is set.
            if (superscriptBox == null)
            {
                subscriptBox.shift = shiftDown;
                resultBox.Add(subscriptContainerBox);
                resultBox.depth = shiftDown + subscriptBox.depth;
                return resultBox;
            }

            // Adjust shift-down amount.
            //shiftDown = Mathf.Max(shiftDown, TEXConfiguration.main.SubMinOnSup * TexContext.Scale);

            // Space between subscript and superscript.
            float scriptsInterSpace = shiftUp - superscriptBox.depth + shiftDown - subscriptBox.height;

            // If baseAtom is null, make it right-aligned

            // Create box containing both superscript and subscript.
            var scriptsBox = VerticalBox.Get();
            scriptsBox.Add(superscriptContainerBox);
            scriptsBox.Add(StrutBox.Get(0, scriptsInterSpace, 0));
            scriptsBox.Add(subscriptContainerBox);
            scriptsBox.height = shiftUp + superscriptBox.height;
            scriptsBox.depth = shiftDown + subscriptBox.depth;
            scriptsBox.shift = baseBox.shift;
            resultBox.Add(scriptsBox);

            return resultBox;
        }


        public override void Flush()
        {
            ObjPool<ScriptsAtom>.Release(this);

            atom?.Flush();
            atom = null;
            superscript?.Flush();
            superscript = null;
            subscript?.Flush();
            subscript = null;

            supDrop = subDrop = marginW = marginH = marginD = supCrampedDrop = 0;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            supDrop = state.Math.scriptAscentOffset * r;
            supCrampedDrop = state.Math.scriptAscentCrampedOffset * r;
            subDrop = state.Math.scriptDescentOffset * r;
            marginW = state.Math.scriptHorizontalMargin * r;
            marginH = state.Math.scriptHeightMargin * r;
            marginD = state.Math.scriptDepthMargin * r;
            crampedMode = state.Environment.current.GetMathStyle().IsCrampedStyle();
            lineHeight = state.Typeface.lineAscent * r;
            lineMedian = state.Typeface.lineMedian * r;
            lineDepth = state.Typeface.lineDescent * r;

            if (value == null) return;
            while (position < value.Length && (value[position] == superScriptChar || value[position] == subScriptChar))
            {
                bool super = value[position] == superScriptChar;
                if ((super ? superscript : subscript) != null)
                    break;

                ProcessIndividual(super, state, value, ref position);
            }
        }

        private void ProcessIndividual(bool super, TexParserState state, string value, ref int position)
        {
            position++;

            if (position < value.Length && (value[position] == value[position - 1]))
            {
                position++;
            }

            state.PushMathStyle(x => super ? x.GetSuperscriptStyle() : x.GetSubscriptStyle());

            var atom = state.parser.ParseToken(value, state, ref position);

            state.PopMathStyle();

            if (super)
                superscript = atom;
            else
                subscript = atom;
        }
    }
}

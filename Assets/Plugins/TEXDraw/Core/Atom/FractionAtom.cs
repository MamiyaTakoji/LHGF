using System;
using UnityEngine;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public class FractionAtom : DisplayAtom
    {
        public override CharType Type => CharTypeInternal.Inner;

        public Atom numerator;
        public Atom denominator;
        public bool line = true;

        float thickness, gapUp, gapDown, margin, lineMedian;
        Color32 color;

        public override Box CreateBox(TexBoxingState state)
        {

            // Create boxes for numerator and demoninator atoms, and make them of equal width.
            var numeratorBox = numerator?.CreateBox(state) ?? StrutBox.Empty;
            var denominatorBox = denominator?.CreateBox(state) ?? StrutBox.Empty;

            float maxWidth = Math.Max(numeratorBox.width, denominatorBox.width) + margin;
            numeratorBox = HorizontalBox.Get(numeratorBox, maxWidth, TexAlignment.Center);
            denominatorBox = HorizontalBox.Get(denominatorBox, maxWidth, TexAlignment.Center);

            // Create result box.
            var resultBox = VerticalBox.Get();

            // add box for numerator.
            resultBox.Add(numeratorBox);

            // Calculate clearance and adjust shift amounts.
            //var axis = TEXConfiguration.main.AxisHeight * TexContext.Scale;

            // Calculate clearance amount.
            float clearance = thickness > 0 ? gapUp : gapUp + this.thickness;

            // Adjust shift amounts.
            var kern1 = gapUp - numeratorBox.depth;
            var kern2 = gapDown - denominatorBox.height;
            var delta1 = clearance - kern1;
            var delta2 = clearance - kern2;
            if (delta1 > 0)
            {
                kern1 += delta1;
            }
            if (delta2 > 0)
            {
                kern2 += delta2;
            }

            if (line)
            {
                // Draw fraction line.

                resultBox.Add(StrutBox.Get(0, kern1, 0));
                resultBox.Add(RuleBox.Get(color, maxWidth, thickness, 0));
                resultBox.Add(StrutBox.Get(0, kern2, 0));
            }
            else
            {
                // Do not draw fraction line.

                var kern = kern1 + thickness + kern2;
                resultBox.Add(StrutBox.Get(0, kern, 0));
            }

            // add box for denominator.
            resultBox.Add(denominatorBox);

            // Adjust height and depth of result box.
            resultBox.height = kern1 + thickness * 0.5f + numeratorBox.TotalHeight;
            resultBox.depth = kern2 + thickness * 0.5f + denominatorBox.TotalHeight;
            resultBox.shift -= lineMedian;
            return resultBox;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            color = state.Color.current;
            lineMedian = state.Typeface.lineMedian * r;


            if (string.IsNullOrEmpty(value)) return;


            SkipWhiteSpace(value, ref position);
            var forcedisplay = command == "cfrac";
            state.PushMathStyle(forcedisplay ? (Func<TexMathStyle, TexMathStyle>)((x) => TexMathStyle.Display) : (x) => x.GetNumeratorStyle());
            r = state.Ratio;
            gapUp = state.Math.upperMinimumDistance * r;
            gapDown = state.Math.lowerMinimumDistance * r;
            thickness = state.Math.lineThickness * r;
            margin = state.Environment.current.GetMathStyle().IsCrampedStyle() ? 0 : state.Math.fractionPadding * r;
            numerator = state.parser.ParseToken(value, state, ref position);
            state.PopMathStyle();
            SkipWhiteSpace(value, ref position);
            state.PushMathStyle(forcedisplay ? (Func<TexMathStyle, TexMathStyle>)((x) => TexMathStyle.Display) : (x) => x.GetDenominatorStyle());
            denominator = state.parser.ParseToken(value, state, ref position);
            state.PopMathStyle();

            if (command.Contains("n"))
                line = false;
        }
        public void ProcessParameters(string command, TexParserState state, string strnum, string strdenom)
        {
            var r = state.Ratio;
            color = state.Color.current;
            lineMedian = state.Typeface.lineMedian * r;

            state.PushMathStyle((x) => x.GetNumeratorStyle());
            r = state.Ratio;
            gapUp = state.Math.upperMinimumDistance * r;
            gapDown = state.Math.lowerMinimumDistance * r;
            thickness = state.Math.lineThickness * r;
            margin = state.Environment.current.GetMathStyle().IsCrampedStyle() ? 0 : state.Math.fractionPadding * r;
            numerator = TryToUnpack(state.parser.Parse(strnum, state));
            state.PopMathStyle();
            state.PushMathStyle((x) => x.GetDenominatorStyle());
            denominator = TryToUnpack(state.parser.Parse(strdenom, state));
            state.PopMathStyle();

            if (command.Contains("n"))
                line = false;
        }

        public override void Flush()
        {
            numerator?.Flush();
            numerator = null;
            denominator?.Flush();
            denominator = null;
            line = true;
            ObjPool<FractionAtom>.Release(this);
        }

        internal static FractionAtom Get()
        {
            return ObjPool<FractionAtom>.Get();
        }
    }
}

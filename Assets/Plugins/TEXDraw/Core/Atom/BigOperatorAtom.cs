using System;
using System.Collections.Generic;
using UnityEngine;

namespace TexDrawLib
{
    public class BigOperatorAtom : ScriptsAtom
    {
        public override CharType Type => CharType.BigOperator;
        public override CharType LeftType => CharType.BigOperator;
        public override CharType RightType => CharType.BigOperator;

        public SymbolAtom sublimit, superlimit;

        private static Box ExpandWidth(Box box, float maxWidth, float leftOffset)
        {
            // Centre specified box in new box of specified width, if necessary.
            var offset = leftOffset + (maxWidth - box.width) / 2;
            if (offset > Math.E)
                box = HorizontalBox.Get(box, offset + box.width, TexAlignment.Right);
            return box;
        }

        public new static ScriptsAtom Get()
        {
            return ObjPool<BigOperatorAtom>.Get();
        }


        public override Box CreateBox(TexBoxingState state)
        {
            if (!limitMode || (subscript == null && superscript == null && sublimit == null && superlimit == null))
            {
                // fallback to usual script mode
                this.crampedMode = true;
                this.marginH = -median; // \int special case
                var box = base.CreateBox(state);
                if (box is HorizontalBox hbox && hbox.children.Count > 0)
                {
                    TexUtility.CentreBox(hbox.children[0], median);
                    hbox.shift = hbox.children[0].shift;
                    hbox.children[0].shift = 0;
                }

                if (superscript != null)
                {
                    if (box is HorizontalBox n1 && n1.children.Count > 1 && n1.children[1] is VerticalBox n2 && n2.children.Count > 0 && n2.children[0] is HorizontalBox n3)
                    {
                        // handle additional bearing (special case for \int)
                        n3.Add(0, StrutBox.Get(leadOffset, 0, 0));
                        n2.width += leadOffset;
                        n1.width += leadOffset;
                    } else if (box is HorizontalBox n4 && n4.children.Count > 1 && n4.children[1] is HorizontalBox n5)
                    {
                        n5.Add(0, StrutBox.Get(leadOffset, 0, 0));
                        n4.width += leadOffset;
                    }
                }
                return box;
            }

            // Create box for base atom.
            Box baseBox = atom.CreateBox(state);


            // Create boxes for upper and lower limits.
            Box upperLimitBox, lowerLimitBox;
            if (superscript is SymbolAtom atom1)
                upperLimitBox = atom1.CreateBoxMinWidth(baseBox.width);
            else
                upperLimitBox = superscript?.CreateBox(state);

            if (subscript is SymbolAtom atom2)
                lowerLimitBox = atom2.CreateBoxMinWidth(baseBox.width);
            else
                lowerLimitBox = subscript?.CreateBox(state);

            if (atom is SymbolAtom ssatom && ssatom.metadata.extension.enabled)
            {
                baseBox.Flush();
                baseBox = ssatom.CreateBoxMinWidth(Mathf.Max(
                    upperLimitBox != null ? upperLimitBox.width : 0,
                    lowerLimitBox != null ? lowerLimitBox.width : 0
                    ));
            }

            Box superlimitBox = null, sublimitBox = null;
            if (sublimit != null || superlimit != null)
            {
                var verbox = VerticalBox.Get(baseBox);
                if (superlimit != null)
                {
                    verbox.Add(0, StrutBox.Get(0, upShift, 0));
                    superlimitBox = superlimit.CreateBoxMinWidth(baseBox.width);
                    baseBox.shift = (superlimitBox.width - baseBox.width) * 0.5f;
                    verbox.Add(0, superlimitBox);
                }
                if (sublimit != null)
                {
                    verbox.Add(StrutBox.Get(0, downShift, 0));
                    sublimitBox = sublimit.CreateBoxMinWidth(baseBox.width);
                    baseBox.shift = (sublimitBox.width - baseBox.width) * 0.5f;
                    verbox.Add(sublimitBox);
                }
                var vTotalHeight = verbox.TotalHeight;
                verbox.height = baseBox.height;
                if (superlimitBox != null)
                    verbox.height += superlimitBox.TotalHeight + upShift;
                verbox.depth = vTotalHeight - verbox.height;
                baseBox = verbox;
            }
            
            // Make all component boxes equally wide.
            var maxWidth = Mathf.Max(Mathf.Max(baseBox.width, upperLimitBox == null ? 0 : upperLimitBox.width),
                      lowerLimitBox == null ? 0 : lowerLimitBox.width);
            var leftMost = Mathf.Min(upOffset, Math.Min(downOffset, 0));

            if (baseBox != null)
                baseBox = ExpandWidth(baseBox, maxWidth, 0 - leftMost);
            if (upperLimitBox != null)
                upperLimitBox = ExpandWidth(upperLimitBox, maxWidth, upOffset - leftMost);
            if (lowerLimitBox != null)
                lowerLimitBox = ExpandWidth(lowerLimitBox, maxWidth, downOffset - leftMost);

            if (leadOffset != 0)
            {
                baseBox = HorizontalBox.Get(baseBox, baseBox.width + leadOffset, TexAlignment.Left);
            }

            var resultBox = VerticalBox.Get();
            var opSpacing = 0;
            var kern = 0f;

            // Create and add box for upper limit.
            if (superscript != null)
            {
                resultBox.Add(StrutBox.Get(0, opSpacing, 0));
                //upperLimitBox.shift += upOffset;
                resultBox.Add(upperLimitBox);
                kern = Mathf.Max(upShift, upMargin - upperLimitBox.depth);
                resultBox.Add(StrutBox.Get(0, kern, 0));
            }

            // Add box for base atom.
            resultBox.Add(baseBox);

            // Create and add box for lower limit.
            if (subscript != null)
            {
                resultBox.Add(StrutBox.Get(0, Mathf.Max(downShift, downMargin - lowerLimitBox.height), 0));
                //lowerLimitBox.shift += downOffset;
                resultBox.Add(lowerLimitBox);
                resultBox.Add(StrutBox.Get(0, opSpacing, 0));
            }

            // Adjust height and depth of result box.
            var baseBoxHeight = atom.Type == CharType.BigOperator ? baseBox.TotalHeight / 2 + median : baseBox.height;
            var totalHeight = resultBox.height + resultBox.depth;
            if (upperLimitBox != null)
                baseBoxHeight += opSpacing + kern + upperLimitBox.height + upperLimitBox.depth;
            resultBox.height = baseBoxHeight;
            resultBox.depth = totalHeight - baseBoxHeight;
            return resultBox;
        }

        // I can't think better way but to hardcode it right here sorry

        static float TopOffset(string name)
        {
            switch (name)
            {
                case "integraltext":
                case "integraldisplay":
                case "contintegraltext":
                case "contintegraldisplay":
                case "iint":
                case "iiint":
                case "iiiint":
                case "idotsint":
                    return 12f;
                case "varint":
                case "varoint":
                    return 8f;
                default:
                    return 0;
            }
        }

        static float BottomOffset(string name)
        {
            switch (name)
            {
                case "integraltext":
                case "integraldisplay":
                case "contintegraltext":
                case "contintegraldisplay":
                case "iint":
                case "iiint":
                case "iiiint":
                case "oiint":
                case "idotsint":
                    return -2.5f;
                case "varint":
                case "varoint":
                    return -1f;
                default:
                    return 0;
            }
        }
        static float LeadOffset(string name)
        {
            switch (name)
            {
                case "integraltext":
                case "integraldisplay":
                case "contintegraltext":
                case "contintegraldisplay":
                case "iint":
                case "iiint":
                case "iiiint":
                case "oiint":
                    return 8f;
                case "idotsint":
                    return 2f;
                case "varint":
                case "varoint":
                case "variint":
                case "variiint":
                case "varoiint":
                    return 1f;
                default:
                    return 0;
            }
        }

        static CharBox GetDisplayBox(SymbolAtom atom)
        {
            var ch = atom.metadata;
            if (ch.larger.Has)
            {
                return atom.CreateBoxSubtituted(ch.larger.Get);
            }
            else if (ch.symbol.EndsWith("text"))
            {
                var altSymbol = ch.symbol.Substring(0, ch.symbol.Length - 4) + "display";
                var ch2 = TEXPreference.main.GetChar(altSymbol, ch.fontIndex);
                if (ch2 != null)
                    return atom.CreateBoxSubtituted(ch2);
            }
            return atom.CreateBoxSubtituted(ch);
        }

        float upMargin, upShift, downMargin, downShift, median, upOffset, downOffset, leadOffset;
        public bool limitMode, textMode;

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            upMargin = state.Math.upperBaselineDistance * r;
            upShift = state.Math.upperMinimumDistance * r;
            downMargin = state.Math.lowerBaselineDistance * r;
            downShift = state.Math.lowerMinimumDistance * r;
            median = state.Typeface.lineMedian * r;
            textMode = state.Environment.current.GetMathStyle() >= TexMathStyle.Text;
            limitMode = !state.parser.integralSymbols.Contains(command);
            TexParserUtility.SkipWhiteSpace(value, ref position);
            if (position < value.Length && value[position] == '\\')
            {
                // see if user overrides the limitMode
                var oldpos = position++;
                string cmd = TexParserUtility.LookForAWord(value, ref position);
                if (cmd == "limits")
                    limitMode = !textMode;
                else if (cmd == "nolimits")
                    limitMode = false;
                else
                    position = oldpos;
            }

            if (command.StartsWith("under"))
            {
                var partial = state.parser.overUnderVariants[command];
                if (partial == "")
                {
                    state.PushMathStyle(x => x.GetSubscriptStyle());
                    subscript = state.parser.ParseToken(value, state, ref position);
                    state.PopMathStyle();
                    atom = state.parser.ParseToken(value, state, ref position);
                }
                else 
                {
                    sublimit = SymbolAtom.Get(TEXPreference.main.GetChar(partial, state.Font.current), state);
                    atom = state.parser.ParseToken(value, state, ref position);
                }
                limitMode = true;
            }
            else if (command.StartsWith("over"))
            {
                var partial = state.parser.overUnderVariants[command];
                if (partial == "")
                {
                    state.PushMathStyle(x => x.GetSuperscriptStyle());
                    superscript = state.parser.ParseToken(value, state, ref position);
                    state.PopMathStyle();
                    atom = state.parser.ParseToken(value, state, ref position);
                }
                else
                {
                    superlimit = SymbolAtom.Get(TEXPreference.main.GetChar(partial, state.Font.current), state);
                    atom = state.parser.ParseToken(value, state, ref position);
                }
                limitMode = true;
            }
            else if (TEXPreference.main.GetChar(command)?.type == CharType.BigOperator)
            {
                var ch = TEXPreference.main.GetChar(command);
                if (!textMode && ch.larger.Has)
                    ch = ch.larger.Get;
                atom = SymbolAtom.Get(ch, state);
            }
            else if (atom == null)
            {
                atom = WordAtom.Get();
                atom.ProcessParameters(command, state);
            }

            upOffset = TopOffset(command) * r;
            downOffset = BottomOffset(command) * r;
            leadOffset = LeadOffset(command) * r;

            base.ProcessParameters(command, state, value, ref position);
        }
        public override void Flush()
        {
            ObjPool<BigOperatorAtom>.Release(this);
            if (sublimit != null)
            {
                sublimit.Flush();
                sublimit = null;
            }
            if (superlimit != null)
            {
                superlimit.Flush();
                superlimit = null;
            }
            upOffset = downOffset = leadOffset = 0;
            base.Flush();
        }
    }

    public class UpperUnderAtom : BigOperatorAtom
    {

    }
}

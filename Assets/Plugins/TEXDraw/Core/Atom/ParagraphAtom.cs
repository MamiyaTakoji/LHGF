using System;
using System.Collections.Generic;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public sealed class ParagraphAtom : LayoutAtom
    {

        public float indent = 0;

        public float alignment = 0;

        bool justified = false;

        float minHeight, minDepth, lineSpace, paraSpace, leftMargin, rightMargin;

        public override CharType Type => CharTypeInternal.Inner;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;

        public static ParagraphAtom Get()
        {
            return ObjPool<ParagraphAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<ParagraphAtom>.Release(this);
            alignment = 0;
            indent = 0;
            justified = false;
            minHeight = 0;
            minDepth = 0;
            leftMargin = 0;
            rightMargin = 0;
            lineSpace = 0;
            paraSpace = 0;
            base.Flush();
            charBuildingBlock.Clear();
        }

        public readonly List<CharAtom> charBuildingBlock = new List<CharAtom>();

        public override void Add(Atom atom)
        {
            if (atom is CharAtom catom)
            {
                charBuildingBlock.Add(catom);
            }
            else
            {
                CleanupWord();
                base.Add(atom);
            }
        }

        public override Box CreateBox(TexBoxingState state)
        {
            CleanupWord();

            if (children.Count == 0)
            {
                var box = StrutBox.Get(0, 0, 0);
                return box;
            }

            state.Push();
            state.width -= leftMargin + rightMargin;
            state.interned = true;

            // restricted = autosized width paragraph (which is, rare).
            Box result = state.restricted ? CreateBoxRestricted(state) : CreateBoxWrappable(state);

            state.Pop();
            return CheckMaster(result);
        }

        public void CleanupWord()
        {
            if (charBuildingBlock.Count > 0)
            {
                var word = WordAtom.Get();
                word.Add(charBuildingBlock);
                charBuildingBlock.Clear();
                word.ProcessLigature();
                base.Add(word);
            }
        }

        private Box CreateBoxWrappable(TexBoxingState state)
        {
            var masterBox = VerticalBox.Get();
            Box overflowedBox = null;
            int i = 0;
            float oriStateWidth = state.width;
            while (i < children.Count || overflowedBox != null)
            {
                HorizontalBox box = HorizontalBox.Get();
                state.width = i == 0 ? oriStateWidth - indent : oriStateWidth;
                if (overflowedBox != null)
                {
                    if (overflowedBox.width > state.width && children[i - 1] is WordAtom)
                        UnpackWordAndDoLetterWrapping(state, masterBox, ref box, overflowedBox);
                    else
                        box.Add(overflowedBox);
                    overflowedBox = null;
                }
                bool softlyBreak = false;
                for (; i < children.Count; i++)
                {
                    var draftBox = children[i].CreateBox(state);
                    if (children[i] is WordAtom watom && box.width + draftBox.width > state.width + 0.01f && (box.children.Count == 0 || watom.hasHyphen))
                    {
                        if (watom.hasHyphen)
                            // try to break by hyphen (or do letter wrapping)
                            UnpackWordByHypens(state, masterBox, ref box, watom, draftBox, box.children.Count == 0);
                        else
                            // Need to unpack and do letter wrapping
                            UnpackWordAndDoLetterWrapping(state, masterBox, ref box, draftBox);
                    }
                    else if (i > 0 && children[i] is ISoftBreak)
                    {
                        softlyBreak = true;
                        overflowedBox = draftBox;
                        i++;
                        break;
                    }
                    else if (i > 0 && (box.width + draftBox.width > state.width + 0.01f && !(children[i] is SpaceAtom)))
                    {
                        overflowedBox = draftBox;
                        i++;
                        break;
                    }
                    else
                        box.Add(draftBox);
                }

                if (box.children[box.children.Count - 1] is StrutBox boxlast && i < children.Count - 1)
                {
                    // delete last space
                    boxlast.Flush();
                    box.children.RemoveAt(box.children.Count - 1);
                    box.Recalculate();
                }

                if (!state.restricted)
                {
                    if (FlexibleAtom.HandleFlexiblesHorizontal(box, state.width))
                    {
                        // Already justified.
                    }
                    else if (justified && !(i == children.Count && overflowedBox == null) && !softlyBreak)
                    {
                        JustifySpaces(state, box);
                    }
                    else if (alignment > 0)
                    {
                        AddAlignment(state, box);
                    }


                    if (i == children.Count && overflowedBox == null)
                    {
                        if (masterBox.children.Count == 0)
                        {
                            masterBox.Flush();
                            return CheckBox(box, true);
                        }
                        else
                        {
                            masterBox.Add(CheckBox(box));
                        }
                    }
                    else
                    {
                        masterBox.Add(CheckBox(box, masterBox.children.Count == 0));
                    }
                }
                else
                {
                    masterBox.Add(box);
                }

            }

            return masterBox;
        }

        private void AddAlignment(TexBoxingState state, HorizontalBox box)
        {
            var spaceAvailable = state.width - box.width;
            var l = StrutBox.Get(spaceAvailable * alignment, 0, 0);
            box.Add(0, l);
        }

        private static void JustifySpaces(TexBoxingState state, HorizontalBox box)
        {
            int spaceCount = 0;
            float spaceAvailable = state.width - box.width;
            for (int j = 1; j < box.children.Count - 1; j++)
                if (box.children[j] is StrutBox boxj && boxj.width > 0)
                    spaceCount++;

            if (spaceCount > 0)
            {
                // unitSpace = spaceAvailable / spaceCount
                float unitSpace = spaceAvailable / spaceCount;
                for (int j = 1; j < box.children.Count; j++)
                    if (box.children[j] is StrutBox boxj && boxj.width > 0)
                        boxj.width += unitSpace;
            }

            box.Recalculate();
        }

        Box CheckBox(HorizontalBox b, bool useindent = false)
        {
            b.height = Math.Max(b.height, minHeight) + lineSpace / 2;
            b.depth = Math.Max(b.depth, minDepth) + lineSpace / 2;
            if (useindent && indent != 0)
            {
                b.Add(0, StrutBox.Get(indent, 0, 0));
            }
            return b;
        }

        Box CheckMaster(Box b)
        {
            b.height += paraSpace / 2;
            b.depth += paraSpace / 2;
            if (leftMargin == 0) return b;
            return HorizontalBox.Get(b, b.width + leftMargin, TexAlignment.Right);
        }

        Box CreateBoxRestricted(TexBoxingState state)
        {
            var box = HorizontalBox.Get();
            for (int i = 0; i < children.Count; i++)
            {
                box.Add(children[i].CreateBox(state));
            }
            return CheckBox(box);
        }


        private void UnpackWordByHypens(TexBoxingState state, VerticalBox masterBox, ref HorizontalBox box, WordAtom watom, Box draftBox, bool allowLetterUnpack)
        {
            var boxes = ((HorizontalBox)draftBox).children;
            Box checkpoint = null;
            var checkOffset = box.children.Count;
            var checkpointIdx = checkOffset - 1;
            var checkpointWidth = 0f;
            for (int k = 0; k < watom.children.Count; k++)
            {
                var ch = watom.children[k];
                var box2 = boxes[k];
                if (box.width + box2.width + checkpointWidth > state.width + 0.01f)
                {                    
                    if (checkpoint == null && allowLetterUnpack)
                    {
                        box.children.Clear();
                        box.Recalculate();
                        UnpackWordAndDoLetterWrapping(state, masterBox, ref box, draftBox);
                        return;
                    }
                    else
                    {
                        masterBox.Add(box);
                        var newbox = HorizontalBox.Get();
                        // rollback till last checkpoint
                        while (box.children.Count > checkpointIdx + 1)
                        {
                            newbox.Add(0, box.children[box.children.Count - 1]);
                            box.width -= box.children[box.children.Count - 1].width;
                            box.children.RemoveAt(box.children.Count - 1);
                        }
                        if (checkpoint != null && checkpointWidth > 0)
                        {
                            // replace the fake one
                            box.children[checkpointIdx].Flush();
                            box.children[checkpointIdx] = checkpoint;
                            box.width += checkpoint.width;
                            checkpoint = null;
                        }
                        if (justified)
                        {
                            JustifySpaces(state, box);
                        }
                        CheckBox(box);
                        checkOffset -= box.children.Count;
                        AddAlignment(state, box);
                        box = newbox;
                    }
                }
                if (ch is HyphenBreak hyp)
                {
                    checkpointIdx = k + checkOffset;
                    if (hyp.show)
                    {
                        // the width is already added
                        checkpoint = box2;
                        checkpointWidth = 0;
                    }
                    else
                    {
                        if (checkpoint != null)
                            checkpoint.Flush();
                        // force show and regenerate
                        checkpoint = hyp.CreateBoxEnforcedShow(state);
                        checkpointWidth = checkpoint.width;
                    }
                }
                box.Add(box2);
            }
            boxes.Clear();
            draftBox.Flush();
        }

        private void UnpackWordAndDoLetterWrapping(TexBoxingState state, VerticalBox masterBox, ref HorizontalBox box, Box draftBox)
        {
            var boxes = ((HorizontalBox)draftBox).children;

            for (int k = 0; k < boxes.Count; k++)
            {
                var box2 = boxes[k];
                if (box.width + box2.width > state.width + 0.01f)
                {
                    masterBox.Add(CheckBox(box));
                    AddAlignment(state, box);
                    box = HorizontalBox.Get();
                }
                box.Add(box2);
            }
            boxes.Clear();
            draftBox.Flush();
        }


        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (!state.Environment.current.IsInline())
            {
                var r = state.Ratio;
                alignment = state.Paragraph.alignment;
                indent = state.Paragraph.indent ? state.Paragraph.indentSpacing * r : 0;
                justified = state.Paragraph.justify;
                minHeight = state.Typeface.lineAscent * r;
                minDepth = state.Typeface.lineDescent * r;
                leftMargin = state.Paragraph.leftPadding * r;
                rightMargin = state.Paragraph.rightPadding * r;
                lineSpace = state.Paragraph.lineSpacing * r;
                paraSpace = state.Paragraph.paragraphSpacing * r;
            }
            
            // a paragraphatom processparameters call is usually after the input.so clean it up
            CleanupWord();

            if (value != null)
                SkipWhiteSpace(value, ref position);
        }

        public override bool HasContent()
        {
            return children.Count > 0 || charBuildingBlock.Count > 0;
        }
    }
}

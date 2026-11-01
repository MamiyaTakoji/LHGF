using System;
using System.Linq;
using UnityEngine;
using static TexDrawLib.TexParserUtility;
using System.Collections.Generic;

namespace TexDrawLib
{
    public class TabularAtom : LayoutAtom
    {
        public static TabularAtom Get()
        {
            return ObjPool<TabularAtom>.Get();
        }

        public override CharType Type => CharTypeInternal.Inner;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;

        public const int maximumColumnSet = 32;

        public struct ColumnMetrics
        {
            public Vector2 alignment;
            public float borderWidth;
            public int borderStyle; // 0 = no border, 1 = single border, 2 = double border
            public float width;
            public float flexibleWidth;
            public string macroBefore; // null - no macro
            public string macroAfter; // null - no macro

        }

        // width 0 == auto
        public ColumnMetrics[] columnMetrics = new ColumnMetrics[maximumColumnSet];

        public int columnCount;

        public override void Flush()
        {
            ObjPool<TabularAtom>.Release(this);
            columnCount = Math.Min(30, columnCount);
            while (columnCount-- >= 0)
            {
                columnMetrics[columnCount + 1] = default;
            }
            columnCount = 0;
            base.Flush();
        }

        protected float minHeight, minDepth, tableSpaceX, tableSpaceY, paraSpace, leftMargin, rightMargin, alignment, median;
        protected bool justified, outerPadding = true;

        public override Box CreateBox(TexBoxingState state)
        {
            if (columnCount == 0) return StrutBox.Empty;

            state.Push();
            state.restricted = true;
            state.interned = true;

            bool hasRecalculated = false, needRecalculate = false;


            List<float> columnSizes = ListPool<float>.Get(Enumerable.Repeat(0f, columnCount));
            List<float> rowHSizes = ListPool<float>.Get(Enumerable.Repeat(0f, children.Count / columnCount));
            List<float> rowDSizes = ListPool<float>.Get(Enumerable.Repeat(0f, children.Count / columnCount));

            recalculate:
            for (int i = 0; i < children.Count; i++)
            {
                var chi = (children[i] as TabularCellAtom);
                if (!needRecalculate)
                    chi.overrideMetric = null;
                Vector3 size = chi.PrecomputeBox(state);
                int x = i % columnCount, y = i / columnCount;
                columnSizes[x] = Math.Max(columnSizes[x], size.x);
                rowHSizes[y] = Math.Max(rowHSizes[y], size.y);
                rowDSizes[y] = Math.Max(rowDSizes[y], size.z);
            }

            int totalColumnAuto = 0; float totalColumnSize = 0f, totalFlexibleSize = 0f;
            for (int i = 0; i < columnCount; i++)
            {
                if (columnMetrics[i].width <= float.Epsilon)
                    totalColumnAuto++;
                else
                    columnSizes[i] = columnMetrics[i].width;
                totalColumnSize += columnSizes[i];
                totalFlexibleSize += columnMetrics[i].flexibleWidth;
            }

            // If Justify, expand
            if (totalFlexibleSize > 0 && state.width > 0)
            {
                float excessUnit = (state.width - totalColumnSize) / totalFlexibleSize;
                for (int i = 0; i < columnCount; i++)
                {
                    if (columnMetrics[i].flexibleWidth > float.Epsilon)
                        columnSizes[i] += excessUnit / columnMetrics[i].flexibleWidth;
                }
            }
            else if (justified && totalColumnAuto > 0 && state.width > totalColumnSize)
            {
                float excessUnit = (state.width - totalColumnSize) / totalColumnAuto;
                for (int i = 0; i < columnCount; i++)
                {
                    if (columnMetrics[i].width <= float.Epsilon)
                        columnSizes[i] += excessUnit;
                }
            }

            for (int i = 0; i < children.Count; i++)
            {
                int x = i % columnCount, y = i / columnCount;
                Vector3 m = new Vector3(columnSizes[x], rowHSizes[y], rowDSizes[y]);
                if ((children[i] as TabularCellAtom).AssignFinalMetric(m))
                    needRecalculate = true;
            }

            if (needRecalculate && !hasRecalculated)
            {
                // required because of reflow effect of using x (expandable) columns
                hasRecalculated = true;
                goto recalculate;
            }

            // Draw
            VerticalBox vbox = VerticalBox.Get();
            HorizontalBox rbox = HorizontalBox.Get();
            for (int i = 0; i < children.Count; i++)
            {
                if (i % columnCount == 0 && i > 0)
                {
                    vbox.Add(rbox);
                    rbox = HorizontalBox.Get();
                }
                rbox.Add(children[i].CreateBox(state));

            }
            vbox.Add(rbox);

            // if ((!justified || totalColumnAuto == 0) && state.width > totalColumnSize)
            {
                TexUtility.CentreBox(vbox, median);
            }
            ListPool<float>.FlushAndRelease(columnSizes);
            ListPool<float>.FlushAndRelease(rowHSizes);
            ListPool<float>.FlushAndRelease(rowDSizes);

            state.Pop();

            return vbox;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            float r = state.Ratio;
            alignment = state.Paragraph.alignment;
            justified = state.Paragraph.justify;
            outerPadding = command != "matrix";
            minHeight = state.Typeface.lineAscent * r;
            minDepth = state.Typeface.lineDescent * r;
            median = state.Typeface.lineMedian * r;
            leftMargin = state.Paragraph.leftPadding * r;
            rightMargin = state.Paragraph.rightPadding * r;
            tableSpaceX = (command == "matrix" || command == "array" || command == "align" ? state.Math.matricePadding : state.Math.framePadding) * r;
            tableSpaceY = (command == "matrix" || command == "array" ? state.Math.matricePadding : state.Math.framePadding) * r;
            paraSpace = state.Paragraph.paragraphSpacing * r;

            if (command == "tabular" || command == "array")
            {
                if (command == "array")
                {
                    justified = false;
                }
                SkipWhiteSpace(value, ref position);
                string columnToken = ReadGroup(value, ref position);
                int seek = 0;
                while (seek < columnToken.Length)
                {
                    SkipWhiteSpace(columnToken, ref seek);
                    if (seek < columnToken.Length)
                    {
                        switch (columnToken[seek++])
                        {
                            case 'l':
                            case 'c':
                            case 'r':
                                if (++columnCount > maximumColumnSet) break;
                                columnMetrics[columnCount - 1].alignment.x = AlignmentCode(
                                    columnToken[seek - 1]);
                                if (seek < columnToken.Length)
                                {
                                    var ch = columnToken[seek];
                                    if (ch == 'p' || ch == 'm' || ch == 'b')
                                    {
                                        seek++;
                                        goto case '\xffd';
                                    }
                                    else if (ch == 'x')
                                    {
                                        seek++;
                                        goto case '\xffe';
                                    }
                                }
                                break;
                            case 'p':
                            case 'm':
                            case 'b':
                                if (++columnCount > maximumColumnSet) break;
                                goto case '\xffd';
                            case '\xffd': // private reserved
                                if (columnCount <= 0) break;
                                columnMetrics[columnCount - 1].alignment.y = AlignmentCode(
                                      columnToken[seek - 1]);
                                if (seek < columnToken.Length)
                                {
                                    var ch = columnToken[seek];
                                    if (ch == 'x')
                                    {
                                        seek++;
                                        goto case '\xffe';
                                    }
                                }
                                break;
                            case 'x':
                                if (++columnCount > maximumColumnSet) break;
                                goto case '\xffe';
                            case '\xffe': // private reserved
                                columnMetrics[columnCount - 1].flexibleWidth = columnMetrics[columnCount - 1].width < float.Epsilon ? 1 : columnMetrics[columnCount - 1].width;
                                break;
                            case '{':
                                if (columnCount <= 0) break;
                                seek--;
                                string unit = ReadGroup(columnToken, ref seek); // seek width
                                columnMetrics[columnCount - 1].width = TexUtility.ParseUnit(unit, state);
                                break;
                            case '|':
                                columnMetrics[columnCount].borderWidth = state.Math.lineThickness * state.Ratio;
                                columnMetrics[columnCount].borderStyle = 1;
                                while (seek < columnToken.Length && columnToken[seek] == '|')
                                {
                                    seek++;
                                    columnMetrics[columnCount].borderStyle++;
                                }
                                break;
                            case '>':
                                columnMetrics[columnCount].macroBefore = ReadGroup(columnToken, ref seek);
                                break;
                            case '<':
                                if (columnCount <= 0) break;
                                columnMetrics[columnCount - 1].macroAfter = ReadGroup(columnToken, ref seek);
                                break;
                        }
                    }
                }
            }
            else if (command == "align")
            {
                tableSpaceX = 0;
                columnCount = maximumColumnSet - 1;
                for (int i = 0; i < columnCount; i++)
                {
                    columnMetrics[i] = new ColumnMetrics
                    {
                        alignment = new Vector2(i % 2 == 0 ? 1 : 0, 0),
                    };
                }
            }
            else if (command == "matrix")
            {
                columnCount = maximumColumnSet - 1;
                justified = false;
                for (int i = 0; i < columnCount; i++)
                {
                    columnMetrics[i] = new ColumnMetrics
                    {
                        alignment = new Vector2(0.5f, 0),
                    };
                }
            }
        }

        public void Interprete(string text, ref int position, TexParserState state)
        {
            int column = 0;
            int realColumnCount = 0;
            int lastrowindex = 0;
            if (columnCount == 0)
                return;

            HorizontalRuleAtom therule = null;
            int theruletimes = 0;
            float theextrabottom = 0;
            while (position < text.Length)
            {
                SkipWhiteSpace(text, ref position);
                state.PushStates();
                LayoutAtom container;
                realColumnCount = Math.Max(realColumnCount, column + 1);
                if (column >= columnCount)
                {
                    // actual column count turns out larger
                    //state.Paragraph.alignment = 0.5f;
                    //realColumnCount = Mathf.Max(column + 1, realColumnCount);
                    container = state.Environment.current.IsMathMode() ? (LayoutAtom)MathAtom.Get() : ParagraphAtom.Get();
                    state.LayoutContainer.Set(container);
                }
                else
                {
                    // Apply macros and other stuff
                    if (!string.IsNullOrEmpty(columnMetrics[column].macroBefore))
                    {
                        container = (state.parser.Parse(columnMetrics[column].macroBefore, state).children[0]) as LayoutAtom;
                    }
                    else
                    {
                        container = state.Environment.current.IsMathMode() ? (LayoutAtom)MathAtom.Get() : ParagraphAtom.Get();
                    }
                    TabularCellAtom.currentAlignment = columnMetrics[column].alignment;
                    TabularCellAtom.currentTableSpace = new Vector2(tableSpaceX, tableSpaceY);
                    state.LayoutContainer.Set(container);
                }
                while (position < text.Length)
                {
                    Atom atom = state.parser.ParseToken(text, state, ref position);
                    if (atom is AlignmentAtom)
                    {
                        atom.Flush();
                        break;
                    }
                    else if (atom is SoftBreakAtom soft)
                    {
                        if (!string.IsNullOrEmpty(columnMetrics[column].macroAfter))
                        {
                            container.Add(state.parser.Parse(columnMetrics[column].macroAfter, state).children[0]);
                        }
                        container.ProcessParameters(string.Empty, state);
                        state.PopStates();
                        while (++column < columnCount)
                        {
                            // force this to get complete cells in a row
                            TabularCellAtom tcelll = TabularCellAtom.Get(container);
                            tcelll.ProcessParameters(string.Empty, state, text, ref position);
                            tcelll.left.borderStyle = columnMetrics[column].borderStyle;
                            tcelll.left.borderWidth = columnMetrics[column].borderWidth;
                            tcelll.left.color = state.Color.current;
                            tcelll.right.borderStyle = columnMetrics[(column + 1) % maximumColumnSet].borderStyle;
                            tcelll.right.borderWidth = columnMetrics[(column + 1) % maximumColumnSet].borderWidth;
                            tcelll.right.color = state.Color.current;
                            children.Add(tcelll);
                            container = RowAtom.Get();
                            state.LayoutContainer.Set(container);
                        }
                        lastrowindex = children.Count;
                        theextrabottom = soft.extraSpace;
                        column--; // should be 0
                        atom.Flush();
                        state.PushStates();
                        break;
                    }
                    else if (atom is HorizontalRuleAtom ratom)
                    {
                        if (therule != null)
                            therule.Flush();
                        therule = ratom;
                        theruletimes++;
                    }
                    else if (atom is DocumentAtom docatom)
                    {
                        Atom aaa = docatom.children[0];
                        docatom.children.Clear();
                        docatom.Flush();
                        container.Add(aaa);
                    }
                    else if (atom is SpaceAtom && !container.HasContent())
                    {
                        atom.Flush();
                        continue;
                    }
                    else if (atom != null)
                    {
                        container.Add(atom);
                    }
                }
                TabularCellAtom tcell = TabularCellAtom.Get();

                if (!string.IsNullOrEmpty(columnMetrics[column].macroAfter))
                {
                    container.Add(state.parser.Parse(columnMetrics[column].macroAfter, state, true).children[0]);
                }
                container.ProcessParameters(string.Empty, state);
                    if (container is ParagraphAtom para)
                    para.CleanupWord();
                if (container.children.Count > 0)
                    tcell.atom = container;
                tcell.ProcessParameters(string.Empty, state, text, ref position);
                tcell.x.size = columnMetrics[column].width;
                tcell.left.borderWidth = columnMetrics[column].borderWidth;
                tcell.left.borderStyle = columnMetrics[column].borderStyle;
                tcell.left.color = state.Color.current;
                if (column == columnCount - 1 && column < maximumColumnSet - 1)
                {
                    tcell.right.borderWidth = columnMetrics[1 + column].borderWidth;
                    tcell.right.borderStyle = columnMetrics[1 + column].borderStyle;
                    tcell.right.color = state.Color.current;
                }
                if (therule != null)
                {
                    for (int i = lastrowindex; i < children.Count; i++)
                    {
                        (children[i] as TabularCellAtom).bottom.padding += therule.top;
                    }
                    tcell.top.borderWidth = therule.thickness;
                    tcell.top.borderStyle = theruletimes;
                    tcell.top.color = therule.color;
                    tcell.top.padding += therule.bottom;
                }
                tcell.top.padding += theextrabottom;
                column = (column + 1) % columnCount;
                if (column == 0 && therule != null)
                {
                    therule.Flush();
                    therule = null;
                    theruletimes = 0;
                }
                children.Add(tcell);
                state.PopStates();
            }
            if (realColumnCount < columnCount)
            {
                // remove exceesss
                for (int i = children.Count; i-- > 0;)
                {
                    if (i % columnCount >= realColumnCount)
                    {
                        children[i].Flush();
                        children.RemoveAt(i);
                    }
                }
                while (columnCount > realColumnCount)
                {
                    if (columnCount < columnMetrics.Length)
                        columnMetrics[columnCount] = default;
                    columnCount--;
                }
            }
            else if (realColumnCount > columnCount)
            {

            }
            if (columnCount == 0)
                return;
            // complete any columns left
            while (children.Count % columnCount != 0)
            {
                TabularCellAtom tcelll = TabularCellAtom.Get();
                tcelll.ProcessParameters(string.Empty, state, text, ref position);
                children.Add(tcelll);
            }
            if (lastrowindex + 1 < children.Count && (children[lastrowindex + 1] is TabularCellAtom tabu) && tabu.atom == null)
            {
                for (int i = Math.Max(0, lastrowindex - columnCount + 1); i <= lastrowindex; i++)
                {
                    if (children[i] is TabularCellAtom tatom)
                    {
                        tatom.bottom = tabu.top;
                    }
                }
                while (children.Count > lastrowindex + 1)
                {
                    children.RemoveAt(children.Count - 1);
                }
            }
            Debug.Assert(children.Count % columnCount == 0);
            if (therule != null)
                therule.Flush();
            if (!outerPadding)
            {
                var rowCount = children.Count / columnCount;
                // override all outer padding to zero
                for (int i = 0; i < columnCount; i++)
                {
                    // top then bottom
                    ((TabularCellAtom)children[i]).top.padding = 0;
                    ((TabularCellAtom)children[children.Count - columnCount + i]).bottom.padding = 0;
                }
                for (int i = 0; i < rowCount; i++)
                {
                    // left then right
                    ((TabularCellAtom)children[i * columnCount]).left.padding = 0;
                    ((TabularCellAtom)children[(i + 1) * columnCount - 1]).right.padding = 0;
                }
            }
        }

        public void MergeAtom(LayoutAtom atom)
        {
            atom = TryToUnpack(atom) as LayoutAtom;
            children.AddRange(atom.children);
            atom.children.Clear();
            atom.Flush();
        }

        static float AlignmentCode(char c)
        {
            switch (c)
            {
                case 'R':
                case 'r':
                case 'b':
                case 'B':
                    return 1f;
                case 'C':
                case 'c':
                case 'm':
                case 'M':
                    return 0.5f;
                case 'L':
                case 'l':
                case 'p':
                case 'P':
                default:
                    return 0;
            }
        }
    }
}

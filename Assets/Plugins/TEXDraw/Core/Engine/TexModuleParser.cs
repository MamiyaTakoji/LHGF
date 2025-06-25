using System;
using System.Collections.Generic;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{
    public partial class TexModuleParser
    {
        public HashSet<char> ignoredChars;
        public HashSet<char> whiteSpaceChars;
        public HashSet<string> allCommands;
        public Dictionary<string, string> aliasCommands;
        public Dictionary<string, Func<Atom>> generalCommands;
        public Dictionary<string, GroupAtom.GroupState> environmentGroups;
        public Dictionary<string, InlineMacroAtom.InlineState> genericCommands;
        public Dictionary<string, InlineMacroAtom.ParametrizedMacro> macroCommands;

        public TexModuleParser()
        {
            // See TexModuleInitiator.cs
            Initialize();
        }


        /// <summary>
        /// Global parsing until the end of string
        /// </summary>
        public DocumentAtom Parse(string text, TexParserState state, bool mergeable = false)
        {
            var p = 0;
            return Parse(text, state, mergeable, ref p);
        }

        /// <summary>
        /// Main parsing loop until the end of string
        /// </summary>
        public DocumentAtom Parse(string text, TexParserState state, bool mergeable, ref int position)
        {
            SkipWhiteSpace(text, ref position);
            var doc = DocumentAtom.Get();
            doc.mergeable = mergeable;
            var layoutAtom = CreateAtom(state);
            state.LayoutContainer.Push(layoutAtom);
            state.Environment.Push(TexEnvironment.Inline, true);

            // Simple macro get called oftenly
            Action newParagraph = () =>
            {
                
                {
                    state.Environment.Push(TexEnvironment.Inline, false);
                    layoutAtom.ProcessParameters(string.Empty, state);
                    doc.Add(layoutAtom);
                    state.LayoutContainer.Set(layoutAtom = CreateAtom(state));
                    state.Environment.Pop();
                }
            };

            while (position < text.Length)
            {
                Atom atom = ParseToken(text, state, ref position);
                if (atom is null)
                {
                    continue;
                }
                else if (atom is DocumentAtom subDoc)
                {
                    // merge

                    bool firstLine = true;
                    foreach (LayoutAtom subline in subDoc.children)
                    {
                        if (firstLine)
                        {
                            if (subDoc.mergeable)
                            {
                                (layoutAtom as ParagraphAtom)?.CleanupWord();
                                if (subline is MathAtom ma)
                                {
                                    var x = MathAtom.Get();
                                    x.ProcessParameters(string.Empty, state);
                                    x.children.AddRange(ma.children);
                                    layoutAtom.Add(x);
                                    doc.Add(layoutAtom);
                                    firstLine = false;
                                    continue;
                                }
                                else if (subline is ParagraphAtom && layoutAtom is MathAtom)
                                {
                                    layoutAtom.Add(subline);
                                    doc.Add(subline); // to fool the sideeffect of mergeable
                                    firstLine = false;
                                    continue;
                                }
                                else 
                                {
                                    subline.children.InsertRange(0, layoutAtom.children);
                                    layoutAtom.children.Clear();
                                    layoutAtom.Flush();
                                }
                            }
                            else if (layoutAtom.HasContent())
                            {
                                state.Environment.Push(TexEnvironment.Inline, false);
                                layoutAtom.ProcessParameters(string.Empty, state);
                                state.Environment.Pop();
                                doc.Add(layoutAtom);
                            }
                            else
                            {
                                layoutAtom.Flush();
                            }
                            firstLine = false;
                        }
                        doc.Add(subline);
                        state.LayoutContainer.Set(layoutAtom = subline);
                    }

                    if (subDoc.mergeable)
                        doc.children.RemoveAt(doc.children.Count - 1);
                    else
                    {
                        state.Environment.Push(TexEnvironment.Inline, false);
                        state.LayoutContainer.Set(layoutAtom = CreateAtom(state));
                        state.Environment.Pop();
                    }
                    subDoc.children.Clear();
                    subDoc.Flush();
                }
                else if (atom is BlockAtom subBlock)
                {
                    // put inline guards (blocks around)
                    if (layoutAtom.HasContent())
                        newParagraph();
                    layoutAtom.Add(subBlock);
                    newParagraph();
                }
                else if (atom is ParagraphBreakAtom)
                {
                    // commit new paragraph
                    atom.Flush();
                    newParagraph();
                }
                else
                {
                    layoutAtom.Add(atom);
                }
            }
            state.Environment.Pop();

            if (layoutAtom.HasContent() || !doc.HasContent())
            {
                if (!mergeable)
                    state.Environment.Push(TexEnvironment.Inline, false);
                layoutAtom.ProcessParameters(string.Empty, state);
                if (!mergeable)
                    state.Environment.Pop();
                doc.Add(layoutAtom);
            }
            else
            {
                layoutAtom.Flush();
            }
            state.LayoutContainer.Pop();
            return doc;
        }

        /// <summary>
        /// Temporal parsing that only seeks for next token.
        /// This ParseToken should assumes all condition are in inline mode.
        /// </summary>
        public Atom ParseToken(string text, TexParserState state, ref int position)
        {
            while (position < text.Length)
            {
                var ch = text[position];
                if (ch == commentChar) // Skip comments
                    SkipCurrentLine(text, ref position);
                else if (ignoredChars.Contains(ch))
                    position++; // Skip non-visible chars
                else
                    break;
            }
            if (position < text.Length)
            {
                var ch = text[position];
                var env = state.Environment.current;

                if (whiteSpaceChars.Contains(ch))
                {
                    if (ProcessSkipWhitespaces(text, ref position) >= 2)
                        return ParagraphBreakAtom.Get();
                    else
                        return SpaceAtom.Get(" ", state);
                }
                else if (ch == escapeChar)
                {
                    position++;
                    string cmd = LookForAWord(text, ref position);
                    if (cmd.Length == 0 && position < text.Length)
                    {
                        char oneCharSymbol = text[position++];
                        if (reservedChars.Contains(oneCharSymbol) && !aliasCommands.ContainsKey(Char2Str(oneCharSymbol)))
                            return ProcessOneCharCommand(state, oneCharSymbol);
                        else if (oneCharSymbol == '[')
                        {
                            var block = GroupAtom.Get();
                            block.ProcessParameters("[", state, text, ref position);
                            return TryToUnpack(block);
                        }
                        else if (oneCharSymbol == '(')
                        {
                            state.Environment.Push(TexEnvironment.MathMode | TexEnvironment.Inline, true);
                            state.PushMathStyle(x => TexMathStyle.Text);
                            state.Font.Push(state.Typeface.mathIndex);
                            position -= 2;
                            var m = Parse(ReadStringGroup(text, ref position, "\\(", "\\)"), state);
                            m.mergeable = true;

                            state.Font.Pop();
                            state.PopMathStyle();
                            state.Environment.Pop();

                            return m;
                        }
                        else
                            cmd = Char2Str(oneCharSymbol);
                    }
                    if (aliasCommands.TryGetValue(cmd, out string alias))
                    {
                        cmd = alias;
                    }
                    if (state.GetMetadata(cmd, out Atom atom) && atom is ScriptedAtom satom)
                    {
                        atom = ScriptedAtom.Get(satom.macro);
                        atom.ProcessParameters(cmd, state, text, ref position);
                        return TryToUnpack(atom);
                    }
                    else if (generalCommands.TryGetValue(cmd, out Func<Atom> call))
                    {
                        Atom tmp = call();
                        if (tmp != null)
                        {
                            tmp.ProcessParameters(cmd, state, text, ref position);
                            return TryToUnpack(tmp);
                        }
                        return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (ch == beginGroupChar)
                {
                    state.PushStates();
                    var p = Parse(ReadGroup(text, ref position), state, true);
                    state.PopStates();
                    return TryToUnpack(p);
                }
                else if (ch == endGroupChar)
                {
                    position++;
                    return ParseToken(text, state, ref position);
                }
                else if (ch == alignmentChar)
                {
                    position++;
                    return AlignmentAtom.Get();
                }
                else if ((ch == subScriptChar || ch == superScriptChar))
                {
                    var bs = ScriptsAtom.RetrieveBaseScript(state.LayoutContainer.current) ?? SpaceAtom.Get("none", state);
                    if (bs is ScriptsAtom) {
                        bs = SpaceAtom.Get("none", state);
                    }
                    bs = ScriptsAtom.Get(bs);
                    bs.ProcessParameters(string.Empty, state, text, ref position);
                    return bs;
                }
                else if (ch == mathModeChar)
                {
                    if (position + 1 < text.Length && text[position + 1] == mathModeChar)
                    {
                        position += 2;
                        var block = GroupAtom.Get();
                        block.ProcessParameters("$$", state, text, ref position);
                        return TryToUnpack(block);
                    }
                    else
                    {
                        state.Environment.Push(TexEnvironment.MathMode, true);
                        state.PushMathStyle(x => TexMathStyle.Text);
                        state.Font.Push(state.Typeface.mathIndex);

                        var m = Parse(ReadGroup(text, ref position, '$', '$'), state);
                        m.mergeable = true;

                        state.Font.Pop();
                        state.PopMathStyle();
                        state.Environment.Pop();

                        return m;
                    }
                }
                else if (state.Font.current != -1 && TEXPreference.main.fonts[state.Font.current].metadata.preferUnicode)
                {
                    return (CharAtom.Get(text[position++], state.Font.current, state));
                }
                else if ((state.Environment.current.IsMathMode() ? mathPunctuationSymbols : textPunctuationSymbols).TryGetValue(text[position], out string symbol))
                {
                    position++;
                    var chc = TEXPreference.main.GetChar(symbol, state.Font.current);
                    return chc == null ? null : (Atom)SymbolAtom.Get(chc, state);
                }
                else
                {
                    string clip;
                    if (char.IsSurrogate(text[position]) && position < text.Length - 2)
                    {
                        clip = text.Substring(position, 2);
                        position += 2;
                    }
                    else
                    {
                        clip = Char2Str(text[position]);
                        position++;
                    }
                    if (TexUnicode.charMappings.TryGetValue(clip, out string clipVal))
                    {
                        if (TexUnicode.subScripts.Contains(clip) || TexUnicode.superScripts.Contains(clip))
                        {
                            return HandleUnicodeScripts(text, state, ref position);
                        }
                        var xxx = Parse(clipVal, state);
                        xxx.mergeable = true;
                        return TryToUnpack(xxx);
                    }
                    else
                    {
                        return CharAtom.Get(clip[0], state.Font.current, state);
                    }
                }
            }
            else
                return null;
        }

        static private Atom HandleUnicodeScripts(string text, TexParserState state, ref int position)
        {
            // special code handling scripts
            var bs = ScriptsAtom.RetrieveBaseScript(state.LayoutContainer.current) ?? SpaceAtom.Get("none", state);
            if (bs is ScriptsAtom)
            {
                bs = SpaceAtom.Get("none", state);
            }
            var bss = ScriptsAtom.Get(bs);
            bss.ProcessParameters(string.Empty, state);
            position--;
            while (position < text.Length)
            {
                if (TexUnicode.subScripts.Contains(Char2Str(text[position])) && bss.subscript == null)
                {
                    var p = CreateAtom(state);
                    var pp = 0;
                    state.PushMathStyle(x => x.GetSubscriptStyle());
                    while (position < text.Length && TexUnicode.subScripts.Contains(Char2Str(text[position])))
                    {
                        pp = 1;
                        var ppp = state.parser.Parse(TexUnicode.charMappings[Char2Str(text[position])].Substring(1), state, true, ref pp);
                        p.Add(TryToUnpack(ppp));
                        position++;
                    }
                    bss.subscript = p;
                    state.PopMathStyle();
                }
                else if (TexUnicode.superScripts.Contains(Char2Str(text[position])) && bss.superscript == null)
                {
                    var p = CreateAtom(state);
                    var pp = 0;
                    state.PushMathStyle(x => x.GetSubscriptStyle());
                    while (position < text.Length && TexUnicode.superScripts.Contains(Char2Str(text[position])))
                    {
                        pp = 1;
                        var ppp = state.parser.Parse(TexUnicode.charMappings[Char2Str(text[position])].Substring(1), state, true, ref pp);
                        p.Add(TryToUnpack(ppp));
                        position++;
                    }
                    bss.superscript = p;
                    state.PopMathStyle();
                }
                else break;
            }
            return bss;
        }

        private int ProcessSkipWhitespaces(string text, ref int position)
        {
            var ch = text[position];
            var newSpaces = 0;
            do
            {
                if (ch == newLineChar)
                    newSpaces++;
            } while (++position < text.Length && whiteSpaceChars.Contains(ch = text[position]));
            return newSpaces;
        }

        private static CharAtom ProcessOneCharCommand(TexParserState state, char oneCharSymbol)
        {
            TexChar oneCharTex;
            if (state.parser.mathPunctuationSymbols.TryGetValue(oneCharSymbol, out string symbol) && (oneCharTex = TEXPreference.main.GetChar(symbol, state.Font.current)) != null)
                return (SymbolAtom.Get(oneCharTex, state));
            else
                return (CharAtom.Get(oneCharSymbol, state.Font.current, state));
        }


        private static LayoutAtom CreateAtom(TexParserState state)
        {
            if (state.Environment.current.IsMathMode())
                return MathAtom.Get();
            else
                return ParagraphAtom.Get();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TexDrawLib.TexParserUtility;

namespace TexDrawLib
{

    public partial class TexModuleParser
    {

        public readonly Dictionary<string, TexMathStyle> mathStyleVariants = new Dictionary<string, TexMathStyle>()
        {
            { "displaystyle", TexMathStyle.Display },
            { "textstyle", TexMathStyle.Text },
            { "scriptstyle", TexMathStyle.Script },
            { "scriptscriptstyle", TexMathStyle.ScriptScript },
        };
        public readonly Dictionary<string, (TexAssetStyle type, TexAssetStyleVariant variant)> styleVariants = new Dictionary<string, (TexAssetStyle, TexAssetStyleVariant)>()
        {
            { "rm", (TexAssetStyle.Normal, TexAssetStyleVariant.Family) },
            { "tt", (TexAssetStyle.Typewriter, TexAssetStyleVariant.Family) },
            { "sf", (TexAssetStyle.SansSerif, TexAssetStyleVariant.Family) },
            { "md", (TexAssetStyle.Normal, TexAssetStyleVariant.Series) },
            { "bf", (TexAssetStyle.Boldface, TexAssetStyleVariant.Series) },
            { "up", (TexAssetStyle.Normal, TexAssetStyleVariant.Shape) },
            { "sl", (TexAssetStyle.Slanted, TexAssetStyleVariant.Shape) },
            { "it", (TexAssetStyle.Italic, TexAssetStyleVariant.Shape) },
            { "sc", (TexAssetStyle.SmallCase, TexAssetStyleVariant.Shape) },
        };

        public readonly Dictionary<TexAssetStyleVariant, string> styleVariantsNaming = new Dictionary<TexAssetStyleVariant, string>()
        {
            { TexAssetStyleVariant.Family, "family" },
            { TexAssetStyleVariant.Series, "series" },
            { TexAssetStyleVariant.Shape, "shape" },
        };

        public readonly Dictionary<char, string> mathPunctuationSymbols = new Dictionary<char, string>()
        {
            { '+', "plus" },
            { '-', "minus" },
            { '*', "asteriskmath" },
            { '/', "slash" },
            { '=', "equal" },
            { '(', "parenleft" },
            { ')', "parenright" },
            { '[', "bracketleft" },
            { ']', "bracketright" },
            { '{', "braceleft" },
            { '}', "braceright" },
            { '<', "less" },
            { '>', "greater" },
            { '|', "bar" },
            { '.', "period" },
            { ',', "comma" },
            { ':', "colon" },
            { ';', "semicolon" },
            { '`', "quoteleft" },
            { '\'', "quoteright" },
            { '"', "quotedblright" },
            { '?', "question" },
            { '!', "exclam" },
            { '@', "at" },
            { '#', "numbersign" },
            { '$', "dollar" },
            { '%', "percent" },
            { '&', "ampersand" },
            { '1', "one" },
            { '2', "two" },
            { '3', "three" },
            { '4', "four" },
            { '5', "five" },
            { '6', "six" },
            { '7', "seven" },
            { '8', "eight" },
            { '9', "nine" },
            { '0', "zero" },
        };

        public readonly Dictionary<char, string> textPunctuationSymbols = new Dictionary<char, string>()
        {
            { '+', "plus" },
            { '-', "hyphen" },
            { '*', "asteriskmath" },
            { '/', "slash" },
            { '=', "equal" },
            { '(', "parenleft" },
            { ')', "parenright" },
            { '[', "bracketleft" },
            { ']', "bracketright" },
            { '{', "braceleft" },
            { '}', "braceright" },
            { '<', "less" },
            { '>', "greater" },
            { '|', "bar" },
            { '.', "period" },
            { ',', "comma" },
            { ':', "colon" },
            { ';', "semicolon" },
            { '`', "quoteleft" },
            { '\'', "quoteright" },
            { '"', "quotedblright" },
            { '?', "question" },
            { '!', "exclam" },
            { '@', "at" },
            { '#', "numbersign" },
            { '$', "dollar" },
            { '%', "percent" },
            { '&', "ampersand" },
            { '1', "one" },
            { '2', "two" },
            { '3', "three" },
            { '4', "four" },
            { '5', "five" },
            { '6', "six" },
            { '7', "seven" },
            { '8', "eight" },
            { '9', "nine" },
            { '0', "zero" },
        };

        public readonly Dictionary<string, (string, string)> matrixDelimVariants = new Dictionary<string, (string, string)>()
        {
            { "pmatrix", ("parenleft", "parenright") },
            { "bmatrix", ("bracketleft", "bracketright") },
            { "Bmatrix", ("braceleft", "braceright") },
            { "vmatrix", ("bar", "bar") },
            { "Vmatrix", ("bardbl", "bardbl") },
        };

        public readonly Dictionary<string, CharType> symbolOverrides = new Dictionary<string, CharType>()
        {
            { "mathord", CharType.Ordinary },
            { "mathop", CharType.BigOperator },
            { "mathbin", CharType.Operator },
            { "mathrel", CharType.Relation },
            { "mathopen", CharType.OpenDelimiter },
            { "mathclose", CharType.CloseDelimiter },
            { "mathpunct", CharType.Punctuation },
            { "mathinner", CharTypeInternal.Inner },
        };

        public readonly string[] colorVariants = new string[] {
            "red", "cyan", "blue", "darkblue", "lightblue", "purple", "yellow",
            "lime", "fuchsia", "white", "silver", "grey", "black", "orange",
            "brown", "maroon", "green", "olive", "navy", "teal", "aqua", "magenta"
        };

        public readonly Dictionary<string, string> overUnderVariants = new Dictionary<string, string> {
            { "underset", "" },
            { "underbrace", "braceright" },
            { "underbracket", "bracketright" },
            { "underparen", "parenright" },
            { "underangle", "angbracketright" },
            { "underleftarrow", "arrowleft" },
            { "underrightarrow", "arrowright" },
            { "underleftrightarrow", "arrowboth" },
            { "overset", "" },
            { "overbrace", "braceleft" },
            { "overbracket", "bracketleft" },
            { "overparen", "parenleft" },
            { "overangle", "angbracketleft" },
            { "overleftarrow", "arrowleft" },
            { "overrightarrow", "arrowright" },
            { "overleftrightarrow", "arrowboth" },
        };

        public readonly string[] delimiterVariants = new string[] { "big", "Big", "bigg", "Bigg" };

        public readonly string[] whitespaceVariants = new string[] { " ", ",", ":", ";", "!", "s", "w", "quad", "qquad", "enskip" };

        public readonly string[] regularFunctions = new string[] {
                "arccos", "cos", "csc", "exp", "ker", "limsup", "min", "sinh",
                "arcsin", "cosh", "deg", "gcd", "lg", "ln", "Pr", "sup",
                "arctan", "cot", "det", "hom", "lim", "log", "sec", "tan",
                "arg", "coth", "dim", "inf", "liminf", "max", "sin", "tanh"
        };

        public readonly HashSet<string> limitFunctions = new HashSet<string> {
                "det", "gcd", "inf", "lim", "liminf", "limsup", "max", "min", "Pr", "sup"
        };

        public readonly HashSet<string> integralSymbols = new HashSet<string>
        {
            "integraltext", "contintegraltext"
        };


        public const float sizeVariantsCoeff = 1.2f;
        public readonly Dictionary<string, float> sizeVariants = new Dictionary<string, float>()
        {
            { "tiny", -2 },
            { "small", -1 },
            { "normalsize", 0 },
            { "large", 1 },
            { "Large", 2 },
            { "LARGE", 3 },
            { "huge", 4 },
            { "Huge", 5 },
            { "HUGE", 6 }, // moresize
        };

        public void Initialize()
        {
            // Unlike before v5.x we index all commands now in dictionary rather
            // than hardcoding them. For simplicity and performance in mind.

            ignoredChars = new HashSet<char>() { '\0', '\r', '\x200c', '\t' };
            whiteSpaceChars = new HashSet<char>() { ' ', '\r', '\t', '\n' };
            macroCommands = new Dictionary<string, InlineMacroAtom.ParametrizedMacro>();
            aliasCommands = new Dictionary<string, string>();
            {
                foreach (var item in TEXConfiguration.main.SymbolAliases)
                {
                    aliasCommands[item.key] = item.value;
                }
            }
            generalCommands = new Dictionary<string, Func<Atom>>();
            {
                if (TEXPreference.main.symbols.Count == 0)
                {
                    // Idk why sometimes it's lazy loaded
                    TEXPreference.main.PushToDictionaries();
                }

                foreach (var item in TEXPreference.main.symbols)
                {
                    var type = TEXPreference.main.GetChar(item.Value).type;
                    if (type == CharType.Accent)
                        generalCommands[item.Key] = AccentedAtom.Get;
                    else if (type == CharType.BigOperator)
                        generalCommands[item.Key] = BigOperatorAtom.Get;
                    else
                        generalCommands[item.Key] = SymbolAtom.Get;
                }

                foreach (var item in regularFunctions)
                    generalCommands[item] = limitFunctions.Contains(item) ? (Func<Atom>)BigOperatorAtom.Get : WordAtom.Get;
                foreach (var item in whitespaceVariants)
                    generalCommands[item] = SpaceAtom.Get;
                foreach (var key in NegateAtom.modes.Keys)
                    generalCommands[key] = NegateAtom.Get;
                foreach (var key in overUnderVariants)
                    generalCommands[key.Key] = BigOperatorAtom.Get;
                generalCommands["link"] = BoxedLinkAtom.Get;
                generalCommands["href"] = BoxedLinkAtom.Get;
                generalCommands["left"] = AutoDelimitedGroupAtom.Get;
                generalCommands["sqrt"] = RootAtom.Get;
                generalCommands["frac"] = FractionAtom.Get;
                generalCommands["nfrac"] = FractionAtom.Get;
                generalCommands["cfrac"] = FractionAtom.Get;
                generalCommands["accent"] = AccentedAtom.Get;
                generalCommands["accentdown"] = AccentedAtom.Get;
                generalCommands["accentabove"] = AccentedAtom.Get;
                generalCommands["raisebox"] = RaiseAtom.Get;

                generalCommands["softbreak"] = SoftBreakAtom.Get;
                generalCommands["-"] = HyphenBreak.Get;
                generalCommands["par"] = ParagraphBreakAtom.Get;
                generalCommands["verb"] = VerbAtom.Get;
                generalCommands["count"] = CounterAtom.Get;
                generalCommands["advance"] = CounterAtom.Get;
                generalCommands["cmultiply"] = CounterAtom.Get;
                generalCommands["cdivide"] = CounterAtom.Get;
                generalCommands["inlineinput"] = InputAtom.Get;
                generalCommands["hbox"] = BoxedAtom.Get;
                generalCommands["vbox"] = BoxedAtom.Get;
                generalCommands["vcenter"] = BoxedAtom.Get;
                generalCommands["vtop"] = BoxedAtom.Get;
                generalCommands["hfil"] = FlexibleAtom.Get;
                generalCommands["vfil"] = FlexibleAtom.Get;
                generalCommands["hfill"] = FlexibleAtom.Get;
                generalCommands["vfill"] = FlexibleAtom.Get;
                generalCommands["hss"] = FlexibleAtom.Get;
                generalCommands["vss"] = FlexibleAtom.Get;

                generalCommands["hline"] = HorizontalRuleAtom.Get;
                generalCommands["toprule"] = HorizontalRuleAtom.Get;
                generalCommands["bottomrule"] = HorizontalRuleAtom.Get;
                generalCommands["midrule"] = HorizontalRuleAtom.Get;

                generalCommands["input"] = InputAtom.Get;
                generalCommands["begingroup"] = GroupAtom.Get;
                generalCommands["begin"] = GroupAtom.Get;
                generalCommands["kern"] = KernAtom.Get;
                generalCommands["vskip"] = VerticalSkipAtom.Get;
                generalCommands["vspace"] = VerticalSkipAtom.Get;
                foreach (var item in TEXConfiguration.main.BlockMacros)
                    if (!string.IsNullOrEmpty(item.key))
                    {
                        var parsed = new InlineMacroAtom.ParametrizedMacro(item);
                        macroCommands[parsed.macroKey] = parsed;
                        generalCommands[parsed.macroKey] = DocumentAtom.Get;
                    }
                foreach (var item in TEXConfiguration.main.InlineMacros)
                    if (!string.IsNullOrEmpty(item.key))
                    {
                        var parsed = new InlineMacroAtom.ParametrizedMacro(item);
                        macroCommands[parsed.macroKey] = parsed;
                        generalCommands[parsed.macroKey] = InlineMacroAtom.Get;
                    }
            }
            environmentGroups = new Dictionary<string, GroupAtom.GroupState>();
            {
                environmentGroups["document"] = new GroupAtom.GroupState("document");
                environmentGroups["math"] = new GroupAtom.GroupState("math", (state) =>
                    {
                        state.Environment.Set(x => x.SetFlag(TexEnvironment.Inline, false));
                        state.Environment.Set(x => x.SetFlag(TexEnvironment.MathMode, true));
                        if (state.Paragraph.centeredMathBlock)
                        {
                            state.Paragraph.alignment = 0.5f;
                        }
                        state.Paragraph.justify = false;
                        state.Font.current = state.Typeface.mathIndex;
                    }
                );
                environmentGroups["verbatim"] = new GroupAtom.GroupState("verbatim", beginState: (state) =>
                    {
                        state.Paragraph.justify = false;
                        state.Paragraph.rightToLeft = false;
                        var font = TEXPreference.main.fonts[state.Font.current = state.Typeface.typewriterIndex];
                        if (font is TexFont)
                        {
                            var f = font as TexFont;
                            var r = state.Size.current / state.Ratio;
                            state.Paragraph.lineSpacing = 0;
                            state.Paragraph.paragraphSpacing = 0;
                            state.Typeface.lineAscent = (f.asset.ascent) * r / f.asset.fontSize;
                            state.Typeface.lineDescent = (f.asset.lineHeight - f.asset.ascent) * r / f.asset.fontSize;
                            state.Typeface.blankSpaceWidth = f.SpaceWidth((int)(state.Size.current * state.Document.retinaRatio) + 1) * r;
                        }
                    }, interpreter: (state, value, pos) =>
                    {
                        var row = DocumentAtom.Get();
                        var line = ParagraphAtom.Get();
                        while (pos < value.Length)
                        {
                            var ch = value[pos++];
                            if (state.parser.ignoredChars.Contains(ch))
                                continue;
                            else if (ch == '\n')
                            {
                                if (line.children.Count == 0)
                                {
                                    line.children.Add(CharAtom.Get(' ', state));
                                }
                                line.ProcessParameters(null, state);
                                row.Add(line);
                                line = ParagraphAtom.Get();

                            }
                            else if (state.parser.whiteSpaceChars.Contains(ch))
                                line.children.Add(CharAtom.Get(' ', state));
                            else
                                line.children.Add(CharAtom.Get(ch, state));
                        }
                        line.ProcessParameters(null, state);
                        row.Add(line);
                        return row;
                    }
                );
                environmentGroups["enumerate"] = new GroupAtom.GroupState("enumerate", beginState: (state) =>
                {
                    if (state.Metadata.TryGetValue("count5", out Atom counter) && counter is CounterAtom catom)
                    {
                        catom.number = 0;
                    }
                    state.SetMetadata("item", ScriptedAtom.Get(macroCommands["labelenumi"]));

                    state.Paragraph.leftPadding += state.Paragraph.bulletSpacing;
                    state.Paragraph.indent = true;
                    state.Paragraph.paragraphSpacing = 0;
                    state.Paragraph.indentSpacing = -state.Paragraph.bulletSpacing;
                });
                environmentGroups["itemize"] = new GroupAtom.GroupState("itemize", beginState: (state) =>
                {
                    state.SetMetadata("item", ScriptedAtom.Get(macroCommands["labelitemi"]));
                    state.Paragraph.leftPadding += state.Paragraph.bulletSpacing;
                    state.Paragraph.indent = true;
                    state.Paragraph.paragraphSpacing = 0;
                    state.Paragraph.indentSpacing = -state.Paragraph.bulletSpacing;
                });
                environmentGroups["flushleft"] = new GroupAtom.GroupState("flushleft", (state) =>
                   {
                       state.Paragraph.alignment = 0;
                       state.Paragraph.justify = false;
                   }
               );
                environmentGroups["flushright"] = new GroupAtom.GroupState("flushright", (state) =>
                    {
                        state.Paragraph.alignment = 1;
                        state.Paragraph.justify = false;
                    }
                );
                environmentGroups["center"] = new GroupAtom.GroupState("center", (state) =>
                    {
                        state.Paragraph.alignment = 0.5f;
                        state.Paragraph.justify = false;
                        state.Paragraph.indent = false;
                    }
                );
                environmentGroups["comment"] = new GroupAtom.GroupState("comment",
                    interpreter: (state, text, pos) =>
                    {
                        return SpaceAtom.Empty;
                    });
                environmentGroups["tabular"] = new GroupAtom.GroupState("tabular",
                    interpreter: (state, text, pos) =>
                    {
                        var table = TabularAtom.Get();
                        table.ProcessParameters("tabular", state, text, ref pos);
                        table.Interprete(text, ref pos, state);
                        return DocumentAtom.GetAsBlock(table, state);
                    });
                environmentGroups["align*"] = environmentGroups["aligned"] = environmentGroups["align"] = new GroupAtom.GroupState("align",
                    interpreter: (state, text, pos) =>
                    {
                        var table = TabularAtom.Get();
                        table.ProcessParameters("align", state, text, ref pos);
                        table.Interprete(text, ref pos, state);
                        return DocumentAtom.GetAsBlock(table, state);
                    });
                environmentGroups["matrix"] = new GroupAtom.GroupState("matrix",
                    interpreter: (state, text, pos) =>
                    {
                        var table = TabularAtom.Get();
                        table.ProcessParameters("matrix", state, text, ref pos);
                        table.Interprete(text, ref pos, state);
                        return table;
                    });
                environmentGroups["array"] = new GroupAtom.GroupState("array",
                    interpreter: (state, text, pos) =>
                    {
                        var table = TabularAtom.Get();
                        table.ProcessParameters("array", state, text, ref pos);
                        table.Interprete(text, ref pos, state);
                        return table;
                    });
                environmentGroups["minipage"] = new GroupAtom.GroupState("minipage",
                    interpreter: (state, text, pos) =>
                    {
                        var container = BoxedAtom.Get();
                        container.mode = "hbox";
                        container.relative = false;
                        char mode = 'm';
                        if (pos < text.Length && text[pos] == '[')
                        {
                            var m = ReadGroup(text, ref pos, '[', ']');
                            if (m.Length > 0) mode = m[0];
                        }
                        if (pos < text.Length && text[pos] == '{')
                        {
                            container.size = TexUtility.ParseUnit(ReadGroup(text, ref pos), state);
                        }
                        else
                        {
                            container.size = 0;
                        }
                        container.atom = state.parser.Parse(text, state, false, ref pos);
                        switch (mode)
                        {
                            case 'm':
                            case 'c':
                                container = BoxedAtom.Get(container);
                                container.ProcessParameters("vcenter", state);
                                break;
                            case 't':
                                container = BoxedAtom.Get(container);
                                container.ProcessParameters("vtop", state);
                                break;
                            case 'b':
                                container = BoxedAtom.Get(container);
                                container.ProcessParameters("vbox", state);
                                break;
                        }
                        return container;
                    });
                foreach (var item in matrixDelimVariants)
                {
                    string key = item.Key, left = item.Value.Item1, right = item.Value.Item2;
                    environmentGroups[key] = new GroupAtom.GroupState(key,
                    interpreter: (state, text, pos) =>
                    {
                        var table = TabularAtom.Get();
                        table.ProcessParameters("matrix", state, text, ref pos);
                        table.Interprete(text, ref pos, state);
                        var delims = AutoDelimitedGroupAtom.Get();
                        delims.left = SymbolAtom.Get(TEXPreference.main.GetChar(left), state);
                        delims.right = SymbolAtom.Get(TEXPreference.main.GetChar(right), state);
                        delims.atom = table;
                        delims.ProcessParameters(null, state);
                        return delims;
                    });
                }
                environmentGroups["smallmatrix"] = new GroupAtom.GroupState("smallmatrix",
                   interpreter: (state, text, pos) =>
                   {
                       state.PushMathStyle(x => TexMathStyle.ScriptScript);
                       var table = TabularAtom.Get();
                       table.ProcessParameters("matrix", state, text, ref pos);
                       table.Interprete(text, ref pos, state);
                       state.PopMathStyle();
                       return table;
                   });
            }
            genericCommands = new Dictionary<string, InlineMacroAtom.InlineState>();
            {
                foreach (var item in TEXPreference.main.fonts)
                {
                    genericCommands[item.name] = new InlineMacroAtom.InlineState(item.name,
                        (state) => state.Font.current = item.assetIndex);
                }
                foreach (var item in symbolOverrides)
                {
                    (int, Atom) overrides(TexParserState state, string value, int pos)
                    {
                        var atom = state.parser.ParseToken(value, state, ref pos);
                        return (pos, OverridedTypeAtom.Get(atom as SymbolAtom, item.Value));
                    };
                    genericCommands[item.Key] = new InlineMacroAtom.InlineState(item.Key,
                        interpreter: overrides,
                        requiresGroupContext: true
                       );
                }
                foreach (var item in styleVariants)
                {
                    var flag = item.Value.type;
                    var variant = item.Value.variant;
                    Action<TexParserState> styleProcessor(bool combining, bool reset, bool? math = null)
                    {
                        int subStyleProcessor(TexParserState state)
                        {
                            var font = TEXPreference.main.fonts[state.Font.current];

                            TexAssetStyle cumulativeFlag = font.metadata.style;
                            if (combining)
                                if (flag == TexAssetStyle.Normal)
                                    // clearing
                                    foreach (var vars in styleVariants)
                                    {
                                        if (vars.Value.variant == variant)
                                            cumulativeFlag &= ~vars.Value.type;
                                    }
                                else cumulativeFlag |= flag;
                            else
                                cumulativeFlag = flag;
                            if (reset)
                                font = TEXPreference.main.fonts[state.Typeface.defaultIndex];
                            else
                                font = font.metadata.baseAsset != null ? font.metadata.baseAsset : font;
                            if (math != null)
                            {
                                var isCurrentMath = state.Environment.current.IsMathMode();
                                if ((bool)math ^ isCurrentMath)
                                {
                                    state.Environment.Set((x) => x.SetFlag(TexEnvironment.MathMode, (bool)math));
                                }
                            }
                            if (cumulativeFlag == TexAssetStyle.Normal)
                                return font.assetIndex;
                            foreach (var variantItem in font.metadata.variantAssets)
                                if (variantItem.metadata.style == cumulativeFlag)
                                    return variantItem.assetIndex;
                            foreach (var variantItem in font.metadata.variantAssets)
                                if (variantItem.metadata.style.HasFlag(cumulativeFlag))
                                    return variantItem.assetIndex;
                            return state.Font.current;
                        }
                        return (TexParserState state) =>
                        {
                            state.Font.current = subStyleProcessor(state);
                        };
                    }

                    genericCommands[item.Key] = new InlineMacroAtom.InlineState(item.Key, styleProcessor(false, false));
                    genericCommands["text" + item.Key] = new InlineMacroAtom.InlineState("text" + item.Key, styleProcessor(false, true, false), requiresGroupContext: true);
                    genericCommands["math" + item.Key] = new InlineMacroAtom.InlineState("math" + item.Key, styleProcessor(false, true, true), requiresGroupContext: true);
                    genericCommands[item.Key + styleVariantsNaming[variant]] = new InlineMacroAtom.InlineState(item.Key, styleProcessor(true, false));
                }
                foreach (var item in sizeVariants)
                {
                    var ratio = Mathf.Pow(sizeVariantsCoeff, item.Value);
                    genericCommands[item.Key] = new InlineMacroAtom.InlineState(item.Key, (state) => state.Size.current = ratio * state.Document.initialSize);
                }
                genericCommands["scriptsize"] = new InlineMacroAtom.InlineState("scriptsize", (state) => state.Size.current = state.Math.scriptRatio * state.Document.initialSize);
                genericCommands["scriptscriptsize"] = new InlineMacroAtom.InlineState("scriptscriptsize", (state) => state.Size.current = state.Math.scriptScriptRatio * state.Document.initialSize);
                foreach (var item in mathStyleVariants)
                {
                    genericCommands[item.Key] = new InlineMacroAtom.InlineState(item.Key, (state) => state.Environment.current.SetMathStyle(item.Value));
                }
                genericCommands["fontsize"] = new InlineMacroAtom.InlineState("fontsize", interpreter: (state, value, pos) =>
                {
                    state.Size.current = TexUtility.ParseUnit(value, ref pos, state);
                    return (pos, null);
                });
                genericCommands["normalfont"] = new InlineMacroAtom.InlineState("normalfont", stateProcessor: (state) =>
                {
                    state.Font.current = state.Typeface.defaultIndex;
                });
                genericCommands["normalmath"] = new InlineMacroAtom.InlineState("normalmath", stateProcessor: (state) =>
                {
                    state.Font.current = state.Typeface.mathIndex;
                });
                genericCommands["mathbb"] = new InlineMacroAtom.InlineState("mathbb", stateProcessor: (state) =>
                {
                    state.Font.current = TEXPreference.main.GetFontByID("msbm")?.assetIndex ?? 0;
                }, requiresGroupContext: true);
                genericCommands["mathcal"] = new InlineMacroAtom.InlineState("mathcal", stateProcessor: (state) =>
                {
                    state.Font.current = TEXPreference.main.GetFontByID("cmsy")?.assetIndex ?? 0;
                }, requiresGroupContext: true);
                for (int intl = 2; intl <= 4; intl++)
                {
                    int inti = intl;
                    string ints = new string('i', inti) + "nt";
                    genericCommands[ints] = new InlineMacroAtom.InlineState(ints, interpreter: (state, value, pos) =>
                    {
                        var box = BigOperatorAtom.Get();
                        var row = RowAtom.Get();
                        var sym = state.Environment.current.GetMathStyle() < TexMathStyle.Text ? "integraldisplay" : "integraltext";
                        var syb = SymbolAtom.Get(TEXPreference.main.GetChar(sym), state);
                        for (int i = 0; i < inti; i++)
                            row.Add(syb);
                        box.atom = OverridedTypeAtom.Get(row, CharType.BigOperator);
                        box.ProcessParameters(ints, state, value, ref pos);
                        return (pos, box);
                    });
                }
                genericCommands["idotsint"] = new InlineMacroAtom.InlineState("idotsint", interpreter: (state, value, pos) =>
                {
                    var box = BigOperatorAtom.Get();
                    var row = RowAtom.Get();
                    var sym = state.Environment.current.GetMathStyle() < TexMathStyle.Text ? "integraldisplay" : "integraltext";
                    var syb = BoxedAtom.Get(SymbolAtom.Get(TEXPreference.main.GetChar(sym), state));
                    syb.ProcessParameters("vcenter", state);
                    syb = BoxedAtom.Get(syb);
                    syb.ProcessParameters("hbox", state);
                    syb.relative = true;
                    syb.size = 8 * state.Ratio; // padding
                    var syb2 = SymbolAtom.Get(TEXPreference.main.GetChar("periodcentered"), state);
                    row.Add(syb);
                    row.Add(syb2);
                    row.Add(syb2);
                    row.Add(syb2);
                    row.Add(syb);
                    box.atom = OverridedTypeAtom.Get(row, CharType.BigOperator);
                    box.ProcessParameters("idotsint", state, value, ref pos);
                    return (pos, box);
                });

                foreach (var item in colorVariants)
                {
                    ColorUtility.TryParseHtmlString(item, out Color color);
                    genericCommands[item] = new InlineMacroAtom.InlineState(item, (state) =>
                    {
                        state.Color.current = color;
                    });
                }
                for (int i = 0; i < delimiterVariants.Length; i++)
                {
                    var item = delimiterVariants[i];
                    var index = i;
                    genericCommands[item] = new InlineMacroAtom.InlineState(item, interpreter: (state, value, pos) =>
                    {
                        var del = state.parser.ParseToken(value, state, ref pos);
                        if (del is CharAtom ch && mathPunctuationSymbols.TryGetValue(ch.character, out string putname))
                        {
                            del.Flush();
                            del = SymbolAtom.Get(TEXPreference.main.GetChar(putname), state);
                        }
                        if (del is SymbolAtom dels)
                        {
                            TexChar meta = dels.metadata;
                            {
                                del.Flush();
                                del = DelimitedSymbolAtom.Get(meta, state, index + 1);
                            }
                        }

                        return (pos, del);
                    });
                }

                (int, Atom) colorProcessor(TexParserState state, string value, int pos)
                {
                    state.Color.current = ColorAtom.ParseColor(value, ref pos);
                    return (pos, null);
                }

                (int, Atom) caseProcessor(TexParserState state, string value, int pos, Action<CharAtom> replacer)
                {
                    var atom = state.parser.ParseToken(value, state, ref pos);
                    if (atom is LayoutAtom)
                    {
                        var latom = atom as LayoutAtom;
                        for (int i = 0; i < latom.children.Count; i++)
                        {
                            var litem = latom.children[i];
                            if (litem is WordAtom watom)
                            {
                                for (int j = 0; j < watom.children.Count; j++)
                                {
                                    if (watom.children[j] is CharAtom wcatom)
                                        replacer(wcatom);
                                }
                            }
                            else if (litem is CharAtom wcatom)
                            {
                                replacer(wcatom);
                            }
                        }
                    }
                    return (pos, atom);
                }

                (int, Atom) numberProcessor(TexParserState state, string value, int pos, Func<int, string> replacer)
                {
                    var atom = state.parser.ParseToken(value, state, ref pos);
                    int number = 0;
                    if (atom is CounterAtom catom)
                    {
                        number = catom.number;
                    }
                    else if (atom is LayoutAtom watom)
                    {
                        foreach (var item in watom.children)
                            if (item is CharAtom wcatom)
                                number = number * 10 + (wcatom.character - '0');
                    }
                    else if (atom is CharAtom ccatom)
                    {
                        number = (ccatom.character - '0');
                    }

                    string str = replacer(number);
                    atom.Flush();
                    var wwatom = WordAtom.Get();
                    for (int i = 0; i < str.Length; i++)
                    {
                        wwatom.Add(CharAtom.Get(str[i], state));
                    }
                    return (pos, wwatom);
                }

                (int, Atom) setLengthProcessor(TexParserState state, string value, int pos)
                {
                    SkipWhiteSpace(value, ref pos);
                    var cmd = ReadGroup(value, ref pos).Trim();
                    var length = ReadGroup(value, ref pos).Trim();
                    if (cmd == "" || length == "") return (pos, null);
                    var val = TexUtility.ParseUnit(length, state);
                    switch (cmd)
                    {
                        case "\\parskip":
                            state.Paragraph.paragraphSpacing = val;
                            break;
                        case "\\parindent":
                            state.Paragraph.indentSpacing = val;
                            break;
                        case "\\leftmargin":
                            state.Paragraph.leftPadding = val;
                            break;
                        case "\\rightmargin":
                            state.Paragraph.rightPadding = val;
                            break;
                        case "\\baselineskip":
                            state.Paragraph.lineSpacing = val;
                            break;
                    }
                    return (pos, null);
                }

                genericCommands["setlength"] = new InlineMacroAtom.InlineState("setlength", interpreter: setLengthProcessor);
                genericCommands["color"] = new InlineMacroAtom.InlineState("color", interpreter: colorProcessor);
                genericCommands["textcolor"] = new InlineMacroAtom.InlineState("color", interpreter: colorProcessor, requiresGroupContext: true);
                genericCommands["displaystyle"] = new InlineMacroAtom.InlineState("displaystyle",
                    (state) => state.Environment.Set((x) => x.SetMathStyle(TexMathStyle.Display)));
                genericCommands["textstyle"] = new InlineMacroAtom.InlineState("textstyle",
                    (state) => state.Environment.Set((x) => x.SetMathStyle(TexMathStyle.Text)));
                genericCommands["raggedleft"] = new InlineMacroAtom.InlineState("raggedleft",
                    (state) => { state.Paragraph.alignment = 0f; state.Paragraph.justify = false; });
                genericCommands["raggedright"] = new InlineMacroAtom.InlineState("raggedright",
                    (state) => { state.Paragraph.alignment = 1f; state.Paragraph.justify = false; });
                genericCommands["centering"] = new InlineMacroAtom.InlineState("centering",
                    (state) => { state.Paragraph.alignment = 0.5f; state.Paragraph.justify = false; });
                genericCommands["justifying"] = new InlineMacroAtom.InlineState("justifying",
                    (state) => { state.Paragraph.justify = true; });
                genericCommands["noindent"] = new InlineMacroAtom.InlineState("noindent",
                    (state) => { state.Paragraph.indent = false; });
                genericCommands["indent"] = new InlineMacroAtom.InlineState("indent",
                    (state) => { state.Paragraph.indent = true; });
                genericCommands["uppercase"] = new InlineMacroAtom.InlineState("uppercase", interpreter:
                    (state, value, pos) => caseProcessor(state, value, pos, (c) => c.character = char.ToUpperInvariant(c.character)));
                genericCommands["lowercase"] = new InlineMacroAtom.InlineState("lowercase", interpreter:
                    (state, value, pos) => caseProcessor(state, value, pos, (c) => c.character = char.ToLowerInvariant(c.character)));
                genericCommands["the"] = new InlineMacroAtom.InlineState("the", interpreter:
                    (state, value, pos) => numberProcessor(state, value, pos, (c) => c.ToString()));
                genericCommands["number"] = new InlineMacroAtom.InlineState("number", interpreter:
                    (state, value, pos) => numberProcessor(state, value, pos, (c) => c.ToString()));
                genericCommands["roman"] = new InlineMacroAtom.InlineState("roman", interpreter:
                    (state, value, pos) => numberProcessor(state, value, pos, (c) => TexUtility.ToRoman(c)));
                genericCommands["columncolor"] = new InlineMacroAtom.InlineState("columncolor", interpreter: (state, text, pos) =>
                {
                    var atom = ColorAtom.Get();
                    atom.ProcessParameters("columncolor", state, text, ref pos);
                    state.SetMetadata("columncolor", atom);
                    return (pos, null);
                });
                genericCommands["def"] = new InlineMacroAtom.InlineState("def", interpreter: (state, value, pos) =>
                {
                    var nextBrace = value.IndexOf(beginGroupChar, pos);
                    if (nextBrace >= 0)
                    {
                        pos = nextBrace;
                        ReadGroup(value, ref pos);
                        return (pos, null);
                    }
                    else
                    {
                        SkipCurrentLine(value, ref pos);
                        return (pos, null);
                    }
                });
                genericCommands["over"] = new InlineMacroAtom.InlineState("frac", interpreter: (state, value, pos) =>
                {
                    var lay = state.LayoutContainer.current;
                    for (int i = lay.children.Count; i-- > 0;)
                        lay.children[i].Flush();
                    lay.children.Clear();
                    var overlay = value.Substring(0, pos - "\\over".Length);
                    var underlay = value.Substring(pos);
                    pos += underlay.Length;
                    var frac = FractionAtom.Get();
                    frac.ProcessParameters("frac", state, overlay, underlay);
                    return (pos, frac);
                });
                genericCommands["choose"] = new InlineMacroAtom.InlineState("pmatrix", interpreter: (state, value, pos) =>
                {
                    var lay = state.LayoutContainer.current;
                    var overlay = MathAtom.Get();
                    overlay.ProcessParameters(string.Empty, state);
                    (overlay.children, lay.children) = (lay.children, overlay.children);
                    var underlay = Parse(value, state, true, ref pos);
                    var frac = FractionAtom.Get();
                    frac.ProcessParameters("nfrac", state);
                    frac.numerator = overlay;
                    frac.denominator = underlay;

                    var delims = AutoDelimitedGroupAtom.Get();
                    delims.left = SymbolAtom.Get(TEXPreference.main.GetChar("parenleft"), state);
                    delims.right = SymbolAtom.Get(TEXPreference.main.GetChar("parenright"), state);
                    delims.atom = frac;
                    return (pos, frac);
                });

                genericCommands["binom"] = new InlineMacroAtom.InlineState("binom", interpreter: (state, value, pos) =>
                {
                    var frac = FractionAtom.Get();
                    frac.ProcessParameters("nfrac", state, value, ref pos);

                    var delims = AutoDelimitedGroupAtom.Get();
                    delims.left = SymbolAtom.Get(TEXPreference.main.GetChar("parenleft"), state);
                    delims.right = SymbolAtom.Get(TEXPreference.main.GetChar("parenright"), state);
                    delims.atom = frac;
                    return (pos, delims);
                });
            }
            {
                foreach (var item in genericCommands)
                    generalCommands[item.Key] = InlineMacroAtom.Get;
            }
            {
                allCommands = new HashSet<string>(
                    generalCommands.Keys.Concat(aliasCommands.Keys)
                );
            }
        }
    }
}
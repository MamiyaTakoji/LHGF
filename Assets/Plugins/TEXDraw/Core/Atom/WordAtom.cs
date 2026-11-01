using System.Collections.Generic;
using System.Linq;

namespace TexDrawLib
{
    public class WordAtom : LayoutAtom
    {
        protected static Dictionary<string, string> ligatureTokens = new Dictionary<string, string>() {
                { "ffi", "ffi" },
                { "ffl", "ffl" },
                { "fi", "fi" },
                { "fl", "fl" },
                { "ff", "ff" },
                { "---", "emdash" },
                { "--", "endash" },
                { "-", "" },
                { "``", "quotedblleft" },
                { "''", "quotedblright" },
            };

        public override CharType Type => isMathFunc ? CharType.Punctuation : CharType.Ordinary;
        public override CharType LeftType => Type;
        public override CharType RightType => Type;

        protected static string[] ligatureFastTokens = ligatureTokens.Keys.ToArray();

        protected static int maxLigatureToken = ligatureTokens.Keys.Max(x => x.Length);

        public bool hasHyphen = false;

        public bool isMathFunc = false;

        public static WordAtom Get()
        {
            var atom = ObjPool<WordAtom>.Get();
            return atom;
        }

        public static WordAtom Get(string command, TexParserState state)
        {
            var atom = Get();
            var fIndex = state.Typeface.functionIndex;
            for (int i = 0; i < command.Length; i++)
            {
                var ch = CharAtom.Get(command[i], fIndex, state);
                atom.Add(ch);
            }
            return atom;
        }


        public void ProcessLigature()
        {
            var ligatureQueueSeek = 0;
            hasHyphen = false;
            while (ligatureQueueSeek < children.Count)
            {
                string matchedLigature = null;
                foreach (var item in ligatureFastTokens)
                {
                    if (ligatureQueueSeek + item.Length > children.Count) continue;

                    bool match = true;
                    for (int i = 0; i < item.Length && match; i++)
                    {
                        if (children[i + ligatureQueueSeek] is HyphenBreak hyp)
                        {
                            hasHyphen = true;
                            match = false;
                            break;
                        }
                        if ((children[i + ligatureQueueSeek] as CharAtom)?.character != item[i])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        matchedLigature = item;
                        break;
                    }
                }
                if (matchedLigature != null)
                {
                    var sample = (children[ligatureQueueSeek] as CharAtom);
                    var font = TEXPreference.main.fonts[sample.fontIndex];
                    if (font.symbols.TryGetValue(ligatureTokens[matchedLigature], out TexChar ch))
                    {
                        var atom = ObjPool<SymbolAtom>.Get();
                        atom.metadata = ch;
                        atom.character = ch.characterIndex;
                        atom.fontIndex = ch.fontIndex;
                        atom.resolution = sample.resolution;
                        atom.color = sample.color;
                        atom.size = sample.size;

                        for (int i = matchedLigature.Length; i-- > 0;)
                        {
                            children[ligatureQueueSeek + i].Flush();
                            children.RemoveAt(ligatureQueueSeek + i);
                        }
                        children.Insert(ligatureQueueSeek, atom);
                    }
                    if (matchedLigature[0] == '-')
                    {
                        var hyp = HyphenBreak.Get();
                        hyp.Copy(children[ligatureQueueSeek] as CharAtom);
                        hyp.show = true;
                        children[ligatureQueueSeek] = hyp;
                        hasHyphen = true;
                    }
                }
                ligatureQueueSeek++;
            }
        }

        public void Add(List<CharAtom> charBuildingBlock)
        {
            foreach (var item in charBuildingBlock)
            {
                Add(item);
            }
        }

        public override void Flush()
        {
            ObjPool<WordAtom>.Release(this);
            word = null;
            hasHyphen = false;
            isMathFunc = false;
            base.Flush();
        }

        public string word;

        public override Box CreateBox(TexBoxingState state)
        {
            var box = HorizontalBox.Get();
            for (int i = 0; i < children.Count; i++)
            {
                box.Add(children[i].CreateBox(state));
            }
            return box;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            if (!string.IsNullOrEmpty(this.word = command))
            {
                var fIndex = state.Typeface.functionIndex;
                command = HandleSpecialWordCase(command);
                for (int i = 0; i < command.Length; i++)
                {
                    var ch = command[i] == ' ' ? (Atom)SpaceAtom.Get("w", state) : CharAtom.Get(command[i], fIndex, state);
                    Add(ch);
                }
                isMathFunc = true;
            }
        }

        static string HandleSpecialWordCase(string cmd)
        {
            // this is dirty but idk man
            switch (cmd)
            {
                case "limsup":
                    return "lim sup";
                case "liminf":
                    return "lim inf";
                default:
                    return cmd;
            }
        }
    }
}

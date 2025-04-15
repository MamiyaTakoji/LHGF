using System;

namespace TexDrawLib
{
    public class AccentedAtom : InlineAtom
    {

        // Atom representing accent symbol to place over base atom.
        public SymbolAtom accent;

        float accentMargin;

        bool forcedown = false;

        public static AccentedAtom Get()
        {
            return ObjPool<AccentedAtom>.Get();
        }

        public static AccentedAtom Get(string command, TexParserState state)
        {
            return ObjPool<AccentedAtom>.Get();
        }

        public override void Flush()
        {
            ObjPool<AccentedAtom>.Release(this);
            atom?.Flush();
            accent?.Flush();
            forcedown = false;
            atom = null;
            accent = null;
        }


        public override Box CreateBox(TexBoxingState state)
        {
            //// Create box for base atom.
            var baseBox = atom == null ? StrutBox.Empty : atom.CreateBox(state);

            if (accent == null)
            {
                return baseBox;
            }

            //// Find character of best scale for accent symbol.
            var acct = (CharBox)accent.CreateBoxMinWidth(baseBox.width);

            var resultBox = HorizontalBox.Get();


            //if ((acct.width) < baseBox.width)
            {
                var delta = baseBox.width - (acct.width);
                acct.italic += delta / 2;
                acct.bearing -= delta / 2;
                acct.width += delta;
            }
            if (forcedown)
            {
                acct.depth += acct.height + baseBox.depth;
                acct.height = -baseBox.depth;
            } 
            else if (acct.depth < 0) 
            {
                if ((-acct.depth - baseBox.height) < accentMargin)
                {
                    var offset = accentMargin - (-acct.depth - baseBox.height);
                    acct.depth -= offset;
                    acct.height += offset;
                }
            }
            //// Create and add box for accent symbol.
            resultBox.Add(acct);

            resultBox.Add(StrutBox.Get(-acct.width, 0, 0));

            //// Centre and add box for base atom. Centre base box and accent box with respect to each other.

            resultBox.Add(baseBox);

            // Adjust height and depth of result box.

            return resultBox;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            accentMargin = state.Math.upperMinimumDistance * state.Ratio / 2;
            TexChar symbol = null;
            TexParserUtility.SkipWhiteSpace(value, ref position);
            if (command.Length == 1)
            {
            }
            else if (command == "accent" || command == "accentdown" || command == "accentabove")
            {
                var n = state.parser.ParseToken(value, state, ref position);
                if (n is SymbolAtom ns)
                {
                    symbol = ns.metadata;
                    n.Flush();
                }
                else if (n is AccentedAtom na)
                {
                    accent = na.accent;
                    atom = na.atom;
                    na.accent = null;
                    na.atom = null;
                    n.Flush();
                }
                forcedown = command == "accentdown";
                if (command == "accentabove")
                    accentMargin *= -2; // amstrong
            }
            else
            {
                symbol = TEXPreference.main.GetChar(command);
                accent = SymbolAtom.Get(symbol, state);
            }
            if (symbol != null && atom == null)
            {
                atom = state.parser.ParseToken(value, state, ref position);
                if (atom is CharAtom cha)
                {
                    atom = ChangeToDotless(cha, state);
                }
                else if (atom is RowAtom ra && ra.children.Count == 1 && ra.children[0] is CharAtom cha2)
                {
                    atom = ChangeToDotless(cha2, state);
                    ra.children.Clear();
                    ra.Flush();
                }
                else if (atom is ParagraphAtom pa)
                {
                    pa.CleanupWord();
                    if (pa.children.Count == 1 && pa.children[0] is WordAtom wa && wa.children.Count == 1 && wa.children[0] is CharAtom cha3)
                    {
                        atom = ChangeToDotless(cha3, state);
                        wa.children.Clear();
                        pa.Flush();
                    }
                }
            }
        }

        static CharAtom ChangeToDotless(CharAtom ch, TexParserState state)
        {
            var c = ch.character;
            switch (c)
            {
                case 'i':
                    ch.Flush();
                    return SymbolAtom.Get(TEXPreference.main.GetChar("dotlessi"), state);
                case 'j':
                    ch.Flush();
                    return SymbolAtom.Get(TEXPreference.main.GetChar("dotlessj"), state);
                default:
                    return ch;
            }
        }
    }
}

// Atom representing single character in specific text style.
using UnityEngine;

namespace TexDrawLib
{
    public class CharAtom : DisplayAtom
    {

        public override CharType Type => CharType.Ordinary;

        public static CharAtom Get(char character, TexParserState state)
        {
            return Get(character, state.Font.current, state);
        }
        public static CharAtom Get(char character, int fontIndex, TexParserState state)
        {
            var atom = ObjPool<CharAtom>.Get();
            atom.character = character;
            atom.fontIndex = fontIndex;
            atom.resolution = state.Document.retinaRatio;
            atom.color = state.Color.current;
            atom.size = state.Size.current;

            return atom;
        }

        public char character;

        public int fontIndex;

        public float size;

        public float resolution;

        public Color32 color;

        public override Box CreateBox(TexBoxingState state)
        {
            return CharBox.Get(fontIndex, character, size, resolution, color);
        }

        public override void Flush()
        {
            character = default;
            fontIndex = -1;
            size = 0;
            resolution = 1;
            ObjPool<CharAtom>.Release(this);
        }

          
        public override string ToString()
        {
            return base.ToString() + " " + new string(character, 1);
        }
    }
}

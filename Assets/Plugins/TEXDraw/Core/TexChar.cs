using System;
using UnityEngine.Serialization;

namespace TexDrawLib
{
  

    [System.Serializable]
    public class TexChar
    {
        [NonSerialized]
        public int index;
        [NonSerialized]
        public int fontIndex;
        [NonSerialized]
        public string fontName;

        public char characterIndex;

        public CharType type = CharType.Ordinary;

        [FormerlySerializedAs("symbolName")]
        public string symbol;

        public TexCharRef larger;

        public TexCharExtension extension;

        public TexAsset Font => TEXPreference.main.fonts[fontIndex];

        public static implicit operator TexCharRef(TexChar ch)
        {
            return new TexCharRef(ch.fontName, ch.characterIndex);
        }
    }

    [Serializable]
    public struct TexCharRef
    {
        public string font;
        public char character;

        public TexCharRef(string font, char character)
        {
            this.font = font;
            this.character = character;
        }

        public TexChar Get => TEXPreference.main.GetChar(font, character);

        public TexAsset GetFont => TEXPreference.main.fontnames[font];

        public bool Has => !string.IsNullOrEmpty(font);
    }

    [Serializable]
    public struct TexCharExtension
    {
        public bool enabled;
        public TexCharRef top;
        public TexCharRef mid;
        public TexCharRef bottom;
        public TexCharRef repeat;
        public bool horizontal;
    }
}

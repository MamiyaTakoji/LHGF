using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace TexDrawLib
{
    [Serializable]
    public struct TexRectOffset
    {
        public float left, right, top, bottom;

        public float horizontal => left + right;

        public float vertical => top + bottom;

        public TexRectOffset(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        public Rect Shrink(Rect r)
        {
            return new Rect(r.x + left, r.y + bottom, r.width - left - right, r.height - top - bottom);
        }

        public Rect Expand(Rect r)
        {
            return new Rect(r.x - left, r.y - bottom, r.width + left + right, r.height + top + bottom);
        }


    }

    public static class TexUtility
    {
        public const int frontBlockIndex = -2;

        public readonly static Dictionary<string, TexUnits> StringToTexUnitTable = new Dictionary<string, TexUnits> {
            { "px", TexUnits.Pixel },
            { "em", TexUnits.Em },
            { "pt", TexUnits.Point },
            { "pc", TexUnits.Pica },
            { "in", TexUnits.Inch },
            { "bp", TexUnits.BigPoint },
            { "cm", TexUnits.Centimeter },
            { "mm", TexUnits.Millimeter },
            { "dd", TexUnits.DidotPoint },
            { "cc", TexUnits.Cicero },
            { "sp", TexUnits.ScaledPoint },
        };

        public readonly static Dictionary<TexUnits, float> TexUnitToInch = new Dictionary<TexUnits, float> {

            /*
            *  pt point (baselines in this manual are 12 pt apart)
               pc pica (1 pc = 12 pt)
               in inch (1 in = 72.27 pt)
               bp big point (72 bp = 1 in)
               cm centimeter (2.54 cm = 1 in)
               mm millimeter (10 mm = 1 cm)
               dd didot point (1157 dd = 1238 pt)
               cc cicero (1 cc = 12 dd)
               sp scaled point (65536 sp = 1 pt)
            */

            { TexUnits.Pixel, 0 },
            { TexUnits.Em, 0 },
            { TexUnits.Point, 1 / 72.27f },
            { TexUnits.Pica, 12f / 72.27f },
            { TexUnits.Inch, 1 },
            { TexUnits.BigPoint, 1f / 72f },
            { TexUnits.Centimeter, 1f / 2.54f },
            { TexUnits.Millimeter, 1f / 25.4f },
            { TexUnits.DidotPoint, 1238f / ( 1157f * 72.27f) },
            { TexUnits.Cicero, 1238f / (1157f * 12 * 72.27f) },
            { TexUnits.ScaledPoint, 1f / (65536f * 72.27f) },
        };


        public static Color32 MultiplyColor(Color32 a, Color b)
        {
            return new Color32((byte)(a.r * b.r), (byte)(a.g * b.g), (byte)(a.b * b.b), (byte)(a.a * b.a));
        }

        public static float ParseUnit(string text, ref int position, TexParserState state)
        {
            return ParseUnit(TexParserUtility.LookForAMetric(text, ref position), state);
        }
        public static float ParseUnit(string text, TexParserState state)
        {
            return ParseUnit(text, state.Ratio * 72.27f, state.Document.initialSize);
        }

        public static float ParseUnit(string s, float PPI, float Rel)
        {
            if (s.Length < 1) return 0;

            if (s.Length > 2 && StringToTexUnitTable.TryGetValue(s.Substring(s.Length - 2), out TexUnits units) && TryParse(s.Substring(0, s.Length - 2), out float value))
            {
                switch (units)
                {
                    case TexUnits.Pixel:
                        return value;
                    case TexUnits.Em:
                        return value * Rel;
                    default:
                        return value * PPI * TexUnitToInch[units];
                }
            }
            else if (TryParse(s, out float f2))
            {
                return f2;
            }
            return 0;
        }

        public static void CentreBox(Box box, float axis = 0)
        {
            box.shift = (box.height - box.depth) / 2 - axis;
        }

        static readonly string[][] romanNumerals = new string[][]
            {
            new string[]{"", "i", "ii", "iii", "iv", "v", "vi", "vii", "viii", "ix"}, // ones
            new string[]{"", "x", "xx", "xxx", "xl", "l", "lx", "lxx", "lxxx", "xc"}, // tens
            new string[]{"", "c", "cc", "ccc", "cd", "d", "dc", "dcc", "dccc", "cm"}, // hundreds
            };

        static StringBuilder romanNumeralsBuilder = new StringBuilder();
        public static string ToRoman(int number)
        {
            romanNumeralsBuilder.Clear();
            if (number >= 1000)
            {
                romanNumeralsBuilder.Append('M');
                number -= 1000;
            }

            // split integer string into array and reverse array
            var intArr = number.ToString().Reverse().ToArray();
            var len = intArr.Length;
            var i = len;

            // starting with the highest place (for 3046, it would be the thousands
            // place, or 3), get the roman numeral representation for that place
            // and add it to the final roman numeral string
            while (i-- > 0)
            {
                romanNumeralsBuilder.Append(romanNumerals[i][intArr[i] - '0']);
            }

            return romanNumeralsBuilder.ToString();
        }

        public static bool TryParse(string s, out float result)
        {
            var format = System.Globalization.CultureInfo.InvariantCulture;
            return float.TryParse(s, System.Globalization.NumberStyles.Float, format, out result);
        }

        public static bool TryParse(string s, out int result)
        {
            var format = System.Globalization.CultureInfo.InvariantCulture;
            return int.TryParse(s, System.Globalization.NumberStyles.Number, format, out result);
        }
    }

    public static class ListExtensions
    {
        static public void EnsureCapacity<T>(this List<T> src, int cap)
        {
            if (src.Capacity < cap)
                src.Capacity = cap;
        }

        static public List<T> GetRangePool<T>(this List<T> source, int index, int count)
        {
            List<T> list = ListPool<T>.Get();

            list.Capacity = Math.Max(list.Capacity, count);

            for (int i = 0; i < count; i++)
            {
                list.Add(source[i + index]);
            }

            return list;
        }
    }

    public static class TexParserUtility
    {
        public const char escapeChar = '\\';
        public const char alignmentChar = '&';
        public const char mathModeChar = '$';
        public const char commentChar = '%';
        public const char parameterChar = '#';
        public const char beginGroupChar = '{';
        public const char endGroupChar = '}';
        public const char superScriptChar = '^';
        public const char subScriptChar = '_';

        public const char newLineChar = '\n';

        public static HashSet<char> reservedChars = new HashSet<char>() { escapeChar, alignmentChar, mathModeChar, commentChar, beginGroupChar, endGroupChar, parameterChar, superScriptChar, subScriptChar };

        public static void Assert(bool eval)
        {
#if UNITY_EDITOR
            if (!eval) throw new Exception("Assert evaluation returns false");
#endif
        }

        static public string LookForALetter(string text, ref int position)
        {
            if (position == text.Length)
                return string.Empty;
            else if (text[position] == beginGroupChar)
                return ReadGroup(text, ref position);
            else if (text[position] == escapeChar)
            {
                position++;
                return "\\" + LookForAWord(text, ref position);
            }
            else
            {
                return new string(text[position++], 1);
            }
        }

        static public string LookForAToken(string text, ref int position)
        {
            SkipWhiteSpace(text, ref position);
            if (position < text.Length)
            {
                if (text[position] == beginGroupChar)
                    return ReadGroup(text, ref position);
                else
                    return LookForAWordOrDigit(text, ref position);
            }
            return string.Empty;
        }

        static public string LookForAWord(string text, ref int position)
        {
            var startPosition = position;
            while (position < text.Length)
            {
                var ch = text[position];
                var isEnd = position == text.Length - 1;
                if (!char.IsLetter(ch) || isEnd)
                {
                    // Escape sequence has ended.
                    if (char.IsLetter(ch))
                        position++;
                    break;
                }
                position++;
            }
            return text.Substring(startPosition, position - startPosition);
        }

        static public string LookForAWordOrDigit(string text, ref int position)
        {
            var startPosition = position;
            while (position < text.Length)
            {
                var ch = text[position];
                var isEnd = position == text.Length - 1;
                if (!(char.IsLetterOrDigit(ch)) || isEnd)
                {
                    // Escape sequence has ended.
                    if (char.IsLetterOrDigit(ch))
                        position++;
                    break;
                }
                position++;
            }
            return text.Substring(startPosition, position - startPosition);
        }

        static public string LookForAMetric(string text, ref int position)
        {
            var startPosition = position;
            if (position < text.Length && text[position] == '{')
            {
                return ReadGroup(text, ref position);
            }
            while (position < text.Length)
            {
                var ch = text[position];
                var isEnd = position == text.Length - 1;
                if (!(char.IsLetterOrDigit(ch) || ch == '.' || ch == '-') || isEnd)
                {
                    // Escape sequence has ended.
                    if (char.IsLetterOrDigit(ch))
                        position++;
                    break;
                }
                position++;
            }
            return text.Substring(startPosition, position - startPosition);
        }

        static public void SkipWhiteSpace(string text, ref int position)
        {
            while (position < text.Length && char.IsWhiteSpace(text[position]))
                position++;
        }

        static public void SkipCurrentLine(string text, ref int position)
        {
            while (position < text.Length && (text[position++]) != newLineChar)
            {
            }
        }

        static public string ReadGroup(string text, ref int position, char openChar = beginGroupChar, char closeChar = endGroupChar)
        {
            if (position == text.Length)
                return string.Empty;

            var readCloseGroup = true;
            if (text[position] != openChar)
                readCloseGroup = false;
            else
                position++;
            var startPosition = position;
            var group = 0;

            while (position < text.Length && !(text[position] == closeChar && group == 0))
            {
                if (text[position] == escapeChar)
                {
                    position++;
                    if (position == text.Length)
                        break;
                }
                else if (text[position] == commentChar)
                    SkipCurrentLine(text, ref position);
                else if (text[position] == closeChar)
                    group--;
                else if (text[position] == openChar)
                    group++;
                position++;
            }

            if (position >= text.Length)
            {
                return text.Substring(startPosition);
            }

            if (readCloseGroup)
            {
                position++;
                return text.Substring(startPosition, position - startPosition - 1);
            }
            return text.Substring(startPosition, position - startPosition);
        }

        private static bool StartsWithOffset(this string s, string match, int offset)
        {
            if (s.Length - offset < match.Length)
            {
                return false;
            }
            for (int i = 0; i < match.Length; i++)
            {
                if (i + offset >= s.Length || s[i + offset] != match[i])
                {
                    return false;
                }
            }
            var end = match.Length + offset;
            if (end < s.Length && char.IsLetterOrDigit(s[end - 1]) && char.IsLetterOrDigit(s[end]))
            {
                return false; // must match whole word
            }
            return true;
        }

        static public string ReadStringGroup(string text, ref int position, string openString, string closeString)
        {
            if (position >= text.Length - closeString.Length)
            {
                position = text.Length;
                return string.Empty;
            }

            var readCloseGroup = true;
            if (!text.StartsWithOffset(openString, position))
                readCloseGroup = false;
            else
                position += openString.Length;
            var startPosition = position;
            var group = 0;

            while (position < text.Length && !(text.StartsWithOffset(closeString, position) && group == 0))
            {
                if (text.StartsWithOffset(openString, position))
                    group++;
                else if (text.StartsWithOffset(closeString, position))
                    group--;
                position++;
            }

            if (position == text.Length)
            {
                return text.Substring(startPosition);
            }

            if (readCloseGroup)
            {
                position += closeString.Length;
                return text.Substring(startPosition, position - startPosition - closeString.Length);
            }
            return text.Substring(startPosition, position - startPosition);
        }

        private static readonly string[] chr = new string[0xFFFF];

        static public string Char2Str(char c)
        {
            return chr[c] ?? (chr[c] = new string(c, 1));
        }
        public static Atom TryToUnpack(Atom atom)
        {         
            if (atom is InlineMacroAtom aaa)
            {
                atom = aaa.atom;
                aaa.atom = null;
                aaa.Flush();
            }
            if (atom is DocumentAtom daa && daa.children.Count == 1)
            {
                atom = daa.children[0];
                daa.children.Clear();
                daa.Flush();
            }
            if (atom is BlockAtom ba && ba.IsUnpackable)
            {
                atom = ba.atom;
                ba.atom = null;
                ba.Flush();
            }
            if (atom is RowAtom rw && rw.children.Count == 1)
            {
                // Unpack to retain the identity
                atom = rw.children[0];
                rw.children.Clear();
                rw.Flush();
            }
            if (atom is ParagraphAtom paa)
            {
                paa.CleanupWord();
                if (paa.children.Count == 1)
                {
                    atom = paa.children[0];
                    paa.children.Clear();
                    paa.Flush();
                }
            }
            if (atom is WordAtom waa && waa.children.Count == 1)
            {
                atom = waa.children[0];
                waa.children.Clear();
                waa.Flush();
            }
            return atom;
        }
    }

    public static class TexEnvironmentUtility
    {


        public static bool IsInline(this TexEnvironment env)
        {
            return (env & TexEnvironment.Inline) == TexEnvironment.Inline;
        }


        public static bool IsMathMode(this TexEnvironment env)
        {
            return (env & TexEnvironment.MathMode) == TexEnvironment.MathMode;
        }




        const int mathStyleBitShift = 32 - 4;

        public static TexMathStyle GetMathStyle(this TexEnvironment env)
        {
            return env.IsMathMode() ? (TexMathStyle)(((int)env & (0b111 << mathStyleBitShift)) >> mathStyleBitShift) : TexMathStyle.Display;
        }

        public static TexEnvironment SetMathStyle(this TexEnvironment env, TexMathStyle style)
        {
            return (TexEnvironment)((((int)env) & ~(0b111 << mathStyleBitShift)) | ((int)style << mathStyleBitShift));
        }

        public static TexEnvironment SetFlag(this TexEnvironment env, TexEnvironment flag, bool value)
        {
            if (value)
            {
                return env | flag;
            }
            else
            {
                return env & (~flag);
            }
        }

        public static void Push(this TexParserState.Param<TexEnvironment> param, TexEnvironment flag, bool value)
        {
            param.Push(param.current.SetFlag(flag, value));
        }

        public static bool IsCrampedStyle(this TexMathStyle style)
        {
            return (int)style % 2 == 1;
        }

        public static TexMathStyle GetCrampedStyle(this TexMathStyle style)
        {
            return (int)style % 2 == 1 ? style : style + 1;
        }

        public static TexMathStyle GetNumeratorStyle(this TexMathStyle style)
        {
            // page 141 texbook
            return style + 2 - 2 * ((int)style / 6);
        }

        public static TexMathStyle GetDenominatorStyle(this TexMathStyle style)
        {
            return (TexMathStyle)(2 * ((int)style / 2) + 1 + 2 - 2 * ((int)style / 6));
        }

        public static TexMathStyle GetRootStyle(this TexMathStyle style)
        {
            return (TexMathStyle)((int)style % 2 + (style < TexMathStyle.Script ? 4 : 6));
        }

        public static TexMathStyle GetSubscriptStyle(this TexMathStyle style)
        {
            return (TexMathStyle)(2 * ((int)style / 4) + 4 + 1);
        }

        public static TexMathStyle GetSuperscriptStyle(this TexMathStyle style)
        {
            return (TexMathStyle)(2 * ((int)style / 4) + 4 + ((int)style % 2));
        }
    }


    [Serializable]
    public struct ScaleOffset
    {
        public float scale;
        public float offset;
    }


    [Serializable]
    public class StringPair
    {
        [FormerlySerializedAs("find")]
        public string key;
        [FormerlySerializedAs("replace")]
        public string value;
    }

    [Serializable]
    public class FindReplace
    {
        public string find;
        public string replace;
    }
}

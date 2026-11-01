using System;
using System.Collections.Generic;

namespace TexDrawLib
{
    public enum ImportCharPresetsType
    {
        Legacy = 0,
        ASCII = 1,
        FullUnicode = 2,
        Alphanumeric = 3,
        Custom = -1,
    }

    public class TexCharPresets
    {
        public const string legacyChars = @"\xC0-\xCF \xB0 \xD1-\xD6 \xB7 \xD8-\xDC \xB5-\xB6 \xDF \xEF \x21-\x7E \xFF";
        public const string oldLegacy = "xC0-xCF,xB0,xD1-xD6,xB7,xD8-xDC,xB5-xB6,xDF,xEF,x21-x7E,xFF";

        public const string asciiChars = "X:01-7F";

        public const string fullChars = "<All>";

        public const string alphanumericChars = "X:30-39,41-5A,61-7A";

        public static string charsFromEnum(ImportCharPresetsType preset)
        {
            switch (preset)
            {
                case ImportCharPresetsType.Legacy: return legacyChars;
                case ImportCharPresetsType.Alphanumeric: return alphanumericChars;
                case ImportCharPresetsType.FullUnicode: return fullChars;
                case ImportCharPresetsType.ASCII: return asciiChars;
                default: return legacyChars;
            }
        }

        public static ImportCharPresetsType guessEnumPresets(string s)
        {
            switch (s)
            {
                case legacyChars: return ImportCharPresetsType.Legacy;
                case alphanumericChars: return ImportCharPresetsType.Alphanumeric;
                case fullChars: return ImportCharPresetsType.FullUnicode;
                case asciiChars: return ImportCharPresetsType.ASCII;
                default: return ImportCharPresetsType.Custom;
            }
        }

        public static char[] CharsFromString(string s)
        {
            var list = new List<char>();
            try
            {
                int i = 0;
                bool lastTokenIsRange = false;
                s += " ";
                while ( i < s.Length)
                {
                    if (s[i] == '\\')
                    {
                        i++;
                        if (i == s.Length)
                            break;
                        else if (s[i] == 'x' || s[i] == 'X')
                        {
                            var i2 = ++i;
                            char ch;
                            do
                            {
                                i++;
                                ch = s[i];
                            } while (i < s.Length && ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')));
                                var number = Convert.ToInt32(s.Substring(i2, i - i2), 16);
                            if (lastTokenIsRange)
                            {
                                var number2 = list[list.Count - 1];
                                while (++number2 <= number)
                                {
                                    list.Add(number2);
                                }
                                lastTokenIsRange = false;
                            }
                            else
                                list.Add((char)number);
                        }
                        else if (s[i] == '\\')
                        {
                            TexParserUtility.SkipCurrentLine(s, ref i);
                        }
                    }
                    else if (s[i] == ',' || char.IsWhiteSpace(s[i]))
                        i++;
                    else if (s[i] == '-')
                    {
                        i++;
                        lastTokenIsRange = true;
                    }
                    else
                        list.Add(s[i++]);
                }
            }
            catch (System.Exception)
            {

            }
            return list.ToArray();
        }
        //public static char[] CharsFromString(string s)
        //{
        //    if (s == oldLegacy)
        //        s = legacyChars;

        //    var ss = s.Split('\n');
        //    var list = new List<char>();
        //    foreach (var n in ss)
        //    {
        //        var inisial = n.Length > 2 ? n.Substring(0, 2) : "";

        //        if (inisial == "X:" || inisial == "D:" || inisial == "C:")
        //        {
        //            var isHex = inisial == "X:";
        //            var isChar = inisial == "C:";
        //            var isLastRange = false;
        //            var startPos = 2;
        //            int rangeStart = 0;
        //            //	try {
        //            var last = n.Length - 1;
        //            for (int i = 2; i < n.Length; i++)
        //            {
        //                var ch = n[i];
        //                if (char.IsWhiteSpace(ch))
        //                    continue;
        //                else if (ch == '-' || ch == ':')
        //                {
        //                    if (isLastRange)
        //                        break;
        //                    isLastRange = true;
        //                    rangeStart = isChar ? n[i - 1] : int.Parse(n.Substring(startPos, i - startPos), isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None);
        //                    startPos = i + 1;
        //                }
        //                else if (ch == ',' || ch == '&' || ch == ';' || i == last)
        //                {
        //                    if (i == last && !(ch == ',' || ch == '&' || ch == ';'))
        //                        i++;
        //                    var parsed = isChar ? n[i - 1] : int.Parse(n.Substring(startPos, i - startPos), isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None);
        //                    if (isLastRange)
        //                    {
        //                        for (int l = rangeStart; l < parsed; l++)
        //                        {
        //                            list.Add((char)l);
        //                        }
        //                    }
        //                    list.Add((char)parsed);
        //                    isLastRange = false;
        //                    startPos = i + 1;
        //                }
        //                else if (isChar || ((ch >= '0' && ch <= '9') || (isHex && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')))))
        //                    continue;
        //                else
        //                    break;
        //            }
        //            //	} catch (Exception) { }
        //        }
        //        else
        //        {
        //            foreach (var c in n)
        //            {
        //                if (!char.IsWhiteSpace(c))
        //                    list.Add(c);
        //            }
        //        }
        //    }
        //    return list.Distinct().Take(256).ToArray();
        //}

        /* OBSOLETE PROGRAM
        public static char[] charsFromString(string s)
        {
            if (string.IsNullOrEmpty(s))
                s = legacyChars;
            else if (s.Contains("All") || s.Contains("all") || s.Contains("ALL"))
                return new char[0];
            int pos = 0, start = 0;
            bool lastIsRange = false;
            var list = new List<char>(8);
            try
            {
                while (pos < s.Length)
                {
                    var ch = s[pos];
                    if (ch == '-' || ch == ':' || ch == ',' || ch == ';' || ch == '&' || pos == s.Length - 1)
                    {
                        string str;
                        if (pos == s.Length - 1)
                            str = s.Substring(start);
                        else
                            str = s.Substring(start, pos - start);
                        if (str.Length == 0)
                        {
                            pos++;
                            continue;
                        }
                        int parsed;
                        if (str[0] == 'x')
                            parsed = int.Parse(str.Substring(1), NumberStyles.AllowHexSpecifier);
                        else if ((str.Length > 1 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X')))
                            parsed = int.Parse(str.Substring(2), NumberStyles.AllowHexSpecifier);
                        else
                            parsed = int.Parse(str);

                        parsed = parsed > 0xffff ? 0xffff : parsed;
                        if (lastIsRange)
                        {
                            for (int i = list[list.Count - 1] + 1; i < parsed; i++)
                                list.Add((char)i);
                        }
                        list.Add((char)parsed);
                        lastIsRange = ch == '-' || ch == ':';
                        pos++;
                        start = pos;
                    }
                    else if ((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F') || ch == '.' || ch == 'x')
                        pos++;
                    else if (char.IsWhiteSpace(ch))
                        pos++;
                    else
                        break;
                }
            }
            catch (Exception) { }
            if (list.Count == 0)
                list.Add('\0');
            return list.Distinct().Take(256).ToArray();
        }
        */
    }
}

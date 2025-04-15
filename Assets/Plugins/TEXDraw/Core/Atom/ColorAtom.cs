using UnityEngine;
using static TexDrawLib.TexParserUtility;
namespace TexDrawLib
{
    public class ColorAtom : AbstractAtom
    {
        public Color32 color;

        public override Box CreateBox(TexBoxingState state)
        {
            return StrutBox.Empty;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            color = ParseColor(value, ref position);
        }

        public static Color32 ParseColor(string value, ref int position)
        {
            SkipWhiteSpace(value, ref position);
            var device = "unity";
            if (position < value.Length && value[position] == '[')
            {
                device = ReadGroup(value, ref position, '[', ']');
                if (device != "rgb" && device != "rgba" && device != "RGB" && device != "RGBA" && device != "gray" && device != "cmyk" && device != "unity")
                    return ParseColor(device, "unity");
                else
                    return ParseColor(LookForAToken(value, ref position), device);

            }
            else
                return ParseColor(LookForAToken(value, ref position), device);
        }

        public static Color32 ParseColor(string color, string device)
        {
            int seek = 0; int iter = 0;
            switch (device)
            {
                case "rgb":
                case "rgba":
                    Vector4 rgba = Vector4.one;
                    SkipWhiteSpace(color, ref seek);
                    while (seek < color.Length && iter < 4)
                    {
                        if (TexUtility.TryParse(LookForAMetric(color, ref seek), out float cf))
                            rgba[iter++] = cf;
                        seek++;
                        SkipWhiteSpace(color, ref seek);
                    }
                    return (Color)rgba;
                case "RGB":
                case "RGBA":
                    Color32 RGBA = Color.white;
                    SkipWhiteSpace(color, ref seek);
                    while (seek < color.Length && iter < 4)
                    {
                        if (byte.TryParse(LookForAMetric(color, ref seek), out byte cf))
                            switch (iter++)
                            {
                                case 0: RGBA.r = cf; break;
                                case 1: RGBA.g = cf; break;
                                case 2: RGBA.b = cf; break;
                                case 3: RGBA.a = cf; break;
                            }
                        seek++;
                        SkipWhiteSpace(color, ref seek);
                    }
                    return RGBA;
                case "gray":
                    return TexUtility.TryParse(color, out float cg) ? new Color(cg, cg, cg) : Color.white;
                case "cmyk":
                case "unity":
                default:
                    return ColorUtility.TryParseHtmlString(color, out Color cc) ? cc : Color.white;
            }
        }

        public override void Flush()
        {
            ObjPool<ColorAtom>.Release(this);
        }

        public static ColorAtom Get()
        {
            return ObjPool<ColorAtom>.Get();
        }
    }
}
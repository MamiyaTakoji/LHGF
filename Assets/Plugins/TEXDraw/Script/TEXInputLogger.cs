using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TexDrawLib;
using System.Text.RegularExpressions;
using Block = TEXInput.Block;
using System.Text;

[AddComponentMenu("TEXDraw/TEXInput Logger")]
public class TEXInputLogger : MonoBehaviour
{

    [NonSerialized]
    internal List<Block> blocks = new List<Block>();

    public string emptyPlaceholder = "";

    public TEXInput input;

    public string ReplaceString(string input)
    {
        blocks.Clear();

        var x = new StringBuilder();

        var i = 0;

        ReplaceString(input, x, ref i, input.Length);

        return x.ToString();
    }

    public void ReplaceString(string input, StringBuilder output, ref int pos, int length)
    {
        while (pos < length)
        {
            ReplaceNextToken(input, output, ref pos, length);
        }

    }

    public void ReplaceNextToken(string input, StringBuilder output, ref int pos, int length)
    {
        if (pos >= length) 
            return;
        var ch = input[pos];
        var dyn = new Block()
        {
            index = blocks.Count,
            start = pos++,
            length = 1,
        };
        var cmd = "\\link[" + blocks.Count + "]";
        if (ch == '\r' || ch == '\t')
        {
        }
        else if (ch == '\n')
        {
            blocks.Add(dyn);
            output.Append("\\\\\n" + cmd + "{}");
        }
        else if (ch == ' ')
        {
            blocks.Add(dyn);
            output.Append(cmd + "{\\ }");
        }
        else if (!TexParserUtility.reservedChars.Contains(ch))
        {
            blocks.Add(dyn);
            output.Append(cmd + "{" + ch + "}");
        }
        else if (ch == '$')
        {
            var end = input.IndexOf('$', pos);
            if (end > 0)
            {
                dyn.length = end - dyn.start;
                blocks.Add(dyn);
                output.Append(cmd + "{$");
                ReplaceString(input, output, ref pos, end);
                pos++;
                output.Append("$}");
                dyn.group = blocks.Count;
                blocks[dyn.index] = dyn;
            }
            else
            {
                pos--;
                ReplaceStringInRaw(input, output, ref pos, length);
            }
        }
        else if (ch == '{')
        {
            var end = pos;
            TexParserUtility.ReadGroup(input, ref end);
            output.Append(cmd + "{");
            if (end == length)
                output.Append("{\\tt\\braceleft}");
            else
                dyn.length = end - dyn.start + 1;
            blocks.Add(dyn);
            ReplaceString(input, output, ref pos, end);
            pos++;
            output.Append("}");
            dyn.group = blocks.Count;
            blocks[dyn.index] = dyn;
        }
        else if (ch == '}')
        {
            blocks.Add(dyn);
            output.Append(cmd + "{\\tt\\braceright}");
        }
        else if (ch == '^' || ch == '_')
        {
            if (pos < length)
            {
                blocks.Add(dyn);
                var x = new StringBuilder();
                ReplaceNextToken(input, x, ref pos, length);
                dyn.group = blocks.Count;
                blocks[dyn.index] = dyn;
                output.Append(ch + "{" + cmd + "{" + x.ToString() + "}}");
            }
            else
            {
                pos--;
                ReplaceStringInRaw(input, output, ref pos, length);
            }
        }
        else if (ch == '\\')
        {
            string word = TexParserUtility.LookForAWord(input, ref pos);
            var parser = this.input.tex.orchestrator.parser;
            if (word.Length == 0)
            {
                if (pos == length)
                {
                    blocks.Add(dyn);
                    output.Append(cmd + "{\\backslash}");
                }
                else if (input[pos] == '[' || input[pos] == '(')
                {
                    // To do
                }
            }
            else if (parser.allCommands.Contains(word))
            {
                dyn.length += word.Length;
                blocks.Add(dyn);
                output.Append(cmd + "{\\" + word);
                if (word == "frac" || word == "nfrac")
                {
                    ReplaceNextToken(input, output, ref pos, length);
                    ReplaceNextToken(input, output, ref pos, length);
                }
                else if (word == "sqrt" || word == "left" || word == "right")
                {
                    ReplaceNextToken(input, output, ref pos, length);
                }
                output.Append("}");
            }
            else
            {
                pos -= word.Length + 1;
                ReplaceStringInRaw(input, output, ref pos, pos + word.Length + 1);
            }
        }
    }

    public void ReplaceStringInRaw(string input, StringBuilder output, ref int pos, int length)
    {
        while (pos < length)
        {
            var ch = input[pos];
            var dyn = new Block()
            {
                index = blocks.Count,
                start = pos++,
                length = 1,
            };
            var cmd = "\\link[" + blocks.Count + "]";
            if (ch == '\r' || ch == '\t')
            {
                continue;
            }
            else if (ch == '\n')
            {
                blocks.Add(dyn);
                output.Append("\\\\\n" + cmd + "{}");
            }
            else if (ch == ' ')
            {
                blocks.Add(dyn);
                output.Append(cmd + "{\\ }");
            }
            else if (!TexParserUtility.reservedChars.Contains(ch))
            {
                blocks.Add(dyn);
                output.Append(cmd + "{\\tt{" + ch + "}}");
            }
            else if (ch == '\\')
            {
                blocks.Add(dyn);
                output.Append(cmd + "{\\tt{\\backslash}}");
            }
            else
            {
                blocks.Add(dyn);
                output.Append(cmd + "\\verb|" + ch + "|");
            }
        }
    }



    internal IEnumerable<Block> GetBlockMatches(int start, int length)
    {
        return blocks.Where((x) => x.start >= start && (x.end) <= (start + length));
    }

    internal Block GetBlockClosest(int pos)
    {
        Block b = blocks[0];
        Block bl = default;
        foreach (var i in blocks)
        {
            if (i.start >= pos)
                break;
            if (i.start > b.start)
            {
                b = i;
            }
            if (i.group > 0 && i.isInside(pos))
            {
                bl = i;
            }
        }
        return bl.group > 0 && b.start != pos - 1 ? bl : b;
    }

    internal Block GetNext(Block block)
    {
        if (block.index < blocks.Count - 1)
            return blocks[block.index + 1];
        else
            return block;
    }

    internal Block GetPrevious(Block block)
    {
        if (block.index > 0)
            return blocks[block.index - 1];
        else
            return block;
    }

    public int GetNearestPosition(Vector2 pos)
    {
        var param = input.tex.orchestrator;
        var boxes = param.rendererState.vertexLinks;

        // scan nearest
        (float dist, Block block, bool right) nearest = (99999, default, false);
        foreach ((string key, Rect area) in boxes)
        {
            var d = SqDistance(area, pos);
            if (d < nearest.dist && TexUtility.TryParse(key, out int index))
                nearest = (d, blocks[index], pos.x > area.center.x);
        }
        return nearest.block.start + (nearest.right ? nearest.block.length : 0);
    }

    private static float SqDistance(Rect r, Vector2 p)
    {
        var c = r.center;
        var dx = Math.Abs(Math.Abs(p.x - c.x) - r.width / 2);
        var dy = Math.Abs(Math.Abs(p.y - c.y) - r.height / 2);
        return dx * dx + dy * dy * 9; // yeah assume vertical is "heavier" here
    }
}

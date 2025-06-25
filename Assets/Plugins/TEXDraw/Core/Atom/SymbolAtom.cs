using System;
using static TexDrawLib.TexParserUtility;
namespace TexDrawLib
{
    public class AutoDelimitedGroupAtom : InlineAtom
    {
        public static AutoDelimitedGroupAtom Get()
        {
            return ObjPool<AutoDelimitedGroupAtom>.Get();
        }

        public float translateGlueCode(int code)
        {
            switch (code)
            {
                case 0:
                    return 0;
                case 1:
                    return thin;
                case 2:
                    return medium;
                case 3:
                default:
                    return thick;
            }
        }

        public override CharType Type => CharTypeInternal.Inner;
        public override CharType LeftType => CharType.OpenDelimiter;
        public override CharType RightType => CharType.CloseDelimiter;

        public override void Flush()
        {
            ObjPool<AutoDelimitedGroupAtom>.Release(this);
            base.Flush();
            if (left != null)
            {
                left.Flush();
                left = null;
            }
            if (right != null)
            {
                right.Flush();
                right = null;
            }
        }

        public SymbolAtom left, right;

        float thin, medium, thick, lineMedian;

        public override Box CreateBox(TexBoxingState state)
        {
            var box = HorizontalBox.Get();
            box.Add(atom?.CreateBox(state) ?? StrutBox.Empty);
            var height = Math.Max(box.depth + lineMedian, box.height - lineMedian) * 2;
            // var height = box.TotalHeight;
            var leftBox = left?.CreateBoxMinHeight(height);
            var rightBox = right?.CreateBoxMinHeight(height);
            if (leftBox != null)
            {
                TexUtility.CentreBox(leftBox, lineMedian);

                var gluel = translateGlueCode(TEXPreference.main.GetGlue(LeftType, atom.LeftType));
                if (gluel > 0)
                    box.Add(0, StrutBox.Get(gluel, 0, 0));

                box.Add(0, leftBox);
            }
            if (rightBox != null)
            {
                TexUtility.CentreBox(rightBox, lineMedian);

                var gluer = translateGlueCode(TEXPreference.main.GetGlue(atom.RightType, RightType));
                if (gluer > 0)
                    box.Add(StrutBox.Get(gluer, 0, 0));

                box.Add(rightBox);
            }
            return box;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var r = state.Ratio;
            thin = state.Math.thinSpace * r;
            medium = state.Math.mediumSpace * r;
            thick = state.Math.thickSpace * r;
            lineMedian = state.Typeface.lineMedian * r;
            if (value == null) return;
            // Capture all inside
            var lastpos = position;
            position -= command.Length + 1;
            var token = ReadStringGroup(value, ref position, "\\left", "\\right");
            var pos2 = 0;
            if (token.Length == 0)
            {
                position = lastpos;
                return;
            }
            var leftToken = state.parser.ParseToken(token, state, ref pos2);
            var rightToken = state.parser.ParseToken(value, state, ref position);
            {
                if (leftToken is CharAtom ch)
                {
                    if (ch.character == ':')
                    {
                        leftToken.Flush();
                        leftToken = SpaceAtom.Empty;
                    }
                    else if (state.parser.mathPunctuationSymbols.TryGetValue(ch.character, out string putname))
                    {
                        leftToken.Flush();
                        leftToken = SymbolAtom.Get(TEXPreference.main.GetChar(putname), state);
                    }
                }
            }
            {
                if (rightToken is CharAtom ch)
                {
                   
                    if (ch.character == ':')
                    {
                        rightToken.Flush();
                        rightToken = SpaceAtom.Empty;
                    }
                    else if (state.parser.mathPunctuationSymbols.TryGetValue(ch.character, out string putname))
                    {
                        rightToken.Flush();
                        rightToken = SymbolAtom.Get(TEXPreference.main.GetChar(putname), state);
                    }
                }
            }
            if (leftToken is SymbolAtom leftSToken) left = leftSToken;
            if (rightToken is SymbolAtom rightSToken) right = rightSToken;
        
            var s = state.parser.Parse(token, state, true, ref pos2);
            atom = TryToUnpack(s);
        }
    }
    public class DelimitedSymbolAtom : SymbolAtom
    {

        public static DelimitedSymbolAtom Get(TexChar ch, TexParserState state, int highStep)
        {
            Assert(ch != null);
            var atom = ObjPool<DelimitedSymbolAtom>.Get();
            atom.SetParameters(state, ch);
            atom.highStep = highStep;
            return atom;
        }

        public int highStep = 0;

        public override Box CreateBox(TexBoxingState state)
        {
            return highStep == 0 ? base.CreateBox(state) : CreateBoxStepped(highStep);
        }

        public override void Flush()
        {
            ObjPool<DelimitedSymbolAtom>.Release(this);
            highStep = 0;
            base.Flush();
        }
    }
    public class SymbolAtom : CharAtom
    {
        public static SymbolAtom Get()
        {
            return ObjPool<SymbolAtom>.Get();
        }

        public static SymbolAtom Get(TexChar ch, TexParserState state)
        {
            var atom = ObjPool<SymbolAtom>.Get();
            atom.SetParameters(state, ch);
            return atom;
        }

        public override void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            var ch = TEXPreference.main.GetChar(command, state.Font.current);
            SetParameters(state, ch);
            base.ProcessParameters(command, state, value, ref position);
        }

        public void SetParameters(TexParserState state, TexChar ch)
        {
            metadata = ch;
            character = ch.characterIndex;
            fontIndex = ch.fontIndex;
            delimiterPadding = state.Math.delimiterPadding * state.Ratio;
            resolution = state.Document.retinaRatio;
            color = state.Color.current;
            size = state.Size.current;
            lineMedian = state.Typeface.lineMedian * state.Ratio;
        }

        public TexChar metadata;

        public float delimiterPadding, lineMedian;


        void PaddDelimiters(VerticalBox box)
        {
            var delimits = StrutBox.Get(0, -delimiterPadding, 0);
            for (int i = 1; i < box.children.Count; i += 2)
                box.Add(i, delimits);
        }


        void PaddDelimiters(HorizontalBox box)
        {
            var delimits = StrutBox.Get(-delimiterPadding, 0, 0);
            for (int i = 1; i < box.children.Count; i += 2)
                box.Add(i, delimits);
        }

        public bool IsAlreadyHorizontal()
        {
            var metadata = this.metadata;
            bool isAlreadyHorizontal = true;
            while (metadata != null)
            {
                if (metadata.extension.enabled && !metadata.extension.horizontal)
                {
                    isAlreadyHorizontal = false;
                    break;
                }
                metadata = metadata.larger.Get;
            }
            return isAlreadyHorizontal;
        }
        public bool IsAlreadyVertical()
        {
            var metadata = this.metadata;
            bool isAlreadyVertical = true;
            while (metadata != null)
            {
                if (metadata.extension.enabled && metadata.extension.horizontal)
                {
                    isAlreadyVertical = false;
                    break;
                }
                metadata = metadata.larger.Get;
            }
            return isAlreadyVertical;
        }

        public Box CreateBoxMinWidth(float minimum)
        {
            var metadata = this.metadata;
            var isAlreadyHorizontal = this.IsAlreadyHorizontal();
            Func<TexChar, Box> create = (TexChar m) => isAlreadyHorizontal ? (Box)CreateBoxSubtituted(m, true) : CreateBoxRotated(m);
            Box box = create(metadata);
            while (box.width < minimum && metadata.larger.Has)
            {
                box.Flush();
                metadata = metadata.larger.Get;
                box = create(metadata);
            }
            if (box.width < minimum && metadata.extension.enabled)
            {
                HorizontalBox container = HorizontalBox.Get();
                TexCharExtension ext = metadata.extension;
                if (ext.horizontal)
                {
                    if (ext.top.Has) container.Add(CreateBoxSubtituted(ext.top.Get));
                    if (ext.bottom.Has) container.Add(CreateBoxSubtituted(ext.bottom.Get));
                    if (ext.mid.Has) container.Add(CreateBoxSubtituted(ext.mid.Get));
                }
                else
                {
                    if (ext.bottom.Has) container.Add(CreateBoxRotated(ext.bottom.Get));
                    if (ext.mid.Has) container.Add(CreateBoxRotated(ext.mid.Get));
                    if (ext.top.Has) container.Add(CreateBoxRotated(ext.top.Get));
                }
                minimum += delimiterPadding * (container.children.Count - 1);
                if (container.width < minimum && ext.repeat.Has)
                {
                    Box repeatBox = create(ext.repeat.Get);
                    do
                    {
                        if (ext.top.Has && ext.bottom.Has)
                        {
                            container.Add(1, repeatBox);
                            if (ext.mid.Has)
                            {
                                container.Add(container.children.Count - 1, repeatBox);
                                minimum += delimiterPadding;
                            }
                        }
                        else if (ext.bottom.Has)
                            container.Add(0, repeatBox);
                        else
                            container.Add(repeatBox);
                        minimum += delimiterPadding;
                    }
                    while (container.width < minimum && container.children.Count < 255);
                }
                box.Flush();
                PaddDelimiters(container);
                box = container;
            }
            return box;
        }

        public Box CreateBoxMinHeight(float minimum)
        {
            var metadata = this.metadata;
            var isAlreadyVertical = this.IsAlreadyVertical();
            Func<TexChar, Box> create = (TexChar m) => isAlreadyVertical ? (Box)CreateBoxSubtituted(m) : CreateBoxRotated(m);
            Box box = create(metadata);
            while (box.TotalHeight < minimum && metadata.larger.Has)
            {
                box.Flush();
                metadata = metadata.larger.Get;
                box = create(metadata);
            }
            if (box.TotalHeight < minimum && metadata.extension.enabled)
            {
                VerticalBox container = VerticalBox.Get();
                TexCharExtension ext = metadata.extension;
                if (ext.horizontal)
                {
                    if (ext.top.Has) container.Add(CreateBoxRotated(ext.top.Get));
                    if (ext.mid.Has) container.Add(CreateBoxRotated(ext.mid.Get));
                    if (ext.bottom.Has) container.Add(CreateBoxRotated(ext.bottom.Get));
                }
                else
                {
                    if (ext.top.Has) container.Add(CreateBoxSubtituted(ext.top.Get));
                    if (ext.mid.Has) container.Add(CreateBoxSubtituted(ext.mid.Get));
                    if (ext.bottom.Has) container.Add(CreateBoxSubtituted(ext.bottom.Get));
                }
                minimum += delimiterPadding * (container.children.Count - 1);
                if (container.TotalHeight < minimum && ext.repeat.Has)
                {
                    Box repeatBox = create(ext.repeat.Get);
                    do
                    {
                        if (ext.top.Has && ext.bottom.Has)
                        {
                            container.Add(1, repeatBox);
                            if (ext.mid.Has)
                            {
                                container.Add(container.children.Count - 1, repeatBox);
                                minimum += delimiterPadding;
                            }
                        }
                        else if (ext.bottom.Has)
                            container.Add(0, repeatBox);
                        else
                            container.Add(repeatBox);
                        minimum += delimiterPadding;
                    }
                    while (container.TotalHeight < minimum && container.children.Count < 255);
                }
                box.Flush();
                PaddDelimiters(container);
                box = container;
            }
            return box;
        }

        public Box CreateBoxStepped(int step)
        {
            var metadata = this.metadata;
            Box box = CreateBoxSubtituted(metadata);
            while (metadata.larger.Has && step-- > 0)
            {
                box.Flush();
                box = CreateBoxSubtituted(metadata = metadata.larger.Get);
            }
            if (metadata.extension.enabled && step >= 0)
            {
                VerticalBox container = VerticalBox.Get();
                TexCharExtension ext = metadata.extension;

                if (ext.top.Has) container.Add(CreateBoxSubtituted(ext.top.Get));
                if (ext.mid.Has) container.Add(CreateBoxSubtituted(ext.mid.Get));
                if (ext.bottom.Has) container.Add(CreateBoxSubtituted(ext.bottom.Get));

                if (!(ext.top.Has && ext.bottom.Has))
                    step++;

                if (ext.repeat.Has && step-- > 0)
                {
                    Box repeatBox = CreateBoxSubtituted(ext.repeat.Get);
                    var dbl = ext.repeat.Get.symbol == "braceex";
                    do
                    {
                        if (ext.top.Has && ext.bottom.Has)
                        {
                            container.Add(1, repeatBox);
                            if (ext.mid.Has || dbl)
                                container.Add(container.children.Count - 1, repeatBox);
                        }
                        else if (ext.bottom.Has)
                            container.Add(0, repeatBox);
                        else
                            container.Add(repeatBox);
                    }
                    while (step-- > 0);
                }
                box.Flush();
                PaddDelimiters(container);
                box = container;
            }
            TexUtility.CentreBox(box, lineMedian);
            return box;
        }

        public CharBox CreateBoxSubtituted(TexChar ch, bool trimming = false)
        {
            var box = CharBox.Get(ch.fontIndex, ch.characterIndex, size, resolution, color);
            if (trimming)
                box.width = box.italic;
            return box;
        }

        public RotatedCharBox CreateBoxRotated(TexChar ch)
        {
            return RotatedCharBox.Get(ch, size, resolution, color);
        }

        public override CharType Type => metadata.type;

        public override void Flush()
        {
            ObjPool<SymbolAtom>.Release(this);
            metadata = null;
            base.Flush();
        }


        public override string ToString()
        {
            return base.ToString() + " " + metadata.symbol + " " + metadata.type;
        }
    }

    // exclusive for minus type override
    public class OverridedTypeAtom : InlineAtom
    {
        public override Box CreateBox(TexBoxingState state)
        {
            return atom.CreateBox(state);
        }

        public CharType overridingType;
        public static OverridedTypeAtom Get()
        {
            return ObjPool<OverridedTypeAtom>.Get();
        }
        public static OverridedTypeAtom Get(Atom atom, CharType type)
        {
            var t = ObjPool<OverridedTypeAtom>.Get();
            t.atom = atom;
            t.overridingType = type;
            return t;
        }
        public override void Flush()
        {
            ObjPool<OverridedTypeAtom>.Release(this);
            base.Flush();
        }
        public override CharType Type => overridingType;
        public override CharType LeftType => overridingType;
        public override CharType RightType => overridingType;
    }
}

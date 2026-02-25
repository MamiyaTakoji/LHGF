using System;

namespace TexDrawLib
{
    public enum CharType
    {
        Ordinary = 0,
        Geometry = 1,
        Operator = 2,
        Relation = 3,
        Punctuation = 4,
        OpenDelimiter = 5,
        CloseDelimiter = 6,
        BigOperator = 7,
        Accent = 9,
    }

    public static class CharTypeInternal
    {
        public const CharType Invalid = (CharType)(-1);
        public const CharType Inner = (CharType)8;
    }

    public enum ExtensionType
    {
        Repeat = 0,
        Top = 1,
        Bottom = 2,
        Mid = 3
    }

    public enum TexAlignment
    {
        Center = 0,
        Left = 1,
        Right = 2,
        Top = 3,
        Bottom = 4
    }

    public enum TexModes
    {
        /// <summary>
        /// Flexible Vertical mode (main pages)
        /// </summary>
        Vertical = 0,
        /// <summary>
        /// Inlined vertical box (like in \vbox)
        /// </summary>
        InternalVertical = 1,
        /// <summary>
        /// Flexible Horizontal mode (in paragraph or math block)
        /// </summary>
        Horizontal = 2,  // Paragraph that is wrappable
        /// <summary>
        /// Restricted horizontal mode (like in \hbox)
        /// </summary>
        RestrictedHorizontal = 3,
    }

    public enum TexUnits
    {
        Pixel, Em, Point, Pica, Inch, BigPoint, Centimeter, Millimeter, DidotPoint, Cicero, ScaledPoint, 
        //LineWidth, BaselineSkip, ParSkip, 
    }

    [Flags]
    public enum TexEnvironment
    {
        /*  Bit code rules for TexEnvironment:
         *  First bit is: 0 = Vertical flow (document, vbox), 1 = Horizontal flow (paragraph, hbox)
         *  Second bit is: 0 = Block (line break allowed), 1 = Inline (line break dissalowed)
         *  Third bit (if block) is: 0 = A block outside paragraph flow, 1 = A block that interrupted paragraph (can't \par)
         *  Fourth bit is: 0 = Normal/Text mode, 1 = Math mode
         *  Fifth bit is: 0 = Condensed spaces, 1 = Verbatim spaces (single line as new line, etc.)
         *  Sixth bit is: 0 = Word-based wrapping, 1 = Letter wrapping
         *  Sixth bit is: 0 = Normal syntax, 1 = Raw syntax (turning off command processing, etc.)
         *  The rest bit is reserved or module-implementation defined
         *  (except if fourth bit is 1, then it's TexMathStyle << 4)
         */
        // HorizontalFlow = 1 << 0,
        Inline = 1 << 1,
        // Interrupt = 1 << 2,
        MathMode = 1 << 3,
        // RawSyntax = 1 << 4,
        // VerbatimSpace = 1 << 5,
        // LetterWrapping = 1 << 6,
        MainDocument = 0,
    }

    public enum TexMathStyle
    {
        Display = 0,
        DisplayCramped = 1,
        Text = 2,
        TextCramped = 3,
        Script = 4,
        ScriptCramped = 5,
        ScriptScript = 6,
        ScriptScriptCramped = 7,
    }

    public enum TexCharKind
    {
        None = -1,
        Numbers = 0,
        Capitals = 1,
        Small = 2,
        Commands = 3,
        Text = 4,
        Unicode = 5
    }


    public enum TexAssetType
    {
        Font = 0,
        Sprite = 1,
        FontSigned = 2
    }

    public enum TexAssetCategory
    {
        Unspecified = -1,
        Serif, SansSerif, Fixed, Cursive,
        MathPackage, SpritePackage
    }

    [Flags]
    public enum TexAssetStyle
    {
        Normal = 0,
        Boldface = 1,
        Slanted = 2,
        Italic = 4,
        Typewriter = 8,
        SansSerif = 16,
        SmallCase = 32,
    }

    public enum TexAssetStyleVariant
    {
        Family = 0,
        Series = 1,
        Shape = 2,
    }

    public enum Wrapping
    {
        NoWrap = 0,
        LetterWrap = 1,
        WordWrap = 2,
        WordWrapJustified = 3,
#if TEXDRAW_REVERSEDWRAPPING
        LetterWrapReversed = 4,
        WordWrapReversed = 5,
        WordWrapReversedJustified = 6,
#endif
    }

    public enum Overflow
    {
        Hidden = 0,
        Visible = 1,
        DownScale = 2,
        // AutoSize = 3,
        // AutoHeight = 4
        BestFit = 5
    }

    public enum DirtyLevel
    {
        None = 0,
        Render = 1,
        Box = 2,
        Parse = 3,
    }

    public enum Filling
    {
        None = 0,
        Rectangle = 1,
        WholeText = 2,
        WholeTextSquared = 3,
        PerLine = 4,

        //PerWord = 5, //Not yet ready
        PerCharacter = 6,

        PerCharacterSquared = 7,
        LocalContinous = 8,
        WorldContinous = 9
    }

    public enum Effects
    {
        Shadow = 0,
        Outline = 1
    }
}

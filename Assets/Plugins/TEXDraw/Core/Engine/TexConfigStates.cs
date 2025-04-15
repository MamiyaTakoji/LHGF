using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace TexDrawLib
{

    [Serializable]
    public struct DocumentState
    {
        //public float stopAtOverflow;
        //public float verticalAlignment;
        public float retinaRatio; // Dynamic Font Specific
        public float signedCofficient; // Static SDF Specific
        public bool debug;

        [HideInInspector]
        // Always overriden by TEXDraw Components
        public float initialSize;

        public float pixelsPerInch;

        public float nativeSize;
    }

    [Serializable]
    public struct ParagraphState
    {
        public float alignment;
        public bool justify;
        public bool indent;
        public bool rightToLeft;
        public bool centeredMathBlock;

        public float lineSpacing;
        public float paragraphSpacing;
        public float indentSpacing;
        public float bulletSpacing;
        public float leftPadding;
        public float rightPadding;
    }

    [Serializable]
    public struct TypefaceState
    {
        public string defaultTypeface;
        public string typewriterTypeface;
        public string mathTypeface;
        public string functionTypeface;

        public float blankSpaceWidth;
        public float lineAscent;
        public float lineMedian;
        public float lineDescent;

        public float underlineLevel;
        public float overlineLevel;
        public float midlineLevel;

        public int defaultIndex => TEXPreference.main.fontnames[defaultTypeface]?.assetIndex ?? 0;
        public int mathIndex => TEXPreference.main.fontnames[mathTypeface]?.assetIndex ?? 0;
        public int typewriterIndex => TEXPreference.main.fontnames[typewriterTypeface]?.assetIndex ?? 0;
        public int functionIndex => TEXPreference.main.fontnames[functionTypeface]?.assetIndex ?? 0;
    }

    [Serializable]
    public struct MathModeState
    {
        public float thinSpace;
        public float mediumSpace;
        public float thickSpace;
        public float scriptRatio;
        public float scriptScriptRatio;
        [FormerlySerializedAs("lineThinkness")]
        public float lineThickness;
        public float radicalLineThickness;
        public float delimiterPadding;
        public float framePadding;
        public float matricePadding;
        [FormerlySerializedAs("fractionClearanceMargin")]
        public float fractionPadding;

        public float upperBaselineDistance;
        public float upperMinimumDistance;
        public float lowerBaselineDistance;
        public float lowerMinimumDistance;
        public float scriptHorizontalMargin;
        [FormerlySerializedAs("scriptVerticalMargin")]
        public float scriptHeightMargin;
        public float scriptDepthMargin;
        public float scriptAscentOffset;
        public float scriptAscentCrampedOffset;
        [FormerlySerializedAs("scriptBaselineOffset")]
        public float scriptDescentOffset;
    }

    public readonly struct AllStates
    {
        public readonly DocumentState Document;
        public readonly ParagraphState Paragraph;
        public readonly TypefaceState Typeface;
        public readonly MathModeState Math;
        public readonly int MetadataStateCount;

        public AllStates(DocumentState document, ParagraphState paragraph, TypefaceState typeface, MathModeState math, int metadataStateCount)
        {
            Document = document;
            Paragraph = paragraph;
            Typeface = typeface;
            Math = math;
            MetadataStateCount = metadataStateCount;
        }
    }
}

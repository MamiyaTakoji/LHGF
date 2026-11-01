using System;
using System.Collections.Generic;
using UnityEngine;

namespace TexDrawLib
{
    public class TexParserState
    {
        public class Param<T>
        {
            public readonly T initial;

            public T current;

            private readonly Stack<T> stack = new Stack<T>();

            public void Push() { stack.Push(current); }

            public void Push(T val) { stack.Push(current); current = val; }

            public void Pop() { current = stack.Pop(); }

            public Param(T def) { this.initial = current = def; }

            public void Set(T val) { current = val; }

            public void Set(Func<T, T> func) { current = func(current); }

            public void Reset() { stack.Clear(); current = initial; }

            public void Reset(T val) { stack.Clear(); current = val; }

            // performance jeez
            // public implicit operator T(Param<T> t) { return t.value; }

            public void Do(T val, Action del)
            {
                Push(val); del(); Pop();
            }
        }

        // Inline configurations, in pixel
        readonly public Param<int> Font = new Param<int>(-1);
        readonly public Param<float> Size = new Param<float>(16);
        readonly public Param<float> Kern = new Param<float>(0);
        readonly public Param<Color32> Color = new Param<Color32>(new Color32(255, 255, 255, 255));
        readonly public Param<TexEnvironment> Environment = new Param<TexEnvironment>(TexEnvironment.MainDocument);
        readonly public Param<LayoutAtom> LayoutContainer = new Param<LayoutAtom>(null);

        public DocumentState Document = new DocumentState();

        public ParagraphState Paragraph = new ParagraphState();

        public TypefaceState Typeface = new TypefaceState();

        public MathModeState Math = new MathModeState();

        public Dictionary<string, Atom> Metadata = new Dictionary<string, Atom>();

        readonly private Stack<(string item, Atom value)> metadataStates = new Stack<(string, Atom)>();

        readonly private Stack<AllStates> allStates = new Stack<AllStates>();

        public void PushMathStyle(Func<TexMathStyle, TexMathStyle> func)
        {
            var before = Environment.current.GetMathStyle();
            var after = func(before);

            {
                var beforeS = MathStyleRatio(before);
                var afterS = MathStyleRatio(after);
                Size.Push(Size.current * afterS / beforeS);
                Environment.Push(Environment.current.SetMathStyle(after));
            }
        }

        public void PopMathStyle()
        {
            Environment.Pop();
            Size.Pop();
        }

        public float MathStyleRatio(TexMathStyle style)
        {
            if (style < TexMathStyle.Script)
                return 1;
            else if (style < TexMathStyle.ScriptScript)
                return Math.scriptRatio;
            else
                return Math.scriptScriptRatio;
        }

        public void PushStates()
        {
            allStates.Push(new AllStates(Document, Paragraph, Typeface, Math, metadataStates.Count));
            Font.Push();
            Size.Push();
            Kern.Push();
            Color.Push();
            Environment.Push();
        }

        public void PopStates()
        {
            var states = allStates.Pop();
            Document = states.Document;
            Paragraph = states.Paragraph;
            Typeface = states.Typeface;
            Math = states.Math;

            while (metadataStates.Count > states.MetadataStateCount)
                PopMetadata();
            Font.Pop();
            Size.Pop();
            Kern.Pop();
            Color.Pop();
            Environment.Pop();
        }

        public Atom GetMetadata(string item) => Metadata.TryGetValue(item, out Atom v) ? v : null;
        public bool GetMetadata(string item, out Atom v) => Metadata.TryGetValue(item, out v);

        public void SetMetadata(string item, Atom value)
        {
            if (Metadata.TryGetValue(item, out Atom v))
            {
                metadataStates.Push((item, v));
            } else
            {
                metadataStates.Push((item, null));
            }
            Metadata[item] = value;
        }

        private void PopMetadata()
        {
            var (item, value) = metadataStates.Pop();
            Metadata[item].Flush();
            if (value != null)
                Metadata[item] = value;
            else
                Metadata.Remove(item);
        }

        // all config values in points, so pixels = (value in pt * (pt to inch)) * (inch to pixels)
        public float Ratio => 1 / 72.27f * Document.pixelsPerInch * (Size.current / Document.nativeSize);
        
        public TexModuleParser parser;

        public void Reset()
        {
            Document = TEXConfiguration.main.Document;
            Paragraph = TEXConfiguration.main.Paragraph;
            Typeface = TEXConfiguration.main.Typeface;
            Math = TEXConfiguration.main.Math;

            Size.Reset();
            Kern.Reset();
            Font.Reset(Typeface.defaultIndex);
            Color.Reset();
            Environment.Reset();
            LayoutContainer.Reset();
            foreach (var item in Metadata)
            {
                item.Value.Flush();
            }
            Metadata.Clear();
            metadataStates.Clear();
            allStates.Clear();
        }
    }
}

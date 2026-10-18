// Atom (smallest unit) of TexFormula.
namespace TexDrawLib
{
    public abstract class Atom : IFlushable
    {
        protected Atom() { }

        public abstract CharType Type { get; }

        public abstract CharType LeftType { get; }

        public abstract CharType RightType { get; }

        public void ProcessParameters(string command, TexParserState state)
        {
            var p = 0;
            ProcessParameters(command, state, null, ref p);
        }

        public virtual void ProcessParameters(string command, TexParserState state, string value, ref int position)
        {
            // if (position < value.Length && (value[position] == ' '))
            //     position++;
        }

        public abstract Box CreateBox(TexBoxingState state);

        public abstract void Flush();

        public bool IsFlushed { get; set; }


        public override string ToString()
        {
            return base.ToString().Replace("TexDrawLib.", string.Empty);
        }
    }
}

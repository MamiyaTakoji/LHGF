
namespace TexDrawLib
{
    public abstract class BlockAtom : Atom
    {
        public Atom atom;

        public override CharType Type => atom.Type;
        public override CharType LeftType => atom?.LeftType ?? CharTypeInternal.Invalid;
        public override CharType RightType => atom?.RightType ?? CharTypeInternal.Invalid;

        public virtual Atom Unpack() => null;

        public bool IsUnpackable => Unpack() != null;

        public override void Flush()
        {
            atom?.Flush();
            atom = null;
        }

        public override Box CreateBox(TexBoxingState state)
        {
            return atom?.CreateBox(state) ?? StrutBox.Get(0, 0, 0);
        }
    }
}

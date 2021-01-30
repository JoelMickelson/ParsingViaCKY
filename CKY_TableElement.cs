namespace ParsingViaCKY
{
    public class CKY_TableElement
    {
        //        public List<int> Symbol = new List<int>();

        //   public int Symbol;

        public CFG_Rule Rule;

        public CKY_TableElement B_Connection = null;
        public CKY_TableElement C_Connection = null;

        public bool ConnectsToTerminal = false;
    }
}
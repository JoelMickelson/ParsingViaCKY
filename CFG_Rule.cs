using System.Collections.Generic;

namespace ParsingViaCKY
{
    public class CFG_Rule
    {


        // symbols are determined during runtime, so they cannot be enumerated
        // even so, it is more efficient to refer to them using ints than strings
        // 

        // however, symbols can also be either terminal or non-terminal
        // the best bet is to let C# handle reference features, and make it an object

        // alternatively, let the grammar class handle how they are referred to, and the rule itself just gets some ints

        //     public CFG_Symbol LHS;
        //     public List<CFG_Symbol> RHS = new List<CFG_Symbol>();


        public int LHS;

        public List<int> RHS;

        public CFG_Rule Via = null;


        public CFG_Rule(CFG_Rule source)
        {
            LHS = source.LHS;
            RHS = new List<int>(source.RHS);
        }

        public CFG_Rule(int lhsRef, List<int> rhsRef, CFG_Rule via = null)
        {
            LHS = lhsRef;
            RHS = rhsRef;

            Via = via;
        }




        public bool IsXRule = false;
        public bool IsDummy = false;

        //        public string LHS;
        //      public List<string> RHS = new List<string>();

        //public override string ToString()
        //{
        //    string s = 
        //}

    }
}
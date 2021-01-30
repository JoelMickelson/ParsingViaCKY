using System;
using System.Collections.Generic;

namespace ParsingViaCKY
{
    public class CKY_TableCell
    {
        public List<CKY_TableElement> Elements = new List<CKY_TableElement>();

        public void Print(ContextFreeGrammar grammar, int i, int j)
        {
            // Console.WriteLine("Table Cell ({0},{1}) contains {2} elements", i, j, Elements.Count);


            string s = $"Table Cell ({i},{j}) contains {Elements.Count} elements";


            //if (Elements.Count > 0)
            //{
            //    s += "  (";

            //    for (int e = 0; e < Elements.Count; e++)
            //    {

            //        //Console.WriteLine(Elements[e].)
            //        s += Elements[e].ToString() + ' ';
            //    }

            //    s += ")";
            //}
            Console.WriteLine(s);

        }
    }
}
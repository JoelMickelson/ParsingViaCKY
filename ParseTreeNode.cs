using System;
using System.Collections.Generic;


namespace ParsingViaCKY
{
    public class ParseTreeNode
    {
        public ParseTreeNode Parent;
        public List<ParseTreeNode> Children = new List<ParseTreeNode>();

        public string Text = "?";
        public string TerminalSymbol = "";


        private bool isXRule = false;
        private CFG_Rule originatingRule = null;
        private bool isDummy = false;

        //public bool IsRoot { get return Parent == null; }


        /// <summary>
        /// recursively generates a parse tree from a corner CKY table element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="grammar"></param>
        /// <param name="parent"></param>
        internal void ExpandNode(CKY_TableElement element, ContextFreeGrammar grammar, ParseTreeNode parent = null)
        {
            if (element == null)
                return;



            string text = grammar.GetSymbolLabelFromCode(element.Rule.LHS);

            this.Text = text;

            this.Parent = parent;

            isXRule = element.Rule.IsXRule;

            originatingRule = element.Rule;

            isDummy = element.Rule.IsDummy;


            // terminal symbols are just "Verb go"
            if (element.ConnectsToTerminal)
            {

                this.TerminalSymbol = grammar.GetSymbolLabelFromCode(element.Rule.RHS[0]);
            }
            else
            {
                // non-terminals are done by element rather than just the rule itself, and must be recursive


                // wouldn't it be nice if there was a list of child connections?

                List<CKY_TableElement> CKY_TableElements = new List<CKY_TableElement>() { element.B_Connection, element.C_Connection };
                foreach (CKY_TableElement e in CKY_TableElements)
                {
                    // create the child node
                    // expand it
                    // set it as a child
                    ParseTreeNode c = new ParseTreeNode();
                    c.ExpandNode(e, grammar, this);
                    this.Children.Add(c);


                }

            }

        }


        /// <summary>
        /// migrates away 'long' rules - the X1, X2 rules that are used in CNF to make everything into an A -> B C form
        /// this method is NOT guaranteed to get them all on the first try
        /// so, it returns an integer that is >0 if there were any long rules found
        /// the caller should keep calling it until it returns 0
        /// </summary>
        /// <returns></returns>
        internal int IntegrateLongRules()
        {
            // gets rid of X-type rules caused by conversion to CNF for RHS length > 2

            // if this node is an X rule, delete it and attach my parent to my children

            // then do this method recursively for each child



            int longRules = 0;

            List<ParseTreeNode> children = new List<ParseTreeNode>(this.Children);


            if (this.isXRule == true)
            {
                Parent.Children.InsertRange(0, this.Children);

                foreach (ParseTreeNode c in this.Children)
                {
                    c.Parent = Parent;
                }

                Parent.Children.Remove(this);


                longRules = 1;
            }


            foreach (ParseTreeNode c in children)
                longRules += c.IntegrateLongRules();

            Console.WriteLine($"integrating.... ({longRules})");

            return longRules;
        }

        /// <summary>
        /// returns the terminal symbols that make up a node's substring
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            string s = "";

            //s += this.Text;
            s += this.TerminalSymbol;

            int size = this.Children.Count;

            for (int i = 0; i < size; i++)
            {
                s += " " + this.Children[i].GetString();
            }

            s = s.TrimStart(' ');
            return s;
        }

        public ParseTreeNode GetConstituentTree(string label)
        {
            if (this.Text == label)
                return this;

            foreach (ParseTreeNode p in this.Children)
            {
                if (p.GetConstituentTree(label) != null)
                    return p;
            }

            return null;
        }

        /// <summary>
        /// restores unit productions to their original state
        /// 
        /// returns the number of unit productions found
        /// </summary>
        /// <param name="cNFGrammar"></param>
        /// <returns></returns>
        internal int ExtendUnitProductions(ContextFreeGrammar grammar)
        {
            // when a grammar is converted to CNF, any rule that is a unit production has a 'via' attribute showing which constituent is being replaced
            // (or better yet, show which grammar rule is being replaced)

            // here's a common situation in a text adventure:
            // DO -> NP -> Noun -> armor
            // the grammar contains rules for DO->armor, NP->armor, and Noun->armor
            // the first two are artificially created for CNF
            // DO->armor is marked as via NP->armor, and NP->armor is marked as via Noun->armor
            // ExtendUnitProductions recognizes if a rule is a unit production, and inserts an intermediate rule
            // in this case, DO->armor is replaced with DO->NP->armor
            // DO->NP is a legitimate rule, and NP->armor will be checked on the next recursion




            // !!!!!! the description of this code is wrong
            // DO -> armor gets changed to DO->Noun->armor, via the DO->Noun relationship
            // The Noun->armor rule is simply found using brute force, and assumed to exist





            int count = 0;


            //if (this.Children.Count == 0)
            //{
            //    return 0;
            //}


            // rather, a new NP->armor is generated, and this DO-> is altered

            //  if (this.TerminalSymbol == "Houston")
            //    ;


            // this screws up the recursion
            //            if (this.originatingRule == null)
            //              return 0;


            // find the grammar rule for 




            if (this.originatingRule != null)
            {
                CFG_Rule via = this.originatingRule.Via;





                if (via != null)
                {

                    count++;


                    Console.WriteLine($"{grammar.GetRuleString(this.originatingRule)} occurs via {grammar.GetRuleString(via)}");

                    ParseTreeNode intermediateNode = new ParseTreeNode();


                    intermediateNode.TerminalSymbol = this.TerminalSymbol;

                    intermediateNode.Text = grammar.GetSymbolLabelFromCode(via.RHS[0]);


                    // the new node inherits this node's children
                    intermediateNode.Children = new List<ParseTreeNode>(this.Children);




                    foreach (ParseTreeNode p in intermediateNode.Children)
                        p.Parent = intermediateNode;

                    // now this node's children becomes just the intermediate node
                    this.Children.Clear();
                    this.Children.Add(intermediateNode);
                    this.TerminalSymbol = "";
                    intermediateNode.Parent = this;

                    //intermediateNode.originatingRule = via;
                    //   intermediateNode.Text = grammar.GetSymbolLabelFromCode(intermediateNode.originatingRule.RHS[0]);


                    // intermediateNode.originatingRule = grammar.FindRuleByText(intermediateNode.Text)

                    // getting the originating rule for the intermediate node is expensive and probably not necessary



                    if (intermediateNode.Children.Count == 1) // it's a unit production too, so it needs to know its originating rule for the via info
                    {
                        intermediateNode.originatingRule = grammar.FindRuleByText(intermediateNode.Text, intermediateNode.Children[0].Text);

                        // it can also point to a terminal symbol
                        if (intermediateNode.originatingRule == null)
                            intermediateNode.originatingRule = grammar.FindRuleByText(intermediateNode.Text, intermediateNode.Children[0].TerminalSymbol);


                    }


                    this.originatingRule = via;




                    //     this.originatingRule = grammar.FindRuleByText(this.Text, intermediateNode.Text);

                    //Console.WriteLine($"Adding {grammar.GetRuleString(intermediateNode.originatingRule)} as an intermediate rule.");

                }

            }

            //if (this.IsUnitProduction())
            //{

            //}


            // now recursively do each child so that the entire parse tree gets done
            foreach (ParseTreeNode child in Children)
            {
                count += child.ExtendUnitProductions(grammar);
            }






            return count;



        }

        internal int RemoveDummyRules(ContextFreeGrammar grammar)
        {

            // starting with:
            // [S [VP [D_GO go][D_TO to][Det the][N store]]]
            // we want:
            // [S [VP [go to [Det the] [N store]]]

            // D_ type rules must be marked as IsDummy
            // whenever the parse tree sees a dummy rule, it changes its parent so that it has that terminal as a child instead of the dummy rule


            int count = 0;


            if (this.isDummy)
            {
                Console.WriteLine($"Found dummy rule: {this.Text}");

                if (this.Parent == null) // it's the top, which would be a weird situation
                {
                    Console.WriteLine("Dummy rule unresolved");
                    return 0;
                }

                int i = Parent.Children.FindIndex(o => o == this);

                Console.WriteLine($"Dummy rule is at index {i} of parent.");


                // converting it to a terminal symbol is easy: just remove the non-terminal label
                this.Text = "?";

            }





            foreach (ParseTreeNode child in Children)
            {
                count += child.RemoveDummyRules(grammar);
            }
            return count;


        }




    }
}
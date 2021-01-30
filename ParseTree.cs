using System;
using System.Collections.Generic;


namespace ParsingViaCKY
{
    public class ParseTree
    {
        private ParseTreeNode treeHead;

        public ParseTree(ParseTreeNode treeHead)
        {
            this.treeHead = treeHead;
        }

        public static List<ParseTree> BuildParseTrees(ContextFreeGrammar originalGrammar, ContextFreeGrammar CNFGrammar, CKY_Table table)
        {
            List<ParseTreeNode> parseTreeNodes = new List<ParseTreeNode>();


            // how to construct a parse tree:
            // do the same thing as table.PrintAllParses, except construct a tree
            // then fix it to de-CNF




            // apparently we're constructing multiple trees, and the table can already have them!

            List<ParseTree> parseTrees = new List<ParseTree>();


            // this is the cell that contains the heads of parses
            // it can also contain other symbols, so watch out
            CKY_TableCell headCell = table.TableCells[0, table.NumberOfWords - 1];





            foreach (CKY_TableElement element in headCell.Elements)
            {
                // ignore non-S pseudo-parses
                if (element.Rule.LHS != CNFGrammar.StartSymbol)
                    continue;

                // how to print in bracked notation:
                // non-terminal symbols are printed as [S, then the constituents, then ]
                // terminal symbols are just printed

                // since the terminal symbols aren't actually in the table, I'll handle them manually

                // the easiest way to do this is recursively

                //s += getStringForElement(element, true);
                //                Console.WriteLine(s);



                ParseTreeNode treeHead = new ParseTreeNode();
                ParseTree tree = new ParseTree(treeHead);
                parseTrees.Add(tree);



                treeHead.ExpandNode(element, CNFGrammar);


                //                ParseTree tree = new ParseTree();
                //              tree.AddNode(element, CNFGrammar);


                tree.DeCNF(originalGrammar, CNFGrammar);


            }

            return parseTrees;

        }


        private void DeCNF(ContextFreeGrammar originalGrammar, ContextFreeGrammar CNFGrammar)
        {
            // let's remove all X-type rules and re-establish >2 long rules

            // have the heads do it recursively
            // but what they're doing: if I'm an X, then get rid of me and attack my children to my parent
            // note that all X rules are B rules, so they go in front

            // also we have to do this more than once, since X rules can and often do connect to each other
            // so just navigate through the entire tree multiple times


            //bool haveMore = false;




















            int l = 0;
            do
            {
                l = this.treeHead.IntegrateLongRules();
                Console.WriteLine($"{l} long rules found.");

            } while (l > 0);


            // to expand unit productions, we reconcile the tree with the original grammar
            // whenever a node->child relationship is not in the grammar, 
            // NO

            // here's how we're doing it
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



            int u = 0;
            do
            {
                u = this.treeHead.ExtendUnitProductions(CNFGrammar);
                Console.WriteLine($"{u} unit productions found.");
            } while (u > 0);



            // now let's convert dummy rules for mixed terminals/non-terminals

            // starting with:
            // [S [VP [D_GO go][D_TO to][Det the][N store]]]
            // we want:
            // [S [VP [go to [Det the] [N store]]]

            // D_ type rules must be marked as IsDummy
            // whenever the parse tree sees a dummy rule, it changes its parent so that it has that terminal as a child instead of the dummy rule

            int d = 0;
            do
            {
                d = this.treeHead.RemoveDummyRules(CNFGrammar);
                Console.WriteLine($"{d} dummy rules found.");
            } while (d > 0);


        }

        internal ParseTreeNode GetRoot()
        {
            return this.treeHead;
        }


        /// <summary>
        /// Shows a conventional parse rendering that resembles "[S [V wear][DO [NP [Det the][Adj red][Adj silk][Noun robe]]]]".
        /// 
        /// Parses come from a CKY_Table. To make these parses useful, we convert them into a ParseTree.
        /// They are modified in the process (to de-CNF them).
        /// </summary>
        internal void Print()
        {
            //            throw new NotImplementedException();



            string s = "";

            ParseTreeNode head = this.treeHead;

            s += getStringForNode(head, true);


            Console.WriteLine(s);



        }

        /// <summary>
        /// recursively builds a conventional [S [V go] [N there]] rendering
        /// </summary>
        /// <param name="node"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        private string getStringForNode(ParseTreeNode node, bool start = false)
        {
            string s = "";

            // terminal symbols are just "Verb go"
            if (node.Children.Count == 0)
            {
                //          if (!start)
                //            s += ' ';


                if (node.Text == "?")
                    s += node.TerminalSymbol + ' ';
                else
                    s += '[' + node.Text + " " + node.TerminalSymbol + ']';
            }
            else
            {
                // non-terminal nodes have a [label {children}] form, generated recursively

                s += '[';
                s += node.Text;
                s += ' ';

                foreach (ParseTreeNode n in node.Children)
                {

                    s += getStringForNode(n);
                }

                s += ']';

            }
            return s;
        }
    }
}
using System;
using System.Collections.Generic;

namespace ParsingViaCKY
{
    public class CKY_Table
    {
        private string sentence;

        public int NumberOfWords;

        public ContextFreeGrammar grammar;

        public CKY_TableCell[,] TableCells;

        string[] words;

        public CKY_Table(string sentence, ContextFreeGrammar grammar)
        {
            this.sentence = sentence;
            this.grammar = grammar;

            words = sentence.Split(' ');
            NumberOfWords = words.Length;

            TableCells = new CKY_TableCell[NumberOfWords, NumberOfWords];
            for (int i = 0; i < NumberOfWords; i++)
            {
                for (int j = 0; j < NumberOfWords; j++)
                {
                    TableCells[i, j] = new CKY_TableCell();
                }
            }
        }

        public void Build()
        {
            int n = NumberOfWords;

            for (int j = 0; j < n; j++)  // iterating through columns
            {
                // the superdiagonal is a part of speech area, so we want to find rules in the form of A->w
                // add all A where A->word[j]
                //List<int> symbols = getTerminalRuleSymbols(words[j]);
                //TableElements[j, j].AddTerminalRules

                Console.WriteLine("\n\r({0}, {0})", j);

                AddTerminalRules(TableCells[j, j], words[j]);

                for (int i = j - 1; i >= 0; i--) // iterating through rows
                {
                    Console.WriteLine("\n\r({0}, {1})", i, j);


                    for (int k = i; k < j; k++)
                    {
                        // add all A->BC where B is in table[i, k] and C is in table[k+1, j]

                        Console.WriteLine("Now getting A->BC for table[{0},{1}] -> table[{2},{3}] table[{4},{5}]", i, j, i, k, k + 1, j);

                        AddNonTerminalRules(TableCells[i, j], TableCells[i, k], TableCells[k + 1, j]);

                    }



                }
            }
        }

        public void PrintAllParses()
        {
            // this is the cell that contains the heads of parses
            // it can also contain other symbols, so watch out
            CKY_TableCell headCell = this.TableCells[0, this.NumberOfWords - 1];

            foreach (CKY_TableElement element in headCell.Elements)
            {
                // ignore non-S pseudo-parses
                if (element.Rule.LHS != grammar.StartSymbol)
                    continue;

                string s = "";

                // how to print in bracked notation:
                // non-terminal symbols are printed as [S, then the constituents, then ]
                // terminal symbols are just printed

                // since the terminal symbols aren't actually in the table, I'll handle them manually

                // the easiest way to do this is recursively
                s += getStringForElement(element, true);



                Console.WriteLine(s);


            }
        }

        // recursively gets a string that represents part or all of a parse tree, in bracketed notation
        private string getStringForElement(CKY_TableElement element, bool start = false)
        {
            string s = "";

            // terminal symbols are just "Verb go"
            if (element.ConnectsToTerminal)
            {
                if (!start)
                    s += ' ';
                s += '[' + grammar.GetSymbolLabelFromCode(element.Rule.LHS) + " " + grammar.GetSymbolLabelFromCode(element.Rule.RHS[0]) + ']';
            }
            else
            {
                // non-terminals are done by element rather than just the rule itself, and must be recursive
                if (!start)
                    s += ' ';
                s += '[';
                s += grammar.GetSymbolLabelFromCode(element.Rule.LHS);
                s += getStringForElement(element.B_Connection);
                s += getStringForElement(element.C_Connection);
                s += ']';
            }

            return s;

        }

        private void AddNonTerminalRules(CKY_TableCell A, CKY_TableCell B, CKY_TableCell C)
        {


            for (int b = 0; b < B.Elements.Count; b++)
            {
                for (int c = 0; c < C.Elements.Count; c++)
                {
                    List<CFG_Rule> rules = grammar.Rules.FindAll(o => o.RHS.Count == 2);

                    rules.RemoveAll(o => o.RHS[0] != B.Elements[b].Rule.LHS);
                    rules.RemoveAll(o => o.RHS[1] != C.Elements[c].Rule.LHS);

                    if (rules.Count > 0)
                    {
                        foreach (CFG_Rule r in rules)
                        {
                            CKY_TableElement entry = new CKY_TableElement();

                            entry.B_Connection = B.Elements[b];
                            entry.C_Connection = C.Elements[c];

                            entry.Rule = r;
                            entry.ConnectsToTerminal = false;

                            A.Elements.Add(entry);

                            Console.WriteLine("Found a match! {0}", grammar.GetRuleString(r));
                        }
                    }
                }
            }
        }

        private void AddTerminalRules(CKY_TableCell cell, string w)
        {
            int wordSymbol = grammar.FindSymbolCode(w);

            if (wordSymbol == -1) // doesn't exist in grammar
                return;


            List<int> symbols = new List<int>();

            List<CFG_Rule> rules = grammar.Rules.FindAll(o => o.RHS.Contains(wordSymbol));

            foreach (CFG_Rule r in rules)
            {
                CKY_TableElement entry = new CKY_TableElement();
                entry.Rule = r;
                entry.ConnectsToTerminal = true;

                cell.Elements.Add(entry);


                Console.WriteLine("Interpreting {0} ({1}) as a {2} ({3}).", w, wordSymbol, grammar.GetSymbolLabelFromCode(r.LHS), r.LHS);
            }

            return;
        }


        // find all A -> w, and return them as a list of symbols for A
        private List<int> getTerminalRuleSymbols(string word)
        {
            int wordSymbol = grammar.FindSymbolCode(word);

            if (wordSymbol == -1) // doesn't exist in grammar
                return new List<int>();


            List<int> symbols = new List<int>();

            List<CFG_Rule> rules = grammar.Rules.FindAll(o => o.RHS.Contains(wordSymbol));

            foreach (CFG_Rule r in rules)
            {
                symbols.Add(r.LHS);
            }

            return symbols;

        }
    } // end of class
}
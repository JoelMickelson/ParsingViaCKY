using System;
using System.Collections.Generic;

namespace ParsingViaCKY
{
    public class ContextFreeGrammar
    {

        public List<CFG_Rule> Rules = new List<CFG_Rule>();

        public List<Tuple<string, int>> SymbolLookupTable = new List<Tuple<string, int>>();


        public List<int> TerminalSymbols = new List<int>();
        public List<int> NonTerminalSymbols = new List<int>();


        // the start symbol is assumed to be the first symbol read in
        // so, the first rule in the grammar should be in the form of S -> something
        public int StartSymbol = 0;


        private int symbolGenerationNextCode = 0;



        // makes a CFG based on an existing source grammar
        public ContextFreeGrammar(ContextFreeGrammar source)
        {
            Rules.AddRange(source.Rules);
            SymbolLookupTable.AddRange(source.SymbolLookupTable);
            TerminalSymbols.AddRange(source.TerminalSymbols);
            NonTerminalSymbols.AddRange(source.NonTerminalSymbols);

            int maxSymbol = 0;
            foreach (Tuple<string, int> t in SymbolLookupTable)
            {
                if (t.Item2 > maxSymbol)
                    maxSymbol = t.Item2;
            }
            symbolGenerationNextCode = maxSymbol + 1;
        }

        public ContextFreeGrammar(string filename)
        {
            int counter = 0;
            string line;
            string[] relationshipMarker = new string[] { "->" };
            System.IO.StreamReader file;
            
            try
            {
                file = new System.IO.StreamReader(filename);
            }
            catch(System.IO.FileNotFoundException e)
            {
                Console.WriteLine("file not found");
                Console.ReadKey();
                
                return;
            }

            while ((line = file.ReadLine()) != null)
            {

                string[] ruleUnits = line.Split(relationshipMarker, StringSplitOptions.None);

                if (ruleUnits.Length != 2)
                    continue;


                string lhs = ruleUnits[0].Trim(' ');

                string[] possibles = ruleUnits[1].Split('|');
                foreach (string p in possibles)
                {
                    //rhs.Add(p.)
                    List<string> rhs = new List<string>();


                    addRule(lhs, p);
                    //string[] rhsSymbols = p.Split(' ');

                    //foreach (string s in rhsSymbols)
                    //{

                    //}

                }

                Console.WriteLine(line);
                counter++;
            }

            file.Close();
            Console.WriteLine("There were {0} lines.", counter);
            Console.WriteLine("");

            // now identify which symbols are terminal and which are non-terminal


            int maxSymbolID = symbolGenerationNextCode;

            for (int i = 0; i < maxSymbolID; i++)
            {
                // find rules for symbol
                // if there are no rules with this symbol on left side, it is terminal

                CFG_Rule rule = Rules.Find(o => o.LHS == i);

                if (rule == null)
                    TerminalSymbols.Add(i);
                else
                    NonTerminalSymbols.Add(i);
            }


        }



        public void PrintAllRules()
        {
            foreach (CFG_Rule rule in this.Rules)
            {
                string s = GetRuleString(rule);

                Console.WriteLine(s);
            }
        }

        public void PrintSymbols()
        {
            Console.WriteLine("");
            Console.WriteLine("Non-Terminal Symbols:");
            foreach (int i in this.NonTerminalSymbols)
                Console.WriteLine("" + GetSymbolLabelFromCode(i) + $"  ({i})");


            Console.WriteLine("");
            Console.WriteLine("Terminal Symbols:");
            foreach (int i in this.TerminalSymbols)
                Console.WriteLine("" + GetSymbolLabelFromCode(i) + $"  ({i})");

        }

        // symbols are ints, which is (possibly) efficient, but which makes it impossible to debug anything
        public string GetRuleString(CFG_Rule rule)
        {
            string lhs = GetSymbolLabelFromCode(rule.LHS);

            List<string> rhs = new List<string>();

            foreach (int i in rule.RHS)
            {
                rhs.Add(GetSymbolLabelFromCode(i));
            }

            string s = lhs + " -> ";
            int index = 0;

            foreach (string r in rhs)
            {
                s += r;


                //if (r != rhs.Last())
                if (index < rhs.Count)  // put a space after each one
                    s += ' ';

                index++;
            }

            if (rule.Via != null)
            {
                s += " via " + GetRuleString(rule.Via);
            }

            return s;
        }

        // gets the label without the risk of creating a new entry
        public string GetSymbolLabelFromCode(int code)
        {
            Tuple<string, int> lookup = SymbolLookupTable.Find(o => o.Item2 == code);

            if (lookup == null)
            {
                return "ERROR";
            }

            return lookup.Item1;
        }

        private void addRule(string lhs, string rhs)
        {
            // S; NP VP
            lhs = lhs.Trim(' ');
            rhs = rhs.Trim(' ');


            if (lhs.Length == 0)
            {
                return;
            }


            int lhsRef = getSymbolCode(lhs);

            List<int> rhsRef = new List<int>();

            string[] rhsElements = rhs.Split(' ');
            foreach (string s in rhsElements)
            {
                string s1 = s.Trim(' ');
                if (s1.Length == 0)
                {
                    continue;
                }

                rhsRef.Add(getSymbolCode(s1));


            }

            this.Rules.Add(new CFG_Rule(lhsRef, rhsRef));
        }

        /// <summary>
        /// returns the rule that would match a relationship between A->B
        /// this version is for unit productions
        /// </summary>
        /// <param name="leftLabel"></param>
        /// <param name="text2"></param>
        /// <returns></returns>
        internal CFG_Rule FindRuleByText(string leftLabel, string rightLabel)
        {
            int left = getSymbolCode(leftLabel);
            int right = getSymbolCode(rightLabel);

            return Rules.Find(o => o.LHS == left && o.RHS.Count == 1 && o.RHS[0] == right);
        }

        // returns the code for a symbol, given its label
        // creates a new code if one does not exist
        // DO NOT MAKE THIS PUBLIC
        private int getSymbolCode(string label)
        {
            Tuple<string, int> lookup = SymbolLookupTable.Find(o => o.Item1 == label);

            int code = 0;

            if (lookup == null) // entry does not exist yet, so let's create it
            {
                code = generateNewSymbolCode();
                SymbolLookupTable.Add(new Tuple<string, int>(label, code));

                Console.WriteLine("Using " + code + " for \"" + label + "\"");
            }
            else
            {
                code = lookup.Item2;
            }

            return code;
        }

        // return the code for a symbol, given its label
        // returns -1 if it wasn't there
        public int FindSymbolCode(string label)
        {
            Tuple<string, int> lookup = SymbolLookupTable.Find(o => o.Item1 == label);

            if (lookup == null)
                return -1;

            return lookup.Item2;

        }

        private int generateNewSymbolCode()
        {
            int ID = symbolGenerationNextCode;
            symbolGenerationNextCode++;

            return ID;
        }




        // convert a grammar to Chomsky Normal Form
        public void ConvertToCNF()
        {
            // the basic approach:
            // use a new ruleset that it built based on the old one
            // then, set up the symbol lists again from scratch

            List<CFG_Rule> adjustedRules = new List<CFG_Rule>();
            List<CFG_Rule> obsoleteRules = new List<CFG_Rule>();

            // generate dummy non-terminals for mixed rules
            // this can be done in a single iteration

            foreach (CFG_Rule rule in this.Rules)
            {
                bool hasTerminal = false;
                bool hasNonTerminal = false;
                foreach (int i in rule.RHS)
                {
                    if (NonTerminalSymbols.Exists(o => o == i))
                    {
                        hasNonTerminal = true;
                        continue;
                    }

                    if (TerminalSymbols.Exists(o => o == i))
                        hasTerminal = true;
                }

                if (hasTerminal && hasNonTerminal)
                {
                    ConvertTerminalsToDummyNonTerminals(rule, adjustedRules);
                    //  obsoleteRules.Add(rule);
                }
            }

            Rules.RemoveAll(o => obsoleteRules.Contains(o));
            Rules.AddRange(adjustedRules);


            obsoleteRules.Clear();






            // expand unit production rules
            List<CFG_Rule> productionRules = new List<CFG_Rule>();
            foreach (CFG_Rule rule in this.Rules)
            {
                // at this point we're just making a list of rules that need to be converted

                if (rule.RHS.Count == 1)   // A -> B
                {
                    if (NonTerminalSymbols.Contains(rule.RHS[0]))    //  "Verb -> go" is no problem, but a non-terminal isn't acceptable
                        productionRules.Add(rule);
                }
            }

            // now we actually do the conversions
            ConvertUnitProductions(productionRules, Rules);

            // this removal isn't working (maybe?) but it turns out to be convenient for de-CNF
            Rules.RemoveAll(o => productionRules.Contains(o));







            // split over-sized rules
            List<CFG_Rule> bigRules = new List<CFG_Rule>();
            foreach (CFG_Rule rule in this.Rules)
            {
                if (rule.RHS.Count > 2)
                    bigRules.Add(rule);
            }


            Rules.RemoveAll(o => bigRules.Contains(o));
            SplitOversizedRules(bigRules, Rules);




        }

        private void ConvertUnitProductions(List<CFG_Rule> productionRules, List<CFG_Rule> targetRules)
        {

            List<CFG_Rule> fixedRules = new List<CFG_Rule>();

            while (productionRules.Count > 0)
            {
                // S -> VP
                // VP -> Verb
                // VP -> Verb NP
                // VP -> Verb PP
                // Verb -> include
                // ------------
                // *S -> Verb
                // S -> Verb NP
                // S -> Verb PP
                // -------------
                // S -> include





                CFG_Rule currentRule = productionRules[0];
                productionRules.RemoveAt(0);

                Console.WriteLine($"Handling unit production {GetRuleString(currentRule)}");


                // on the first iteration this won't be true, but as we flatten the ruleset they are added to the productionRules for checking
                if (currentRule.RHS.Count > 1)  // it's 2 symbols on the RHS (removal of >2 should have already been done)
                {

                    fixedRules.Add(currentRule);
                    continue;

                }
                else if (TerminalSymbols.Contains(currentRule.RHS[0])) // terminal symbols are OK
                {
                    fixedRules.Add(currentRule);
                    continue;
                }


                int rhs = currentRule.RHS[0];

                // find all rules that rhs has as the lhs

                // so we might start with DO->NP, which is unacceptable under CNF
                // NP->Det N | NP-> N
                // we want DO -> each of these RHS
                // they are also marked as "via" that rule

                List<CFG_Rule> descendants = Rules.FindAll(o => o.LHS == rhs);

                foreach (CFG_Rule r in descendants)
                {

                    //   if (currentRule.LHS == 2 && r.RHS[0] == 11)
                    //   ;
                    //if (currentRule.LHS == 2 && RHS[0] == 11)


                    // add a new rule where the LHS is from the current rule, and the RHS is from the descendants

                    //                    CFG_Rule via = Rules.Find(o => o.LHS == currentRule.LHS && o.RHS.) 


                    CFG_Rule via = currentRule;



                    CFG_Rule newRule = new CFG_Rule(currentRule.LHS, new List<int>(r.RHS), via);

                    Console.WriteLine($"Adding rule {GetRuleString(newRule)}");

                    productionRules.Add(newRule);





                }



            }

            targetRules.AddRange(fixedRules);

        }

        int XCount = 1;
        // splits rules where the RHS has more than 2 symbols
        private void SplitOversizedRules(List<CFG_Rule> oversizedRules, List<CFG_Rule> targetRules)
        {
            List<CFG_Rule> fixedRules = new List<CFG_Rule>();

            while (oversizedRules.Count > 0)
            {
                CFG_Rule currentRule = oversizedRules[0];

                if (currentRule.RHS.Count <= 2)
                {
                    oversizedRules.RemoveAt(0);
                    continue;
                }

                // VP -> GO TO Det N
                // ----------------
                // VP -> X1 Det N
                // X1 -> GO TO
                // 
                // VP -> X2 N
                // X2 -> X1 Det

                // make a name for the intermediate symbol
                string intermediateSymbolName = "X" + XCount;
                XCount++;

                // now register it with the symbol registry
                int intermediateSymbol = getSymbolCode(intermediateSymbolName);
                NonTerminalSymbols.Add(intermediateSymbol);

                // generate and add the intermediate rule
                int replace1 = currentRule.RHS[0];
                int replace2 = currentRule.RHS[1];
                targetRules.Add(new CFG_Rule(intermediateSymbol, new List<int>() { replace1, replace2 }) { IsXRule = true });



                // remove one symbol, and replace the other with the new one
                currentRule.RHS.RemoveRange(0, 1);
                currentRule.RHS[0] = intermediateSymbol;


                // now add it to the new set, since currentRule is part of the oversizedRule set that will be removed
                if (currentRule.RHS.Count <= 2)
                {
                    oversizedRules.Remove(currentRule);
                    fixedRules.Add(currentRule);
                    continue;
                }
                // targetRules.Add(new CFG_Rule(currentRule));




            }

            targetRules.AddRange(fixedRules);
        }

        // generates dummy non-terminals to prevent terminals and non-terminals from coexisting in same rule
        private void ConvertTerminalsToDummyNonTerminals(CFG_Rule rule, List<CFG_Rule> targetRules)
        {
            // mixing terminals and non-terminals

            // INF-VP -> to VP
            // --------------
            // INF-VP -> TO VP
            // TO -> to

            // VP -> go to Det N
            // -----------------
            // VP -> GO TO Det N
            // GO -> go
            // TO -> to


            for (int i = 0; i < rule.RHS.Count; i++)
            {
                int symbol = rule.RHS[i];

                if (TerminalSymbols.Exists(o => o == rule.RHS[i])) // it's a terminal symbol, so let's change it
                {
                    // generate the dummy symbol
                    string dummyName = "D_" + GetSymbolLabelFromCode(symbol);
                    dummyName = dummyName.ToUpper();

                    int dummySymbol = getSymbolCode(dummyName); // this registers the symbol's label as well as the int code
                    NonTerminalSymbols.Add(dummySymbol);

                    // add the "GO -> go" rule
                    targetRules.Add(new CFG_Rule(dummySymbol, new List<int>() { symbol }) { IsDummy = true });

                    // change the terminal symbol in the existing rule to the new dummy symbol
                    rule.RHS[i] = dummySymbol;

                }

            }

        }




    } // end of class
}
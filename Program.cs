using System;
using System.Collections.Generic;

namespace ParsingViaCKY
{
    class Program
    {
        static void Main(string[] args)
        {

            ContextFreeGrammar L1;
            ContextFreeGrammar L2;


            // L1 = new ContextFreeGrammar("test1.txt");
            L1 = new ContextFreeGrammar("game2.txt");
            // L1 = new ContextFreeGrammar("mixed.txt");
            //  L1 = new ContextFreeGrammar("L1.txt");
            //L1 = new ContextFreeGrammar("game.txt");
            //foreach ()

            Console.WriteLine("Grammar loaded.");
            //   Console.ReadKey();

            Console.WriteLine("");



            Console.WriteLine("All rules as loaded.");
            L1.PrintAllRules();

            L1.PrintSymbols();



            Console.WriteLine("");
            //  Console.WriteLine("PRESS ANY KEY TO CONVERT TO CHOMSKY NORMAL FORM");
            Console.WriteLine("");

            // Console.ReadKey(true);


            //L2 = new ContextFreeGrammar(L1);
            L2 = new ContextFreeGrammar("game2.txt");
            L2.ConvertToCNF();


            Console.WriteLine("All rules after conversion to Chomsky Normal Form.");
            L2.PrintAllRules();

            L2.PrintSymbols();
            // Console.ReadKey(true);


            Console.WriteLine("");



            //  string sentence = "book the flight through Houston";
            //string sentence = "wear red robe";
            //string sentence = "hi";
            string sentence = "wear the leather armor";
            //string sentence = "go to the store";
            // string sentence = "wear the armor";
            //  string sentence = "wear the red silk robe";




            CKY_Table table = new CKY_Table(sentence, L2);


            Console.WriteLine("Now performing parse on \"{0}\", which has {1} words.", sentence, table.NumberOfWords);
            Console.WriteLine("");



            table.Build();


            Console.WriteLine("");


            for (int i = 0; i < table.NumberOfWords; i++)
            {
                for (int j = 0; j < table.NumberOfWords; j++)
                {
                    table.TableCells[i, j].Print(L2, i, j);
                }
            }


            Console.WriteLine("");


            // at this point, table[0, NumberOfWords - 1] contains heads for parse trees

            table.PrintAllParses();



            List<ParseTree> trees = ParseTree.BuildParseTrees(L1, L2, table);

            Console.WriteLine($"{trees.Count} parse trees constructed.");

            //BuildParseTree(L1, L2, table);




            foreach (ParseTree t in trees)
            {
                t.Print();



                // Console.WriteLine($"Verb: {t.GetConstituentString("V")}");


                ParseTreeNode S = t.GetRoot();
                ParseTreeNode DO = S.GetConstituentTree("DO");
                ParseTreeNode V = S.GetConstituentTree("V");

                string directObject = DO.GetString();
                string verb = V.GetString();


                Console.WriteLine($"{S.GetString()} has been processed.");
                Console.WriteLine($"Verb: {verb}");
                Console.WriteLine($"Direct Object: {directObject}");


            }






            // 





            //for (char c = (char)0; c < (char)255; c++)
            //{
            //    Console.Write("" + c);
            //}

            Console.ReadKey(true);
        }




    }
}

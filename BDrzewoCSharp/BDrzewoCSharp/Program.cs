using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDrzewoCSharp
{
    class Program
    {

        static private List<instruction> generateInst(int num)
        {
            int maxNum = num * 10;
            string rec = "GTC02165";
            var instrL = new List<instruction>();
            var rand = new Random();
            for (int i =0; i < num; i++)
            {
                instruction.operation opr = instruction.operation.add;
                int key = rand.Next(maxNum);
                instrL.Add(new instruction(opr, rec, key));
            }
            return instrL;
        }

        static private List<instruction> wczytaj(string file)
        {
            string instr = File.ReadAllText(file);
            instr = instr.Trim();
            string[] instrs = instr.Split('\n');
            List<instruction> instrsL = new List<instruction>();
            foreach(string ins in instrs)
            {
                char op = ins[0];
                int key = int.Parse(ins.Substring(1, 6));
                string record = ins.Substring(7).Trim();
                instruction.operation opr;
                if(op == 'A')
                {
                    opr = instruction.operation.add;
                }
                else
                {
                    opr = instruction.operation.add;
                }
                instrsL.Add(new instruction(opr, record, key));
            }
            return instrsL;
        }

        static void Main(string[] args)
        {
            string odp;
            Console.WriteLine("Aby wczytać operacje z pliku, wpisz 1, aby wpisywać dane ręcznie wpisz 0, aby wygenerować dane wpisz 2");
            odp = Console.ReadLine();
            while(odp != "0" && odp != "1" && odp != "2")
            {
                Console.WriteLine("Niepoprawna wartość, wpisz 1 (wczytywanie), 0 (wpisywanie), lub 2 (generowanie)");
                odp = Console.ReadLine();
            }

            List<instruction> instructions;

            if(odp == "1")
            {
                Console.WriteLine("Podaj nazwę pliku");
                odp = Console.ReadLine();
                instructions = wczytaj(odp);
            }
            if(odp == "2")
            {
                Console.WriteLine("Podaj ilość instrukcji do wygenerowania");
                odp = Console.ReadLine();
                int o = int.Parse(odp);
                instructions = generateInst(o);
            }
            else
            {
                instructions = null;
            }
            Console.WriteLine("Podaj nazwę pliku BDrzewa i pliku z rekordami");
            string BDplik = Console.ReadLine();
            string Rplik = Console.ReadLine();

            BTree tree = new BTree(BDplik, Rplik, true);

            if(instructions != null)
            {
                //float mean = 0;
                foreach(var inst in instructions)
                {
                    tree.operacjeDyskowe = 0;
                    inst.execute(tree);
                    tree.show(false);
                    Console.WriteLine(tree.operacjeDyskowe);
                    //Console.WriteLine($"Wykonano operację na drzewie. Ilość operacji dyskowych wyniosła {tree.operacjeDyskowe}");
                    //mean += tree.operacjeDyskowe;
                    //if(inst.key < 100)
                    //{
                    //    if(inst.key % 10 == 0)
                    //    {
                    //        mean = mean / 10;
                    //        Console.WriteLine(mean);
                    //        Console.ReadLine();
                    //        mean = 0;
                    //    }
                    //}else if(inst.key < 1000)
                    //{
                    //    if(inst.key % 100 == 0)
                    //    {
                    //        mean = mean / 100;
                    //        Console.WriteLine(mean);
                    //        Console.ReadLine();
                    //        mean = 0;
                    //    }
                    //}
                    //else if (inst.key < 10000)
                    //{
                    //    if (inst.key % 1000 == 0)
                    //    {
                    //        mean = mean / 1000;
                    //        Console.WriteLine(mean);
                    //        Console.ReadLine();
                    //        mean = 0;
                    //    }
                    //}
                    //else if (inst.key < 100000)
                    //{
                    //    if (inst.key % 10000 == 0)
                    //    {
                    //        mean = mean / 10000;
                    //        Console.WriteLine(mean);
                    //        Console.ReadLine();
                    //        mean = 0;
                    //    }
                    //}
                    //tree.operacjeDyskowe = 0;
                    
                    //tree.show(false);
                }
            }

            Console.WriteLine("Aby dodać rekord wpisz 'add %key %record', gdzie %key to wartość klucza, a %rekord do nazwa rekordu.\nAby wyszukać rekord wpisz 'search %key'.\nAby pokazać stan plików wpisz 'show'\nAby pokazać wszystkie rekordy posortowane według klucza wpisz 'sorted'\nAby zakończyć program wpisz quit");
            while(odp != "quit")
            {
                odp = Console.ReadLine();
                string[] odps = odp.Split(' ');
                if (odps[0] == "quit")
                {
                    return;
                }
                else if (odps[0] == "add")
                {
                    if (odps.Length < 3)
                    {
                        Console.WriteLine($"Zbyt mało argumentów dla operacji add ({odps.Length - 1}, wymagane 2)");
                    }
                    else
                    {
                        int key = int.Parse(odps[1]);
                        string rec = odps[2];
                        while(rec.Length < 8)
                        {
                            rec = rec + " ";
                        }
                        if(rec.Length > 8)
                        {
                            rec = rec.Substring(0, 8);
                        }
                        string insert = tree.Insert(key, rec);
                        Console.WriteLine($"Insertion: {insert}");
                        Console.WriteLine($"Ilość operacji dyskowych wyniosła {tree.operacjeDyskowe}");
                        tree.operacjeDyskowe = 0;
                    }
                } else if (odps[0] == "search") {
                    if(odps.Length < 2)
                    {
                        Console.WriteLine($"Zbyt mało argumentów dla operacji search ({odps.Length - 1}, wymagane 1)");
                    }
                    else
                    {
                        int key = int.Parse(odps[1]);
                        string rec = tree.Search(key);
                        Console.WriteLine($"Search: {rec}");
                        Console.WriteLine($"Ilość operacji dyskowych wyniosła {tree.operacjeDyskowe}");
                        tree.operacjeDyskowe = 0;
                    }
                }
                else if(odps[0] == "show")
                {
                    tree.show(false);
                }
                else if(odps[0] == "sorted")
                {
                    tree.show(true);
                }
                else
                {
                    Console.WriteLine("Niepoprawne polecenie. Aby dodać rekord wpisz 'add %key %record', aby wyszukać rekord wpisz 'search %key', aby wyjść wpisz 'quit'");
                }
            }
        }
    }
}

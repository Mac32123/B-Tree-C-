using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDrzewoCSharp
{
    class BTree
    {
        private const int d = 2;
        private BDPage actPage;
        private BDPage parent;
        private BDPage sibling;
        private string indexFile;
        private string recordsFile;
        private int root = 0;
        private List<int> usedPages;
        public int operacjeDyskowe { set; get; }

        public BTree(string indexFile, string recordsFile, bool create)
        {
            this.indexFile = indexFile;
            this.recordsFile = recordsFile;
            if (create)
            {
                actPage = new BDPage(indexFile, 0, -1, true);
                actPage.save();
                usedPages = new List<int>();
                usedPages.Add(0);
                RecFile.fileName = this.recordsFile;
                if (File.Exists(recordsFile))
                {
                    File.Delete(recordsFile);
                }
            }
            operacjeDyskowe = 0;
        }

        public string Search(int key)
        {
            int addr = Search(key, 0).address;
            if (addr == -1) return "Record not found";
            return RecFile.readRecord(key, addr);

        }

        public response Search(int key, int nothing)
        {
            response resp;
            actPage = new BDPage(indexFile, root, false);
            operacjeDyskowe++;
            resp = actPage.Search(key);
            if (resp.key != -1) return resp;
            else if (resp.pointer == -1) return resp;
            while(resp.pointer != -1)
            {
                actPage = new BDPage(indexFile, resp.pointer, false);
                operacjeDyskowe++;
                resp = actPage.Search(key);
                if (resp.key != -1) return resp;
            }
            return resp;
        }

        public bool Compensate(int key, int address, int pointer, int index, bool right)
        {
            if (sibling.fill >= 2 * d) return false;                            
            response overflow = actPage.Insert(key, address, pointer, right);       //overflow zawiera nadmiarowy klucz, adres i index, przesuwanie zgodnie z kierunkiem zadanym w right
            int tmpParent = parent.getKey(index);
            int tmpAddr = parent.getAddress(index);
            parent.setKey(overflow.key, index);
            parent.setAddresses(overflow.address, index);
            if (right)
            {
                sibling.Insert(tmpParent, tmpAddr, overflow.pointer, true);
            }
            else
            {
                sibling.Insert(tmpParent, tmpAddr, overflow.pointer, true);
                int tmpP = sibling.getPointer(sibling.fill + 1);                //konieczna zamiana wskaźników, ponieważ wskaźnik z nadmiarowego rekordu musi być na końcu
                sibling.setPointer(overflow.pointer, sibling.fill + 1);
                sibling.setPointer(tmpP, sibling.fill);
            }
            int pntr = overflow.pointer;
            if (pntr != -1)
            {
                BDPage child = new BDPage(indexFile, pntr, false);
                child.parent = sibling.myNum;
                child.save();
                operacjeDyskowe++;
            }
            parent.save();
            sibling.save();
            actPage.save();
            operacjeDyskowe += 3;
            return true;
        }

        public bool Compensation(int key, int address, int pointer)
        {
            if (actPage.isRoot) return false;
            parent = new BDPage(indexFile, actPage.parent, false);                      //dostęp do rodzica
            operacjeDyskowe++;
            int index = parent.SearchPointerIndex(actPage.myNum);
            if(!(index == 2*d))                                                 //najpierw chcemy sprawdzić prawego brata, chyba że nie istnieje
            {
                if (parent.getPointer(index + 1) != -1)
                {
                    sibling = new BDPage(indexFile, parent.getPointer(index + 1), false);
                    operacjeDyskowe++;
                    if (Compensate(key, address, pointer, index, true)) return true;      //jeżeli się nie zkompensuje to może jeszcze spróbować drugiego brata
                }
            }       
            if (!(index == 0))
            {
                sibling = new BDPage(indexFile, parent.getPointer(index - 1), false);
                operacjeDyskowe++;
                return Compensate(key, address, pointer, index - 1, false);          //index -1, ponieważ jeżeli bieżemy lewego brata, to klucz ich rodzica znajduje sie na lewo od wskaźnika
            }
            return false;
        }

        private void Split(int key, int address, int pointer)
        {
            //if (!actPage.isRoot)
            {
                int index = 0;
                int rootIndex = 0;
                while (usedPages.Contains(index))
                {
                    index++;
                }
                usedPages.Add(index);
                if (actPage.isRoot)
                {
                    while (usedPages.Contains(rootIndex))
                    {
                        rootIndex++;
                    }
                    usedPages.Add(rootIndex);
                }
                if (actPage.isRoot)
                {
                    parent = new BDPage(indexFile, rootIndex, -1, true);
                    operacjeDyskowe++;
                    parent.setPointer(actPage.myNum, 0);
                    actPage.parent = rootIndex;
                    root = rootIndex;
                    actPage.isRoot = false;
                }
                sibling = new BDPage(indexFile, index, parent.myNum, true);
                operacjeDyskowe++;
                response overflow = actPage.Insert(key, address, pointer, true);
                int pntr;
                for (int i = 0; i < d; i++)
                {
                    pntr = actPage.getPointer(i);
                    if(pntr != -1)
                    {
                        BDPage child = new BDPage(indexFile, pntr, false);
                        child.parent = sibling.myNum;
                        child.save();
                        operacjeDyskowe += 2;
                    }
                    sibling.setPointer(pntr, i);
                    sibling.setKey(actPage.getKey(i), i);
                    sibling.setAddresses(actPage.getAddress(i), i);
                }
                pntr = actPage.getPointer(d);
                if (pntr != -1)
                {
                    BDPage child = new BDPage(indexFile, pntr, false);
                    child.parent = sibling.myNum;
                    child.save();
                    operacjeDyskowe += 2;
                }
                sibling.setPointer(pntr, d);
                sibling.save();
                operacjeDyskowe++;

                int midKey = actPage.getKey(d);
                int midAddr = actPage.getAddress(d);
                int midPtr = index;
                for (int i =0; i < d - 1; i++)
                {
                    actPage.setPointer(actPage.getPointer(d + i + 1), i);
                    actPage.setKey(actPage.getKey(d + i + 1), i);
                    actPage.setAddresses(actPage.getAddress(d + i + 1), i);
                }
                actPage.setPointer(overflow.pointer, d - 1);
                actPage.setKey(overflow.key, d - 1);
                actPage.setAddresses(overflow.address, d - 1);
                int lastpointer = actPage.getPointer(d + d);
                for(int i = d; i < 2*d; i++)
                {
                    actPage.setKey(-1, i);
                    actPage.setAddresses(-1, i);
                    actPage.setPointer(-1, i);
                }
                actPage.setPointer(-1, 2 * d);
                actPage.setPointer(lastpointer, d);
                actPage.save();
                operacjeDyskowe++;
                actPage = parent;
                Insert(midKey, midAddr, midPtr);
            }
        }

        private string Insert(int key, int address, int pointer)
        {
            if(actPage.fill < 2 * d)                                //rekord zmieści się na stronie
            {
                actPage.Insert(key, address, pointer, true);
                actPage.save();
                operacjeDyskowe++;
                return "OK";
            }
            if (Compensation(key, address, pointer)) return "OK";       //próba kompensacji
            Split(key, address, pointer);
            return "OK";
        }

        public string Insert(int key, string record)
        {
            response resp = Search(key, 0);
            if (resp.key != -1) return "Already exists";
            int address = RecFile.putRecord(key, record);
            string status = Insert(key, address, -1);
            return status;
        }

        public void show(bool mode)             // 0 = show, 1 = sorted
        {
            if (mode)
            {
                Console.WriteLine("Records: ");
                showSortedRecords();
            }
            else
            {
                Console.WriteLine("BTree File:");
                showTree();
                Console.WriteLine("Record File:");
                showRecords();
            }
        }

        private void showSortedRecords()
        {
            showSortedRec(root);
            Console.WriteLine();
        }

        private void showSortedRec(int pointer)
        {
            BDPage Page = new BDPage(indexFile, pointer, false);
            int pntr;
            for (int i = 0; i < 2 * d + 1; i++)
            {
                pntr = Page.getPointer(i);
                if (pntr != -1) showSortedRec(pntr);
                if (i != 2 * d)
                {
                    int addr = Page.getAddress(i);
                    int key = Page.getKey(i);
                    if (key != -1)
                    {
                        string record = RecFile.readRecord(key, addr);
                        Console.WriteLine($"Key: {key}, record: {record}");
                    }
                }
            }
        }

        private void showRecords()
        {
            RecFile.showAll();
        }

        private void showRec(int pointer)
        {
            BDPage Page = new BDPage(indexFile, pointer, false);
            Console.WriteLine($"{pointer}: ");
            if (Page.parent == -1) Console.WriteLine("Parent: none");
            else Console.WriteLine($"Parent: {Page.parent}");
            for (int i = 0; i < 2 * d; i++)
            {
                Console.Write($"p{i}: {Page.getPointer(i)} ");
                Console.Write($"k{i}: {Page.getKey(i)} ");
                Console.Write($"a{i}: {Page.getAddress(i)} ");
            }
            Console.Write($"p{2 * d}: {Page.getPointer(2 * d)}\n");

            Console.WriteLine();

            int pntr;
            for(int i =0; i < 2*d+1; i++)
            {
                pntr = Page.getPointer(i);
                if (pntr != -1) showRec(pntr);
                else
                {
                    break;
                }
            }  
        }

        public void showTree()
        {
            showRec(root);
            Console.WriteLine();
        }

    }
}

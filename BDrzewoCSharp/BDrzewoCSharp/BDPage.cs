using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDrzewoCSharp
{
    class response
    {
         public int key { get; }
         public int address { get; }
         public int pointer { get; }
        public response(int key, int address, int pointer)
        {
            this.address = address;
            this.pointer = pointer;
            this.key = key;
        }
    }
    class BDPage
    {
        private const int d = 2;
        private int[] pointers = new int[d*2 + 1];
        private int[] keys = new int[d * 2];
        private int[] addresses = new int[d * 2];
        public int parent { get; set; }
        public int myNum { get; }
        public int fill { get; }
        private bool filled = false;
        public bool isRoot { get; set; }
        private string fileName;
        private int size = 2 * d * 3 * sizeof(int) + 2* sizeof(int);

        public BDPage(string fileName, int pageNumber, bool create) : this(fileName, pageNumber, -1, create) { }

        public BDPage(string fileName, int pageNumber, int parent, bool create)
        {
            myNum = pageNumber;
            fill = 0;
            this.fileName = fileName;
            if (create)
            { 
                for (int i = 0; i < d * 2; i++)
                {
                    keys[i] = -1;
                    pointers[i] = -1;
                    addresses[i] = -1;
                }
                pointers[d * 2] = -1;
                this.parent = parent;
            }
            else
            {
                FileStream fs = new FileStream(fileName, FileMode.Open);
                byte[] buffer = new byte[size];
                fs.Seek(myNum * size, SeekOrigin.Begin);
                fs.Read(buffer, 0, size);
                fs.Close();
                int[] all = new int[6 * d + 2];
                Buffer.BlockCopy(buffer, 0, all, 0, size);
                for (int i =0; i < 2*d; i++)
                {
                    int j = 3 * i;
                    pointers[i] = all[j];
                    keys[i] = all[j + 1];
                    if (!filled && keys[i] == -1) { 
                        fill = i;
                        filled = true;
                    }
                    addresses[i] = all[j + 2];
                }
                if (!filled)
                {
                    fill = d * 2;
                    filled = true;
                }
                pointers[2 * d] = all[all.Length - 2];
                this.parent = all[all.Length - 1];
            }
            if (this.parent == -1)
            {
                isRoot = true;
            }
            else
            {
                isRoot = false;
            }
        }
        public void save()
        {
            int[] all = new int[6 * d + 2];
            for(int i =0; i < 2*d; i++)
            {
                int j = 3 * i;
                all[j] = pointers[i];
                all[j + 1] = keys[i];
                all[j + 2] = addresses[i];
            }
            all[all.Length - 2] = pointers[2 * d];
            all[all.Length - 1] = parent;
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(all, 0, buffer, 0, size);
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            fs.Seek(myNum * size, SeekOrigin.Begin);
            fs.Write(buffer, 0, size);
            fs.Close();
        }
        public int getKey(int index)
        {
            return keys[index];
        }
        public void setKey(int key, int index)
        {
            keys[index] = key;
        }
        public int getPointer(int index)
        {
            return pointers[index];
        }
        public void setPointer(int pointer, int index)
        {
            pointers[index] = pointer;
        }
        public int getAddress(int index)
        {
            return addresses[index];
        }
        public void setAddresses(int address, int index)
        {
            addresses[index] = address;
        }

        public int SearchPointerIndex(int pointer)
        {
            for(int i =0; i < 2*d +1; i++)
            {
                if(pointer == pointers[i])
                {
                    return i;
                }
            }
            return -1;
        }

        public response Search(int key)
        {
            for(int i =0; i < 2*d; i++)
            {
                if(key == keys[i])
                {
                    response resp = new response(key, addresses[i], -1);
                    return resp;
                }
                else if (keys[i] > key)
                {
                    response resp = new response(-1, -1, pointers[i]);
                    return resp;
                }
                else if (keys[i] == -1)
                {
                    response resp = new response(-1, -1, pointers[i]);
                    return resp;
                }
            }
            response resp1 = new response(-1, -1, pointers[2*d]);
            return resp1;
        }
        public response Insert(int key, int address, int pointer, bool direction) // true = normalnie, false = do tyłu
        {
            int index = -1;
            for (int i = 0; i < 2 * d; i++)
            {
                if(keys[i] > key)
                {
                    index = i;
                    break;
                }else if(keys[i] == -1)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) index = 2 * d;
            if (!direction) index--;
            if (index < 0) return new response(key, address, pointer);
            if (index == 2 * d) return new response(key, address, pointer);
            int tmpK = keys[index];
            int tmpA = addresses[index];
            int tmpP = pointers[index];
            int tmpK2;
            int tmpA2;
            int tmpP2;
            keys[index] = key;
            addresses[index] = address;
            pointers[index] = pointer;
            if (direction)
            {
                for (int i = index + 1; i <= fill && i < d * 2; i++)
                {
                    tmpK2 = keys[i];
                    tmpA2 = addresses[i];
                    tmpP2 = pointers[i];
                    keys[i] = tmpK;
                    addresses[i] = tmpA;
                    pointers[i] = tmpP;
                    tmpK = tmpK2;
                    tmpA = tmpA2;
                    tmpP = tmpP2;
                }
                tmpP2 = pointers[Math.Min(fill + 1, d*2)];
                pointers[Math.Min(fill + 1, d*2)] = tmpP;
                tmpP = tmpP2;
            }
            else
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    tmpK2 = keys[i];
                    tmpA2 = addresses[i];
                    tmpP2 = pointers[i];
                    keys[i] = tmpK;
                    addresses[i] = tmpA;
                    pointers[i] = tmpP;
                    tmpK = tmpK2;
                    tmpA = tmpA2;
                    tmpP = tmpP2;
                }
            }
            response resp = new response(tmpK, tmpA, tmpP);
            return resp;
        }
    }
}

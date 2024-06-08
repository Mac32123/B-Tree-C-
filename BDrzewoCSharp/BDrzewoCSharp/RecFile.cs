using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDrzewoCSharp
{
    class RecFile
    {
        public static string fileName { get; set; }
        private const int recordSize = 13;

        public static int putRecord(int key, string record)
        {
            byte[] buffer1;
            byte[] buffer2;
            byte[] buffer;
            buffer1 = BitConverter.GetBytes(key);
            buffer2 = Encoding.UTF8.GetBytes(record + "\n");
            buffer = buffer1.Concat(buffer2).ToArray();
            FileStream fs = new FileStream(fileName, FileMode.Append);
            int addr = (int) fs.Position;
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            return addr;
        }

        public static string readRecord(int key, int addr)
        {
            byte[] buffer = new byte[recordSize];
            FileStream fs = new FileStream(fileName, FileMode.Open);
            fs.Seek(addr, SeekOrigin.Begin);
            fs.Read(buffer, 0, recordSize);
            fs.Close();
            int readKey = BitConverter.ToInt32(buffer, 0);
            if(readKey != key)
            {
                return "Error, invalid key\n";
            }
            string str = Encoding.UTF8.GetString(buffer, 4, recordSize - 4);
            str = str.Trim();
            return str;
        }

        public static void showAll()
        {
            var info = new FileInfo(fileName);
            byte[] buffer = new byte[info.Length];
            FileStream fs = new FileStream(fileName, FileMode.Open); 
            fs.Read(buffer, 0, (int) info.Length);
            fs.Close();
            int iterations = (int) info.Length / recordSize;
            for(int i =0; i < iterations; i++)
            {
                int key = BitConverter.ToInt32(buffer, i * recordSize);
                string str = Encoding.UTF8.GetString(buffer, i * recordSize + 4, recordSize - 4);
                str = str.Trim();
                Console.WriteLine($"Key: {key}, record: {str}");
            }
            Console.WriteLine();
        }

    }
}

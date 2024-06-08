using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDrzewoCSharp
{
    class instruction
    {
        public enum operation
        {
            add
        }
        public operation op;
        public string record;
        public int key;

        public instruction(operation op, string record, int key)
        {
            this.op = op;
            this.record = record;
            this.key = key;
        }

        public void execute(BTree tree)
        {
            if(op == operation.add)
            {
                tree.Insert(key, record);
            }
        }

    }
}

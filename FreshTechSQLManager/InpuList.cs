using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    public class InputList
    {
        public string Name { get; set; }

        public InputType Type { get; set; }

        public List<string> InputItems { get; set;}
    }
}

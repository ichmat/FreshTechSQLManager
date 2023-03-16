using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FreshTechSQLManager.CommandReader;

namespace FreshTechSQLManager
{
    public class Command
    {
        public string CommandString { get; set; }
        public CommandType CommandType { get; set; }
    }
}

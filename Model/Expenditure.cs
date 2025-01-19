using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBudget.Model
{
    public class Expenditure
    {
        public String Name { get; set; }
        public Int32 StatusId { get; set; }
        public Decimal Money { get; set; }
    }
}

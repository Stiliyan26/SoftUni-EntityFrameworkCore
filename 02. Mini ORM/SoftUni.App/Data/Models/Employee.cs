using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUni.App.Data.Models
{
    public class Employee : ISalary
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int GetSalary()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{this.Id} - {this.Name}";
        }
    }
}

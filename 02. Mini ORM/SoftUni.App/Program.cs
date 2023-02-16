using MiniORM;
using SoftUni.App.Data.Models;
using System;
using System.Collections.Generic;

namespace SoftUni.App
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Employee> employeeList = new List<Employee>()
            {
                new Employee()
                {
                    Id = 1,
                    Name = "Pesho"
                },
                new Employee()
                {
                    Id = 2,
                    Name = "Gosho"
                }
            };
            /*ChangeTracker<Employee> changeTracker = new ChangeTracker<Employee>(employeeList);
            Console.WriteLine(string.Join(", ", changeTracker.AllEntities));*/
        }
    }
}

using Softuni.Data;
using Softuni.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Softuni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using SoftUniContext dbContext = new SoftUniContext();

            string result = DeleteProjectById(dbContext);
            Console.WriteLine(result);
            Console.WriteLine("---------");
            Console.WriteLine($"Elapsed time for query: {stopwatch.ElapsedMilliseconds}");
        }

        /*--Problem 03*/
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();

            var allEmployees = context
                .Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,       
                    e.JobTitle,
                    e.Salary
                })
                .ToArray();


            foreach (var emp in allEmployees)
            {
                output
                    .AppendLine($"{emp.FirstName} {emp.LastName} {emp.MiddleName} {emp.JobTitle} {emp.Salary:f2}");
            }

            return output
                    .ToString()
                    .TrimEnd();
        }

        /*--Problem 05*/
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();

            var rndEmployees = context
                .Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DepartmentName = e.Department.Name,
                    e.Salary
                })
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToArray();

            foreach (var rndE in rndEmployees)
            {
                output
                    .AppendLine($"{rndE.FirstName} {rndE.LastName} from {rndE.DepartmentName} - {rndE.Salary:f2}");
            }

            return output
                .ToString()
                .TrimEnd();
        }

        /*--Problem 06*/
        public static async Task<string> AddNewAddressToEmployee(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();

            Address newAddress = new Address()
            {
                AddressText = "Vitoshka 15",
                TownId = 4
            };

            context
                .Addresses
                .Add(newAddress);

            Employee nakov = context
                .Employees
                .FirstOrDefault(e => e.LastName == "Nakov");

            nakov.Address = newAddress;
            await context.SaveChangesAsync();

            string[] addressTexts = context
                .Employees
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .Select(e => e.Address.AddressText)
                .ToArray();

            foreach (var address in addressTexts)
            {
                output
                    .AppendLine(address);
            }
                
            return output
                .ToString()
                .TrimEnd();
        }

        /*--Problem 07*/

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder();

            var employeesWithProjects = context
                .Employees
                .Where(e => e.EmployeesProjects.Any(ep => ep.Project.StartDate.Year >= 2001
                                                       && ep.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    AllProjects = e.EmployeesProjects
                        .Select(ep => new
                        {
                            ProjectName = ep.Project.Name,
                            StartDate = ep
                                .Project
                                .StartDate
                                .ToString("M/d/yyyy h:mm:ss tt"),
                            EndDate = ep.Project.EndDate.HasValue
                                ? ep.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt")
                                : "not finished"
                        })
                        .ToArray()
                })
                .ToArray();

            foreach (var e in employeesWithProjects)
            {
                output
                    .AppendLine($"{e.FirstName} {e.LastName} - Manager: {e.ManagerFirstName}, {e.ManagerLastName}");

                foreach (var p in e.AllProjects)
                {
                    output
                        .AppendLine($"{p.ProjectName} - {p.StartDate} - {p.EndDate}");
                }
            }

            return output
                .ToString()
                .TrimEnd();
        }

        /*Problem 14*/
        public static string DeleteProjectById(SoftUniContext context)
        {
            StringBuilder output = new StringBuilder(); 

            Project projectToDelte = context
                .Projects
                .Find(2);

            EmployeesProject[] referredEmployee = context
                .EmployeesProjects
                .Where(ep => ep.ProjectId == projectToDelte.ProjectId)
                .ToArray();

            context.EmployeesProjects.RemoveRange(referredEmployee);
            context.Projects.Remove(projectToDelte);
            context.SaveChanges();

            string[] projectNames = context
                .Projects
                .Take(10)
                .Select(p => p.Name)
                .ToArray();

            foreach (var pName in projectNames) {
                output
                    .AppendLine(pName);
            }

            return output
                    .ToString()
                    .TrimEnd();
        }
    }
}
        
namespace TeisterMask.DataProcessor
{
    using Data;
    using Microsoft.VisualBasic;
    using Newtonsoft.Json;
    using System.Globalization;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Xml.Serialization;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ExportDto;

    public class Serializer
    {
        public static string ExportProjectWithTheirTasks(TeisterMaskContext context)
        {
            ExportProjectDto[] projects = context
                .Projects
                .Where(p => p.Tasks.Any())
                .Select(p => new ExportProjectDto()
                {
                    TasksCount = p.Tasks.Count().ToString(),
                    ProjectName = p.Name,
                    HasEndDate = p.DueDate.HasValue ? "Yes" : "No",
                    Tasks = p.Tasks
                        .Select(t => new ExportTaskDto()
                        {
                            Name = t.Name,
                            Label = t.LabelType.ToString(),
                        })
                        .OrderBy(t => t.Name)
                        .ToArray()
                })
                .OrderByDescending(p => p.Tasks.Length)
                .ThenBy(p => p.ProjectName)
                .ToArray();

            /* StringBuilder sb = new StringBuilder();
             using StringWriter writer = new StringWriter(sb);

             XmlRootAttribute root = new XmlRootAttribute("Projects");
             XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
             namespaces.Add(string.Empty, string.Empty);

             XmlSerializer serializer = new XmlSerializer(typeof(ExportProjectDto[]), root);
             serializer.Serialize(writer, projects, namespaces);*/

            string result = SerializeXml("Projects", projects);

            return result;
        }

        //Helper method to seriazlize
        public static string SerializeXml<T>(string root, T dto) 
        {
            StringBuilder sb = new StringBuilder();
            using StringWriter writer = new StringWriter(sb);

            XmlRootAttribute xmlRoot = new XmlRootAttribute(root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            XmlSerializer serializer = new XmlSerializer(typeof(T), xmlRoot);
            serializer.Serialize(writer, dto, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string ExportMostBusiestEmployees(TeisterMaskContext context, DateTime date)
        {
            var employees = context
                .Employees
                .Where(e => e.EmployeesTasks.Any(et => et.Task.OpenDate >= date))
                .ToArray()
                .Select(e => new
                {
                    Username = e.Username,
                    Tasks = e.EmployeesTasks
                        .Where(et => et.Task.OpenDate >= date)
                        .ToArray()
                        .OrderByDescending(et => et.Task.DueDate)
                        .ThenBy(et => et.Task.Name)
                        .Select(et => new
                        {
                            TaskName = et.Task.Name,
                            OpenDate = et.Task.OpenDate.ToString("d", CultureInfo.InvariantCulture),
                            DueDate = et.Task.DueDate.ToString("d", CultureInfo.InvariantCulture),
                            LabelType = Enum.GetName(typeof(LabelType), et.Task.LabelType),
                            ExecutionType = Enum.GetName(typeof(ExecutionType), et.Task.ExecutionType)
                        })
                        .ToArray()
                })
                .OrderByDescending(e => e.Tasks.Length)
                .ThenBy(e => e.Username)
                .Take(10)
                .ToArray();

            string json = JsonConvert.SerializeObject(employees, Formatting.Indented);

            return json;
        }
    }
}
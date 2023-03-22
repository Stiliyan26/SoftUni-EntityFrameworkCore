// ReSharper disable InconsistentNaming

namespace TeisterMask.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Text;
    using System.Xml.Serialization;
    using TeisterMask.DataProcessor.ImportDto;
    using TeisterMask.Data.Models;
    using System.Globalization;
    using Microsoft.VisualBasic;
    using TeisterMask.Data.Models.Enums;
    using System.Threading.Tasks;
    using System.Text.Json.Nodes;
    using Newtonsoft.Json;
    using System.Data.SqlTypes;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportProjectDto[] projectDtos = DeserializeXml<ImportProjectDto[]>("Projects", xmlString);

            ICollection<Project> validProjects = new List<Project>();
            foreach (ImportProjectDto pDto in projectDtos)
            {
                if (!IsValid(pDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isOpenDateValid = DateTime
                    .TryParseExact(pDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime validOpenDate);

                if (!isOpenDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime? validDueDate = null;

                if (!string.IsNullOrEmpty(pDto.DueDate))
                {
                    bool isDueDateValid = DateTime
                    .TryParseExact(pDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime validProjectDueDate);

                    if (!isDueDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    validDueDate = validProjectDueDate;
                }


                List<Data.Models.Task> validTasks = new List<Data.Models.Task>();
                foreach (ImportTaskDto tDto in pDto.Tasks)
                {
                    if (!IsValid(tDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isTaskOpenDateValid = DateTime
                        .TryParseExact(tDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime validTaskOpenDate);

                    if (!isTaskOpenDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isTaskDueDateValid = DateTime
                        .TryParseExact(tDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime validTaskDueDate);

                    if (!isTaskDueDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (validTaskOpenDate < validOpenDate || validDueDate.HasValue && validTaskDueDate > validDueDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isExecutionTypeValid = Enum
                        .TryParse(typeof(ExecutionType), tDto.ExecutionType, out object validExecutionType);

                    if (!isExecutionTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isLabelTypeValid = Enum
                        .TryParse(typeof(LabelType), tDto.LabelType, out object validLabelTypee);

                    if (!isLabelTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Data.Models.Task newTask = new Data.Models.Task()
                    {
                        Name = tDto.Name,
                        OpenDate = validTaskOpenDate,
                        DueDate = validTaskDueDate,
                        ExecutionType = (ExecutionType)validExecutionType,
                        LabelType = (LabelType)validLabelTypee
                    };

                    validTasks.Add(newTask);
                }

                Project newProject = new Project()
                {
                    Name = pDto.Name,
                    OpenDate = validOpenDate,
                    DueDate = validDueDate,
                    Tasks = validTasks
                };

                validProjects.Add(newProject);
                sb.AppendLine(string.Format(SuccessfullyImportedProject, newProject.Name, newProject.Tasks.Count));
            }

            context.Projects.AddRange(validProjects);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportEmplpyeeDto[] employeeDtos = JsonConvert
                .DeserializeObject<ImportEmplpyeeDto[]>(jsonString);

            ICollection<Employee> validEmployees = new List<Employee>();

            foreach (ImportEmplpyeeDto empDto in employeeDtos)
            {
                if (!IsValid(empDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Employee newEmp = new Employee()
                {
                    Username = empDto.Username,
                    Email = empDto.Email,
                    Phone = empDto.Phone,
                };

                int[] uniqieTaskIds = empDto
                    .Tasks
                    .Distinct()
                    .ToArray();

                List<EmployeeTask> empTasks = new List<EmployeeTask>();
                foreach (int taskId in uniqieTaskIds)
                {
                    Data.Models.Task task = context
                        .Tasks
                        .Find(taskId);

                    if (task == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    EmployeeTask empTask = new EmployeeTask()
                    {
                        Task = task,
                        Employee = newEmp
                    };

                    empTasks.Add(empTask);
                }

                newEmp.EmployeesTasks = empTasks;

                validEmployees.Add(newEmp);
                sb.AppendLine(string.Format(SuccessfullyImportedEmployee, newEmp.Username, newEmp.EmployeesTasks.Count));
            }

            context.Employees.AddRange(validEmployees);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        //Helper method deserializer

        public static T DeserializeXml<T>(string root, string xmlString)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute(root);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            StringReader reader = new StringReader(xmlString);

            T dtos = (T)xmlSerializer
                .Deserialize(reader);

            return dtos;
        }
        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
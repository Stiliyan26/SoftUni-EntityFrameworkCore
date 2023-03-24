namespace Footballers.DataProcessor
{
    using Footballers.Data;
    using Footballers.Data.Models;
    using Footballers.Data.Models.Enums;
    using Footballers.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCoach
            = "Successfully imported coach - {0} with {1} footballers.";

        private const string SuccessfullyImportedTeam
            = "Successfully imported team - {0} with {1} footballers.";

        public static string ImportCoaches(FootballersContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportCoachWithFootballersDto[] coacheDtos = DeserializeXml<ImportCoachWithFootballersDto[]>("Coaches", xmlString);

            ICollection<Coach> validCoaches = new List<Coach>();

            foreach (ImportCoachWithFootballersDto cDto in coacheDtos)
            {
                if (!IsValid(cDto) || string.IsNullOrEmpty(cDto.Nationality))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                ICollection<Footballer> validFootballers = new List<Footballer>();

                foreach (ImportFootballerDto fDto in cDto.Footballers)
                {
                    if (!IsValid(fDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isContractStartDateValid = DateTime
                        .TryParseExact(fDto.ContractStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime validContractStartDate);

                    if (!isContractStartDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isContractEndDateValid = DateTime
                       .TryParseExact(fDto.ContractEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                       DateTimeStyles.None, out DateTime validContractEndDate);

                    if (!isContractEndDateValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    if (validContractStartDate > validContractEndDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isBesSkillsTypeValid = Enum
                        .TryParse(typeof(BestSkillType), fDto.BestSkillType, out object validBestSkillType);

                    if (!isBesSkillsTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isPositionTypeValid = Enum
                        .TryParse(typeof(BestSkillType), fDto.PositionType, out object validPositionType);

                    if (!isPositionTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Footballer newFootballer = new Footballer()
                    {
                        Name = fDto.Name,
                        ContractStartDate = validContractStartDate,
                        ContractEndDate = validContractEndDate,
                        BestSkillType = (BestSkillType)validBestSkillType,
                        PositionType = (PositionType)validPositionType
                    };

                    validFootballers.Add(newFootballer);
                }

                Coach newCoach = new Coach()
                {
                    Name = cDto.Name,
                    Nationality = cDto.Nationality,
                    Footballers = validFootballers
                };

                validCoaches.Add(newCoach);
                sb.AppendLine(string.Format(SuccessfullyImportedCoach, newCoach.Name, newCoach.Footballers.Count));
            }

            context.Coaches.AddRange(validCoaches);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportTeamWithFootballersDto[] teamDtos = JsonConvert
                .DeserializeObject<ImportTeamWithFootballersDto[]>(jsonString);

            ICollection<Team> validTeams = new List<Team>();

            foreach (ImportTeamWithFootballersDto tDto in teamDtos)
            {
                if (!IsValid(tDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (tDto.Trophies == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                int trophis = int.Parse(tDto.Trophies);

                if (trophis == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Team newTeam = new Team()
                {
                    Name = tDto.Name,
                    Nationality = tDto.Nationality,
                    Trophies = trophis,
                };

                int[] uniqueFootballersIds = tDto.Footballers
                    .Distinct()
                    .ToArray();

                ICollection<TeamFootballer> validTeamFootballers = new List<TeamFootballer>();
                foreach (int fId in uniqueFootballersIds)
                {
                    Footballer existingFootballer = context
                        .Footballers
                        .FirstOrDefault(f => f.Id == fId);

                    if (existingFootballer == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    TeamFootballer tf = new TeamFootballer()
                    {
                        Footballer = existingFootballer,
                        Team = newTeam
                    };

                    validTeamFootballers.Add(tf);
                }

                newTeam.TeamsFootballers = validTeamFootballers;
                validTeams.Add(newTeam);
                sb.AppendLine(string.Format(SuccessfullyImportedTeam, newTeam.Name, newTeam.TeamsFootballers.Count));
            }

            context.Teams.AddRange(validTeams);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

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

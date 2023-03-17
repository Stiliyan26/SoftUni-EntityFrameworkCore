namespace Theatre.DataProcessor
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfulImportPlay
            = "Successfully imported {0} with genre {1} and a rating of {2}!";

        private const string SuccessfulImportActor
            = "Successfully imported actor {0} as a {1} character!";

        private const string SuccessfulImportTheatre
            = "Successfully imported theatre {0} with #{1} tickets!";



        public static string ImportPlays(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute root = new XmlRootAttribute("Plays");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPlayDto[]), root);

            StringReader reader = new StringReader(xmlString);
            ImportPlayDto[] playDtos = (ImportPlayDto[])xmlSerializer.Deserialize(reader);

            ICollection<Play> validPlays = new List<Play>();

            foreach (ImportPlayDto pDto in playDtos)
            {
                if (!IsValid(pDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                bool isInCorrectFormat = TimeSpan
                    .TryParseExact(pDto.Duration, "c",
                        CultureInfo.InvariantCulture, out TimeSpan validDuration);

                if (!isInCorrectFormat || validDuration.Hours < 1)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isGenreEnumValid = Enum
                    .TryParse(typeof(Genre), pDto.Genre, out object validGenre);

                if (!isGenreEnumValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Play play = new Play()
                {
                    Title = pDto.Title,
                    Duration = validDuration,
                    Rating = pDto.Raiting,
                    Genre = (Genre)validGenre,
                    Description = pDto.Description,
                    Screenwriter = pDto.Screenwriter
                };

                validPlays.Add(play);
                sb.AppendLine(string.Format(SuccessfulImportPlay, play.Title, play.Genre, play.Rating));
            }

            context.Plays.AddRange(validPlays);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCasts(TheatreContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute root = new XmlRootAttribute("Casts");
            XmlSerializer xmlSerializer
                = new XmlSerializer(typeof(ImportCastDto[]), root);

            using StringReader reader = new StringReader(xmlString);
            ImportCastDto[] castDtos = (ImportCastDto[])xmlSerializer.Deserialize(reader);

            ICollection<Cast> validCasts = new List<Cast>();
            foreach (var cDto in castDtos)
            {
                if (!IsValid(cDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Cast cast = new Cast()
                {
                    FullName = cDto.FullName,
                    IsMainCharacter = cDto.IsMainCharacter,
                    PhoneNumber = cDto.PhoneNumber,
                    PlayId = cDto.PlayId,
                };
                string character = cast.IsMainCharacter ? "main" : "lesser";

                validCasts.Add(cast);
                sb.AppendLine(string.Format(SuccessfulImportActor, cast.FullName, character));
            }

            context.Casts.AddRange(validCasts);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportTtheatersTickets(TheatreContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportTheatreWithTicketsDto[] theatreDtos = JsonConvert
                .DeserializeObject<ImportTheatreWithTicketsDto[]>(jsonString);

            ICollection<Theatre> validTheatres = new List<Theatre>();

            foreach (var theatreDto in theatreDtos)
            {
                if (!IsValid(theatreDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Theatre theatre = new Theatre()
                {
                    Name = theatreDto.Name,
                    NumberOfHalls = theatreDto.NumberOfHalls,
                    Director = theatreDto.Director,
                };

                foreach (var ticketDto in theatreDto.Tickets)
                {

                    if (!IsValid(ticketDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
                    Ticket ticket = new Ticket()
                    {
                        Price = ticketDto.Price,
                        RowNumber = ticketDto.RowNumber,
                        PlayId = ticketDto.PlayId,
                    };

                    theatre.Tickets.Add(ticket);
                }

                

                validTheatres.Add(theatre);
                sb.AppendLine(string.Format(SuccessfulImportTheatre, theatre.Name, theatre.Tickets.Count));
            }

            context.Theatres.AddRange(validTheatres);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}

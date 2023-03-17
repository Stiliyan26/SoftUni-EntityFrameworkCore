namespace Theatre.DataProcessor
{
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.Data.Models;
    using Theatre.Data.Models.Enums;
    using Theatre.DataProcessor.ExportDto;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {
            var theaters = context
                .Theatres
                .Where(t => t.NumberOfHalls >= numbersOfHalls && t.Tickets.Count > 19)
                .Select(theat => new
                {
                    Name = theat.Name,
                    Halls = theat.NumberOfHalls,
                    TotalIncome = theat
                        .Tickets
                        .Where(ticket => 1 <= ticket.RowNumber && ticket.RowNumber <= 5)
                        .Sum(ticket => ticket.Price),
                    Tickets = theat
                        .Tickets
                        .Where(t => 1 <= t.RowNumber && t.RowNumber <= 5)
                        .Select(t => new
                            {
                                Price = t.Price,
                                RowNumber = t.RowNumber,
                            })
                        .OrderByDescending(t => t.Price)
                        .ToArray(),
                })
                .OrderByDescending(t => t.Halls)
                .ThenBy(t => t.Name)
                .ToArray();

            string json = JsonConvert.SerializeObject(theaters, Formatting.Indented);
            return json;
        }

        public static string ExportPlays(TheatreContext context, double raiting)
        {
            ExportPlayDto[] playDtos = context.Plays
                .Include(p => p.Casts)
                .ToList()
                .Where(p => p.Rating <= raiting)
                .Select(p => new ExportPlayDto()
                {
                    Title = p.Title,
                    Duration = p.Duration.ToString("c"),
                    Rating = p.Rating == 0f ? "Premier" : p.Rating.ToString(CultureInfo.InvariantCulture),
                    Genre = Enum.GetName(typeof(Genre), p.Genre),
                    Actors = p.Casts
                        .Where(c => c.IsMainCharacter)
                        .Select(c => new ExportActorDto()
                        {
                            FullName = c.FullName,
                            MainCharacter = $"Plays main character in '{p.Title}'."
                        })
                        .OrderByDescending(p => p.FullName)
                        .ToArray()
                })
                .OrderBy(p => p.Title)
                .ThenByDescending(p => p.Genre)
                .ToArray();
                

            StringBuilder sb = new StringBuilder();
            using StringWriter writer = new StringWriter(sb);

            XmlRootAttribute root = new XmlRootAttribute("Plays");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportPlayDto[]), root);

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            xmlSerializer.Serialize(writer, playDtos, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}

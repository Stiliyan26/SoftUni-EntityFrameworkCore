namespace VaporStore.DataProcessor
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.ExportDto;

    public static class Serializer
    {
        public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
        {
            var gamesByGenre = context
                .Genres
                .Where(ge => genreNames.Contains(ge.Name))
                .Select(ge => new
                {
                    Id = ge.Id,
                    Genre = ge.Name,
                    Games = ge.Games
                    .Where(ga => ga.Purchases.Any())
                        .Select(ga => new
                        {
                            Id = ga.Id,
                            Title = ga.Name,
                            Developer = ga.Developer.Name,
                            Tags = string.Join(", ", ga.GameTags.Select(gt => gt.Tag.Name).ToArray()),
                            Players = ga.Purchases.Count()
                        })
                        .ToArray(),
                    TotalPlayers = ge.Games.Sum(game => game.Purchases.Count)
                })
                .ToArray();


            string jsonString = JsonConvert.SerializeObject(gamesByGenre, Formatting.Indented);

            return jsonString;
        }

        public static string ExportUserPurchasesByType(VaporStoreDbContext context, string purchaseType)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportUserWithPurchaseDto[]), new XmlRootAttribute("Users"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter stringWriter = new StringWriter(sb);

            PurchaseType purchaseTypeEnum = Enum.Parse<PurchaseType>(purchaseType);

            var users = context
                .Users
                .ToArray()
                .Where(u => u.Cards.Any(c => c.Purchases.Any()))
                .Select(u => new ExportUserWithPurchaseDto()
                {
                    Username = u.Username,
                    Purchases = context
                        .Purchases
                        .ToArray()
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum)
                        .OrderBy(p => p.Date)
                        .Select(p => new ExportPurchaseDto()
                        {
                            Card = p.Card.Number,
                            Cvc = p.Card.Cvc,
                            Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                            Game = new ExportGameDto()
                            {
                                Title = p.Game.Name,
                                Genre = p.Game.Genre.Name,
                                Price = p.Game.Price
                            }
                        })
                        .ToArray(),
                    TotalSpent = context
                        .Purchases
                        .ToArray()
                        .Where(p => p.Card.User.Username == u.Username && p.Type == purchaseTypeEnum)
                        .Sum(p => p.Game.Price)
                })
                .Where(u => u.Purchases.Length > 0)
                .OrderByDescending(u => u.TotalSpent)
                .ThenBy(u => u.Username)
                .ToArray();

            xmlSerializer.Serialize(stringWriter, users, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}

/*
 Card = p.Card.Number,
                                Cvc = c.Cvc,
                                Date = p.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                                Game = new ExportGameDto()
                                {
                                    Title = p.Game.Name,
                                    Genre = p.Game.Genre.Name,
                                    Price = p.Game.Price
                                }
 */
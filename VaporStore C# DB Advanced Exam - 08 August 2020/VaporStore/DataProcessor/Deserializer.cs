namespace VaporStore.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.Data.Models.Enums;
    using VaporStore.DataProcessor.ImportDto;

    public static class Deserializer
    {
        public const string ErrorMessage = "Invalid Data";

        public const string SuccessfullyImportedGame = "Added {0} ({1}) with {2} tags";

        public const string SuccessfullyImportedUser = "Imported {0} with {1} cards";

        public const string SuccessfullyImportedPurchase = "Imported {0} for {1}";

        public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportGameDto[] gameDtos = JsonConvert
                .DeserializeObject<ImportGameDto[]>(jsonString);

            List<Game> validGames = new List<Game>();
            List<Developer> newDevelopers = new List<Developer>();
            List<Genre> newGenres = new List<Genre>();
            List<Tag> newTags = new List<Tag>();

            foreach (ImportGameDto gameDto in gameDtos)
            {
                if (!IsValid(gameDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isReleaseDateValid = DateTime
                    .TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime validReleaseDate);

                if (!isReleaseDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Game newGame = new Game()
                {
                    Name = gameDto.Name,
                    Price = gameDto.Price,
                    ReleaseDate = validReleaseDate
                };

                Developer developer = newDevelopers
                    .FirstOrDefault(d => d.Name == gameDto.Developer);

                if (developer == null)
                {
                    Developer newDeveloper = new Developer()
                    {
                        Name = gameDto.Developer,
                    };

                    newDevelopers.Add(newDeveloper);
                    developer = newDeveloper;
                }

                newGame.Developer = developer;

                Genre genre = newGenres
                    .FirstOrDefault(g => g.Name == gameDto.Genre);

                if (genre == null)
                {
                    Genre newGenre = new Genre()
                    {
                        Name = gameDto.Genre
                    };

                    newGenres.Add(newGenre);
                    genre = newGenre;
                }

                newGame.Genre = genre;

                List<GameTag> newGameTags = new List<GameTag>();

                foreach (var tagNameDto in gameDto.Tags)
                {
                    if (string.IsNullOrEmpty(tagNameDto))
                    {
                        continue;
                    }

                    Tag tag = newTags
                        .FirstOrDefault(t => t.Name == tagNameDto);

                    if (tag == null)
                    {
                        Tag newTag = new Tag()
                        {
                            Name = tagNameDto,
                        };
                        newTags.Add(newTag);

                        tag = newTag;
                    }

                    GameTag newGameTag = new GameTag()
                    {
                        Tag = tag,
                        Game = newGame
                    };

                    newGameTags.Add(newGameTag);
                }

                if (newGameTags.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                newGame.GameTags = newGameTags;
                validGames.Add(newGame);
                sb.AppendLine(string.Format(SuccessfullyImportedGame, newGame.Name, newGame.Genre.Name, newGame.GameTags.Count));
            }

            context.Games.AddRange(validGames);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportUserWithCardsDto[] userDtos = JsonConvert
                .DeserializeObject<ImportUserWithCardsDto[]>(jsonString);

            List<User> importUsers = new List<User>();

            foreach (ImportUserWithCardsDto userDto in userDtos)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                User newUser = new User()
                {
                    FullName = userDto.FullName,
                    Username = userDto.Username,
                    Email = userDto.Email,
                    Age = userDto.Age,
                };

                List<Card> cards = new List<Card>();

                foreach (ImportCardDto cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isTypeValid = Enum
                        .TryParse(typeof(CardType), cardDto.Type, out object validCardType);

                    if (!isTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Card newCard = new Card()
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.CVC,
                        Type = (CardType)validCardType
                    };

                    cards.Add(newCard);
                }

                newUser.Cards = cards;
                importUsers.Add(newUser);

                sb.AppendLine(string.Format(SuccessfullyImportedUser, newUser.Username, newUser.Cards.Count));
            }

            context.Users.AddRange(importUsers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("Purchases");
            XmlSerializer xmlSerializer =
                new XmlSerializer(typeof(ImportPurchaseDto[]), xmlRoot);

            StringReader reader = new StringReader(xmlString);

            ImportPurchaseDto[] purchaseDtos = (ImportPurchaseDto[])
                xmlSerializer.Deserialize(reader);

            List<Purchase> validPurchases = new List<Purchase>();

            foreach (ImportPurchaseDto pDto in purchaseDtos)
            {
                if (!IsValid(pDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isTypeValid = Enum
                    .TryParse(typeof(PurchaseType), pDto.Type, out object validType);

                if (!isTypeValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isDateValid = DateTime.
                    TryParseExact(pDto.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime validDate);

                if (!isDateValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Card card = context
                    .Cards
                    .Include(c => c.User)
                    .FirstOrDefault(c => c.Number == pDto.Card);

                if (card == null)
                {
                    sb.AppendLine(ErrorMessage);    
                    continue;
                }

                Game game = context
                    .Games
                    .FirstOrDefault(g => g.Name == pDto.title);

                Purchase newPurchase = new Purchase()
                {
                    Type = (PurchaseType)validType,
                    ProductKey = pDto.Key,
                    Card = card,
                    Date = validDate,
                    Game = game
                };

                validPurchases.Add(newPurchase);

                User wantedUser = new User();


                sb.AppendLine(string.Format(SuccessfullyImportedPurchase, pDto.title, card.User.Username));
            }

            context.Purchases.AddRange(validPurchases);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
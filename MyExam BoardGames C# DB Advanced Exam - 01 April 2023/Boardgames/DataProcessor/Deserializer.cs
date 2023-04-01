namespace Boardgames.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;
    using Boardgames.Data;
    using Boardgames.Data.Models;
    using Boardgames.Data.Models.Enums;
    using Boardgames.DataProcessor.ImportDto;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCreator
            = "Successfully imported creator – {0} {1} with {2} boardgames.";

        private const string SuccessfullyImportedSeller
            = "Successfully imported seller - {0} with {1} boardgames.";

        public static string ImportCreators(BoardgamesContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportCreatorDto[] creatorDtos = DeserializeXml<ImportCreatorDto[]>("Creators", xmlString);

            ICollection<Creator> validCreators = new List<Creator>();

            foreach (ImportCreatorDto cDto in creatorDtos)
            {
                if (!IsValid(cDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Creator newCreator = new Creator()
                {
                    FirstName = cDto.FirstName,
                    LastName = cDto.LastName
                };

                ICollection<Boardgame> validBoardGames = new List<Boardgame>();

                foreach (ImportBoardGameByCreatorDto bDto in cDto.Boardgames)
                {
                    if (!IsValid(bDto) || string.IsNullOrEmpty(bDto.Name))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isCategoryTypeValid = Enum
                        .TryParse(typeof(CategoryType), bDto.CategoryType, out object validCategoryType);

                    if (!isCategoryTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Boardgame newBoardGame = new Boardgame()
                    {
                        Name = bDto.Name,
                        Rating = bDto.Rating,
                        YearPublished = bDto.YearPublished,
                        CategoryType = (CategoryType)validCategoryType,
                        Mechanics = bDto.Mechanics
                    };

                    validBoardGames.Add(newBoardGame);
                }

                newCreator.Boardgames = validBoardGames;
                validCreators.Add(newCreator);
                sb.AppendLine(string.Format(SuccessfullyImportedCreator,
                    newCreator.FirstName, newCreator.LastName, newCreator.Boardgames.Count));
            }

            context.Creators.AddRange(validCreators);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportSellers(BoardgamesContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportSellerDto[] sellerDtos = JsonConvert.DeserializeObject<ImportSellerDto[]>(jsonString);

            ICollection<Seller> validSellers = new List<Seller>();

            foreach (ImportSellerDto sDto in sellerDtos)
            {
                if (!IsValid(sDto) || string.IsNullOrEmpty(sDto.Country))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                int[] uniqueBoardgameids = sDto
                    .Boardgames
                    .Distinct()
                    .ToArray();

                ICollection<BoardgameSeller> validBoardgameSellers = new List<BoardgameSeller>();
                foreach (int bgId in uniqueBoardgameids)
                {
                    Boardgame existingBoardGame = context
                        .Boardgames
                        .FirstOrDefault(b => b.Id == bgId);

                    if (existingBoardGame == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    BoardgameSeller newBoardGameSeller = new BoardgameSeller()
                    {
                        Boardgame = existingBoardGame
                    };

                    validBoardgameSellers.Add(newBoardGameSeller);
                }

                Seller validSeller = new Seller()
                {
                    Name = sDto.Name,
                    Address = sDto.Address,
                    Country = sDto.Country,
                    Website = sDto.Website,
                    BoardgamesSellers = validBoardgameSellers
                };

                validSellers.Add(validSeller);
                sb.AppendLine(string.Format(SuccessfullyImportedSeller, validSeller.Name, validSeller.BoardgamesSellers.Count));
            }

            context.Sellers.AddRange(validSellers);
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

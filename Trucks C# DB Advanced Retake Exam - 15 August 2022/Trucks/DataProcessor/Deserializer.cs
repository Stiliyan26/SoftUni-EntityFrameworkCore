namespace Trucks.DataProcessor
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlTypes;
    using System.Text;
    using System.Text.Json.Nodes;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportDespatcherDto[] despatcherDtos = DeserializeXml<ImportDespatcherDto[]>("Despatchers", xmlString);

            ICollection<Despatcher> validDespatchers = new List<Despatcher>();

            foreach (ImportDespatcherDto dDto in despatcherDtos)
            {
                if (!IsValid(dDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (string.IsNullOrEmpty(dDto.Position))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Despatcher newDespathcer = new Despatcher()
                {
                    Name = dDto.Name,
                    Position = dDto.Position
                };

                ICollection<Truck> validTrucks = new List<Truck>();

                foreach (ImportTruckDto truckDto in dDto.Trucks)
                {
                    if (!IsValid(truckDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isCategoryTypeValid = Enum
                        .TryParse(typeof(CategoryType), truckDto.CategoryType, out object validCategoryType);

                    if (!isCategoryTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isMakeTypeValid = Enum
                        .TryParse(typeof(MakeType), truckDto.MakeType, out object validMakeType);

                    if (!isMakeTypeValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Truck newTruck = new Truck()
                    {
                        RegistrationNumber = truckDto.RegistrationNumber,
                        VinNumber = truckDto.VinNumber,
                        TankCapacity = truckDto.TankCapacity,
                        CargoCapacity = truckDto.CargoCapacity,
                        CategoryType = (CategoryType)validCategoryType,
                        MakeType = (MakeType)validMakeType,
                    };

                    validTrucks.Add(newTruck);
                }

                newDespathcer.Trucks = validTrucks;

                validDespatchers.Add(newDespathcer);
                sb.AppendLine(string.Format(SuccessfullyImportedDespatcher, newDespathcer.Name, newDespathcer.Trucks.Count));
            }

            context.Despatchers.AddRange(validDespatchers);
            context.SaveChanges();  
            return sb.ToString().TrimEnd();
        }
        public static string ImportClient(TrucksContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportClientDto[] clinetDtos = JsonConvert
                .DeserializeObject<ImportClientDto[]>(jsonString);

            ICollection<Client> validClinets = new List<Client>();

            foreach (ImportClientDto cDto in clinetDtos)
            {
                if (!IsValid(cDto) || cDto.Type == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Client newClinet = new Client()
                {
                    Name = cDto.Name,
                    Nationality = cDto.Nationality,
                    Type = cDto.Type
                };

                ICollection<int> uniqueTruckIds = cDto.Trucks
                    .Distinct()
                    .ToList();

                ICollection<ClientTruck> validClientsTrucks = new List<ClientTruck>();

                foreach (int tId in uniqueTruckIds)
                {
                    Truck newTruck = context
                        .Trucks
                        .FirstOrDefault(t => t.Id == tId);

                    if (newTruck == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    ClientTruck newClientTruck = new ClientTruck()
                    {
                        Truck = newTruck,
                    };

                    validClientsTrucks.Add(newClientTruck);
                }

                newClinet.ClientsTrucks = validClientsTrucks;
                validClinets.Add(newClinet);
                sb.AppendLine(string.Format(SuccessfullyImportedClient, newClinet.Name, newClinet.ClientsTrucks.Count));
            }

            context.AddRange(validClinets);
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
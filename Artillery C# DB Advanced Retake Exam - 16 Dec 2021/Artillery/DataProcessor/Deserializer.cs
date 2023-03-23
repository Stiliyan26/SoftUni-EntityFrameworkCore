namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        private const string ErrorMessage =
            "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportCountryDto[] countrieDtos = DeserializeXml<ImportCountryDto[]>("Countries", xmlString);

            ICollection<Country> validCountries = new List<Country>();

            foreach (var countryDto in countrieDtos)
            {
                if (!IsValid(countryDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Country country = new Country()
                {
                    CountryName = countryDto.CountryName,
                    ArmySize = countryDto.ArmySize,
                };

                validCountries.Add(country);
                sb.AppendLine(string.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));
            }

            context.Countries.AddRange(validCountries);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportManufacturerDto[] manufacturerDtos = DeserializeXml<ImportManufacturerDto[]>("Manufacturers", xmlString);

            ICollection<Manufacturer> validManufacturers = new List<Manufacturer>();
            ICollection<string> manufacturersNames = new List<string>();
            foreach (ImportManufacturerDto mDto in manufacturerDtos)
            {
                if (!IsValid(mDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (manufacturersNames.Contains(mDto.ManufacturerName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                manufacturersNames.Add(mDto.ManufacturerName);

                Manufacturer manufacturer = new Manufacturer()
                {
                    ManufacturerName = mDto.ManufacturerName,
                    Founded = mDto.Founded
                };

                string[] foundedInfo = manufacturer
                    .Founded
                    .Split(", ", StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();

                string townName = foundedInfo[foundedInfo.Length - 2];
                string countryName = foundedInfo[foundedInfo.Length - 1];

                string combinedInfo = $"{townName}, {countryName}";
                    
                validManufacturers.Add(manufacturer);
                sb.AppendLine(string.Format(SuccessfulImportManufacturer, manufacturer.ManufacturerName, combinedInfo));
            }

            context.Manufacturers.AddRange(validManufacturers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            ImportShellDto[] shellDtos = DeserializeXml<ImportShellDto[]>("Shells", xmlString);

            ICollection<Shell> validShells = new List<Shell>();

            foreach (ImportShellDto shellDto in shellDtos)
            {
                if (!IsValid(shellDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Shell newShell = new Shell()
                {
                    ShellWeight = shellDto.ShellWeight,
                    Caliber = shellDto.Caliber,
                };

                validShells.Add(newShell);
                sb.AppendLine(string.Format(SuccessfulImportShell, newShell.Caliber, newShell.ShellWeight));
            }

            context.Shells.AddRange(validShells);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportGunDto[] gunDtos = JsonConvert
                .DeserializeObject<ImportGunDto[]>(jsonString);

            ICollection<Gun> validGuns = new List<Gun>();

            foreach (ImportGunDto gunDto in gunDtos)
            {
                if (!IsValid(gunDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                bool isGunTypeValid = Enum
                    .TryParse(typeof(GunType), gunDto.GunType, out object validGunType);

                if (!isGunTypeValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                ICollection<CountryGun> newCountryGuns = new List<CountryGun>();

                foreach (ImportCountryGunsDto cgDto in gunDto.Countries)
                {
                    CountryGun newCountryGun = new CountryGun()
                    {
                        CountryId = cgDto.Id,
                    };

                    newCountryGuns.Add(newCountryGun);
                }

                Gun newGun = new Gun()
                {
                    ManufacturerId = gunDto.ManufacturerId,
                    GunWeight = gunDto.GunWeight,
                    BarrelLength = gunDto.BarrelLength,
                    NumberBuild = gunDto.NumberBuild,
                    Range = gunDto.Range,
                    GunType = (GunType)validGunType,
                    ShellId = gunDto.ShellId,
                    CountriesGuns = newCountryGuns
                };

                sb.AppendLine(string.Format(SuccessfulImportGun, newGun.GunType, newGun.GunWeight, newGun.BarrelLength));
                validGuns.Add(newGun);
            }

            context.Guns.AddRange(validGuns);
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
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
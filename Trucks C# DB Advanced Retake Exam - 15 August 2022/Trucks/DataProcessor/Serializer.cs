namespace Trucks.DataProcessor
{
    using Data;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System.Text;
    using System.Xml.Serialization;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ExportDto;

    public class Serializer
    {
        public static string ExportDespatchersWithTheirTrucks(TrucksContext context)
        {
            ExportDespatcherDto[] despathcers = context
                .Despatchers
                .Include(d => d.Trucks)
                .Where(d => d.Trucks.Any())
                .ToArray()
                .OrderByDescending(d => d.Trucks.Count())
                .ThenBy(t => t.Name)
                .Select(d => new ExportDespatcherDto()
                {
                    TrucksCount = d.Trucks.Count().ToString(),
                    DespatcherName = d.Name,
                    Trucks = d.Trucks
                        .Select(t => new ExportTruckDto()
                        {
                            RegistrationNumber = t.RegistrationNumber,
                            Make = t.MakeType.ToString()
                        })
                        .OrderBy(t => t.RegistrationNumber)
                        .ToArray()
                })
                .ToArray();

            string xmlStr = SerializeXml<ExportDespatcherDto[]>("Despatchers", despathcers);
            return xmlStr;
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            object despatchers = context
               .Clients
               .Include(d => d.ClientsTrucks)
               .Where(d => d.ClientsTrucks.Any(t => t.Truck.TankCapacity >= capacity))
               .ToArray()
               .Select(d => new
               {
                   Name = d.Name,
                   Trucks = d.ClientsTrucks
                        .Where(t => t.Truck.TankCapacity >= capacity)
                        .Select(t => new
                        {
                            TruckRegistrationNumber = t.Truck.RegistrationNumber,
                            VinNumber = t.Truck.VinNumber,
                            TankCapacity = t.Truck.TankCapacity,
                            CargoCapacity = t.Truck.CargoCapacity,
                            CategoryType = t.Truck.CategoryType.ToString(),
                            MakeType = t.Truck.MakeType.ToString()
                        })
                        .OrderBy(t => t.MakeType)
                        .ThenByDescending(t => t.CargoCapacity)
                        .ToArray()
               })
               .OrderByDescending(d => d.Trucks.Length)
               .ThenBy(d => d.Name)
               .Take(10)
               .ToArray();

            string jsonString = JsonConvert.SerializeObject(despatchers, Formatting.Indented);

            return jsonString;
        }

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
    }
}

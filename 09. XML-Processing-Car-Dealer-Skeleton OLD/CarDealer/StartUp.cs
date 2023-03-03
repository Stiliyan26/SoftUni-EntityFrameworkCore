using CarDealer.Data;
using System.IO;
using System;
using System.Xml.Serialization;
using CarDealer.Dtos.Import;
using CarDealer.Models;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using System.Globalization;
using CarDealer.Dtos.Export;
using System.Text;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            CarDealerContext dbContext= new CarDealerContext();
            /*dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();*/

            string xml = File.ReadAllText("../../../Datasets/sales.xml");

           /* string result = ImportSuppliers(dbContext, xml);
            Console.WriteLine(result);*/

        /*    string result2 = ImportParts(dbContext, xml);
            Console.WriteLine(result2);

            string result3 = ImportCars(dbContext, xml);
            Console.WriteLine(result3);

            string result4 = ImportCustomers(dbContext, xml);
            Console.WriteLine(result4);

            string result5 = ImportSales(dbContext, xml);
            Console.WriteLine(result5);*/

            /*string result6 = GetCarsWithDistance(dbContext);
            Console.WriteLine(result6);*/

            /*string result7 = GetCarsFromMakeBmw(dbContext);
            Console.WriteLine(result7);*/

            /*string result8 = GetLocalSuppliers(dbContext);
            Console.WriteLine(result8);*/

            /*string result9 = GetCarsWithTheirListOfParts(dbContext);
            Console.WriteLine(result9);*/

            /*string result10 = GetTotalSalesByCustomer(dbContext);
            Console.WriteLine(result10);*/

            /*string result11 = GetSalesWithAppliedDiscount(dbContext);
            Console.WriteLine(result11);*/
        }

        //Problem 09
        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Suppliers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSupplierDto[]), xmlRoot);

            using StringReader reader= new StringReader(inputXml);

            ImportSupplierDto[] supplierDtos = (ImportSupplierDto[])xmlSerializer
                    .Deserialize(reader);

            Supplier[] suppliers = supplierDtos
                .Select(dto => new Supplier
                {
                    Name = dto.Name,
                    IsImporter = dto.IsImporter,
                })
                .ToArray();

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Length}";
        }

        //Problem 10
        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Parts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportPartDto[]), xmlRoot);

            using StringReader reader = new StringReader(inputXml);
            ImportPartDto[] partDtos = (ImportPartDto[])xmlSerializer
                .Deserialize(reader);

            ICollection<Part> parts = new List<Part>();
            foreach (ImportPartDto pDto in partDtos)
            {
                if (!context.Suppliers.Any(s => s.Id == pDto.SupplierId))
                {
                    continue;
                }

                Part part = new Part()
                {
                    Name = pDto.Name,   
                    Price = pDto.Price,
                    Quantity = pDto.Quantity,
                    SupplierId = pDto.SupplierId,
                };

                parts.Add(part);
            }

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        //Problem 11
        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Cars");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCarDto[]), xmlRoot);

            using StringReader reader = new StringReader(inputXml);
            ImportCarDto[] carDtos = (ImportCarDto[])xmlSerializer
                .Deserialize(reader);

            ICollection<Car> cars = new List<Car>();
            foreach (ImportCarDto cDto in carDtos)
            {
                Car car = new Car()
                {
                    Make = cDto.Make,
                    Model = cDto.Model,
                    TravelledDistance = cDto.TraveledDistance,
                };

                foreach (int partId in cDto.Parts.Select(p => p.Id).Distinct())
                {
                    if (!context.Parts.Any(p => p.Id == partId))
                    {
                        continue;
                    }

                    PartCar partCar = new PartCar()
                    {
                        PartId = partId
                    };

                    car.PartCars.Add(partCar);
                }   

                cars.Add(car);
            }
            context.Cars.AddRange(cars);
            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        //Problem 12
        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            string rootName = "Customers";
            ImportCustomerDto[] customerDtos = Deserialize<ImportCustomerDto[]>(inputXml, rootName);

            Customer[] customers = customerDtos
                .Select(dto => new Customer()
                {
                    Name = dto.Name,
                    BirthDate = DateTime.Parse(dto.BirthDate, CultureInfo.InvariantCulture),
                    IsYoungDriver = dto.isYoungDriver
                })
                .ToArray();

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Length}";
        }

        //Problem 13
        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            string rootName = "Sales";
            ImportSaleDto[] saleDtos = Deserialize<ImportSaleDto[]>(inputXml, rootName);

            ICollection<Sale> sales = new List<Sale>();
            foreach (ImportSaleDto sDto in saleDtos)
            {
                bool doesCarExist = context
                    .Cars
                    .Any(c => sDto.CarId == c.Id);  

                if (!doesCarExist)
                {
                    continue;
                }

                Sale sale = new Sale()
                {
                    CarId = sDto.CarId,
                    CustomerId = sDto.CustomerId,
                    Discount = sDto.Discount,
                };

                sales.Add(sale);
            }

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        //Problem 14
        public static string GetCarsWithDistance(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportCarsWithDistanceDto[] carDtos = context
                .Cars
                .Where(c => c.TravelledDistance > 2000000)
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .Take(10)
                .Select(c => new ExportCarsWithDistanceDto
                {
                    Make = c.Make,
                    Model = c.Model,
                    TravelDistance = c.TravelledDistance
                })
                .ToArray();

            XmlRootAttribute xmlRoot = new XmlRootAttribute("cars");
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsWithDistanceDto[]), xmlRoot);

            using StringWriter writer = new StringWriter(sb); 
            xmlSerializer.Serialize(writer, carDtos, namespaces);

            return sb
                .ToString() 
                .TrimEnd();
        }

        //Problem 15    
        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportBMWCarsDto[] bmwDtos = context
                .Cars
                .Where(c => c.Make == "BMW")
                .OrderBy(c => c.Model)
                .ThenByDescending(c => c.TravelledDistance)
                .Select(c => new ExportBMWCarsDto()
                {
                    Id = c.Id,
                    Model = c.Model,
                    TraveledDistance = c.TravelledDistance
                })
                .ToArray();

            XmlRootAttribute xmlRootAttribute = new XmlRootAttribute("cars"); 
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportBMWCarsDto[]), xmlRootAttribute);

            using StringWriter writer = new StringWriter(sb);
            xmlSerializer.Serialize(writer, bmwDtos, namespaces);

            return sb
                .ToString() 
                .TrimEnd();
        }

        //Problem 16
        public static string GetLocalSuppliers(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportLocalSuppliersDto[] suppliersDtos = context
                .Suppliers
                .Where(s => !s.IsImporter)
                .Select(s => new ExportLocalSuppliersDto()
                {
                    Id = s.Id,
                    Name = s.Name,
                    PartsCount = s.Parts.Count
                })
                .ToArray();

            XmlRootAttribute rootAttr = new XmlRootAttribute("suppliers");
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(); 
            namespaces.Add(string.Empty, string.Empty);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportLocalSuppliersDto[]), rootAttr);
            StringWriter stringWriter= new StringWriter(sb);
            xmlSerializer.Serialize(stringWriter, suppliersDtos, namespaces);

            return sb
                .ToString() 
                .TrimEnd();
        }

        //Problem 17
        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportCarsWithPartsDto[] carDtos = context
                .Cars
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Take(5)
                .Select(c => new ExportCarsWithPartsDto()
                {
                    Make = c.Make,
                    Model = c.Model,
                    TraveledDistance = c.TravelledDistance,
                    Parts = c.PartCars
                        .OrderByDescending(pc => pc.Part.Price)
                        .Select(pc => new ExportPartsByCarDto()
                        {
                            Name = pc.Part.Name,
                            Price = pc.Part.Price
                        })
                        .ToArray()
                })
                .ToArray();

            string rootName = "cars";

            string output = Serizalize(rootName, carDtos);

            return output;
        }

        //Problem 18
        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportCustomerWithCarsDto[] customerDtos = context
                .Customers
                .Where(c => 1 <= c.Sales.Count)
                .Select(c => new ExportCustomerWithCarsDto()
                {
                    FullName = c.Name,
                    BoughtCars = c.Sales.Count,
                    SpentMoney = c
                    .Sales
                    .SelectMany(s => s
                        .Car
                        .PartCars
                        .Select(pc => pc.Part.Price))
                    .Sum()
                })
                .OrderByDescending(c => c.SpentMoney)
                .ToArray();

            string rootName = "customers";
            string result = Serizalize(rootName, customerDtos);

            return result;
        }

        //Problem 19 
        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            StringBuilder sb = new StringBuilder();

            ExportSalesAllInfoDto[] saleDtos = context
                .Sales
                .Select(s => new ExportSalesAllInfoDto()
                {
                    Car = new ExportCarWithAttributesDto()
                    {
                        Make = s.Car.Make,  
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance,
                    },
                    Discount = s.Discount,
                    CustomerName = s.Customer.Name,
                    Price = s
                        .Car
                        .PartCars
                        .Sum(pc => pc.Part.Price),
                    PriceWithDiscount = s.Car.PartCars.Sum(pc => pc.Part.Price) - 
                        (s.Car.PartCars.Sum(pc => pc.Part.Price) * (s.Discount / 100))
                })
                .ToArray();

            string rootName = "sales";
            string output = Serizalize(rootName, saleDtos);

            return output;
        }

        //Helper method
        private static T Deserialize<T>(string inputXml, string rootName)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringReader reader = new StringReader(inputXml);

            T dtos = (T)xmlSerializer
                    .Deserialize(reader);

            return dtos;
        }

        //Helper method
        private static string Serizalize<T>(string rootName, T dto)
        {
            StringBuilder sb = new StringBuilder();

            XmlRootAttribute xmlRoot = new XmlRootAttribute(rootName);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), xmlRoot);

            using StringWriter writer = new StringWriter(sb);
            xmlSerializer.Serialize(writer, dto, namespaces);

            return sb
                .ToString()
                .TrimEnd();
        }
    }
}
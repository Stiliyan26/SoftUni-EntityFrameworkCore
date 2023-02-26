using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

using ProductShop.Data;
using ProductShop.DTOs.User;
using ProductShop.Models;

using AutoMapper;
using Newtonsoft.Json;
using ProductShop.DTOs.Product;
using ProductShop.DTOs.Category;
using ProductShop.DTOs.CategoryProduct;
using System.Linq;
using AutoMapper.QueryableExtensions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Diagnostics;

namespace ProductShop
{
    public class StartUp
    {
        /*private static IMapper mapper;*/
        private static string filePath;

        public static void Main(string[] args)
        {

            /* mapper = new Mapper(new MapperConfiguration(cfg =>
             {
                 cfg.AddProfile<ProductShopProfile>();
             }));*/

            Mapper.Initialize(cfg => cfg.AddProfile(typeof(ProductShopProfile)));

            ProductShopContext dbContext = new ProductShopContext();

            InitializeDatasetFilePath("categories.json");

            //string inputJson = File.ReadAllText(filePath);

            /*dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();*/

            /* Console.WriteLine($"Database copy was created!");*/
            /*string output = ImportUsers(dbContext, inputJson);
            Console.WriteLine(output);*/

            /*string output2 = ImportProducts(dbContext, inputJson);
            Console.WriteLine(output2);*/

            /*string output3 = ImportCategories(dbContext, inputJson);
            Console.WriteLine(output3);*/

            /*string output4 = ImportCategoryProducts(dbContext, inputJson);
            Console.WriteLine(output4);*/

            InitializeOutputFilePath("users-and-products.json");

            /*string jsonResult = GetProductsInRange(dbContext);
            File.WriteAllText(filePath, jsonResult);*/

            /*string jsonResult2 = GetSoldProducts(dbContext);
            File.WriteAllText(filePath, jsonResult2);*/

            /*string result = GetCategoriesByProductsCount(dbContext);
            File.WriteAllText(filePath, result);*/

            string result08 = GetUsersWithProducts(dbContext);
            File.WriteAllText(filePath, result08); 
        }
        private static void InitializeDatasetFilePath(string fileName)
        {
            filePath =
                Path.Combine(Directory.GetCurrentDirectory(), "../../../Datasets/", fileName);
        }
        /// <summary>
        /// Executes all validation attributes in a class and returns true or false depending on
        /// validation result
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        private static void InitializeOutputFilePath(string fileName)
        {
            filePath =
                Path.Combine(Directory.GetCurrentDirectory(), "../../../Results/", fileName);
        }


        private static bool isValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult);
            return isValid;
        }

        //Problem 01
        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            ImportUserDto[] userDtos = JsonConvert.DeserializeObject<ImportUserDto[]>(inputJson);

            ICollection<User> users = new List<User>();

            foreach (ImportUserDto uDto in userDtos)
            {
                if (!isValid(uDto))
                {
                    continue;
                }

                User user = Mapper.Map<User>(uDto);
                users.Add(user);
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count}";
        }

        //Problem 02
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            ImportProductDto[] productDtos = JsonConvert
                .DeserializeObject<ImportProductDto[]>(inputJson);

            ICollection<Product> validProducts = new List<Product>();
            foreach (ImportProductDto pDto in productDtos)
            {
                if (!isValid(pDto))
                {
                    continue;
                }

                Product product = Mapper.Map<Product>(pDto);
                validProducts.Add(product);
            }

            context.Products.AddRange(validProducts);
            context.SaveChanges();

            return $"Successfully imported {validProducts.Count}";
        }

        //Problem 03
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            ImportCategoryDto[] categoryDtos = JsonConvert
                .DeserializeObject<ImportCategoryDto[]>(inputJson);

            ICollection<Category> validCategories = new List<Category>();
            foreach (ImportCategoryDto cDto in categoryDtos)
            {
                if (!isValid(cDto))
                {
                    continue;
                }

                Category category = Mapper.Map<Category>(cDto);
                validCategories.Add(category);
            }

            context.AddRange(validCategories);
            context.SaveChanges();
            return $"Successfully imported {validCategories.Count}";
        }

        //Problem 04
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            ImportCategoryProductDto[] categoryProdcutsDtos = JsonConvert
                .DeserializeObject<ImportCategoryProductDto[]>(inputJson);

            ICollection<CategoryProduct> validCategoryProduct = new List<CategoryProduct>();

            foreach (ImportCategoryProductDto cpDto in categoryProdcutsDtos)
            {
                if (!isValid(cpDto))
                {
                    continue;
                }

                CategoryProduct categoryProduct = Mapper.Map<CategoryProduct>(cpDto);
                validCategoryProduct.Add(categoryProduct);
            }

            context.AddRange(validCategoryProduct);
            context.SaveChanges();

            return $"Successfully imported {validCategoryProduct.Count}";
        }

        //Problem 05
        public static string GetProductsInRange(ProductShopContext context)
        {
            ExportProductsInRangeDto[] products = context
                .Products
                .Where(p => 500 <= p.Price && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .ProjectTo<ExportProductsInRangeDto>()
                .ToArray();

            string exportJson = JsonConvert
                .SerializeObject(products, Formatting.Indented);

            return exportJson;
        }

        //Problem 06
        public static string GetSoldProducts(ProductShopContext context)
        {
            //Way 1 - Manual mapping
            /*var users = context
                .Users
                .Include(u => u.ProductsSold)
                .ThenInclude(p => p.Buyer)
                .Where(u => u.ProductsSold.Any() && u.ProductsSold.Any(p => p.Buyer != null))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    soldProducts = u.ProductsSold
                        .Where(p => p.Buyer != null)
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price,
                            buyerFirstName = p.Buyer.FirstName,
                            buyerLastName = p.Buyer.LastName
                        })
                        .ToArray()
                })
                .ToArray();*/

            //Way 2 - Auto mapping
            ExportUser08[] users = context
                .Users
                .Where(u => u.ProductsSold.Any(p => p.BuyerId.HasValue))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ProjectTo<ExportUser08>()
                .ToArray();

            string exportJson = JsonConvert
                .SerializeObject(users, Formatting.Indented);

            return exportJson;
        }

        //Problem 07
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            //Way 1 Auto mapper
            ExportCategoryDto[] categories = context
                .Categories
                .OrderByDescending(c => c.CategoryProducts.Count())
                .ProjectTo<ExportCategoryDto>()
                .ToArray();

            //Way 2 Manual mapping
            /*var categories = context
                .Categories
                .OrderByDescending(c => c.CategoryProducts.Count)
                .Select(c => new
                {
                    category = c.Name,
                    productsCount = c.CategoryProducts.Count,
                    averagePrice = $"{(double)c.CategoryProducts.Average(cp => cp.Product.Price):F2}",
                    totalRevenue = $"{(double)c.CategoryProducts.Sum(cp => cp.Product.Price):F2}"
                })
                .ToArray();*/

            string exportJson = JsonConvert
                .SerializeObject(categories, Formatting.Indented);

            return exportJson;
        }

        //Problem 08
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var usersCountInf = context
                .Users
                .Where(u => u.ProductsSold.Any() && u.ProductsSold.Any(p => p.Buyer != null))
                .Count();

            var usersInfo = context
                .Users
                .Where(u => u.ProductsSold.Any() && u.ProductsSold.Any(p => p.Buyer != null))
                .OrderByDescending(u => u
                    .ProductsSold
                    .Where(p => p.Buyer != null)
                    .Count())
                .Select(u => new
                {
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold.Count,
                        products = u.ProductsSold
                        .Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                        .ToArray()
                    }
                })
                .ToArray();

            var userInfoObj = new
            {
                usersCount = usersCountInf,
                users = usersInfo
            };

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            string json = JsonConvert
                .SerializeObject(userInfoObj, settings);

            return json;
        }
    }
}
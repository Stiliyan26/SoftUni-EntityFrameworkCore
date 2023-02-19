namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var context = new BookShopContext();
            /*DbInitializer.ResetDatabase(context);*/

            /*string command = Console.ReadLine();
            Console.WriteLine(GetBooksByAgeRestriction(context, command));*/

            /*Console.WriteLine(GetGoldenBooks(context));*/

            /*int year = int.Parse(Console.ReadLine());
            Console.WriteLine(GetBooksNotReleasedIn(context, year));*/

            /*string endsWithLetter = Console.ReadLine();
            GetAuthorNamesEndingIn(context, endsWithLetter);*/

            /* Console.WriteLine(CountCopiesByAuthor(context));*/

            /*Console.WriteLine(GetTotalProfitByCategory(context));*/

            Console.WriteLine(GetMostRecentBooks(context));
        }
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            StringBuilder output = new StringBuilder();

            /*AgeRestriction ageRestEnum = 
                Enum.Parse<AgeRestriction>(command, true);*/

            AgeRestriction ageRestriction;
            bool parseSuccess =
                Enum.TryParse<AgeRestriction>(command, true, out ageRestriction);

            if (!parseSuccess)
            {
                return String.Empty;
            }

            string[] bookTitles = context
                .Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToArray();

            return string.Join(Environment.NewLine, bookTitles);
        }

        public static string GetGoldenBooks(BookShopContext context)
        {                                          //(int)b.EditionType == 2
            string[] bookTitles = context          //b.EditionType.ToString() == EditionType.Gold.ToString()
                .Books                                         //(EditionType)2
                .Where(b => b.Copies < 5000 && b.EditionType == EditionType.Gold)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, bookTitles);
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            string[] bookTitles = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return string.Join(Environment.NewLine, bookTitles);
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            string[] authorNames = context
                .Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a => $"{a.FirstName} {a.LastName}")
                .ToArray()
                .OrderBy(n => n)
                .ToArray();

            return string.Join(Environment.NewLine, authorNames);
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var bookCopiesByAuthor = context
                .Authors
                .Include(a => a.Books)
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    TotalCopies = a.Books.Sum(b => b.Copies)
                })
                .OrderByDescending(o => o.TotalCopies)
                .ToArray();

            foreach (var bookInfo in bookCopiesByAuthor)
            {
                output
                    .AppendLine($"{bookInfo.FirstName} {bookInfo.LastName} - {bookInfo.TotalCopies}");
            }

            return output
                .ToString()
                .TrimEnd();
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var porfitByCategory = context
                .Categories
                .Include(c => c.CategoryBooks)
                .ThenInclude(cb => cb.Book)
                .Select(c => new
                {
                    c.Name,
                    Profit = c.CategoryBooks
                        .Sum(cb => cb.Book.Copies * cb.Book.Price)
                })
                .OrderByDescending(c => c.Profit)
                .ThenBy(c => c.Name)
                .ToArray();

            foreach (var cp in porfitByCategory)
            {
                output
                    .AppendLine($"{cp.Name} ${cp.Profit:f2}");
            }

            return output
                .ToString()
                .TrimEnd();
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            StringBuilder output = new StringBuilder();

            var booksByCategory = context
                .Categories
                .Include(c => c.CategoryBooks)
                .ThenInclude(cb => cb.Book)
                .Select(c => new
                {
                    CategoryName = c.Name,
                    MostRecentBooks = c.CategoryBooks
                                            .OrderByDescending(c => c.Book.ReleaseDate.Value)
                                            .Select(cb => new
                                            {
                                                BookTitle = cb.Book.Title,
                                                ReleaseDate = cb.Book.ReleaseDate.Value.Year,
                                            })
                                            .Take(3)
                })
                .OrderBy(c => c.CategoryName)
                .ToArray();

            foreach (var bc in booksByCategory)
            {
                output
                    .AppendLine($"--{bc.CategoryName}");

                foreach (var book in bc.MostRecentBooks)
                {
                    output
                        .AppendLine($"{book.BookTitle} ({book.ReleaseDate})");
                }
            }

            return output
                    .ToString() 
                    .TrimEnd();    
        }

        public static void IncreasePrices(BookShopContext context)
        {
            IQueryable<Book> booksToUpdate = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year < 2010);

            foreach (Book book in booksToUpdate)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }
    }
}

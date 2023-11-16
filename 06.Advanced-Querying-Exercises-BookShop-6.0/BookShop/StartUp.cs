namespace BookShop
{
    using BookShop.Models;
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
    using System.Globalization;
    using System.Linq;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            DbInitializer.ResetDatabase(db);
            string input = Console.ReadLine();
            Console.WriteLine(GetBooksReleasedBefore(db, input));
        }

        // Ex. 2
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            if (!Enum.TryParse<AgeRestriction>(command, true, out var ageRestriction))
            {
                return $"{command} is not a valid age restriction";
            }

            var books = context.Books
                .Where(b => b.AgeRestriction == ageRestriction)
                .Select(b => new
                {
                    b.Title
                })
                .OrderBy(b => b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b => b.Title));
        }

        // Ex. 3
        public static string GetGoldenBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.EditionType == EditionType.Gold &&
                            b.Copies < 5000)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b=>b.Title));
        }
        // Ex. 4
        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Price > 40)
                .Select(b => new
                {
                    b.Title,
                    b.Price
                })
                .OrderByDescending(b => b.Price)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b=> $"{b.Title} - ${b.Price:f2}"));
        }
        // Ex. 5
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            var books = context.Books
                .Where(b => b.ReleaseDate.Value.Year != year).ToList()
                .OrderBy(b => b.BookId);
            return string.Join(Environment.NewLine, books.Select(b=>b.Title));
        }
        // Ex. 6
        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            string[] categories = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(c=>c.ToLower()).ToArray();

            var books = context.Books
                .Select(b => new
                {
                    b.Title,
                    b.BookCategories
                })
                .Where(b => b.BookCategories.Any(bc=>categories.Contains(bc.Category.Name.ToLower())))
                .OrderBy(b=>b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b=>b.Title));
        }
        // Ex. 7
        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var parsedDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var books = context.Books
                .Where(b => b.ReleaseDate < parsedDate)
                .Select(b => new
                {
                    b.Title,
                    b.EditionType,
                    b.Price,
                    b.ReleaseDate,
                })
                .OrderByDescending(b => b.ReleaseDate);
                
            return string.Join(Environment.NewLine, books.Select(b=> $"{b.Title} - {b.EditionType} - ${b.Price}"));
        }
        // Ex. 8
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var authors = context.Authors
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    authorNmae = a.FirstName + " " + a.LastName,
                })
                .Where(a => a.FirstName.EndsWith(input))
                .OrderBy(a => a.authorNmae)
                .ToList();

            return string.Join(Environment.NewLine, authors.Select(a => $"{a.FirstName} {a.LastName}"));
        }
        // Ex. 9
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var books = context.Books
                .Select(b => new
                {
                    lowerTitle = b.Title.ToLower(),
                    b.Title
                })
                .Where(b => b.lowerTitle.Contains(input.ToLower()))
                .OrderBy(b=>b.Title)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b => $"{b.Title}"));
        }
        // Ex. 10
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var books = context.Books
                .Select(b => new
                {
                    lowerLastName = b.Author.LastName.ToLower(),
                    b.BookId,
                    b.Title,
                    authorName = b.Author.FirstName + " " + b.Author.LastName
                })
                .Where(a => a.lowerLastName.StartsWith(input.ToLower()))
                .OrderBy(b=>b.BookId)
                .ToList();

            return string.Join(Environment.NewLine, books.Select(b => $"{b.Title} ({b.authorName})"));
        }
        // Ex. 11
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context.Books
                .Where(b => b.Title.Length > lengthCheck);
            
                return books.Count();
        }
        // Ex. 12
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var authors = context.Authors
                .Select(a => new
                {
                    fullName = a.FirstName + " " + a.LastName,
                    totalCopies = a.Books.Select(x=>x.Copies).Sum()
                })        
                .OrderByDescending(a=>a.totalCopies)
                .ToList();
            return string.Join(Environment.NewLine, authors.Select(a => $"{a.fullName} - {a.totalCopies}"));
        }
        // Ex. 13
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var Categories = context.Categories
                .Select(c => new
                {
                    Name = c.Name,
                    totalprofit = c.CategoryBooks.Sum(cb => cb.Book.Copies * cb.Book.Price)
                })
                .OrderByDescending(c=>c.totalprofit)
                .ToList();
            return string.Join(Environment.NewLine, Categories.Select(c=>$"{c.Name} ${c.totalprofit:f2}"));
        }
        // Ex. 14
        public static string GetMostRecentBooks(BookShopContext context)
        {
            string output = string.Empty;

            var categories = context.Categories
                .Select(c => new
                {
                    c.Name,
                    mostrecent = c.CategoryBooks.Select(c => c.Book).OrderByDescending(b => b.ReleaseDate).Take(3)
                })
                .OrderBy(c => c.Name);
            categories.Select(c => new
            {
            }).ToList();
            foreach (var category in categories)
            {
                output += Environment.NewLine + $"--{category.Name}";
                foreach (var book in category.mostrecent)
                {
                   output += Environment.NewLine + book.Title + " " + $"({book.ReleaseDate.Value.Year})";

                }
            }

            return output;
        }
        // Ex. 15
        public static void IncreasePrices(BookShopContext context)
        {
           var books =  context.Books.Where(
                b => b.ReleaseDate.Value.Year < 2010);
                
            foreach (var book in books)
            {
                book.Price += 5;
            }
            context.SaveChanges();
        }
        // Ex. 16
        public static int RemoveBooks(BookShopContext context)
        {
            int copies = 0;
            var books = context.Books
                .Where(b => b.Copies < 4200);
            copies = books.Count();
            context.Books.RemoveRange(books);
            context.SaveChanges();
            return copies;
        }
    }
}



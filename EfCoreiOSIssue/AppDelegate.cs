using Foundation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UIKit;
using Xamarin.Essentials;

namespace Blank
{
    public class Customer
    {
        public Guid Id { get; set; } // you don't have to define non nullable properties as object

        public string Name { get; set; } // you can use int, string, bool, etc.

        public object SomeGuidProp1 { get; set; } // A nullable Guid property.
        public object SomeGuidProp2 { get; set; }
        public Guid SomeGuidProp3 { get; set; }

        public object BirthDate1 { get; set; } // A nullable date time offset property.
        public object BirthDate2 { get; set; }
        public object BirthDate3 { get; set; }
        public DateTimeOffset BirthDate4 { get; set; } // it's not nullable! So you don't have to define it as object.

        public object Salary1 { get; set; } // A nullable decimal property.
        public object Salary2 { get; set; }
        public object Salary3 { get; set; }
        public decimal Salary4 { get; set; }
    }

    public static class PropertyBuilderExtensions
    {
        private static readonly ValueConverter<object, byte[]> GuidToByteArrayConverter = new ValueConverter<object, byte[]>(v => ((Guid)v).ToByteArray(), v => new Guid(v));
        private static readonly ValueConverter<object, string> DecimalToStringConverter = new ValueConverter<object, string>(v => ((decimal)v).ToString("0.0###########################", CultureInfo.InvariantCulture), v => decimal.Parse(v, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture));
        private static readonly ValueConverter<object, string> DateTimeOffsetToStringConverter = new ValueConverter<object, string>(v => ((DateTimeOffset)v).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture), v => DateTimeOffset.Parse(v, CultureInfo.InvariantCulture));
        private static readonly ValueConverter<object, string> DateTimeToStringConverter = new ValueConverter<object, string>(v => ((DateTime)v).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF", CultureInfo.InvariantCulture), v => DateTime.Parse(v, CultureInfo.InvariantCulture));

        public static PropertyBuilder<object> AsNullableGuid(this PropertyBuilder<object> prop)
        {
            return prop
                .HasColumnType("BLOB")
                .HasConversion(GuidToByteArrayConverter);
        }

        public static PropertyBuilder<object> AsNullableDecimal(this PropertyBuilder<object> prop)
        {
            return prop
                .HasColumnType("TEXT")
                .HasConversion(DecimalToStringConverter);
        }

        public static PropertyBuilder<object> AsNullableDateTimeOffset(this PropertyBuilder<object> prop)
        {
            return prop
                .HasColumnType("TEXT")
                .HasConversion(DateTimeOffsetToStringConverter);
        }

        public static PropertyBuilder<object> AsNullableDateTime(this PropertyBuilder<object> prop)
        {
            return prop
                .HasColumnType("TEXT")
                .HasConversion(DateTimeOffsetToStringConverter);
        }
    }

    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbFileName = Path.Combine(FileSystem.AppDataDirectory, "App.db");

            optionsBuilder.UseSqlite($"Filename={dbFileName}")
                .ConfigureWarnings(warnings =>
                {
                    warnings.Throw(RelationalEventId.QueryClientEvaluationWarning);
                    warnings.Throw(RelationalEventId.ValueConversionSqlLiteralWarning);
                });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .Property(c => c.SomeGuidProp1).AsNullableGuid();

            modelBuilder.Entity<Customer>()
                .Property(c => c.SomeGuidProp2).AsNullableGuid();

            modelBuilder.Entity<Customer>()
                .Property(c => c.BirthDate1).AsNullableDateTimeOffset();

            modelBuilder.Entity<Customer>()
                .Property(c => c.BirthDate2).AsNullableDateTimeOffset();

            modelBuilder.Entity<Customer>()
                .Property(c => c.BirthDate3).AsNullableDateTimeOffset();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Salary1).AsNullableDecimal();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Salary2).AsNullableDecimal();

            modelBuilder.Entity<Customer>()
                .Property(c => c.Salary3).AsNullableDecimal();
        }

        public DbSet<Customer> Customers { get; set; }
    }

    [Register(nameof(AppDelegate))]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window
        {
            get;
            set;
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            SQLitePCL.Batteries_V2.Init();

            using (AppDbContext dbContext = new AppDbContext())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                dbContext.Customers.Add(new Customer
                {
                    Id = Guid.Parse("e528df7b-0a92-4eae-be64-7dd18805cbd2"),
                    SomeGuidProp1 = Guid.Parse("e528df7b-0a92-4eae-be64-7dd18805cbd2"),
                    SomeGuidProp2 = null,
                    SomeGuidProp3 = Guid.Parse("e528df7b-0a92-4eae-be64-7dd18805cbd2"),
                    Name = "One",
                    BirthDate1 = DateTimeOffset.UtcNow,
                    BirthDate2 = DateTimeOffset.UtcNow,
                    BirthDate3 = null,
                    BirthDate4 = DateTimeOffset.UtcNow,
                    Salary1 = null,
                    Salary2 = 1.1m,
                    Salary3 = -1.02m,
                    Salary4 = 2.2m
                });

                dbContext.SaveChanges();

                Customer customer = dbContext.Customers.First();
            }

            Window = new UIWindow(UIScreen.MainScreen.Bounds)
            {
                RootViewController = new UIViewController()
            };

            Window.MakeKeyAndVisible();

            return true;
        }
    }
}

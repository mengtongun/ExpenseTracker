using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using BCryptNet = BCrypt.Net.BCrypt;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Infrastructure.Data.Seeding;

public class DataSeeder(ApplicationDbContext dbContext, ILogger<DataSeeder> logger) : IDataSeeder
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<DataSeeder> _logger = logger;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Users.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Database already contains data. Skipping seeding.");
            return;
        }

        var now = DateTime.UtcNow;

        var users = BuildUsers(now);

        await _dbContext.Users.AddRangeAsync(users, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Seeded {UserCount} users, {ExpenseCount} expenses, {RecurringExpenseCount} recurring expenses, and {ReportCount} reports.",
            users.Count,
            users.Sum(u => u.Expenses.Count),
            users.Sum(u => u.RecurringExpenses.Count),
            users.Sum(u => u.Reports.Count));
    }

    private static List<User> BuildUsers(DateTime now)
    {
        static DateOnly ToDate(DateTime date) => DateOnly.FromDateTime(date);
        static string FormatDate(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        static string RangeJson(DateOnly from, DateOnly to, string? label = null)
        {
            var payload = new Dictionary<string, object?>
            {
                ["from"] = FormatDate(from),
                ["to"] = FormatDate(to)
            };

            if (!string.IsNullOrWhiteSpace(label))
            {
                payload["label"] = label;
            }

            return JsonSerializer.Serialize(payload);
        }

        static User CreateUser(string username, string email, string fullName, bool isActive, DateTime? lastLoginAt)
        {
            return new User
            {
                Username = username,
                Email = email,
                FullName = fullName,
                PasswordHash = BCryptNet.HashPassword("Password123!"),
                IsActive = isActive,
                LastLoginAt = lastLoginAt
            };
        }

        static Dictionary<string, Category> AddCategories(User user, params (string Name, string? Description)[] categories)
        {
            var result = new Dictionary<string, Category>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, description) in categories)
            {
                var category = new Category
                {
                    Name = name,
                    Description = description
                };

                user.Categories.Add(category);
                result[name] = category;
            }

            return result;
        }

        static RecurringExpense CreateRecurring(User user, Category? category, decimal amount, string currency, string description,
            RecurrenceFrequency frequency, DateOnly startDate, DateOnly nextOccurrence, DateOnly? endDate = null, bool isActive = true)
        {
            var recurring = new RecurringExpense
            {
                User = user,
                Category = category,
                Amount = amount,
                Currency = currency,
                Description = description,
                Frequency = frequency,
                StartDate = startDate,
                NextOccurrence = nextOccurrence,
                EndDate = endDate,
                IsActive = isActive
            };

            user.RecurringExpenses.Add(recurring);
            category?.RecurringExpenses.Add(recurring);
            return recurring;
        }

        static void AddExpense(User user, Category? category, decimal amount, string currency, string description, DateTime date,
            bool isRecurring = false, RecurringExpense? recurringExpense = null)
        {
            var expense = new Expense
            {
                User = user,
                Category = category,
                Amount = amount,
                Currency = currency,
                Description = description,
                ExpenseDate = ToDate(date),
                IsRecurring = isRecurring,
                RecurringExpense = recurringExpense
            };

            user.Expenses.Add(expense);
            category?.Expenses.Add(expense);
            recurringExpense?.Expenses.Add(expense);
        }

        var users = new List<User>();

        #region Alex Morgan
        var alex = CreateUser("alex.morgan", "alex.morgan@example.com", "Alex Morgan", true, now.AddDays(-2));
        var alexCategories = AddCategories(alex,
            ("Groceries", "Food and grocery shopping"),
            ("Housing", "Rent, mortgage, and home supplies"),
            ("Utilities", "Electricity, water, and other utilities"),
            ("Transportation", "Fuel, rideshares, and transit"),
            ("Entertainment", "Movies, concerts, and streaming"),
            ("Healthcare", "Medical and wellness"));

        var rentRecurring = CreateRecurring(
            alex,
            alexCategories["Housing"],
            1850m,
            "USD",
            "Apartment rent in Seattle",
            RecurrenceFrequency.Monthly,
            ToDate(new DateTime(now.Year, now.Month, 1).AddMonths(-10)),
            ToDate(new DateTime(now.Year, now.Month, 1).AddMonths(1)));

        var gymRecurring = CreateRecurring(
            alex,
            alexCategories["Healthcare"],
            79m,
            "USD",
            "Downtown wellness club membership",
            RecurrenceFrequency.Weekly,
            ToDate(now.AddMonths(-6)),
            ToDate(now.AddDays(7)));

        var rentBaseDate = new DateTime(now.Year, now.Month, 1);
        for (var offset = -3; offset <= 0; offset++)
        {
            var date = rentBaseDate.AddMonths(offset);
            AddExpense(
                alex,
                rentRecurring.Category,
                rentRecurring.Amount,
                rentRecurring.Currency,
                $"Apartment rent for {date:MMMM yyyy}",
                date,
                true,
                rentRecurring);
        }

        for (var i = 1; i <= 6; i++)
        {
            var date = now.AddDays(-7 * i);
            AddExpense(
                alex,
                gymRecurring.Category,
                gymRecurring.Amount,
                gymRecurring.Currency,
                "Weekly gym membership",
                date,
                true,
                gymRecurring);
        }

        var alexExpenseDefinitions = new (string Category, decimal Amount, string Currency, string Description, int DaysOffset)[]
        {
            ("Groceries", 124.87m, "USD", "Costco bulk groceries", -5),
            ("Groceries", 86.42m, "USD", "Trader Joe's weekly run", -12),
            ("Entertainment", 58.25m, "USD", "Date night at the cinema", -16),
            ("Transportation", 42.10m, "USD", "Gas refill at Shell", -9),
            ("Utilities", 213.48m, "USD", "Seattle City Light utility bill", -25),
            ("Healthcare", 35.00m, "USD", "Massage therapy copay", -18),
            ("Entertainment", 15.99m, "USD", "Music streaming subscription", -2),
            ("Transportation", 23.75m, "USD", "Ride share to airport", -28),
            ("Groceries", 19.60m, "USD", "Morning coffee and pastries", -1),
            ("Utilities", 94.33m, "USD", "Internet provider bill", -32),
            ("Groceries", 140.51m, "USD", "Farmer's market weekend haul", -44),
            ("Healthcare", 220.00m, "USD", "Annual physical copay", -55)
        };

        foreach (var item in alexExpenseDefinitions)
        {
            AddExpense(
                alex,
                alexCategories[item.Category],
                item.Amount,
                item.Currency,
                item.Description,
                now.AddDays(item.DaysOffset));
        }

        AddExpense(alex, null, 48.75m, "USD", "Cash withdrawal for neighborhood fair", now.AddDays(-21));
        AddExpense(alex, null, 12.00m, "USD", "Parking meter downtown", now.AddDays(-3));

        var alexMonthlyRangeStart = ToDate(new DateTime(now.Year, now.Month, 1).AddMonths(-1));
        var alexMonthlyRangeEnd = ToDate(new DateTime(now.Year, now.Month, 1).AddDays(-1));

        alex.Reports.Add(new Report
        {
            User = alex,
            ReportType = "MonthlySummary",
            Parameters = RangeJson(alexMonthlyRangeStart, alexMonthlyRangeEnd, "PreviousMonth"),
            FileName = null,
            ContentType = null
        });

        alex.Reports.Add(new Report
        {
            User = alex,
            ReportType = "CategoryBreakdown",
            Parameters = JsonSerializer.Serialize(new
            {
                from = FormatDate(ToDate(now.AddMonths(-3))),
                to = FormatDate(ToDate(now)),
                category = "Groceries"
            })
        });

        users.Add(alex);
        #endregion

        #region Priya Patel
        var priya = CreateUser("priya.patel", "priya.patel@example.co.uk", "Priya Patel", true, now.AddDays(-4));
        var priyaCategories = AddCategories(priya,
            ("Food & Dining", "Home cooking and restaurants"),
            ("Travel", "Flights, hotels, and commuting"),
            ("Childcare", "School, daycare, and activities"),
            ("Utilities", "Household utilities"),
            ("Shopping", "Clothing and personal shopping"),
            ("Savings", "Automatic savings and investments"));

        var childcareRecurring = CreateRecurring(
            priya,
            priyaCategories["Childcare"],
            260m,
            "GBP",
            "Bi-weekly daycare fees",
            RecurrenceFrequency.BiWeekly,
            ToDate(now.AddMonths(-5)),
            ToDate(now.AddDays(14)));

        var travelFundRecurring = CreateRecurring(
            priya,
            priyaCategories["Savings"],
            500m,
            "GBP",
            "Quarterly family travel fund",
            RecurrenceFrequency.Quarterly,
            ToDate(new DateTime(now.Year - 1, 1, 1)),
            ToDate(new DateTime(now.Year, 7, 1)),
            null,
            true);

        var childcareBaseDate = now.AddDays(-14 * 6);
        for (var i = 0; i < 6; i++)
        {
            var occurrenceDate = childcareBaseDate.AddDays(14 * i);
            AddExpense(
                priya,
                childcareRecurring.Category,
                childcareRecurring.Amount,
                childcareRecurring.Currency,
                "Little Explorers daycare",
                occurrenceDate,
                true,
                childcareRecurring);
        }

        for (var q = -3; q <= 0; q++)
        {
            var occurrenceDate = new DateTime(now.Year, ((now.Month - 1) / 3 + 1) * 3, 1).AddMonths(3 * q);
            AddExpense(
                priya,
                travelFundRecurring.Category,
                travelFundRecurring.Amount,
                travelFundRecurring.Currency,
                "Contribution to family travel fund",
                occurrenceDate,
                true,
                travelFundRecurring);
        }

        var priyaExpenses = new (string Category, decimal Amount, string Currency, string Description, int DaysOffset)[]
        {
            ("Food & Dining", 52.80m, "GBP", "Brunch at Borough Market", -6),
            ("Travel", 8.20m, "GBP", "Oyster card top-up", -2),
            ("Shopping", 134.99m, "GBP", "Spring wardrobe update", -13),
            ("Utilities", 178.45m, "GBP", "British Gas utility bill", -29),
            ("Food & Dining", 24.15m, "GBP", "Afternoon tea", -18),
            ("Travel", 415.90m, "EUR", "Flight to Barcelona for summer holiday", -40),
            ("Savings", 200.00m, "GBP", "Automatic ISA transfer", -30),
            ("Food & Dining", 11.50m, "GBP", "Coffee with colleagues", -1),
            ("Travel", 55.00m, "GBP", "Rail tickets to Oxford", -22),
            ("Shopping", 89.00m, "GBP", "Children's school supplies", -16),
            ("Utilities", 62.30m, "GBP", "Water board invoice", -48),
            ("Food & Dining", 96.75m, "GBP", "Weekly supermarket delivery", -9),
            ("Savings", 150.00m, "GBP", "Stocks & Shares ISA", -62)
        };

        foreach (var item in priyaExpenses)
        {
            AddExpense(
                priya,
                priyaCategories[item.Category],
                item.Amount,
                item.Currency,
                item.Description,
                now.AddDays(item.DaysOffset));
        }

        AddExpense(priya, null, 37.95m, "GBP", "Local charity event donation", now.AddDays(-11));
        AddExpense(priya, null, 15.00m, "GBP", "Birthday gift for colleague", now.AddDays(-19));

        priya.Reports.Add(new Report
        {
            User = priya,
            ReportType = "YearToDate",
            Parameters = RangeJson(ToDate(new DateTime(now.Year, 1, 1)), ToDate(now)),
            FileName = null,
            ContentType = null
        });

        priya.Reports.Add(new Report
        {
            User = priya,
            ReportType = "CategoryBreakdown",
            Parameters = JsonSerializer.Serialize(new
            {
                from = FormatDate(ToDate(now.AddMonths(-6))),
                to = FormatDate(ToDate(now)),
                categories = new[] { "Food & Dining", "Travel" }
            })
        });

        users.Add(priya);
        #endregion

        #region Kenji Tanaka
        var kenji = CreateUser("kenji.tanaka", "kenji.tanaka@example.jp", "Kenji Tanaka", true, now.AddDays(-1));
        var kenjiCategories = AddCategories(kenji,
            ("Dining", "Restaurants and cafes"),
            ("Utilities", "Household utilities"),
            ("Subscriptions", "Digital subscriptions"),
            ("Transportation", "Transit and commuting"),
            ("Wellness", "Health and wellness"),
            ("Education", "Courses and learning"));

        var coffeeRecurring = CreateRecurring(
            kenji,
            kenjiCategories["Dining"],
            450m,
            "JPY",
            "Morning espresso at station cafe",
            RecurrenceFrequency.Daily,
            ToDate(now.AddDays(-20)),
            ToDate(now.AddDays(1)));

        for (var day = 5; day >= 1; day--)
        {
            AddExpense(
                kenji,
                coffeeRecurring.Category,
                coffeeRecurring.Amount,
                coffeeRecurring.Currency,
                "Morning espresso",
                now.AddDays(-day),
                true,
                coffeeRecurring);
        }

        var softwareRecurring = CreateRecurring(
            kenji,
            kenjiCategories["Subscriptions"],
            11000m,
            "JPY",
            "Annual design software license",
            RecurrenceFrequency.Yearly,
            ToDate(new DateTime(now.Year - 3, 5, 15)),
            ToDate(new DateTime(now.Year + 1, 5, 15)));

        AddExpense(
            kenji,
            softwareRecurring.Category,
            softwareRecurring.Amount,
            softwareRecurring.Currency,
            "Design suite license renewal",
            new DateTime(now.Year, 5, 15),
            true,
            softwareRecurring);

        var kenjiExpenses = new (string Category, decimal Amount, string Currency, string Description, int DaysOffset)[]
        {
            ("Dining", 2450m, "JPY", "Ramen dinner with friends", -7),
            ("Education", 48000m, "JPY", "UX masterclass course", -35),
            ("Transportation", 12000m, "JPY", "Commuter rail pass", -3),
            ("Utilities", 8900m, "JPY", "Electric utility bill", -12),
            ("Wellness", 6500m, "JPY", "Yoga studio drop-in", -9),
            ("Subscriptions", 980m, "JPY", "Music streaming plan", -1),
            ("Dining", 3200m, "JPY", "Sushi lunch", -15),
            ("Transportation", 5600m, "JPY", "Taxi ride home", -22),
            ("Utilities", 4100m, "JPY", "Mobile carrier invoice", -28),
            ("Wellness", 14500m, "JPY", "Annual health check", -65),
            ("Education", 27500m, "JPY", "Online language course", -53),
            ("Dining", 1850m, "JPY", "Afternoon cafe meetup", -4)
        };

        foreach (var item in kenjiExpenses)
        {
            AddExpense(
                kenji,
                kenjiCategories[item.Category],
                item.Amount,
                item.Currency,
                item.Description,
                now.AddDays(item.DaysOffset));
        }

        AddExpense(kenji, null, 7800m, "JPY", "Family gift contribution", now.AddDays(-17));
        AddExpense(kenji, null, 3400m, "JPY", "Cash for office snacks", now.AddDays(-8));

        kenji.Reports.Add(new Report
        {
            User = kenji,
            ReportType = "MonthlySummary",
            Parameters = RangeJson(ToDate(new DateTime(now.Year, now.Month, 1)), ToDate(now)),
            FileName = null,
            ContentType = null
        });

        kenji.Reports.Add(new Report
        {
            User = kenji,
            ReportType = "Custom",
            Parameters = JsonSerializer.Serialize(new
            {
                title = "Education vs Wellness",
                from = FormatDate(ToDate(now.AddMonths(-2))),
                to = FormatDate(ToDate(now)),
                compareCategories = new[] { "Education", "Wellness" }
            })
        });

        users.Add(kenji);
        #endregion

        #region Maria Gomez
        var maria = CreateUser("maria.gomez", "maria.gomez@example.es", "Maria Gomez", false, now.AddDays(-90));
        var mariaCategories = AddCategories(maria,
            ("Home", "Mortgage and home improvements"),
            ("Food", "Groceries and markets"),
            ("Business", "Freelance and consulting expenses"),
            ("Family", "Family activities and education"),
            ("Travel", "Weekend trips and vacations"),
            ("Miscellaneous", "Everything else"));

        var mortgageRecurring = CreateRecurring(
            maria,
            mariaCategories["Home"],
            980.75m,
            "EUR",
            "Mortgage payment",
            RecurrenceFrequency.Monthly,
            ToDate(new DateTime(now.Year - 5, 4, 1)),
            ToDate(new DateTime(now.Year, now.Month, 1).AddMonths(1)),
            null,
            false);

        var tutoringRecurring = CreateRecurring(
            maria,
            mariaCategories["Family"],
            120m,
            "EUR",
            "Weekly language tutoring",
            RecurrenceFrequency.Weekly,
            ToDate(now.AddMonths(-8)),
            ToDate(now.AddDays(7)),
            ToDate(now.AddMonths(2)),
            true);

        for (var offset = -2; offset <= 0; offset++)
        {
            var date = new DateTime(now.Year, now.Month, 5).AddMonths(offset);
            AddExpense(
                maria,
                mortgageRecurring.Category,
                mortgageRecurring.Amount,
                mortgageRecurring.Currency,
                $"Mortgage payment for {date:MMMM yyyy}",
                date,
                true,
                mortgageRecurring);
        }

        for (var week = 1; week <= 8; week++)
        {
            var date = now.AddDays(-7 * week + 1);
            AddExpense(
                maria,
                tutoringRecurring.Category,
                tutoringRecurring.Amount,
                tutoringRecurring.Currency,
                "Weekly Spanish tutoring for Diego",
                date,
                true,
                tutoringRecurring);
        }

        var mariaExpenses = new (string Category, decimal Amount, string Currency, string Description, int DaysOffset)[]
        {
            ("Food", 72.40m, "EUR", "Local market produce", -5),
            ("Business", 310.00m, "EUR", "Conference registration fee", -33),
            ("Travel", 215.50m, "EUR", "Weekend trip to Valencia", -40),
            ("Miscellaneous", 45.00m, "EUR", "Birthday decorations", -12),
            ("Food", 18.90m, "EUR", "Cafe meetup", -4),
            ("Family", 68.30m, "EUR", "Museum passes", -21),
            ("Home", 129.99m, "EUR", "Smart thermostat upgrade", -65),
            ("Business", 580.00m, "USD", "Laptop purchase for freelance work", -70),
            ("Travel", 92.00m, "EUR", "Train tickets for family outing", -14),
            ("Miscellaneous", 35.70m, "EUR", "Charity donation", -28),
            ("Family", 240.00m, "EUR", "Summer camp deposit", -50),
            ("Food", 102.15m, "EUR", "Monthly wholesale order", -32)
        };

        foreach (var item in mariaExpenses)
        {
            AddExpense(
                maria,
                mariaCategories[item.Category],
                item.Amount,
                item.Currency,
                item.Description,
                now.AddDays(item.DaysOffset));
        }

        AddExpense(maria, null, 55.00m, "EUR", "Cash for neighborhood fundraiser", now.AddDays(-18));
        AddExpense(maria, null, 27.50m, "EUR", "Taxi ride", now.AddDays(-7));

        maria.Reports.Add(new Report
        {
            User = maria,
            ReportType = "CategoryBreakdown",
            Parameters = JsonSerializer.Serialize(new
            {
                from = FormatDate(ToDate(now.AddMonths(-4))),
                to = FormatDate(ToDate(now.AddMonths(-1))),
                includeInactiveUser = true
            })
        });

        maria.Reports.Add(new Report
        {
            User = maria,
            ReportType = "RecurringOverview",
            Parameters = JsonSerializer.Serialize(new
            {
                snapshot = FormatDate(ToDate(now)),
                includeInactiveRecurring = true
            })
        });

        users.Add(maria);
        #endregion

        return users;
    }
}

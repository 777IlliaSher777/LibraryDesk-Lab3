using LibraryDesk.Core.Legacy;
using Xunit;

namespace LibraryDesk.Tests;

/// <summary>
/// Перевіряє початкову поведінку обробки формуляра видачі.
/// </summary>
public class LegacyLoanProcessorTests
{
    /// <summary>
    /// Перевіряє розрахунок суми для постійного клієнта.
    /// </summary>
    [Fact]
    public void ProcessRegularClientReturnsExpectedTotal()
    {
        List<BookRow> lines = new()
        {
            new BookRow
            {
                Sku = "BOOK-001",
                Quantity = 12,
                UnitPrice = 100m
            },
            new BookRow
            {
                Sku = "BOOK-002",
                Quantity = 1,
                UnitPrice = 250m
            }
        };

        decimal total = LegacyLoanProcessor.Process(
            1,
            "Іваненко",
            "ivanenko@example.com",
            true,
            lines,
            new DateOnly(2026, 3, 10),
            string.Empty,
            "New",
            false,
            60m,
            out string? newStatus);

        Assert.Equal(1292.70m, total);
        Assert.Equal("Paid", newStatus);
    }

    /// <summary>
    /// Перевіряє розрахунок суми для невеликої видачі.
    /// </summary>
    [Fact]
    public void ProcessSmallOrderReturnsExpectedTotal()
    {
        List<BookRow> lines = new()
        {
            new BookRow
            {
                Sku = "BOOK-003",
                Quantity = 2,
                UnitPrice = 200m
            }
        };

        decimal total = LegacyLoanProcessor.Process(
            2,
            "Петренко",
            "petrenko@example.com",
            false,
            lines,
            new DateOnly(2026, 3, 10),
            "SALE10",
            "Paid",
            false,
            60m,
            out string? newStatus);

        Assert.Equal(420m, total);
        Assert.Equal("Shipped", newStatus);
    }

    /// <summary>
    /// Перевіряє обробку документа без книжкових позицій.
    /// </summary>
    [Fact]
    public void ProcessWithoutLinesReturnsMinusOne()
    {
        List<BookRow> lines = new();

        decimal total = LegacyLoanProcessor.Process(
            3,
            "Шевченко",
            "shevchenko@example.com",
            false,
            lines,
            new DateOnly(2026, 3, 10),
            string.Empty,
            "New",
            false,
            60m,
            out string? newStatus);

        Assert.Equal(-1m, total);
        Assert.Equal("New", newStatus);
    }
}

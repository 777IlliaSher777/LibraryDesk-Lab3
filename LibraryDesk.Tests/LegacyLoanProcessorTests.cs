using LibraryDesk.Core;
using LibraryDesk.Core.Legacy;
using Xunit;

namespace LibraryDesk.Tests;

/// <summary>
/// Перевіряє поведінку диспетчера обробки формуляра.
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
            LoanStatus.New,
            false,
            60m,
            out LoanStatus newStatus);

        Assert.Equal(1292.70m, total);
        Assert.Equal(LoanStatus.Active, newStatus);
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
            LoanStatus.Active,
            false,
            60m,
            out LoanStatus newStatus);

        Assert.Equal(420m, total);
        Assert.Equal(LoanStatus.Active, newStatus);
    }

    /// <summary>
    /// Перевіряє виняток для порожнього списку позицій.
    /// </summary>
    [Fact]
    public void ProcessWithoutLinesThrowsArgumentException()
    {
        List<BookRow> lines = new();

        Assert.Throws<ArgumentException>(() =>
            LegacyLoanProcessor.Process(
                3,
                "Шевченко",
                "shevchenko@example.com",
                false,
                lines,
                new DateOnly(2026, 3, 10),
                string.Empty,
                LoanStatus.New,
                false,
                60m,
                out _));
    }

    /// <summary>
    /// Перевіряє виняток для нульової кількості книжок.
    /// </summary>
    [Fact]
    public void ProcessWithZeroQuantityThrowsArgumentOutOfRangeException()
    {
        List<BookRow> lines = new()
        {
            new BookRow
            {
                Sku = "BOOK-004",
                Quantity = 0,
                UnitPrice = 100m
            }
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LegacyLoanProcessor.Process(
                4,
                "Коваленко",
                "kovalenko@example.com",
                false,
                lines,
                new DateOnly(2026, 3, 10),
                string.Empty,
                LoanStatus.New,
                false,
                60m,
                out _));
    }
}

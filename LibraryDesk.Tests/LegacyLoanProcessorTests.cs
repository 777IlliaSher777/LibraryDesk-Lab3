using LibraryDesk.Core;
using LibraryDesk.Core.Legacy;
using Xunit;

namespace LibraryDesk.Tests;

/// <summary>
/// Перевіряє розрахунок вартості формуляра видачі.
/// </summary>
public class LegacyLoanProcessorTests
{
    [Fact]
    public void CalculateRegularClientReturnsExpectedTotal()
    {
        TotalRequest request = new(
            DocumentId: 1,
            Client: new ClientInfo(
                "Іваненко",
                "i@ex.com",
                true),
            Lines: new List<BookRow>
            {
                new()
                {
                    Sku = "A1",
                    Quantity = 12,
                    UnitPrice = 100m
                },
                new()
                {
                    Sku = "B2",
                    Quantity = 1,
                    UnitPrice = 250m
                }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: LoanStatus.New,
            DeliveryPrice: 60m);

        TotalResult result =
            DocumentTotalCalculator.Calculate(request);

        Assert.Equal(1292.70m, result.Total);
        Assert.Equal(LoanStatus.Active, result.Status);
    }

    [Fact]
    public void CalculateSmallOrderReturnsExpectedTotal()
    {
        TotalRequest request = new(
            DocumentId: 2,
            Client: new ClientInfo(
                "Петренко",
                "p@ex.com",
                false),
            Lines: new List<BookRow>
            {
                new()
                {
                    Sku = "BOOK-001",
                    Quantity = 2,
                    UnitPrice = 200m
                }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: PricingRules.SaleCoupon,
            Status: LoanStatus.Active,
            DeliveryPrice: 60m);

        TotalResult result =
            DocumentTotalCalculator.Calculate(request);

        Assert.Equal(420m, result.Total);
        Assert.Equal(LoanStatus.Active, result.Status);
    }

    [Fact]
    public void CalculateWithoutLinesThrowsArgumentException()
    {
        TotalRequest request = new(
            DocumentId: 3,
            Client: new ClientInfo(
                "Іваненко",
                "i@ex.com",
                false),
            Lines: new List<BookRow>(),
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: LoanStatus.New,
            DeliveryPrice: 60m);

        Assert.Throws<ArgumentException>(
            () => DocumentTotalCalculator.Calculate(request));
    }

    [Fact]
    public void CalculateWithZeroQuantityThrowsArgumentOutOfRangeException()
    {
        TotalRequest request = new(
            DocumentId: 4,
            Client: new ClientInfo(
                "Іваненко",
                "i@ex.com",
                false),
            Lines: new List<BookRow>
            {
                new()
                {
                    Sku = "BOOK-002",
                    Quantity = 0,
                    UnitPrice = 100m
                }
            },
            CreatedAt: new DateOnly(2026, 3, 10),
            CouponCode: null,
            Status: LoanStatus.New,
            DeliveryPrice: 60m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => DocumentTotalCalculator.Calculate(request));
    }
}

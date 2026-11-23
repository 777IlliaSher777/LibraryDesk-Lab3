namespace LibraryDesk.Core.Legacy;

/// <summary>
/// Містить окремі підпрограми для розрахунку вартості видачі.
/// </summary>
public static class DocumentTotalCalculator
{
    /// <summary>
    /// Обчислює суму книжкових позицій з урахуванням гуртової знижки.
    /// </summary>
    /// <param name="lines">Список книжкових позицій.</param>
    /// <returns>Проміжна сума позицій.</returns>
    public static decimal SubtotalOf(
        IReadOnlyList<BookRow> lines)
    {
        LoanGuards.EnsureLinesValid(lines);

        decimal subtotal = 0m;

        foreach (BookRow line in lines)
        {
            decimal sum = line.Quantity * line.UnitPrice;

            if (line.Quantity >= PricingRules.BulkQuantity)
            {
                sum -= sum * PricingRules.BulkRate;
            }

            subtotal += sum;
        }

        return subtotal;
    }

    /// <summary>
    /// Обчислює знижку для постійного клієнта.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="isRegularClient">Ознака постійного клієнта.</param>
    /// <returns>Розмір знижки.</returns>
    public static decimal ClientDiscountOf(
        decimal amount,
        bool isRegularClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return isRegularClient
            ? amount * PricingRules.RegularClientRate
            : 0m;
    }

    /// <summary>
    /// Обчислює знижку залежно від суми замовлення.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <returns>Розмір знижки.</returns>
    public static decimal VolumeDiscountOf(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        if (amount > PricingRules.LargeOrderFrom)
        {
            return amount * PricingRules.LargeOrderRate;
        }

        if (amount > PricingRules.MediumOrderFrom)
        {
            return amount * PricingRules.MediumOrderRate;
        }

        return 0m;
    }

    /// <summary>
    /// Обчислює знижку за промокодом.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="couponCode">Код купона.</param>
    /// <returns>Розмір знижки.</returns>
    public static decimal CouponDiscountOf(
        decimal amount,
        string? couponCode)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return couponCode switch
        {
            PricingRules.SaleCoupon =>
                amount * PricingRules.CouponSaleRate,

            PricingRules.FixedCoupon
                when amount > PricingRules.CouponFixedFrom =>
                PricingRules.CouponFixed,

            _ => 0m
        };
    }

    /// <summary>
    /// Обчислює знижку за створення документа у неділю.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="createdAt">Дата створення документа.</param>
    /// <returns>Розмір знижки.</returns>
    public static decimal WeekendDiscountOf(
        decimal amount,
        DateOnly createdAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);

        return createdAt.DayOfWeek == DayOfWeek.Sunday
            ? amount * PricingRules.WeekendRate
            : 0m;
    }

    /// <summary>
    /// Визначає вартість доставки.
    /// </summary>
    /// <param name="amount">Поточна сума.</param>
    /// <param name="deliveryPrice">Вартість доставки.</param>
    /// <returns>Сума доставки.</returns>
    public static decimal DeliveryFeeOf(
        decimal amount,
        decimal deliveryPrice)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryPrice);

        return amount < PricingRules.FreeDeliveryFrom
            ? deliveryPrice
            : 0m;
    }

    /// <summary>
    /// Визначає наступний стан формуляра.
    /// </summary>
    /// <param name="current">Поточний стан.</param>
    /// <param name="total">Підсумкова сума.</param>
    /// <returns>Наступний стан формуляра.</returns>
    public static LoanStatus NextStatusOf(
        LoanStatus current,
        decimal total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total);

        return current switch
        {
            LoanStatus.New when total > 0m =>
                LoanStatus.Active,

            _ => current
        };
    }
}

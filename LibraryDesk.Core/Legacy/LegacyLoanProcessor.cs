namespace LibraryDesk.Core.Legacy;

/// <summary>
/// Містить диспетчер обробки формуляра видачі книжок.
/// </summary>
public static class LegacyLoanProcessor
{
    /// <summary>
    /// Виконує послідовну обробку формуляра видачі.
    /// </summary>
    /// <param name="docId">Номер документа.</param>
    /// <param name="clientName">Ім'я клієнта.</param>
    /// <param name="clientEmail">Електронна пошта клієнта.</param>
    /// <param name="isRegular">Ознака постійного клієнта.</param>
    /// <param name="lines">Список книжкових позицій.</param>
    /// <param name="createdAt">Дата створення документа.</param>
    /// <param name="couponCode">Код купона.</param>
    /// <param name="status">Поточний стан документа.</param>
    /// <param name="printToConsole">Ознака виведення звіту.</param>
    /// <param name="deliveryPrice">Вартість доставки.</param>
    /// <param name="newStatus">Новий стан документа.</param>
    /// <returns>Підсумкова сума видачі.</returns>
    public static decimal Process(
        int docId,
        string? clientName,
        string? clientEmail,
        bool isRegular,
        IReadOnlyList<BookRow>? lines,
        DateOnly createdAt,
        string? couponCode,
        LoanStatus status,
        bool printToConsole,
        decimal deliveryPrice,
        out LoanStatus newStatus)
    {
        LoanGuards.EnsureHeaderValid(
            docId,
            clientName,
            clientEmail);

        LoanGuards.EnsureLinesValid(lines);

        decimal total = DocumentTotalCalculator.SubtotalOf(lines!);

        total -= DocumentTotalCalculator.ClientDiscountOf(
            total,
            isRegular);

        total -= DocumentTotalCalculator.VolumeDiscountOf(total);

        total -= DocumentTotalCalculator.CouponDiscountOf(
            total,
            couponCode);

        total -= DocumentTotalCalculator.WeekendDiscountOf(
            total,
            createdAt);

        total += DocumentTotalCalculator.DeliveryFeeOf(
            total,
            deliveryPrice);

        total = status == LoanStatus.Cancelled
            ? 0m
            : Math.Round(Math.Max(total, 0m), 2);

        newStatus = DocumentTotalCalculator.NextStatusOf(
            status,
            total);

        if (printToConsole)
        {
            Console.WriteLine("Документ № " + docId);
            Console.WriteLine("Клієнт: " + clientName);

            foreach (BookRow line in lines!)
            {
                Console.WriteLine(
                    line.Sku + " x " + line.Quantity);
            }

            Console.WriteLine("Разом: " + total);
            Console.WriteLine("Стан: " + newStatus);
        }

        return total;
    }
}

namespace LibraryDesk.Core.Legacy;

/// <summary>
/// Містить реалізацію обробки формуляра видачі книжок.
/// </summary>
public static class LegacyLoanProcessor
{
    /// <summary>
    /// Обчислює загальну суму видачі та визначає наступний стан документа.
    /// </summary>
    /// <param name="docId">Номер документа.</param>
    /// <param name="clientName">Ім'я клієнта.</param>
    /// <param name="clientEmail">Електронна пошта клієнта.</param>
    /// <param name="isRegular">Ознака постійного клієнта.</param>
    /// <param name="lines">Список книжкових позицій.</param>
    /// <param name="createdAt">Дата створення документа.</param>
    /// <param name="couponCode">Промокод.</param>
    /// <param name="status">Поточний стан документа.</param>
    /// <param name="printToConsole">Ознака виведення звіту в консоль.</param>
    /// <param name="deliveryPrice">Вартість доставки.</param>
    /// <param name="newStatus">Новий стан документа.</param>
    /// <returns>Загальна сума видачі.</returns>
    public static decimal Process(
        int docId,
        string? clientName,
        string? clientEmail,
        bool isRegular,
        List<BookRow>? lines,
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

        newStatus = status;
        decimal total = 0m;

        foreach (BookRow line in lines!)
        {
            decimal sum = line.Quantity * line.UnitPrice;

            if (line.Quantity >= 10)
            {
                sum *= 0.95m;
            }

            total += sum;
        }

        if (isRegular)
        {
            total *= 0.93m;
        }

        if (total > 5000m)
        {
            total *= 0.9m;
        }
        else if (total > 2000m)
        {
            total *= 0.95m;
        }

        if (couponCode == "SALE10")
        {
            total -= total * 0.1m;
        }
        else if (couponCode == "MINUS200" && total > 1000m)
        {
            total -= 200m;
        }

        if (createdAt.DayOfWeek == DayOfWeek.Sunday)
        {
            total *= 0.98m;
        }

        if (total < 1000m)
        {
            total += deliveryPrice;
        }

        if (total < 0m)
        {
            total = 0m;
        }

        total = Math.Round(total, 2);

        if (status == LoanStatus.New && total > 0m)
        {
            newStatus = LoanStatus.Active;
        }
        else if (status == LoanStatus.Cancelled)
        {
            newStatus = LoanStatus.Cancelled;
            total = 0m;
        }

        if (printToConsole)
        {
            Console.WriteLine($"Документ №: {docId}");
            Console.WriteLine($"Клієнт: {clientName}");
            Console.WriteLine($"Пошта: {clientEmail}");

            foreach (BookRow line in lines!)
            {
                Console.WriteLine($"{line.Sku} x {line.Quantity}");
            }

            Console.WriteLine($"Сума: {total:F2}");
            Console.WriteLine($"Стан: {newStatus}");
        }

        return total;
    }
}

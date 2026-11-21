namespace LibraryDesk.Core.Legacy;

/// <summary>
/// Містить початкову реалізацію обробки формуляра видачі книжок.
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
    /// <returns>Загальна сума або -1 у разі помилки.</returns>
    public static decimal Process(
        int docId,
        string? clientName,
        string? clientEmail,
        bool isRegular,
        List<BookRow>? lines,
        DateOnly createdAt,
        string? couponCode,
        string? status,
        bool printToConsole,
        decimal deliveryPrice,
        out string? newStatus)
    {
        newStatus = status;
        decimal total = 0m;

        if (docId <= 0)
        {
            Console.WriteLine("Помилка: номер документа");
            return -1m;
        }

        if (string.IsNullOrEmpty(clientName))
        {
            Console.WriteLine("Помилка: немає клієнта");
            return -1m;
        }

        if (string.IsNullOrEmpty(clientEmail) ||
            !clientEmail.Contains('@'))
        {
            Console.WriteLine("Помилка: пошта клієнта");
            return -1m;
        }

        if (lines is null || lines.Count == 0)
        {
            Console.WriteLine("Помилка: немає позицій");
            return -1m;
        }

        if (status != "New" &&
            status != "Paid" &&
            status != "Shipped" &&
            status != "Cancelled")
        {
            Console.WriteLine("Помилка: невідомий стан");
            return -1m;
        }

        foreach (BookRow line in lines)
        {
            if (line.Quantity <= 0)
            {
                Console.WriteLine("Помилка: кількість");
                return -1m;
            }

            if (line.UnitPrice < 0m)
            {
                Console.WriteLine("Помилка: ціна");
                return -1m;
            }

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

        if (status == "New" && total > 0m)
        {
            newStatus = "Paid";
        }
        else if (status == "Paid")
        {
            newStatus = "Shipped";
        }
        else if (status == "Cancelled")
        {
            newStatus = "Cancelled";
            total = 0m;
        }

        if (printToConsole)
        {
            Console.WriteLine($"Документ №: {docId}");
            Console.WriteLine($"Клієнт: {clientName}");
            Console.WriteLine($"Пошта: {clientEmail}");

            foreach (BookRow line in lines)
            {
                Console.WriteLine($"{line.Sku} x {line.Quantity}");
            }

            Console.WriteLine($"Сума: {total:F2}");
            Console.WriteLine($"Стан: {newStatus}");
        }

        return total;
    }
}

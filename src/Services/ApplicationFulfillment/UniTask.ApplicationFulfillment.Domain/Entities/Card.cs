using System.Text.RegularExpressions;

namespace UniTask.ApplicationFulfillment.Domain.Entities;

internal sealed class Card
{
    internal bool ValidateCard(string cardNumber)
    {
        var normalizedNumber = Normalize(cardNumber);
        if (normalizedNumber.Length == 0 || !normalizedNumber.All(char.IsDigit))
            return false;

        var sum = 0;
        var alternate = false;
        for (var index = normalizedNumber.Length - 1; index >= 0; index--)
        {
            var digit = normalizedNumber[index] - '0';
            if (alternate)
            {
                digit *= 2;
                if (digit > 9)
                    digit -= 9;
            }

            sum += digit;
            alternate = !alternate;
        }

        return sum % 10 == 0;
    }

    internal string GetCardType(string cardNumber)
    {
        var normalizedNumber = Normalize(cardNumber);

        if (Regex.IsMatch(normalizedNumber, @"^4(\d{12}|\d{15}|\d{18})$"))
            return "Visa";
        if (Regex.IsMatch(normalizedNumber, @"^(5[1-5]\d{14}|2(2[2-9][1-9]|2[3-9]\d{2}|[3-6]\d{3}|7([01]\d{2}|20\d))\d{10})$"))
            return "MasterCard";
        if (Regex.IsMatch(normalizedNumber, @"^3[47]\d{13}$"))
            return "American Express";
        if (Regex.IsMatch(normalizedNumber, @"^(6011\d{12}|65\d{14}|64[4-9]\d{13}|622(1[2-9][6-9]|[2-8]\d{2}|9([01]\d|2[0-5]))\d{10})$"))
            return "Discover";
        if (Regex.IsMatch(normalizedNumber, @"^(352[89]|35[3-8]\d)\d{12}$"))
            return "JCB";
        if (Regex.IsMatch(normalizedNumber, @"^3(0[0-5]|[68]\d)\d{11}$"))
            return "Diners Club";
        if (Regex.IsMatch(normalizedNumber, @"^(50|5[6-9]|6\d)\d{10,17}$"))
            return "Maestro";

        return "Unknown";
    }

    private static string Normalize(string cardNumber) =>
        cardNumber.Replace(" ", string.Empty).Replace("-", string.Empty);
}

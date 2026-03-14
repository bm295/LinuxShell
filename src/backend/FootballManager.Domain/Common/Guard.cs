namespace FootballManager.Domain.Common;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value.Trim();
    }

    public static decimal AgainstNegative(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        return value;
    }

    public static int AgainstOutOfRange(int value, int minimum, int maximum, string parameterName)
    {
        if (value < minimum || value > maximum)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {minimum} and {maximum}.");
        }

        return value;
    }
}

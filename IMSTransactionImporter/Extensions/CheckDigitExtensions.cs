namespace IMSTransactionImporter.Extensions;

public static class CheckDigitExtensions
{
    public static string AddNonDomesticRatesCheckDigit(this string value)
    {
        return CalculateAndAppendCheckDigit(value);
    }

    public static string AddCouncilTaxCheckDigit(this string value)
    {
        return CalculateAndAppendCheckDigit(value);
    }

    public static string AddFixedPenaltyNoticeCheckDigit(this string value)
    {
        return CalculateModulus11CheckDigit7(value);
    }

    public static string AddHousingBenefitsOverpayment7CheckDigit(this string value)
    {
        return CalculateModulus11CheckDigit7(value);
    }
    
    public static string AddHousingBenefitsOverpayment6CheckDigit(this string value)
    {
        return CalculateModulus11CheckDigit6(value);
    }

    
    // Mask ########c
    // Weightings 9 8 7 6 5 4 3 2
    // Modulus 11
    // Group 0
    // Subtract From 11
    // Check Digit Map 1:A 2:B 3:C 4:D 5:E 6:F 7:G 8:H 9:I 10:J 11:K
    public static string AddHousingRentsCheckDigit(this string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 8)
            throw new ArgumentException("Input must be exactly 8 digits", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Input must contain only digits", nameof(value));

        // Weightings array
        int[] weights = [9, 8, 7, 6, 5, 4, 3, 2];

        // Calculate sum of digit * weight
        var sum = 0;
        for (var i = 0; i < 8; i++)
        {
            var digit = int.Parse(value[i].ToString());
            sum += digit * weights[i];
        }

        // Calculate check digit (modulus 11)
        var checkDigit = 11 - (sum % 11);

        // Map check digit to letter
        var checkDigitMap = new Dictionary<int, char>
        {
            {1, 'A'}, {2, 'B'}, {3, 'C'}, {4, 'D'}, {5, 'E'},
            {6, 'F'}, {7, 'G'}, {8, 'H'}, {9, 'I'}, {10, 'J'},
            {11, 'K'}
        };

        return $"{value}{checkDigitMap[checkDigit]}";
    }
    
    
    // Mask ######c
    // Weightings 1 2 3 4 5 6
    // Modulus 10
    // Group 0
    // Subtract From 0
    // Check Digit Map 0:A 1:C 2:E 3:F 4:H 5:J 6:K 7:L 8:M 9:P
    private static string CalculateAndAppendCheckDigit(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 6)
            throw new ArgumentException("Input must be exactly 6 digits", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Input must contain only digits", nameof(value));

        // Weightings array
        int[] weights = [1, 2, 3, 4, 5, 6];

        // Calculate sum of digit * weight
        var sum = 0;
        for (var i = 0; i < 6; i++)
        {
            var digit = int.Parse(value[i].ToString());
            sum += digit * weights[i];
        }

        // Calculate check digit (modulus 10)
        var checkDigit = sum % 10;

        // Map check digit to letter
        var checkDigitMap = new Dictionary<int, char>
        {
            {0, 'A'}, {1, 'C'}, {2, 'E'}, {3, 'F'}, {4, 'H'},
            {5, 'J'}, {6, 'K'}, {7, 'L'}, {8, 'M'}, {9, 'P'}
        };

        return $"{value}{checkDigitMap[checkDigit]}";
    }
    
    // Mask #######c
    // Weightings 8 7 6 5 4 3 2
    // Modulus 11
    // Group 0
    // Subtract From 11
    // Check Digit Map 1:A 2:B 3:C 4:D 5:E 6:F 7:G 8:H 9:I 10:J 11:K
    private static string CalculateModulus11CheckDigit7(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 7)
            throw new ArgumentException("Input must be exactly 7 digits", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Input must contain only digits", nameof(value));

        // Weightings array
        int[] weights = [8, 7, 6, 5, 4, 3, 2];

        // Calculate sum of digit * weight
        var sum = 0;
        for (var i = 0; i < 7; i++)
        {
            var digit = int.Parse(value[i].ToString());
            sum += digit * weights[i];
        }

        // Calculate check digit (modulus 11)
        var checkDigit = 11 - (sum % 11);

        // Map check digit to letter
        var checkDigitMap = new Dictionary<int, char>
        {
            {1, 'A'}, {2, 'B'}, {3, 'C'}, {4, 'D'}, {5, 'E'},
            {6, 'F'}, {7, 'G'}, {8, 'H'}, {9, 'I'}, {10, 'J'},
            {11, 'K'}
        };

        return $"{value}{checkDigitMap[checkDigit]}";
    }
    
    // Mask ######c
    // Weightings 7 6 5 4 3 2
    // Modulus 11
    // Group 0
    // Subtract From 11
    // Check Digit Map 1:A 2:B 3:C 4:D 5:E 6:F 7:G 8:H 9:I 10:J 11:K
    private static string CalculateModulus11CheckDigit6(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length != 6)
            throw new ArgumentException("Input must be exactly 6 digits", nameof(value));

        if (!value.All(char.IsDigit))
            throw new ArgumentException("Input must contain only digits", nameof(value));

        // Weightings array
        int[] weights = [7, 6, 5, 4, 3, 2];

        // Calculate sum of digit * weight
        var sum = 0;
        for (var i = 0; i < 6; i++)
        {
            var digit = int.Parse(value[i].ToString());
            sum += digit * weights[i];
        }

        // Calculate check digit (modulus 11)
        var checkDigit = 11 - (sum % 11);

        // Map check digit to letter
        var checkDigitMap = new Dictionary<int, char>
        {
            {1, 'A'}, {2, 'B'}, {3, 'C'}, {4, 'D'}, {5, 'E'},
            {6, 'F'}, {7, 'G'}, {8, 'H'}, {9, 'I'}, {10, 'J'},
            {11, 'K'}
        };

        return $"{value}{checkDigitMap[checkDigit]}";
    }
}
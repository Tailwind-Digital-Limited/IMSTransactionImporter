using IMSTransactionImporter.Extensions;

namespace IMSTransactionsImporter.Tests;

public class CheckDigitExtensionsTests
{
    [Theory]
    [InlineData("520396", "520396E")]
    [InlineData("521636", "521636L")]
    [InlineData("517647", "517647H")]
    public void AddNonDomesticRatesCheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddNonDomesticRatesCheckDigit();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("103012", "103012L")]
    [InlineData("361774", "361774J")]
    [InlineData("442258", "442258P")]
    [InlineData("399832", "399832L")]
    [InlineData("487737", "487737K")]
    [InlineData("498216", "498216J")]
    public void AddCouncilTaxCheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddCouncilTaxCheckDigit();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0303591", "0303591C")]
    [InlineData("0303616", "0303616B")]
    [InlineData("0303579", "0303579D")]
    [InlineData("0303609", "0303609J")]
    [InlineData("0303592", "0303592A")]
    public void AddFixedPenaltyNoticeCheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddFixedPenaltyNoticeCheckDigit();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("0635157", "0635157C")]
    [InlineData("0635105", "0635105K")]
    [InlineData("0635078", "0635078J")]
    [InlineData("0635074", "0635074G")]
    [InlineData("0635133", "0635133F")]
    public void AddHousingBenefitsOverpayment7CheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddHousingBenefitsOverpayment7CheckDigit();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("634768", "634768A")]
    [InlineData("634827", "634827K")]
    [InlineData("634492", "634492E")]
    [InlineData("634225", "634225F")]
    [InlineData("633917", "633917D")]
    public void AddHousingBenefitsOverpayment6CheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddHousingBenefitsOverpayment6CheckDigit();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("97000023", "97000023E")]
    [InlineData("90000001", "90000001E")]
    [InlineData("90014921", "90014921C")]
    [InlineData("90017993", "90017993G")]
    [InlineData("90015311", "90015311C")]
    [InlineData("90012015", "90012015K")]
    [InlineData("90014096", "90014096H")]
    public void AddHousingRentsCheckDigit_ReturnsCorrectCheckDigit(string input, string expected)
    {
        var result = input.AddHousingRentsCheckDigit();
        Assert.Equal(expected, result);
    }
}
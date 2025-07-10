using IMSTransactionImporter.Transformers;

public class PayrollDeductionsTransformerTests
{
    [Theory]
    [InlineData("Council Tax", "2", "3", 0.0)]
    [InlineData("HB Overpayment", "6", "3", 0.0)]
    [InlineData("Housing Rents", "8", "3", 0.0)]
    [InlineData("Income", "10", "3", 0.0)]
    public void Convert_WithValidFundNames_SetsCorrectFundDetails(string fundName, string expectedFundCode,
        string expectedVatCode, float expectedVatRate)
    {
        // Arrange
        var transaction = new PayrollDeductionTransaction
        {
            TransactionDate = DateTimeOffset.Now,
            CustomerReference = "TEST123",
            Amount = 100.00m,
            FundName = fundName,
            EmployeeNameNumber = "EMP001",
            RowNumber = 1
        };

        // Act
        var result = PayrollDeductionsTransformer.Convert(transaction);

        // Assert
        Assert.Equal(expectedFundCode, result.FundCode);
        Assert.Equal(expectedVatCode, result.VatCode);
        Assert.Equal(expectedVatRate, result.VatRate);
    }

    [Fact]
    public void Convert_WithUnknownFundName_SetsDefaultValues()
    {
        // Arrange
        var transaction = new PayrollDeductionTransaction
        {
            TransactionDate = DateTimeOffset.Now,
            CustomerReference = "TEST123",
            Amount = 100.00m,
            FundName = "Unknown Fund",
            EmployeeNameNumber = "EMP001",
            RowNumber = 1
        };

        // Act
        var result = PayrollDeductionsTransformer.Convert(transaction);

        // Assert
        Assert.Null(result.FundCode);
        Assert.Equal("1", result.VatCode);
        Assert.Equal(0, result.VatRate);
    }

    [Fact]
    public void Convert_GeneratesCorrectPspReference()
    {
        // Arrange
        var transaction = new PayrollDeductionTransaction
        {
            TransactionDate = DateTimeOffset.Now,
            RowNumber = 42
        };
        var expected = $"PYD-{DateTime.Now:yyMMdd}-42";

        // Act
        var result = PayrollDeductionsTransformer.Convert(transaction);

        // Assert
        Assert.Equal(expected, result.PspReference);
    }

    [Fact]
    public void Convert_SetsCorrectDefaults()
    {
        // Arrange
        var transaction = new PayrollDeductionTransaction
        {
            TransactionDate = DateTimeOffset.Now,
            Amount = 100.00m
        };

        // Act
        var result = PayrollDeductionsTransformer.Convert(transaction);

        // Assert
        Assert.Equal("51", result.MopCode);
        Assert.Equal("S", result.OfficeCode);
        Assert.Equal(16, result.InternalReference.Length);
    }
}
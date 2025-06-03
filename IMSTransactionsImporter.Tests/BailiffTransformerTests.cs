using Xunit;
using System;
using IMSTransactionImporter.Transformers;

public class BailiffTransformerTests
{
    [Theory]
    [InlineData("13/05/2025", "521636L", 1168.94, "NDR", "1219226", 1, "5")]
    [InlineData("14/05/2025", "789012X", 250.00, "Council Tax", "1219227", 2, "2")]
    [InlineData("15/05/2025", "345678Y", 500.50, "PCN", "1219228", 3, "9")]
    public void Convert_WithVariousBailiffTransactions_ReturnsCorrectProcessedTransaction(
        string date, 
        string customerRef, 
        decimal amount, 
        string fundName, 
        string liabilityNumber, 
        int rowNumber,
        string expectedFundCode)
    {
        // Arrange
        var transformer = new BailiffTransformer();
        var bailiffTransaction = new BailiffTransaction
        {
            TransactionDate = DateTime.Parse(date),
            CustomerReference = customerRef,
            Amount = amount,
            FundName = fundName,
            LiabilityOrderNumber = liabilityNumber,
            RowNumber = rowNumber
        };

        // Act
        var result = BailiffTransformer.Convert(bailiffTransaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerRef, result.Reference);
        Assert.Equal(amount, result.Amount);
        Assert.Equal("20", result.MopCode);
        Assert.Equal("S", result.OfficeCode);
        Assert.Equal(16, result.InternalReference.Length);
        Assert.Equal($"{liabilityNumber} (Liability order number)", result.Narrative);
        Assert.Equal(expectedFundCode, result.FundCode);
        Assert.Equal("3", result.VatCode);
        Assert.Equal(0m, result.VatRate);
        Assert.Equal(0m, result.VatAmount);
        Assert.StartsWith($"Bailiff-{DateTime.Now:yyyyMMdd}-{rowNumber}", result.PspReference);
        Assert.Equal(DateTime.Parse(date).ToString("O"), result.TransactionDate);
    }
}
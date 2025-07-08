using IMSTransactionImporter.Classes;
using IMSTransactionImporter.Transformers;

namespace IMSTransactionsImporter.Tests;

public class PipTransformerTests
{
    private readonly PIPTransformer _transformer = new();

    [Theory]
    [InlineData("98265029000800950031019", "8", "95003101A")] // pos12=8, chars 15-22 housing rent
    [InlineData("98265029000200004119268", "2", "411926C")] // pos12=2, chars 17-22 council tax
    [InlineData("98265029000500004349268", "5", "434926H")] // pos12=5, chars 17-22 non domestic rates
    [InlineData("98265029000600006879438", "6", "0687943H")] // pos12=6, pos16-22 housing benefit overpayment
    [InlineData("98265029000600068794380", "6", "6879438J")] // pos12=6, pos16-22 housing benefit overpayment
    [InlineData("98265029000700006000000", "6", "0600000B")] // pos12=7, pos16_17=06
    [InlineData("98265029127700001000000", "1", "0100000D")] // pos12=7, pos16_17=01
    [InlineData("98265029127700002000000", "1", "0200000H")] // pos12=7, pos16_17=02
    [InlineData("98265029127700003000000", "1", "0300000A")] // pos12=7, pos16_17=03
    [InlineData("98265029127700004000000", "1", "0400000E")] // pos12=7, pos16_17=04
    [InlineData("98265029127700005000000", "1", "0500000I")] // pos12=7, pos16_17=05
    [InlineData("982650291222ABCDEF00000", "2", "000000A")] // pos12=2
    [InlineData("982650291255ABCDEF00000", "5", "000000A")] // pos12=5
    [InlineData("982650291288ABCDEF00000", "8", "00000000K")] // pos12=8
    public void SetFundCodeAndAccountReference_ShouldSetCorrectValues(string reference, string expectedFundCode, string expectedAccountRef)
    {
        // Arrange
        var transaction = new IMSProcessedTransaction { Reference = reference };

        // Act
        _transformer.SetFundCodeAndAccountReference(transaction);

        // Assert
        Assert.Equal(expectedFundCode, transaction.FundCode);
        Assert.Equal(expectedAccountRef, transaction.AccountReference);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("98265028")] // Wrong prefix
    public void SetFundCodeAndAccountReference_WithInvalidInput_ShouldNotModifyTransaction(string reference)
    {
        // Arrange
        var transaction = new IMSProcessedTransaction 
        { 
            Reference = reference,
            FundCode = "original",
            AccountReference = "original"
        };

        // Act
        _transformer.SetFundCodeAndAccountReference(transaction);

        // Assert
        Assert.Equal("original", transaction.FundCode);
        Assert.Equal("original", transaction.AccountReference);
    }

    [Theory]
    [InlineData("9826502900020000000000", "2")] // pos12=2
    [InlineData("9826502900030000000000", "3")] // pos12=3
    [InlineData("9826502900040000000000", "4")] // pos12=4
    public void SetFundCodeAndAccountReference_WithNumericPos12_ShouldSetFundCodeToPos12Value(string reference, string expectedFundCode)
    {
        // Arrange
        var transaction = new IMSProcessedTransaction { Reference = reference };

        // Act
        _transformer.SetFundCodeAndAccountReference(transaction);

        // Assert
        Assert.Equal(expectedFundCode, transaction.FundCode);
    }

    [Fact]
    public void SetFundCodeAndAccountReference_WithNonNumericPos12_ShouldNotSetFundCode()
    {
        // Arrange
        var transaction = new IMSProcessedTransaction 
        { 
            Reference = "98265029000x00950031019",
            FundCode = ""
        };

        // Act
        _transformer.SetFundCodeAndAccountReference(transaction);

        // Assert
        Assert.Equal("", transaction.FundCode);
    }
}
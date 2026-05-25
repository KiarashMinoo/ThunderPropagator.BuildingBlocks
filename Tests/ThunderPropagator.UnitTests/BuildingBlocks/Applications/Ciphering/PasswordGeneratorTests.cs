using ThunderPropagator.BuildingBlocks.Application.Ciphering;

namespace ThunderPropagator.UnitTests.BuildingBlocks.Applications.Ciphering;

public
#if !DEBUG
    sealed
#endif
    class PasswordGeneratorTests
{
    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void Generate_ShouldReturn_PasswordOfExactLength(int length)
    {
        var password = PasswordGenerator.Generate(length);

        Assert.Equal(length, password.Length);
    }

    [Fact]
    public void Generate_WithLengthBelowMinimum_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PasswordGenerator.Generate(3));
    }

    [Fact]
    public void Generate_WithNoCharacterTypes_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => PasswordGenerator.Generate(8, s =>
        {
            s.IncludeUpperCase = false;
            s.IncludeLowerCase = false;
            s.IncludeNumbers = false;
            s.IncludeSymbols = false;
        }));
    }

    [Fact]
    public void Generate_ShouldNotProduceIndexOutOfRange_ForAllSupportedLengths()
    {
        for (var length = 4; length <= 64; length++)
        {
            var ex = Record.Exception(() => PasswordGenerator.Generate(length));
            Assert.Null(ex);
        }
    }

    [Fact]
    public void Generate_IndexDistribution_ShouldCoverAllValues()
    {
        // Generate enough passwords to verify no index is systematically skipped.
        // With the old bug, only values masked by (int.MaxValue % charSetSize) could appear.
        var seen = new HashSet<char>();
        const string digits = "23456789";

        for (var i = 0; i < 500; i++)
        {
            var password = PasswordGenerator.Generate(16, s =>
            {
                s.IncludeUpperCase = false;
                s.IncludeLowerCase = false;
                s.IncludeNumbers = true;
                s.IncludeSymbols = false;
                s.BeginWithLetter = false;
            });

            foreach (var c in password)
                seen.Add(c);
        }

        // All 8 digits should appear within 500 passwords if distribution is unbiased.
        foreach (var c in digits)
            Assert.Contains(c, seen);
    }

    [Fact]
    public void Generate_WithPreventDuplicateCharacters_ShouldHaveNoDuplicates()
    {
        var password = PasswordGenerator.Generate(8, s =>
        {
            s.PreventDuplicateCharacters = true;
        });

        Assert.Equal(password.Length, password.Distinct().Count());
    }
}

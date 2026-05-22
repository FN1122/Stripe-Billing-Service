using Xunit;

namespace StripeBilling.Tests;

public class SampleTests
{
    [Fact]
    public void TestExample()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var result = 1 + 1;
        
        // Assert
        Assert.Equal(expected, result);
    }
}

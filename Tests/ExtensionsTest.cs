namespace Tests;

public class ExtensionsTest {

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("a", true)]
    [InlineData("a b", true)]
    public void stringHasText(string? str, bool expected) {
        str.HasText().Should().Be(expected);
    }

}
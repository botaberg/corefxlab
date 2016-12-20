using Xunit;

namespace System.Text.Primitives.Tests
{
    public partial class PrimitiveParserTests
    {
        [Theory]
        // TODO: Add test cases with invalid Uri strings
        // default(Uri) is null, so parsedValue.ToString() will throw NullReferenceException
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier", true, "https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier", 73)]
        public unsafe void ParseUriInvariant(string text, bool expectSuccess, string expectedValue, int expectedConsumed)
        {
            Uri parsedValue;
            int consumed;
            ReadOnlySpan<byte> utf8Span = UtfEncode(text, false);
            ReadOnlySpan<char> utf16Span = new ReadOnlySpan<byte>(UtfEncode(text, true)).Cast<byte, char>();
            byte[] textBytes = utf8Span.ToArray();
            char[] textChars = utf16Span.ToArray();
            bool result;

            result = PrimitiveParser.InvariantUtf8.TryParseUri(utf8Span, out parsedValue);
            Assert.Equal(expectSuccess, result);
            Assert.Equal(expectedValue, parsedValue.ToString());

            result = PrimitiveParser.InvariantUtf8.TryParseUri(utf8Span, out parsedValue, out consumed);
            Assert.Equal(expectSuccess, result);
            Assert.Equal(expectedValue, parsedValue.ToString());
            Assert.Equal(expectedConsumed, consumed);

            fixed (byte* arrayPointer = textBytes)
            {
                result = PrimitiveParser.InvariantUtf8.TryParseUri(arrayPointer, textBytes.Length, out parsedValue);

                Assert.Equal(expectSuccess, result);
                Assert.Equal(expectedValue, parsedValue.ToString());

                result = PrimitiveParser.InvariantUtf8.TryParseUri(arrayPointer, textBytes.Length, out parsedValue, out consumed);
                Assert.Equal(expectSuccess, result);
                Assert.Equal(expectedValue, parsedValue.ToString());
                Assert.Equal(expectedConsumed, consumed);
            }

            result = PrimitiveParser.InvariantUtf16.TryParseUri(utf16Span, out parsedValue);
            Assert.Equal(expectSuccess, result);
            Assert.Equal(expectedValue, parsedValue.ToString());

            result = PrimitiveParser.InvariantUtf16.TryParseUri(utf16Span, out parsedValue, out consumed);
            Assert.Equal(expectSuccess, result);
            Assert.Equal(expectedValue, parsedValue.ToString());
            Assert.Equal(expectedConsumed, consumed);

            fixed (char* arrayPointer = textChars)
            {
                result = PrimitiveParser.InvariantUtf16.TryParseUri(arrayPointer, textBytes.Length, out parsedValue);
                Assert.Equal(expectSuccess, result);
                Assert.Equal(expectedValue, parsedValue.ToString());

                result = PrimitiveParser.InvariantUtf16.TryParseUri(arrayPointer, textBytes.Length, out parsedValue, out consumed);
                Assert.Equal(expectSuccess, result);
                Assert.Equal(expectedValue, parsedValue.ToString());
                Assert.Equal(expectedConsumed, consumed);
            }
        }
    }
}
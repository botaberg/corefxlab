using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Xunit;
using Microsoft.Xunit.Performance;
using System.Text.Internal;

namespace System.Text.Primitives.Tests
{
    public partial class PrimitiveParserPerfTests
    {
        [Benchmark]
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier")]
        private static void BaselineStringToUri(string text)
        {
            Uri uri;
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < LoadIterations; i++)
                    {
                        Uri.TryCreate(text, UriKind.Absolute, out uri);
                    }
                }
            }
        }

        [Benchmark]
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier")]
        private static void InternalParserByteSpanToUri(string text)
        {
            Uri uri;
            int bytesConsumed;
            byte[] utf8ByteArray = Encoding.UTF8.GetBytes(text);
            ReadOnlySpan<byte> utf8ByteSpan = new ReadOnlySpan<byte>(utf8ByteArray);
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < LoadIterations; i++)
                    {
                        InternalParser.TryParseUri(utf8ByteSpan, out uri, out bytesConsumed);
                    }
                }
            }
        }

        [Benchmark]
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier")]
        private unsafe static void InternalParserByteStarToUri(string text)
        {
            Uri uri;
            int bytesConsumed;
            byte[] utf8ByteArray = Encoding.UTF8.GetBytes(text);
            foreach (var iteration in Benchmark.Iterations)
            {
                fixed (byte* utf8ByteStar = utf8ByteArray)
                {
                    using (iteration.StartMeasurement())
                    {
                        for (int i = 0; i < LoadIterations; i++)
                        {
                            InternalParser.TryParseUri(utf8ByteStar, 0, utf8ByteArray.Length, out uri, out bytesConsumed);
                        }
                    }
                }
            }
        }

        [Benchmark]
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier")]
        private static void PrimitiveParserByteSpanToUri(string text)
        {
            Uri uri;
            int bytesConsumed;
            byte[] utf8ByteArray = Encoding.UTF8.GetBytes(text);
            ReadOnlySpan<byte> utf8ByteSpan = new ReadOnlySpan<byte>(utf8ByteArray);
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < LoadIterations; i++)
                    {
                        PrimitiveParser.InvariantUtf8.TryParseUri(utf8ByteSpan, out uri, out bytesConsumed);
                    }
                }
            }
        }

        [Benchmark]
        [InlineData("https://my.uri.djkadjald.org/wiki/This_is_a_Universal#Resource_Identifier")]
        private unsafe static void PrimitiveParserByteStarToUri(string text)
        {
            Uri uri;
            int bytesConsumed;
            byte[] utf8ByteArray = Encoding.UTF8.GetBytes(text);
            foreach (var iteration in Benchmark.Iterations)
            {
                fixed (byte* utf8ByteStar = utf8ByteArray)
                {
                    using (iteration.StartMeasurement())
                    {
                        for (int i = 0; i < LoadIterations; i++)
                        {
                            PrimitiveParser.InvariantUtf8.TryParseUri(utf8ByteStar, utf8ByteArray.Length, out uri, out bytesConsumed);
                        }
                    }
                }
            }
        }
    }
}

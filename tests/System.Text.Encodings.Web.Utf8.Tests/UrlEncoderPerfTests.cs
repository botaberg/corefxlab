using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Xunit.Performance;

namespace System.Text.Encodings.Web.Utf8.Tests
{
    public class UrlEncoderPerfTests
    {
        [Benchmark(InnerIterationCount = 10000)]
        [InlineData("%D0%A4", "Ф")]
        [InlineData("%d0%a4", "Ф")]
        [InlineData("%E0%A4%AD", "भ")]
        [InlineData("%e0%A4%Ad", "भ")]
        [InlineData("%F0%A4%AD%A2", "𤭢")]
        [InlineData("%F0%a4%Ad%a2", "𤭢")]
        [InlineData("%48%65%6C%6C%6F%20%57%6F%72%6C%64", "Hello World")]
        [InlineData("%48%65%6C%6C%6F%2D%C2%B5%40%C3%9F%C3%B6%C3%A4%C3%BC%C3%A0%C3%A1", "Hello-µ@ßöäüàá")]
        [InlineData("%C3%84ra%20Benetton", "Ära Benetton")]
        [InlineData("%E6%88%91%E8%87%AA%E6%A8%AA%E5%88%80%E5%90%91%E5%A4%A9%E7%AC%91%E5%8E%BB%E7%95%99%E8%82%9D%E8%83%86%E4%B8%A4%E6%98%86%E4%BB%91", "我自横刀向天笑去留肝胆两昆仑")]
        private static void TestDecode(string raw, string expected)
        {
            var input = GetBytes(raw);
            var destination = new Span<byte>(new byte[input.Length]);

            int len = 0;
            foreach (var iteration in Benchmark.Iterations)
            {
                using (iteration.StartMeasurement())
                {
                    for (int i = 0; i < Benchmark.InnerIterationCount; i++)
                    {
                        len = UrlEncoder.Decode(input, destination);
                    }
                }
            }

            Assert.True(len <= input.Length);

            var unescaped = destination.Slice(0, len);
            Assert.False(unescaped == input.Slice(0, len));

            var outputDecoded = Encoding.UTF8.GetString(unescaped.ToArray());
            Assert.Equal(expected, outputDecoded);
        }

        static Span<byte> GetBytes(string sample) =>
            new Span<byte>(sample.Select(c => (byte)c).ToArray());
    }
}

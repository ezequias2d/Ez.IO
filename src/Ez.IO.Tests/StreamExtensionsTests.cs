using System;
using System.IO;
using Xunit;

namespace Ez.IO.Tests
{
    public class StreamExtensionsTests
    {

        [Fact]
        public void WriteAndReadStringTest1()
        {
            using var stream = new MemoryStream();
            var str = "This is a string sample!";

            stream.WriteString(str);

            using var otherStream = new MemoryStream();
            stream.WriteTo(otherStream);

            otherStream.Position = 0;
            var result = otherStream.ReadString();

            Assert.Equal(str, result);
        }

        [Fact]
        public void CopyToTest1()
        {
            using var stream = new MemoryStream();
            var array = new byte[32];
            (new Random(123)).NextBytes(array);

            stream.Write(array);

            using var otherStream = new MemoryStream();

            var toRead = 16L;
            stream.Position = 0;
            stream.CopyTo(otherStream, toRead);

            var result = otherStream.ToArray();
            Assert.Equal(toRead, result.LongLength);
            for (var i = 0; i < toRead; i++)
                Assert.Equal(array[i], result[i]);
        }

        [Fact]
        public void WriteAndReadStructTest1()
        {
            using var stream = new MemoryStream();
            var value = new SampleStructure
            {
                Value1 = int.MinValue,
                Value2 = int.MaxValue,
                Value3 = 0,
            };

            stream.WriteStructure(value);

            using var otherStream = new MemoryStream();
            stream.WriteTo(otherStream);
            otherStream.Position = 0;

            var result = otherStream.ReadStructure<SampleStructure>();

            Assert.Equal(value, result);
        }

        private struct SampleStructure : IEquatable<SampleStructure>
        {
            public int Value1;
            public int Value2;
            public int Value3;

            public override bool Equals(object obj) =>
                obj is SampleStructure ss && Equals(ss);

            public bool Equals(SampleStructure other) =>
                Value1 == other.Value1 &&
                Value2 == other.Value2 &&
                Value3 == other.Value3;

            public override int GetHashCode() => HashCode.Combine(Value1, Value2, Value3);

            public override string ToString() => $"[{Value1}, {Value2}, {Value3}]";
        }
    }
}

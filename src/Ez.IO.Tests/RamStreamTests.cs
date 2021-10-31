using Ez.Memory;
using System;
using Xunit;

namespace Ez.IO.Tests
{
    public class RamStreamTests
    {

        [Fact]
        public void RamStreamWriteAndReadTest1()
        {
            using var ms2 = new RamStream();

            // load random data
            var bytArr = new byte[256];
            new Random(123).NextBytes(bytArr);

            // writes
            ms2.Write(bytArr, 0, bytArr.Length);

            // copy to another stream
            using var otherStream = new RamStream();
            ms2.Position = 0;
            ms2.CopyTo(otherStream);

            otherStream.Flush();
            otherStream.Position = 0;
            var bytArrRet = new byte[(int)otherStream.Length];

            // reads from stream
            otherStream.Read(bytArrRet, 0, (int)otherStream.Length);

            // compare with original stream
            for (int i = 0; i < bytArr.Length; i++)
                Assert.Equal(bytArr[i], bytArrRet[i]);
        }

        [Fact]
        public void RamStreamRead()
        {
            using var stream = new RamStream();
            // load random data
            var bytArr = new byte[256];
            new Random(123).NextBytes(bytArr);
            stream.Write(bytArr);
            stream.Position = 0;

            var tmp = new byte[127];
            stream.Read(tmp, 0, 15);

            Assert.True(MemUtil.Equals<byte>(new Span<byte>(tmp, 0, 15), new Span<byte>(bytArr, 0, 15)));
        }
    }
}

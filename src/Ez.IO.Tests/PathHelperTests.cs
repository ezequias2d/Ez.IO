using System.IO;
using Xunit;

namespace Ez.IO.Tests
{
    public class PathHelperTests
    {
        private static string[] GetPaths() => new [] { "home", "user", "folder", "file.txt" };
        
        [Fact]
        public void SeparatePathTest1()
        {
            var paths = GetPaths();
            var fullPath = Path.Combine(paths);
            var separatePath = PathHelper.SeparatePath(fullPath);

            Assert.Equal(paths.Length, separatePath.Length);
            for (var i = 0; i < paths.Length; i++)
                Assert.Equal(paths[i], separatePath[i]);
        }

        [Fact]
        public void GetFolderNameTest1()
        {
            var paths = GetPaths();
            var fullPath = Path.Combine(paths);

            var folderName = PathHelper.GetFolderName(fullPath);
            Assert.Equal(paths[paths.Length - 2], folderName);
        }

    }
}

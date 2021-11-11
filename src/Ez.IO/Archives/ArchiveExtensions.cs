using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.IO.Archives
{
    public static class ArchiveExtensions
    {
        public static IEnumerable<string> GetFiles(this IArchive archive, string path)
        {
            foreach(var entryName in archive.EntryNames)
            {
                if (path.Equals(Path.GetDirectoryName(entryName), StringComparison.InvariantCulture)) 
                {
                    var relative = entryName.Substring(path.Length);
                    var paths = PathHelper.SeparatePath(relative);

                    if (paths.Length == 1)
                    {
                        yield return entryName;
                    }
                }

            }
        }

        public static IEnumerable<string> GetDirectories(this IArchive archive, string path)
        {
            var set = new HashSet<string>();
            foreach (var entryName in archive.EntryNames)
            {
                if (string.Compare(path, 0, Path.GetDirectoryName(entryName), 0, path.Length, false) == 0)
                {
                    var relative = entryName.Substring(path.Length);
                    var paths = PathHelper.SeparatePath(relative);

                    if(paths.Length > 1)
                        set.Add(paths[0]);
                }
            }

            return set;
        }
    }
}

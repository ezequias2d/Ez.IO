using System;
using System.IO;

using SZipArchiveEntry = System.IO.Compression.ZipArchiveEntry;
namespace Ez.IO.Archives.Zip
{
    internal class ZipArchiveEntry : IArchiveEntry
    {
        private SZipArchiveEntry _entry;
        private ZipArchive _archive;
        public ZipArchiveEntry(ZipArchive archive, SZipArchiveEntry entry)
        {
            _entry = entry;
            _archive = archive;
        }

        public IArchive Archive => _archive;

        public long Length => _entry.Length;

        public string Name => _entry.Name;

        public string FullName => _entry.FullName;

        public DateTimeOffset LastWriteTime { get => _entry.LastWriteTime; set => _entry.LastWriteTime = value; }

        public bool IsDisposed { get; private set; }

        public long CompressedLength => _entry.CompressedLength;

        public void Delete()
        {
            Dispose();
            _entry.Delete();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            
            _archive.Remove(this);
            _entry = null;
            _archive = null;
            IsDisposed = true;
        }

        public Stream Open() => _entry.Open();
    }
}

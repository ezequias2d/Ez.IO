using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SZipArchive = System.IO.Compression.ZipArchive;

namespace Ez.IO.Archives.Zip
{
    public class ZipArchive : IArchive
    {
        private readonly SZipArchive _archive;
        private Dictionary<string, ZipArchiveEntry> _entries;

        public ZipArchive(Stream stream, ArchiveMode archiveMode, bool leaveOpen, Encoding? entryNameEncoding)
        {
            _archive = new(stream, ToSystem(archiveMode), leaveOpen, entryNameEncoding);
            _entries = new Dictionary<string, ZipArchiveEntry>();

            if(archiveMode != ArchiveMode.Create)
            {
                foreach (var entry in _archive.Entries)
                {
                    _entries.Add(entry.FullName, new ZipArchiveEntry(this, entry));
                }
            }
        }

        public ArchiveMode Mode => _archive.Mode switch
        {
            System.IO.Compression.ZipArchiveMode.Read => ArchiveMode.Read,
            System.IO.Compression.ZipArchiveMode.Create => ArchiveMode.Create,
            System.IO.Compression.ZipArchiveMode.Update => ArchiveMode.Update,
            _ => throw new NotImplementedException(),
        };

        public IReadOnlyCollection<string> EntryNames => _entries.Keys;

        public bool IsDisposed { get; private set; }

        public IArchiveEntry CreateEntry(string entryName)
        {
            var entry = new ZipArchiveEntry(this, _archive.CreateEntry(entryName));
            Add(entry);
            return entry;
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            IsDisposed = true;

            if (disposing)
            {
                var entries = new ZipArchiveEntry[_entries.Count];
                _entries.Values.CopyTo(entries, 0);
                foreach (var entry in entries)
                    entry.Dispose();

                _archive.Dispose();
            }
            _entries.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IArchiveEntry GetEntry(string entryName)
        {
            if (_entries.TryGetValue(entryName, out var entry))
                return entry;
            
            var sEntry = _archive.GetEntry(entryName);
            if(sEntry is not null)
            {
                entry = new ZipArchiveEntry(this, sEntry);
                Add(entry);
                return entry;
            }

            return null;
        }

        internal void Add(ZipArchiveEntry entry) => _entries.Add(entry.FullName, entry);

        internal bool Remove(ZipArchiveEntry entry) => _entries.Remove(entry.FullName);

        private static System.IO.Compression.ZipArchiveMode ToSystem(ArchiveMode archiveMode) => archiveMode switch
        {
            ArchiveMode.Read => System.IO.Compression.ZipArchiveMode.Read,
            ArchiveMode.Create => System.IO.Compression.ZipArchiveMode.Create,
            ArchiveMode.Update => System.IO.Compression.ZipArchiveMode.Update,
            _ => throw new NotImplementedException(),
        };
    }
}

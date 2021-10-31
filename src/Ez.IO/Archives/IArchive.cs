using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using System.Text;

namespace Ez.IO.Archives
{
    /// <summary>
    /// Represents a package of resources.
    /// </summary>
    public interface IArchive : IDisposable
    {
        /// <summary>
        /// Gets a value that describes the type of action the zip archive can perform on entries.
        /// </summary>
        ArchiveMode Mode { get; }

        /// <summary>
        /// Gets the collection of entries that are currently in the archive.
        /// </summary>
        /// <exception cref="NotSupportedException">The archive does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
        /// <exception cref="InvalidDataException">The archive is corrupt, and its entries cannot be retrieved.</exception>
        IReadOnlyCollection<string> EntryNames { get; }

        /// <summary>
        /// Creates an empty entry that has the specified path and entry name in the archive.
        /// </summary>
        /// <param name="entryName">A path, relative to the root of the archive, that specifies the name
        /// of the entry to be created.</param>
        /// <returns>An empty <see cref="IArchiveEntry"/> in the archive.</returns>
        /// <exception cref="ArgumentException"><paramref name="entryName"/> is <see cref="string.Empty"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entryName"/> is null.</exception>
        /// <exception cref="NotSupportedException">The archive does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
        IArchiveEntry CreateEntry(string entryName);

        /// <summary>
        /// Retrieves a wrapper for the specified entry in the archive.
        /// </summary>
        /// <param name="entryName">A path, relative to the root of the archive, that identifies the entry
        /// to retrieve.</param>
        /// <returns>A wrapper for the specified entry in the archive; null if the entry does not exist in 
        /// the archive.</returns>
        /// <exception cref="ArgumentException"><paramref name="entryName"/> is <see cref="string.Empty"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="entryName"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The archive does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">The archive has been disposed.</exception>
        /// <exception cref="InvalidDataException">The archive is corrupt, and its entries cannot be retrieved.</exception>
        IArchiveEntry GetEntry(string entryName);
    }
}

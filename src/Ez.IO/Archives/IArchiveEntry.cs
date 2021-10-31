using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ez.IO.Archives
{
    /// <summary>
    /// Represents a file within a <see cref="IArchive"/>.
    /// </summary>
    public interface IArchiveEntry
    {
        /// <summary>
        /// Gets the <see cref="IArchive"/> that the entry belongs to.
        /// </summary>
        IArchive Archive { get; }

        /// <summary>
        /// Gets the size of the entry in the <see cref="IArchive"/>.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// The 32-bit Cyclic Redundant Check.
        /// </summary>
        uint Crc32C { get; }

        /// <summary>
        /// Gets the file name of the entry in the <see cref="IArchive"/>.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the relative path of the entry in the <see cref="IArchive"/>.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets or sets the last time the entry in the archive was changed.
        /// </summary>      
        /// <value>The last time the entry in the zip archive was changed.</value>
        /// <exception cref="NotSupportedException">The attempt to set this property failed,
        /// because the archive for the entry is in <see cref="ArchiveMode.Read"/> mode.</exception>
        /// <exception cref="IOException">The archive mode is set to <see cref="ArchiveMode.Update"/>
        /// and the entry has been opened.</exception>
        DateTimeOffset LastWriteTime { get; set; }

        /// <summary>
        /// Opens the entry from the zip archive.
        /// </summary>
        /// <returns>The stream that represents the contents of the entry.</returns>
        /// <exception cref="IOException">Thrown when the entry is already currently open for writing.</exception>
        /// <exception cref="IOException">The entry has been deleted from the archive.</exception>
        /// <exception cref="IOException">The archive for this entry was opened with the 
        /// <see cref="ArchiveMode.Create"/> mode, and this entry has already been written to.</exception>
        /// <exception cref="InvalidDataException">The entry is either missing from the archive or is
        /// corrupt and cannot be read.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="IArchive"/> for this entry has been
        /// disposed.</exception>
        Stream Open();

        /// <summary>
        /// Deletes the entry from the archive.
        /// </summary>
        /// <exception cref="IOException">The entry is already open for reading or writing.</exception>
        /// <exception cref="NotSupportedException">The <see cref="IArchive"/> for this entry was opened in a mode 
        /// other than <see cref="ArchiveMode.Update"/>.</exception>
        /// <exception cref="ObjectDisposedException">The <see cref="IArchive"/> for this entry has been 
        /// disposed.</exception>
        void Delete();

        /// <summary>
        /// The relative path of the entry, which is the value stored in the <see cref="FullName"/> property.
        /// </summary>
        string ToString();
    }
}

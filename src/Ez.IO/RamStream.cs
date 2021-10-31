using Ez.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ez.IO
{
    /// <summary>
    /// Creates a stream whose backing store is a <see cref="MemoryBlock"/>.
    /// </summary>
    public sealed class RamStream : Stream, IDisposable
    {
        private MemoryBlock _memoryBlock;
        private long _capacity;
        private long _length;
        private long _position;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RamStream"/> class with an expandable capacity initialized to zero.
        /// </summary>
        public RamStream()
        {
            _capacity = 0;
            _length = 0;
            _position = 0;
            _disposed = false;
            _memoryBlock = MemoryBlock.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RamStream"/> class with an expandable capacity initialized as specified.
        /// </summary>
        /// <param name="capacity">The initial size of the internal <see cref="MemoryBlock"/> in bytes. (<seealso cref="MemUtil.MaxAllocSize"/>)</param>
        public RamStream(long capacity) : this()
        {
            Capacity = capacity;
        }

        /// <summary>
        /// Gets or sets the number of bytes allocated for this <see cref="RamStream"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">A capacity is set that is negative or less 
        /// than the current length of the stream.</exception>
        /// <exception cref="ObjectDisposedException">The current stream is closed.</exception>
        public long Capacity
        {
            get
            {
                EnsureNotClosed();
                return _capacity;
            }
            set
            {
                if (value < Length || value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                EnsureNotClosed();
                
                if (value != _capacity)
                {
                    if(value > 0)
                    {
                        MemoryBlock newMemoryBlock = new(value);

                        if(_length > 0)
                            MemUtil.Copy(newMemoryBlock.Ptr, _memoryBlock.Ptr, _length);

                        if (_memoryBlock != MemoryBlock.Empty)
                            _memoryBlock.Dispose();
                        _memoryBlock = newMemoryBlock;
                    }
                    else
                        _memoryBlock = MemoryBlock.Empty;
                    
                    _capacity = _memoryBlock.TotalSize;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => !_disposed;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => !_disposed;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => !_disposed;

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override long Length
        {
            get
            {
                EnsureNotClosed();
                return _length;
            }
        }

        /// <summary>
        /// Gets or sets the current position within the stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The position is set to a negative value 
        /// or a value greater than <see cref="MemUtil.MaxAllocSize"/>.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override long Position 
        {
            get
            {
                EnsureNotClosed();
                return _position;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                EnsureNotClosed();

                if (value > MemUtil.MaxAllocSize)
                    throw new ArgumentOutOfRangeException($"The {nameof(value)} exceeds the maximum allocation.");

                _position = value;
            }
        }

        /// <summary>
        /// Gets an EphemeralMemoryBlock for the internal <see cref="MemoryBlock"/>.
        /// </summary>
        public EphemeralMemoryBlock EphemeralMemoryBlock => _memoryBlock;

        /// <summary>
        /// Overrides the <see cref="Stream.Flush"/> method so that no action is performed.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous flush operation.</returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            try
            {
                Flush();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to a buffer.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array
        /// with the values between <paramref name="offset"/> and (<paramref name="offset"/> + 
        /// <paramref name="count"/> - 1) replaced by the characters read from the current 
        /// stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing data from the current stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes written into the buffer. This can be less than 
        /// the number of bytes requested if that number of bytes are not currently available, 
        /// or zero if the end of the stream is reached before any bytes are read.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is 
        /// <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or 
        /// <paramref name="count"/> is negative.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> subtracted from the 
        /// buffer length is less than <paramref name="count"/>.</exception>
        /// <exception cref="ObjectDisposedException">The current stream is closed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            EnsureNotClosed();

            int toRead = (int)Math.Min(_length - _position, count);

            if (toRead <= 0)
                return 0;
            
            MemUtil.Copy(new Span<byte>(buffer, 0, toRead), MemUtil.Add(_memoryBlock.Ptr, _position));

            _position += toRead;

            return toRead;
        }

        /// <summary>
        /// Sets the position within the current stream to the specified value.
        /// </summary>
        /// <param name="offset">The new position within the stream. This is 
        /// relative to the <paramref name="origin"/> parameter, and can be
        /// positive or negative.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/>, which
        /// acts as the seek reference point.</param>
        /// <returns>The new position within the stream, calculated by combining 
        /// the initial reference point and the offset.</returns>
        /// <exception cref="IOException">Seeking is attempted before the beginning 
        /// of the stream.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> 
        /// is greater than <see cref="MemUtil.MaxAllocSize"/>.</exception>
        /// <exception cref="ArgumentException">There is an invalid <see cref="SeekOrigin"/>
        /// or <paramref name="offset"/> caused an arithmetic overflow.</exception>
        /// <exception cref="ObjectDisposedException">The current stream is closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotClosed();

            if (offset > MemUtil.MaxAllocSize)
                throw new ArgumentOutOfRangeException(nameof(offset));

            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset < 0)
                            throw SeekBeforeBegin();
                        _position = offset;
                    }
                    break;
                case SeekOrigin.Current:
                    {
                        if(unchecked(_position + offset) < 0)
                            throw SeekBeforeBegin();
                        _position = offset;
                    }
                    break;
                case SeekOrigin.End:
                    {
                        long aux = unchecked(_length + offset);
                        if (aux < 0)
                            throw SeekBeforeBegin();
                        _position = aux;
                    }
                    break;
                default:
                    throw new ArgumentException(nameof(origin));
            }

            return _position;
        }

        /// <summary>
        /// Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="value">The value at which to set the length.</param>
        /// <exception cref="ObjectDisposedException">The current stream is closed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative 
        /// or is greater than <see cref="MemUtil.MaxAllocSize"/>.</exception>
        public override void SetLength(long value)
        {
            if (value < 0 || value > MemUtil.MaxAllocSize)
                throw new ArgumentOutOfRangeException(nameof(value));

            EnsureNotClosed();

            bool allocatedNewArray = EnsureCapacity(value);
            if(!allocatedNewArray && value > _length)
                MemUtil.Set(MemUtil.Add(_memoryBlock.Ptr, _length), 0, value - _length);
                

            _length = value;
            if (_position > value)
                _position = value;
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from a buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at 
        /// which to begin copying bytes to the current stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <exception cref="ObjectDisposedException">The current stream is closed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is 
        /// <see langword="null"/></exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> 
        /// is greater than the buffer length.</exception>        
        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count);
            EnsureNotClosed();

            var size = _position + count;

            if (size > _length)
            {
                EnsureCapacity(size);
                _length = size;
            }

            MemUtil.Copy(MemUtil.Add(_memoryBlock.Ptr, _position), new ReadOnlySpan<byte>(buffer, offset, count));
            
            _position = size;
        }

        private bool EnsureCapacity(long value)
        {
            if (value < 0)
                throw new IOException("The stream is too long.");

            if(value > _capacity)
            {
                var newCapacity = Math.Max(value, 1024);

                if (newCapacity < _capacity * 2)
                    newCapacity = _capacity * 2;

                if (_capacity * 2 > MemUtil.MaxAllocSize)
                    newCapacity = Math.Max(value, MemUtil.MaxAllocSize);

                Capacity = newCapacity;
                return true;
            }
            return false;
        }

        private void EnsureNotClosed()
        {
            if (_disposed)
                throw new ObjectDisposedException("Thre stream is closed");
        }

        private static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), $"the parameter {nameof(offset)} must be non-negative.");

            if ((uint)count > buffer.Length - offset)
                throw new ArgumentException("buffer is too tinny.", nameof(count));
        }

        private static IOException SeekBeforeBegin()
        {
            return new IOException("An attempt was made to move the position before the beginning of the stream.");
        }

        /// <summary>
        /// Release resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and
        /// unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_memoryBlock != MemoryBlock.Empty)
                    _memoryBlock.Dispose();
                _memoryBlock = null;
                _disposed = true;
            }
        }
    }
}

// Copyright (c) 2021 ezequias2d <ezequiasmoises@gmail.com> and the Ez contributors
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
using Ez.Memory;
using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Ez.IO
{
    /// <summary>
    /// <see cref="Stream"/> extensions.
    /// </summary>
    public static class StreamExtensions
    {

        /// <summary>
        /// Write a string in the stream that ends with 0(byte). 
        /// 
        /// The string is written with utf8 encoding.
        /// <seealso cref="ReadString(Stream)"/>
        /// </summary>
        /// <param name="stream">Stream to write.</param>
        /// <param name="value">String to be written.</param>
        public static void WriteString(this Stream stream, string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            stream.Write(data, 0, data.Length);
            stream.WriteByte(0);
        }

        /// <summary>
        /// Reads a string from the stream that ends with a 0(byte).
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>The string readed.</returns>
        public static string ReadString(this Stream stream)
        {
            byte[] data;
            using (var bufferStream = new MemoryStream(65536))
            {
                {
                    int aux;
                    while ((aux = stream.ReadByte()) != -1 && aux != 0)
                        bufferStream.WriteByte((byte)aux);
                }
                data = bufferStream.ToArray();
            }

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Write a <see cref="ReadOnlySpan{T}"/> array data in the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of items in the <paramref name="array"/>.</typeparam>
        /// <param name="stream">The stream to write.</param>
        /// <param name="array">The span to be written.</param>
        public static void WriteSpan<T>(this Stream stream, ReadOnlySpan<T> array) where T : unmanaged
        {
            if (array == null || array.Length == 0)
                return;

            var source = MemUtil.Cast<T, byte>(array);
            stream.Write(source);
        }

        /// <summary>
        /// Read a <see cref="ReadOnlySpan{T}"/> from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of items in the returned <see cref="ReadOnlySpan{T}"/>.</typeparam>
        /// <param name="stream">The stream to read.</param>
        /// <param name="count">The count of T items to read.</param>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> with items read from the <paramref name="stream"/>.</returns>
        public static ReadOnlySpan<T> ReadSpan<T>(this Stream stream, uint count) where T : unmanaged
        {
            if (count == 0)
                return Array.Empty<T>();

            var array = MemUtil.Cast<byte, T>(ArrayPool<byte>.Shared.Rent((int)(count * MemUtil.SizeOf<T>()))).Slice(0, (int)count);

            byte[] buffer = new byte[count * MemUtil.SizeOf<T>()];
            stream.Read(buffer, 0, buffer.Length);

            MemUtil.Copy<T, byte>(array, buffer);

            return array;
        }

        /// <summary>
        /// Write a T structure data in the stream.
        /// </summary>
        /// <typeparam name="T">The type of the structure to write.</typeparam>
        /// <param name="stream">The stream to write.</param>
        /// <param name="value">The T structure to written.</param>
        public static void WriteStructure<T>(this Stream stream, T value) where T : unmanaged
        {
            var buffer = new byte[MemUtil.SizeOf<T>()];
            var span = MemUtil.Cast<byte, T>(buffer);
            span[0] = value;
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Read a T structure data from the stream.
        /// </summary>
        /// <typeparam name="T">The type of the structure to read.</typeparam>
        /// <param name="stream">The stream to read.</param>
        /// <returns>A structure read from the stream.</returns>
        public static T ReadStructure<T>(this Stream stream) where T : unmanaged
        {
            byte[] buffer = new byte[MemUtil.SizeOf<T>()];
            stream.Read(buffer, 0, buffer.Length);
            var span = MemUtil.Cast<byte, T>(buffer);
            return span[0];
        }

        /// <summary>
        /// Reads bytes from current stream and writes them to another stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <param name="count">The number of bytes to copy.</param>
        /// <param name="bufferSize">The size of the buffer. This value must be 
        /// greater than zero. The default size is 1048576.</param>
        /// <returns>The number of bytes copied.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> or
        /// <paramref name="source"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">When <paramref name="source"/> cannot be read or <paramref name="destination"/> cannot be written.</exception>
        public static long CopyTo(this Stream source, Stream destination, long count, int bufferSize = 1048576)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!source.CanRead)
                throw new NotSupportedException("The current stream does not support reading.");
            if (!destination.CanWrite)
                throw new NotSupportedException("The destination does not support writing.");

            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            long copied = 0;
            int readed;
            bufferSize = (int)Math.Min(buffer.Length, count);
            while(copied < count && (readed = source.Read(buffer, 0, bufferSize)) != 0)
            {
                copied += readed;
                destination.Write(buffer, 0, readed);
            }
            ArrayPool<byte>.Shared.Return(buffer);

            return copied;
        }
    }
}

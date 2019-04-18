﻿/*
   Derived from zlib header files (zlib license)
   Copyright (C) 1995-2017 Jean-loup Gailly and Mark Adler

   C# Wrapper based on zlibnet v1.3.3 (https://zlibnet.codeplex.com/)
   Copyright (C) @hardon (https://www.codeplex.com/site/users/view/hardon)
   
   Maintained by Hajin Jang
   Copyright (C) 2017-2019 Hajin Jang

   zlib license

   This software is provided 'as-is', without any express or implied
   warranty.  In no event will the authors be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.
   2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.
   3. This notice may not be removed or altered from any source distribution.
*/

using System;
using System.IO;
// ReSharper disable UnusedMember.Global

namespace Joveler.Compression.ZLib
{
    #region Crc32Stream
    public class Crc32Stream : Stream
    {
        #region Fields and Properties
        public uint Checksum { get; private set; } = Crc32Checksum.InitChecksum;
        public Stream BaseStream { get; }
        #endregion

        #region Constructor
        public Crc32Stream(Stream stream)
        {
            NativeMethods.CheckZLibLoaded();
            BaseStream = stream;
        }
        #endregion

        #region Stream Methods
        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = BaseStream.Read(buffer, offset, count);
            fixed (byte* bufPtr = buffer.AsSpan(offset, count))
            {
                Checksum = NativeMethods.Crc32(Checksum, bufPtr, (uint)bytesRead);
            }
            return bytesRead;
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            fixed (byte* bufPtr = buffer.AsSpan(offset, count))
            {
                Checksum = NativeMethods.Crc32(Checksum, bufPtr, (uint)count);
            }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanWrite => BaseStream.CanWrite;
        public override bool CanSeek => BaseStream.CanSeek;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
        #endregion
    }
    #endregion

    #region Adler32Stream
    public class Adler32Stream : Stream
    {
        #region Fields and Properties
        public uint Checksum { get; private set; } = Adler32Checksum.InitChecksum;
        public Stream BaseStream { get; }
        #endregion

        #region Constructor
        public Adler32Stream(Stream stream)
        {
            NativeMethods.CheckZLibLoaded();
            BaseStream = stream;
        }
        #endregion

        #region Stream Methods
        public override unsafe int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = BaseStream.Read(buffer, offset, count);
            fixed (byte* bufPtr = buffer.AsSpan(offset, count))
            {
                Checksum = NativeMethods.Adler32(Checksum, bufPtr, (uint)bytesRead);
            }
            return bytesRead;
        }

        public override unsafe void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);
            fixed (byte* bufPtr = buffer.AsSpan(offset, count))
            {
                Checksum = NativeMethods.Adler32(Checksum, bufPtr, (uint)count);
            }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanWrite => BaseStream.CanWrite;
        public override bool CanSeek => BaseStream.CanSeek;

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override long Length => BaseStream.Length;

        public override long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }
        #endregion
    }
    #endregion

    #region Crc32Checksum
    public class Crc32Checksum
    {
        #region Fields and Properties
        internal const uint InitChecksum = 0;
        public uint Checksum { get; private set; }
        #endregion

        #region Constructor
        public Crc32Checksum()
        {
            NativeMethods.CheckZLibLoaded();
            Reset();
        }
        #endregion

        #region Append, Reset
        public uint Append(byte[] buffer, int offset, int count)
        {
            Checksum = Crc32(Checksum, buffer, offset, count);
            return Checksum;
        }

        public uint Append(ReadOnlySpan<byte> span)
        {
            Checksum = Crc32(Checksum, span);
            return Checksum;
        }

        public uint Append(Stream stream)
        {
            byte[] buffer = new byte[NativeMethods.BufferSize];
            while (stream.Position < stream.Length)
            {
                int readByte = stream.Read(buffer, 0, NativeMethods.BufferSize);
                Checksum = Crc32(Checksum, buffer, 0, readByte);
            }
            return Checksum;
        }

        public void Reset()
        {
            Checksum = InitChecksum;
        }
        #endregion

        #region zlib crc32 Wrapper
        public static unsafe uint Crc32(byte[] buffer, int offset, int count)
        {
            NativeMethods.CheckZLibLoaded();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            fixed (byte* bufPtr = buffer.AsSpan(offset))
            {
                return NativeMethods.Crc32(InitChecksum, bufPtr, (uint)count);
            }
        }

        public static unsafe uint Crc32(ReadOnlySpan<byte> span)
        {
            NativeMethods.CheckZLibLoaded();

            fixed (byte* bufPtr = span)
            {
                return NativeMethods.Crc32(InitChecksum, bufPtr, (uint)span.Length);
            }
        }

        public static uint Crc32(Stream stream)
        {
            uint checksum = InitChecksum;

            byte[] buffer = new byte[NativeMethods.BufferSize];
            int readByte;
            do
            {
                readByte = stream.Read(buffer, 0, buffer.Length);
                checksum = Crc32(checksum, buffer, 0, readByte);
            }
            while (0 < readByte);

            return checksum;
        }

        public static unsafe uint Crc32(uint checksum, byte[] buffer, int offset, int count)
        {
            NativeMethods.CheckZLibLoaded();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);
            fixed (byte* bufPtr = buffer.AsSpan(offset))
            {
                return NativeMethods.Crc32(checksum, bufPtr, (uint)count);
            }
        }

        public static unsafe uint Crc32(uint checksum, ReadOnlySpan<byte> span)
        {
            NativeMethods.CheckZLibLoaded();

            fixed (byte* bufPtr = span)
            {
                return NativeMethods.Crc32(checksum, bufPtr, (uint)span.Length);
            }
        }

        public static uint Crc32(uint checksum, Stream stream)
        {
            byte[] buffer = new byte[NativeMethods.BufferSize];
            int readByte;
            do
            {
                readByte = stream.Read(buffer, 0, buffer.Length);
                checksum = Crc32(checksum, buffer, 0, readByte);
            }
            while (0 < readByte);

            return checksum;
        }
        #endregion
    }
    #endregion

    #region Adler32Checksum
    public class Adler32Checksum
    {
        #region Fields and Properties
        internal const uint InitChecksum = 1;
        public uint Checksum { get; private set; }
        #endregion

        #region Constructor
        public Adler32Checksum()
        {
            NativeMethods.CheckZLibLoaded();

            Reset();
        }
        #endregion

        #region Append, Reset
        public uint Append(byte[] buffer, int offset, int count)
        {
            Checksum = Adler32(Checksum, buffer, offset, count);
            return Checksum;
        }

        public uint Append(ReadOnlySpan<byte> span)
        {
            Checksum = Adler32(Checksum, span);
            return Checksum;
        }

        public uint Append(Stream stream)
        {
            byte[] buffer = new byte[NativeMethods.BufferSize];
            while (stream.Position < stream.Length)
            {
                int readByte = stream.Read(buffer, 0, NativeMethods.BufferSize);
                Checksum = Adler32(Checksum, buffer, 0, readByte);
            }
            return Checksum;
        }

        public void Reset()
        {
            Checksum = InitChecksum;
        }
        #endregion

        #region zlib adler32 Wrapper
        public static unsafe uint Adler32(byte[] buffer, int offset, int count)
        {
            NativeMethods.CheckZLibLoaded();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            fixed (byte* bufPtr = buffer.AsSpan(offset))
            {
                return NativeMethods.Adler32(InitChecksum, bufPtr, (uint)count);
            }
        }

        public static unsafe uint Adler32(ReadOnlySpan<byte> span)
        {
            NativeMethods.CheckZLibLoaded();

            fixed (byte* bufPtr = span)
            {
                return NativeMethods.Adler32(InitChecksum, bufPtr, (uint)span.Length);
            }
        }

        public static uint Adler32(Stream stream)
        {
            uint checksum = InitChecksum;

            byte[] buffer = new byte[NativeMethods.BufferSize];
            int readByte;
            do
            {
                readByte = stream.Read(buffer, 0, buffer.Length);
                checksum = Adler32(checksum, buffer, 0, readByte);
            }
            while (0 < readByte);

            return checksum;
        }

        public static unsafe uint Adler32(uint checksum, byte[] buffer, int offset, int count)
        {
            NativeMethods.CheckZLibLoaded();

            DeflateStream.ValidateReadWriteArgs(buffer, offset, count);

            fixed (byte* bufPtr = buffer.AsSpan(offset))
            {
                return NativeMethods.Adler32(checksum, bufPtr, (uint)count);
            }
        }

        public static unsafe uint Adler32(uint checksum, ReadOnlySpan<byte> span)
        {
            NativeMethods.CheckZLibLoaded();

            fixed (byte* bufPtr = span)
            {
                return NativeMethods.Adler32(checksum, bufPtr, (uint)span.Length);
            }
        }

        public static uint Adler32(uint checksum, Stream stream)
        {
            byte[] buffer = new byte[NativeMethods.BufferSize];
            int readByte;
            do
            {
                readByte = stream.Read(buffer, 0, buffer.Length);
                checksum = Adler32(checksum, buffer, 0, readByte);
            }
            while (0 < readByte);

            return checksum;
        }
        #endregion
    }
    #endregion
}
﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;

namespace Benchmark
{
    public class HashBench
    {
        #region Fields and Properties
        private string _sampleDir;
        private string _destDir;

        // SrcFiles
        [ParamsSource(nameof(SrcFileNames))]
        public string SrcFileName { get; set; }
        public IReadOnlyList<string> SrcFileNames { get; set; } = new string[]
        {
            "Banner.bmp",
            "Banner.svg",
            "Type4.txt",
        };
        public Dictionary<string, byte[]> SrcFiles = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        #endregion

        #region Startup and Cleanup
        [GlobalSetup]
        public void GlobalSetup()
        {
            Program.NativeGlobalInit();

            _sampleDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "..", "Samples"));

            _destDir = Path.GetTempFileName();
            File.Delete(_destDir);
            Directory.CreateDirectory(_destDir);

            foreach (string srcFileName in SrcFileNames)
            {
                string srcFile = Path.Combine(_sampleDir, "Raw", srcFileName);
                using MemoryStream ms = new MemoryStream();
                using (FileStream fs = new FileStream(srcFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fs.CopyTo(ms);
                }

                SrcFiles[srcFileName] = ms.ToArray();
            }
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (Directory.Exists(_destDir))
                Directory.Delete(_destDir);
            Program.NativeGlobalCleanup();
        }
        #endregion

        #region xxHash32
#pragma warning disable IDE1006 // 명명 스타일
        [Benchmark]
        public uint xxHash32_K4osManaged()
        {
            byte[] compData = SrcFiles[SrcFileName];
            K4os.Hash.xxHash.XXH32 xxh32 = new K4os.Hash.xxHash.XXH32();
            xxh32.Update(compData);
            return xxh32.Digest();
        }
#pragma warning restore IDE1006 // 명명 스타일
        #endregion

        #region xxHash64
#pragma warning disable IDE1006 // 명명 스타일
        [Benchmark]
        public ulong xxHash64_K4osManaged()
        {
            byte[] compData = SrcFiles[SrcFileName];
            K4os.Hash.xxHash.XXH64 xxh64 = new K4os.Hash.xxHash.XXH64();
            xxh64.Update(compData);
            return xxh64.Digest();
        }
#pragma warning restore IDE1006 // 명명 스타일
        #endregion

        #region Adler32
        [Benchmark]
        public uint Adler32_ZLibNative()
        {
            byte[] compData = SrcFiles[SrcFileName];
            Joveler.Compression.ZLib.Checksum.Adler32Checksum crc32 = new Joveler.Compression.ZLib.Checksum.Adler32Checksum();
            return crc32.Append(compData);
        }
        #endregion

        #region CRC32
        [Benchmark]
        public uint CRC32_ZLibNative()
        {
            byte[] compData = SrcFiles[SrcFileName];
            Joveler.Compression.ZLib.Checksum.Crc32Checksum crc32 = new Joveler.Compression.ZLib.Checksum.Crc32Checksum();
            return crc32.Append(compData);
        }

        [Benchmark]
        public uint CRC32_XZNative()
        {
            byte[] compData = SrcFiles[SrcFileName];
            Joveler.Compression.XZ.Checksum.Crc32Checksum crc32 = new Joveler.Compression.XZ.Checksum.Crc32Checksum();
            return crc32.Append(compData);
        }

        [Benchmark]
        public byte[] CRC32_ForceManaged()
        {
            byte[] compData = SrcFiles[SrcFileName];
            using Force.Crc32.Crc32Algorithm crc32 = new Force.Crc32.Crc32Algorithm();
            return crc32.ComputeHash(compData);
        }

        [Benchmark]
        public uint CRC32_K4osManaged()
        {
            byte[] compData = SrcFiles[SrcFileName];
            K4os.Hash.Crc.Crc32 crc32 = new K4os.Hash.Crc.Crc32();
            crc32.Update(compData);
            return crc32.Digest();
        }

        [Benchmark]
        public ulong CRC32_TomatoManaged()
        {
            byte[] compData = SrcFiles[SrcFileName];
            InvertedTomato.IO.Crc crc32 = InvertedTomato.IO.CrcAlgorithm.CreateCrc32();
            crc32.Append(compData);
            return crc32.Check;
        }
        #endregion

        #region CRC64
        [Benchmark]
        public ulong CRC64_XZbNative()
        {
            byte[] compData = SrcFiles[SrcFileName];
            Joveler.Compression.XZ.Checksum.Crc64Checksum crc64 = new Joveler.Compression.XZ.Checksum.Crc64Checksum();
            return crc64.Append(compData);
        }
        #endregion
    }
}

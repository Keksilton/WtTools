using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WtTools.Formats.Extensions;
using WtTools.Formats.Vromfs;
using ZstdNet;

namespace WtTools.Formats
{
    public class VromfsInfo
    {
        internal Header Header { get; set; }
        internal ExtHeader ExtHeader { get; set; }

        private static readonly byte[] _firstObfs = new byte[] { 0x55, 0xaa, 0x55, 0xaa, 0x0f, 0xf0, 0x0f, 0xf0, 0x55, 0xaa, 0x55, 0xaa, 0x48, 0x12, 0x48, 0x12 };
        private static readonly byte[] _secondObfs = new byte[] { 0x48, 0x12, 0x48, 0x12, 0x55, 0xaa, 0x55, 0xaa, 0x0f, 0xf0, 0x0f, 0xf0, 0x55, 0xaa, 0x55, 0xaa };

        internal NameMap NameMap { get; set; }
        public FileRecord[] Files { get; set; }
        internal DecompressionOptions DecompressionOptions { get; set; }

        /// <summary>
        /// Read Virtual ROM File System file 
        /// </summary>
        /// <param name="vromfsPath">Path to the .vromfs file</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static VromfsInfo FromFile(string vromfsPath)
        {
            var fileInfo = new FileInfo(vromfsPath);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException($"File '{fileInfo.FullName} doesn't exist");
            }
            var vromfsData = File.ReadAllBytes(fileInfo.FullName);
            var vromfs = new VromfsInfo(vromfsData);
            return vromfs;
        }

        public VromfsInfo(byte[] vromfsData)
        {
            using var ms = new MemoryStream(vromfsData);
            using var reader = new BinaryReader(ms);
            Header = new Header(reader);
            if (Header.Magic == "VRFx")
            {
                ExtHeader = new ExtHeader(reader);
            }
            var data = reader.ReadToEnd();
            if (Header.PackageType != VromfsPackageType.NotPacked)
            {
                data = Deobfuscate(data);
                data = Decompress(data);
            }
            ReadFiles(data);
        }
        
        #region Processing

        /// <summary>
        /// Deobfuscate the data.
        /// </summary>
        /// <param name="data">Obfuscated data</param>
        /// <returns>Modified array</returns>
        private byte[] Deobfuscate(byte[] data)
        {
            var pad = (int)Header.PackedSize % 4;
            var middleSize = (int)Header.PackedSize - (Header.PackedSize >= 32 ? 32 : (Header.PackedSize >= 16 ? 16 : 0)) - pad;
            int j = 16 + middleSize;
            var result = data[0..(middleSize + 32 + pad)];
            for (int i = 0; i < 16; ++i, ++j)
            {
                result[i] = (byte)(result[i] ^ _firstObfs[i]);
                result[j] = (byte)(result[j] ^ _secondObfs[i]);
            }
            
            return result;
        }

        /// <summary>
        /// Decompress the data according to the compression recognized in Header.
        /// </summary>
        /// <param name="compressedData">Compressed data</param>
        /// <returns>Decompressed data</returns>
        private byte[] Decompress(byte[] compressedData)
        {
            if (Header.PackageType == VromfsPackageType.ZstdPacked)
            {
                using var decompressor = new Decompressor();
                var data = decompressor.Unwrap(compressedData);
                return data;
            }
            return compressedData;
        }

        #endregion

        private void ReadFiles(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var fileRecordsOffset = reader.ReadUInt32();
            var filesCount = reader.ReadUInt32();
            _ = reader.ReadBytes(8);
            var dataOffset = reader.ReadUInt32();
            stream.Seek(fileRecordsOffset, SeekOrigin.Begin);
            var firstFilenameOffset = reader.ReadUInt32();
            stream.Seek(firstFilenameOffset, SeekOrigin.Begin);

            Files = new FileRecord[filesCount];
            int nmIndex = 0, dictIndex = 0;
            bool nmFound = false, dictFound = false;
            for (int i = 0; i < filesCount; ++i)
            {
                var record = new FileRecord()
                {
                    Name = reader.BaseStream.ReadTerminatedString()
                };
                if (!dictFound && record.Name.EndsWith(".dict"))
                {
                    dictIndex = i;
                    dictFound = true;
                }
                else if (!nmFound && record.Name.EndsWith("?nm"))
                {
                    record.Name = "nm";
                    nmIndex = i;
                    nmFound = true;
                }
                Files[i] = record;
            }

            stream.Seek(dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < filesCount; ++i)
            {
                Files[i].Offset = reader.ReadUInt32();
                Files[i].Size = (int)reader.ReadUInt32();
                reader.BaseStream.Seek(8, SeekOrigin.Current);
            }
            var dataSpan = data.AsSpan();

            for (int i = 0; i < filesCount; ++i)
            {
                //stream.Seek(Files[i].Offset, SeekOrigin.Begin);
                Files[i].Data = dataSpan.Slice((int)Files[i].Offset, Files[i].Size).ToArray();
            }
            if (dictFound)
            {
                DecompressionOptions = new DecompressionOptions(Files[dictIndex].Data);
            }
            else
            {
                DecompressionOptions = new DecompressionOptions();
            }

            if (nmFound)
            {
                NameMap = new NameMap(Files[nmIndex].Data, DecompressionOptions);
            }
        }
    }
}

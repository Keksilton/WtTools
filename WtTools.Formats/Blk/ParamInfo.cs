using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WtTools.Formats.Extensions;

namespace WtTools.Formats.Blk
{
    internal struct ParamInfo
    {
        public int Id;
        public string Name;
        public DataType Type;
        public object Value;

        public ParamInfo(BinaryReader reader, BlkInfo blk)
        {
            var data = reader.ReadBytes(8);
            Id = (data[0] | (data[1] << 8) | (data[2] << 16));
            Type = (DataType)data[3];
            int index = (data[4] | (data[5] << 8) | (data[6] << 16) | ((data[7] & 0x7f) << 24));
            switch (Type)
            {
                case DataType.Str:
                    var strIndex = index;
                    var tagged = (data[7] >> 7) == 1;
                    Value = blk.GetStringValue(strIndex, tagged);
                    break;
                case DataType.Int:
                    Value = (data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
                    break;
                case DataType.Float:
                    Value = ReadFloat(data.AsSpan(4, 4));
                    break;
                case DataType.Color:
                    Value = new byte[] { data[6], data[5], data[4], data[7] };
                    break;
                case DataType.Size:
                    Value = new ushort[] { ReadUShort(data[4..6]), ReadUShort(data[6..8]) };
                    break;
                case DataType.Typex7:
                    Value = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
                    break;
                case DataType.Long:
                    Value = ParamInfo.ReadLong(blk.LargeData.Slice(index, 8));
                    break;
                case DataType.Vec2:
                    var raw = blk.LargeData.Slice(index, 8);
                    Value = new uint[]
                    {
                        (uint)ReadInt(raw[0..4]),
                        (uint)ReadInt(raw[4..8])
                    };
                    break;
                case DataType.Vec2F:
                    raw = blk.LargeData.Slice(index, 8);
                    Value = new float[]
                    {
                        ReadFloat(raw.Slice(0,4)),
                        ReadFloat(raw.Slice(4, 4))
                    };
                    break;
                case DataType.Vec3:
                    raw = blk.LargeData.Slice(index, 12);
                    Value = new uint[]
                    {
                        (uint)ReadInt(raw[0..4]),
                        (uint)ReadInt(raw[4..8]),
                        (uint)ReadInt(raw[8..12])
                    };
                    break;
                case DataType.Vec3F:
                    raw = blk.LargeData.Slice(index, 12).ToArray();
                    Value = new float[]
                    {
                        ReadFloat(raw[0..4]),
                        ReadFloat(raw[4..8]),
                        ReadFloat(raw[8..12])
                    };
                    break;
                case DataType.Vec4F:
                    raw = blk.LargeData.Slice(index, 16).ToArray();
                    Value = new float[]
                    {
                        ReadFloat(raw[0..4]),
                        ReadFloat(raw[4..8]),
                        ReadFloat(raw[8..12]),
                        ReadFloat(raw[12..16])
                    };
                    break;
                case DataType.M4x3F:
                    raw = blk.LargeData.Slice(index, 48);
                    Value = new float[][]
                    {
                        new float[] {ReadFloat(raw[0..4]),  ReadFloat(raw[4..8]), ReadFloat(raw[8..12])},
                        new float[] {ReadFloat(raw[12..16]),  ReadFloat(raw[16..20]), ReadFloat(raw[20..24])},
                        new float[] {ReadFloat(raw[24..28]),  ReadFloat(raw[28..32]), ReadFloat(raw[32..36])},
                        new float[] {ReadFloat(raw[36..40]),  ReadFloat(raw[40..44]), ReadFloat(raw[44..48])}
                    };
                    break;
                case DataType.Typex:
                    Value = (data[4] == 0);
                    break;
                case DataType.Bool:
                    Value = data[4] == 1;
                    break;
                default:
                    throw new Exception($"Unknown type: 0x{data[3]:x}");
            };
            Name = blk.GetStringValue(Id);
        }

        private static int ReadInt(ReadOnlySpan<byte> data)
        {
            return data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
        }

        private static ushort ReadUShort(ReadOnlySpan<byte> data)
        {
            return (ushort)(data[0] | (data[1] << 8));
        }

        private static float ReadFloat(ReadOnlySpan<byte> data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    float* fPtr = (float*)ptr;
                    return (float)(*fPtr);
                }
            }
        }

        private static long ReadLong(ReadOnlySpan<byte> data)
        {
            return (data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24) | (data[4] << 32) | (data[5] << 40) | (data[6] << 48) | (data[7] << 56));
        }
    }
}

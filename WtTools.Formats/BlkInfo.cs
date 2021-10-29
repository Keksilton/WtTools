using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WtTools.Formats.Blk;
using WtTools.Formats.Extensions;
using ZstdNet;
using Newtonsoft.Json.Linq;

namespace WtTools.Formats
{
    public class BlkInfo
    {
        public string Name { get; set; }
        public VromfsInfo Parent { get; set; }
        public byte PackedType { get; set; }
        internal BlockInfo Root { get; set; }
        internal NameMap NameMap { get; set; }
        private byte[] _largeDataSegment = Array.Empty<byte>();
        internal ReadOnlySpan<byte> LargeData => _largeDataSegment;

        internal bool NameMapExists => NameMap.Count > 0 || Parent.NameMap.Count > 0;

        public BlkInfo(string name, byte[] data, VromfsInfo vromfs)
        {
            Name = name;
            Parent = vromfs;
            PackedType = data[0];
            ProcessData(data);
        }

        private void ProcessData(byte[] data)
        {
            data = Decompress(data);
            if (PackedType >= 1 && PackedType <= 5)
            {
                ParseBinary(data);
            }
            else
            {
                ParseText(data);
            }
        }

        private byte[] Decompress(byte[] data)
        {
            var packedType = data[0];
            byte[] blkData = Array.Empty<byte>();
            switch (packedType)
            {
                case 1:
                    blkData = data.AsMemory(1).ToArray();
                    break;
                case 2:
                    //Console.WriteLine($"packed_type:{packedType}, file:{Name}");
                    break;
                case 3:
                    blkData = data.AsMemory(1).ToArray();
                    break;
                case 4:
                case 5:
                    using (var fileStream = new MemoryStream(data.AsMemory(1).ToArray()))
                    using (var decompressionStream = new DecompressionStream(fileStream, Parent.DecompressionOptions))
                    {
                        blkData = decompressionStream.ReadToEnd();
                    }
                    break;
                default:
                    blkData = data;
                    break;
            }
            return blkData;
        }

        private void ParseBinary(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            NameMap = reader.ReadNameMap();
            int blocksCount = reader.Read7BitEncodedInt();
            int paramsCount = reader.Read7BitEncodedInt();
            int largeDataSize = reader.Read7BitEncodedInt();
            _largeDataSegment = reader.ReadBytes(largeDataSize);
            var parameters = new ParamInfo[paramsCount];
            var blocks = new BlockInfo[blocksCount];
            for (int i = 0; i < paramsCount; ++i)
            {
                parameters[i] = new ParamInfo(reader, this);
            }
            for (int i = 0; i < blocksCount; ++i)
            {
                blocks[i] = new BlockInfo(reader, this);
            }
            int pIndex = paramsCount - 1;
            for (int i = blocksCount - 1; i >= 0; --i)
            {
                if (blocks[i].Params.Length > 0)
                {
                    for (int j = blocks[i].Params.Length - 1; j >= 0; --j)
                    {
                        blocks[i].Params[j] = parameters[pIndex--];
                    }
                }
                if (blocks[i].Blocks.Length > 0)
                {
                    var bIndex = blocks[i].BlockOffset;
                    for (int j = 0; j < blocks[i].Blocks.Length; ++j)
                    {
                        blocks[i].Blocks[j] = blocks[bIndex + j];
                    }
                }
            }

            if (blocksCount > 0)
            {
                Root = blocks[0];
            }
            else
            {
                if (paramsCount > 0)
                {
                    Root = new BlockInfo(parameters);
                }
            }
        }

        private void ParseText(byte[] data)
        {
            var content = data.ToUTF8String();
            var lines = content.Split("\r", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            ParseText(lines);


        }

        internal string GetStringValue(int index)
        {
            if (NameMap.Count > 0)
                return NameMap[index];
            else
                return Parent.NameMap[index];
        }

        private BlockInfo ParseText(string[] lines, int offset = 0)
        {
            var comments = new Dictionary<int, string>();
            var result = new BlockInfo();
            var sections = new List<BlockInfo>();
            var parameters = new List<ParamInfo>();
            for (int i = offset; i < lines.Length; ++i)
            {
                var line = lines[i];
                var comIndex = line.IndexOf("//");
                if (comIndex >= 0)
                {
                    var comment = line.Substring(comIndex + 2, line.Length - comIndex - 2);
                    comments.Add(i, comment);
                    line = line.Substring(0, comIndex).Trim();
                }
                if (!string.IsNullOrEmpty(line))
                {
                    var fbrIndex = line.IndexOf("{");
                    if (fbrIndex >= 0)
                    {
                        var sectionName = line.Substring(0, fbrIndex).Trim();
                        var section = ParseText(lines, i + 1);
                        sections.Add(section);
                    }
                    else if (line.Contains('}'))
                    {
                        break;
                    }
                    else if (line.StartsWith("include"))
                    {
                        //if (!result.ContainsKey("include"))
                        //{
                        //    result.Add("include", new List<string>());
                        //}
                        //((List<string>)result["include"]).Add(line[7..].Trim());
                    }
                    else
                    {
                        var a = line.Split("=").Select(x => x.Trim()).ToArray();
                        var scIndex = a[0].LastIndexOf(':');
                        var vValue = a[1];
                        var vName = a[0][0..(scIndex)].Trim('"');
                        var vType = a[0][(scIndex + 1)..];
                        var p = new ParamInfo()
                        {
                            Name = vName,
                            Type = (DataType)Enum.Parse(typeof(DataName), vType, true)
                        };
                        p.Value = p.Type switch
                        {
                            DataType.Str => vValue,
                            DataType.Int => int.Parse(vValue),
                            DataType.Float => float.Parse(vValue),
                            DataType.Long => long.Parse(vValue),
                            DataType.Color => vValue.Split(',').Select(b => byte.Parse(b)).ToArray(),
                            DataType.Vec2F or DataType.Vec3F or DataType.Vec4F => vValue.Split(',').Select(b => float.Parse(b)).ToArray(),
                            DataType.Vec2 or DataType.Vec3 => vValue.Split(',').Select(b => uint.Parse(b)).ToArray(),
                            _ => throw new NotImplementedException($"Unrecognized type: {vType}")
                        };
                    }
                }
            }
            return result;
        }

        public string ToJSON()
        {
            var dict = Root.ToDictionary();
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dict, Formatting.Indented);
            return json;
        }

        public string ToStrict() => Root.ToStrict();
    }
}

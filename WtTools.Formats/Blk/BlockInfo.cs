using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Blk
{
    internal struct BlockInfo
    {
        public int Id;
        public string Name;
        public int BlockOffset;
        public ParamInfo[] Params;
        public BlockInfo[] Blocks;

        public BlockInfo(BinaryReader reader, BlkInfo blk)
        {
            Id = reader.Read7BitEncodedInt() - 1;
            var paramsCount = reader.Read7BitEncodedInt();
            var blocksCount = reader.Read7BitEncodedInt();
            Params = new ParamInfo[paramsCount];
            Blocks = new BlockInfo[blocksCount];
            if (blocksCount > 0)
            {
                BlockOffset = reader.Read7BitEncodedInt();
            }
            else
            {
                BlockOffset = 0;
            }
            if (Id >= 0)
            {
                Name = blk.GetStringValue(Id);
            }
            else
            {
                Name = String.Empty;
            }
        }

        public BlockInfo(ParamInfo[] @params)
        {
            Params = @params;
            Id = -1;
            Name = String.Empty;
            Blocks = null;
            BlockOffset = 0;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var result = new Dictionary<string, object>();
            if (Params != null)
            {
                foreach (var item in Params)
                {
                    if (result.ContainsKey(item.Name))
                    {
                        if (result[item.Name] is List<object> list)
                        {
                            list.Add(item.Value);
                        }
                        else
                        {
                            var temp = new List<object>
                            {
                                result[item.Name]
                            };
                            temp.Add(item.Value);
                            result[item.Name] = temp;
                        }
                    }
                    else
                    {
                        result.Add(item.Name, item.Value);
                    }
                }
            }
            if (Blocks != null)
            {
                foreach (var item in Blocks)
                {
                    if (result.ContainsKey(item.Name))
                    {
                        if (result[item.Name] is List<object> list)
                        {
                            list.Add(item.ToDictionary());
                        }
                        else
                        {
                            var temp = new List<object>
                            {
                                result[item.Name]
                            };
                            temp.Add(item.ToDictionary());
                            result[item.Name] = temp;
                        }
                    }
                    else
                    {
                        result.Add(item.Name, item.ToDictionary());
                    }
                }
            }
            return result;
        }

        public string ToStrict()
        {
            var builder = new StringBuilder();
            foreach (var item in Params)
            {
                builder.AppendLine($"{item.Name}:{((DataName)item.Type).ToString().ToLower()}={item.Value}");
            }
            foreach (var item in Blocks)
            {
                builder.AppendLine(string.Join("\n  ", item.ToStrict().Split('\n')));
            }
            return builder.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Formats.Blk
{
    public enum DataType
    {
        Size = 0x00,
        Str = 0x01,
        Int = 0x02,
        Float = 0x03,
        Vec2F = 0x04,
        Vec3F = 0x05,
        Vec4F = 0x06,
        Vec2 = 0x07,
        Vec3 = 0x08,
        Bool = 0x09,
        Color = 0x0a,
        M4x3F = 0x0b,
        Long = 0x0c,
        Typex7 = 0x10,
        Typex = 0x89
    }

    public enum DataName
    {
        Size = DataType.Size,
        T = DataType.Str,
        I = DataType.Int,
        R = DataType.Float,
        P2 = DataType.Vec2F,
        P3 = DataType.Vec3F,
        P4 = DataType.Vec4F,
        Ip2 = DataType.Vec2,
        Ip3 = DataType.Vec3,
        B = DataType.Bool,
        C = DataType.Color,
        M = DataType.M4x3F,
        I64 = DataType.Long,
        TypeX7 = DataType.Typex7,
        Typex = DataType.Typex
    }
}

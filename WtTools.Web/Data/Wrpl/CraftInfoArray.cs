using Newtonsoft.Json;
using System;

namespace WtTools.Web.Data.Wrpl
{
    /// <summary>
    /// This class is an absolute mess.
    /// </summary>
    [JsonObject]
    public class CraftsInfoArray : IEnumerable<CraftInfo>
    {
        public bool __array { get; set; }
        public CraftInfo array0 { get; set; }
        public CraftInfo array1 { get; set; }
        public CraftInfo array2 { get; set; }
        public CraftInfo array3 { get; set; }
        public CraftInfo array4 { get; set; }
        public CraftInfo array5 { get; set; }
        public CraftInfo array6 { get; set; }
        public CraftInfo array7 { get; set; }
        public CraftInfo array8 { get; set; }
        public CraftInfo array9 { get; set; }

        public CraftInfo this[int index]
        {
            get => index switch
            {
                0 => array0,
                1 => array1,
                2 => array2,
                3 => array3,
                4 => array4,
                5 => array5,
                6 => array6,
                7 => array7,
                8 => array8,
                9 => array9,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private int _length = -1;

        public int Length
        {
            get
            {
                if (this._length == -1)
                {
                    for (int i = 0; i < 10; ++i)
                    {
                        if (this[i] == null)
                            break;
                        _length = i + 1;
                    }
                }
                return _length;
            }
        }

        public IEnumerator<CraftInfo> GetEnumerator()
        {
            return new CraftsInfoEnum(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}

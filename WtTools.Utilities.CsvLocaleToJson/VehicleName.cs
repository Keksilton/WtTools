using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Utilities.CsvLocaleToJson
{
    public struct VehicleName
    {
        public string this[int index]
        {
            get => index switch
            {
                1 => English,
                2 => French,
                3 => Italian,
                4 => German,
                5 => Spanish,
                6 => Russian,
                7 => Polish,
                8 => Czech,
                9 => Turkish,
                10 => Chinese,
                11 => Japanese,
                12 => Portuguese,
                13 => Ukrainian,
                14 => Serbian,
                15 => Hungarian,
                16 => Korean,
                17 => Belarusian,
                18 => Romanian,
                19 => TChinese,
                20 => HChinese,
                21 => Comments,
                22 => MaxChars,
                _ => throw new ArgumentOutOfRangeException()
            };
            set
            {
                switch (index)
                {
                    case 1: English = value; break;
                    case 2: French = value; break;
                    case 3: Italian = value; break;
                    case 4: German = value; break;
                    case 5: Spanish = value; break;
                    case 6: Russian = value; break;
                    case 7: Polish = value; break;
                    case 8: Czech = value; break;
                    case 9: Turkish = value; break;
                    case 10: Chinese = value; break;
                    case 11: Japanese = value; break;
                    case 12: Portuguese = value; break;
                    case 13: Ukrainian = value; break;
                    case 14: Serbian = value; break;
                    case 15: Hungarian = value; break;
                    case 16: Korean = value; break;
                    case 17: Belarusian = value; break;
                    case 18: Romanian = value; break;
                    case 19: TChinese = value; break;
                    case 20: HChinese = value; break;
                    case 21: Comments = value; break;
                    case 22: MaxChars = value; break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public string English { get; set; }
        public string French { get; set; }
        public string Italian { get; set; }
        public string German { get; set; }
        public string Spanish { get; set; }
        public string Russian { get; set; }
        public string Polish { get; set; }
        public string Czech { get; set; }
        public string Turkish { get; set; }
        public string Chinese { get; set; }
        public string Japanese { get; set; }
        public string Portuguese { get; set; }
        public string Ukrainian { get; set; }
        public string Serbian { get; set; }
        public string Hungarian { get; set; }
        public string Korean { get; set; }
        public string Belarusian { get; set; }
        public string Romanian { get; set; }
        public string TChinese { get; set; }
        public string HChinese { get; set; }
        public string Comments { get; set; }
        public string MaxChars { get; set; }
    }
}

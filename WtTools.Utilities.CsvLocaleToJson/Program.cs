using WtTools.Utilities.CsvLocaleToJson;
using System.Text.Json.Serialization;
using System.Text.Json;

Console.Title = "CSV locale to JSON";

if (args.Length != 1)
{
    if (args.Length == 0)
    {
        throw new ArgumentException("Providea csv file");
    }
    else
    {
        throw new ArgumentException("You can only pass one file at a time");
    }
}

var file = new FileInfo(args[0]);
if (!file.Exists)
{
    throw new FileNotFoundException(args[0]);
}
Console.WriteLine("Opening");
using var reader = file.OpenText();
var result = new Dictionary<string, string>();
_ = reader.ReadLine();
Console.WriteLine("Converting");
while (!reader.EndOfStream)
{
    var line = reader.ReadLine();
    var values = line?.Split(';')?.Select(v => v.Trim('\"')).ToArray() ?? Array.Empty<string>();
    if (values.Length > 0 || !string.IsNullOrEmpty(values[0]))
    {
        if (!values[0].EndsWith("_shop") || result.ContainsKey(values[0]))
            continue;
        var names = new VehicleName();
        for (int i = 1; i < values.Length; i++)
        {
            names[i] = values[i];
        }
        result.Add(values[0][..^5], names.English);
    }
}

var fileName = file.FullName;
if (fileName.EndsWith(".csv"))
{
    fileName = fileName[..^4];
}
fileName += ".json";

using var stream = File.CreateText(fileName);
var serializer = new Newtonsoft.Json.JsonSerializer();
serializer.Serialize(stream, result);

Console.WriteLine("Complete.");
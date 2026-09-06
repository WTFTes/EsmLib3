using System.Text;

namespace EsmLib3;

public class EsmEncoding
{
    public static readonly EsmEncoding CentralOrWesternEuropean;
    public static readonly EsmEncoding Cyrillic;
    public static readonly EsmEncoding English;

    static EsmEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        CentralOrWesternEuropean = new EsmEncoding("win1250");
        Cyrillic = new EsmEncoding("win1251");
        English = new EsmEncoding("win1252");
    }

    public string Name { get; }
    
    public Encoding SystemEncoding { get; }

    public EsmEncoding(string name)
    {
        SystemEncoding = name switch
        {
            "win1250" => Encoding.GetEncoding("windows-1250"),
            "win1251" => Encoding.GetEncoding("windows-1251"),
            "win1252" => Encoding.GetEncoding("windows-1252"),
            _ => throw new Exception($"Unsupported encoding {name}")
        };

        Name = name;
    }
}

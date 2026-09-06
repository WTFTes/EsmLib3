namespace EsmLib3.Translation.Readers;

public abstract class TranslationReader
{
    protected delegate void LoadDelegate(string part1, string part2);
    
    private string _filePath;
    
    public EsmEncoding mEncondig { get; set; } = EsmEncoding.English;

    public void Open(string filePath)
    {
        _filePath = filePath;
    }

    protected void Read(LoadDelegate callback)
    {
        var lines = File.ReadAllLines(_filePath, mEncondig.SystemEncoding);

        foreach (var line in lines)
        {
            var parts = line.Split('\t');

            callback(parts[0], parts[1]);
        }
    }
}

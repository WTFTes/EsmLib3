namespace EsmLib3.Translation.Readers;

// Topic translations
public class MrkReader : LocalizationReader
{
    public override List<LocalizationRecord> Read() => Read(1);
}

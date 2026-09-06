namespace EsmLib3.Translation.Readers;

// Cell and region names translations
public class CelReader : LocalizationReader
{
    public override List<LocalizationRecord> Read() => Read(0);
}

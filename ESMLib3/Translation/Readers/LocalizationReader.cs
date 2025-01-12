namespace EsmLib3.Translation.Readers;

public abstract class LocalizationReader : TranslationReader
{
    public abstract List<LocalizationRecord> Read();

    protected List<LocalizationRecord> Read(int originIndex)
    {
        List<LocalizationRecord> result = new();

        Read((part1, part2) =>
        {
            result.Add(new()
            {
                OriginalText = originIndex == 0 ? part1 : part2,
                TranslatedText = originIndex == 0 ? part2 : part1,
            });
        });

        return result;
    }
}

namespace EsmLib3.Translation.Readers;

// Phraseform variants
public class TopReader : TranslationReader
{
    public List<TopRecord> Read()
    {
        Dictionary<string, TopRecord> result = new();

        Read((part1, part2) =>
        {
            if (result.TryGetValue(part2, out var record))
                record.Variants.Add(part1);
            else
                result[part2] = new TopRecord()
                {
                    Topic = part2,
                    Variants = [part1]
                };
        });

        return result.Select(_ => _.Value).ToList();
    }
}

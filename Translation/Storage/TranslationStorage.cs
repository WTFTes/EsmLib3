namespace EsmLib3.Translation.Storage;

public class TranslationStorage
{
    private readonly Dictionary<string, LocalizationRecord> _translations = new();
    
    private readonly Dictionary<string, LocalizationRecord> _reverseTranslations = new();

    public IEnumerable<LocalizationRecord> Translations => _translations.Values;
    
    public void Append(List<LocalizationRecord> records)
    {
        foreach (var record in records)
        {
            _translations[record.OriginalText] = record;
            _reverseTranslations[record.TranslatedText] = record;
        }
    }

    public string? Lookup(string text) => _translations.TryGetValue(text, out var record) ? record.TranslatedText : null;

    public string? ReverseLookup(string text) =>
        _reverseTranslations.TryGetValue(text, out var record) ? record.OriginalText : null;
}
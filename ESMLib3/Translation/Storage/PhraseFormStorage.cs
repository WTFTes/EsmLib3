using EsmLib3.Translation.Readers;

namespace EsmLib3.Translation.Storage;

public class PhraseFormStorage
{
    private Dictionary<string, string> _normalForms = new();
    
    private Dictionary<string, TopRecord> _phraseForms = new();

    public void Append(List<TopRecord> records)
    {
        foreach (var record in records)
        {
            foreach (var v in record.Variants)
                _normalForms[v] = record.Topic;

            _phraseForms[record.Topic] = record;
        }
    }

    public string? LookupNormalized(string text) => _normalForms.GetValueOrDefault(text);

    public List<string>? LookupForms(string text) =>
        _phraseForms.TryGetValue(text, out var result) ? result.Variants : null;
}

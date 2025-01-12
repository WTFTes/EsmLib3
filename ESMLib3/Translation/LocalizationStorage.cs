using EsmLib3.Translation.Readers;
using EsmLib3.Translation.Storage;

namespace EsmLib3.Translation;

public class LocalizationStorage
{
    public TranslationStorage CellNames { get; } = new();

    public TranslationStorage DialogueNames { get; } = new();

    public PhraseFormStorage PhraseForms { get; } = new();

    public static LocalizationStorage Load(string esmPath, EsmEncoding encoding)
    {
        LocalizationStorage storage = new();

        storage.AppendCel(Path.ChangeExtension(esmPath, ".cel"), encoding);
        storage.AppendMrk(Path.ChangeExtension(esmPath, ".mrk"), encoding);
        storage.AppendTop(Path.ChangeExtension(esmPath, ".top"), encoding);

        return storage;
    }

    private void AppendTop(string path, EsmEncoding encoding)
    {
        if (!File.Exists(path))
            return;
        var reader = new TopReader() { mEncondig = encoding };
        reader.Open(path);
        PhraseForms.Append(reader.Read());
    }

    private void AppendMrk(string path, EsmEncoding encoding)
    {
        if (!File.Exists(path))
            return;
        var reader = new MrkReader() { mEncondig = encoding };
        reader.Open(path);
        DialogueNames.Append(reader.Read());
    }

    private void AppendCel(string path, EsmEncoding encoding)
    {
        if (!File.Exists(path))
            return;
        var reader = new CelReader() { mEncondig = encoding };
        reader.Open(path);
        CellNames.Append(reader.Read());
    }
}

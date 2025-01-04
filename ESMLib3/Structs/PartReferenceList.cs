using EsmLib3.Enums;

namespace EsmLib3.Structs;

// A list of references to body parts
public class PartReferenceList
{
    public List<PartReference> mParts { get; } = new();

    /// Load one part, assumes the subrecord name was already read
    public void Add(EsmReader reader)
    {
        PartReference part = new();
        reader.getHT(() => part.mPart = (PartReferenceType)reader.BinaryReader.ReadByte());

        part.mMale = reader.getHNORefId(RecordName.BNAM);
        part.mFemale = reader.getHNORefId(RecordName.CNAM);

        mParts.Add(part);
    }

    public void Save(EsmWriter writer)
    {
        throw new NotImplementedException();    
    }
}

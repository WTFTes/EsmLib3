using EsmLib3.Enums;
using EsmLib3.RefIds;

namespace EsmLib3.Structs;

// Reference to body parts
public class PartReference
{
    public PartReferenceType mPart { get; set; } // possible values [0, 26] (byte)
    public RefId mMale { get; set; }
    public RefId mFemale { get; set; }
}
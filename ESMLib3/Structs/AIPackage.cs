using EsmLib3.Structs.AIPackages;

namespace EsmLib3.Structs;

/// \note Used for storaging packages in a single container
/// w/o manual memory allocation accordingly to policy standards
public abstract class AIPackage
{
    public RecordName mType { get; private set; }

    public static AIPackage Create(RecordName name)
    {
        AIPackage? ai = null;
        switch (name)
        {
            case RecordName.AI_W:
                ai = new AIWander();
                break;
            case RecordName.AI_T:
                ai = new AITravel();
                break;
            case RecordName.AI_F:
            case RecordName.AI_E:
                ai = new AITarget();
                break;
            case RecordName.AI_A:
                ai = new AIActivate();
                break;
        }

        if (ai == null)
            throw new Exception($"Unknown AI package {name.ToMagic()}");

        ai.mType = name;

        return ai;
    }

    /// \note for AITarget only, placed here to stick with union,
    /// overhead should be not so awful
    public string mCellName { get; set; }
}

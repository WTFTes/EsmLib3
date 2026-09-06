using EsmLib3.Structs.AIPackages;

namespace EsmLib3.Structs;

/// \note Used for storaging packages in a single container
/// w/o manual memory allocation accordingly to policy standards
public abstract class AIPackage
{
    public RecordName mType { get; private set; }

    public static AIPackage Create(RecordName name)
    {
        AIPackage? ai = name switch
        {
            RecordName.AI_W => new AIWander(),
            RecordName.AI_T => new AITravel(),
            RecordName.AI_F or RecordName.AI_E => new AITarget(),
            RecordName.AI_A => new AIActivate(),
            _ => null
        };

        if (ai == null)
            throw new Exception($"Unknown AI package {name.ToMagic()}");

        ai.mType = name;

        return ai;
    }
}

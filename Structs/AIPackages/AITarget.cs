namespace EsmLib3.Structs.AIPackages;

public class AITarget : AIPackage
{
    public float mX { get; set; }

    public float mY { get; set; }

    public float mZ { get; set; }

    public short mDuration { get; set; }

    public string mId { get; set; } // NAME32

    public byte mShouldRepeat { get; set; }
    
    public string mCellName { get; set; }
}
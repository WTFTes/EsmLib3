namespace EsmLib3.Structs.AIPackages;

public class AIWander : AIPackage
{
    public short mDistance { get; set; }
    
    public short mDuration { get; set; }
    
    public byte mTimeOfDay { get; set; }
    
    public byte[] mIdle { get; } = new byte[8];
    
    public byte mShouldRepeat { get; set; }
}
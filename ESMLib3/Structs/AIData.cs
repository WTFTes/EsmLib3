using EsmLib3.Enums;

namespace EsmLib3.Structs;

public class AIData
{
    public ushort mHello { get; set; } // This is the base value for greeting distance [0, 65535]
    
    public byte mFight { get; set; } // These are probabilities [0, 100]
    
    public byte mFlee { get; set; }
    
    public byte mAlarm { get; set; }
    
    public Services mServices { get; set; } // See the Services enum (int)

    public void Blank()
    {
        mHello = mFight = mFlee = mAlarm = 0;
        mServices = 0;
    }
}; // 12 bytes

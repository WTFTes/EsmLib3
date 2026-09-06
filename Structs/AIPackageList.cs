using EsmLib3.Structs.AIPackages;

namespace EsmLib3.Structs;

public class AIPackageList
{
    public List<AIPackage> mList { get; set; } = new();

    /// Add a single AIPackage, assumes subrecord name was already read
    public void Add(EsmReader reader)
    {
        if (reader.retSubName() == RecordName.CNDT)
        {
            if (mList.Count == 0)
                throw new Exception("AIPackage with an CNDT applying to no cell.");

            if (mList[^1] is not AITarget ai)
                throw new Exception("Last AI is not AITarget");
            
            ai.mCellName = reader.getHString();
        }
        else if (reader.retSubName() == RecordName.AI_W)
        {
            AIWander ai = (AIPackage.Create(reader.retSubName()) as AIWander)!;
            reader.getHT(() =>
            {
                ai.mDistance = reader.BinaryReader.ReadInt16();
                ai.mDuration = reader.BinaryReader.ReadInt16();
                ai.mTimeOfDay = reader.BinaryReader.ReadByte();
                for (var i = 0; i < ai.mIdle.Length; ++i)
                    ai.mIdle[i] = reader.BinaryReader.ReadByte();
                ai.mShouldRepeat = reader.BinaryReader.ReadByte();
            });
            
            mList.Add(ai);
        }
        else if (reader.retSubName() == RecordName.AI_T)
        {
            AITravel ai = (AIPackage.Create(reader.retSubName()) as AITravel)!;
            reader.getHT(() =>
            {
                ai.mX = reader.BinaryReader.ReadSingle();
                ai.mY = reader.BinaryReader.ReadSingle();
                ai.mZ = reader.BinaryReader.ReadSingle();
                ai.mShouldRepeat = reader.BinaryReader.ReadByte();
                reader.skip(3); // padding
            });
            
            mList.Add(ai);
        }
        else if (reader.retSubName() == RecordName.AI_E || reader.retSubName() == RecordName.AI_F)
        {
            AITarget ai = (AIPackage.Create(reader.retSubName()) as AITarget)!;
            reader.getHT(() =>
            {
                ai.mX = reader.BinaryReader.ReadSingle();
                ai.mY = reader.BinaryReader.ReadSingle();
                ai.mZ = reader.BinaryReader.ReadSingle();
                ai.mDuration = reader.BinaryReader.ReadInt16();
                ai.mId = reader.getString(32);
                ai.mShouldRepeat = reader.BinaryReader.ReadByte();
                reader.skip(1); // padding
            });
            
            mList.Add(ai);
        }
        else if (reader.retSubName() == RecordName.AI_A)
        {
            AIActivate ai = (AIPackage.Create(reader.retSubName()) as AIActivate)!;
            reader.getHT(() =>
            {
                ai.mName = reader.getString(32);
                ai.mShouldRepeat = reader.BinaryReader.ReadByte();
            });
            
            mList.Add(ai);
        }
    }

    public void Save(EsmWriter writer)
    {
        foreach (var package in mList)
        {
            switch (package.mType)
            {
                case RecordName.AI_W:
                {
                    var w = (AIWander)package;
                    writer.writeHNT(package.mType, () =>
                    {
                        writer.Write(w.mDistance);
                        writer.Write(w.mDuration);
                        writer.Write(w.mTimeOfDay);
                        foreach (var i in w.mIdle)
                            writer.Write(i);
                        writer.Write(w.mShouldRepeat);
                    });
                    break;
                }
                case RecordName.AI_T:
                {
                    var t = (AITravel)package;
                    writer.writeHNT(package.mType, () =>
                    {
                        writer.Write(t.mX);
                        writer.Write(t.mY);
                        writer.Write(t.mZ);
                        writer.Write(t.mShouldRepeat);  
                        writer.Write(new byte[3]);
                    });
                    break;
                }
                case RecordName.AI_A:
                {
                    var a = (AIActivate)package;
                    writer.writeHNT(package.mType, () =>
                    {
                        writer.writeHString(a.mName, 32);
                        writer.Write(a.mShouldRepeat);
                    });
                    break;
                }
                case RecordName.AI_E:
                case RecordName.AI_F:
                {
                    var t = (AITarget)package;
                    writer.writeHNT(package.mType, () =>
                    {
                        writer.Write(t.mX);
                        writer.Write(t.mY);
                        writer.Write(t.mZ);
                        writer.Write(t.mDuration);
                        writer.writeHString(t.mId, 32);
                        writer.Write(t.mShouldRepeat);
                        writer.Write((byte)0);
                    });
                    writer.writeHNOCString(RecordName.CNDT, t.mCellName);
                    break;
                }
                default:
                    break;
            }
        }
    }
}
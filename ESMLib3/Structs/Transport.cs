using System.Diagnostics;

namespace EsmLib3.Structs;

/// List of travel service destination. Shared by CREA and NPC_ records.
public class Transport
{
    public class Dest
    {
        public Position mPos { get; set; } = new();

        public string mCellName { get; set; }
    }

    public readonly List<Dest> mList = new();

    /// Load one destination, assumes the subrecord name was already read
    public void Add(EsmReader reader)
    {
        if (reader.retSubName() == RecordName.DODT)
        {
            Dest dodt = new();
            reader.getHT(() =>
            {
                dodt.mPos.X = reader.BinaryReader.ReadSingle();
                dodt.mPos.Y = reader.BinaryReader.ReadSingle();
                dodt.mPos.Z = reader.BinaryReader.ReadSingle();
                dodt.mPos.RotX = reader.BinaryReader.ReadSingle();
                dodt.mPos.RotY = reader.BinaryReader.ReadSingle();
                dodt.mPos.RotZ = reader.BinaryReader.ReadSingle();
            });
            mList.Add(dodt);
        }
        else if (reader.retSubName() == RecordName.DNAM)
        {
            var name = reader.getHString();
            if (mList.Count == 0)
                Debug.WriteLine("Encountered DNAM record without DODT record, skipped.");
            else
                mList[^1].mCellName = name;
        }
    }

    public void Save(EsmWriter writer)
    {
        throw new NotImplementedException();
    }
}
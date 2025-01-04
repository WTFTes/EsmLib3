namespace EsmLib3.Structs;

public class EffectList
{
    /** Defines a spell effect. Shared between SPEL (Spells), ALCH
     (Potions) and ENCH (Item enchantments) records
     */
    public class ENAMstruct
    {
        // Magical effect, hard-coded ID
        public short mEffectID { get; set; }

        // Which skills/attributes are affected (for restore/drain spells
        // etc.)
        public sbyte mSkill { get; set; } // -1 if N/A

        public sbyte mAttribute { get; set; } // -1 if N/A

        // Other spell parameters
        public int mRange { get; set; } // 0 - self, 1 - touch, 2 - target (RangeType enum)

        public int mArea { get; set; }

        public int mDuration { get; set; }

        public int mMagnMin { get; set; }

        public int mMagnMax { get; set; }
    }

    public List<ENAMstruct> mList { get; set; } = new();

    public void Add(EsmReader reader)
    {
        ENAMstruct s = new();
        reader.getHT(() =>
        {
            s.mEffectID = reader.BinaryReader.ReadInt16();
            s.mSkill = reader.BinaryReader.ReadSByte();
            s.mAttribute = reader.BinaryReader.ReadSByte();
            s.mRange = reader.BinaryReader.ReadInt32();
            s.mArea = reader.BinaryReader.ReadInt32();
            s.mDuration = reader.BinaryReader.ReadInt32();
            s.mMagnMin = reader.BinaryReader.ReadInt32();
            s.mMagnMax = reader.BinaryReader.ReadInt32();
        });

        mList.Add(s);
    }

    public void Load(EsmReader reader)
    {
        mList.Clear();
        while (reader.IsNextSub(RecordName.ENAM))
            Add(reader);
    }

    public void Save(EsmWriter writer)
    {
        throw new NotImplementedException();
    }
}

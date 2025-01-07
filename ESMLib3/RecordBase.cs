using EsmLib3.Enums;
using EsmLib3.Records;
using Activator = EsmLib3.Records.Activator;

namespace EsmLib3;

public abstract class RecordBase
{
    public RecordFlag mFlags;
    
    public RecordName mType;
    
    public bool IsDeleted { get; set; }

    public static RecordBase? Create(RecordName name)
    {
        RecordBase record = null;
        switch (name)
        {
            case RecordName.GMST:
                record = new TypedRecord<GameSetting>();
                break;
            case RecordName.GLOB:
                record = new TypedRecord<Global>();
                break;
            case RecordName.CLAS:
                record = new TypedRecord<Class>();
                break;
            case RecordName.FACT:
                record = new TypedRecord<Faction>();
                break;
            case RecordName.RACE:
                record = new TypedRecord<Race>();
                break;
            case RecordName.SOUN:
                record = new TypedRecord<Sound>();
                break;
            case RecordName.SKIL:
                record = new TypedRecord<Skill>();
                break;
            case RecordName.MGEF:
                record = new TypedRecord<MagicEffect>();
                break;
            case RecordName.SCPT:
                record = new TypedRecord<Script>();
                break;
            case RecordName.REGN:
                record = new TypedRecord<Region>();
                break;
            case RecordName.BSGN:
                record = new TypedRecord<BirthSign>();
                break;
            case RecordName.LTEX:
                record = new TypedRecord<LandTexture>();
                break;
            case RecordName.STAT:
                record = new TypedRecord<Static>();
                break;
            case RecordName.DOOR:
                record = new TypedRecord<Door>();
                break;
            case RecordName.SPEL:
                record = new TypedRecord<Spell>();
                break;
            case RecordName.MISC:
                record = new TypedRecord<Miscellaneous>();
                break;
            case RecordName.WEAP:
                record = new TypedRecord<Weapon>();
                break;
            case RecordName.CONT:
                record = new TypedRecord<Container>();
                break;
            case RecordName.CREA:
                record = new TypedRecord<Creature>();
                break;
            case RecordName.BODY:
                record = new TypedRecord<Body>();
                break;
            case RecordName.LIGH:
                record = new TypedRecord<Light>();
                break;
            case RecordName.ENCH:
                record = new TypedRecord<Enchantment>();
                break;
            case RecordName.NPC_:
                record = new TypedRecord<Npc>();
                break;
            case RecordName.ARMO:
                record = new TypedRecord<Armor>();
                break;
            case RecordName.CLOT:
                record = new TypedRecord<Clothing>();
                break;
            case RecordName.REPA:
                record = new TypedRecord<Repair>();
                break;
            case RecordName.ACTI:
                record = new TypedRecord<Activator>();
                break;
            case RecordName.APPA:
                record = new TypedRecord<Apparatus>();
                break;
            case RecordName.LOCK:
                record = new TypedRecord<Lockpick>();
                break;
            case RecordName.PROB:
                record = new TypedRecord<Probe>();
                break;
            case RecordName.INGR:
                record = new TypedRecord<Ingredient>();
                break;
            case RecordName.BOOK:
                record = new TypedRecord<Book>();
                break;
            case RecordName.ALCH:
                record = new TypedRecord<Potion>();
                break;
            case RecordName.LEVI:
                record = new TypedRecord<ItemLevList>();
                break;
            case RecordName.LEVC:
                record = new TypedRecord<CreatureLevList>();
                break;
            case RecordName.CELL:
                record = new TypedRecord<Cell>();
                break;
            case RecordName.LAND:
                record = new TypedRecord<Land>();
                break;
            case RecordName.PGRD:
                record = new TypedRecord<Pathgrid>();
                break;
            case RecordName.SNDG:
                record = new TypedRecord<SoundGenerator>();
                break;
            case RecordName.DIAL:
                record = new TypedRecord<Dialogue>();
                break;
            case RecordName.INFO:
                record = new TypedRecord<DialInfo>();
                break;  
            case RecordName.SSCR:
                record = new TypedRecord<StartScript>();
                break;
        }

        if (record == null)
            return null;

        record.mType = name;

        return record;
    }

    public abstract void Load(EsmReader reader);
    public abstract void Save(EsmWriter writer);
}

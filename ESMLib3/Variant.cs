namespace EsmLib3;

public class Variant
{
    public enum VarType
    {
        Unknown,
        None,
        Short,
        Int,
        Long,
        Float,
        String
    }

    public enum Format
    {
        Global,
        Gmst,
        Info,
        Local,
    }

    public VarType Type { get; set; } = VarType.None;

    public void Read(EsmReader reader, Format format)
    {
        var type = VarType.Unknown;

        if (format == Format.Global)
        {
            var typeId = reader.getHNString(RecordName.FNAM);

            if (typeId == "s")
                type = VarType.Short;
            else if (typeId == "l")
                type = VarType.Long;
            else if (typeId == "f")
                type = VarType.Float;
            else
                throw new Exception("Illegal global variable type " + typeId);
        }
        else if (format == Format.Gmst)
        {
            if (!reader.HasMoreSubs)
                type = VarType.None;
            else
            {
                reader.GetSubName();
                var name = reader.retSubName();

                if (name == RecordName.STRV)
                    type = VarType.String;
                else if (name == RecordName.INTV)
                    type = VarType.Int;
                else if (name == RecordName.FLTV)
                    type = VarType.Float;
                else
                    throw new Exception($"Invalid subrecord: {name.ToMagic()}");
            }
        }
        else if (format == Format.Info)
        {
            reader.GetSubName();

            var name = reader.retSubName();
            if (name == RecordName.INTV)
                type = VarType.Int;
            else if (name == RecordName.FLTV)
                type = VarType.Float;
            else
                throw new Exception($"Invalid subrecord: {name.ToMagic()}");
        }
        else if (format == Format.Local)
        {
            reader.GetSubName();

            var name = reader.retSubName();
            if (name == RecordName.INTV)
                type = VarType.Int;
            else if (name == RecordName.FLTV)
                type = VarType.Float;
            else if (name == RecordName.STTV)
                type = VarType.Short;
            else
                throw new Exception($"Invalid subrecord: {name.ToMagic()}");
        }

        Type = type;

        switch (type)
        {
            case VarType.String:
                ReadStringVariant(reader, format);
                break;
            case VarType.Int:
            case VarType.Long:
            case VarType.Short:
                ReadIntVariant(reader, format);
                break;
            case VarType.Float:
                ReadFloatVariant(reader, format);
                break;
        }
    }

    private void ReadFloatVariant(EsmReader reader, Format format)
    {
        if (format == Format.Global)
            reader.getHNT(RecordName.FLTV, () => FloatValue = reader.BinaryReader.ReadSingle());
        else if (format == Format.Gmst || format == Format.Info || format == Format.Local)
            reader.getHT(() => FloatValue = reader.BinaryReader.ReadSingle());
    }

    private void ReadIntVariant(EsmReader reader, Format format)
    {
        if (format == Format.Global)
        {
            float value = 0;
            reader.getHNT(RecordName.FLTV, () => value = reader.BinaryReader.ReadSingle());

            if (Type == VarType.Short)
            {
                if (float.IsNaN(value))
                    IntValue = 0;
                else
                    IntValue = (short)value;
            }
            else if (Type == VarType.Long)
                IntValue = (int)value;
            else
                throw new Exception("Unsupported global variable integer type");
        }
        else if (format == Format.Gmst || format == Format.Info)
        {
            if (Type != VarType.Int)
                throw new Exception($"Unsupported {format} variable integer type");

            reader.getHT(() => IntValue = reader.BinaryReader.ReadInt32());
        }
        else if (format == Format.Local)
        {
            if (Type == VarType.Short)
                reader.getHT(() => IntValue = reader.BinaryReader.ReadInt16());
            else if (Type == VarType.Int)
                reader.getHT(() => IntValue = reader.BinaryReader.ReadInt32());
            else
                throw new Exception("Unsupported local variable integer type");
        }
    }

    private void ReadStringVariant(EsmReader reader, Format format)
    {
        if (format == Format.Global)
            throw new Exception("Global variables of type string not supported");
        if (format == Format.Info)
            throw new Exception("Info variables of type string not supported");
        if (format == Format.Local)
            throw new Exception("Local variables of type string not supported");

        StringValue = reader.getHString();
    }

    public void Write(EsmWriter writer, Format format)
    {
        if (Type == VarType.Unknown)
            throw new Exception("Can't write variant of unknown type");

        if (Type == VarType.None)
        {
            if (format == Format.Global)
                throw new Exception("Can't write variant of type none to global format");
            if (format == Format.Info)
                throw new Exception("Can't write variant of type none to info format");
            if (format == Format.Local)
                throw new Exception("Can't write variant of type none to local format");
            // nothing to do for GMST format
        }
        else if (Type == VarType.String)
            WriteStringValue(writer, format);
        else if (Type == VarType.Int || Type == VarType.Long || Type == VarType.Short)
            WriteIntValue(writer, format);
        else if (Type == VarType.Float)
            WriteFloatValue(writer, format);
    }

    private void WriteFloatValue(EsmWriter writer, Format format)
    {
        if (format == Format.Global)
        {
            writer.writeHNString(RecordName.FNAM, "f");
            writer.writeHNT(RecordName.FLTV, FloatValue);
        }
        else if (format == Format.Gmst || format == Format.Info || format == Format.Local)
        {
            writer.writeHNT(RecordName.FLTV, FloatValue);
        }
    }

    private void WriteIntValue(EsmWriter writer, Format format)
    {
        if (format == Format.Global)
        {
            if (Type == VarType.Short || Type == VarType.Long)
            {
                writer.writeHNString(RecordName.FNAM, Type == VarType.Short ? "s" : "l");
                writer.writeHNT(RecordName.FLTV, (float)IntValue);
            }
            else
                throw new Exception("unsupported global variable integer type");
        }
        else if (format == Format.Gmst || format == Format.Info)
        {
            if (Type != VarType.Int)
                throw new Exception($"Unsupported " + (format == Format.Gmst ? "gmst" : "info") +
                                    " variable integer type");

            writer.writeHNT(RecordName.INTV, IntValue);
        }
        else if (format == Format.Local)
        {
            if (Type == VarType.Short)
                writer.writeHNT(RecordName.STTV, IntValue);
            else if (Type == VarType.Int)
                writer.writeHNT(RecordName.INTV, IntValue);
            else
                throw new Exception("unsupported local variable integer type");
        }
    }

    private void WriteStringValue(EsmWriter writer, Format format)
    {
        if (format == Format.Global)
            throw new Exception("global variables of type string not supported");

        if (format == Format.Info)
            throw new Exception("info variables of type string not supported");

        if (format == Format.Local)
            throw new Exception("local variables of type string not supported");

        // GMST
        writer.writeHNString(RecordName.STRV, StringValue);
    }

    public string StringValue { get; set; }

    public int IntValue { get; set; }

    public float FloatValue { get; set; }
}

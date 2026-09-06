namespace EsmLib3.RefIds;

public struct RefId : IEquatable<RefId>
{
    public RefId()
    {
    }

    public static RefId StringRefId(string value)
    {
        if (string.IsNullOrEmpty(value))
            return new RefId();
        
        return new RefId(new StringRefId(value));
    }
    
    public static RefId FormIdRefId(FormId formId) => new(formId);
    
    public static RefId Generated(ulong value) => new(new GeneratedRefId(value));
    
    public static RefId Index(RecordName name, uint value) => new(new IndexRefId(name, value));

    public static RefId Esm3ExteriorCell(int x, int y) => new(new Esm3ExteriorCellRefId(x, y));
    
    private RefId(StringRefId value)
    {
        Type = RefIdType.SizedString;
        _stringRefId = value;
    }

    private RefId(FormId value)
    {
        if (value.IsZeroOrUnset)
            return;

        if (value.HasContentFile)
        {
            Type = RefIdType.FormId;
            _formId = value;
            return;
        }

        throw new Exception("RefId can't be a generated FormId");
    }

    private RefId(GeneratedRefId value)
    {
        Type = RefIdType.Generated;
        _generatedRefId = value;
    }

    private RefId(IndexRefId value)
    {
        Type = RefIdType.Index;
        _indexRefId = value;
    }
    
    private RefId(Esm3ExteriorCellRefId value)
    {
        Type = RefIdType.Esm3ExteriorCell;
        _esm3ExteriorCellRefId = value;
    }

    private StringRefId? _stringRefId;
    
    private FormId? _formId;
    
    private GeneratedRefId? _generatedRefId;
    
    private IndexRefId? _indexRefId;
    
    private Esm3ExteriorCellRefId? _esm3ExteriorCellRefId;
    
    public RefIdType Type { get; private set; } = RefIdType.Empty;

    public string GetRefIdString()
    {
        switch (Type)
        {
            case RefIdType.Empty:
                return "";
            case RefIdType.SizedString:
            case RefIdType.UnsizedString:
                return _stringRefId.Value.Value;
            default:
                throw new Exception($"RefId {Type} not a string");
        }
    }

    public static bool operator ==(RefId c1, RefId c2)
    {
        return c1.Equals(c2);
    }

    public static bool operator ==(RefId c1, string c2)
    {
        return (c1.Type == RefIdType.SizedString || c1.Type == RefIdType.UnsizedString) &&
               c1._stringRefId!.Value.Value == c2;
    }
    
    public static bool operator ==(string c1, RefId c2)
    {
        return c2 == c1;
    }

    public static bool operator !=(string c1, RefId c2)
    {
        return c2 != c1;
    }

    public static bool operator !=(RefId c1, string c2)
    {
        return !(c1 == c2);
    }

    public static bool operator !=(RefId c1, RefId c2)
    {
        return !c1.Equals(c2);
    }

    public bool Equals(RefId other)
    {
        switch (Type)
        {
            case RefIdType.Empty:
                return other.Type == RefIdType.Empty || other.IsEmpty;
            case RefIdType.SizedString:
            case RefIdType.UnsizedString:
                return (other.Type == RefIdType.SizedString || other.Type == RefIdType.UnsizedString) &&
                       other._stringRefId == _stringRefId;
            case RefIdType.FormId:
                if (other.IsEmpty && _formId.Value.IsZeroOrUnset)
                    return true;

                return other.Type == RefIdType.FormId && other._formId == _formId;
            case RefIdType.Generated:
                return other.Type == RefIdType.Generated && other._generatedRefId == _generatedRefId;
            case RefIdType.Index:
                return other.Type == RefIdType.Index && other._indexRefId == _indexRefId;
            case RefIdType.Esm3ExteriorCell:
                return other.Type == RefIdType.Esm3ExteriorCell &&
                       other._esm3ExteriorCellRefId == _esm3ExteriorCellRefId;
        }

        return false;
    }

    public bool IsEmpty
    {
        get
        {
            if (Type == RefIdType.Empty)
                return true;

            if (Type == RefIdType.FormId && _formId.Value.IsZeroOrUnset)
                return true;

            if ((Type == RefIdType.SizedString || Type == RefIdType.UnsizedString) &&
                string.IsNullOrEmpty(_stringRefId.Value.Value))
                return true;

            return false;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        return Equals((RefId)obj);
    }

    public override int GetHashCode()
    {
        switch (Type)
        {
            case RefIdType.Empty:
                return HashCode.Combine(0, Type);
            case RefIdType.SizedString:
            case RefIdType.UnsizedString:
                return HashCode.Combine(_stringRefId!.Value, Type);
            case RefIdType.FormId:
                return HashCode.Combine(_formId!.Value, Type);
            case RefIdType.Generated:
                return HashCode.Combine(_generatedRefId!.Value, Type);
            case RefIdType.Index:
                return HashCode.Combine(_indexRefId!.Value, Type);
            case RefIdType.Esm3ExteriorCell:
                return HashCode.Combine(_esm3ExteriorCellRefId!.Value, Type);
        }

        throw new Exception("Unknown ref id type");
    }
    
    public override string ToString()
    {
        switch (Type)
        {
            case RefIdType.Empty:
                return "";
            case RefIdType.SizedString:
            case RefIdType.UnsizedString:
                return _stringRefId.ToString();
            case RefIdType.FormId:
                return _formId.ToString();
            case RefIdType.Generated:
                return _generatedRefId.ToString();
            case RefIdType.Index:
                return _indexRefId.ToString();
            case RefIdType.Esm3ExteriorCell:
                return _esm3ExteriorCellRefId.ToString();
        }

        throw new Exception("Unknown ref id type");
    }

    public string ToDebugString()
    {
        switch (Type)
        {
            case RefIdType.Empty:
                return "Empty{}";
            case RefIdType.SizedString:
            case RefIdType.UnsizedString:
                return _stringRefId.Value.ToDebugString();
            case RefIdType.FormId:
                return _formId.Value.ToDebugString();
            case RefIdType.Generated:
                return _generatedRefId.Value.ToDebugString();
            case RefIdType.Index:
                return _indexRefId.Value.ToDebugString();
            case RefIdType.Esm3ExteriorCell:
                return _esm3ExteriorCellRefId.Value.ToDebugString();
        }

        throw new Exception("Unknown ref id type");
    }

    public void Save(EsmWriter writer, bool sized)
    {
        var type = Type;
        if (type == RefIdType.SizedString || type == RefIdType.UnsizedString)
            type = sized ? RefIdType.SizedString : RefIdType.UnsizedString;

        writer.Write((byte)type);
        
        switch (type)
        {
            case RefIdType.Empty:
                return;
            case RefIdType.SizedString:
                writer.writeStringWithSize(GetRefIdString());
                return;
            case RefIdType.UnsizedString:
                writer.writeHString(GetRefIdString());
                return;
            case RefIdType.FormId:
                writer.Write(_formId!.Value.Index);
                writer.Write(_formId!.Value.ContentFile);
                return;
            case RefIdType.Generated:
                writer.Write(_generatedRefId!.Value.Value);
                return;
            case RefIdType.Index:
                writer.Write((int)_indexRefId!.Value.RecordType);
                writer.Write(_indexRefId!.Value.Value);
                return;
            case RefIdType.Esm3ExteriorCell:
                writer.Write(_esm3ExteriorCellRefId!.Value.X);
                writer.Write(_esm3ExteriorCellRefId!.Value.Y);
                return;
            default:
                throw new Exception("Unknown ref id type");
        }
    }
}

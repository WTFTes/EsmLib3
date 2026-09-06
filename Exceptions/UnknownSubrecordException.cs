namespace EsmLib3.Exceptions;

public class UnknownSubrecordException : Exception
{
    public UnknownSubrecordException(RecordName name) : base($"Unknown subrecord: {name.ToMagic()}")
    {
    }
}

namespace EsmLib3.Exceptions;

public class MissingSubrecordException : Exception
{
    public MissingSubrecordException(RecordName name) : base($"Missing {name.ToMagic()} subrecord")
    {
    }
}

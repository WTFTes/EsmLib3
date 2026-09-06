namespace EsmLib3;

public struct ReadSettings
{
    public List<RecordName> RecordsToGet;

    public bool SkippedRecordsToRaw;

    public bool SkipUnknownRecords;
    
    public bool UnknownRecordsToRaw;
}
namespace EsmLib3;

public struct Context
{
    public Context()
    {
    }

    public long leftRec;
    public uint leftSub;
    public long leftFile;

    public RecordName RecName;
    public RecordName SubName;
    public long FileSize;

    // True if subName has been read but not used.
    public bool subCached;

    public string FileName;

    // When working with multiple esX files, we will generate lists of all files that
    //  actually contribute to a specific cell. Therefore, we need to store the index
    //  of the file belonging to this contest. See CellStore::(list/load)refs for details.
    public int index;
    public List<int> parentFileIndices = new();

    // File position. Only used for stored contexts, not regularly
    // updated within the reader itself.
    public long filePos;
}

using EsmLib3.RefIds;

namespace EsmLib3.Structs;

/* Moved cell reference tracking object. This mainly stores the target cell
        of the reference, so we can easily know where it has been moved when another
        plugin tries to move it independently.
    Unfortunately, we need to implement this here.
    */
public class MovedCellRef
{
    public FormId mRefNum { get; set; }

    // Coordinates of target exterior cell
    public int[] mTarget { get; } = new int[2];

    // The content file format does not support moving objects to an interior cell.
    // The save game format does support moving to interior cells, but uses a different mechanism
    // (see the MovedRefTracker implementation in MWWorld::CellStore for more details).
}

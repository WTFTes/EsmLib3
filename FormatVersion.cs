namespace EsmLib3;

public enum FormatVersion : uint
{
    DefaultFormatVersion = 0,
    CurrentContentFormatVersion = 1,
    MaxOldGoldValueFormatVersion = 5,
    MaxOldFogOfWarFormatVersion = 6,
    MaxUnoptimizedCharacterDataFormatVersion = 7,
    MaxOldTimeLeftFormatVersion = 8,
    MaxIntFallbackFormatVersion = 10,
    MaxOldRestockingFormatVersion = 14,
    MaxClearModifiersFormatVersion = 16,
    MaxOldAiPackageFormatVersion = 17,
    MaxOldSkillsAndAttributesFormatVersion = 18,
    MaxOldCreatureStatsFormatVersion = 19,
    MaxLimitedSizeStringsFormatVersion = 22,
    MaxStringRefIdFormatVersion = 23,
    MaxSavedGameCellNameAsRefIdFormatVersion = 24,
    MaxNameIsRefIdOnlyFormatVersion = 25,
    MaxUseEsmCellIdFormatVersion = 26,
    MaxActiveSpellSlotIndexFormatVersion = 27,
    MaxOldCountFormatVersion = 30,
    CurrentSaveGameFormatVersion = 31,

    MinSupportedSaveGameFormatVersion = 5,
    OpenMW0_48SaveGameFormatVersion = 21,
    OpenMW0_49SaveGameFormatVersion = CurrentSaveGameFormatVersion,
}
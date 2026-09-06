# EsmLib3

A .NET (net10.0) library for reading and writing **Morrowind (TES3)** archive files: `.esm`/`.esp` plugins and `.ess` savegames. It is a C# port of the ESM parsing/serialization code from [OpenMW](https://gitlab.com/OpenMW/openmw), reworked into an idiomatic .NET API.

## What it does

Morrowind's data files (masters, plugins, savegames) are all stored in the same binary "ESM" container format: a top-level `TES3` header record followed by a flat sequence of tagged records (`GMST`, `NPC_`, `CELL`, `CREA`, `DIAL`, …), each built from tagged sub-records. EsmLib3 implements that format end to end:

- **Reading** — `EsmReader` parses the record/sub-record binary stream into strongly-typed record objects (one C# class per record type, see [`Records/`](Records/)).
- **Writing** — `EsmWriter` serializes those same record objects back into a valid ESM binary stream, recomputing all record/sub-record size headers.
- **Round-tripping** — since read and write share the same record model, a file can be loaded, inspected/modified in memory, and saved back out.
- **Record model** — 40+ record types (`NPC`, `Creature`, `Cell`, `Faction`, `Spell`, `Weapon`, `Armor`, `Dialogue`, `Land`, `LandTexture`, `Pathgrid`, leveled lists, etc. — see [`Records/`](Records/)), plus shared structures for things like AI packages, inventories, spell lists and cell references (see [`Structs/`](Structs/)).
- **RefIds** — Bethesda's reference-id encoding evolved over the years (plain strings, generated ids, `FormId`s, indexed ids, ESM3 exterior-cell ids); [`RefIds/`](RefIds/) models all of them behind one `RefId` type, and `EsmReader`/`EsmWriter` pick the right encoding based on the file's `FormatVersion`.
- **Format-version awareness** — [`FormatVersion.cs`](FormatVersion.cs) tracks the many on-disk format revisions used across vanilla Morrowind and different OpenMW releases (fixed vs. size-prefixed strings, string vs. structured `RefId`s, savegame layout changes, etc.), so the reader/writer adapt automatically to the file being processed.
- **Encodings** — built-in support for the three code pages Morrowind localizations use: English (`windows-1252`), Central/Western European (`windows-1250`) and Cyrillic (`windows-1251`) — see [`EsmEncoding.cs`](EsmEncoding.cs).
- **Translations** — [`Translation/`](Translation/) can additionally load the `.cel`/`.mrk`/`.top` side files some localized Morrowind releases ship (cell-name and dialogue-name translation tables, and topic "phrase form" tables for normalizing dialogue keywords across languages).

## Project layout

```
EsmReader.cs / EsmWriter.cs   binary record/sub-record stream parsing & serialization
EsmData.cs                    in-memory representation of a loaded file (header + records)
Header.cs                     the TES3 header record (version, master list, savegame info)
RecordBase.cs / TypedRecord.cs / AbstractRecord.cs   record type dispatch & base classes
RecordName.cs                 4-byte record/sub-record tag enum (NAME, FNAM, GMST, NPC_, ...)
Records/                      one class per top-level record type (NPC, Cell, Spell, ...)
Structs/                      shared sub-structures reused across records
RefIds/                       reference-id variants (string, FormId, generated, indexed, ...)
Enums/                        game enums (EsmVersion, Services, PartReferenceType, RecordFlag)
Translation/                  .cel/.mrk/.top localization side-file readers
Exceptions/                   parsing-specific exceptions
```

## Installation

Add a reference to the `EsmLib3` project/csproj from your solution, or build the class library and reference the resulting DLL. Targets `net10.0`.

## Usage

### Reading an archive

```csharp
using EsmLib3;
using EsmLib3.Records;

using var reader = new EsmReader();
reader.open("Morrowind.esm");

EsmData data = reader.Read();

Console.WriteLine($"Author: {data.Header.mData.author}");
Console.WriteLine($"Records: {data.RecordCount}");

// Strongly-typed access to a specific record type
foreach (var npc in data.RecordData<Npc>())
    Console.WriteLine(npc.mName);

// Or grouped by record tag
foreach (var (type, records) in data.GetGroupedRecords())
    Console.WriteLine($"{type}: {records.Count}");
```

### Reading selectively

`ReadSettings` lets you restrict which record types get fully parsed, and decide what happens to the rest (skip, or keep as raw untouched bytes so they can be written back unmodified):

```csharp
var settings = new ReadSettings
{
    RecordsToGet = [RecordName.NPC_, RecordName.CELL],
    SkippedRecordsToRaw = true, // records not in RecordsToGet are kept as raw bytes
};

var data = reader.Read(settings);
```

### Writing an archive

```csharp
using var reader = new EsmReader();
reader.open("input.esp");
var data = reader.Read();

// ... inspect or mutate data.Records ...

using var writer = new EsmWriter();
writer.save("output.esp", data);
```

`WriteSettings.SkipDeleted` can be used to drop records flagged as deleted instead of re-emitting their tombstone entries.

### Choosing an encoding

Non-English Morrowind releases use different code pages for in-file strings:

```csharp
using var reader = new EsmReader { mEncoding = EsmEncoding.Cyrillic };
reader.open("Morrowind_ru.esm");
var data = reader.Read();
```

### Loading localization side files

Some localized releases ship extra `.cel`/`.mrk`/`.top` files alongside the `.esm`/`.esp` next to it (translated cell names, dialogue/topic names, and phrase-form normalization tables):

```csharp
using EsmLib3.Translation;

var localization = LocalizationStorage.Load("Morrowind_ru.esm", EsmEncoding.Cyrillic);

var normalized = localization.PhraseForms.LookupNormalized("двемеров");
```

## Status

This is an active port/rewrite in progress — reading is implemented for the classic Morrowind (TES3) record set, and writing mirrors it back out. APIs may still change as coverage and OpenMW-format-version compatibility are extended.

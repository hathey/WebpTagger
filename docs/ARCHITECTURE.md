# WebpTagger — Architecture

## Overview

WebpTagger is a Windows Forms desktop application for browsing a folder of
`.webp` images, viewing them as thumbnails, and managing a set of free-text
tags per image. Tags are persisted **inside the WebP file itself** as EXIF
metadata, so they travel with the file and remain visible to other tools
(including Windows Explorer's "Tags" property).

## Solution layout

```
WebpTagger/
├── WebpTagger.slnx
├── .gitignore
├── README.md
├── docs/
│   └── ARCHITECTURE.md
├── src/
│   ├── WebpTagger.Core/            # Net 10 class library, no WinForms dependency
│   │   ├── Models/
│   │   │   └── ImageItem.cs
│   │   ├── Services/
│   │   │   ├── IDirectoryScanner.cs
│   │   │   ├── DirectoryScanner.cs
│   │   │   ├── ITaggingEngine.cs
│   │   │   ├── TaggingEngine.cs
│   │   │   ├── IThumbnailService.cs
│   │   │   ├── ThumbnailService.cs
│   │   │   ├── TagLibrary.cs
│   │   └── WebpTagger.Core.csproj
│   └── WebpTagger.UI/               # Net 10 windows (WinForms) executable
│       ├── Program.cs
│       ├── MainForm.cs / .Designer.cs / .resx
│       ├── Controls/
│       │   ├── TagEditorPanel.cs / .Designer.cs / .resx
│       │   └── TagListSidebar.cs / .Designer.cs / .resx
│       └── WebpTagger.UI.csproj
└── tests/
    └── WebpTagger.Tests/             # xUnit
        ├── TaggingEngineTests.cs
        ├── TagLibraryTests.cs
        ├── DirectoryScannerTests.cs
        └── WebpTagger.Tests.csproj
```

`WebpTagger.Core` has zero dependency on `System.Windows.Forms` /
`System.Drawing` so it can be unit tested headless and reused (e.g. in a CLI
or other front end later). It targets `net10.0`. `WebpTagger.UI` targets
`net10.0-windows` and references `Core`. `WebpTagger.Tests` targets `net10.0`
and references `Core`.

## Image / EXIF library

**SixLabors.ImageSharp** (>= 3.1) is used for:
- Decoding/encoding `.webp` (built-in WebP codec, both lossy and lossless).
- Reading and writing the EXIF profile (`Image.Metadata.ExifProfile`).
- Resizing for thumbnails.

ImageSharp is pure managed code (no native binaries), which keeps the build
and the repo small. It is licensed under the Six Labors Split License — free
for individuals and organizations under the revenue threshold defined in that
license; see `README.md` for a note on this.

## Tag storage format (EXIF)

Tags are stored as a single string in the EXIF **`XPKeywords`** tag
(EXIF tag ID `0x9C9E`, one of the Windows "XP" extension tags). This is the
same field Windows Explorer reads/writes for a file's **Tags** property, so
tags applied in WebpTagger are visible in Explorer's property panel and vice
versa.

- Multiple tags are joined with `;` (semicolon + no extra spaces are
  trimmed on save), e.g. `sunset;beach;vacation`.
- On read, the value is split on `;`, each part is trimmed, empty entries are
  discarded, and duplicates (case-insensitive) are removed.
- If the tag is absent, the image has zero tags.
- When saving, if the tag list is empty the `XPKeywords` EXIF entry is
  removed entirely (rather than writing an empty string).

`TaggingEngine` is the single place that knows about this encoding — if the
storage format ever needs to change (e.g. to XMP `dc:subject`), only this
class changes.

### Preserving image data on save

When tags are saved, `TaggingEngine` re-encodes the WebP file. To minimize
quality loss it inspects the existing `WebpMetadata.FileFormat` on the loaded
image and reuses that (lossy vs. lossless) when constructing the
`WebpEncoder` used to re-save, so lossless files remain lossless. ImageSharp
does not expose the original "quality" value used to encode a lossy file, so
re-saving a lossy file re-encodes it at the encoder's default quality (75).
This is a known limitation: editing tags on a lossy WebP causes a small
additional generation of lossy compression.

## Core types

### `Models/ImageItem.cs`
Plain data holder for one image in the currently-open folder:
- `FilePath` (string, full path)
- `FileName` (derived)
- `Tags` (`List<string>`, current in-memory tag set)
- `LastWriteTimeUtc` (DateTime, used for thumbnail cache invalidation)
- `IsDirty` (bool — tags edited but not yet written to disk)

### `Services/IDirectoryScanner.cs` + `DirectoryScanner.cs`
- `Task<IReadOnlyList<ImageItem>> ScanAsync(string directoryPath, IProgress<DirectoryScanProgress>? progress, CancellationToken ct)`
- Enumerates top-level `*.webp` files in the given directory (case-insensitive
  extension match).
- For each file, calls `TaggingEngine.ReadTagsAsync` to populate `Tags` using
  `Image.Identify` (metadata-only decode — does not decode pixel data, so
  scanning a folder of large images stays fast).
- Reports progress as `(int completed, int total, string currentFile)`.

### `Services/ITaggingEngine.cs` + `TaggingEngine.cs`
- `Task<List<string>> ReadTagsAsync(string filePath, CancellationToken ct)`
- `Task SaveTagsAsync(string filePath, IReadOnlyList<string> tags, CancellationToken ct)`
- Implements the EXIF `XPKeywords` encoding described above.

### `Services/IThumbnailService.cs` + `ThumbnailService.cs`
- `Task<byte[]> GetThumbnailAsync(string filePath, DateTime lastWriteTimeUtc, int maxSize, CancellationToken ct)`
- Returns a PNG-encoded thumbnail (`maxSize` × `maxSize` bounding box,
  aspect-ratio preserved) as a `byte[]`, suitable for
  `Image.FromStream(new MemoryStream(bytes))` on the UI side.
- **Two-level cache**:
  1. In-memory `ConcurrentDictionary<string, (DateTime mtime, byte[] png)>`
     for the lifetime of the process.
  2. On-disk cache under
     `%LocalAppData%\WebpTagger\ThumbnailCache\<sha256(path)>.png`, keyed by a
     hash of the full file path; a cache entry is considered valid only if
     its file mtime is >= the source file's `LastWriteTimeUtc`.
- Cache is invalidated automatically whenever the source file's
  `LastWriteTimeUtc` changes (e.g. after `SaveTagsAsync` re-encodes the file —
  which changes pixel data only if the encoder re-quantizes, but the mtime
  always changes).

### `Services/TagLibrary.cs`
The "tag list manager" — aggregates the set of distinct tags across every
`ImageItem` currently loaded.
- `IReadOnlyList<string> Tags` — sorted (ordinal, case-insensitive), de-duplicated.
- `void Rebuild(IEnumerable<ImageItem> items)` — called after a directory scan.
- `void Add(string tag)` — called when the user types a brand-new tag in the
  editor, so it immediately appears in the sidebar for reuse on other images
  even before any image is saved.
- `event EventHandler? Changed` — UI subscribes to refresh the sidebar
  `ListBox`.

## UI layer (`WebpTagger.UI`)

### `MainForm`
Top-level layout (using a `TableLayoutPanel` / `SplitContainer` combination):

- **Top toolbar**: "Open Folder…" button (opens `FolderBrowserDialog`),
  current path label, "Save All" button, progress bar (visible during scans).
- **Center**: `ListView` in `LargeIcon` view backed by an `ImageList`
  populated from `ThumbnailService` — this is the thumbnail grid. Supports
  multi-selection.
- **Right side** (`SplitContainer`, vertical):
  - **Top**: `TagEditorPanel` — shows the union/intersection of tags for the
    currently selected image(s), lets the user add a free-text tag (creates
    it in `TagLibrary` too) or remove a tag (chip-style `FlowLayoutPanel`
    with small "x" buttons).
  - **Bottom**: `TagListSidebar` — `ListBox`/`CheckedListBox` of all tags
    from `TagLibrary` for the open folder; double-click (or "Apply" button)
    applies the tag to every currently-selected image.
- **Status strip** (bottom): scan progress text, dirty-file count, save
  status.

### Event flow
1. **Open Folder** → `FolderBrowserDialog` → `DirectoryScanner.ScanAsync`
   (background `Task`, progress reported via `IProgress<T>` marshaled to the
   UI thread) → populate `ListView` items (thumbnails loaded incrementally,
   each via `ThumbnailService.GetThumbnailAsync` on a background task) →
   `TagLibrary.Rebuild(items)`.
2. **Selecting items** in the `ListView` → `TagEditorPanel` refreshes to show
   the selected image(s)' tags.
3. **Adding/removing a tag** in `TagEditorPanel` → updates the in-memory
   `ImageItem.Tags` for all selected items, sets `IsDirty = true`, and (for
   new tags) calls `TagLibrary.Add(tag)`.
4. **Applying a tag from `TagListSidebar`** → same effect as 3 but for an
   existing tag applied to all selected items.
5. **Save All** → iterates dirty `ImageItem`s, calls
   `TaggingEngine.SaveTagsAsync`, clears `IsDirty`, refreshes thumbnail cache
   entry (mtime changed).
6. **Closing the form** with unsaved (`IsDirty`) items prompts the user to
   save, discard, or cancel.

All long-running work (scanning, thumbnailing, EXIF I/O) runs off the UI
thread via `async`/`await` so the UI remains responsive on large folders.

## Testing (`WebpTagger.Tests`, xUnit)

- **`TaggingEngineTests`**: generate small in-memory WebP files (via
  ImageSharp), round-trip tags through `SaveTagsAsync`/`ReadTagsAsync`,
  including: no tags, one tag, many tags, tags with whitespace/duplicates,
  removing all tags clears `XPKeywords`.
- **`TagLibraryTests`**: `Rebuild`/`Add` produce a sorted, de-duplicated,
  case-insensitive tag list and raise `Changed`.
- **`DirectoryScannerTests`**: a temp directory containing a mix of `.webp`
  and non-`.webp` files yields only the `.webp` files; empty/non-existent
  directories are handled without throwing.

## Build

```
dotnet build WebpTagger.slnx
dotnet test WebpTagger.slnx
dotnet run --project src/WebpTagger.UI
```

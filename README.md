# WebpTagger

A Windows Forms desktop app for browsing a folder of `.webp` images as
thumbnails and managing free-text tags for them. Tags are stored **inside
each WebP file** as EXIF metadata, so they're visible in Windows Explorer's
"Tags" property and travel with the file.

## Features

- Pick a folder and view all `.webp` images in it as a thumbnail grid.
- Select one or more images and view/edit their tags.
- Add new tags (free text) or remove existing ones via a chip-style editor.
- A folder-wide "All Tags" list lets you re-apply any previously-used tag to
  the current selection with one click.
- Tags are written back to the file's EXIF `XPKeywords` field, the same field
  Windows Explorer uses, so they round-trip with Explorer's Tags property.
- Async directory scanning and thumbnail generation (with on-disk caching) so
  large folders stay responsive.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (LTS)
- Windows (Windows Forms / `net10.0-windows`)

## Building and running

```
dotnet build WebpTagger.slnx
dotnet test WebpTagger.slnx
dotnet run --project src/WebpTagger.UI
```

## Project structure

```
src/WebpTagger.Core/   Tag storage, directory scanning, thumbnails (no UI deps)
src/WebpTagger.UI/     Windows Forms application
tests/WebpTagger.Tests/ xUnit tests for WebpTagger.Core
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for a full description of
the architecture, the EXIF tag storage format, and the UI's event flow.

## Tag storage format

Tags are stored as a single semicolon-separated string in the EXIF
`XPKeywords` tag (`0x9C9E`) — e.g. `beach;sunset;vacation`. On save, tags are
trimmed, de-duplicated case-insensitively, and an empty tag list removes the
`XPKeywords` entry entirely. See
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#tag-storage-format-exif) for
details.

## Using the app

1. Click **Open Folder...** and choose a folder containing `.webp` files.
   Thumbnails load incrementally as they're generated.
2. Select one or more images in the grid. The right-hand **Tags** panel shows
   the union of tags across the selection.
3. Type a new tag and press Enter (or click **Add Tag**) to add it to all
   selected images. Click the "x" on a tag chip to remove it.
4. Use the **All Tags** list on the bottom-right to re-apply an existing tag
   to the current selection — double-click a tag or select it and click
   **Apply to Selected**.
5. Click **Save All** to write all changed tags back to disk. Images with
   unsaved changes are marked with `*` in the grid. Closing the app or
   opening a new folder with unsaved changes prompts you to save first.

## Notes and limitations

- Re-saving a **lossy** WebP re-encodes it at the encoder's default quality
  (ImageSharp does not expose the original encode quality), so editing tags
  on a lossy file causes a small additional generation of compression.
  Lossless files remain lossless. See
  [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md#preserving-image-data-on-save).
- Only top-level `.webp` files in the selected folder are shown (no recursive
  subfolder scanning).
- [SixLabors.ImageSharp](https://sixlabors.com/) is used for WebP decoding,
  encoding, and EXIF read/write. This project pins ImageSharp to the 3.x line
  (Six Labors Split License — free for most uses); ImageSharp 4.x requires a
  commercial license even for non-commercial use.

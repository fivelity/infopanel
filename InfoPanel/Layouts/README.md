# InfoPanel Layouts

This directory contains layout definitions for InfoPanel display configurations.

## Available Layouts

### ultrawide-fullscreen.json
**Optimized for**: 21:9 and 32:9 ultrawide displays

A 6-column × 3-row grid layout with oversized controls:
- 2 large hero panels (2×2 grid span)
- 4 center panels (1×1 grid span)
- 4 bottom panels (2×1 or 1×1 span)

**Best for**: 3440×1440, 5120×1440, 2560×1080 resolutions

```
┌─────────────────────────┬──────────┬──────────┬─────────────────────────┐
│                         │          │          │                         │
│      Hero Panel         │  Center  │  Center  │      Hero Panel         │
│         Left            │  Top 1   │  Top 2   │         Right           │
│       (2×2 Grid)        ├──────────┼──────────┤       (2×2 Grid)        │
│                         │  Center  │  Center  │                         │
│                         │ Middle 1 │ Middle 2 │                         │
├─────────────────────────┼──────────┼──────────┼─────────────────────────┤
│     Bottom Left         │  Bottom  │  Bottom  │     Bottom Right        │
│      (2×1 Wide)         │ Center 1 │ Center 2 │      (2×1 Wide)         │
└─────────────────────────┴──────────┴──────────┴─────────────────────────┘
```

## Layout File Format

Layouts are defined in JSON format with the following structure:

```json
{
  "id": "unique-layout-id",
  "name": "Display Name",
  "description": "Layout description",
  "author": "Author Name",
  "version": "1.0.0",
  "type": "Grid|Canvas|Dock|Stack",
  "grid": { /* Grid configuration */ },
  "regions": [ /* Panel regions */ ],
  "metadata": { /* Additional info */ }
}
```

## Creating Custom Layouts

1. Copy an existing layout file
2. Modify the `id`, `name`, and `description`
3. Adjust grid columns, rows, and regions
4. Save with a unique filename
5. Restart InfoPanel to load

## Grid Layout Properties

- **columns**: Number of grid columns
- **rows**: Number of grid rows
- **columnWidths**: Array of column widths (e.g., "1*", "2*", "300")
- **rowHeights**: Array of row heights
- **gap**: Space between panels (pixels)
- **padding**: Outer padding (left, top, right, bottom)

## Region Properties

- **id**: Unique region identifier
- **name**: Display name
- **column**: Starting column (0-indexed)
- **row**: Starting row (0-indexed)
- **columnSpan**: Number of columns to span
- **rowSpan**: Number of rows to span
- **minWidth/minHeight**: Minimum dimensions
- **alignment**: Horizontal alignment
- **verticalAlignment**: Vertical alignment

## Tips

- Use star-based widths (*) for responsive columns
- Larger outer columns work well for ultrawide displays
- Keep gap between 8-24px for visual separation
- Set minimum sizes to ensure readability
- Test on your target resolution

## More Information

See `ULTRAWIDE_THEME_LAYOUT.md` in the root directory for detailed documentation.

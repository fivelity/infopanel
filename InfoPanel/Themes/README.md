# InfoPanel Themes

This directory contains theme definitions for InfoPanel visual styling.

## Available Themes

### dark-blue-ultrawide.json
**Optimized for**: Ultrawide displays with high contrast needs

A beautiful dark blue theme featuring:
- Deep blue primary colors (#1E3A8A, #3B82F6)
- Very dark blue background (#0F172A)
- High contrast text (#F1F5F9)
- Complete design token system

**Color Palette**:
```
Primary:    #1E3A8A (Deep Blue)
Accent:     #3B82F6 (Bright Blue)
Background: #0F172A (Very Dark Blue)
Text:       #F1F5F9 (Almost White)
Success:    #10B981 (Green)
Warning:    #F59E0B (Amber)
Error:      #EF4444 (Red)
```

## Theme File Format

Themes are defined in JSON format with the following structure:

```json
{
  "id": "unique-theme-id",
  "name": "Display Name",
  "description": "Theme description",
  "author": "Author Name",
  "version": "1.0.0",
  "colors": { /* Color tokens */ },
  "typography": { /* Font settings */ },
  "spacing": { /* Spacing values */ },
  "elevation": { /* Shadow definitions */ },
  "motion": { /* Animation settings */ }
}
```

## Design Tokens

### Colors
- **Primary/Secondary/Accent**: Main color system
- **Background layers**: Background, BackgroundSecondary, BackgroundTertiary
- **Text colors**: TextPrimary, TextSecondary, TextTertiary, TextDisabled
- **Semantic colors**: Success, Warning, Error, Info
- **Interactive states**: Hover, Pressed variants for all colors
- **Borders**: Border, BorderHover, BorderPressed

### Typography
- **Font families**: Primary and monospace
- **Font sizes**: XSmall (10px) to Huge (48px)
- **Font weights**: Light (300) to Bold (700)
- **Line heights**: Tight, Normal, Relaxed
- **Letter spacing**: Tight, Normal, Wide

### Spacing
- **Space sizes**: XSmall (4px) to Huge (64px)
- **Border radius**: Small (4px) to Full (9999px)
- **Border width**: Thin (1px) to Thick (4px)

### Elevation
- **Shadow levels**: None, Small, Medium, Large, XLarge, Inner
- Uses rgba for shadow colors with opacity

### Motion
- **Durations**: Instant (100ms) to Slower (700ms)
- **Easing curves**: Standard, Decelerate, Accelerate, Sharp

## Creating Custom Themes

1. Copy an existing theme file
2. Modify the `id`, `name`, and `description`
3. Update color values (use hex format: #RRGGBB)
4. Adjust typography, spacing, elevation, motion as needed
5. Save with a unique filename
6. Restart InfoPanel to load

## Color Guidelines

- Use hex color format: `#RRGGBB`
- Ensure sufficient contrast (WCAG AA minimum)
- Provide hover/pressed states for interactive colors
- Use semantic colors consistently
- Test on your target display

## Tips

- **High contrast**: Use 7:1 or higher for text on background
- **Consistent accents**: Use same accent color throughout
- **Semantic meaning**: Green for success, red for errors, etc.
- **Layer depth**: Use 3 background layers for visual hierarchy
- **Test thoroughly**: Check all UI elements with your theme

## Contrast Ratios (WCAG Standards)

- **AAA**: 7:1 or higher (recommended for large displays)
- **AA**: 4.5:1 or higher (minimum for body text)
- **AA Large**: 3:1 or higher (for large text 18px+)

## More Information

See `ULTRAWIDE_THEME_LAYOUT.md` in the root directory for detailed documentation.

## Example Color Schemes

### Dark Blue (Current)
```
Primary:    #1E3A8A
Background: #0F172A
Text:       #F1F5F9
```

### Potential Variants
```
Dark Purple:
Primary:    #6B21A8
Background: #1A0B2E
Text:       #F3E8FF

Dark Green:
Primary:    #065F46
Background: #022C22
Text:       #ECFDF5

Dark Red:
Primary:    #991B1B
Background: #1C0A0A
Text:       #FEF2F2
```

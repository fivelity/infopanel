# Dark Blue Ultrawide Theme & Layout

## 🎨 Dark Blue Ultrawide Theme

A beautiful dark blue theme specifically designed for ultrawide displays with high contrast and elegant blue accents.

### Color Palette

**Primary Colors:**
- Primary: `#1E3A8A` (Deep Blue)
- Accent: `#3B82F6` (Bright Blue)
- Background: `#0F172A` (Very Dark Blue)

**Background Layers:**
- Background: `#0F172A` (Darkest - main background)
- Background Secondary: `#1E293B` (Dark - cards and panels)
- Background Tertiary: `#334155` (Medium - elevated surfaces)

**Text Colors:**
- Text Primary: `#F1F5F9` (Almost White - high contrast)
- Text Secondary: `#CBD5E1` (Light Gray - secondary text)
- Text Tertiary: `#94A3B8` (Medium Gray - hints and labels)

**Semantic Colors:**
- Success: `#10B981` (Green)
- Warning: `#F59E0B` (Amber)
- Error: `#EF4444` (Red)
- Info: `#06B6D4` (Cyan)

### Typography

- **Font Family**: Segoe UI, Arial, sans-serif
- **Monospace**: Consolas, Courier New, monospace
- **Sizes**: 10px to 48px (XSmall to Huge)
- **Weights**: 300 to 700 (Light to Bold)

### Design Features

- **High Contrast**: Optimized for visibility on large displays
- **Blue Accent System**: Consistent blue color scheme throughout
- **Elegant Shadows**: Depth through subtle elevation
- **Smooth Animations**: 100ms to 700ms duration ranges

## 📐 Ultrawide Fullscreen Layout

A 6-column by 3-row grid layout optimized for ultrawide displays (21:9, 32:9) with oversized controls.

### Layout Structure

```
┌─────────────┬────┬────┬────┬────┬─────────────┐
│             │    │    │    │    │             │
│   Hero      │ C1 │ C2 │    │    │   Hero      │
│   Left      │    │    │    │    │   Right     │
│   (2x2)     ├────┼────┤    │    │   (2x2)     │
│             │ C3 │ C4 │    │    │             │
├─────────────┼────┼────┼────┼────┼─────────────┤
│  Bottom     │ BC1│ BC2│    │    │  Bottom     │
│  Left (2x1) │    │    │    │    │  Right(2x1) │
└─────────────┴────┴────┴────┴────┴─────────────┘
```

### Regions

**Hero Panels (Large Feature Areas):**
1. **Hero Left** (Columns 0-1, Rows 0-1)
   - 2x2 grid span
   - Minimum: 400x300px
   - Perfect for: Large graphs, system overview, main metrics

2. **Hero Right** (Columns 4-5, Rows 0-1)
   - 2x2 grid span
   - Minimum: 400x300px
   - Perfect for: Large visualizations, media, status displays

**Center Panels (Standard Size):**
3. **Center Top 1** (Column 2, Row 0) - 200x150px min
4. **Center Top 2** (Column 3, Row 0) - 200x150px min
5. **Center Middle 1** (Column 2, Row 1) - 200x150px min
6. **Center Middle 2** (Column 3, Row 1) - 200x150px min

**Bottom Panels (Wide):**
7. **Bottom Left** (Columns 0-1, Row 2) - 300x150px min
8. **Bottom Center 1** (Column 2, Row 2) - 200x150px min
9. **Bottom Center 2** (Column 3, Row 2) - 200x150px min
10. **Bottom Right** (Columns 4-5, Row 2) - 300x150px min

### Layout Features

**Optimized for Ultrawide:**
- 6-column grid efficiently uses horizontal space
- Wider outer columns (1.5x) for hero panels
- 16px gap between panels for visual separation
- 32px horizontal padding, 24px vertical padding

**Oversized Controls:**
- Minimum panel sizes ensure readability from distance
- Large hero panels for primary information
- Balanced distribution across screen width

**Flexible Grid:**
- Star-based column widths: `1.5*, 1*, 1*, 1*, 1*, 1.5*`
- Equal row heights: `1*, 1*, 1*`
- Responsive to different ultrawide resolutions

### Recommended Use Cases

**3440x1440 (21:9 Ultrawide):**
- Perfect fit with all panels clearly visible
- Hero panels: ~650px wide
- Center panels: ~430px wide

**5120x1440 (32:9 Super Ultrawide):**
- Excellent use of extra width
- Hero panels: ~970px wide
- Center panels: ~650px wide

**2560x1080 (21:9 Standard):**
- Still works well with slightly smaller panels
- Hero panels: ~480px wide
- Center panels: ~320px wide

## 🚀 How to Use

### Applying the Theme

1. Open InfoPanel
2. Navigate to **Appearance** page
3. Go to **Themes** tab
4. Select **"Dark Blue Ultrawide"**
5. Click **"Apply Theme"**

### Applying the Layout

1. Stay on **Appearance** page
2. Go to **Layouts** tab
3. Select **"Ultrawide Fullscreen"**
4. Click **"Apply Layout"**

### Creating a Workspace

1. Go to **Workspace** tab
2. Click **"New Workspace"**
3. Name it "Ultrawide Blue Setup"
4. The current theme and layout will be saved
5. Click **"Save"**

## 💡 Design Philosophy

### Theme Design
- **Immersive**: Dark blue creates a professional, focused environment
- **High Visibility**: Strong contrast for readability on large displays
- **Consistent**: Blue accent system ties all UI elements together
- **Modern**: Clean, flat design with subtle depth through shadows

### Layout Design
- **Efficient**: Makes full use of ultrawide horizontal space
- **Balanced**: Hero panels on edges, detailed info in center
- **Scalable**: Works across different ultrawide resolutions
- **Practical**: Oversized panels readable from normal viewing distance

## 🎯 Best Practices

### Panel Placement Recommendations

**Hero Left Panel:**
- System overview dashboard
- Large performance graphs
- Main application status

**Hero Right Panel:**
- Media player
- Large visualizations
- Secondary monitoring

**Center Panels:**
- CPU/GPU metrics
- Temperature sensors
- Network stats
- Storage info

**Bottom Panels:**
- Time and date
- Weather
- Quick stats
- System information

### Color Usage Tips

- Use accent blue (`#3B82F6`) for interactive elements
- Use success green for positive metrics
- Use warning amber for attention items
- Use error red sparingly for critical alerts

## 📊 Technical Specifications

### Theme File
- **Location**: `InfoPanel/Themes/dark-blue-ultrawide.json`
- **Format**: JSON
- **Size**: ~3.3 KB
- **Tokens**: Colors, Typography, Spacing, Elevation, Motion

### Layout File
- **Location**: `InfoPanel/Layouts/ultrawide-fullscreen.json`
- **Format**: JSON
- **Size**: ~5.4 KB
- **Type**: Grid Layout
- **Regions**: 10 defined regions

## 🔄 Customization

Both the theme and layout can be exported, modified, and re-imported:

1. **Export**: Use the Export button on the Appearance page
2. **Edit**: Modify the JSON file with your preferred colors/layout
3. **Import**: Use the Import button to load your custom version

### Theme Customization Examples

Change primary color:
```json
"primary": "#YOUR_COLOR_HERE"
```

Adjust background darkness:
```json
"background": "#0A0F1A"  // Even darker
```

### Layout Customization Examples

Add more columns:
```json
"columns": 8
```

Adjust padding:
```json
"padding": {
  "left": 48,
  "top": 32,
  "right": 48,
  "bottom": 32
}
```

## 🌟 Features

✅ Beautiful dark blue color scheme
✅ Optimized for 21:9 and 32:9 displays
✅ Oversized controls for easy viewing
✅ Efficient use of horizontal space
✅ High contrast for readability
✅ Professional appearance
✅ Smooth animations
✅ Consistent design language
✅ Export/Import support
✅ Fully customizable

Enjoy your ultrawide InfoPanel experience! 🚀

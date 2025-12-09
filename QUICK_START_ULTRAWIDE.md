# Quick Start: Dark Blue Ultrawide Setup

## ⚡ 3-Step Setup

### Step 1: Launch InfoPanel
```
Run InfoPanel.exe
```

### Step 2: Open Appearance Settings
```
Click "Appearance" in the navigation menu
```

### Step 3: Apply Theme & Layout
```
1. Go to "Themes" tab
   → Select "Dark Blue Ultrawide"
   → Click "Apply Theme"

2. Go to "Layouts" tab
   → Select "Ultrawide Fullscreen"
   → Click "Apply Layout"

3. (Optional) Go to "Workspace" tab
   → Click "New Workspace"
   → Name it and save
```

## 🎯 What You Get

### Theme: Dark Blue Ultrawide
- Beautiful dark blue color scheme
- High contrast for large displays
- Professional appearance
- Smooth animations

### Layout: Ultrawide Fullscreen
- 6-column × 3-row grid
- 10 panel regions
- Optimized for 21:9 and 32:9 displays
- Oversized controls for easy viewing

## 📐 Layout Regions

```
┌─────────┬────┬────┬────┬────┬─────────┐
│ Hero L  │ C1 │ C2 │    │    │ Hero R  │
│ (Large) ├────┼────┤    │    │ (Large) │
│         │ C3 │ C4 │    │    │         │
├─────────┼────┼────┼────┼────┼─────────┤
│ Bot L   │BC1 │BC2 │    │    │ Bot R   │
└─────────┴────┴────┴────┴────┴─────────┘
```

**10 Regions:**
1. Hero Left (2×2) - Large feature panel
2. Hero Right (2×2) - Large feature panel
3-6. Center Panels (1×1) - Standard metrics
7-10. Bottom Panels (2×1 or 1×1) - Info bar

## 🎨 Color Reference

**Quick Colors:**
- Primary: `#1E3A8A` (Deep Blue)
- Accent: `#3B82F6` (Bright Blue)
- Background: `#0F172A` (Very Dark)
- Text: `#F1F5F9` (Almost White)

**Semantic:**
- Success: `#10B981` (Green)
- Warning: `#F59E0B` (Amber)
- Error: `#EF4444` (Red)

## 💡 Panel Recommendations

**Hero Panels (Large):**
- System overview dashboards
- Large graphs/visualizations
- Media players
- Main status displays

**Center Panels (Medium):**
- CPU/GPU metrics
- Temperature sensors
- Network stats
- Storage info

**Bottom Panels (Wide):**
- Time and date
- Weather
- Quick stats
- System information

## 🔧 Customization

### Export & Modify
```
1. Click "Export Theme" or "Export Layout"
2. Edit the JSON file
3. Click "Import" to load your version
```

### Create Workspace
```
1. Set up your theme + layout
2. Go to Workspace tab
3. Click "New Workspace"
4. Name it and save
5. Load anytime with one click
```

## 📊 Supported Resolutions

✅ **3440×1440** (21:9 Ultrawide) - Perfect
✅ **5120×1440** (32:9 Super Ultrawide) - Excellent
✅ **2560×1080** (21:9 Standard) - Good
✅ **3840×1600** (24:10 Ultrawide) - Great

## 🎬 Features

- ✅ High contrast colors
- ✅ Oversized controls
- ✅ Efficient space usage
- ✅ Professional design
- ✅ Smooth animations
- ✅ Easy customization
- ✅ Export/Import support
- ✅ Workspace saving

## 🚀 Performance Tips

1. **Use Hero Panels** for resource-intensive visualizations
2. **Center Panels** for lightweight metrics
3. **Bottom Panels** for static information
4. **Adjust gap/padding** in layout JSON if needed

## 📝 Files Created

```
InfoPanel/
├── Themes/
│   └── dark-blue-ultrawide.json
└── Layouts/
    └── ultrawide-fullscreen.json
```

These files are automatically copied to the output directory during build.

## 🆘 Troubleshooting

**Theme not showing?**
- Rebuild the project
- Check Themes folder exists in output directory
- Restart InfoPanel

**Layout not applying?**
- Ensure layout is selected before clicking Apply
- Check Layouts folder exists in output directory
- Try restarting InfoPanel

**Colors look wrong?**
- Verify theme is applied (check Themes tab)
- Check display color settings
- Try re-applying the theme

## 📚 More Information

See full documentation:
- `ULTRAWIDE_THEME_LAYOUT.md` - Complete guide
- `THEME_LAYOUT_PREVIEW.md` - Visual preview
- `APPEARANCE_PAGE_IMPLEMENTATION.md` - Technical details

---

**Enjoy your ultrawide InfoPanel experience!** 🎉

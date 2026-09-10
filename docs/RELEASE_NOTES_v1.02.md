# MDPlus v1.02 Release Notes

> **Release Date:** September 10, 2026  
> **Tag:** `v1.02`  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.02 brings a major visual and operational upgrade. From Windows 11 Fluent dynamic pill scrollbars and three brand-new light themes to dynamic Windows DWM title bar styling, WCAG AA contrast enhancements, and robust one-click updates with UAC elevation, v1.02 makes MDPlus feel faster, more native, and more polished than ever.

---

## 🚀 Key Feature Highlights

### 1. Modern Fluent Dynamic Pill Scrollbars
- **Fluent UI Pill Aesthetic:** Replaced legacy Aero2/classic scrollbars with Windows 11 Fluent dynamic pill scrollbars featuring rounded thumb geometries, adaptive track spacing, and semi-transparent idle visibility.
- **Dynamic Hover & Expand:** Scrollbar thumbs subtly expand (from 6px to 8px) and brighten on hover/drag for effortless navigation while fading back into the background when reading to maximize focus.
- **Zero Layout Jitter:** Clean 10px fixed layout reservation prevents text reflow during hover transitions.
- **Consistent Viewport Polish:** Globally styled across the primary document viewer, rich Markdown previewer, raw editor, and Table of Contents (TOC) sidebar.

### 2. Three New Designer Light Themes & Menu Subgrouping
- **One Light:** Crisp, modern light palette inspired by Atom One Light, featuring balanced cool-gray backgrounds (`#FAFAFA`), subtle borders, and vivid syntax highlighting.
- **Solarized Light:** The classic precision palette by Ethan Schoonover, with iconic warm cream paper tones (`#FDF6E3`) and mathematically tuned contrasting accents.
- **Quiet Light:** Gentle, low-contrast light theme inspired by VS Code Quiet Light, offering soft lavender and muted slate hues for strain-free daytime reading.
- **Structured Theme Menu:** Organized the **Theme** menu into intuitive **Dark Themes** (Dark+, Monokai, Nord, Dracula, GitHub Dark) and **Light Themes** (GitHub Light, One Light, Solarized Light, Quiet Light) submenus with active checkmark indicators.

### 3. Table of Contents (TOC) Readability Overhaul
- **WCAG AA High-Contrast Compliance:** Re-engineered typography and brush hierarchies across the Table of Contents sidebar for all light themes, achieving contrast ratios exceeding **7:1** (up to **14.2:1**).
- **Clear Visual Hierarchy:** Heading levels 1 through 6 now feature differentiated indentations, prominent section indicators, and legible hover highlights that prevent washed-out text.
- **Active Heading Tracking:** Instant visual feedback when navigating or clicking section anchors.

### 4. Native Windows DWM Title Bar Dynamic Styling
- **Direct Desktop Window Manager (DWM) Integration:** Utilizes native Windows Desktop Window Manager APIs (`DwmSetWindowAttribute`) to dynamically paint the system window caption, title bar background, and border to match the active theme.
- **Dark/Light Mode Sync:** Synchronizes `DWMWA_USE_IMMERSIVE_DARK_MODE` and custom RGB COLORREFs (`DWMWA_CAPTION_COLOR`, `DWMWA_TEXT_COLOR`, `DWMWA_BORDER_COLOR`).
- **State Resilience:** Reliably maintains custom title bar theming across minimize/restore, maximize, and full-screen (`F11`) toggle transitions on Windows 10 (1809+) and Windows 11.

### 5. Session Restore & Tab Lifecycle Enhancements
- **Automatic Session Restoration:** Preserves open document paths, active tab selection, and viewport positions across app launches (configurable in Settings).
- **Tab Productivity Shortcuts:** Full keyboard navigation support with `Ctrl + Tab` (next tab), `Ctrl + Shift + Tab` (previous tab), and `Ctrl + W` (close tab).
- **Single-Click Tab Close:** Sleek, integrated tab close buttons with unsaved change prompts protecting your work.
- **Empty State Shell:** Elegant welcome shell when all tabs are closed, offering quick actions to open or create documents.

### 6. Integrated GitHub Release Updater with UAC Elevation
- **Automated Startup Checks:** Non-blocking background update checks debounced to once every 24 hours to conserve network bandwidth and respect GitHub API rate limits.
- **1-Click Update Dialog:** Interactive modal showing release version differences, full release highlights, and real-time download progress.
- **Cryptographic SHA-256 Verification:** Automatically fetches published `SHA256SUMS.txt` manifests and verifies the downloaded `MDPlus-Setup.exe` cryptographic checksum before launching.
- **Reliable UAC Elevation & Clean Exit:** Fixes an issue where updates would not trigger after hash verification. Now reliably prompts for administrator privileges (`runas` verb) for machine-wide setup, isolates the working directory to `%TEMP%\MDPlusUpdate` to prevent file locks, and smoothly closes MDPlus while saving your workspace session state.

---

## 🛡️ Notepad++ Standard Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
```

---

## 📦 Official Release Downloads

- [**`MDPlus-Setup.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper
- [**`MDPlus.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus.exe) — Standalone Portable Single-File Binary
- [**`MDPlus-win-x64.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**`MDPlus-1.02-src.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-1.02-src.zip) — Source Code Archive
- [**`SHA256SUMS.txt`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

---

## 🧪 Verification & Test Suite Results

- **Unit Test Suite:** 95 / 95 Passed (0 Failed, 0 Warnings)
- **Opaque-Box E2E Suite:** 53 / 53 Passed across all 5 tiers

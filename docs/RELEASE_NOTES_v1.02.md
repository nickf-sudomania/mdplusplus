# MDPlus v1.02 Release Notes

> **Release Tag:** `v1.02`  
> **Release Date:** September 9, 2026  
> **Architecture:** Windows 64-bit (`win-x64`)  
> **Framework:** .NET 8.0 Windows Desktop Runtime  

---

## 🌟 Executive Summary

MDPlus **v1.02** is a major feature and refinement release delivering an elevated, modern Windows 11 Fluent aesthetic, expanded theme choices, enhanced reading readability in light environments, seamless window and session lifecycle management, and a hardened cryptographic auto-updater.

Whether reading large documentation trees, reviewing Markdown reports, or editing documents across multiple tabs, v1.02 makes MDPlus feel faster, more native, and more polished than ever.

---

## 🚀 Key Feature Highlights

### 1. Modern Fluent Dynamic Pill Scrollbars
- **Fluent UI Pill Aesthetic:** Replaced legacy Aero2 scrollbars with Windows 11 Fluent dynamic pill scrollbars featuring rounded thumb geometries, adaptive track spacing, and semi-transparent idle visibility.
- **Dynamic Hover & Expand:** Scrollbar thumbs subtly expand and brighten on hover/drag for effortless navigation while fading back into the background when reading to maximize focus.
- **Consistent Viewport Polish:** Seamlessly styled across the primary document viewer, rich Markdown previewer, and Table of Contents (TOC) sidebar.

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
- **State Resilience:** Reliably maintains custom title bar theming across minimize/restore, maximize, and full-screen (F11) toggle transitions on Windows 10 (1809+) and Windows 11.

### 5. Session Restore & Tab Lifecycle Enhancements
- **Automatic Session Restoration:** Preserves open document paths, active tab selection, and viewport positions across app launches (configurable in Settings).
- **Tab Productivity Shortcuts:** Full keyboard navigation support with `Ctrl + Tab` (next tab), `Ctrl + Shift + Tab` (previous tab), and `Ctrl + W` (close tab).
- **Single-Click Tab Close:** Sleek, integrated tab close buttons with unsaved change prompts protecting your work.
- **Empty State Shell:** Elegant welcome shell when all tabs are closed, offering quick actions to open or create documents.

### 6. Integrated GitHub Release Updater with UAC Elevation
- **Automated Startup Checks:** Non-blocking background update checks debounced to once every 24 hours to conserve network bandwidth and respect GitHub API rate limits.
- **1-Click Update Dialog:** Interactive modal showing release version differences, full release highlights, and real-time download progress.
- **Cryptographic SHA-256 Verification:** Automatically fetches published `SHA256SUMS.txt` manifests and verifies the downloaded `MDPlus-Setup.exe` cryptographic checksum before launching.
- **Reliable UAC Elevation & Clean Exit:** Fixes an issue where updates would not trigger after hash verification. Now reliably prompts for administrator privileges (`runas` verb) for machine-wide setup and smoothly closes MDPlus while saving your workspace session state.

---

## 🛡️ Notepad++ Standard Cryptographic Verification

All binaries and archives are verified using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

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

### Official SHA-256 Checksums:

```
cec8a604b1f3f51149610a70b548e0a0af4793adcd8563712b8836b594b4f3fa  MDPlus.exe
9bf8239e92b7a65e78433f4c5b894636a17257c31a1649b6e5903aeca8b1a138  MDPlus-win-x64.zip
60a630b3e14a1c232a79040ad42495dc103681a1c5e3f6aa6810a80de34f675e  MDPlus-1.02-src.zip
fcad772d8e98987240249659dbd72e3eeab8271ebb11bb81104a73e452db977f  MDPlus-Setup.exe
```

---

## 📦 Official Release Downloads

| Asset | Description | Size | SHA-256 Digest |
|---|---|---|---|
| [**`MDPlus-Setup.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-Setup.exe) | Official Windows Setup Installer with .NET 8 Runtime Bootstrapper | ~2.6 MB | `fcad772d8e98987240249659dbd72e3eeab8271ebb11bb81104a73e452db977f` |
| [**`MDPlus.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus.exe) | Standalone portable executable (single-file binary) | ~880 KB | `cec8a604b1f3f51149610a70b548e0a0af4793adcd8563712b8836b594b4f3fa` |
| [**`MDPlus-win-x64.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-win-x64.zip) | Portable zip package with sample documents & license | ~420 KB | `9bf8239e92b7a65e78433f4c5b894636a17257c31a1649b6e5903aeca8b1a138` |
| [**`MDPlus-1.02-src.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/MDPlus-1.02-src.zip) | Complete source code distribution archive | ~1.2 MB | `60a630b3e14a1c232a79040ad42495dc103681a1c5e3f6aa6810a80de34f675e` |
| [**`SHA256SUMS.txt`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.02/SHA256SUMS.txt) | Master SHA-256 checksum manifest | 332 B | *Matches published hashes above* |

---

## 🧪 Verification & Test Suite Results

- **Unit Test Suite:** 94 / 94 Passed (0 Failed, 1,416 ms execution time)
- **Opaque-Box E2E Suite:** 53 / 53 Passed across all 5 tiers (Feature Coverage, Boundaries, Combinations, Real-World, and Adversarial Hardening)
- **Accessibility & Contrast:** Verified WCAG AA contrast compliance across all 8 light and dark themes

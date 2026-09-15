# MDPlus v1.14.3 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.14.3  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14.3 fixes formula truncation for wide and complex LaTeX math expressions (such as fractions with extended multi-term denominators) by introducing horizontal overflow scrolling with seamless vertical document mouse-wheel pass-through.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Horizontal Overflow Scrolling for Display Math
- **`MathScrollViewer` Container:** Display math cards now wrap formula content in an automated horizontal scroll viewer (`HorizontalScrollBarVisibility = Auto`).
- **No Text Truncation:** Wide formulas with verbose fractions, long equations, and multi-term denominators no longer clip at the right edge of the card or viewport.
- **Card Boundary Integrity:** The rounded card container (`Border`) stays neatly bounded within the document margins and column width, with horizontal scrolling happening smoothly inside the card.
- **Compact Formats Preserved:** Short formulas like `$$E = mc^2$$` remain compact and centered without unnecessary whitespace or scrollbars.

### 2. Seamless Mouse-Wheel Routing & Horizontal Navigation
- **Uninterrupted Document Scrolling:** Bypasses standard WPF `ScrollViewer` vertical wheel interception when Shift is not held, allowing natural mouse-wheel scrolling to bubble directly to the document viewer without pausing or freezing.
- **Shift + Wheel Horizontal Scroll:** Holding Shift while using the mouse wheel smoothly scrolls formulas horizontally in responsive 30–60px increments.
- **Trackpad & Gesture Support:** Full native support for Windows precision touchpads (two-finger horizontal swipe) and scrollbar thumb dragging.

### 3. Version 1.14.3 Synchronization
- Synchronized version 1.14.3 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.14.3.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.14.3.checksums.sha256` alongside backwards-compatible manifests for all previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14.3-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14.3-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14.3-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/MDPlus-1.14.3-src.zip) — Source Code Archive
- [**MDPlus.1.14.3.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/MDPlus.1.14.3.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.3/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

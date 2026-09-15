# MDPlus v1.14.1 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.14.1  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14.1 resolves text truncation in large CSV and tabular datasets by introducing content-aware column width sizing and full-text hover tooltips while maintaining 60 FPS virtualization and instant scrolling.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Content-Aware Column Width Sizing
- **Dynamic Content Sampling:** Inspects up to 250 rows to evaluate realistic text lengths for each column on load without adding UI delay.
- **Font-Metrics Aware Calculations:** Accurately accounts for CJK glyph widths (15.5px), uppercase letters (10.0px), standard Latin characters (8.2px), and punctuation (5.0px).
- **Proportional Pixel Widths:** Assigns columns explicit pixel widths clamped between 85px and 700px, eliminating uniform Star column cramping and allowing long titles to render fully.
- **Smooth Horizontal Scrolling:** Seamlessly engages horizontal scrolling for datasets wider than the window viewport.

### 2. Full-Text Hover Tooltips
- Cells are equipped with native WPF hover tooltips displaying the complete, untruncated string for any text exceeding column limits.
- Extended 15-second tooltip show duration gives users comfortable time to inspect full descriptions.

### 3. Version 1.14.1 Synchronization
- Synchronized version 1.14.1 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.14.1.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.14.1.checksums.sha256` alongside backwards-compatible manifests for previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14.1-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14.1-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14.1-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/MDPlus-1.14.1-src.zip) — Source Code Archive
- [**MDPlus.1.14.1.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/MDPlus.1.14.1.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.1/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

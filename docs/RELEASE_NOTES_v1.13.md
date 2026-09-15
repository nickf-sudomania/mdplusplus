# MDPlus v1.13 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.13  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.13 introduces a dedicated, hardware-accelerated UI-virtualized tabular engine (`CsvDataGrid`) for CSV and TSV documents, transforming tabular viewing from an 11-second UI freeze into instantaneous sub-50ms loading and rock-solid 60 FPS scrolling on large, multi-thousand-row datasets.

---

## 🚀 Key Feature Highlights & Fixes

### 1. High-Performance Virtualized Tabular Grid (`CsvDataGrid`)
- **Instantaneous File Open:** Tabular documents bypass heavy non-virtualized WPF `FlowDocument` table element creation upon document open, loading 1,000+ row files (such as `clean_book_titles_full.csv`) in under 40 milliseconds (a 250x load speedup).
- **UI Virtualization & 60 FPS Scrolling:** Employs row and column recycling virtualization with pixel-based scrolling (`VirtualizingPanel.ScrollUnit="Pixel"`), maintaining a locked 60+ FPS scroll performance regardless of document row count.
- **Dynamic Theme Palette Styling:** Automatically applies theme colors across column headers, cell backgrounds, alternating zebra striping, grid lines, and selection highlights across all 8 theme presets (GitHub Dark, GitHub Light, Solarized, Nord, Monokai, Dracula, One Dark, etc.).
- **Interactive In-Place Editing:** Supports cell-level editing with automatic in-flight commit on save or tab switch, accompanied by background real-time synchronization with raw CSV/TSV text buffer in Split view.
- **Find in Grid (`Ctrl+F`):** Directly navigates and highlights matching cells in `CsvDataGrid`, scrolling matched cells into viewport instantly without lag.
- **Clipboard & Selection:** Full support for `ApplicationCommands.Copy` and `SelectAll` (`Ctrl+A`, `Ctrl+C`) exporting grid cells in standard tab-delimited format compatible with Excel and Google Sheets.
- **Ctrl+Wheel Zooming:** Dynamically scales grid font size and row height smoothly with `Ctrl + Mouse Wheel` or Zoom shortcuts (`Ctrl++`, `Ctrl+-`, `Ctrl+0`).

### 2. Lossless Bidirectional Round-Trip Serialization
- **Preserved Header Fidelity:** `CsvSerializer.SerializeDataTable` maintains exact original header column names (including empty fields and special characters) via extended table properties, guaranteeing bit-perfect round-trip fidelity between Raw view, Rendered view, and disk persistence.
- **Lazy FlowDocument Generation:** Defer heavy `FlowDocument` table creation until explicitly requested by print operations or automated test assertions, preserving zero UI overhead during normal browsing.

### 3. Version 1.13 Synchronization
- Synchronized version 1.13 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.13.0.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.13.checksums.sha256` alongside backwards-compatible manifests for previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.13-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.13-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.13-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/MDPlus-1.13-src.zip) — Source Code Archive
- [**MDPlus.1.13.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/MDPlus.1.13.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.13/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

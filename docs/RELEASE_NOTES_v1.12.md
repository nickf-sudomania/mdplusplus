# MDPlus v1.12 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.12  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.12 delivers performance hardening, memory leak mitigations, and robust FlowDocument Table layout stabilization for tabular data files (CSV, TSV), ensuring smooth UI responsiveness, in-place editing, and full search accessibility.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Robust FlowDocument Tabular Architecture & Starvation Protection
- **FlowDocument Table Layout:** Restored clean WPF FlowDocument Table elements for CSV and TSV files with semi-bold headers, theme-adaptive borders, alternating row striping, and automatic numeric right-alignment.
- **UI Starvation Guardrails:** Capped visual table rows at 3,000 rows (`MaxVisualRows`) with a styled notice banner directing users to Raw view (`Ctrl+3`) for arbitrarily large data streams.
- **Search & In-Place Editing Fidelity:** Ensured all tabular data remains fully searchable via `FindBar` (`Ctrl+F`), selectable, and editable in-place without losing document formatting.
- **Hoisted Layout Resources:** Hoisted theme brushes, border thicknesses, and margins during table generation to eliminate allocations and GC pressure.

### 2. Large File Data-Loss Safeguard & Memory Hardening
- **Lossless Stream Retention:** Added `IsVisualCapped` protection across `DocumentTabItem.Save()` and `MainWindow.SyncTabFromControls()`. Visual preview caps (3,000 CSV rows or 2,500 JSON lines) can never overwrite the underlying raw text buffer, guaranteeing 100% data retention when saving large datasets.
- **Read-Only Capped Previews:** FlowDocument views exceeding visual caps are set to read-only while preserving full text selection, copying, and searchability (`Ctrl+F`), directing users to Raw view (`Ctrl+3`) for editing large datasets.
- **External Reload State Cleanliness:** Fixed external file watcher reload in `MainWindow` so programmatic updates suppress dirty tracking and mark tabs clean, preventing spurious dirty flags upon background file modifications.
- **RFC 4180 / TSV Delimiter Parsing:** Fixed edge-case in `CsvParser` where delimiter-only whitespace (e.g. single tab `\t`) was incorrectly dropped, ensuring single-tab TSV files parse cleanly into two empty columns.
- **Zero-Allocation Line Counting:** Optimized `StatsText` across text and tabular documents using allocation-free line scanning instead of array allocations.
- **Window Event Lifecycles:** Guaranteed that modal and tool dialogs cleanly unhook global event subscriptions (`ThemeManager.Instance.ThemeChanged`) upon closing.

### 3. Version 1.12 Synchronization
- Synchronized version 1.12 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.12.0.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.12.checksums.sha256` alongside backwards-compatible manifests for previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.12-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.12-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.12-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/MDPlus-1.12-src.zip) — Source Code Archive
- [**MDPlus.1.12.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/MDPlus.1.12.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.12/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

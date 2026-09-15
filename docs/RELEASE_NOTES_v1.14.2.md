# MDPlus v1.14.2 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.14.2  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14.2 restores equal-width Star columns that fit entirely within the viewport and introduces text wrapping with dynamic row heights, so all cell content is fully visible without horizontal scrolling or text truncation.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Equal-Width Star Columns (No Horizontal Scroll)
- **Viewport-Fitted Layout:** All columns evenly divide the available window width using WPF `DataGridLengthUnitType.Star`, eliminating horizontal scrollbars entirely.
- **Minimum Column Width:** Each column enforces a 60px minimum to remain readable even with many columns.

### 2. Text Wrapping with Dynamic Row Heights
- **Full Text Visibility:** Replaced `TextTrimming.CharacterEllipsis` with `TextWrapping.Wrap`, so long strings wrap within their cell instead of being truncated.
- **Auto-Sizing Rows:** Removed fixed `RowHeight` in favor of `Double.NaN` (auto), allowing each row to grow dynamically to accommodate wrapped text content.
- **Minimum Row Height:** Maintains a comfortable 32px minimum row height that scales proportionally with zoom level.

### 3. Editing Consistency
- **Wrapped Editing:** In-cell text editors also wrap text and use top-aligned layout, matching the display style.
- **Theme Re-Apply Parity:** Theme switching preserves text wrapping and top alignment in editing styles.

### 4. Dead Code Removal
- Removed `EstimateColumnWidth()` and `MeasureTextWidth()` helper methods (39 lines) that are no longer used with Star column sizing.

### 5. Version 1.14.2 Synchronization
- Synchronized version 1.14.2 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.14.2.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.14.2.checksums.sha256` alongside backwards-compatible manifests for previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14.2-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14.2-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14.2-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/MDPlus-1.14.2-src.zip) — Source Code Archive
- [**MDPlus.1.14.2.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/MDPlus.1.14.2.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.2/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

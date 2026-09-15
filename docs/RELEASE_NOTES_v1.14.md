# MDPlus v1.14 Release Notes

> **Release Date:** September 15, 2026  
> **Tag:** v1.14  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14 pairs the sub-50ms virtualized tabular rendering engine introduced in v1.13 with beautifully polished cell formatting, generous breathing room, and smart column alignment.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Polished Cell Formatting & Typography
- **Generous 12px Cell Padding:** Cell text now enjoys generous 12px horizontal and 4px vertical padding, eliminating cramped layouts and keeping text elegantly spaced away from grid boundaries.
- **Automatic Numeric & Currency Right-Alignment:** Inspects column data to detect numbers, currency amounts ($, €, £, ¥), percentages (%), and accounting formats (123.45), automatically right-aligning both data cells and column headers.
- **Crisp Header Divider & Styling:** Column headers feature semi-bold typography, matching alignments, 12px padding, and a distinct 2px bottom border dividing headers from data rows.
- **Clean In-Cell Editors:** In-cell text editors automatically inherit the active theme palette, generous padding, and numeric alignment, with no jarring system-default white backgrounds or dotted focus rectangles (FocusVisualStyle = null).
- **Comfortable 36px Row Height:** Base row height set to a comfortable 36px with smooth zoom scaling (Ctrl + Scroll).

### 2. Full Retention of 60 FPS Virtualization & Data Integrity
- Virtualized row and column recycling ensures multi-thousand-row spreadsheets load instantaneously (< 40ms) and scroll buttery-smooth at 60 FPS without layout jitter.
- Complete bidirectional round-trip fidelity between Raw view, Rendered view, and disk persistence.

### 3. Version 1.14 Synchronization
- Synchronized version 1.14 across MDPlus.csproj, AssemblyInfo.cs (1.14.0.0), MainWindow.xaml, UpdateService.cs, Inno Setup installer script (MDPlus.iss), uild.ps1, welcome.md, and README.md.
- Generated multi-format release manifests including MDPlus.1.14.checksums.sha256 alongside backwards-compatible manifests for previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

`powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14-src.zip -Algorithm SHA256
`

`cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14-src.zip SHA256
`

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/MDPlus-1.14-src.zip) — Source Code Archive
- [**MDPlus.1.14.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/MDPlus.1.14.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

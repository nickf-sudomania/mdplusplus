# MDPlus v1.14.5 Release Notes

> **Release Date:** September 29, 2026  
> **Tag:** v1.14.5  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14.5 delivers a precision UI rendering fix that corrects baseline alignment for simple inline math expressions, ensuring inline equations perfectly align with surrounding text typography instead of rendering like superscript elements.

---

## ?? Key Feature Highlights & Fixes

### 1. Typographic Baseline Alignment for Inline Math
- **Baseline-Anchored Alignment:** Corrected BaselineAlignment.Center to BaselineAlignment.Baseline in MarkdownToWpfConverter when parsing inline math $ ... $ elements into InlineUIContainer blocks.
- **Renderer Spacing Adjustments:** Removed a counter-productive hardcoded negative margin hack (Margin = new Thickness(1, 3, 1, -3)) inside LatexMathRenderer that was fighting WPF's native baseline flow.
- **Typographic Parity:** Simple inline math elements (like < 1.00, =0, or variables like CCC) now flow naturally on the text baseline without floating 4px higher than standard typography.

### 2. Version 1.14.5 Synchronization
- Synchronized version 1.14.5 across MDPlus.csproj, AssemblyInfo.cs (1.14.5.0), MainWindow.xaml, UpdateService.cs, Inno Setup installer script (MDPlus.iss), uild.ps1, welcome.md, and README.md.
- Generated multi-format release manifests including MDPlus.1.14.5.checksums.sha256 alongside backwards-compatible manifests for all previous versions.

---

## ?? Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

`powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14.5-src.zip -Algorithm SHA256
`

`cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14.5-src.zip SHA256
`

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14.5-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/MDPlus-1.14.5-src.zip) — Source Code Archive
- [**MDPlus.1.14.5.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/MDPlus.1.14.5.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.5/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

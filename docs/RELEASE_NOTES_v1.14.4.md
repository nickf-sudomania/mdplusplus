# MDPlus v1.14.4 Release Notes

> **Release Date:** September 17, 2026  
> **Tag:** v1.14.4  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.14.4 delivers a surgical UI readability enhancement: theme-aware text cursor (caret) colors across all dark and light themes, ensuring high-contrast caret visibility on dark editor and input backgrounds.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Theme-Aware Text Cursor (Caret) Color Parity
- **High-Contrast Caret in Dark Themes:** Previously, the text cursor could default to standard black on dark backgrounds in themes such as GitHub Dark, Nord, One Dark, and Monokai, making the insertion point difficult to locate. The cursor (`CaretBrush`) is now dynamically synchronized to match the active text color (`ForegroundBrush` / `palette.EditorFg`).
- **Universal Application Across All Input Surfaces:**
  - **Rendered Editor:** `MarkdownScrollViewer` sets `CaretBrush` to the active theme text brush in both XAML and runtime theme switches.
  - **Raw Markdown Source Editor:** `RawMarkdownTextBox` dynamically updates `CaretBrush` on theme changes (`ApplyTheme`).
  - **Find / Search Bar:** `FindTextBox` binds `CaretBrush` directly to its dynamic `Foreground`.
  - **Integrity Verification Dialog:** File path and checksum input textboxes dynamically synchronize `CaretBrush` with theme foreground colors.
  - **Global WPF Controls:** Global `TextBox` and `RichTextBox` styles bind `CaretBrush="{DynamicResource ForegroundBrush}"`.

### 2. Version 1.14.4 Synchronization
- Synchronized version 1.14.4 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.14.4.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.14.4.checksums.sha256` alongside backwards-compatible manifests for all previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.14.4-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.14.4-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.14.4-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/MDPlus-1.14.4-src.zip) — Source Code Archive
- [**MDPlus.1.14.4.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/MDPlus.1.14.4.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.14.4/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

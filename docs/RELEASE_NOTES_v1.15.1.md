# MDPlus v1.15.1 Release Notes

> **Release Date:** October 1, 2026  
> **Tag:** v1.15.1  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.15.1 delivers critical layout and typography enhancements across Mermaid flowcharts, inline mathematics, and UI scrollbar ergonomics.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Mermaid Flowchart Bidirectional Connectors & Collision Prevention
- **Bidirectional & Reverse Arrow Syntax:** Added full parsing, AST modeling, and dual-tangent vector arrowheads for bidirectional (`<-->`, `<==>`, `<-.->`) and reverse (`<--`, `<==`, `<-.-`) connectors.
- **Elimination of Orphaned `<` Boxes:** Resolved a syntax tokenizer edge case where `<-->` caused the leading `<` to be parsed as a standalone node.
- **Edge Label De-Collision:** Introduced bounding-box overlap detection in `MermaidLayoutEngine` that automatically detects intersecting edge labels and offsets colliding pills with a vertical de-collision buffer.

### 2. Precision Inline Math Typographic Unification & Harmonization
- **Unified TextBlock Merging:** Adjacent relational operators and digits (such as `$< 0$`, `$\ge 4.5$`, `$\le 0$`) now merge into a single `TextBlock` element, guaranteeing that symbols and numbers render on the exact same OpenType font baseline.
- **MathFont / TextFont Baseline Harmonization:** When formulas mix text mode (`\text{...}`) with mathematical symbols, Cambria Math glyphs are automatically harmonized with Segoe UI text baselines to eliminate optical baseline steps.
- **Display Math List Promotion & Clean Spacing:** Display math blocks (`$$...$$`) within list items are cleanly promoted to dedicated block containers with leading and trailing continuation line breaks trimmed, eliminating unwanted double gaps.

### 3. Scrollbar Ergonomics & Thumb Usability
- **Expanded Hit Target:** Widened the scrollbar track from 10px to **14px** and configured `Background="Transparent"` on the thumb root grid so clicking anywhere along the track reliably initiates dragging.
- **Enlarged Minimum Size:** Increased thumb `MinHeight` and `MinWidth` from 24px to **40px**, ensuring the thumb remains easily visible and clickable even on massive documents.
- **High-Contrast Dark Mode Readability:** Switched the resting thumb brush to high-contrast `MutedForegroundBrush` (`#8B949E`), widened the visual indicator from 6px to **8px** (expanding to 10px on hover, 12px on drag), and increased resting opacity to **0.65** (0.90 on hover, 1.0 on drag).

### 4. Version 1.15.1 Synchronization
- Synchronized version 1.15.1 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.15.1.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.15.1.checksums.sha256` alongside backwards-compatible manifests for all previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.15.1-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.15.1-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.15.1-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/MDPlus-1.15.1-src.zip) — Source Code Archive
- [**MDPlus.1.15.1.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/MDPlus.1.15.1.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.1/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

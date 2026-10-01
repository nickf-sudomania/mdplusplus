# MDPlus v1.15.0 Release Notes

> **Release Date:** October 1, 2026  
> **Tag:** v1.15.0  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.15.0 introduces two major visual layout enhancements: a **pure native C# vector rendering engine for Mermaid flowcharts** and **precision typographic baseline calibration for inline LaTeX mathematics**.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Pure Native C# Mermaid Flowchart Engine
- **Zero-Dependency Architecture:** Fully native .NET 8 WPF parser and vector renderer implemented from scratch without Chromium, WebViews, or any external NuGet packages. Maintains MDPlus's signature instant startup (<150ms) and lightweight ~1.2 MB single-file footprint.
- **Syntax & AST Support:**
  - **Orientations:** `TD`, `TB`, `BT`, `LR`, `RL` across both `graph` and `flowchart` directives.
  - **5 Node Shapes:** Rectangles `[text]`, Rounded `(text)`, Circles `((text))`, Diamonds `{text}`, and Stadium pills `([text])`.
  - **6 Connection Styles:** Normal `-->`, Dotted `-.->`, Thick `==>`, Open `---`, Dotted Open `-.-`, and Thick Open `===`.
  - **Edge Labels:** Inline (`-- label -->`) and pipe (`-->|label|`) connectors.
  - **Subgraphs & Comments:** Nested subgraph clusters and comment (`%%`) handling.
- **Sugiyama Layered Graph Layout:** Native 5-phase algorithm handling cycle reversal, ranking, crossing minimization, non-overlapping coordinate positioning, and smooth cubic Bézier spline routing with rotated arrowheads.
- **Dynamic Theming & 60 FPS Scrolling:** Nodes, strokes, and labels automatically synchronize with `ThemePalette` across all light and dark themes (WCAG AA/AAA compliant). Mouse wheel events bubble through `MermaidScrollViewer` for uninterrupted 60 FPS document scrolling.
- **Graceful Fallback:** Malformed or unsupported syntax safely falls back to standard syntax-highlighted code blocks with the language badge and copy button.

### 2. Inline Math Typographic Baseline & Optical Alignment
- **Zero-Sum Proportional Vertical Offset:** Fixed the regression where simple inline formulas (`$ROIC > 18\%$`, `$> 18\%$`, `$\ge +1.5\%$`, `$\le 30\times$`) floated elevated above text lines like superscripts. Restored `BaselineAlignment.Center` with font size-proportional margins `(1, vOffset, 1, -vOffset)` where $\text{vOffset} = \text{round}(\text{fontSize} \times \frac{2.5}{14.5}, 1)$.
- **Ambient Context Font Sizing:** Propagates ambient font sizes from headings, list items, and paragraphs to ensure exact 0.00 DIPs optical baseline parity with surrounding text runs across all layouts.

### 3. Version 1.15.0 Synchronization
- Synchronized version 1.15.0 across `MDPlus.csproj`, `AssemblyInfo.cs` (1.15.0.0), `MainWindow.xaml`, `UpdateService.cs`, Inno Setup installer script (`MDPlus.iss`), `build.ps1`, `welcome.md`, and `README.md`.
- Generated multi-format release manifests including `MDPlus.1.15.0.checksums.sha256` alongside backwards-compatible manifests for all previous versions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.15.0-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.15.0-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.15.0-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/MDPlus-1.15.0-src.zip) — Source Code Archive
- [**MDPlus.1.15.0.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/MDPlus.1.15.0.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.0/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

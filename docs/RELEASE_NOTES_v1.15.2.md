# MDPlus v1.15.2 Release Notes

> **Release Date:** October 1, 2026  
> **Tag:** v1.15.2  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.15.2 delivers pixel-perfect typographic baseline alignment across inline code pills, HTML keycaps/code, and mathematical equations, along with robust currency delimiter protection against greedy TeX pairing.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Typographic Baseline Alignment & Punctuation Grounding
- **Baseline Offset Recalibration:** Fixed an overcompensation regression where a `+2.5` top margin pushed inline code pills (`CodeInline`) and math formulas (`LatexMathRenderer`) ~3 DIPs below the font baseline.
- **Micro-Metric Font Collinearity:** Replaced hardcoded margins with a font-size proportional formula:
  $$\text{vOffset} = \text{round}\left(\text{fontSize} \times -\frac{1.0}{15.0}, 1\right)$$
  ensuring Cascadia Code and Cambria Math glyphs sit on the exact same typographic baseline ($y=115$ px) as Segoe UI text across all headings and line heights.
- **HTML Element Parity:** Synchronized `<kbd>` keycaps and `<code>` tags in `HtmlWpfRenderer.cs` with the new calibrated baseline margin `(-1.0, 1.0)`.
- **Punctuation Alignment:** Colons (`:`), commas (`,`), and parentheses (`(` and `)`) adjacent to inline pills now sit naturally and symmetrically on the text baseline without floating or sinking.

### 2. Currency Delimiter Protection
- **Greedy Math Delimiter Isolation:** Enhanced `MarkdownParser.cs` single-dollar inline math scanning with whitespace lookahead and boundary checking.
- **Prose Preservation:** Unescaped currency amounts (e.g. `$2B`, `$10M`, `$100`) are cleanly preserved as literal currency text rather than pairing with subsequent equations (`$\le$`, `$ROE$`) and collapsing intervening text into italic math blocks.

### 3. Comprehensive Verification & Independent Visual QA
- **Unit & E2E Test Suites:** 100% pass rate across all 458 automated test cases (172 unit tests + 286 E2E tests across Tiers 1–5).
- **Independent Visual QA:** Verified by vision-enabled subagent performing micro-metric pixel analysis on high-resolution rendered bitmaps with 1px typographic baseline guide overlays.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.15.2-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.15.2-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.15.2-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/MDPlus-1.15.2-src.zip) — Source Code Archive
- [**MDPlus.1.15.2.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/MDPlus.1.15.2.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.2/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

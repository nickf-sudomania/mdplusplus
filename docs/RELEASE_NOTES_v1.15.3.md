# MDPlus v1.15.3 Release Notes

> **Release Date:** October 8, 2026  
> **Tag:** v1.15.3  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.15.3 introduces a native, zero-dependency WPF Print Preview and printing subsystem powered by an in-memory XPS pipeline, dynamic running headers/footers with automatic page numbering, customizable paper and margin presets, and page range slicing.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Native Print Preview & XPS Printing Subsystem
- **Dedicated Print Preview Window (`PrintPreviewWindow`):** A modern preview window equipped with zoom controls (Fit Page, Fit Width, 50%, 75%, 100%, 150%, 200%), keyboard shortcuts, page navigation (Previous/Next, jump to page), and one-click printing.
- **In-Memory XPS Generation (`PrintDocumentBuilder`):** High-fidelity document pagination converted directly to XPS format in-memory using `MemoryStream` and WPF `PackageStore`—requiring zero temporary files and zero disk I/O.
- **Dynamic Running Headers & Footers (`HeaderFooterDocumentPaginator`):** Automatic header containing document title and print date, and footer displaying "Page X of Y" with crisp hairline separator rules.
- **Paper & Margin Presets (`PrintSettings`):** Built-in support for Letter, A4, and Legal page sizes in Portrait and Landscape orientations, with Normal (1"), Narrow (0.5"), and Wide (1.5") margin presets.
- **Light Theme Print Optimization:** Enforces crisp, paper-optimized light styling with proper high-contrast text and border treatments during pagination, regardless of whether dark mode is currently active in the viewer.
- **Page Range Slicing (`PageRangeDocumentPaginator`):** Full support for printing all pages, the current page, or arbitrary page selections (e.g. `1-3, 5`).
- **Seamless Shortcuts & Menus:** Integrated into the File menu and Hamburger menu with `Ctrl+P` and `Ctrl+Shift+P` shortcuts.

### 2. Comprehensive Test Suite
- **100% Pass Rate:** 464 total automated tests (178 unit tests and 286 E2E tests across Tiers 1–5) passing with zero regressions.
- **Print Subsystem Verification:** Dedicated test cases verifying `PrintSettings`, paginator slicing, header/footer measurement, and XPS package generation.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.15.3-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.15.3-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.15.3-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/MDPlus-1.15.3-src.zip) — Source Code Archive
- [**MDPlus.1.15.3.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/MDPlus.1.15.3.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.3/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

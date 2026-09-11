# MDPlus v1.1 Release Notes

> **Release Date:** September 11, 2026  
> **Tag:** v1.1  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.1 delivers a major reliability fix for the editing and saving pipeline across all supported file formats (Markdown, JSON, Plain Text, Logs, CSV/TSV Tables, and Configuration files), ensuring that edits made in Rendered, Raw, or Split views are faithfully and losslessly persisted to disk.

---

## 🚀 Key Feature Highlights & Bug Fixes

### 1. Multi-Format Lossless Saving Pipeline (Bug Fix)
- **Resolved Save Bug:** Fixed an issue where editing a `.json` document (or `.txt`, `.log`, `.ini`, `.cfg`, `.yaml`, `.xml`) in Rendered or Split mode and pressing Save (`Ctrl+S` / `File > Save`) failed to write modified text to disk because the save routine only updated Markdown and CSV FlowDocuments.
- **Dedicated FlowDocument Serializers:**
  - `JsonToFlowDocumentConverter.Serialize`: Intelligently extracts verbatim JSON text from the FlowDocument visual tree, discarding transient error banners, empty placeholders, and truncation notices.
  - `PlainTextToFlowDocumentConverter.Serialize`: Extracts clean multiline plain text, log, or config content without double-newline artifacts.
  - `DocumentFormatHelper.SerializeFlowDocument`: Unified serialization entry point routing FlowDocuments to their respective format serializers across all 10 supported formats.
- **Tab Lifecycle & Split-View Synchronization:**
  - Bidirectional synchronization between the active viewer controls (`MarkdownViewer`, `RawMarkdownTextBox`) and `DocumentTabItem` via `SyncTabFromControls`.
  - Edit-source tracking (`_lastEditSource`) ensures whichever pane was edited in Split view is honored as the source of truth without being clobbered by the other pane.
  - Switching tabs or toggling view modes now guarantees pending edits are flushed into the tab buffer before control content is replaced.

### 2. Universal File Format Support & Integrity
- **Markdown (.md, .markdown):** In-place WYSIWYG editing, CommonMark/GFM rendering, and lossless Markdown serialization.
- **Tabular Data (.csv, .tsv):** High-speed RFC 4180 parsing, interactive grid tables, and bidirectional CSV serialization.
- **Structured Data (.json):** 2-space pretty-printing, theme-based token syntax highlighting, and clean round-trip save support.
- **Plain Text & Logs (.txt, .log):** Ergonomic typography, zoom scaling, and verified disk persistence.
- **Configuration Files (.ini, .cfg, .yaml, .yml, .xml):** Verbatim preservation and syntax-friendly monospace viewing and editing.

### 3. Version 1.1 Synchronization
- Fully synchronized version numbers across `MDPlus.csproj` (1.1), `AssemblyInfo.cs` (1.1.0.0), `MainWindow.xaml` title, `UpdateService.cs`, Inno Setup script (`MDPlus.iss`), and `build.ps1`.
- Updated release verification manifests targeting `MDPlus.1.1.checksums.sha256`.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.1-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.1-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.1-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/MDPlus-1.1-src.zip) — Source Code Archive
- [**MDPlus.1.1.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/MDPlus.1.1.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.1/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

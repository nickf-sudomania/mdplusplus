# MDPlus v1.09 Release Notes

> **Release Date:** September 11, 2026  
> **Tag:** v1.09  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.09 expands the hyper-fast native Windows reader into a universal text and structured data viewer, adding native support for **Plain Text, CSV/TSV Tabular Data, and JSON Documents** with clean, highly readable formatted layouts while preserving sub-150ms startup speed, low memory footprint (< 35 MB), and zero external runtime dependencies.

---

## 🚀 Key Feature Highlights

### 1. Multi-Format Text Document Loading & File Type Detection
- **Universal Text Support:** Seamlessly open and edit standard text formats alongside Markdown:
  - Plain text & logs: `.txt`, `.log`
  - Tabular datasets: `.csv`, `.tsv`
  - Structured data: `.json`
  - Configuration files: `.ini`, `.cfg`, `.yaml`, `.yml`, `.xml`
- **Multi-Format Dialog Filters:** File > Open dialog defaults to "All Supported Files (`*.md`, `*.txt`, `*.log`, `*.csv`, `*.tsv`, `*.json`, `*.ini`, `*.cfg`, `*.yaml`, `*.yml`, `*.xml`)".
- **Multi-Source Ingestion:** Full support across drag-and-drop from Windows Explorer, command-line arguments, session restore, and new tab creation.
- **Visual Format Badges:** Tab headers feature clean, format-specific badges (`MD`, `CSV`, `TSV`, `JSON`, `LOG`, `TXT`, `CFG`, `XML`, `YAML`).

### 2. High-Legibility Formatted CSV & TSV Table Layouts
- **RFC 4180 Parsing:** Single-pass, allocation-conscious tokenizer accurately parses comma- and tab-delimited files, handling quoted fields, escaped quotes (`""`), and embedded line breaks.
- **Styled FlowDocument Tables:** Displays tabular data in clean, beautifully styled tables with distinct semi-bold headers, 1px grid borders, subtle alternating row stripes, and numeric right-alignment.
- **Rendered & Raw Monospace Toggle:** Instant toggling between Rendered Table view and Raw text view using `Ctrl+1` (Rendered) and `Ctrl+3` (Raw).
- **Bidirectional Round-Trip Fidelity:** Lossless serialization preserves quotes and structure when editing and saving.

### 3. Structured JSON Pretty-Printing & Syntax Highlighting
- **Zero-Baggage Formatting:** High-speed 2-space pretty-printing powered directly by .NET 8 built-in `System.Text.Json` (`JsonDocument` / `Utf8JsonWriter`) with zero external libraries.
- **Theme-Aware Syntax Coloring:** Token highlights (keys, strings, numbers, booleans, null) dynamically adapt to all 8 curated light and dark theme palettes (`GitHub Dark`, `GitHub Light`, `Nord`, `One Dark`, `Monokai`, `One Light`, `Solarized Light`, `Quiet Light`).
- **Resilient Error Fallback:** Malformed JSON gracefully displays raw text with an inline syntax notification banner without crashing or interrupting reading.

### 4. Plain Text & Log Typography
- **Optimized Font Stacks:** Cascadia Code / Consolas monospace typography for log files; Segoe UI for plain text documents.
- **Reader Controls:** Full support for smooth font zooming (`Ctrl + Plus/Minus/0`), line wrapping, and instant in-page find navigation (`Ctrl+F`).

### 5. Inno Setup File Associations & Shell Integration
- **Optional Association Tasks:** Installer includes configurable tasks for `.txt`, `.csv`, `.tsv`, and `.json` file associations.
- **Default Apps Registration:** Full integration with Windows "Default Apps" capabilities.
- **Shell Context Menu:** Registers right-click "Open with MDPlus" context menu verbs across all supported formats for quick access.

### 6. Performance & Memory Guardrails
- **Instant Cold Startup:** Launches in under 150 ms with zero background bloat.
- **Low Memory Footprint:** Consumes under 35 MB idle RAM (managed heap ~8–12 MB).
- **Sub-50ms 5,000-Line Parsing:** 5,000-line CSV and JSON documents parse in under 50 ms.
- **Zero External Dependencies:** Native WPF, DirectWrite, Direct3D, and .NET 8 BCL runtime only. No Chromium, Electron, or WebView2 baggage.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.09-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.09-src.zip SHA256
```

---

## 📦 Official Release Downloads

- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.09-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/MDPlus-1.09-src.zip) — Source Code Archive
- [**MDPlus.1.09.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/MDPlus.1.09.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.09/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

---

## 🧪 Verification & Test Suite Results

- **Unit Test Suite:** **112 / 112 Passed** (0 Failed, 0 Warnings)
- **E2E Test Suite:** **58 / 58 Passed** (0 Failed across Tiers 1–5)
- **Benchmark Suite:** 5,000-line Markdown parsing (< 25 ms), serialization (< 50 ms), CSV & JSON parsing (< 50 ms).

# MDPlus v1.08 Release Notes

> **Release Date:** September 10, 2026  
> **Tag:** 1.08  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.08 delivers **Hardened In-Reader Hyperlink Navigation & Bounded Hit-Testing**, addressing mouse event capture contention in the WPF reader engine, preventing empty margin false-positives, providing graceful feedback for missing files, and sanitizing release packaging.

---

## 🚀 Key Feature Highlights

### 1. Hardened Mouse Capture & Click Routing
- **Mouse Capture Management:** Resolved WPF RichTextBox internal text editor selection drag contention by caching the resolved Hyperlink reference directly on PreviewMouseLeftButtonDown in _mouseDownHyperlink and explicitly calling CaptureMouse() with e.Handled = true.
- **Text Selection Preservation:** Preserves smooth text selection drag capabilities with an intelligent micro-drag threshold (<= 6px) to comfortably accommodate finger jitter during physical clicks while preventing text editor drag sequences from hijacking clicks.
- **Clean Capture Release:** Safely releases mouse capture in a finally block on PreviewMouseLeftButtonUp and resets on OnLostMouseCapture.

### 2. Bounded Hit-Testing & Empty Margin Protection
- **Line Margin Guarding:** Enhanced FindHyperlinkAtPoint using GetPositionFromPoint(point, snapToText: true) combined with character bounding box validation via pointer.GetCharacterRect and explicit charRect.IsEmpty guarding.
- **Zero False-Positives:** Completely prevents empty whitespace and padding to the right of lines from falsely triggering hyperlinks.
- **Nested Inline Code Support:** Seamlessly resolves parent hyperlinks even when clicking inside nested inline code spans ([code_function()](api.md)) or styled text borders.

### 3. Asynchronous Tab Swapping
- **UI Tree Stability:** In src/MainWindow.xaml.cs, document loading and tab replacement are dispatched asynchronously via Dispatcher.BeginInvoke(...), allowing mouse up events to complete cleanly before the visual tree is replaced.

### 4. User Feedback on Missing Target Files
- **Non-Existent File Feedback:** When clicking a link to a missing markdown document, MDPlus now displays a status bar notification (File not found: <filename>) rather than failing silently.
- **Executable Blocking:** Blocks arbitrary executable files (.exe, .bat, .cmd, .pdf, .vbs) from executing through reader links, ensuring sandboxed reader safety.

### 5. Packaging & Build Pipeline Sanitation
- **Recursive Nesting Guard:** Fixed build.ps1 to prevent recursive directory nesting (releases/sample_docs/sample_docs/) during repeated publish runs.
- **Cryptographic Verification:** Master checksum manifest (SHA256SUMS.txt) and versioned manifests (MDPlus.1.08.checksums.sha256) generated automatically with strict SHA-256 verification.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

`powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
`

`cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
`

---

## 📦 Official Release Downloads

- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.08/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.08/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.08/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.08-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.08/MDPlus-1.08-src.zip) — Source Code Archive
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.08/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

---

## 🧪 Verification & Test Suite Results

- **Unit Test Suite:** **112 / 112 Passed** (0 Failed, 0 Warnings in 2.8s)
- **E2E Test Suite:** **58 / 58 Passed** (0 Failed across Tiers 1-5)

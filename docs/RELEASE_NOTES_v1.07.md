# MDPlus v1.07 Release Notes

> **Release Date:** September 10, 2026  
> **Tag:** `v1.07`  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.07 introduces **In-Reader Markdown Link Navigation & Heading Anchors**, enabling seamless jumping between related markdown documents, relative subpaths, and section anchors directly inside the reader without ever leaving the application.

---

## 🚀 Key Feature Highlights

### 1. In-Reader Markdown Link Navigation
- **Relative & Local Markdown Links:** Clicking markdown links (e.g. `[Guide](guide.md)`, `[Specs](../docs/spec.markdown)`, `file:///C:/path/doc.md`) or raw HTML `<a href="doc.md">` tags resolves paths relative to the active document's directory.
- **Supported Formats:** Transparently detects and opens `.md`, `.markdown`, `.mdown`, and `.mkd` files.
- **Fallback Resolution:** Resolves paths relative to current document base directory, working directory, application root, and `sample_docs/`.

### 2. Heading Anchor Scrolling & Slug Normalization
- **Anchor Jumping:** Supports both intra-document anchor links (e.g. `[Jump to Details](#details)`) and cross-document target anchors (e.g. `[Section](guide.md#advanced-configuration)`).
- **Slug Normalization:** Robust slug matching normalizes punctuation, spaces, dashes, underscores, and emojis so anchors reliably match ATX headings and custom heading tags.
- **Deep Visual Tree Search:** Recursively inspects FlowDocument Paragraphs, Sections, Lists, Callouts, and Tables to scroll target headings into view and align the text caret.

### 3. Tab Awareness & `OpenFilesInNewTab` Honor
- **Tab Reuse:** If the target document is already open in an existing tab, MDPlus switches directly to that tab and scrolls to the requested anchor without re-reading or duplicating tabs.
- **User Preference Respected:** Fully honors the user's `OpenFilesInNewTab` setting:
  - When `true`: Opens the target markdown file in a new tab.
  - When `false`: Replaces the current tab in-place (with dirty state protection and unsaved changes confirmation).
- **Placeholder Tab Cleanup:** Automatically replaces pristine untitled/welcome tabs when opening documentation files.

### 4. RichTextBox Hand Cursor & Click Routing
- **Interactive Cursor:** Overrides `OnQueryCursor` on `MarkdownScrollViewer` to display `Cursors.Hand` over hyperlinks in the reader.
- **Text Selection Preservation:** Intelligent preview mouse tracking triggers navigation on click while preserving 6px drag threshold for smooth text selection and editing.

### 5. Security Sandboxing & Safe URL Dispatch
- **Safe External Navigation:** Standard web URLs (`http:`, `https:`, `mailto:`, `ftp:`) are safely routed to the default system web browser.
- **Arbitrary Execution Defense:** Blocks execution of arbitrary executable formats, batch scripts, and system protocols to ensure reader safety.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
```

---

## 📦 Official Release Downloads

- [**`MDPlus-Setup.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.07/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper
- [**`MDPlus.exe`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.07/MDPlus.exe) — Standalone Portable Single-File Binary
- [**`MDPlus-win-x64.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.07/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**`MDPlus-1.07-src.zip`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.07/MDPlus-1.07-src.zip) — Source Code Archive
- [**`SHA256SUMS.txt`**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.07/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

---

## 🧪 Verification & Test Suite Results

- **Unit Test Suite:** 100% Passed (0 Failed, 0 Warnings)
- **E2E Test Suite:** 100% Passed (0 Failed, 0 Warnings)

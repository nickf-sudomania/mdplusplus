# MDPlus v1.15.4 Release Notes

> **Release Date:** November 7, 2026  
> **Tag:** v1.15.4  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.15.4 introduces a native Folder View Explorer sidebar (`Ctrl+E`), a Combined Folder View ("See All in One Go", `Ctrl+Shift+F`) that renders an entire folder of documents into a single scrollable view with clickable file index links, CLI/drag-and-drop directory support, and Windows Explorer folder context menu integration.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Folder Explorer Sidebar & Tabbed Sidebar Switcher
- **Tabbed Sidebar Switcher (`📁 FILES` | `📑 OUTLINE`):** Switch seamlessly between the Folder Explorer (`Ctrl+E`) and Document Outline (`Ctrl+T`) in the left sidebar.
- **Hierarchical Folder Tree (`FolderService` & `FolderNodeItem`):** Scans directories (ignoring hidden/build artifacts like `.git`, `node_modules`, `bin`, `obj`), sorts directories first (`readme`/`index` prioritized), and provides instant search filtering and an `MD only` filter toggle.
- **1-Click File Opening & `Alt+Up`/`Alt+Down` Navigation:** Click any file in the Folder Explorer to open it, or cycle through sibling files in the folder using `Alt+Up` and `Alt+Down`.
- **Auto-Tracking Active File Folder:** Automatically tracks the current document's parent folder and highlights the active file in the tree.

### 2. Combined Folder View ("See All in One Go")
- **Unified Continuous Document (`Ctrl+Shift+F`):** Concatenates all Markdown/text files in a folder into a single scrollable tab with a clickable file table-of-contents, per-file section headers, automatic YAML frontmatter stripping, and ATX heading demotion (`#` → `##`).
- **Open All in Tabs:** One-click button in the Folder Explorer to open all files in the current folder as individual tabs.
- **Multiple Entry Points:** Open folders via `File > Open Folder...` (`Ctrl+Shift+O`), Welcome screen button, folder drag-and-drop, CLI arguments (`MDPlus.exe <folder>`), or the Windows Explorer `"Open Folder with MDPlus"` context menu in the Inno Setup installer.

### 3. Comprehensive Test Suite
- **100% Pass Rate:** 474 total automated tests (183 unit tests and 291 E2E tests across Tiers 1–5) passing with zero regressions.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.15.4-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.15.4-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper, Multi-Format Associations & Folder Context Menu
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.15.4-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/MDPlus-1.15.4-src.zip) — Source Code Archive
- [**MDPlus.1.15.4.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/MDPlus.1.15.4.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.15.4/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

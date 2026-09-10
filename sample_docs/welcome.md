---
title: "Welcome to MDPlus"
author: "Nick"
date: "2026-09-09"
version: "1.02"
category: "Documentation"
---

# Welcome to MDPlus

> A high-performance, lightweight native Windows Markdown viewer crafted for speed, elegance, and zero Electron bloat.

---

## ⚡ Why MDPlus?

Most modern Markdown viewers (such as MarkText, Obsidian, or VS Code preview) are built on **Chromium and Electron**. That means:
- Heavy RAM usage (300 MB to 700 MB)
- Sluggish startup times (2 to 5 seconds)
- High battery consumption on laptops

**MDPlus** takes the classic **Notepad / Notepad++ philosophy** and applies it to Markdown reading:
- **DirectWrite & Direct3D hardware-accelerated rendering** via native Windows WPF.
- **Sub-150ms startup** — launches practically instantly.
- **~25 MB RAM consumption** — tiny system footprint.
- **Tabbed interface** — open multiple `.md` files without opening multiple windows.
- **Live File Watcher** — keep MDPlus open while you edit files in vim, Neovim, or Notepad. MDPlus reloads instantly without flicker.

---

## 🛠️ Essential Shortcuts

| Shortcut | Function | Description |
| :--- | :--- | :--- |
| `Ctrl + O` | Open File | Open one or more `.md` documents |
| `Ctrl + N` | New Tab | Open an additional document tab |
| `Ctrl + W` | Close Tab | Close active document tab |
| `F5` / `Ctrl + R` | Reload | Reload document from disk |
| `Ctrl + F` | Quick Find | Search text with match count & navigation |
| `Ctrl + T` | Toggle Outline | Show / hide the Table of Contents sidebar |
| `Ctrl + 1` | Rendered View | Full beautiful styled reading mode |
| `Ctrl + 2` | Split View | Side-by-side formatted and raw source |
| `Ctrl + 3` | Raw View | Fast raw monospace syntax view |
| `Ctrl + + / -` | Zoom In/Out | Scale font size smoothly |
| `Ctrl + 0` | Reset Zoom | Reset font scale to 100% |
| `F8` | Toggle Theme | Instant Dark / Light mode switch |
| `F11` | Full Screen | Distraction-free reading |
| `Ctrl + Shift + S` | Export HTML | Save self-contained HTML page |
| `Ctrl + P` | Print | Native Windows Print dialog |

---

## 🚀 GitHub Flavored Markdown Demos

### Task Lists / Checklists
- [x] Native Windows UI integration
- [x] High-speed CommonMark & GFM parser
- [x] Syntax-highlighted code blocks with 1-click copy
- [x] Live file reloading on disk changes
- [ ] Drag and drop your own Markdown files into this window!

### GitHub Callout Alerts

> [!NOTE]
> MDPlus natively parses GitHub Alert syntax (`[!NOTE]`, `[!TIP]`, `[!IMPORTANT]`, `[!WARNING]`, `[!CAUTION]`).

> [!TIP]
> You can drag-and-drop any `.md` file directly from Windows Explorer into MDPlus to view it immediately!

> [!WARNING]
> When external file changes are detected, MDPlus automatically updates your view without losing your place.

### Formatted Code Block

```csharp
// High-performance parser execution in MDPlus
var parser = new MarkdownParser();
var document = parser.Parse(fileContent);

// Instant native DirectWrite document rendering
var converter = new MarkdownToWpfConverter(directory, isDarkTheme);
FlowDocument flowDoc = converter.Convert(document);
```

---

### 📚 Explore Sample Documentation
- 📖 [GitHub Flavored Markdown Features Guide](gfm_features.md)
- 💻 [Multi-Language Syntax-Highlighted Code Samples](code_samples.md)

---

*MDPlus — The lightweight Markdown reader Windows always deserved.*

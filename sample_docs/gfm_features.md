# GitHub Flavored Markdown (GFM) Feature Showcase

This document provides a comprehensive verification suite for all GFM elements rendered by MDPlus.

---

## 1. Typography & Inline Styles

Here is normal text with **bold font**, *italicized font*, ***bold italic font***, ~~strikethrough text~~, ==highlighted text==, and `inline code elements`.

Special characters can be escaped with backslashes: \*not italic\*, \_not italic\_, \[not a link\].

Auto-links are automatically detected: <https://github.com> or email <contact@example.com>.

---

## 2. Headings & Table of Contents Hierarchy

### Level 3 Subheading
#### Level 4 Minor Heading
##### Level 5 Small Heading
###### Level 6 Caption Heading

Setext headings also render seamlessly:

Setext Level 1
==============

Setext Level 2
--------------

---

## 3. GitHub Callout Alerts

> [!NOTE]
> Useful information that users should know, even when skimming content.

> [!TIP]
> Helpful advice for doing things better or more easily.

> [!IMPORTANT]
> Key information users need to know to achieve their goal.

> [!WARNING]
> Urgent info that needs user immediate attention to avoid problems.

> [!CAUTION]
> Advises about risks or negative outcomes of certain actions.

---

## 4. Complex Tables

| ID | Project | Architecture | Language | Status | Performance |
| :--- | :--- | :---: | :---: | :---: | ---: |
| 001 | MDPlus | Native WPF | C# 12 | Active | **< 150ms** |
| 002 | Notepad++ | Native Win32 | C++ | Active | **< 100ms** |
| 003 | MarkText | Electron | JS / Web | Slow | ~3,200ms |
| 004 | Obsidian | Electron | JS / Web | Heavy | ~2,800ms |

---

## 5. Multi-level Lists & Task Checklists

- [x] Implement AST data structures
- [x] Implement zero-dependency lexer & parser
  - [x] Headings with anchor generation
  - [x] Tables with column alignment
  - [x] GitHub alert callouts
- [ ] Future enhancements
  - [ ] Custom plugin interface
  - [ ] MathJax LaTeX rendering engine
  - [ ] Diagram Mermaid charts

### Nested Ordered Lists

1. Step 1: Clone the repository
2. Step 2: Build the solution
   1. Open PowerShell
   2. Run `dotnet build`
3. Step 3: Launch `MDPlus.exe`

---

## 6. Blockquotes

> "Premature optimization is the root of all evil."
> — Donald Knuth

> Multi-line blockquote:
> Software engineering is about managing complexity and delivering value reliably.

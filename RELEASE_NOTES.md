# MDPlus Release Notes

## Version 1.14.2 (Latest)

See [docs/RELEASE_NOTES_v1.14.2.md](docs/RELEASE_NOTES_v1.14.2.md) for full details.

### Highlights
- **Equal-Width Star Columns:** All columns evenly divide the viewport width — no horizontal scrollbar. Every column is always visible.
- **Text Wrapping:** Long cell text wraps within its column instead of being truncated with ellipsis. Row heights auto-expand to fit wrapped content.
- **Dynamic Row Heights:** Removed fixed 36px row height; rows auto-size while maintaining a comfortable 32px minimum that scales with zoom.
- **Dead Code Removal:** Cleaned up 39 lines of unused `EstimateColumnWidth` and `MeasureTextWidth` methods.

## Version 1.14.1

See [docs/RELEASE_NOTES_v1.14.1.md](docs/RELEASE_NOTES_v1.14.1.md) for full details.

### Highlights
- **Content-Aware Column Width Sizing:** Dynamically measures column text using font metrics (accounting for wide CJK characters, uppercase letters, and punctuation) to size columns appropriately, eliminating text truncation and ellipsis cutting on long titles.
- **Full-Text Hover Tooltips:** Cells provide full-text tooltips with an extended 15-second display timer so users can comfortably read any ultra-long fields.
- **Smooth Horizontal Scrolling:** Explicit pixel sizing enables natural horizontal scrolling for datasets wider than the window without sacrificing 60 FPS virtualization.

## Version 1.14

See [docs/RELEASE_NOTES_v1.14.md](docs/RELEASE_NOTES_v1.14.md) for full details.

### Highlights
- **Polished Cell Formatting:** Restored generous 12px horizontal and 4px vertical cell padding, eliminating cramped layouts and keeping text elegantly spaced away from grid lines.
- **Automatic Numeric & Currency Right-Alignment:** Inspects column data to detect numbers, currency amounts (`$`, `€`, `£`, `¥`), percentages (`%`), and accounting formats `(123.45)`, automatically right-aligning both data cells and column headers.
- **Crisp Header Styling & Dividers:** Semi-bold typography, matching alignments, 12px padding, and a distinct 2px bottom border dividing headers from data rows.
- **Comfortable 36px Row Height:** Increased default row height to 36px with smooth zoom scaling (`Ctrl + Scroll`).
- **Clean In-Cell Editors:** Themed text editor with matching padding, alignment, and no dotted focus rectangles (`FocusVisualStyle = null`).

## Version 1.13

See [docs/RELEASE_NOTES_v1.13.md](docs/RELEASE_NOTES_v1.13.md) for full details.

### Highlights
- **High-Performance Virtualized Tabular Grid (`CsvDataGrid`):** Integrated a hardware-accelerated, UI-virtualized data grid for CSV and TSV files, transforming 11-second UI freezes on 1,000+ row datasets into instant sub-40ms loading and rock-solid 60 FPS pixel scrolling.
- **Dynamic Theme Palette Theming:** Seamless theme styling for table headers, cell backgrounds, alternating rows, grid lines, and selection across all 8 presets.
- **Interactive In-Place Editing & Synchronization:** Instant in-place cell editing with auto-commit and background real-time synchronization with raw CSV text in Split view.
- **Grid Find Navigation (`Ctrl+F`):** Directly navigates and highlights matching cells in `CsvDataGrid`, scrolling matched cells into viewport instantly without lag.
- **Copy & Selection:** Full support for `ApplicationCommands.Copy` and `SelectAll` (`Ctrl+A`, `Ctrl+C`) exporting grid cells in standard tab-delimited format compatible with Excel and Google Sheets.
- **Lossless Round-Trip Persistence:** Extended table properties preserve exact original column headers (including empty cells and special characters) during file save and view toggles.

## Version 1.12

See [docs/RELEASE_NOTES_v1.12.md](docs/RELEASE_NOTES_v1.12.md) for full details.

### Highlights
- **Tabular Data FlowDocument Stabilization:** Restored clean WPF FlowDocument Table architecture for CSV and TSV files with 3,000-row capping (`MaxVisualRows`) and interactive notice banners, preserving in-place editing, theme adaptive styling, and `FindBar` search accessibility.
- **Large Dataset Data-Loss Safeguard:** Added `IsVisualCapped` protection to prevent truncated FlowDocuments from ever overwriting full raw text buffers during save or tab synchronization.
- **External Reload State Cleanliness:** Fixed external file watcher reload to suppress dirty tracking and mark tabs clean, eliminating spurious dirty flags upon background file modifications.
- **RFC 4180 / TSV Delimiter Parsing:** Fixed edge-case in `CsvParser` where delimiter-only whitespace (e.g. single tab `\t`) was dropped.
- **Memory & Allocation Optimizations:** Enforced event unhooking on modal dialog closure (`ThemeManager.Instance.ThemeChanged`) and zero-allocation line counting in `StatsText`.
- **Synchronized Version 1.12:** Unified version numbers across all binaries, installers, documentation, and release checksum manifests.

## Version 1.11

See [docs/RELEASE_NOTES_v1.11.md](docs/RELEASE_NOTES_v1.11.md) for full details.

### Highlights
- **Authenticode Digital Signing:** Integrated Authenticode digital signing using SHA-256 and DigiCert timestamping into the release pipeline for `MDPlus.exe` and `MDPlus-Setup.exe`.
- **Installer Sanitation:** Eliminated standard output pollution during automated installer generation.
- **Release Verification:** Verified package digests against SHA-256 release checksum manifests.

## Version 1.10

See [docs/RELEASE_NOTES_v1.10.md](docs/RELEASE_NOTES_v1.10.md) (and [docs/RELEASE_NOTES_v1.1.md](docs/RELEASE_NOTES_v1.1.md)) for full details.

### Highlights
- **Multi-Format Lossless Saving Pipeline:** Fixed critical bug where saving JSON documents (and plain text, log, config documents) in rendered or split view failed to persist edits to disk.
- **Dedicated FlowDocument Serializers:** Added native FlowDocument serializers for JSON (`JsonToFlowDocumentConverter.Serialize`) and Plain Text / Logs / Config (`PlainTextToFlowDocumentConverter.Serialize`), routed through `DocumentFormatHelper.SerializeFlowDocument`.
- **Split View & Tab Switching Synchronization:** Guaranteed bidirectional synchronization between controls and document buffers via `SyncTabFromControls` with edit source tracking (`_lastEditSource`).
- **Version 1.10 Synchronization & Decimal Equivalence:** Fully synchronized version 1.10 / 1.10.0.0 across all build, installer, and documentation assets, with `UpdateService.CompareVersions` explicitly treating `1.1` and `1.10` as equivalent releases.

## Version 1.09

See [docs/RELEASE_NOTES_v1.09.md](docs/RELEASE_NOTES_v1.09.md) for full details.

### Highlights
- **Multi-Format Text Document Loading:** Open, view, and edit `.txt`, `.log`, `.csv`, `.tsv`, `.json`, `.ini`, `.cfg`, `.yaml`, `.yml`, and `.xml` alongside Markdown with dynamic format detection and tab strip format badges.
- **High-Legibility Formatted CSV & TSV Tables:** RFC 4180 compliant parser renders clean tables with semi-bold headers, 1px grid borders, subtle alternating row stripes, and numeric right-alignment, with instant toggling between Rendered Table (`Ctrl+1`) and Raw Monospace (`Ctrl+3`).
- **Structured JSON Pretty-Printing & Syntax Highlighting:** High-speed 2-space formatting using built-in `System.Text.Json` with token highlights matching all 8 themes, plus resilient syntax error fallback.
- **Plain Text & Log Typography:** Monospace Cascadia Code font stack for logs and clean Segoe UI for plain text, with zoom scaling (`Ctrl + Plus/Minus/0`) and in-page find (`Ctrl+F`).
- **Inno Setup File Associations:** Installer tasks and registry configuration for `.txt`, `.csv`, `.tsv`, and `.json` files, including right-click "Open with MDPlus" context menu verbs.

## Version 1.08

See [docs/RELEASE_NOTES_v1.08.md](docs/RELEASE_NOTES_v1.08.md) for full details.

### Highlights
- **Hardened Mouse Capture & Click Routing:** Explicit mouse capture and cached hyperlink routing on `PreviewMouseLeftButtonDown` in `MarkdownScrollViewer`, preventing WPF's text editor drag-selection engine from intercepting link clicks.
- **Bounded Hit-Testing & Empty Margin Protection:** `GetPositionFromPoint(point, snapToText: true)` combined with character bounding box validation (`pointer.GetCharacterRect`) and `charRect.IsEmpty` guarding ensures empty whitespace to the right of lines never triggers hyperlinks, while nested inline code spans (`[`code()`](api.md)`) hit-test with pinpoint precision.
- **Asynchronous Document Loading:** Dispatches tab replacement via `Dispatcher.BeginInvoke(...)` so mouse events finish cleanly before the document visual tree is replaced.
- **Missing File Feedback:** Non-existent markdown documents trigger a clear status bar notification (`File not found: <filename>`) rather than failing silently, while executables (`.exe`, `.pdf`) are safely blocked.
- **Packaging Sanitation:** Fixed `build.ps1` to eliminate recursive subdirectory nesting during release publishing.

## Version 1.07

See [docs/RELEASE_NOTES_v1.07.md](docs/RELEASE_NOTES_v1.07.md) for full details.

### Highlights
- **In-Reader Markdown Link Navigation:** Seamlessly jump to other local `.md`, `.markdown`, `.mdown`, and `.mkd` files directly within the reader. Relative paths resolve against the active document's directory, with fallback to working directory and sample docs.
- **Heading Anchor Scrolling & Slug Normalization:** Automatic navigation and smooth scrolling to heading anchors (`#heading-anchor`), supporting robust slug normalization across spaces, punctuation, underscores, and emojis.
- **Multi-Tab Awareness & Tab Reuse:** Jumping to an already open document switches directly to that tab and scrolls to the requested anchor.
- **User Tab Preference Honored:** Fully respects the `OpenFilesInNewTab` setting—reusing the current tab in-place (with unsaved changes confirmation) when disabled, or opening in a new tab when enabled.
- **Interactive Hand Cursor & Click Routing:** Dynamic `Cursors.Hand` feedback over reader links and preview click detection that preserves text selection with a 6px drag threshold.
- **Secure Web Dispatch:** External URLs (`http:`, `https:`, `mailto:`, `ftp:`) are safely routed to the default web browser, while arbitrary executable files remain blocked.

## Version 1.06

See [docs/RELEASE_NOTES_v1.06.md](docs/RELEASE_NOTES_v1.06.md) for full details.

### Highlights
- **Typographic Baseline Alignment:** Mathematical formulas ($A = k \times B$, $W^2 = W^1 + d$), inline code spans (`[BaseDomainGenerator]`), and keycaps (<kbd>Ctrl</kbd>) now align on the font baseline with surrounding plain text and parentheses.
- **Vector LaTeX Math Plugin:** Crisp native inline (`$...$`) and display (`$$...$$`) LaTeX math formulas rendered with pure WPF vector primitives (fractions, radicals, Greek letters, integrals, matrices). Zero Chromium or MathJax dependencies.
- **Native HTML Rendering Plugin:** Raw HTML tags (`<kbd>`, `<sub>`, `<sup>`, `<u>`, `<mark>`, `<span>` with CSS style parsing) and interactive collapsible `<details>` disclosure widgets rendered without WebBrowser or WebView2.
- **Modular Plugins Menu:** Easily toggle LaTeX Math Rendering and HTML Rendering on/off from the Main Menu or Hamburger Menu with instant document re-rendering.
- **Sub-Millisecond Zero-Overhead Performance:** Empirically verified negligible performance impact (< 3 ms cold startup difference, sub-millisecond document parsing, 0 allocations on plain markdown).
- **Two-Way FlowDocument Round-Trip:** Extended `MarkdownSerializer` with full fidelity support for LaTeX formulas and raw HTML markup tags.

## Version 1.03

See [docs/RELEASE_NOTES_v1.03.md](docs/RELEASE_NOTES_v1.03.md) for full details.

### Highlights
- **Vector LaTeX Math Plugin:** Crisp native inline (`$...$`) and display (`$$...$$`) LaTeX math formulas rendered with pure WPF vector primitives (fractions, radicals, Greek letters, integrals, matrices). Zero Chromium or MathJax dependencies.
- **Native HTML Rendering Plugin:** Raw HTML tags (`<kbd>`, `<sub>`, `<sup>`, `<u>`, `<mark>`, `<span>` with CSS style parsing) and interactive collapsible `<details>` disclosure widgets rendered without WebBrowser or WebView2.
- **Modular Plugins Menu:** Easily toggle LaTeX Math Rendering and HTML Rendering on/off from the Main Menu or Hamburger Menu with instant document re-rendering.
- **Sub-Millisecond Zero-Overhead Performance:** Empirically verified negligible performance impact (< 3 ms cold startup difference, sub-millisecond document parsing, 0 allocations on plain markdown).
- **Two-Way FlowDocument Round-Trip:** Extended `MarkdownSerializer` with full fidelity support for LaTeX formulas and raw HTML markup tags.
- **HTML Exporter Math & Tag Styling:** Standalone HTML export with KaTeX/MathJax-compatible math blocks and styled HTML tags.

## Version 1.02

### Highlights
- **Modern Fluent Dynamic Pill Scrollbars:** Sleek Windows 11 rounded pill scrollbars across editor, previewer, and Table of Contents with dynamic hover/expand transitions and subtle idle state.
- **Three New Designer Light Themes:** One Light, Solarized Light, and Quiet Light added to the theme suite.
- **Light/Dark Menu Subgrouping:** Theme menu is now organized into convenient Dark Themes and Light Themes submenus.
- **Table of Contents Readability:** High-contrast WCAG AA compliant text and hierarchical styling for light themes (up to 14.2:1 contrast ratio).
- **Dynamic Windows DWM Title Bar Theming:** Native Windows Desktop Window Manager integration dynamically styles caption and title bar colors to match active themes.
- **Session Restore & Multi-Tab Enhancements:** Tab navigation (Ctrl+Tab, Ctrl+Shift+Tab, Ctrl+W), single-click close buttons, dirty state protection, and graceful empty shell.
- **Hardened GitHub Auto-Updater:** 1-click update verification, automated 24h debounced startup checks, and reliable UAC elevation (`runas` verb) with clean session preservation and shutdown.

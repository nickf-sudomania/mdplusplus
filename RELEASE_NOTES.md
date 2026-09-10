# MDPlus Release Notes

## Version 1.07 (Latest)

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

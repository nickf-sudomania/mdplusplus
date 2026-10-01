# Project: MDPlus Mermaid Flowchart & Inline Math Typography

## Architecture
- **Markdown Pipeline**:
  - `MarkdownParser.cs` fences ```` ```mermaid ```` into `CodeBlock { Language = "mermaid" }`.
  - `MarkdownToWpfConverter.cs` converts blocks/inlines to FlowDocument elements.
    - Intercepts `code.Language == "mermaid"` -> calls `MermaidFlowchartParser.TryParse` and `MermaidFlowchartRenderer.Render`.
    - Converts `MathInline` -> calls `LatexMathRenderer.RenderMath`, wraps in `InlineUIContainer` with calibrated `BaselineAlignment` and margins.
  - `MarkdownSerializer.cs` preserves two-way round-trip via `BlockUIContainer.Tag = new CodeBlockTag { Language = "mermaid", Code = rawCode }`.
- **Mermaid Flowchart Engine (`src/Core/Mermaid/`)**:
  - `MermaidModel.cs`: Pure AST data structures (`MermaidFlowchartGraph`, `MermaidNode`, `MermaidEdge`, `MermaidSubgraph`, enums).
  - `MermaidFlowchartParser.cs`: Fast single-pass scanner & recursive descent parser with error fallback.
  - `MermaidLayoutEngine.cs`: Sugiyama layered DAG layout (cycle reversal, longest-path ranking, dummy nodes, barycentric crossing reduction, coordinate assignment).
  - `MermaidFlowchartRenderer.cs`: Native WPF vector rendering (`Canvas`, `Path`, `Border`, `TextBlock`) with dynamic `ThemePalette` binding.
  - `MermaidScrollViewer.cs`: Custom `ScrollViewer` that bubbles vertical wheel scrolling to parent viewer.
- **Math Typography (`src/Core/LatexMathRenderer.cs` & `MarkdownToWpfConverter.cs`)**:
  - `InlineUIContainer.BaselineAlignment = BaselineAlignment.Center`.
  - `LatexMathRenderer`: Calibrated zero-sum offset margin `new Thickness(1, vOffset, 1, -vOffset)` where `vOffset = Math.Round(effectiveFontSize * (2.5 / 14.5), 1)`.
  - Ambient font size propagation for headings.

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Mermaid Block Interception & Fallback | Intercept `code.Language == "mermaid"`; fallback to standard code block with badge & copy on error/non-flowchart; lossless serialization tag | M2 / M3 | ORIGINAL_REQUEST R1, R2 |
| 2 | Mermaid AST Data Model | Graph, Node, Edge, Subgraph, Orientation, Shape, StrokeStyle, ArrowHead enums | M2 | ORIGINAL_REQUEST R1 |
| 3 | Mermaid Orientation Parsing | `graph TD`, `graph TB`, `graph LR`, `graph RL`, `graph BT`, `flowchart` equivalents | M2 | ORIGINAL_REQUEST R1 |
| 4 | Mermaid Node Shape Parsing | Rectangle `[]`, Rounded `()`, Circle `(())`, Diamond `{}`, Stadium `([])` with label escaping | M2 | ORIGINAL_REQUEST R1 |
| 5 | Mermaid Connection & Arrow Syntax | `-->`, `-.->`, `==>`, `---`, `-.-`, `===`, edge labels via `-->\|label\|` and `-- label -->` | M2 | ORIGINAL_REQUEST R1 |
| 6 | Native Layered Graph Layout Engine | Sugiyama 5-phase DAG layout (cycle breaking, ranking, dummy nodes, crossing minimization, coordinate assignment) | M3 | ORIGINAL_REQUEST R2 |
| 7 | Horizontal & Vertical Coordinate Mapping | Mapping abstract axes $(u, v)$ to $(X, Y)$ for `TD`/`TB`/`BT`/`LR`/`RL`, text metric bounding boxes | M3 | ORIGINAL_REQUEST R2 |
| 8 | Native WPF Vector Element Generation | `Canvas`, `Border`, `TextBlock`, `PathGeometry`, smooth cubic Bezier connectors, rotated arrowheads, label badges | M3 | ORIGINAL_REQUEST R2 |
| 9 | Dynamic ThemePalette Integration | Nodes, borders, text, and connector strokes bound to active `ThemePalette`; dynamic updates on theme cycle; WCAG AA/AAA contrast | M3 | ORIGINAL_REQUEST R2 |
| 10 | 60 FPS Scrolling & UI Performance | Frozen geometry/brushes/pens, layout isolation, mouse wheel bubbling, sub-20ms layout for 20 nodes | M3 | ORIGINAL_REQUEST R2, R4 |
| 11 | Inline Math Baseline & Margin Calibration | Restore `BaselineAlignment.Center` in `MarkdownToWpfConverter`, apply `(1, vOffset, 1, -vOffset)` margin in `LatexMathRenderer` | M1 | ORIGINAL_REQUEST R3 |
| 12 | Multi-Context Typographic Robustness | Alignment consistency across `Paragraph` (LineHeight=24), `ListItem` (LineHeight=NaN), Headings; zero clipping | M1 | ORIGINAL_REQUEST R3 |
| 13 | Query 2 Financial Schema Validation | Verify `$ROIC > 18\%$`, `$> 18\%$`, `$\ge +1.5\%$`, `$\le 30\times$` optical alignment and character bounds | M1 | ORIGINAL_REQUEST R3 |
| 14 | Automated Test Suite (Mermaid & Math) | Comprehensive unit tests for parser, node extraction, layout calculations, math baseline alignment bounds, 0 regressions | M4 / M-E2E | ORIGINAL_REQUEST AC |
| 15 | 20-Node Flowchart Performance Benchmark | Automated benchmark confirming 20-node flowchart parses and renders in < 25ms (< 20ms layout overhead) | M4 / M-E2E | ORIGINAL_REQUEST R4, AC |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Inline Math Typographic Baseline & Optical Alignment | Fix `LatexMathRenderer.cs` margin, `MarkdownToWpfConverter.cs` baseline alignment, update existing test, add layout bounds test | none | DONE |
| M2 | Mermaid AST, Grammar & Parser | Implement AST classes, tokenizer, `MermaidFlowchartParser.cs`, syntax fallback, unit tests | none | DONE |
| M3 | WPF Vector Layout Engine & Theme-Aware Renderer | Implement `MermaidLayoutEngine.cs`, `MermaidFlowchartRenderer.cs`, `MermaidScrollViewer.cs`, wire into `MarkdownToWpfConverter.cs`, theme palette integration | M2 | DONE |
| M-E2E | E2E Testing Suite (Tiers 1-4) | Independent requirement-driven test infra, test cases Tiers 1-4 across all 15 features, publish `TEST_READY.md` | none | DONE |
| M4 | Final Milestone: E2E Test Pass, Adversarial Hardening (Tier 5) & Benchmarks | Pass 100% E2E tests (Tiers 1-4), Tier 5 adversarial hardening, 20-node benchmark < 25ms, verify `.\build.ps1 -Action Test` | M1, M3, M-E2E | DONE |

## Interface Contracts
### MermaidFlowchartParser ↔ MarkdownToWpfConverter
```csharp
namespace MDPlus.Core.Mermaid
{
    public static class MermaidFlowchartParser
    {
        public static bool TryParse(string text, out MermaidFlowchartGraph graph, out string? errorMessage);
    }
}
```

### MermaidFlowchartRenderer ↔ MarkdownToWpfConverter
```csharp
namespace MDPlus.Core.Mermaid
{
    public static class MermaidFlowchartRenderer
    {
        public static BlockUIContainer Render(MermaidFlowchartGraph graph, ThemePalette palette, string rawCode);
    }
}
```

### MermaidLayoutEngine ↔ MermaidFlowchartRenderer
```csharp
namespace MDPlus.Core.Mermaid
{
    public sealed class MermaidLayoutEngine
    {
        public static MermaidLayoutResult Layout(MermaidFlowchartGraph graph, double maxAvailableWidth = double.PositiveInfinity);
    }
}
```

### LatexMathRenderer ↔ MarkdownToWpfConverter
```csharp
namespace MDPlus.Core
{
    public static class LatexMathRenderer
    {
        public static UIElement RenderMath(string latex, ThemePalette palette, double fontSize = 14.5, bool isDisplay = false);
    }
}
```

## Code Layout
- `src/Core/LatexMathRenderer.cs`: Math rendering and container margins
- `src/Core/MarkdownToWpfConverter.cs`: FlowDocument block and inline conversion
- `src/Core/Mermaid/MermaidModel.cs`: Flowchart AST models and enums
- `src/Core/Mermaid/MermaidFlowchartParser.cs`: Syntax tokenizer and recursive descent parser
- `src/Core/Mermaid/MermaidLayoutEngine.cs`: Sugiyama DAG layout and coordinate calculation
- `src/Core/Mermaid/MermaidFlowchartRenderer.cs`: WPF vector element builder and theme styling
- `src/Core/Mermaid/MermaidScrollViewer.cs`: Wheel-bubbling scroll container
- `tests/MermaidTests.cs`: Unit & layout tests for Mermaid parsing and rendering
- `tests/TestRunner.cs`: Existing and updated test suite

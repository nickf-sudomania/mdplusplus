# E2E Test Infra: MDPlus Mermaid Flowchart & Inline Math Typography

## Test Philosophy
- Opaque-box, requirement-driven. No dependency on implementation design.
- Pure .NET 8 test execution via `dotnet test` / `TestRunner.cs`.
- Methodology: Category-Partition + Boundary Value Analysis (BVA) + Pairwise Combinatorial + Real-World Workload Testing.

## Feature Inventory
| # | Feature | Source (requirement) | Tier 1 (Min 5) | Tier 2 (Min 5) | Tier 3 (Pairwise) | Tier 4 (Workload) |
|---|---------|---------------------|:--------------:|:--------------:|:-----------------:|:-----------------:|
| 1 | Mermaid Block Interception & Fallback | ORIGINAL_REQUEST R1, AC | 5 | 5 | ✓ | ✓ |
| 2 | Mermaid AST Data Model | ORIGINAL_REQUEST R1 | 5 | 5 | ✓ | ✓ |
| 3 | Mermaid Orientation Parsing | ORIGINAL_REQUEST R1 | 5 | 5 | ✓ | ✓ |
| 4 | Mermaid Node Shape Parsing | ORIGINAL_REQUEST R1 | 5 | 5 | ✓ | ✓ |
| 5 | Mermaid Connection & Arrow Syntax | ORIGINAL_REQUEST R1 | 5 | 5 | ✓ | ✓ |
| 6 | Native Layered Graph Layout Engine | ORIGINAL_REQUEST R2 | 5 | 5 | ✓ | ✓ |
| 7 | Horizontal & Vertical Coordinate Mapping | ORIGINAL_REQUEST R2 | 5 | 5 | ✓ | ✓ |
| 8 | Native WPF Vector Element Generation | ORIGINAL_REQUEST R2 | 5 | 5 | ✓ | ✓ |
| 9 | Dynamic ThemePalette Integration | ORIGINAL_REQUEST R2 | 5 | 5 | ✓ | ✓ |
| 10 | 60 FPS Scrolling & UI Performance | ORIGINAL_REQUEST R2, R4 | 5 | 5 | ✓ | ✓ |
| 11 | Inline Math Baseline & Margin Calibration | ORIGINAL_REQUEST R3 | 5 | 5 | ✓ | ✓ |
| 12 | Multi-Context Typographic Robustness | ORIGINAL_REQUEST R3 | 5 | 5 | ✓ | ✓ |
| 13 | Query 2 Financial Schema Validation | ORIGINAL_REQUEST R3 | 5 | 5 | ✓ | ✓ |
| 14 | Automated Test Suite (Mermaid & Math) | ORIGINAL_REQUEST AC | 5 | 5 | ✓ | ✓ |
| 15 | 20-Node Flowchart Performance Benchmark | ORIGINAL_REQUEST R4, AC | 5 | 5 | ✓ | ✓ |

## Test Architecture
- **Runner**: `dotnet test tests\MDPlus.Tests.csproj` or standalone test executable `tests\TestRunner.cs`.
- **Pass/Fail Semantics**: All assertions must pass (exit code 0). Zero unhandled exceptions.
- **Directory Layout**:
  - `tests/`: Existing test runner and test classes.
  - `tests/Mermaid/`: Mermaid syntax, shapes, connections, layout, and renderer tests.
  - `tests/Typography/`: Inline math baseline alignment, character bounds, line height invariance tests.

## Coverage Thresholds
- Tier 1: Feature Coverage (>= 75 test cases, >= 5 per feature)
- Tier 2: Boundary & Corner Cases (>= 75 test cases, >= 5 per feature)
- Tier 3: Cross-Feature Combinations (>= 15 pairwise interaction tests)
- Tier 4: Real-World Application Scenarios (>= 8 complex integration scenarios)
- Total Target: >= 173 test cases

## Real-World Application Scenarios (Tier 4)
| # | Scenario | Features Exercised | Complexity |
|---|----------|--------------------|------------|
| 1 | Financial Screener Schema Query 2 Document | F11, F12, F13 | High |
| 2 | Software Architecture Multi-Tier Flowchart (20+ nodes) | F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F15 | High |
| 3 | Decision Tree with Diamond Nodes & Labeled Branches | F3, F4, F5, F6, F8, F9 | Medium |
| 4 | State Machine with Cyclic Feedback Loops & Dotted Transitions | F4, F5, F6, F7, F8 | High |
| 5 | Mixed Document with Headings, Inline Math, Lists, and Flowcharts | F1, F8, F9, F11, F12, F13 | High |
| 6 | Theme Switching Live Cycle with Rendered Flowchart & Math | F8, F9, F11 | Medium |
| 7 | Degraded Malformed Code Block with Mixed Syntax | F1, F5, F8 | Medium |
| 8 | Large 25-Node Benchmark Graph Performance Under 25ms | F6, F10, F15 | High |

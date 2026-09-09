# Code Syntax Highlighting Samples

MDPlus features a built-in, lightning-fast syntax tokenizing engine supporting the most popular programming languages without requiring any heavy third-party JavaScript libraries (like Prism or Highlight.js).

---

## 1. C# (.NET)

```csharp
using System;
using System.IO;
using System.Threading.Tasks;

namespace MDPlus.Sample
{
    public class MarkdownService
    {
        private readonly string _cachePath;

        public MarkdownService(string cachePath)
        {
            _cachePath = cachePath ?? throw new ArgumentNullException(nameof(cachePath));
        }

        public async Task<int> ProcessFileAsync(string path)
        {
            if (!File.Exists(path)) return 0;
            string content = await File.ReadAllTextAsync(path);
            return content.Length;
        }
    }
}
```

---

## 2. Python

```python
import sys
import os
from typing import List, Optional

class DocumentIndex:
    def __init__(self, root_dir: str):
        self.root_dir = root_dir
        self.files: List[str] = []

    def scan_markdown(self) -> int:
        for root, _, filenames in os.walk(self.root_dir):
            for name in filenames:
                if name.endswith(".md"):
                    self.files.append(os.path.join(root, name))
        return len(self.files)

if __name__ == "__main__":
    indexer = DocumentIndex(".")
    print(f"Found {indexer.scan_markdown()} documents.")
```

---

## 3. TypeScript / JavaScript

```typescript
interface DocumentMetadata {
    title: string;
    wordCount: number;
    tags: string[];
    isPublished: boolean;
}

export function calculateReadingTime(wordCount: number): string {
    const wordsPerMinute = 200;
    const minutes = Math.ceil(wordCount / wordsPerMinute);
    return `${minutes} min read`;
}
```

---

## 4. SQL

```sql
SELECT 
    d.DocumentId,
    d.Title,
    COUNT(h.HeadingId) AS TotalHeadings,
    d.CreatedAt
FROM Documents d
LEFT JOIN Headings h ON d.DocumentId = h.DocumentId
WHERE d.IsArchived = 0
GROUP BY d.DocumentId, d.Title, d.CreatedAt
ORDER BY d.CreatedAt DESC
LIMIT 50;
```

---

## 5. JSON Configuration

```json
{
  "appName": "MDPlus",
  "version": "1.0.0",
  "nativeWpf": true,
  "features": [
    "syntax_highlighting",
    "table_of_contents",
    "live_reload",
    "dark_light_theme"
  ],
  "performance": {
    "startupMs": 120,
    "memoryMb": 24.5
  }
}
```

---

## 6. Shell / PowerShell

```powershell
# Build and publish MDPlus as a single-file executable
Write-Host "Compiling MDPlus Release Build..." -ForegroundColor Cyan

dotnet publish src\MDPlus.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o bin\publish

Write-Host "Build complete: bin\publish\MDPlus.exe" -ForegroundColor Green
```

# MDPlus v1.11 Release Notes

> **Release Date:** September 12, 2026  
> **Tag:** v1.11  
> **Target Framework:** .NET 8.0 Windows Desktop (WPF)  

MDPlus v1.11 introduces Authenticode digital signing to the release and build pipeline, ensuring that all published executables and installers are verified and tamper-proof.

---

## 🚀 Key Feature Highlights & Fixes

### 1. Authenticode Digital Signing
- **Digital Signing Integration:** Integrated Microsoft `signtool.exe` into the build and publishing pipeline (`build.ps1`).
- **Cryptographic Signatures:** MDPlus binaries (`MDPlus.exe`) and installer packages (`MDPlus-Setup.exe`) are digitally signed with Authenticode signatures using SHA-256 digest algorithms and trusted DigiCert timestamping (`http://timestamp.digicert.com`).
- **Local Development Signing:** Bundled local code-signing certificate (`MDPlus_CodeSign.pfx`) for deterministic offline verification and local test builds.

### 2. Installer & Packaging Fixes
- Fixed installer output pollution during automated build passes.
- Ensured setup installer package is signed automatically after Inno Setup compilation.
- Validated all package artifacts against SHA-256 manifest digests.

---

## 🛡️ Cryptographic Verification

All distributed assets are authenticated using cryptographic SHA-256 digests. You can verify your local downloads with PowerShell or Command Prompt:

```powershell
# PowerShell verification:
Get-FileHash MDPlus-Setup.exe -Algorithm SHA256
Get-FileHash MDPlus.exe -Algorithm SHA256
Get-FileHash MDPlus-1.11-src.zip -Algorithm SHA256
```

```cmd
:: Command Prompt verification:
certutil -hashfile MDPlus-Setup.exe SHA256
certutil -hashfile MDPlus.exe SHA256
certutil -hashfile MDPlus-1.11-src.zip SHA256
```

### Official Assets
- [**MDPlus-Setup.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/MDPlus-Setup.exe) — Windows Setup Installer with .NET 8 Bootstrapper & Multi-Format Associations
- [**MDPlus.exe**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/MDPlus.exe) — Standalone Portable Single-File Binary
- [**MDPlus-win-x64.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/MDPlus-win-x64.zip) — Portable Zip Distribution
- [**MDPlus-1.11-src.zip**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/MDPlus-1.11-src.zip) — Source Code Archive
- [**MDPlus.1.11.checksums.sha256**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/MDPlus.1.11.checksums.sha256) — Notepad++ Compatible Checksum Manifest
- [**SHA256SUMS.txt**](https://github.com/nickf-sudomania/mdplusplus/releases/download/v1.11/SHA256SUMS.txt) — Master Cryptographic Checksum Manifest

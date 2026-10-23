# Distributing Pandora's Box Optimizer

The finished app is **`PandorasBoxOptimizer.exe`** (space-free name, for clean URLs).

- Size: ~59 MB (self-contained — users need **no** .NET install)
- SHA-256 (v1.0.0): `1F442FF208A42AA3C00DAA994AF18699699BF0FB506BD89B857574821CAF6CEC`

---

## Step 1 — Host it on GitHub Releases (recommended)

GitHub Releases is free, versioned, on a domain people trust, and gives a **permanent "latest" URL** so your one-liner never changes between updates.

### Option A — GitHub CLI (fastest)

First make sure `gh` is logged in as **nullxiv** (it may be set to another account):

```powershell
gh auth login          # choose GitHub.com, and sign in as nullxiv
gh auth switch --user nullxiv   # if nullxiv is already added but not active
```

Then, from the folder that has `PandorasBoxOptimizer.exe`:

```powershell
# 1. Create the repo (once). Must be PUBLIC for the no-login download command to work.
gh repo create pandoras-box-optimizer --public --description "Pandora's Box FPS Optimizer for Modern Warfare IV"

# 2. Publish a release and attach the exe (repeat per version, bump the tag)
gh release create v1.0.0 "PandorasBoxOptimizer.exe" `
  --repo nullxiv/pandoras-box-optimizer `
  --title "Pandora's Box Optimizer v1.0.0" `
  --notes "One-click MW4 FPS profiles + real system scan. SHA-256: 1F442FF208A42AA3C00DAA994AF18699699BF0FB506BD89B857574821CAF6CEC"
```

### Option B — GitHub website

1. Create a new **public** repo (e.g. `pandoras-box-optimizer`).
2. Go to **Releases → Draft a new release**.
3. Tag `v1.0.0`, give it a title, and **drag `PandorasBoxOptimizer.exe` into the "Attach binaries" box**.
4. **Publish release.**

Either way your download URL becomes (note `/latest/` — always points at the newest release):

```
https://github.com/nullxiv/pandoras-box-optimizer/releases/latest/download/PandorasBoxOptimizer.exe
```

---

## Step 2 — The command people run

Downloads to temp, launches, no install. **Two forms — the context matters:**

**PowerShell / Windows Terminal** (paste directly — NO `\"` escaping):

```powershell
$o="$env:TEMP\PandorasBoxOptimizer.exe"; irm 'https://github.com/nullxiv/pandoras-box-optimizer/releases/latest/download/PandorasBoxOptimizer.exe' -OutFile $o; Start-Process $o
```

**Run box (Win+R) or Command Prompt** (needs the `\"` escaping):

```bat
powershell -NoProfile -Command "$o=\"$env:TEMP\PandorasBoxOptimizer.exe\"; irm 'https://github.com/nullxiv/pandoras-box-optimizer/releases/latest/download/PandorasBoxOptimizer.exe' -OutFile $o; Start-Process $o"
```

> ⚠️ Don't paste the Run-box form into an open PowerShell window — the `\"` escaping isn't
> valid there and it errors with `'=\' is not recognized`. Use the first form in PowerShell.

---

## Publishing an update later

Rebuild, then publish a new tag — the `/latest/` URL auto-updates, so the command above keeps working unchanged:

```powershell
dotnet publish -c Release    # inside the PandorasBoxOptimizer project folder
copy "PandorasBoxOptimizer\bin\Release\net10.0-windows\win-x64\publish\PandorasBox Optimizer.exe" "PandorasBoxOptimizer.exe"
gh release create v1.0.1 "PandorasBoxOptimizer.exe" --repo nullxiv/pandoras-box-optimizer --title "v1.0.1" --notes "..."
```

---

## Trust & safety notes (be upfront with users)

- **SmartScreen:** unsigned, so Windows shows "Windows protected your PC" → *More info → Run anyway*. A code-signing certificate removes this.
- **`irm ... ; Start-Process` is the same pattern shady "config" accounts use.** Hosting on a visible public GitHub repo (with source + README) is what makes it trustworthy — not the command itself. Keep the repo public and the code visible.
- **Checksum:** anyone can verify a download matches before running:
  ```powershell
  (Get-FileHash "$env:TEMP\PandorasBoxOptimizer.exe" -Algorithm SHA256).Hash
  ```
  It should equal the SHA-256 published in the release notes.
- Consider committing the **source** to the same repo so people can read exactly what it does.

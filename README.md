# Pandora's Box Optimizer

One-click FPS profiles for **Call of Duty: Modern Warfare IV**, with a real system scan.

![screenshot](screenshot.png)

## Download & run

**Easiest — no commands:** download **`PandorasBoxOptimizer.exe`** from the
[latest release](https://github.com/nullxiv/pandoras-box-optimizer/releases/latest) and **double-click it**.
It's self-contained — nothing to install.

Windows SmartScreen will say **"Windows protected your PC"** (it's unsigned) → **More info → Run anyway**.

<details>
<summary><b>Prefer a one-line terminal command?</b></summary>

**PowerShell / Windows Terminal** — paste as-is (no `\"`):

```powershell
$o="$env:TEMP\PandorasBoxOptimizer.exe"; irm 'https://github.com/nullxiv/pandoras-box-optimizer/releases/latest/download/PandorasBoxOptimizer.exe' -OutFile $o; Start-Process $o
```

**Command Prompt / Run box (Win+R)** — this form has the `\"` escaping:

```bat
powershell -NoProfile -Command "$o=\"$env:TEMP\PandorasBoxOptimizer.exe\"; irm 'https://github.com/nullxiv/pandoras-box-optimizer/releases/latest/download/PandorasBoxOptimizer.exe' -OutFile $o; Start-Process $o"
```

Don't mix them — the Run-box form errors (`'=\' is not recognized`) if pasted into PowerShell.
</details>

## What it does

- **Scans your PC** (GPU / CPU / RAM / display) and recommends the profile that fits it.
- **Max FPS** — every setting floored for the highest possible frame rate.
- **Balanced** — sharp image (DLSS Balanced, bullet impacts on), still well above most refresh rates.
- Patches **only** the performance settings — your resolution, monitor, audio and keybinds are left untouched.
- **Backs up** your current config before every change, with a one-click **Restore**.

## Safety

- Edits only `%LOCALAPPDATA%\Activision\Call of Duty\Players\s.1.0.cod26.txt*`, after backing it up.
- Verify a download: its SHA-256 should match the value in each release's notes.
- Build it yourself (needs the **.NET 10 SDK**):
  ```powershell
  cd PandorasBoxOptimizer
  dotnet publish -c Release
  ```

## Note

Launch Modern Warfare IV once (reach the main menu, then quit) so the game creates its settings
files — then run this tool.

# Code Formatting & Style Guide

This repository enforces **automatic, deterministic formatting** across all languages.
The goal is simple:

- eliminate whitespace-only diffs
- avoid formatter arguments in PRs
- ensure the same output on every machine
- make formatting a tooling concern, not a social one

---

## High-level principles

- Formatting is **owned by tools**, not individuals
- Formatting rules are **versioned in the repository**
- Editors are free to vary; output is not
- Only **staged files** are formatted during normal work
- A single baseline reformat is applied once, then enforced forever

---

## Enforcement layers (in order)

Formatting is enforced through several complementary layers:

1. **`.gitattributes`**
   - Controls how Git stores text files
   - Normalises line endings to LF

2. **`.editorconfig`**
   - Controls whitespace, indentation, and basic conventions
   - Respected by Visual Studio, VS Code, Rider, and formatters

3. **Language-specific formatters (CSharpier)**
   - Own structural layout and wrapping
   - Opinionated and deterministic

4. **`pre-commit` hooks**
   - Automatically format staged files before commit
   - Prevents inconsistent code from entering the repository

---

## Line endings and whitespace (all files)

Handled by `.gitattributes` and `.editorconfig`.

### Rules

- Line endings: **LF**
- Charset: **UTF-8**
- Trailing whitespace: **removed**
- Final newline: **required**
- Tabs: **never used** (spaces only)

These rules apply to _all text files_, regardless of language.

---

## C# (.cs)

### Tool: **CSharpier**

C# formatting is handled exclusively by **CSharpier**, an opinionated, configuration-free formatter.

CSharpier:

- owns all C# layout decisions
- ignores most C# formatting options in `.editorconfig` by design
- produces identical output on all machines
- removes formatting as a discussion topic

If we ever need to exclude something explicitly, we can add `.csharpierignore`.

### Key expectations

- Method signatures are wrapped automatically when appropriate:
- Braces are placed consistently
- Parameters, arguments, and expressions are wrapped deterministically
- Manual alignment will be overwritten

### What developers should do

- Write clear, idiomatic C#
- Do not manually align parameters or arguments
- Let CSharpier handle layout

### Related tools

- `.editorconfig` still applies for:
  - line endings
  - indentation size
  - naming rules
  - analyzer severity
- `dotnet format` may still be used for:
  - project files (`.csproj`, `.sln`)
  - analyzer fixes

---

### Purpose

- Ensure consistent, readable Markdown
- Avoid reflow churn
- Preserve semantic whitespace where required

### Notable rules

- Line length is not strictly enforced
- Inline HTML is allowed
- Trailing whitespace is preserved where meaningful

### Editor support

VS Code users are encouraged (but not required) to install:

```text
DavidAnson.vscode-markdownlint
```

Pre-commit hooks enforce the same rules regardless of editor.

---

## JavaScript / TypeScript / Web assets

### Tool: **Prettier**

Applies to:

- `.js`, `.mjs`, `.ts`, `.tsx`
- `.css`, `.scss`
- `.html`
- `.json`
- `.yml`, `.yaml`
- `.md`

### Behaviour

- Opinionated, minimal configuration
- Consistent wrapping and indentation
- Trailing commas where supported
- Single quotes for strings

Generated or vendor files are excluded via `.prettierignore`.

---

## Python (.py)

- Indentation: **2 spaces**
- Whitespace and line endings enforced via `.editorconfig`
- Structural formatting may be added later if needed

---

## Shell / PowerShell

Applies to:

- `.sh`
- `.ps1`

Rules:

- Indentation and whitespace via `.editorconfig`
- Line endings normalised
- No structural formatter enforced yet

---

## SQL (.sql)

- Whitespace and line endings enforced
- No SQL formatter enforced yet
- Formatting is intentionally left minimal to avoid semantic risk

---

## Infrastructure & config files

Applies to:

- Terraform (`.tf`)
- YAML
- JSON
- TOML

Handled primarily by:

- `.editorconfig`
- Prettier (where applicable)

---

## Binary and generated files

The following are **not formatted**:

- Images (`.png`, `.ico`, `.svg`)
- Fonts (`.woff`, `.woff2`)
- Source maps (`.map`)
- Vendor libraries
- Generated build output

These are excluded from formatters to avoid noise and corruption.

---

## Pre-commit behaviour

Formatting is applied automatically:

- **on commit**
- **only to staged files**

Developers do not need to:

- manually run formatters
- configure their editor
- remember formatting commands

To install hooks (one-time):

```bash
python -m pip install pre-commit
pre-commit install
dotnet tool restore
```

---

## One-time baseline formatting

A single commit applies formatting to the entire repository.
After this point:

- formatting diffs should be minimal
- PRs should not contain whitespace-only noise

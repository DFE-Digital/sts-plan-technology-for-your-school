#!/usr/bin/env python3
from __future__ import annotations

import sys
from collections import Counter
from pathlib import Path


# Extension -> "type" mapping.
# This is not only "languages"; it's "what is this file?"
EXT_TO_TYPE: dict[str, str] = {
    # C# / .NET
    ".cs": "C#",
    ".csproj": ".NET project",
    ".sln": "Visual Studio solution",
    ".props": "MSBuild",
    ".targets": "MSBuild",
    ".cshtml": "Razor (CSHTML)",
    # JS / TS
    ".js": "JavaScript",
    ".mjs": "JavaScript (ESM)",
    ".cjs": "JavaScript (CJS)",
    ".jsx": "JavaScript (React)",
    ".ts": "TypeScript",
    ".mts": "TypeScript (ESM)",
    ".cts": "TypeScript (CJS)",
    ".tsx": "TypeScript (React)",
    ".json": "JSON",
    ".jsonc": "JSONC",
    ".map": "Source map (generated?)",
    ".lock": "Lockfile",
    # Python
    ".py": "Python",
    ".toml": "TOML",
    # Shell / scripting
    ".sh": "Shell",
    ".ps1": "PowerShell",
    # Infra / config
    ".yml": "YAML",
    ".yaml": "YAML",
    ".tf": "Terraform",
    ".tfvars": "Terraform vars",
    ".env": "Env file",
    ".ini": "INI",
    ".config": "Config",
    ".xml": "XML",
    # Web / styling
    ".html": "HTML",
    ".css": "CSS",
    ".scss": "Sass/SCSS",
    # Docs
    ".md": "Markdown",
    ".txt": "Text",
    ".example": "Example/template file",
    # SQL
    ".sql": "SQL",
    # Testing / specs
    ".feature": "Gherkin",
    # API tooling
    ".bru": "Bruno (API client)",
    # Diagrams / misc
    ".drawio": "Draw.io diagram",
    # Assets (avoid formatting)
    ".png": "Image (PNG)",
    ".ico": "Image (ICO)",
    ".svg": "Vector image (SVG)",
    ".woff2": "Font (WOFF2)",
    ".woff": "Font (WOFF)",
    ".gitkeep": "Repo placeholder",
    ".code-workspace": "VS Code workspace",
}


# Filename-only mapping (no extension / special)
NAME_TO_TYPE: dict[str, str] = {
    "Dockerfile": "Docker",
    "Makefile": "Make",
    "LICENSE": "License",
    "NOTICE": "License/Notice",
    "COPYING": "License",
}


# Dotfiles (treated as filenames)
DOTFILE_TO_TYPE: dict[str, str] = {
    ".editorconfig": "EditorConfig",
    ".gitattributes": "Git attributes",
    ".gitignore": "Git ignore",
    ".gitmodules": "Git submodules",
    ".dockerignore": "Docker ignore",
    ".npmrc": "NPM config",
    ".yarnrc": "Yarn config",
    ".python-version": "Python version",
}


SHEBANG_TO_TYPE: list[tuple[str, str]] = [
    ("python", "Python (shebang)"),
    ("python3", "Python (shebang)"),
    ("bash", "Shell (bash shebang)"),
    ("sh", "Shell (sh shebang)"),
    ("zsh", "Shell (zsh shebang)"),
    ("node", "JavaScript (node shebang)"),
    ("deno", "JavaScript/TypeScript (deno shebang)"),
]


def read_first_line(path: Path) -> str:
    try:
        with path.open("rb") as f:
            line = f.readline(256)
        return line.decode("utf-8", errors="replace").strip()
    except OSError:
        return ""


def classify(repo_root: Path, rel_path: str) -> tuple[str, str]:
    """
    Returns (bucket, type_label)
    - bucket: extension bucket or special filename, used for extension counts
    - type_label: inferred type/language group
    """
    p = Path(rel_path)
    name = p.name
    lower_name = name.lower()

    # dotfiles
    if lower_name in DOTFILE_TO_TYPE:
        return lower_name, DOTFILE_TO_TYPE[lower_name]

    # known special filenames
    if name in NAME_TO_TYPE:
        return name, NAME_TO_TYPE[name]

    # extension mapping
    ext = p.suffix.lower()
    if ext:
        type_label = EXT_TO_TYPE.get(ext, "Unknown")
        return ext, type_label

    # no extension: try shebang
    abs_path = repo_root / p
    first = read_first_line(abs_path)
    if first.startswith("#!"):
        low = first.lower()
        for token, t in SHEBANG_TO_TYPE:
            if token in low:
                return "(no_ext)", t
        return "(no_ext)", "Script (unknown shebang)"

    # no extension: fallback by filename patterns
    if lower_name in {"readme", "license"}:
        return "(no_ext)", "Docs/Legal"

    return "(no_ext)", "Unknown (no_ext)"


def main() -> int:
    if len(sys.argv) != 3:
        print(
            "Usage: report-repo-languages.py <repo_root> <tracked-files.txt>",
            file=sys.stderr,
        )
        return 2

    repo_root = Path(sys.argv[1]).resolve()
    files_list = Path(sys.argv[2]).resolve()

    if not files_list.exists():
        print(f"File not found: {files_list}", file=sys.stderr)
        return 2

    pair_counts: Counter[tuple[str, str]] = Counter()

    unknown_paths: list[str] = []
    unknown_no_ext_paths: list[str] = []

    with files_list.open("r", encoding="utf-8", errors="replace") as f:
        for line in f:
            rel = line.strip()
            if not rel:
                continue

            bucket, type_label = classify(repo_root, rel)
            pair_counts[(bucket, type_label)] += 1

            if type_label == "Unknown":
                unknown_paths.append(rel)
            elif type_label == "Unknown (no_ext)":
                unknown_no_ext_paths.append(rel)

    print("=== File types (bucket + classification) ===\n")
    max_bucket_len = max(len(bucket) for (bucket, _) in pair_counts) + 1

    for (bucket, type_label), n in pair_counts.most_common():
        print(f"{n:6}  {bucket:<{max_bucket_len}}  {type_label}")

    if unknown_paths or unknown_no_ext_paths:
        print("\n=== Unknowns to map ===")

        if unknown_paths:
            print(f"{len(unknown_paths):5}  Unknown (extension not mapped)\n")
            for p in unknown_paths[:50]:
                print(p)
            if len(unknown_paths) > 50:
                print(f"... ({len(unknown_paths) - 50} more)")

        if unknown_no_ext_paths:
            print(f"\n{len(unknown_no_ext_paths):5}  Unknown (no extension)\n")
            for p in unknown_no_ext_paths[:50]:
                print(f"{'':<4}- {p}")
            if len(unknown_no_ext_paths) > 50:
                print(f"{'':<4}... ({len(unknown_no_ext_paths) - 50} more)")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

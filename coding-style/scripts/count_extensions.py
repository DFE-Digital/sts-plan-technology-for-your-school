#!/usr/bin/env python3
from __future__ import annotations

import sys
from collections import Counter
from pathlib import Path


# Files that have "semantic meaning" without an extension
SPECIAL_FILENAMES: dict[str, str] = {
    "Dockerfile": "Dockerfile",
    "Makefile": "Makefile",
    "LICENSE": "LICENSE",
    "NOTICE": "NOTICE",
    "COPYING": "COPYING",
}


# Dotfiles to treat as their own buckets
SPECIAL_DOTFILES = {
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    ".gitmodules",
    ".dockerignore",
    ".npmrc",
    ".yarnrc",
    ".python-version",
}


def bucket_for_path(path_str: str) -> str:
    p = Path(path_str)
    name = p.name

    if name in SPECIAL_FILENAMES:
        return SPECIAL_FILENAMES[name]

    lower = name.lower()
    if lower in SPECIAL_DOTFILES:
        return lower

    # Dotfile like ".something" that isn't in our special list
    if name.startswith(".") and name.count(".") == 1:
        return name

    ext = p.suffix.lower()
    return ext if ext else "(no_ext)"


def main() -> int:
    if len(sys.argv) != 2:
        print("Usage: count_extensions.py <tracked-files.txt>", file=sys.stderr)
        return 2

    input_file = Path(sys.argv[1])

    if not input_file.exists():
        print(f"File not found: {input_file}", file=sys.stderr)
        return 2

    counter: Counter[str] = Counter()

    with input_file.open("r", encoding="utf-8", errors="replace") as f:
        for line in f:
            rel = line.strip()
            if not rel:
                continue
            counter[bucket_for_path(rel)] += 1

    for ext, n in counter.most_common():
        print(f"{n:7}  {ext}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())

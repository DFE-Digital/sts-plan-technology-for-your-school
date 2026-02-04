#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./coding-style/scripts/list-file-extensions.sh [directory]

What it does:
  - Writes a list of git-tracked files to: coding-style/output/tracked-files.txt
    or coding-style/output/<directory-name>-tracked-files.txt if a directory is specified
  - Then prints:
    - counts by extension with inferred language/type

Run from anywhere inside the repo.
EOF
}

MODE="all"
TARGET_DIR=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help)    usage; exit 0 ;;
    *)            TARGET_DIR="$1"; shift; continue ;;
  esac
done

REPO_ROOT="$(git rev-parse --show-toplevel)"
STYLE_DIR="$REPO_ROOT/coding-style"
SCRIPTS_DIR="$STYLE_DIR/scripts"
OUTPUT_DIR="$STYLE_DIR/output"

# Determine output filename and git pathspec
GIT_PATHSPEC=""
if [[ -n "$TARGET_DIR" ]]; then
  DIR_NAME=$(basename "$TARGET_DIR")
  TRACKED_FILES="$OUTPUT_DIR/${DIR_NAME}-tracked-files.txt"
  GIT_PATHSPEC="$TARGET_DIR"
else
  TRACKED_FILES="$OUTPUT_DIR/tracked-files.txt"
fi

mkdir -p "$OUTPUT_DIR"

echo "Repo root: $REPO_ROOT"
echo "Output:    $TRACKED_FILES"

echo "Collecting tracked files from git..."
if [[ -n "$GIT_PATHSPEC" ]]; then
  git -C "$REPO_ROOT" ls-files -- "$GIT_PATHSPEC" > "$TRACKED_FILES"
else
  git -C "$REPO_ROOT" ls-files > "$TRACKED_FILES"
fi
echo "Wrote $(wc -l < "$TRACKED_FILES") paths"

echo

python "$SCRIPTS_DIR/report-repo-languages.py" "$REPO_ROOT" "$TRACKED_FILES"

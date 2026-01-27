#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./linting/scripts/list-file-extensions.sh

What it does:
  - Writes a list of git-tracked files to: linting/output/tracked-files.txt
  - Then prints:
    - counts by extension with inferred language/type

Run from anywhere inside the repo.
EOF
}

MODE="all"

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h|--help)    usage; exit 0 ;;
    *)            echo "Unknown arg: $1" >&2; usage; exit 2 ;;
  esac
done

REPO_ROOT="$(git rev-parse --show-toplevel)"
LINTING_DIR="$REPO_ROOT/linting"
SCRIPTS_DIR="$LINTING_DIR/scripts"
OUTPUT_DIR="$LINTING_DIR/output"

TRACKED_FILES="$OUTPUT_DIR/tracked-files.txt"

mkdir -p "$OUTPUT_DIR"

echo "Repo root: $REPO_ROOT"
echo "Output:    $TRACKED_FILES"

echo "Collecting tracked files from git..."
git -C "$REPO_ROOT" ls-files > "$TRACKED_FILES"
echo "Wrote $(wc -l < "$TRACKED_FILES") paths"

echo

python "$SCRIPTS_DIR/report-repo-languages.py" "$REPO_ROOT" "$TRACKED_FILES"

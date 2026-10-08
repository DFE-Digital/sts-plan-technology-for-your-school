#!/usr/bin/env bash
set -euo pipefail

# =========================
# Config
# =========================
ENV_FILE=".env"
CONTENT_TYPES_FILE="content-types.txt"
SQL_OUTPUT_FILE="1.insert-content-$(date +%Y%m%d).sql"

if [ ! -f "$ENV_FILE" ]; then
  echo "Missing $ENV_FILE file. Please create one with CONTENTFUL_TOKEN, ENVIRONMENT, SPACE_ID." >&2
  exit 1
fi

set -a
# shellcheck disable=SC1090
source "$ENV_FILE"
set +a

# Verify required vars
: "${CONTENTFUL_TOKEN:?CONTENTFUL_TOKEN is not set in .env}"
: "${ENVIRONMENT:?ENVIRONMENT is not set in .env}"
: "${SPACE_ID:?SPACE_ID is not set in .env}"

env="$ENVIRONMENT"
spaceId="$SPACE_ID"
token="$CONTENTFUL_TOKEN"

baseUrl="https://cdn.contentful.com/spaces/$spaceId/environments/$env/entries?access_token=$token"

# =========================
# Dependencies
# =========================
command -v curl >/dev/null || { echo "curl is required" >&2; exit 1; }
command -v jq   >/dev/null || { echo "jq is required" >&2; exit 1; }
command -v base64 >/dev/null || { echo "base64 is required" >&2; exit 1; }

# =========================
# Read content types
# =========================
read_content_types() {
  local -a types=()

  if [ -f "$CONTENT_TYPES_FILE" ]; then
    while IFS= read -r line; do
      # Strip comments and trim whitespace
      line="${line%%#*}"
      line="$(echo "$line" | awk '{$1=$1;print}')"
      [ -z "$line" ] && continue
      types+=("$line")
    done < "$CONTENT_TYPES_FILE"

    echo "Loaded content types from $CONTENT_TYPES_FILE: ${types[*]}" >&2
  else
    echo "Content types file $CONTENT_TYPES_FILE not found. Add some to $CONTENT_TYPES_FILE (one per line)." >&2
    exit 1
  fi

  if [ ${#types[@]} -eq 0 ]; then
    echo "No content types found. Add some to $CONTENT_TYPES_FILE (one per line)." >&2
    exit 1
  fi

  printf '%s\n' "${types[@]}"
}

# =========================
# Helper: fetch all entries with pagination
# =========================
get_contentful_entries() {
  local contentType=$1
  local limit=100
  local skip=0
  local total=1
  local allItems="[]"
  local response=""
  local url=""

  while [ "$skip" -lt "$total" ]; do
    url="$baseUrl&content_type=$contentType&limit=$limit&skip=$skip"
    response="$(curl -s "$url")"

    local items
    items="$(echo "$response" | jq '.items')"
    allItems="$(echo "$allItems" "$items" | jq -s '.[0] + .[1]')"

    total="$(echo "$response" | jq -r '.total')"
    if ! [[ "$total" =~ ^[0-9]+$ ]]; then
      echo "Unexpected 'total' value for number of $contentType entries (total = $total). Exiting." >&2
      exit 1
    fi

    skip=$((skip + limit))
  done

  local sys
  sys="$(echo "$response" | jq '.sys')"

  # Output structure as JSON (stdout)
  echo "{\"sys\": $sys, \"total\": $(echo "$allItems" | jq 'length'), \"skip\": 0, \"limit\": $limit, \"items\": $allItems}"

  local length
  length="$(echo "$allItems" | jq 'length')"
  echo "Retrieved $length $contentType entries" >&2
}

# =========================
# Function to base64-encode JSON for SQL (single line)
# =========================
json_to_base64() {
  local json="$1"

  # GNU base64 supports -w 0; BSD/macOS does not
  if base64 --help 2>/dev/null | grep -q -- "-w"; then
    echo -n "$json" | base64 -w 0
  else
    echo -n "$json" | base64 | tr -d '\n'
  fi
}

# =========================
# Download + encode Contentful data
# =========================
declare -A contentTypeToBase64=()

mapfile -t contentTypes < <(read_content_types)

for ct in "${contentTypes[@]}"; do
  echo "Fetching $ct entries from Contentful..."
  json="$(get_contentful_entries "$ct")"
  contentTypeToBase64["$ct"]="$(json_to_base64 "$json")"
done

echo "Data encoded successfully"

# =========================
# Build SQL script
# =========================
cat > "$SQL_OUTPUT_FILE" <<'EOF'
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'contentful')
    EXEC('CREATE SCHEMA contentful;');

IF OBJECT_ID('dbo.ContentfulImport') IS NOT NULL
    DROP TABLE dbo.ContentfulImport;

IF OBJECT_ID('contentful.contentfulImport') IS NULL
    CREATE TABLE contentful.contentfulImport (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ContentType NVARCHAR(50),
        JsonResponse NVARCHAR(MAX),
        ImportedAt DATETIME2 DEFAULT SYSUTCDATETIME()
    );

DELETE FROM contentful.contentfulImport;

-- Required SET options for XML decoding
SET ANSI_WARNINGS ON;
SET ANSI_PADDING ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET CONCAT_NULL_YIELDS_NULL ON;

-- Insert JSON payloads (Base64 encoded)
EOF

for ct in "${contentTypes[@]}"; do
  b64="${contentTypeToBase64[$ct]}"

  cat >> "$SQL_OUTPUT_FILE" <<EOF

INSERT INTO contentful.contentfulImport (ContentType, JsonResponse)
VALUES (
  '$ct',
  CAST(
    CAST(N'' AS XML).value('xs:base64Binary("$b64")', 'VARBINARY(MAX)')
    AS VARCHAR(MAX) -- interpret as UTF-8
  )
);
EOF
done

echo "✅ SQL script written to $SQL_OUTPUT_FILE"

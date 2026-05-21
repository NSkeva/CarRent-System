#!/usr/bin/env bash
# Zajednicka logika za pronalaženje Cursor transkripta i izvoz po labu.

cursor_transcripts_root() {
  local candidate=""
  for candidate in \
    "${CURSOR_TRANSCRIPTS_DIR:-}" \
    "$HOME/.cursor/projects/home-nskeva-Documents-GitHub-CarRent-System/agent-transcripts" \
    "$HOME/.cursor/projects/home-nskeva-Documents-Github-CarRent-System/agent-transcripts"
  do
    if [[ -n "$candidate" && -d "$candidate" ]]; then
      printf '%s\n' "$candidate"
      return 0
    fi
  done
  return 1
}

cursor_latest_main_transcript() {
  local root="$1"
  local latest=""

  if [[ -n "${CURSOR_TRANSCRIPT_FILE:-}" && -f "${CURSOR_TRANSCRIPT_FILE}" ]]; then
    printf '%s\n' "${CURSOR_TRANSCRIPT_FILE}"
    return 0
  fi

  latest="$(
    find "$root" -mindepth 2 -maxdepth 2 -type f -name '*.jsonl' ! -path '*/subagents/*' -printf '%T@ %p\n' 2>/dev/null \
      | sort -rn \
      | head -n 1 \
      | cut -d' ' -f2- \
      || true
  )"

  if [[ -z "$latest" ]]; then
    latest="$(ls -t "$root"/*/*.jsonl 2>/dev/null | grep -v '/subagents/' | head -n 1 || true)"
  fi

  if [[ -n "$latest" && -f "$latest" ]]; then
    printf '%s\n' "$latest"
    return 0
  fi

  return 1
}

# Izvoz odredenog raspona linija (ukljucivo).
cursor_export_transcript_slice() {
  local transcript_file="$1"
  local out_file="$2"
  local start_line="$3"
  local end_line="${4:-}"

  mkdir -p "$(dirname "$out_file")"

  if [[ -z "$end_line" ]]; then
    sed -n "${start_line},\$p" "$transcript_file" >"$out_file"
  else
    sed -n "${start_line},${end_line}p" "$transcript_file" >"$out_file"
  fi
}

# Dinamicki raspon po korisnickim porukama (lab-3 / lab-4 markeri).
cursor_export_transcript_by_markers() {
  local transcript_file="$1"
  local out_file="$2"
  local start_pattern="$3"
  local end_pattern="${4:-}"

  python3 - "$transcript_file" "$out_file" "$start_pattern" "$end_pattern" <<'PY'
import json
import sys

transcript_path, out_path, start_pat, end_pat = sys.argv[1:5]
start_pat = start_pat.lower()
end_pat = end_pat.lower() if end_pat else ""

lines = []
with open(transcript_path, encoding="utf-8") as handle:
    lines = handle.readlines()

def user_text(line: str) -> str:
    try:
        obj = json.loads(line)
    except json.JSONDecodeError:
        return ""
    if obj.get("role") != "user":
        return ""
    chunks = []
    for part in obj.get("message", {}).get("content", []):
        if part.get("type") == "text":
            chunks.append(part.get("text", ""))
    return "\n".join(chunks).lower()

start_idx = None
end_idx = None

for idx, line in enumerate(lines):
    text = user_text(line)
    if not text:
        continue
    if start_idx is None and start_pat in text:
        start_idx = idx
        continue
    if start_idx is not None and end_pat and end_pat in text:
        end_idx = idx
        break

if start_idx is None:
    sys.stderr.write(f"Start marker not found: {start_pat}\n")
    sys.exit(1)

selected = lines[start_idx:] if end_idx is None else lines[start_idx:end_idx]

with open(out_path, "w", encoding="utf-8") as handle:
    handle.writelines(selected)

print(f"Exported {len(selected)} lines to {out_path}")
PY
}

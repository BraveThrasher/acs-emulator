#!/usr/bin/env bash
set -euo pipefail

CSPROJ="./AcsEmulatorAPI/AcsEmulatorAPI.csproj"
get_prop() { grep -oP "(?<=<${1}>)[^<]+" "$CSPROJ" 2>/dev/null || true; }

VERSION_RAW=$(grep -oP '(?<=<Version>)[^<]+' "$CSPROJ" 2>/dev/null || true)
if echo "$VERSION_RAW" | grep -qP '^\$\([^)]+\)$'; then
  REF=$(echo "$VERSION_RAW" | grep -oP '(?<=\$\()[^)]+(?=\))')
  VERSION_BASE=$(get_prop "$REF")
else
  VERSION_BASE="$VERSION_RAW"
fi

VERSION_BASE="${VERSION_BASE:-$(get_prop 'Version')}"
VERSION_BASE="${VERSION_BASE:-$(get_prop 'VersionPrefix')}"
VERSION_BASE="${VERSION_BASE:-$(get_prop 'PackageVersion')}"
VERSION_BASE="${VERSION_BASE:-$(get_prop 'AssemblyVersion')}"
VERSION_BASE="${VERSION_BASE:-0.0.0}"

VERSION_SUFFIX=$(get_prop 'VersionSuffix')
if [ -n "$VERSION_SUFFIX" ]; then
  VERSION_BASE="${VERSION_BASE}-${VERSION_SUFFIX}"
fi

DATE=$(date -u +%Y%m%d)
VERSION="${VERSION_BASE}.${DATE}"

echo "Extracted base version: ${VERSION_BASE}"
echo "Calculated VERSION: ${VERSION}"
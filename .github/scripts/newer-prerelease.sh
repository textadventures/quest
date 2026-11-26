#!/usr/bin/env bash
# Prints the newest prerelease tag that is newer than a stable tag, or nothing
# if there isn't one. Reads the tags to consider from stdin, one per line, e.g.
#
#   git ls-remote --tags --refs origin 'v*' | sed 's#.*refs/tags/##' \
#     | newer-prerelease.sh v6.1.0
#
# A prerelease is older than its own release (v6.1.0-rc.1 < v6.1.0), so only a
# prerelease of a later version counts. deploy-play.yml uses this to decide
# whether a stable release also goes to play-beta.questviva.com — see
# docs/release-channels.md.
set -euo pipefail

stable="${1#v}"
case "$stable" in
  *-*) echo "newer-prerelease.sh: $1 is not a stable tag" >&2; exit 1 ;;
esac

# True if version $1 is strictly greater than $2 (both plain X.Y.Z).
greater() {
  [ "$1" != "$2" ] && [ "$(printf '%s\n%s\n' "$1" "$2" | sort -V | tail -n1)" == "$1" ]
}

newest=""
newest_version=""
while IFS= read -r tag; do
  version="${tag#v}"
  case "$version" in
    *-*) ;;
    *) continue ;;
  esac
  base="${version%%-*}"
  greater "$base" "$stable" || continue
  # Among the candidates, sort -V orders prereleases of one version correctly
  # (beta.9 < beta.10 < rc.1), and their bases differ otherwise.
  if [ -z "$newest" ] || [ "$(printf '%s\n%s\n' "$version" "$newest_version" | sort -V | tail -n1)" == "$version" ]; then
    newest="$tag"
    newest_version="$version"
  fi
done

if [ -n "$newest" ]; then
  echo "$newest"
fi

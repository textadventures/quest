#!/usr/bin/env bash
# Prints the newest stable tag that is newer than a stable tag, or nothing if
# there isn't one. Reads the tags to consider from stdin, one per line, e.g.
#
#   git ls-remote --tags --refs origin 'v*' | sed 's#.*refs/tags/##' \
#     | newer-stable.sh v6.0.5
#
# A stable tag with a newer stable tag is a maintenance release of an older
# line (6.0.N after 6.1.0), which mustn't take over play.questviva.com,
# GitHub's Latest release or npm's `latest` dist-tag. deploy-play.yml,
# npm-publish.yml and finalize-release.yml use this to tell — see
# docs/release-channels.md.
set -euo pipefail

stable="${1#v}"
case "$stable" in
  *-*) echo "newer-stable.sh: $1 is not a stable tag" >&2; exit 1 ;;
esac

newest=""
newest_version="$stable"
while IFS= read -r tag; do
  version="${tag#v}"
  case "$version" in
    *-*) continue ;;
  esac
  # Only plain X.Y.Z tags count.
  [[ "$version" =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]] || continue
  if [ "$version" != "$newest_version" ] && \
     [ "$(printf '%s\n%s\n' "$version" "$newest_version" | sort -V | tail -n1)" == "$version" ]; then
    newest="$tag"
    newest_version="$version"
  fi
done

if [ -n "$newest" ]; then
  echo "$newest"
fi

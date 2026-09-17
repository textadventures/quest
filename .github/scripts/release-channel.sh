#!/usr/bin/env bash
# Prints the release channel ("stable" or "prerelease") for a version tag,
# e.g. `release-channel.sh v6.1.0-beta.1` prints "prerelease". Every
# tag-triggered workflow uses this to decide where a release goes — see
# docs/release-channels.md.
set -euo pipefail

version="${1#v}"

case "$version" in
  # Temporary: 6.0.0's release candidates are what play.questviva.com serves
  # until 6.0.0 itself ships, so they keep the stable behaviour. Remove this
  # line as part of the 6.0.0 cut.
  6.0.0-rc.*) echo stable ;;
  *-*) echo prerelease ;;
  *) echo stable ;;
esac

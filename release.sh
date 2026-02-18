#!/bin/bash
set -e

VERSION=$(cat VERSION)
TAG="v$VERSION"

# Must be on main with a clean tree
BRANCH=$(git branch --show-current)
if [ "$BRANCH" != "main" ]; then
  echo "❌ Not on main branch (currently on $BRANCH)"
  exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
  echo "❌ Working tree is not clean. Commit or stash changes first."
  exit 1
fi

# Check local and remote are in sync
git fetch origin main --quiet
LOCAL=$(git rev-parse HEAD)
REMOTE=$(git rev-parse origin/main)
if [ "$LOCAL" != "$REMOTE" ]; then
  echo "❌ Local main is not in sync with origin/main. Push or pull first."
  exit 1
fi

echo "Tagging $TAG and pushing..."
git tag "$TAG"
git push origin "$TAG"
echo "✅ Pushed $TAG — GitHub Actions will build and publish the Docker image."

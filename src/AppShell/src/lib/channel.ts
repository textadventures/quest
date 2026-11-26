import { PUBLIC_APPSHELL_VERSION } from "$app/env/public";

// The release this build was made from, without its "v" (e.g. "6.1.0"), or
// null for a build that isn't a release: a dev build, or a manual
// deploy-play.yml run, whose version is "<short SHA>-<run ID>-<attempt>".
const version = PUBLIC_APPSHELL_VERSION.replace(/^v/, "");

// A stable release has no "-" suffix, the same rule as
// .github/scripts/release-channel.sh. A non-release build counts as a
// prerelease, since it's built from main.
export const isStableBuild = /^\d+\.\d+\.\d+$/.test(version);

export const stableVersion = isStableBuild ? version : null;

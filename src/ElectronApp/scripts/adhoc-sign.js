// Free ad-hoc code signing (no Apple Developer cert needed) as a stopgap
// until real Developer ID signing is wired in (see docs/electron-desktop-app.md,
// "Resolved decisions"). Apple Silicon refuses to even launch a completely
// unsigned app ("damaged, can't be opened") rather than the milder
// "unidentified developer" prompt Intel Macs give unsigned apps — ad-hoc
// signing (`codesign --sign -`) satisfies the mandatory-signature
// requirement without a real identity, downgrading that to the normal
// bypassable Gatekeeper warning.
//
// electron-builder invokes this as its "afterSign" hook — CommonJS, not the
// .mjs used elsewhere in this project, since that's what electron-builder's
// hook loader expects.

const { execFileSync } = require("node:child_process");
const path = require("node:path");

module.exports = async function adhocSign(context) {
    // A real identity means electron-builder's own signing step already ran.
    if (process.env.CSC_LINK) return;

    const appPath = path.join(context.appOutDir, `${context.packager.appInfo.productFilename}.app`);
    execFileSync("codesign", ["--force", "--deep", "--sign", "-", appPath], { stdio: "inherit" });
};

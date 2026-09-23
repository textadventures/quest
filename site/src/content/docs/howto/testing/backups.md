---
title: Backing up your game
description: Why it's worth keeping backups of your game source, whether it's a local draft in the browser or a file on disk
---

However your game is stored, it exists in exactly one place until you make a copy of it somewhere else. That matters more than it sounds like it should - an accidental delete, a script change you regret, or a browser/disk problem can lose you real work, and none of it is as recoverable as you might expect.

## Local drafts in the browser

If you didn't pick a folder when you created your game, it's stored inside the browser itself - not as a file you can find and copy. That storage can be cleared by a "Clear browsing data" cleanup, a browser reinstall, running in a private/incognito window, or just running low on disk space, and none of those give you any warning it's about to happen.

**Backup…**, in the **File** menu, downloads a `.zip` of your game and its assets that you can import again later with **Open a game file…**. It's not automatic - you have to remember to do it - so the editor reminds you every so often with a banner at the top of the screen. Don't dismiss it without backing up at least occasionally.

Keep more than just the latest one. If you only ever keep a single backup and overwrite it each time, you can back up moments after breaking something and still have no way back to how the game was before. A dated `.zip` every so often - after finishing a room, before a big change - costs almost nothing and means you can always get back to a recent working state, not just the most recent one.

## Games saved to a folder

If your game is an ordinary file on disk - the desktop app always saves this way, and so does "Save to folder…" in a browser that supports it - it survives all of the above, because it isn't tied to any one browser or browser profile.

That's not the same as being safe from *you*, though. The editor's own **Undo** button only reverses edits made in your current session - close the game and reopen it, and that history is gone.

So if you delete an object, rewrite a script, or make some other change you later regret, and you've saved since, the one copy of the file on disk is the only one that exists. Treat it like anything else on your computer you'd be unhappy to lose:

- Keep dated copies every so often, the same as for a browser draft - copy the `.aslx` file (and its folder, if it has one) somewhere else before a big change.
- A folder that syncs to the cloud (Dropbox, OneDrive, iCloud Drive, and similar) protects against a lost or damaged disk, and some of them keep old versions of a file you can restore from.
- If you're comfortable with version control, a tool like [Git](https://git-scm.com/) works well here - `.aslx` is plain text, so it diffs and commits cleanly, and you get a full history you can step back through and compare, not just your last few copies.

## See also

- [Publishing your game](/publishing#the-publish-process) - **Publish** is not a backup, it's a build for players
- [Where your game is kept](/intro#browser-or-desktop) - the difference between a local draft and a game saved to a folder

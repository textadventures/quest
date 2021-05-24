---
layout: index
title: How to run Quest on Linux
---

Very grateful to redyoshi49q for working out how to do this. His/her instructions are reproduced as-is. The discusion can be found [here](http://textadventures.co.uk/forum/samples/topic/nwsz5gf0te6qdtfrwzdmga/quest-5-8-working-in-wine).

***

I seem to have finally gotten Quest working in Wine on Linux, thanks in no small part due to helpful tidbits that were dropped in the bug report viewable at https://bugs.winehq.org/show_bug.cgi?id=43408 .

Below are the commands (with some portability tweaks) that I used; it's assumed that:
* winecfg and winetricks are already installed (Ubuntu/Debian users, as of this post, may need to use the Github version of winetricks rather than the distro repository version if there's complaints of a sha256 mismatch)
* quest580.exe has already been downloaded and is in ~/Downloads
* a recent Wine is installed (I got this working on Wine 6.9 from Wine's package repos rather than a distro's Wine package)

```
cd ~/Games

WINEARCH=win32 WINEPREFIX="/home/$USER/Games/Quest" winetricks -q windowscodecs gdiplus corefonts dotnet40 vb6run speechsdk

WINEARCH=win32 WINEPREFIX="/home/$USER/Games/Quest" winecfg

# at this point, a winecfg window will pop up
# you will need to set the emulated Windows version to something more recent than XP
# if you don't, the installer will complain in the next step; Windows 7 worked for me

WINEARCH=win32 WINEPREFIX="/home/$USER/Games/Quest" wine ../Downloads/quest580.exe
```

There might be a winetricks package or two that isn't strictly necessary, but the speechsdk package /does/ appear to be strictly necessary, and since the speechsdk package only supports 32 bit Wine prefixes, the WINEARCH=win32 environment is necessary.

The compatibility may not necessarily be perfect, but so far, Quest running in Wine /does/ appear to run games as expected for me (based on my rather limited testing).  I hope this walkthrough is able to help those of you who have been unable to play Quest games due to using Linux systems.

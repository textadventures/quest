Note: _This is only applicable to the off-line editor. If working on line, you can add a spell-checker to the browser and it will check the text as you type._

The Quest off-line editor has no spell checker, which is a sad failing in a genre that is all about the written word. Here is a technique that will allow you to spell check your work. However, it is still vital that you get you game properly beta-tested!

## Notepad++

Notepad++ is a text editor, and is useful for editing library files and other things when creating Quest games.

Notepad++ can be downloaded here:
https://notepad-plus-plus.org/

Follow the download link, then click _Download_ to download the installer.

The plugin is here:
https://sourceforge.net/projects/dspellcheck/

Click the "Download DSpellCheck.zip" button.

There is supposed to be an instability issue with the plugin, and you may find Notepad++ warns you about that; just ignore it (and hope for the best!).

Once both are installed, you should be able to go into Windows Explorer, and find your game file. Make a copy of it before doing anything else, just in case you mess it all up!

Right click on the file, and select "Edit with Notepad++". Quest uses XML to organise all the data, and that is what you are now looking it. If you have never seen it before, it will look pretty daunting. XML tags are surrounded by < and >, and you should ignore everything inside those. We can use a feature of Notepad++ to make that easier.

[[https://github.com/ThePix/quest/blob/master/notepad_context.png]]

Go to _Settings - Style Configutator..._ and on the left side of the dialogue box, select XML. At the bottom, in the "User ext.:" box type "aslx" (without quotes). This will tell Notepad++ to treat Quest games as XML. You can also change the colours used here, by the way. The important ones are DEFAULT and CDATA, and I set these to be black and dark grey respectively, and both in bold.

[[https://github.com/ThePix/quest/blob/master/notepad_xml.png]]

Click save and close, and it should all be a lot more colourful. This should help you see what is an XML tag, and should be ignored, and what is actual content, and should be spell-checked.

Some of the content will be scripts; it should be obvious what is a script as it is code, but you should spell check any sections of a script that is inside quote marks too (sadly no colours to help you here).
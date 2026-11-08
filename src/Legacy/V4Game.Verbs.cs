using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QuestViva.Common;
using Constants = Microsoft.VisualBasic.Constants;
using Strings = Microsoft.VisualBasic.Strings;

namespace QuestViva.Legacy;

public partial class V4Game
{
    private async Task DoAddRemove(int childId, int parentId, bool add, Context ctx)
    {
        if (add)
        {
            await AddToObjectProperties("parent=" + _objs[parentId].ObjectName, childId, ctx);
            _objs[childId].ContainerRoom = _objs[parentId].ContainerRoom;
        }
        else
        {
            await AddToObjectProperties("not parent", childId, ctx);
        }

        if (ASLVersion >= 410)
            // Putting something in a container implicitly makes that
            // container "seen". Otherwise we could try to "look at" the
            // object we just put in the container and have disambigution fail!
        {
            await AddToObjectProperties("seen", parentId, ctx);
        }

        await UpdateVisibilityInContainers(ctx, _objs[parentId].ObjectName);
    }

    private async Task DoLook(int id, Context ctx, bool showExamineError = false, bool showDefaultDescription = true)
    {
        string objectContents;
        var foundLook = false;

        // First, set the "seen" property, and for ASL >= 391, update visibility for any
        // object that is contained by this object.

        if (ASLVersion >= 391)
        {
            await AddToObjectProperties("seen", id, ctx);
            await UpdateVisibilityInContainers(ctx, _objs[id].ObjectName);
        }

        // First look for action, then look
        // for property, then check define
        // section:

        string lookLine;
        var o = _objs[id];

        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if (o.Actions[i].ActionName == "look")
            {
                foundLook = true;
                await ExecuteScript(o.Actions[i].Script, ctx);
                break;
            }
        }

        if (!foundLook)
        {
            for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
            {
                if (o.Properties[i].PropertyName == "look")
                {
                    // do this odd RetrieveParameter stuff to convert any variables
                    await Print(await GetParameter("<" + o.Properties[i].PropertyValue + ">", ctx), ctx);
                    foundLook = true;
                    break;
                }
            }
        }

        if (!foundLook)
        {
            for (int i = o.DefinitionSectionStart, loopTo2 = o.DefinitionSectionEnd; i <= loopTo2; i++)
            {
                if (BeginsWith(_lines[i], "look "))
                {
                    lookLine = Strings.Trim(GetEverythingAfter(_lines[i], "look "));

                    if (Strings.Left(lookLine, 1) == "<")
                    {
                        await Print(await GetParameter(_lines[i], ctx), ctx);
                    }
                    else
                    {
                        await ExecuteScript(lookLine, ctx, id);
                    }

                    foundLook = true;
                }
            }
        }

        if (ASLVersion >= 391)
        {
            objectContents = await ListContents(id, ctx);
        }
        else
        {
            objectContents = "";
        }

        if (!foundLook & showDefaultDescription)
        {
            PlayerError err;

            if (showExamineError)
            {
                err = PlayerError.DefaultExamine;
            }
            else
            {
                err = PlayerError.DefaultLook;
            }

            // Print "Nothing out of the ordinary" or whatever, but only if we're not going to list
            // any contents.

            if (string.IsNullOrEmpty(objectContents))
            {
                await PlayerErrorMessage(err, ctx);
            }
        }

        if (!string.IsNullOrEmpty(objectContents) & (objectContents != "<script>"))
        {
            await Print(objectContents, ctx);
        }
    }

    private async Task DoOpenClose(int id, bool open, bool showLook, Context ctx)
    {
        if (open)
        {
            await AddToObjectProperties("opened", id, ctx);
            if (showLook)
            {
                await DoLook(id, ctx, showDefaultDescription: false);
            }
        }
        else
        {
            await AddToObjectProperties("not opened", id, ctx);
        }

        await UpdateVisibilityInContainers(ctx, _objs[id].ObjectName);
    }

    private async Task ExecAddRemove(string cmd, Context ctx)
    {
        int childId;
        string childName;
        var doAdd = default(bool);
        int sepPos = default, parentId, sepLen = default;
        string parentName;
        var verb = "";
        string action;
        var foundAction = default(bool);
        var actionScript = "";
        bool propertyExists;
        string textToPrint;
        bool isContainer;
        bool gotObject;
        int childLength;
        var noParentSpecified = false;

        if (BeginsWith(cmd, "put "))
        {
            verb = "put";
            doAdd = true;
            sepPos = Strings.InStr(cmd, " on ");
            sepLen = 4;
            if (sepPos == 0)
            {
                sepPos = Strings.InStr(cmd, " in ");
                sepLen = 4;
            }

            if (sepPos == 0)
            {
                sepPos = Strings.InStr(cmd, " onto ");
                sepLen = 6;
            }
        }
        else if (BeginsWith(cmd, "add "))
        {
            verb = "add";
            doAdd = true;
            sepPos = Strings.InStr(cmd, " to ");
            sepLen = 4;
        }
        else if (BeginsWith(cmd, "remove "))
        {
            verb = "remove";
            doAdd = false;
            sepPos = Strings.InStr(cmd, " from ");
            sepLen = 6;
        }

        if (sepPos == 0)
        {
            noParentSpecified = true;
            sepPos = Strings.Len(cmd) + 1;
        }

        childLength = sepPos - (Strings.Len(verb) + 2);

        if (childLength < 0)
        {
            await PlayerErrorMessage(PlayerError.BadCommand, ctx);
            _badCmdBefore = verb;
            return;
        }

        childName = Strings.Trim(Strings.Mid(cmd, Strings.Len(verb) + 2, childLength));

        gotObject = false;

        if ((ASLVersion >= 392) & doAdd)
        {
            childId = await Disambiguate(childName, _currentRoom + ";inventory", ctx);

            if (childId > 0)
            {
                if (_objs[childId].ContainerRoom == "inventory")
                {
                    gotObject = true;
                }
                else
                {
                    // Player is not carrying the object they referred to. So, first take the object.
                    await Print("(first taking " + _objs[childId].Article + ")", ctx);
                    // Try to take the object
                    ctx.AllowRealNamesInCommand = true;
                    await ExecCommand("take " + _objs[childId].ObjectName, ctx, false, dontSetIt: true);

                    if (_objs[childId].ContainerRoom == "inventory")
                    {
                        gotObject = true;
                    }
                }

                if (!gotObject)
                {
                    _badCmdBefore = verb;
                    return;
                }
            }
            else
            {
                if (childId != -2)
                {
                    await PlayerErrorMessage(PlayerError.NoItem, ctx);
                }

                _badCmdBefore = verb;
                return;
            }
        }

        else
        {
            childId = await Disambiguate(childName, "inventory;" + _currentRoom, ctx);

            if (childId <= 0)
            {
                if (childId != -2)
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = verb;
                return;
            }
        }

        if (noParentSpecified & doAdd)
        {
            await SetStringContents("quest.error.article", _objs[childId].Article, ctx);
            await PlayerErrorMessage(PlayerError.BadPut, ctx);
            return;
        }

        if (doAdd)
        {
            action = "add";
        }
        else
        {
            action = "remove";
        }

        if (!noParentSpecified)
        {
            parentName = Strings.Trim(Strings.Mid(cmd, sepPos + sepLen));

            parentId = await Disambiguate(parentName, _currentRoom + ";inventory", ctx);

            if (parentId <= 0)
            {
                if (parentId != -2)
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = Strings.Left(cmd, sepPos + sepLen);
                return;
            }
        }
        else
        {
            // Assume the player was referring to the parent that the object is already in,
            // if it is even in an object already

            if (!IsYes(GetObjectProperty("parent", childId, true, false)))
            {
                await PlayerErrorMessage(PlayerError.CantRemove, ctx);
                return;
            }

            parentId = GetObjectIdNoAlias(GetObjectProperty("parent", childId));
        }

        // Check if parent is a container

        isContainer = IsYes(GetObjectProperty("container", parentId, true, false));

        if (!isContainer)
        {
            if (doAdd)
            {
                await PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }

            return;
        }

        // Check object is already held by that parent

        if (IsYes(GetObjectProperty("parent", childId, true, false)))
        {
            if (doAdd & ((Strings.LCase(GetObjectProperty("parent", childId, false, false)) ?? "") ==
                         (Strings.LCase(_objs[parentId].ObjectName) ?? "")))
            {
                await PlayerErrorMessage(PlayerError.AlreadyPut, ctx);
            }
        }

        // Check parent and child are accessible to player
        var canAccessObject = PlayerCanAccessObject(childId);
        if (!canAccessObject.CanAccessObject)
        {
            if (doAdd)
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessObject.ErrorMsg);
            }
            else
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessObject.ErrorMsg);
            }

            return;
        }

        var canAccessParent = PlayerCanAccessObject(parentId);
        if (!canAccessParent.CanAccessObject)
        {
            if (doAdd)
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantPut, ctx, canAccessParent.ErrorMsg);
            }
            else
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantRemove, ctx, canAccessParent.ErrorMsg);
            }

            return;
        }

        // Check if parent is a closed container

        if (!IsYes(GetObjectProperty("surface", parentId, true, false)) &
            !IsYes(GetObjectProperty("opened", parentId, true, false)))
        {
            // Not a surface and not open, so can't add to this closed container.
            if (doAdd)
            {
                await PlayerErrorMessage(PlayerError.CantPut, ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.CantRemove, ctx);
            }

            return;
        }

        // Now check if it can be added to (or removed from)

        // First check for an action
        var o = _objs[parentId];
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (action ?? ""))
            {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }

        if (foundAction)
        {
            await SetStringContents("quest." + Strings.LCase(action) + ".object.name", _objs[childId].ObjectName, ctx);
            await ExecuteScript(actionScript, ctx, parentId);
        }
        else
        {
            // Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, parentId, true, false));

            if (!propertyExists)
            {
                // Show error message
                if (doAdd)
                {
                    await PlayerErrorMessage(PlayerError.CantPut, ctx);
                }
                else
                {
                    await PlayerErrorMessage(PlayerError.CantRemove, ctx);
                }
            }
            else
            {
                textToPrint = GetObjectProperty(action, parentId, false, false);
                if (string.IsNullOrEmpty(textToPrint))
                {
                    // Show default message
                    if (doAdd)
                    {
                        await PlayerErrorMessage(PlayerError.DefaultPut, ctx);
                    }
                    else
                    {
                        await PlayerErrorMessage(PlayerError.DefaultRemove, ctx);
                    }
                }
                else
                {
                    await Print(textToPrint, ctx);
                }

                await DoAddRemove(childId, parentId, doAdd, ctx);
            }
        }
    }

    private async Task ExecAddRemoveScript(string parameter, bool add, Context ctx)
    {
        int childId, parentId = default;
        string commandName;
        string childName;
        var parentName = "";
        int scp;

        if (add)
        {
            commandName = "add";
        }
        else
        {
            commandName = "remove";
        }

        scp = Strings.InStr(parameter, ";");
        if ((scp == 0) & add)
        {
            LogASLError("No parent specified in '" + commandName + " <" + parameter + ">", LogType.WarningError);
            return;
        }

        if (scp != 0)
        {
            childName = Strings.LCase(Strings.Trim(Strings.Left(parameter, scp - 1)));
            parentName = Strings.LCase(Strings.Trim(Strings.Mid(parameter, scp + 1)));
        }
        else
        {
            childName = Strings.LCase(Strings.Trim(parameter));
        }

        childId = GetObjectIdNoAlias(childName);
        if (childId == 0)
        {
            LogASLError("Invalid child object name specified in '" + commandName + " <" + parameter + ">",
                LogType.WarningError);
            return;
        }

        if (scp != 0)
        {
            parentId = GetObjectIdNoAlias(parentName);
            if (parentId == 0)
            {
                LogASLError("Invalid parent object name specified in '" + commandName + " <" + parameter + ">",
                    LogType.WarningError);
                return;
            }

            await DoAddRemove(childId, parentId, add, ctx);
        }
        else
        {
            await AddToObjectProperties("not parent", childId, ctx);
            await UpdateVisibilityInContainers(ctx, _objs[parentId].ObjectName);
        }
    }

    private async Task ExecOpenClose(string cmd, Context ctx)
    {
        int id;
        string name;
        var doOpen = default(bool);
        bool isOpen, foundAction = default;
        var action = "";
        var actionScript = "";
        bool propertyExists;
        string textToPrint;
        bool isContainer;

        if (BeginsWith(cmd, "open "))
        {
            action = "open";
            doOpen = true;
        }
        else if (BeginsWith(cmd, "close "))
        {
            action = "close";
            doOpen = false;
        }

        name = GetEverythingAfter(cmd, action + " ");

        id = await Disambiguate(name, _currentRoom + ";inventory", ctx);

        if (id <= 0)
        {
            if (id != -2)
            {
                await PlayerErrorMessage(PlayerError.BadThing, ctx);
            }

            _badCmdBefore = action;
            return;
        }

        // Check if it's even a container

        isContainer = IsYes(GetObjectProperty("container", id, true, false));

        if (!isContainer)
        {
            if (doOpen)
            {
                await PlayerErrorMessage(PlayerError.CantOpen, ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.CantClose, ctx);
            }

            return;
        }

        // Check if it's already open (or closed)

        isOpen = IsYes(GetObjectProperty("opened", id, true, false));

        if (doOpen & isOpen)
        {
            // Object is already open
            await PlayerErrorMessage(PlayerError.AlreadyOpen, ctx);
            return;
        }

        if (!doOpen & !isOpen)
        {
            // Object is already closed
            await PlayerErrorMessage(PlayerError.AlreadyClosed, ctx);
            return;
        }

        // Check if it's accessible, i.e. check it's not itself inside another closed container

        var canAccessObject = PlayerCanAccessObject(id);
        if (!canAccessObject.CanAccessObject)
        {
            if (doOpen)
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantOpen, ctx, canAccessObject.ErrorMsg);
            }
            else
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.CantClose, ctx, canAccessObject.ErrorMsg);
            }

            return;
        }

        // Now check if it can be opened (or closed)

        // First check for an action
        var o = _objs[id];
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if ((Strings.LCase(o.Actions[i].ActionName) ?? "") == (action ?? ""))
            {
                foundAction = true;
                actionScript = o.Actions[i].Script;
                break;
            }
        }

        if (foundAction)
        {
            await ExecuteScript(actionScript, ctx, id);
        }
        else
        {
            // Now check for a property
            propertyExists = IsYes(GetObjectProperty(action, id, true, false));

            if (!propertyExists)
            {
                // Show error message
                if (doOpen)
                {
                    await PlayerErrorMessage(PlayerError.CantOpen, ctx);
                }
                else
                {
                    await PlayerErrorMessage(PlayerError.CantClose, ctx);
                }
            }
            else
            {
                textToPrint = GetObjectProperty(action, id, false, false);
                if (string.IsNullOrEmpty(textToPrint))
                {
                    // Show default message
                    if (doOpen)
                    {
                        await PlayerErrorMessage(PlayerError.DefaultOpen, ctx);
                    }
                    else
                    {
                        await PlayerErrorMessage(PlayerError.DefaultClose, ctx);
                    }
                }
                else
                {
                    await Print(textToPrint, ctx);
                }

                await DoOpenClose(id, doOpen, true, ctx);
            }
        }
    }

    private async Task ExecDrop(string obj, Context ctx)
    {
        bool found;
        int parentId = default, id;

        id = await Disambiguate(obj, "inventory", ctx);

        if (id > 0)
        {
            found = true;
        }
        else
        {
            found = false;
        }

        if (!found)
        {
            if (id != -2)
            {
                if (ASLVersion >= 391)
                {
                    await PlayerErrorMessage(PlayerError.NoItem, ctx);
                }
                else
                {
                    await PlayerErrorMessage(PlayerError.BadDrop, ctx);
                }
            }

            _badCmdBefore = "drop";
            return;
        }

        // If object is inside a container, it must be removed before it can be dropped.
        var isInContainer = false;
        if (ASLVersion >= 391)
        {
            if (IsYes(GetObjectProperty("parent", id, true, false)))
            {
                isInContainer = true;
                var parent = GetObjectProperty("parent", id, false, false);
                parentId = GetObjectIdNoAlias(parent);
            }
        }

        var dropFound = false;
        var dropStatement = "";

        for (int i = _objs[id].DefinitionSectionStart, loopTo = _objs[id].DefinitionSectionEnd; i <= loopTo; i++)
        {
            if (BeginsWith(_lines[i], "drop "))
            {
                dropStatement = GetEverythingAfter(_lines[i], "drop ");
                dropFound = true;
                break;
            }
        }

        await SetStringContents("quest.error.article", _objs[id].Article, ctx);

        if (!dropFound | BeginsWith(dropStatement, "everywhere"))
        {
            if (isInContainer)
            {
                // So, we want to drop an object that's in a container or surface. So first
                // we have to remove the object from that container.
                string parentDisplayName;

                if (!string.IsNullOrEmpty(_objs[parentId].ObjectAlias))
                {
                    parentDisplayName = _objs[parentId].ObjectAlias;
                }
                else
                {
                    parentDisplayName = _objs[parentId].ObjectName;
                }

                await Print("(first removing " + _objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                await ExecCommand("remove " + _objs[id].ObjectName, ctx, false, dontSetIt: true);

                if (!string.IsNullOrEmpty(GetObjectProperty("parent", id, false, false)))
                    // removing the object failed
                {
                    return;
                }
            }
        }

        if (!dropFound)
        {
            await PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            await PlayerItem(_objs[id].ObjectName, false, ctx);
        }
        else if (BeginsWith(dropStatement, "everywhere"))
        {
            await PlayerItem(_objs[id].ObjectName, false, ctx);
            if (Strings.InStr(dropStatement, "<") != 0)
            {
                await Print(await GetParameter(dropStatement, ctx), ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.DefaultDrop, ctx);
            }
        }
        else if (BeginsWith(dropStatement, "nowhere"))
        {
            if (Strings.InStr(dropStatement, "<") != 0)
            {
                await Print(await GetParameter(dropStatement, ctx), ctx);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.CantDrop, ctx);
            }
        }
        else
        {
            await ExecuteScript(dropStatement, ctx);
        }
    }

    private async Task ExecExamine(string command, Context ctx)
    {
        var item = Strings.LCase(Strings.Trim(GetEverythingAfter(command, "examine ")));

        if (string.IsNullOrEmpty(item))
        {
            await PlayerErrorMessage(PlayerError.BadExamine, ctx);
            _badCmdBefore = "examine";
            return;
        }

        var id = await Disambiguate(item, _currentRoom + ";inventory", ctx);

        if (id <= 0)
        {
            if (id != -2)
            {
                await PlayerErrorMessage(PlayerError.BadThing, ctx);
            }

            _badCmdBefore = "examine";
            return;
        }

        var o = _objs[id];

        // Find "examine" action:
        for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
        {
            if (o.Actions[i].ActionName == "examine")
            {
                await ExecuteScript(o.Actions[i].Script, ctx, id);
                return;
            }
        }

        // Find "examine" property:
        for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
        {
            if (o.Properties[i].PropertyName == "examine")
            {
                await Print(o.Properties[i].PropertyValue, ctx);
                return;
            }
        }

        // Find "examine" tag:
        for (int i = o.DefinitionSectionStart + 1, loopTo2 = _objs[id].DefinitionSectionEnd - 1; i <= loopTo2; i++)
        {
            if (BeginsWith(_lines[i], "examine "))
            {
                var action = Strings.Trim(GetEverythingAfter(_lines[i], "examine "));
                if (Strings.Left(action, 1) == "<")
                {
                    await Print(await GetParameter(_lines[i], ctx), ctx);
                }
                else
                {
                    await ExecuteScript(action, ctx, id);
                }

                return;
            }
        }

        await DoLook(id, ctx, true);
    }

    private async Task ExecGive(string giveString, Context ctx)
    {
        var article = "";
        string item, character;
        Thing type;
        var id = default(int);
        var script = "";
        var toPos = Strings.InStr(giveString, " to ");

        if (toPos == 0)
        {
            toPos = Strings.InStr(giveString, " the ");
            if (toPos == 0)
            {
                await PlayerErrorMessage(PlayerError.BadGive, ctx);
                return;
            }

            item = Strings.Trim(Strings.Mid(giveString, toPos + 4, Strings.Len(giveString) - (toPos + 2)));
            character = Strings.Trim(Strings.Mid(giveString, 1, toPos));
        }
        else
        {
            item = Strings.Trim(Strings.Mid(giveString, 1, toPos));
            character = Strings.Trim(Strings.Mid(giveString, toPos + 3, Strings.Len(giveString) - (toPos + 2)));
        }

        if (ASLVersion >= 281)
        {
            type = Thing.Object;
        }
        else
        {
            type = Thing.Character;
        }

        // First see if player has "ItemToGive":
        if (ASLVersion >= 280)
        {
            id = await Disambiguate(item, "inventory", ctx);

            if (id == -1)
            {
                await PlayerErrorMessage(PlayerError.NoItem, ctx);
                _badCmdBefore = "give";
                _badCmdAfter = "to " + character;
                return;
            }

            if (id == -2)
            {
                return;
            }

            article = _objs[id].Article;
        }
        else
        {
            // ASL2:
            var notGot = true;

            for (int i = 1, loopTo = _numberItems; i <= loopTo; i++)
            {
                if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(item) ?? ""))
                {
                    if (!_items[i].Got)
                    {
                        notGot = true;
                        i = _numberItems;
                    }
                    else
                    {
                        notGot = false;
                    }
                }
            }

            if (notGot)
            {
                await PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }

        if (ASLVersion >= 281)
        {
            var foundScript = false;
            var foundObject = false;

            var giveToId = await Disambiguate(character, _currentRoom, ctx);
            if (giveToId > 0)
            {
                foundObject = true;
            }

            if (!foundObject)
            {
                if (giveToId != -2)
                {
                    await PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                }

                _badCmdBefore = "give " + item + " to";
                return;
            }

            // Find appropriate give script ****
            // now, for "give a to b", we have
            // ItemID=a and GiveToObjectID=b

            var o = _objs[giveToId];

            for (int i = 1, loopTo1 = o.NumberGiveData; i <= loopTo1; i++)
            {
                if ((o.GiveData[i].GiveType == GiveType.GiveSomethingTo) &
                    ((Strings.LCase(o.GiveData[i].GiveObject) ?? "") == (Strings.LCase(_objs[id].ObjectName) ?? "")))
                {
                    foundScript = true;
                    script = o.GiveData[i].GiveScript;
                    break;
                }
            }

            if (!foundScript)
            {
                // check a for give to <b>:

                var g = _objs[id];

                for (int i = 1, loopTo2 = g.NumberGiveData; i <= loopTo2; i++)
                {
                    if ((g.GiveData[i].GiveType == GiveType.GiveToSomething) &
                        ((Strings.LCase(g.GiveData[i].GiveObject) ?? "") ==
                         (Strings.LCase(_objs[giveToId].ObjectName) ?? "")))
                    {
                        foundScript = true;
                        script = g.GiveData[i].GiveScript;
                        break;
                    }
                }
            }

            if (!foundScript)
            {
                // check b for give anything:
                script = _objs[giveToId].GiveAnything;
                if (!string.IsNullOrEmpty(script))
                {
                    foundScript = true;
                    await SetStringContents("quest.give.object.name", _objs[id].ObjectName, ctx);
                }
            }

            if (!foundScript)
            {
                // check a for give to anything:
                script = _objs[id].GiveToAnything;
                if (!string.IsNullOrEmpty(script))
                {
                    foundScript = true;
                    await SetStringContents("quest.give.object.name", _objs[giveToId].ObjectName, ctx);
                }
            }

            if (foundScript)
            {
                await ExecuteScript(script, ctx, id);
            }
            else
            {
                await SetStringContents("quest.error.charactername", _objs[giveToId].ObjectName, ctx);

                var gender = Strings.Trim(_objs[giveToId].Gender);
                gender = Strings.UCase(Strings.Left(gender, 1)) + Strings.Mid(gender, 2);
                await SetStringContents("quest.error.gender", gender, ctx);

                await SetStringContents("quest.error.article", article, ctx);
                await PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
            }
        }
        else
        {
            // ASL2:
            var block = GetThingBlock(character, _currentRoom, type);

            if (((block.StartLine == 0) & (block.EndLine == 0)) |
                !IsAvailable(character + "@" + _currentRoom, type, ctx))
            {
                await PlayerErrorMessage(PlayerError.BadCharacter, ctx);
                return;
            }

            var realName = _chars[GetThingNumber(character, _currentRoom, type)].ObjectName;

            // now, see if there's a give statement for this item in
            // this characters definition:

            var giveLine = 0;
            for (int i = block.StartLine + 1, loopTo3 = block.EndLine - 1; i <= loopTo3; i++)
            {
                if (BeginsWith(_lines[i], "give"))
                {
                    var ItemCheck = await GetParameter(_lines[i], ctx);
                    if ((Strings.LCase(ItemCheck) ?? "") == (Strings.LCase(item) ?? ""))
                    {
                        giveLine = i;
                    }
                }
            }

            if (giveLine == 0)
            {
                if (string.IsNullOrEmpty(article))
                {
                    article = "it";
                }

                await SetStringContents("quest.error.charactername", realName, ctx);
                await SetStringContents("quest.error.gender", Strings.Trim(await GetGender(character, true, ctx)), ctx);
                await SetStringContents("quest.error.article", article, ctx);
                await PlayerErrorMessage(PlayerError.ItemUnwanted, ctx);
                return;
            }

            // now, execute the statement on GiveLine
            await ExecuteScript(GetSecondChunk(_lines[giveLine]), ctx);
        }
    }

    private async Task ExecLook(string lookLine, Context ctx)
    {
        string item;

        if (Strings.Trim(lookLine) == "look")
        {
            await ShowRoomInfo(_currentRoom, ctx);
        }
        else
        {
            if (ASLVersion < 391)
            {
                var atPos = Strings.InStr(lookLine, " at ");

                if (atPos == 0)
                {
                    item = GetEverythingAfter(lookLine, "look ");
                }
                else
                {
                    item = Strings.Trim(Strings.Mid(lookLine, atPos + 4));
                }
            }
            else if (BeginsWith(lookLine, "look at "))
            {
                item = GetEverythingAfter(lookLine, "look at ");
            }
            else if (BeginsWith(lookLine, "look in "))
            {
                item = GetEverythingAfter(lookLine, "look in ");
            }
            else if (BeginsWith(lookLine, "look on "))
            {
                item = GetEverythingAfter(lookLine, "look on ");
            }
            else if (BeginsWith(lookLine, "look inside "))
            {
                item = GetEverythingAfter(lookLine, "look inside ");
            }
            else
            {
                item = GetEverythingAfter(lookLine, "look ");
            }

            if (ASLVersion >= 280)
            {
                var id = await Disambiguate(item, "inventory;" + _currentRoom, ctx);

                if (id <= 0)
                {
                    if (id != -2)
                    {
                        await PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }

                    _badCmdBefore = "look at";
                    return;
                }

                await DoLook(id, ctx);
            }
            else
            {
                if (BeginsWith(item, "the "))
                {
                    item = GetEverythingAfter(item, "the ");
                }

                lookLine = RetrLine("object", item, "look", ctx);

                if (lookLine != "<unfound>")
                {
                    // Check for availability
                    if (!IsAvailable(item, Thing.Object, ctx))
                    {
                        lookLine = "<unfound>";
                    }
                }

                if (lookLine == "<unfound>")
                {
                    lookLine = RetrLine("character", item, "look", ctx);

                    if (lookLine != "<unfound>")
                    {
                        if (!IsAvailable(item, Thing.Character, ctx))
                        {
                            lookLine = "<unfound>";
                        }
                    }

                    if (lookLine == "<unfound>")
                    {
                        await PlayerErrorMessage(PlayerError.BadThing, ctx);
                        return;
                    }

                    if (lookLine == "<undefined>")
                    {
                        await PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                        return;
                    }
                }
                else if (lookLine == "<undefined>")
                {
                    await PlayerErrorMessage(PlayerError.DefaultLook, ctx);
                    return;
                }

                var lookData = Strings.Trim(GetEverythingAfter(Strings.Trim(lookLine), "look "));
                if (Strings.Left(lookData, 1) == "<")
                {
                    var LookText = await GetParameter(lookLine, ctx);
                    await Print(LookText, ctx);
                }
                else
                {
                    await ExecuteScript(lookData, ctx);
                }
            }
        }
    }

    private async Task ExecSpeak(string cmd, Context ctx)
    {
        if (BeginsWith(cmd, "the "))
        {
            cmd = GetEverythingAfter(cmd, "the ");
        }

        var name = cmd;

        // if the "speak" parameter of the character c$'s definition
        // is just a parameter, say it - otherwise execute it as
        // a script.

        if (ASLVersion >= 281)
        {
            var speakLine = "";

            var ObjID = await Disambiguate(name, "inventory;" + _currentRoom, ctx);
            if (ObjID <= 0)
            {
                if (ObjID != -2)
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                }

                _badCmdBefore = "speak to";
                return;
            }

            var foundSpeak = false;

            // First look for action, then look
            // for property, then check define
            // section:

            var o = _objs[ObjID];

            for (int i = 1, loopTo = o.NumberActions; i <= loopTo; i++)
            {
                if (o.Actions[i].ActionName == "speak")
                {
                    speakLine = "speak " + o.Actions[i].Script;
                    foundSpeak = true;
                    break;
                }
            }

            if (!foundSpeak)
            {
                for (int i = 1, loopTo1 = o.NumberProperties; i <= loopTo1; i++)
                {
                    if (o.Properties[i].PropertyName == "speak")
                    {
                        speakLine = "speak <" + o.Properties[i].PropertyValue + ">";
                        foundSpeak = true;
                        break;
                    }
                }
            }

            // For some reason ASL3 < 311 looks for a "look" tag rather than
            // having had this set up at initialisation.
            if ((ASLVersion < 311) & !foundSpeak)
            {
                for (int i = o.DefinitionSectionStart, loopTo2 = o.DefinitionSectionEnd; i <= loopTo2; i++)
                {
                    if (BeginsWith(_lines[i], "speak "))
                    {
                        speakLine = _lines[i];
                        foundSpeak = true;
                    }
                }
            }

            if (!foundSpeak)
            {
                await SetStringContents("quest.error.gender",
                    Strings.UCase(Strings.Left(_objs[ObjID].Gender, 1)) + Strings.Mid(_objs[ObjID].Gender, 2), ctx);
                await PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
                return;
            }

            speakLine = GetEverythingAfter(speakLine, "speak ");

            if (BeginsWith(speakLine, "<"))
            {
                var text = await GetParameter(speakLine, ctx);
                if (ASLVersion >= 350)
                {
                    await Print(text, ctx);
                }
                else
                {
                    await Print('"' + text + '"', ctx);
                }
            }
            else
            {
                await ExecuteScript(speakLine, ctx, ObjID);
            }
        }

        else
        {
            var line = RetrLine("character", cmd, "speak", ctx);
            var type = Thing.Character;

            var data = Strings.Trim(GetEverythingAfter(Strings.Trim(line), "speak "));

            if ((line != "<unfound>") & (line != "<undefined>"))
            {
                // Character exists; but is it available??
                if (!IsAvailable(cmd + "@" + _currentRoom, type, ctx))
                {
                    line = "<undefined>";
                }
            }

            if (line == "<undefined>")
            {
                await PlayerErrorMessage(PlayerError.BadCharacter, ctx);
            }
            else if (line == "<unfound>")
            {
                await SetStringContents("quest.error.gender", Strings.Trim(await GetGender(cmd, true, ctx)), ctx);
                await SetStringContents("quest.error.charactername", cmd, ctx);
                await PlayerErrorMessage(PlayerError.DefaultSpeak, ctx);
            }
            else if (BeginsWith(data, "<"))
            {
                data = await GetParameter(line, ctx);
                await Print('"' + data + '"', ctx);
            }
            else
            {
                await ExecuteScript(data, ctx);
            }
        }
    }

    private async Task ExecTake(string item, Context ctx)
    {
        var parentID = default(int);
        string parentDisplayName;
        var foundItem = true;
        var foundTake = false;
        var id = await Disambiguate(item, _currentRoom, ctx);

        if (id < 0)
        {
            foundItem = false;
        }
        else
        {
            foundItem = true;
        }

        if (!foundItem)
        {
            if (id != -2)
            {
                if (ASLVersion >= 410)
                {
                    id = await Disambiguate(item, "inventory", ctx);
                    if (id >= 0)
                    {
                        // Player already has this item
                        await PlayerErrorMessage(PlayerError.AlreadyTaken, ctx);
                    }
                    else
                    {
                        await PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }
                }
                else if (ASLVersion >= 391)
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                }
                else
                {
                    await PlayerErrorMessage(PlayerError.BadItem, ctx);
                }
            }

            _badCmdBefore = "take";
            return;
        }

        await SetStringContents("quest.error.article", _objs[id].Article, ctx);

        var isInContainer = false;

        if (ASLVersion >= 391)
        {
            var canAccessObject = PlayerCanAccessObject(id);
            if (!canAccessObject.CanAccessObject)
            {
                await PlayerErrorMessage_ExtendInfo(PlayerError.BadTake, ctx, canAccessObject.ErrorMsg);
                return;
            }

            isInContainer = IsYes(GetObjectProperty("parent", id, true, false));
            var parent = GetObjectProperty("parent", id, false, false);
            parentID = GetObjectIdNoAlias(parent);
        }

        if (ASLVersion >= 280)
        {
            var t = _objs[id].Take;

            if (isInContainer & ((t.Type == TextActionType.Default) | (t.Type == TextActionType.Text)))
            {
                // So, we want to take an object that's in a container or surface. So first
                // we have to remove the object from that container.

                if (!string.IsNullOrEmpty(_objs[parentID].ObjectAlias))
                {
                    parentDisplayName = _objs[parentID].ObjectAlias;
                }
                else
                {
                    parentDisplayName = _objs[parentID].ObjectName;
                }

                await Print("(first removing " + _objs[id].Article + " from " + parentDisplayName + ")", ctx);

                // Try to remove the object
                ctx.AllowRealNamesInCommand = true;
                await ExecCommand("remove " + _objs[id].ObjectName, ctx, false, dontSetIt: true);

                if (!string.IsNullOrEmpty(GetObjectProperty("parent", id, false, false)))
                {
                    // removing the object failed
                    return;
                }
            }

            if (t.Type == TextActionType.Default)
            {
                await PlayerErrorMessage(PlayerError.DefaultTake, ctx);
                await PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Text)
            {
                await Print(t.Data, ctx);
                await PlayerItem(item, true, ctx, id);
            }
            else if (t.Type == TextActionType.Script)
            {
                await ExecuteScript(t.Data, ctx, id);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
        else
        {
            // find 'take' line
            for (int i = _objs[id].DefinitionSectionStart + 1, loopTo = _objs[id].DefinitionSectionEnd - 1;
                 i <= loopTo;
                 i++)
            {
                if (BeginsWith(_lines[i], "take"))
                {
                    var script = Strings.Trim(GetEverythingAfter(Strings.Trim(_lines[i]), "take"));
                    await ExecuteScript(script, ctx, id);
                    foundTake = true;
                    i = _objs[id].DefinitionSectionEnd;
                }
            }

            if (!foundTake)
            {
                await PlayerErrorMessage(PlayerError.BadTake, ctx);
            }
        }
    }

    private async Task ExecUse(string useLine, Context ctx)
    {
        int endOnWith;
        var useDeclareLine = "";
        string useOn, useItem;

        useLine = Strings.Trim(GetEverythingAfter(useLine, "use "));

        int roomId;
        roomId = GetRoomID(_currentRoom, ctx);

        var onWithPos = Strings.InStr(useLine, " on ");
        if (onWithPos == 0)
        {
            onWithPos = Strings.InStr(useLine, " with ");
            endOnWith = onWithPos + 4;
        }
        else
        {
            endOnWith = onWithPos + 2;
        }

        if (onWithPos != 0)
        {
            useOn = Strings.Trim(Strings.Right(useLine, Strings.Len(useLine) - endOnWith));
            useItem = Strings.Trim(Strings.Left(useLine, onWithPos - 1));
        }
        else
        {
            useOn = "";
            useItem = useLine;
        }

        // see if player has this item:

        var id = default(int);
        bool notGotItem;
        if (ASLVersion >= 280)
        {
            var foundItem = false;

            id = await Disambiguate(useItem, "inventory", ctx);
            if (id > 0)
            {
                foundItem = true;
            }

            if (!foundItem)
            {
                if (id != -2)
                {
                    await PlayerErrorMessage(PlayerError.NoItem, ctx);
                }

                if (string.IsNullOrEmpty(useOn))
                {
                    _badCmdBefore = "use";
                }
                else
                {
                    _badCmdBefore = "use";
                    _badCmdAfter = "on " + useOn;
                }

                return;
            }
        }
        else
        {
            notGotItem = true;

            for (int i = 1, loopTo = _numberItems; i <= loopTo; i++)
            {
                if ((Strings.LCase(_items[i].Name) ?? "") == (Strings.LCase(useItem) ?? ""))
                {
                    if (!_items[i].Got)
                    {
                        notGotItem = true;
                        i = _numberItems;
                    }
                    else
                    {
                        notGotItem = false;
                    }
                }
            }

            if (notGotItem)
            {
                await PlayerErrorMessage(PlayerError.NoItem, ctx);
                return;
            }
        }

        var useScript = "";
        bool foundUseScript;
        bool foundUseOnObject;
        int useOnObjectId;
        bool found;
        if (ASLVersion >= 280)
        {
            foundUseScript = false;

            if (string.IsNullOrEmpty(useOn))
            {
                if (ASLVersion < 410)
                {
                    var r = _rooms[roomId];
                    for (int i = 1, loopTo1 = r.NumberUse; i <= loopTo1; i++)
                    {
                        if ((Strings.LCase(_objs[id].ObjectName) ?? "") == (Strings.LCase(r.Use[i].Text) ?? ""))
                        {
                            foundUseScript = true;
                            useScript = r.Use[i].Script;
                            break;
                        }
                    }
                }

                if (!foundUseScript)
                {
                    useScript = _objs[id].Use;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                    }
                }
            }
            else
            {
                foundUseOnObject = false;

                useOnObjectId = await Disambiguate(useOn, _currentRoom, ctx);
                if (useOnObjectId > 0)
                {
                    foundUseOnObject = true;
                }
                else
                {
                    useOnObjectId = await Disambiguate(useOn, "inventory", ctx);
                    if (useOnObjectId > 0)
                    {
                        foundUseOnObject = true;
                    }
                }

                if (!foundUseOnObject)
                {
                    if (useOnObjectId != -2)
                    {
                        await PlayerErrorMessage(PlayerError.BadThing, ctx);
                    }

                    _badCmdBefore = "use " + useItem + " on";
                    return;
                }

                // now, for "use a on b", we have
                // ItemID=a and UseOnObjectID=b

                // first check b for use <a>:

                var o = _objs[useOnObjectId];

                for (int i = 1, loopTo2 = o.NumberUseData; i <= loopTo2; i++)
                {
                    if ((o.UseData[i].UseType == UseType.UseSomethingOn) &
                        ((Strings.LCase(o.UseData[i].UseObject) ?? "") == (Strings.LCase(_objs[id].ObjectName) ?? "")))
                    {
                        foundUseScript = true;
                        useScript = o.UseData[i].UseScript;
                        break;
                    }
                }

                if (!foundUseScript)
                {
                    // check a for use on <b>:
                    var u = _objs[id];
                    for (int i = 1, loopTo3 = u.NumberUseData; i <= loopTo3; i++)
                    {
                        if ((u.UseData[i].UseType == UseType.UseOnSomething) &
                            ((Strings.LCase(u.UseData[i].UseObject) ?? "") ==
                             (Strings.LCase(_objs[useOnObjectId].ObjectName) ?? "")))
                        {
                            foundUseScript = true;
                            useScript = u.UseData[i].UseScript;
                            break;
                        }
                    }
                }

                if (!foundUseScript)
                {
                    // check b for use anything:
                    useScript = _objs[useOnObjectId].UseAnything;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                        await SetStringContents("quest.use.object.name", _objs[id].ObjectName, ctx);
                    }
                }

                if (!foundUseScript)
                {
                    // check a for use on anything:
                    useScript = _objs[id].UseOnAnything;
                    if (!string.IsNullOrEmpty(useScript))
                    {
                        foundUseScript = true;
                        await SetStringContents("quest.use.object.name", _objs[useOnObjectId].ObjectName, ctx);
                    }
                }
            }

            if (foundUseScript)
            {
                await ExecuteScript(useScript, ctx, id);
            }
            else
            {
                await PlayerErrorMessage(PlayerError.DefaultUse, ctx);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(useOn))
            {
                useDeclareLine = await RetrLineParam("object", useOn, "use", useItem, ctx);
            }
            else
            {
                found = false;
                for (int i = 1, loopTo4 = _rooms[roomId].NumberUse; i <= loopTo4; i++)
                {
                    if ((Strings.LCase(_rooms[roomId].Use[i].Text) ?? "") == (Strings.LCase(useItem) ?? ""))
                    {
                        useDeclareLine = "use <> " + _rooms[roomId].Use[i].Script;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    useDeclareLine = await FindLine(GetDefineBlock("game"), "use", useItem);
                }

                if (!found & string.IsNullOrEmpty(useDeclareLine))
                {
                    await PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }

            if ((useDeclareLine != "<unfound>") & (useDeclareLine != "<undefined>") & !string.IsNullOrEmpty(useOn))
            {
                // Check for object availablity
                if (!IsAvailable(useOn, Thing.Object, ctx))
                {
                    useDeclareLine = "<undefined>";
                }
            }

            if (useDeclareLine == "<undefined>")
            {
                useDeclareLine = await RetrLineParam("character", useOn, "use", useItem, ctx);

                if (useDeclareLine != "<undefined>")
                {
                    // Check for character availability
                    if (!IsAvailable(useOn, Thing.Character, ctx))
                    {
                        useDeclareLine = "<undefined>";
                    }
                }

                if (useDeclareLine == "<undefined>")
                {
                    await PlayerErrorMessage(PlayerError.BadThing, ctx);
                    return;
                }

                if (useDeclareLine == "<unfound>")
                {
                    await PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                    return;
                }
            }
            else if (useDeclareLine == "<unfound>")
            {
                await PlayerErrorMessage(PlayerError.DefaultUse, ctx);
                return;
            }

            var script = Strings.Right(useDeclareLine,
                Strings.Len(useDeclareLine) - Strings.InStr(useDeclareLine, ">"));
            await ExecuteScript(script, ctx);
        }
    }
}

namespace QuestViva.Cli;

// The MCP server's instructions, which clients pass to the agent along with the tools. Short, so
// agents read them: how to play, and with --debug-tools, how to fix a game, with enough ASL to
// start and a pointer to docs for the rest.
internal static class McpInstructions
{
    private const string Play = """
        qv plays Quest text adventure games. Call start_game to begin, then send_command with what a player would type: a command such as "look" or "take lamp", or the answer to a menu or yes/no question the game is waiting on. Each response is the game's own text. Lines in square brackets aren't part of it: [Exits: ...] and [Status: ...] show what a browser player sees beside the text, and others say what the game is waiting for.
        """;

    private const string Debug = """


        The debug tools reach past what a player can do: evaluate reads the game's state, tick and send_event drive timers and JavaScript events, run_walkthrough runs a walkthrough saved in the game, check finds problems in the game's file, list_functions and describe_function show the functions it can call, and docs looks things up in Quest's documentation.

        To fix or change a game, edit its .aslx file, which is XML, with your own file tools (a published .quest file can't be edited). Then call check, which reads the file as it is now, and start_game, which restarts the game from the file so you can play your change. check, run_walkthrough, list_functions and describe_function also take the game's path, so you can use them before starting it. To test a fix, add a walkthrough that reproduces the problem and ends in an assert:<expression> step, and run it with run_walkthrough to see it fail before you fix the game.

        Scripts are ASL, Quest's own language, not JavaScript or C#:
        - One command per line, no semicolons. Blocks use braces: if (x > 1) { ... } else if (x = 0) { ... } else { ... }, foreach (item, list) { ... }, for (i, 1, 10) { ... }, while (x < 5) { ... }, switch (x) { case (1, 2) { ... } default { ... } }.
        - = is both assignment and comparison, <> is not equal, and the logical operators are and, or and not. + joins strings.
        - object.attribute reads and sets an attribute, and this is the object a script belongs to. Local variables need no declaration: x = 3.
        - msg ("text") prints. A function is called as a statement, DoThing (a, b), or in an expression, x = GetDisplayAlias(lamp). The game's own functions are <function> elements in its file.
        Look up a command or function with docs before using it, for example docs with query "foreach" or "GetBoolean", and run check after every edit.
        """;

    public static string For(bool debugTools) => debugTools ? Play + Debug : Play;
}

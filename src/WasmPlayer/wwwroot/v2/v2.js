// Placeholder for player UI v2 (#2595). It implements the player-UI adapter (see
// legacy/adapter.js for what each member is for) just far enough to show a game's text and send
// commands, so the boot-time UI selection (#2593) can be tested with ?ui=v2. Everything else is
// accepted and ignored.

(function () {
    "use strict";

    const output = () => document.getElementById("qv-v2-output");

    function addText(html) {
        output()?.insertAdjacentHTML("beforeend", html);
    }

    function ignore() {}

    // Accepts any QuestUI call (docs/ui-v2-contract.md), including the map and transcript
    // namespaces, so a 610 game runs; only output does anything yet.
    function acceptAnything(members) {
        return new Proxy(members, {
            get: (target, name) => (name in target ? target[name] : acceptAnything(function () {})),
        });
    }
    window.QuestUI = acceptAnything({
        addText,
        clearScreen: () => { output().innerHTML = ""; },
    });
    window.ASLEvent = (name, param) => WebPlayer.uiSendEvent(name, "" + param);

    window.QuestPlayerUis = window.QuestPlayerUis || {};
    window.QuestPlayerUis.v2 = {
        addTextAndScroll: addText,
        createNewDiv: ignore,
        bindMenu: ignore,
        showMenu: ignore,
        showQuestion: ignore,
        beginWait: ignore,
        setTurnPending: ignore,
        beginPause: ignore,
        updateLocation: ignore,
        setGameName: (name) => { document.title = name; },
        clearScreen: () => { output().innerHTML = ""; },
        panesVisible: ignore,
        updateStatus: ignore,
        setBackground: ignore,
        setForeground: ignore,
        updateList: ignore,
        updateCompass: ignore,
        updateObjectLinks: ignore,
        gameFinished: ignore,
        requestNextTimerTick: ignore,
        uiShow: ignore,
        uiHide: ignore,
        setCompassDirections: ignore,
        setInterfaceString: ignore,
        setPanelContents: ignore,
        loadHtml: (html) => { output().innerHTML = html; },
        markScrollPosition: ignore,
        scrollToEnd: ignore,
        requestSave: () => GameSaver.save(),

        async start() {
            const form = document.getElementById("qv-v2-command");
            const input = document.getElementById("qv-v2-input");
            form.addEventListener("submit", async (e) => {
                e.preventDefault();
                const command = input.value;
                input.value = "";
                window.canSendCommand = false;
                await WebPlayer.sendCommand(command, 0, null);
            });
            input.focus();
        },
        resetOutput: () => { output().innerHTML = ""; },

        getOutputHtml: () => output().innerHTML,
        gameSaved: () => addText("Game saved.<br>"),
        clearUnsavedProgress: ignore,
        setCanSave: ignore,

        waitEnded: ignore,
        pauseEnded: ignore,
        beginSoundWait: () => false,
        endSoundWait: () => WebPlayer.uiEndWait(),
        rememberImageSize: ignore,
        setAnimateScroll: ignore,
    };
})();

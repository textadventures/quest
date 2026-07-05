---
title: Introduction
sidebar:
  order: 1
---

:::caution
Quest Viva is currently in development.

What you **can** do: You can use Quest Viva to host Quest games on any website.

What you **can't** (yet) do: The Editor is still under construction. So if you are looking to create a game, [please use Quest 5](https://textadventures.co.uk/quest) for now.
:::

## What is Quest Viva?

Quest Viva is the "next version" of [Quest 5](https://textadventures.co.uk/quest).

Quest 5 has been around since 2011, and has a number of limitations. It was written in .NET Framework 4, meaning it only ran offline on Windows. For online play in a web browser, it required a Windows Server to be running. The game playing and editing experiences differed between desktop and browser versions. Online players sometimes found games ran slowly, and they could be kicked out of their session if there were network issues.

Quest Viva addresses all of these limitations. It runs using the latest cross-platform version of .NET, meaning it works on Windows, Mac and Linux. The online player is now compiled to WASM, meaning all the code needed for a game runs directly in the browser - once the game has started, there's no dependency on a server connection. This means games run faster, any number of players can play at the same time, and the game continues to run even if the network connection is lost.

## Why not call it Quest 6?

Because there's [another system](https://github.com/ThePix/QuestJS) already called that (although that is more commonly referred to as QuestJS).

## Sounds great, how do I get started?

If you want to create a game, you will still need to use [Quest 5](https://textadventures.co.uk/quest) for now.

If you've already created a game with Quest 5, then great - you can now host your game on any website. See [Hosting your game](/guides/hosting/).
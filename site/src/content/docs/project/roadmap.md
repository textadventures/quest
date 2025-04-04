---
title: Roadmap
---

# Roadmap

Here's a rough roadmap with my current ideas for major Quest Viva projects. The version numbers are notional - I may change my mind about the numbering, but I think the sequence of projects is broadly correct.

## Quest Viva 6.0 - WebPlayer

This is well under way. Soon, we'll launch this as the way the "Play online" feature works for games on textadventures.co.uk, and turn off the "old" WebPlayer from Quest 5.

## Quest Viva 7.0 - WasmPlayer

This will allow games to run entirely within the browser, _without_ a server.

In principle this would "simply" be running the WebPlayer Blazor app via WASM, but Quest Viva currently uses threads, which Blazor WASM does not (yet) support.

Even if Blazor WASM does gain threading support, it's not great that the code works this way, so we need to move the code over to using `async` instead.

That's quite a big change, and will require a change to the expression evaluator, so we can deal with async functions.

I plan to switch Quest over to using [NCalc](https://github.com/ncalc/ncalc), which does have an async version. That's quite a big change in its own right, which will need a bunch of testing.

Once that's in place, we can start implementing `async` everywhere, and remove the threading.

We will do all of this in WebPlayer. Once things are working there, _then_ the "simple" switch over to using Blazor WASM should work. Should!

## Quest Viva 8.0 - WebEditor

Next we'll want to have a way for people to create games, so we'll need to write a new version of WebEditor which can run in Blazor on .NET 9 (or whatever version of .NET is the latest by the time we get there).
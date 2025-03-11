---
title: WebPlayer
---

Quest Viva's **WebPlayer** lets you serve any Quest game to a web browser. The game code itself runs on the server, with the web browser sending the player's input and receiving the game's output.

There are two ways of running WebPlayer:
- run using Docker
- build from the Quest Viva source code

## Run WebPlayer using Docker

First, install and run [Docker](https://www.docker.com/). Then, you can get a basic instance of WebPlayer up and running using:

```
docker run -p 8080:8080 ghcr.io/textadventures/quest-viva-webplayer:latest
```

WebPlayer will now be running at <http://localhost:8080>

While Quest Viva is under active development, you may want to pass the `--pull always` flag so that Docker always fetches the latest version, instead of using a cached one:

```
docker run -p 8080:8080 --pull always ghcr.io/textadventures/quest-viva-webplayer:latest
```

You may also want to fetch a specific version:

```
docker run -p 8080:8080 --pull always ghcr.io/textadventures/quest-viva-webplayer:6.0.0-alpha.12
```

You can see the available image versions on the [`quest-viva-webplayer` package page](https://github.com/textadventures/quest/pkgs/container/quest-viva-webplayer).

### `compose.yaml` file

For easier configuration, you can create a `compose.yaml` file:

```
services:
  webplayer:
    image: ghcr.io/textadventures/quest-viva-webplayer:latest
    ports:
      - "8080:8080"
```

You can then run this using:

```
docker compose up
```

This also accepts the `--pull always` flag, if you want to ensure you're running the very latest version:

```
docker compose up --pull always
```

## Build from source

As an alternative to Docker, you can build and run directly on your machine.

First, you will need to download and install the [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download).

Next, use `git` to clone the Quest Viva source code from <https://github.com/textadventures/quest>

Then you can run using:

```
dotnet run --project src/WebPlayer/WebPlayer.csproj
```

You should then have WebPlayer running, by default at <http://localhost:5052>

## Configuration

You can choose a game to serve on the home page. The way to set this configuration varies depending on whether you're running via Docker, or building from source.

### Docker configuration

For Docker, you need to specify a `Home__File` environment variable to point to the game, _and_ that game needs to be available in the Docker environment. To do that, you need to set up a Docker volume.

Here's an example that serves `Moquette.quest` which is stored in `~/Downloads`:

```
services:
  webplayer:
    image: ghcr.io/textadventures/quest-viva-webplayer:latest
    ports:
      - "8080:8080"
    environment:
      Home__File: "/data/Moquette.quest"
    volumes:
      - "~/Downloads/Moquette.quest:/data/Moquette.quest:ro"
```

### Source configuration

If you're building from source, you just need to update `appsettings.json`, like this:

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Home": {
    "File": "/Users/alexwarren/Downloads/Moquette.quest"
  }
}

```
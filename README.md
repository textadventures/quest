Quest Viva
==========

Quest Viva is an open-source system for creating and playing text adventure games. Designed for accessibility and flexibility, Quest Viva makes it easy for anyone to create a game without needing any previous experience of programming. For those who want more control, Quest Viva also offers powerful scripting capabilities.

Formerly known as Quest 5, Quest Viva is a modern cross-platform update, currently in development.

## Features

- **User-Friendly Interface** - Create text adventures with an intuitive editor, no coding required.
- **Powerful Scripting** - Extend your games with custom logic using Quest's scripting language.
- **Cross-Platform** - Games can be played in a web browser, or offline on Windows, Mac and Linux.
- **Multimedia Support** - Add images and sounds to your games, run JavaScript, and embed videos.
- **Extensible and Open-Source** - Modify and expand Quest to suit your needs.
- **Multiple languages** - Quest Viva supports creating games in English, French, German, Spanish, Dutch, Portuguese and more.

## Getting started

**⚠️⚠️⚠️ If you just want to create a game, then [please use Quest 5 for now](https://textadventures.co.uk/quest) ⚠️⚠️⚠️**

### For Developers testing and contributing to Quest Viva

- [Install Docker](https://www.docker.com/)
- Clone this GitHub repository
- Run `docker compose up --build`

## Community and Support

- [Documentation](https://docs.textadventures.co.uk/quest)
- For help, [join us in Discord](https://textadventures.co.uk/community/discord) or post in [Quest discussions](https://github.com/textadventures/quest/discussions)

---

## Sample compose.yml

```
services:
  webplayer:
    image: webplayer
    build:
      context: .
      dockerfile: src/WebPlayer/Dockerfile
    ports:
      - "8080:8080"
    environment:
      Home__File: "/data/game.quest"
    volumes:
      - "/path/to/game.quest:/data/game.quest:ro"

```
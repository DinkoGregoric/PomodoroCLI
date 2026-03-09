# Pomodoro

A terminal-based Pomodoro timer built in .NET.

## Installation

Download the latest binary for your platform from the [Releases](../../releases/latest) page.

| Platform | File |
|----------|------|
| Windows  | `pomodoro-win-x64.exe` |
| Linux    | `pomodoro-linux-x64` |
| macOS (Intel) | `pomodoro-osx-x64` |
| macOS (Apple Silicon) | `pomodoro-osx-arm64` |

No .NET runtime required — binaries are self-contained.

**macOS / Linux:** mark the binary as executable after downloading:
```bash
chmod +x pomodoro-osx-arm64
```

## What it does

Runs Pomodoro sessions from your terminal. The classic workflow: 25 minutes of work, short break, repeat, long break every few sessions. Everything is configurable if the defaults don't suit you.

The settings menu lets you tweak:

- **Timing** — work duration, short/long break lengths, how often you get a long break, and a max pause time (after which the session resets so you can't leave it paused indefinitely)
- **Notifications** — sound alert when a phase ends
- **Diagnostics** — event logging, mostly for debugging
- **Progression** - not implemented yet, but eventually the idea is to optionally ramp up work durations over time, so you gradually build toward a longer focus target rather than jumping straight to it

Settings persist between sessions (`%APPDATA%/Pomodoro/settings.json` on Windows, `~/.config/Pomodoro/settings.json` on macOS/Linux).

## Controls

| Key | Action |
|-----|--------|
| `S` | Start / Pause / Resume |
| `K` | Skip current phase |
| `T` | Reset session |
| `Q` | Quit |

## Project structure

Three projects:

```
Pomodoro.Core           Domain logic — state machine, settings models, CQRS commands
Pomodoro.Infrastructure Settings persistence (JSON file, thread-safe, self-healing)
Pomodoro.CLI            Terminal UI built with Spectre.Console
```

## Tech

- .NET 10
- [Spectre.Console](https://spectreconsole.net/) for the terminal UI

## License

[MIT](LICENSE)

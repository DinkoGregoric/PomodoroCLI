# Pomodoro

A terminal-based Pomodoro timer built in .NET. Nothing fancy in terms of UI — just a clean CLI that gets out of the way and lets you focus.

## What it does

Runs Pomodoro sessions from your terminal. The classic workflow: 25 minutes of work, short break, repeat, long break every few sessions. Everything is configurable if the defaults don't suit you.

The settings menu lets you tweak:

- **Timing** — work duration, short/long break lengths, how often you get a long break, and a max pause time (after which the session resets so you can't just leave it paused indefinitely)
- **Progression** — optionally ramp up work durations over time, so you gradually build toward a longer focus target rather than jumping straight to it
- **Notifications** — sound alerts with volume control and a few sound options
- **Diagnostics** — event logging, mostly for debugging

Settings persist to `%APPDATA%/Pomodoro/settings.json` between sessions.

## Project structure

Three projects following a clean layered architecture:

```
Pomodoro.Core           Domain logic — state machine, settings models, CQRS commands
Pomodoro.Infrastructure Settings persistence (JSON file, thread-safe, self-healing)
Pomodoro.CLI            Terminal UI built with Spectre.Console
```

The core state machine handles phase transitions (Idle → Work → Break → Work...), pause/resume with time tracking, and auto-progression based on completed sessions. The infrastructure layer is deliberately simple — just a JSON file with some resilience around corruption and missing files.

## Status

The settings UI and underlying engine are complete. The actual session timer loop (the part that counts down and advances phases automatically) is still being wired up. The architecture is in place — it's just a matter of hooking the `PomodoroStateMachine` into a live UI loop.

## Tech

- .NET 10
- [Spectre.Console](https://spectreconsole.net/) for the terminal UI

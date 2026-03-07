using Pomodoro.CLI.UseCases.Pomodoro;
using Pomodoro.Core.Commands;
using Pomodoro.Core.Domain;
using Pomodoro.Core.Engine;
using Pomodoro.Core.Events;
using Pomodoro.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Pomodoro.CLI.Views
{
    internal class PomodoroView(
        TimeProvider timeProvider,
        PomodoroStateMachine stateMachine,
        ICommandDispatcher dispatcher,
        StartUseCase start,
        PauseUseCase pause,
        ResumeUseCase resume,
        ResetUseCase reset,
        SkipUseCase skip)
    {
        private static readonly string[] SpinnerFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];
        private readonly PomodoroState state = stateMachine.State;
        private int _spinnerIndex = 0;
        private string? _sessionExpiredMessage = null;

        internal async Task Display()
        {
            AnsiConsole.Write(new Markup("[bold]Controls:[/] [[S]] Start/Pause/Resume  [[K]] Skip  [[T]] Reset  [[Q]] Quit\n", Styles.Default));
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Q)
            {
                return;
            }

            if (key.Key == ConsoleKey.S)
            {
                await start.ExecuteAsync();

                stateMachine.SessionExpiredDueToPauseTimeout += OnSessionExpired;
                try
                {
                    await AnsiConsole.Live(BuildDisplay())
                        .StartAsync(async ctx =>
                        {
                            var quit = false;

                            while (!quit)
                            {
                                if (Console.KeyAvailable)
                                {
                                    switch (Console.ReadKey(intercept: true).Key)
                                    {
                                        case ConsoleKey.S:
                                            _sessionExpiredMessage = null;
                                            if (state.CurrentPhase == Phase.Idle || !state.PhaseStartTimeUtc.HasValue)
                                                await start.ExecuteAsync();
                                            else if (state.IsRunning)
                                                await pause.ExecuteAsync();
                                            else
                                                await resume.ExecuteAsync();
                                            break;
                                        case ConsoleKey.K:
                                            await skip.ExecuteAsync();
                                            break;
                                        case ConsoleKey.T:
                                            await reset.ExecuteAsync();
                                            break;
                                        case ConsoleKey.Q:
                                            await reset.ExecuteAsync();
                                            quit = true;
                                            continue;
                                    }
                                }

                                await dispatcher.DispatchAsync(new AdvanceTimeCommand());
                                ctx.UpdateTarget(BuildDisplay());

                                await Task.Delay(250);
                            }
                        });
                }
                finally
                {
                    stateMachine.SessionExpiredDueToPauseTimeout -= OnSessionExpired;
                }
            }
        }

        private void OnSessionExpired(object? sender, SessionExpiredEventArgs e)
        {
            _sessionExpiredMessage = $"[red]Session reset: {e.Phase} phase paused longer than the allowed {e.MaxAllowedPauseDuration} minutes.[/]";
        }

        private IRenderable BuildDisplay()
        {
            var progress = ComputeProgress();
            var remaining = ComputeRemaining();

            var barWidth = 40;
            var filled = (int)(barWidth * progress);
            var spinner = state.IsRunning ? $"[green]{SpinnerFrames[_spinnerIndex++ % SpinnerFrames.Length]}[/] " : string.Empty;
            var bar = $"{spinner}[green]{new string('█', filled)}[/][grey]{new string('░', barWidth - filled)}[/]";

            string statusLine;
            if (state.CurrentPhase == Phase.Idle || !state.PhaseStartTimeUtc.HasValue)
            {
                statusLine = "[dim][[S]] Start [[Q]] Quit[/]";
            }
            else if (state.PausedAtUtc.HasValue)
            {
                statusLine = "[yellow]Paused[/]  [dim][[S]] Resume  [[K]] Skip  [[T]] Reset  [[Q]] Quit[/]";
            }
            else
            {
                statusLine = "[green]Running[/]  [dim][[S]] Pause  [[K]] Skip  [[T]] Reset  [[Q]] Quit[/]";
            }

            var rows = new List<IRenderable>
            {
                new Markup($"[bold]{state.CurrentPhase}[/]"),
                new Markup($"{bar} [bold]{progress:P0}[/]"),
                new Markup($"Remaining: [bold]{remaining:mm\\:ss}[/]"),
                new Rule(),
                new Markup(statusLine)
            };

            if (_sessionExpiredMessage != null)
                rows.Insert(3, new Markup(_sessionExpiredMessage));

            return new Panel(new Rows(rows)).RoundedBorder().Header("[bold red]🍅 Pomodoro[/]");
        }

        private double ComputeProgress()
        {
            if (!state.PhaseStartTimeUtc.HasValue || !state.PhaseDuration.HasValue)
                return 0;

            TimeSpan elapsed;

            if (state.PausedAtUtc.HasValue)
            {
                elapsed = state.PausedAtUtc.Value - state.PhaseStartTimeUtc.Value - state.PauseAccumulated;
            }
            else
            {
                var currentTime = timeProvider.GetUtcNow();
                elapsed = currentTime - state.PhaseStartTimeUtc.Value - state.PauseAccumulated;
            }

            return Math.Clamp(elapsed / state.PhaseDuration.Value, 0, 1);
        }

        private TimeSpan ComputeRemaining()
        {
            if (!state.PhaseStartTimeUtc.HasValue || !state.PhaseDuration.HasValue)
                return state.PhaseDuration ?? TimeSpan.Zero;

            if (state.PausedAtUtc.HasValue)
            {
                return GetRemainingTime(state.PausedAtUtc.Value);
            }

            return GetRemainingTime(timeProvider.GetUtcNow());
        }

        private TimeSpan GetRemainingTime(DateTimeOffset asOfTime)
        {
            if (!state.PhaseStartTimeUtc.HasValue || !state.PhaseDuration.HasValue)
                return state.PhaseDuration ?? TimeSpan.Zero;

            var elapsed = asOfTime - state.PhaseStartTimeUtc.Value - state.PauseAccumulated;
            var remaining = state.PhaseDuration.Value - elapsed;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }
}

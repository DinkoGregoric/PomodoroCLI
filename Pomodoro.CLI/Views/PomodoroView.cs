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
        PomodoroEngine engine,
        StartUseCase start,
        PauseUseCase pause,
        ResumeUseCase resume,
        ResetUseCase reset,
        SkipUseCase skip,
        IAppLogger logger)
    {
        private static readonly string[] SpinnerFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];
        private readonly PomodoroState state = engine.State;
        private readonly ProgressionSettings _progression = engine.Settings.Progression;
        private readonly TimingSettings _timing = engine.Settings.Timing;
        private int _spinnerIndex = 0;
        private string? _sessionExpiredMessage = null;
        private string? _phaseCompletedMessage = null;
        private CancellationTokenSource? _beepCts;

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

                engine.SessionExpiredDueToPauseTimeout += OnSessionExpired;
                engine.PhaseCompleted += OnPhaseCompleted;
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
                                            _beepCts?.Cancel();
                                            _sessionExpiredMessage = null;
                                            _phaseCompletedMessage = null;
                                            if (state.CurrentPhase == Phase.Idle || !state.PhaseStartTimeUtc.HasValue)
                                            {
                                                var r = await start.ExecuteAsync();
                                                if (r.IsFailure) logger.Warning($"Start failed: {r.Error.Message}");
                                            }
                                            else if (state.IsRunning)
                                            {
                                                var r = await pause.ExecuteAsync();
                                                if (r.IsFailure) logger.Warning($"Pause failed: {r.Error.Message}");
                                            }
                                            else
                                            {
                                                var r = await resume.ExecuteAsync();
                                                if (r.IsFailure) logger.Warning($"Resume failed: {r.Error.Message}");
                                            }
                                            break;
                                        case ConsoleKey.K:
                                        {
                                            var r = await skip.ExecuteAsync();
                                            if (r.IsFailure) logger.Warning($"Skip failed: {r.Error.Message}");
                                            break;
                                        }
                                        case ConsoleKey.T:
                                        {
                                            var r = await reset.ExecuteAsync();
                                            if (r.IsFailure) logger.Warning($"Reset failed: {r.Error.Message}");
                                            break;
                                        }
                                        case ConsoleKey.Q:
                                        {
                                            var r = await reset.ExecuteAsync();
                                            if (r.IsFailure) logger.Warning($"Reset on quit failed: {r.Error.Message}");
                                            quit = true;
                                            continue;
                                        }
                                    }
                                }

                                await engine.Dispatcher.DispatchAsync(new AdvanceTimeCommand());
                                ctx.UpdateTarget(BuildDisplay());

                                await Task.Delay(250);
                            }
                        });
                }
                finally
                {
                    engine.SessionExpiredDueToPauseTimeout -= OnSessionExpired;
                    engine.PhaseCompleted -= OnPhaseCompleted;
                    _beepCts?.Cancel();
                }
            }
        }

        private void OnSessionExpired(object? sender, SessionExpiredEventArgs e)
        {
            logger.Warning($"Session expired: {e.Phase} phase paused longer than {e.MaxAllowedPauseDuration} minutes");
            _sessionExpiredMessage = $"[red]Session reset: {e.Phase} phase paused longer than the allowed {e.MaxAllowedPauseDuration} minutes.[/]";
        }

        private void OnPhaseCompleted(object? sender, PhaseCompletedEventArgs e)
        {
            logger.Info($"Phase completed: {e.CompletedPhase} -> {e.NextPhase}");
            _phaseCompletedMessage = $"[green]{e.CompletedPhase} phase ended.[/] Press [[S]] to start {e.NextPhase}.";

            if (!e.PlaySound)
                return;

            _beepCts?.Cancel();
            _beepCts = new CancellationTokenSource();
            _ = BeepAsync(_beepCts.Token);
        }

        private static async Task BeepAsync(CancellationToken ct)
        {
            for (int i = 0; i < 5; i++)
            {
                if (ct.IsCancellationRequested) return;
                await Task.Run(Console.Beep, ct).ConfigureAwait(false);
                if (i < 4)
                {
                    await Task.Delay(1000, ct).ConfigureAwait(false);
                }
            }
        }

        private Panel BuildDisplay()
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
            };

            if (_progression.ProgressionEnabled)
            {
                rows.Add(_timing.WorkMinutes < _progression.TargetWorkMinutes
                    ? new Markup($"Progression: [bold]{_timing.WorkMinutes}→{_progression.TargetWorkMinutes}[/] min | {_progression.SessionsCompletedTowardStep}/{_progression.RequiredCompletionsToApplyStep} sessions")
                    : new Markup($"Progression: [bold]{_timing.WorkMinutes}[/] min (target reached)"));
            }

            rows.Add(new Rule());
            rows.Add(new Markup(statusLine));

            if (_phaseCompletedMessage != null)
                rows.Insert(rows.Count - 2, new Markup(_phaseCompletedMessage));
            if (_sessionExpiredMessage != null)
                rows.Insert(rows.Count - 2, new Markup(_sessionExpiredMessage));

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

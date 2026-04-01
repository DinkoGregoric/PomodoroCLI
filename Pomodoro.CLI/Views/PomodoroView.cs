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
                                            if (engine.State.CurrentPhase == Phase.Idle || !engine.State.PhaseStartTimeUtc.HasValue)
                                            {
                                                var r = await start.ExecuteAsync();
                                                if (r.IsFailure) logger.Warning($"Start failed: {r.Error.Message}");
                                            }
                                            else if (engine.State.IsRunning)
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
            var spinner = engine.State.IsRunning ? $"[green]{SpinnerFrames[_spinnerIndex++ % SpinnerFrames.Length]}[/] " : string.Empty;
            var bar = $"{spinner}[green]{new string('█', filled)}[/][grey]{new string('░', barWidth - filled)}[/]";

            string statusLine;
            if (engine.State.CurrentPhase == Phase.Idle || !engine.State.PhaseStartTimeUtc.HasValue)
            {
                statusLine = "[dim][[S]] Start [[Q]] Quit[/]";
            }
            else if (engine.State.PausedAtUtc.HasValue)
            {
                statusLine = "[yellow]Paused[/]  [dim][[S]] Resume  [[K]] Skip  [[T]] Reset  [[Q]] Quit[/]";
            }
            else
            {
                statusLine = "[green]Running[/]  [dim][[S]] Pause  [[K]] Skip  [[T]] Reset  [[Q]] Quit[/]";
            }

            var rows = new List<IRenderable>
            {
                new Markup($"[bold]{engine.State.CurrentPhase}[/]"),
                new Markup($"{bar} [bold]{progress:P0}[/]"),
                new Markup($"Remaining: [bold]{remaining:mm\\:ss}[/]"),
            };

            if (engine.ProgressionDetails.ProgressionEnabled)
            {
                rows.Add(engine.ProgressionDetails.WorkMinutes < engine.ProgressionDetails.TargetWorkMinutes
                    ? new Markup($"Progression: [bold]{engine.ProgressionDetails.WorkMinutes}→{engine.ProgressionDetails.TargetWorkMinutes}[/] min | {engine.ProgressionDetails.SessionsCompletedTowardStep}/{engine.ProgressionDetails.RequiredCompletionsToApplyStep} sessions")
                    : new Markup($"Progression: [bold]{engine.ProgressionDetails.WorkMinutes}[/] min (target reached)"));
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
            if (!engine.State.PhaseStartTimeUtc.HasValue || !engine.State.PhaseDuration.HasValue)
                return 0;

            TimeSpan elapsed;

            if (engine.State.PausedAtUtc.HasValue)
            {
                elapsed = engine.State.PausedAtUtc.Value - engine.State.PhaseStartTimeUtc.Value - engine.State.PauseAccumulated;
            }
            else
            {
                var currentTime = timeProvider.GetUtcNow();
                elapsed = currentTime - engine.State.PhaseStartTimeUtc.Value - engine.State.PauseAccumulated;
            }

            return Math.Clamp(elapsed / engine.State.PhaseDuration.Value, 0, 1);
        }

        private TimeSpan ComputeRemaining()
        {
            if (!engine.State.PhaseStartTimeUtc.HasValue || !engine.State.PhaseDuration.HasValue)
                return engine.State.PhaseDuration ?? TimeSpan.Zero;

            if (engine.State.PausedAtUtc.HasValue)
            {
                return GetRemainingTime(engine.State.PausedAtUtc.Value);
            }

            return GetRemainingTime(timeProvider.GetUtcNow());
        }

        private TimeSpan GetRemainingTime(DateTimeOffset asOfTime)
        {
            if (!engine.State.PhaseStartTimeUtc.HasValue || !engine.State.PhaseDuration.HasValue)
                return engine.State.PhaseDuration ?? TimeSpan.Zero;

            var elapsed = asOfTime - engine.State.PhaseStartTimeUtc.Value - engine.State.PauseAccumulated;
            var remaining = engine.State.PhaseDuration.Value - elapsed;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }
}

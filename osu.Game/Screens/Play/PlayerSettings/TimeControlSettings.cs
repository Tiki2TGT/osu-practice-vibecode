using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class TimeControlSettings : PlayerSettingsGroup, IKeyBindingHandler<GlobalAction>
    {
        private const int padding = 10;
        private double userPlaybackRateBeforeFastForward;

        public readonly Bindable<double> UserPlaybackRate = new BindableDouble(1)
        {
            MinValue = 0.05,
            MaxValue = 2,
            Precision = 0.01,
        };

        [Resolved]
        private Player player { get; set; } = null!;

        [Resolved]
        private GameplayClockContainer gameplayClock { get; set; } = null!;

        private readonly IBindable<bool> isPaused = new BindableBool();

        private PlayerSliderBar<double> rateSlider = null!;
        private OsuSpriteText multiplierText = null!;
        private IconButton pausePlay = null!;

        public TimeControlSettings()
            : base("Time control")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,

                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, padding),

                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,

                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5, 0),

                            Children = new Drawable[]
                            {
                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.FastBackward,
                                    Action = () => seek(-10),
                                    TooltipText = "Seek backward 10 seconds",
                                },

                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Backward,
                                    Action = () => seek(-1),
                                    TooltipText = "Seek backward 1 second",
                                },

                                pausePlay = new IconButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,

                                    Scale = new Vector2(1.4f),
                                    IconScale = new Vector2(1.4f),

                                    Action = () =>
                                    {
                                        if (gameplayClock.IsRunning)
                                            gameplayClock.Stop();
                                        else
                                            gameplayClock.Start();
                                    },
                                },

                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.Forward,
                                    Action = () => seek(1),
                                    TooltipText = "Seek forward 1 second",
                                },

                                new SeekButton
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Icon = FontAwesome.Solid.FastForward,
                                    Action = () => seek(10),
                                    TooltipText = "Seek forward 10 seconds",
                                },
                            },
                        },

                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,

                            Children = new Drawable[]
                            {
                                rateSlider = new PlayerSliderBar<double>
                                {
                                    LabelText = PlayerSettingsOverlayStrings.PlaybackSpeed,
                                    Current = UserPlaybackRate,
                                },

                                multiplierText = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,

                                    Font = OsuFont.GetFont(
                                        weight: FontWeight.Bold
                                    ),

                                    Margin = new MarginPadding
                                    {
                                        Right = 20
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            rateSlider.Current.BindValueChanged(
                multiplier =>
                    multiplierText.Text =
                        $"{multiplier.NewValue:0.00}x",
                true
            );

            isPaused.BindTo(gameplayClock.IsPaused);

            isPaused.BindValueChanged(paused =>
            {
                if (!paused.NewValue)
                {
                    pausePlay.TooltipText = ToastStrings.PauseTrack;
                    pausePlay.Icon = FontAwesome.Regular.PauseCircle;
                }
                else
                {
                    pausePlay.TooltipText = ToastStrings.PlayTrack;
                    pausePlay.Icon = FontAwesome.Regular.PlayCircle;
                }
            }, true);
        }

        private void seek(double seconds)
        {
            double target = Math.Clamp(
                gameplayClock.CurrentTime + seconds * 1000,
                0,
                player.GameplayState.Beatmap.GetLastObjectTime()
            );

            seekToTime(target);
        }

        private void seekToTime(double target)
        {
            bool wasPaused = gameplayClock.IsPaused.Value;

            player.Seek(target);

            if (wasPaused)
            {
                gameplayClock.Start();
                Scheduler.AddOnce(gameplayClock.Stop);
            }
}

        private void stepObject(int direction)
        {
            var hitObjects = player.GameplayState.Beatmap.HitObjects;

            if (hitObjects.Count == 0)
                return;

            double currentTime = gameplayClock.CurrentTime;

            // Small tolerance so pressing the same direction again
            // doesn't keep selecting the object we're already sitting on.
            const double tolerance = 1;

            if (direction < 0)
            {
                var previous = hitObjects
                            .LastOrDefault(o => o.StartTime < currentTime - tolerance);

                seekToTime(previous?.StartTime ?? 0);
            }
            else
            {
                var next = hitObjects
                        .FirstOrDefault(o => o.StartTime > currentTime + tolerance);

                seekToTime(next?.StartTime ?? hitObjects[^1].StartTime);
            }
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.SeekReplayBackward:
                    seek(-5 * UserPlaybackRate.Value);
                    return true;

                case GlobalAction.SeekReplayForward:
                    seek(5 * UserPlaybackRate.Value);
                    return true;

                case GlobalAction.StepReplayBackward:
                    stepObject(-1);
                    return true;

                case GlobalAction.StepReplayForward:
                    stepObject(1);
                    return true;

                case GlobalAction.TogglePauseReplay:
                    if (gameplayClock.IsPaused.Value)
                        gameplayClock.Start();
                    else
                        gameplayClock.Stop();

                    return true;

                case GlobalAction.FastForwardReplay:
                    if (e.Repeat)
                        return false;

                    userPlaybackRateBeforeFastForward = UserPlaybackRate.Value;
                    UserPlaybackRate.Value *= 2;
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.FastForwardReplay)
                UserPlaybackRate.Value = userPlaybackRateBeforeFastForward;
        }

        private partial class SeekButton : IconButton
        {
            public SeekButton()
            {
                AddInternal(new RepeatingButtonBehaviour(this));
            }
        }
    }
}

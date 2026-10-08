using System;
using System.Globalization;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PracticeCheckpointDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved(CanBeNull = true)]
        private PracticeCheckpointHudState? checkpointState { get; set; }

        private OsuSpriteText currentCheckpointText = null!;
        private OsuSpriteText currentTimeText = null!;

        private FillFlowContainer checkpointList = null!;

        public PracticeCheckpointDisplay()
        {
            // This becomes the initial size only.
            // The skin editor can resize Width and Height independently.
            Size = new Vector2(300, 220);

            Masking = true;
            CornerRadius = 8;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.75f,
                },

                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(10),

                    ColumnDimensions = new[]
                    {
                        new Dimension()
                    },

                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 52),
                        new Dimension(),
                    },

                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 2),

                                Children = new Drawable[]
                                {
                                    currentCheckpointText = new OsuSpriteText
                                    {
                                        Font = OsuFont.Default.With(
                                            size: 18,
                                            weight: FontWeight.Bold
                                        ),
                                    },

                                    currentTimeText = new OsuSpriteText
                                    {
                                        Font = OsuFont.Default.With(size: 13),
                                    },
                                }
                            },
                        },

                        new Drawable[]
                        {
                            new OsuScrollContainer(Direction.Vertical)
                            {
                                RelativeSizeAxes = Axes.Both,

                                Child = checkpointList = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,

                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 2),
                                }
                            },
                        },
                    }
                },
            };
        }
        private void onCheckpointStateChanged()
        {
            Schedule(updateState);
        }
        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (checkpointState != null)
            {
                checkpointState.Changed += onCheckpointStateChanged;
                updateState();
            }
            else
            {
                // Useful when viewed somewhere without a live Player state,
                // such as a skin-layout preview.
                showPreviewState();
            }
        }

        private void updateState()
        {
            if (checkpointState == null)
                return;

            int count = checkpointState.Checkpoints.Count;
            int activeIndex = checkpointState.ActiveIndex;

            if (activeIndex < 0)
            {
                currentCheckpointText.Text = count == 0
                    ? "PRACTICE • START"
                    : $"PRACTICE • START • {count} CP";

                currentTimeText.Text = "Start of map";
            }
            else
            {
                currentCheckpointText.Text =
                    $"PRACTICE • CP {activeIndex + 1} / {count}";

                currentTimeText.Text =
                    formatTime(checkpointState.Checkpoints[activeIndex]);
            }

            checkpointList.Clear();

            checkpointList.Add(createRow(
                "START",
                activeIndex == -1
            ));

            for (int i = 0; i < count; i++)
            {
                checkpointList.Add(
                    createCheckpointRow(
                        i,
                        checkpointState.Checkpoints[i],
                        i == activeIndex
                    )
                );
            }
        }

        private void showPreviewState()
        {
            currentCheckpointText.Text = "PRACTICE • CP 2 / 4";
            currentTimeText.Text = "1:17.438";

            checkpointList.Clear();

            checkpointList.Add(createRow("START", false));
            checkpointList.Add(createRow("CP 1    0:32.125", false));
            checkpointList.Add(createRow("CP 2    1:17.438", true));
            checkpointList.Add(createRow("CP 3    1:49.850", false));
            checkpointList.Add(createRow("CP 4    2:15.200", false));
        }

        private static Drawable createRow(string text, bool active)
        {
            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 24,

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = active ? 0.15f : 0,
                    },

                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,

                        X = 6,

                        Text = active
                            ? $"● {text}"
                            : $"  {text}",

                        Font = OsuFont.Default.With(
                            size: 13,
                            weight: active
                                ? FontWeight.Bold
                                : FontWeight.Regular
                        ),
                    },
                }
            };
        }

        private Drawable createCheckpointRow(
            int index,
            double time,
            bool active)
        {
            var timeTextBox = new OsuTextBox
            {
                RelativeSizeAxes = Axes.X,
                Height = 24,

                Text = formatTime(time),

                SelectAllOnFocus = true,
                CommitOnFocusLost = true,
                ReleaseFocusOnCommit = true,
            };

            timeTextBox.OnCommit += (_, _) =>
            {
                if (tryParseTime(timeTextBox.Text, out double newTime))
                {
                    checkpointState?.RequestTimestampEdit(index, newTime);
                }
                else
                {
                    // Invalid input: restore previous timestamp.
                    timeTextBox.Text = formatTime(time);
                }
            };

            return new Container
            {
                RelativeSizeAxes = Axes.X,
                Height = 30,

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White,
                        Alpha = active ? 0.15f : 0,
                    },

                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,

                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Absolute, 60),
                            new Dimension(),
                        },

                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,

                                    X = 6,

                                    Text = active
                                        ? $"● CP {index + 1}"
                                        : $"  CP {index + 1}",

                                    Font = OsuFont.Default.With(
                                        size: 13,
                                        weight: active
                                            ? FontWeight.Bold
                                            : FontWeight.Regular
                                    ),
                                },

                                timeTextBox,
                            },
                        },
                    },
                }
            };
        }

        private static string formatTime(double milliseconds)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(milliseconds);

            return time.TotalHours >= 1
                ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:000}"
                : $"{time.Minutes}:{time.Seconds:00}.{time.Milliseconds:000}";
        }

        private static bool tryParseTime(
            string text,
            out double milliseconds)
        {
            milliseconds = 0;

            string[] parts = text.Trim().Split(':');

            if (parts.Length is < 1 or > 3)
                return false;

            double totalSeconds = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                if (!double.TryParse(
                        parts[i],
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                    return false;

                if (value < 0)
                    return false;

                totalSeconds = totalSeconds * 60 + value;
            }

            milliseconds = totalSeconds * 1000;

            return true;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (checkpointState != null)
                checkpointState.Changed -= onCheckpointStateChanged;

            base.Dispose(isDisposing);
        }
    }
}

using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.HUD
{
    public class PracticeCheckpointHudState
    {
        private readonly List<double> checkpoints = new();

        public IReadOnlyList<double> Checkpoints => checkpoints;

        public int ActiveIndex { get; private set; } = -1;

        public event Action? Changed;

        public void SetState(IReadOnlyList<double> newCheckpoints, int activeIndex)
        {
            checkpoints.Clear();
            checkpoints.AddRange(newCheckpoints);

            ActiveIndex = activeIndex;

            Changed?.Invoke();
        }
        public Action<int, double>? EditTimestampRequested { get; set; }

        public void RequestTimestampEdit(int index, double time)
        {
            EditTimestampRequested?.Invoke(index, time);
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Play
{
    public partial class HotkeyRetryOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        public Action? SetPracticeCheckpoint { get; init; }

        public Action? ReplacePracticeCheckpoint { get; init; }

        public Action? DeletePracticeCheckpoint { get; init; }

        public Action? PreviousPracticeCheckpoint { get; init; }

        public Action? NextPracticeCheckpoint { get; init; }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.SetPracticeCheckpoint:
                    SetPracticeCheckpoint?.Invoke();
                    return true;

                case GlobalAction.ReplacePracticeCheckpoint:
                    ReplacePracticeCheckpoint?.Invoke();
                    return true;

                case GlobalAction.DeletePracticeCheckpoint:
                    DeletePracticeCheckpoint?.Invoke();
                    return true;

                case GlobalAction.PreviousPracticeCheckpoint:
                    PreviousPracticeCheckpoint?.Invoke();
                    return true;

                case GlobalAction.NextPracticeCheckpoint:
                    NextPracticeCheckpoint?.Invoke();
                    return true;

                case GlobalAction.QuickRetry:
                    BeginConfirm();
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action != GlobalAction.QuickRetry)
                return;

            AbortConfirm();
        }

        protected override void Confirm()
        {
            base.Confirm();

            // Not removing immediately can lead to delays due to async disposal.
            // This is done here rather than in `Player` because it's simpler to handle.
            RemoveAudioAdjustments();
        }
    }
}

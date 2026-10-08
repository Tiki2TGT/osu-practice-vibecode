using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModTimeControl : Mod, IApplicableToPlayer
    {
        public override string Name => "Time Control";

        public override string Acronym => "TM";

        public override ModType Type => ModType.Automation;

        public override LocalisableString Description =>
            "Adds replay-style playback controls for practice.";

        public void ApplyToPlayer(Player player)
        {
            player.EnableTimeControl();
        }
    }
}

using StardewModdingAPI;

namespace DesertEagle
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Monitor.Log(
                "Desert Eagle loaded successfully!",
                LogLevel.Info
            );
        }
    }
}

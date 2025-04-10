namespace LessHiddenWallSFX
{
    using System;
    using EntityComponent;
    using HarmonyLib;
    using JumpKing;
    using JumpKing.Mods;

    [JumpKingMod("Zebra.LessHiddenWallSFX")]
    public static class ModEntry
    {
        private static Type RaymanWall { get; set; }

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad() => RaymanWall = AccessTools.TypeByName("JumpKing.Props.RaymanWall.RaymanWallEntity");

        /// <summary>
        /// Called by Jump King when the Level Starts
        /// </summary>
        [OnLevelStart]
        public static void OnLevelStart()
        {
            var contentManager = Game1.instance.contentManager;
            var level = contentManager.level;
            if (level == null)
            {
                return;
            }

            // Really it doesn't mute it but sets it to null so theres nothing to play.
            // In a way it is muting it.
            var muteWalls = false;
            foreach (var tag in level.Info.Tags)
            {
                if (tag == "MuteHiddenWallSFX")
                {
                    muteWalls = true;
                    break;
                }
            }
            if (!muteWalls)
            {
                return;
            }

            foreach (var entity in EntityManager.instance.Entities)
            {
                if (!(entity.GetType() == RaymanWall))
                {
                    continue;
                }
                _ = Traverse.Create(entity).Field("m_appear_sfx").SetValue(null);
            }
        }
    }
}

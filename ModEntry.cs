namespace LessHiddenWallSFX
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using EntityComponent;
    using HarmonyLib;
    using JumpKing;
    using JumpKing.Mods;
    using Microsoft.Xna.Framework;

    [JumpKingMod("Zebra.LessHiddenWallSFX")]
    public static class ModEntry
    {
        /// <summary>The RaymanWallEnity type.</summary>
        private static Type RaymanWall { get; set; }
        /// <summary>Regex to match the entire tag defining screen numbers.</summary>
        private static Regex TagRegex { get; set; } = new Regex(@"^MuteHiddenWallSFX=\(\s*(\d+(?:-\d+)?(?:\s*,\s*\d+(?:-\d+)?)*?)\s*\)$");
        /// <summary>Matches all numbers of ranges into their own group.</summary>
        private static Regex NumberRegex { get; set; } = new Regex(@"\d+(?:-\d+)?");

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
            foreach (var tag in level.Info.Tags)
            {
                if (tag == "MuteHiddenWallSFX")
                {
                    MuteAll();
                    break;
                }
                var match = TagRegex.Match(tag);
                if (match.Success)
                {
                    MuteScreens(GetMutedScreens(match.Value));
                    break;
                }
            }
        }

        /// <summary>
        /// Mutes all RaymanWalls by setting their sound reference to null.
        /// </summary>
        private static void MuteAll()
        {
            foreach (var entity in EntityManager.instance.Entities)
            {
                if (!(entity.GetType() == RaymanWall))
                {
                    continue;
                }
                _ = Traverse.Create(entity).Field("m_appear_sfx").SetValue(null);
            }
        }

        /// <summary>
        /// Creates a HashSet of Screens that RaymanWalls are supposed to be muted on. The tag contained either single
        /// numbers that can be directly added to the mute screens or a range of the form x-y. This results in all screens
        /// from x to y (inclusive) being muted.
        /// </summary>
        /// <param name="tagInside">The screen numbers that have been defined inside the tag.</param>
        /// <returns>A HashSet containing all screens that RaymanWalls are supposed to be muted on.</returns>
        private static HashSet<int> GetMutedScreens(string tagInside)
        {
            var screens = new HashSet<int>();
            var values = (from Match match in NumberRegex.Matches(tagInside) select match.Value).ToList();
            foreach (var value in values)
            {
                if (value.Contains("-"))
                {
                    var parts = value.Split('-');
                    var start = int.Parse(parts[0]);
                    var end = int.Parse(parts[1]);
                    if (end <= start)
                    {
                        continue;
                    }
                    foreach (var i in Enumerable.Range(start, end - start + 1))
                    {
                        _ = screens.Add(i);
                    }
                }
                else
                {
                    _ = screens.Add(int.Parse(value));
                }
            }
            return screens;
        }

        /// <summary>
        /// Mutes RaymanWalls by setting their sound reference to null should their screen position
        /// be inside the HashSet of screens that RaymanWalls are supposed to be muted on.
        /// </summary>
        /// <param name="screens">Screens that RaymanWalls are supposed to be muted on.</param>
        private static void MuteScreens(HashSet<int> screens)
        {
            foreach (var entity in EntityManager.instance.Entities)
            {
                if (!(entity.GetType() == RaymanWall))
                {
                    continue;
                }
                var traverse = Traverse.Create(entity);
                // - 360 because screen 1 is positive coordinates.
                // From 360 to 0 is screen 1 => 0 to -360.
                // From 0 to -360 is screen 2 => -360 to -720.
                var screen = ((int)Traverse.Create(entity).Field("m_position").GetValue<Vector2>().Y - 360) / -360;
                if (screens.Contains(screen))
                {
                    _ = traverse.Field("m_appear_sfx").SetValue(null);
                }
            }
        }
    }
}

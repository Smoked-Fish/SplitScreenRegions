using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using StardewValley.Menus;
using Microsoft.CodeAnalysis;
using StardewValley.Characters;
using StardewValley.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace SplitScreenRegions.Framework.Patches
{
    internal class SplitScreenPatch : PatchTemplate
    {
        internal SplitScreenPatch(Harmony harmony) : base(harmony, typeof(Game1)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(Game1.SetWindowSize), nameof(SetWindowSizePrefix), [typeof(int), typeof(int)]);
        }
        private static void CanBeRemovedPostFix(Furniture __instance, Farmer who, ref bool __result)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }

        private static bool SetWindowSizePrefix(Game1 __instance, int w, int h)
        {
            if (!ModEntry.modConfig.EnableMod)
                return true;

            Rectangle oldWindow = new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (w < 1280 && !Game1.graphics.IsFullScreen)
                {
                    w = 1280;
                }
                if (h < 720 && !Game1.graphics.IsFullScreen)
                {
                    h = 720;
                }
            }
            if (!Game1.graphics.IsFullScreen && __instance.Window.AllowUserResizing)
            {
                Game1.graphics.PreferredBackBufferWidth = w;
                Game1.graphics.PreferredBackBufferHeight = h;
            }
            if (__instance.IsMainInstance && Game1.graphics.SynchronizeWithVerticalRetrace != Game1.options.vsyncEnabled)
            {
                Game1.graphics.SynchronizeWithVerticalRetrace = Game1.options.vsyncEnabled;
                IGameLogger log = (IGameLogger)typeof(Game1).GetField("log", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                log.Verbose("Vsync toggled: " + Game1.graphics.SynchronizeWithVerticalRetrace);
            }
            Game1.graphics.ApplyChanges();
            try
            {
                if (Game1.graphics.IsFullScreen)
                {
                    __instance.localMultiplayerWindow = new Rectangle(0, 0, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
                }
                else
                {
                    __instance.localMultiplayerWindow = new Rectangle(0, 0, w, h);
                }
            }
            catch (Exception)
            {
            }
            Game1.defaultDeviceViewport = new Viewport(__instance.localMultiplayerWindow);
            List<Vector4> screen_splits = new List<Vector4>();
            if (GameRunner.instance.gameInstances.Count <= 1)
            {
                screen_splits.Add(new Vector4(0f, 0f, 1f, 1f));
            }
            else
            {
                switch (GameRunner.instance.gameInstances.Count)
                {
                    case 2:
                        screen_splits.Add(ModEntry.modConfig.TwoPlayer[0]);
                        screen_splits.Add(ModEntry.modConfig.TwoPlayer[1]);
                        break;
                    case 3:
                        screen_splits.Add(ModEntry.modConfig.ThreePlayer[0]);
                        screen_splits.Add(ModEntry.modConfig.ThreePlayer[1]);
                        screen_splits.Add(ModEntry.modConfig.ThreePlayer[2]);
                        break;
                    case 4:
                        screen_splits.Add(ModEntry.modConfig.FourPlayer[0]);
                        screen_splits.Add(ModEntry.modConfig.FourPlayer[1]);
                        screen_splits.Add(ModEntry.modConfig.FourPlayer[2]);
                        screen_splits.Add(ModEntry.modConfig.FourPlayer[3]);
                        break;
                }
            }
            if (GameRunner.instance.gameInstances.Count <= 1)
            {
                __instance.zoomModifier = 1f;
            }
            else
            {
                __instance.zoomModifier = 0.5f;
            }
            Vector4 current_screen_split = screen_splits[__instance.instanceIndex];
            Vector2? old_ui_dimensions = null;
            if (__instance.uiScreen != null)
            {
                old_ui_dimensions = new Vector2(__instance.uiScreen.Width, __instance.uiScreen.Height);
            }
            __instance.localMultiplayerWindow.X = (int)((float)w * current_screen_split.X);
            __instance.localMultiplayerWindow.Y = (int)((float)h * current_screen_split.Y);
            __instance.localMultiplayerWindow.Width = (int)Math.Ceiling((float)w * current_screen_split.Z);
            __instance.localMultiplayerWindow.Height = (int)Math.Ceiling((float)h * current_screen_split.W);
            try
            {
                int sw = (int)Math.Ceiling((float)__instance.localMultiplayerWindow.Width * (1f / Game1.options.zoomLevel));
                int sh = (int)Math.Ceiling((float)__instance.localMultiplayerWindow.Height * (1f / Game1.options.zoomLevel));
                __instance.screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, sw, sh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                __instance.screen.Name = "Screen";
                int uw = (int)Math.Ceiling((float)__instance.localMultiplayerWindow.Width / Game1.options.uiScale);
                int uh = (int)Math.Ceiling((float)__instance.localMultiplayerWindow.Height / Game1.options.uiScale);
                __instance.uiScreen = new RenderTarget2D(Game1.graphics.GraphicsDevice, uw, uh, mipMap: false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                __instance.uiScreen.Name = "UI Screen";
            }
            catch (Exception)
            {
            }
            Game1.updateViewportForScreenSizeChange(fullscreenChange: false, __instance.localMultiplayerWindow.Width, __instance.localMultiplayerWindow.Height);
            if (old_ui_dimensions.HasValue && old_ui_dimensions.Value.X == (float)__instance.uiScreen.Width && old_ui_dimensions.Value.Y == (float)__instance.uiScreen.Height)
            {
                return false;
            }
            Game1.PushUIMode();
            Game1.textEntry?.gameWindowSizeChanged(oldWindow, new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
            foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
            {
                onScreenMenu.gameWindowSizeChanged(oldWindow, new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
            }
            Game1.currentMinigame?.changeScreenSize();
            Game1.activeClickableMenu?.gameWindowSizeChanged(oldWindow, new Rectangle(Game1.viewport.X, Game1.viewport.Y, Game1.viewport.Width, Game1.viewport.Height));
            if (Game1.activeClickableMenu is GameMenu gameMenu2)
            {
                if (gameMenu2.GetCurrentPage() is OptionsPage optionsPage)
                {
                    optionsPage.preWindowSizeChange();
                }
                GameMenu gameMenu = (GameMenu)(Game1.activeClickableMenu = new GameMenu(gameMenu2.currentTab));
                if (gameMenu.GetCurrentPage() is OptionsPage newOptionsPage)
                {
                    newOptionsPage.postWindowSizeChange();
                }
            }
            Game1.PopUIMode();

            return false;
        }
    }
}

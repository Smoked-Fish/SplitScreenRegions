using HarmonyLib;
using Microsoft.Xna.Framework;
using SplitScreenRegions.Framework.Interfaces;
using SplitScreenRegions.Framework.Managers;
using SplitScreenRegions.Framework.Patches;
using SplitScreenRegions.Framework.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace SplitScreenRegions
{
    public class ModEntry : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static ModConfig modConfig;
        internal static Multiplayer multiplayer;
        internal static IGenericModConfigMenuApi configApi;

        // Managers
        internal static ApiManager apiManager;

        public override void Entry(IModHelper helper)
        {
            // Setup i18n
            I18n.Init(helper.Translation);

            // Setup the monitor, helper, config and multiplayer
            monitor = Monitor;
            modHelper = helper;
            modConfig = Helper.ReadConfig<ModConfig>();
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup the manager
            apiManager = new ApiManager();

            // Apply the patches
            var harmony = new Harmony(ModManifest.UniqueID);

            new SplitScreenPatch(harmony).Apply();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Input.ButtonsChanged += OnButtonChanged;
            ButtonOptions.onClick += OnReset;
        }



        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            configApi = apiManager.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu", false);
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && configApi != null)
            {
                configApi.Register(ModManifest, () => modConfig = new ModConfig(), () => Helper.WriteConfig(modConfig));
                configApi.OnFieldChanged(ModManifest, new Action<string, object>(this.OnFieldChanged));
                configApi.AddPageLink(ModManifest, "2PPage", () => String.Concat("> ", I18n.Config_SplitScreenRegions_2PViewports_Title()));
                configApi.AddPageLink(ModManifest, "3PPage", () => String.Concat("> ", I18n.Config_SplitScreenRegions_3PViewports_Title()));
                configApi.AddPageLink(ModManifest, "4PPage", () => String.Concat("> ", I18n.Config_SplitScreenRegions_4PViewports_Title()));
                configApi.AddPageLink(ModManifest, "OtherPage", () => String.Concat("> ", I18n.Config_SplitScreenRegions_Other_Title()));



                configApi.AddPage(ModManifest, "2PPage", I18n.Config_SplitScreenRegions_2PViewports_Title);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_ResetPage_Name, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.2.ALL");
                AddSimpleHorizontalSeparator(ModManifest);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player1_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.2.1");
                this.SetupViewportOptions(configApi, 1, 2, () => modConfig.TwoPlayer[0], value => modConfig.TwoPlayer[0] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player2_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.2.2");
                this.SetupViewportOptions(configApi, 2, 2, () => modConfig.TwoPlayer[1], value => modConfig.TwoPlayer[1] = value);

                configApi.AddPage(ModManifest, "3PPage", I18n.Config_SplitScreenRegions_3PViewports_Title);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_ResetPage_Name, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.3.ALL");
                AddSimpleHorizontalSeparator(ModManifest);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player1_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.3.1");
                this.SetupViewportOptions(configApi, 1, 3, () => modConfig.ThreePlayer[0], value => modConfig.ThreePlayer[0] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player2_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.3.2");
                this.SetupViewportOptions(configApi, 2, 3, () => modConfig.ThreePlayer[1], value => modConfig.ThreePlayer[1] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player3_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.3.3");
                this.SetupViewportOptions(configApi, 3, 3, () => modConfig.ThreePlayer[2], value => modConfig.ThreePlayer[2] = value);

                configApi.AddPage(ModManifest, "4PPage", I18n.Config_SplitScreenRegions_4PViewports_Title);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_ResetPage_Name, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.4.ALL");
                AddSimpleHorizontalSeparator(ModManifest);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player1_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.4.1");
                this.SetupViewportOptions(configApi, 1, 4, () => modConfig.FourPlayer[0], value => modConfig.FourPlayer[0] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player2_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.4.2");
                this.SetupViewportOptions(configApi, 2, 4, () => modConfig.FourPlayer[1], value => modConfig.FourPlayer[1] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player3_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.4.3");
                this.SetupViewportOptions(configApi, 3, 4, () => modConfig.FourPlayer[2], value => modConfig.FourPlayer[2] = value);
                AddButtonOption(ModManifest, I18n.Config_SplitScreenRegions_Player4_Title, I18n.Config_SplitScreenRegions_Reset_Name, fieldId: "ResetButton.4.4");
                this.SetupViewportOptions(configApi, 4, 4, () => modConfig.FourPlayer[3], value => modConfig.FourPlayer[3] = value);

                configApi.AddPage(ModManifest, "OtherPage", I18n.Config_SplitScreenRegions_Other_Title);
                configApi.AddBoolOption(ModManifest, () => modConfig.EnableMod, value => modConfig.EnableMod = value, I18n.Config_SplitScreenRegions_EnableMod_Name, I18n.Config_SplitScreenRegions_EnableMod_Description);
                configApi.AddBoolOption(ModManifest, () => modConfig.UpdateMainPlayer, value => modConfig.UpdateMainPlayer = value, I18n.Config_SplitScreenRegions_UpdateMainPlayer_Name, I18n.Config_SplitScreenRegions_UpdateMainPlayer_Name);
                configApi.AddBoolOption(ModManifest, () => modConfig.BackOutOnReset, value => modConfig.BackOutOnReset = value, I18n.Config_SplitScreenRegions_BackOutOnReset_Name, I18n.Config_SplitScreenRegions_BackOutOnReset_Description);
                configApi.AddKeybindList(ModManifest, () => modConfig.ResetKey, value => modConfig.ResetKey = value, I18n.Config_SplitScreenRegions_ResetKey_Name, I18n.Config_SplitScreenRegions_ResetKey_Description);
            }
        }

        private void OnButtonChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (modConfig.ResetKey.JustPressed())
            {
                modConfig.ResetVectorListToDefault(modConfig.TwoPlayer, modConfig.DefaultTwoPlayer);
                modConfig.ResetVectorListToDefault(modConfig.ThreePlayer, modConfig.DefaultThreePlayer);
                modConfig.ResetVectorListToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer);

                try
                {
                    if (GameRunner.instance.gameInstances.Count != 0)
                    {
                        int w = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferWidth : GameRunner.instance.Window.ClientBounds.Width);
                        int h = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferHeight : GameRunner.instance.Window.ClientBounds.Height);
                        GameRunner.instance.ExecuteForInstances(instance => instance.SetWindowSize(w, h));
                        Game1.graphics.ApplyChanges();
                    }
                }
                catch (Exception ex)
                {
                    monitor.Log($"Failed to refresh viewports: {ex}", LogLevel.Error);
                }
            }
        }

        private void SetupViewportOptions(IGenericModConfigMenuApi configMenu, int player, int numPlayers, Func<Vector4> getVec, Action<Vector4> setVec)
        {
            configMenu.AddNumberOption(ModManifest, () => getVec().X, value => { Vector4 vector4 = getVec(); setVec(new Vector4(value, vector4.Y, vector4.Z, vector4.W)); ; },
                () => I18n.Config_SplitScreenRegions_X_Name(player), () => I18n.Config_SplitScreenRegions_X_Description(player), 0.0f, 1f, 0.01f, value => value.ToString("0.00"), $"{numPlayers}.{player}.X");

            configMenu.AddNumberOption(ModManifest, () => getVec().Y, value => { Vector4 vector4 = getVec(); setVec(new Vector4(vector4.X, value, vector4.Z, vector4.W)); },
                () => I18n.Config_SplitScreenRegions_Y_Name(player), () => I18n.Config_SplitScreenRegions_Y_Description(player), 0.0f, 1f, 0.01f, value => value.ToString("0.00"), $"{numPlayers}.{player}.Y");

            configMenu.AddNumberOption(ModManifest, () => getVec().Z, value => { Vector4 vector4 = getVec(); setVec(new Vector4(vector4.X, vector4.Y, value, vector4.W)); },
                () => I18n.Config_SplitScreenRegions_W_Name(player), () => I18n.Config_SplitScreenRegions_W_Description(player), 0.0f, 1f, 0.01f, value => value.ToString("0.00"), $"{numPlayers}.{player}.Z");

            configMenu.AddNumberOption(ModManifest, () => getVec().W, value => { Vector4 vector4 = getVec(); setVec(new Vector4(vector4.X, vector4.Y, vector4.Z, value)); },
                () => I18n.Config_SplitScreenRegions_H_Name(player), () => I18n.Config_SplitScreenRegions_H_Description(player), 0.0f, 1f, 0.01f, value => value.ToString("0.00"), $"{numPlayers}.{player}.W");
        }

        private void OnFieldChanged(string field, object value)
        {
            try
            {
                string[] parts = field.Split('.');
                switch (parts[0])
                {
                    case "2":
                        HandleFieldChange(modConfig.TwoPlayer, int.Parse(parts[1]) - 1, parts[2], value);
                        break;
                    case "3":
                        HandleFieldChange(modConfig.ThreePlayer, int.Parse(parts[1]) - 1, parts[2], value);
                        break;
                    case "4":
                        HandleFieldChange(modConfig.FourPlayer, int.Parse(parts[1]) - 1, parts[2], value);
                        break;
                    default:
                        break;
                }

                if (GameRunner.instance.gameInstances.Count != 0)
                {
                    int w = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferWidth : GameRunner.instance.Window.ClientBounds.Width);
                    int h = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferHeight : GameRunner.instance.Window.ClientBounds.Height);
                    GameRunner.instance.ExecuteForInstances(instance => instance.SetWindowSize(w, h));
                    Game1.graphics.ApplyChanges();
                }

            }
            catch (Exception e)
            {
                monitor.Log($"Failed to refresh viewports: {e}", LogLevel.Error);
            }
        }

        private void HandleFieldChange(Vector4[] numPlayersArray, int player, string toChange, object value)
        {
            // Don't live update the main player
            if (!modConfig.UpdateMainPlayer && player == 0) return;
            switch (toChange)
            {
                case "X":
                    numPlayersArray[player].X = (float)value;
                    break;
                case "Y":
                    numPlayersArray[player].Y = (float)value;
                    break;
                case "Z":
                    numPlayersArray[player].Z = (float)value;
                    break;
                case "W":
                    numPlayersArray[player].W = (float)value;
                    break;
                default:
                    break;
            }

            //monitor.Log(numPlayersArray[player].ToString(), LogLevel.Debug);
        }

        private void OnReset(ButtonClickEventArgs args)
        {
            Game1.playSound("backpackIN");
            string[] parts = args.FieldID.Split('.');
            int numPlayers = int.Parse(parts[1]);
            string toReset = parts[2];
            if (modConfig.BackOutOnReset)
                configApi.OpenModMenu(ModManifest);

            //monitor.Log(numPlayers + "," + toReset, LogLevel.Debug);
            switch (numPlayers)
            {
                case 2:
                    switch (toReset)
                    {
                        case "1":
                            modConfig.ResetVectorToDefault(modConfig.TwoPlayer, modConfig.DefaultTwoPlayer, 0);
                            break;
                        case "2":
                            modConfig.ResetVectorToDefault(modConfig.TwoPlayer, modConfig.DefaultTwoPlayer, 1);
                            break;
                        case "ALL":
                            modConfig.ResetVectorListToDefault(modConfig.TwoPlayer, modConfig.DefaultTwoPlayer);
                            break;
                        default:
                            break;
                    }
                    break;
                case 3:
                    switch (toReset)
                    {
                        case "1":
                            modConfig.ResetVectorToDefault(modConfig.ThreePlayer, modConfig.DefaultThreePlayer, 0);
                            break;
                        case "2":
                            modConfig.ResetVectorToDefault(modConfig.ThreePlayer, modConfig.DefaultThreePlayer, 1);
                            break;
                        case "3":
                            modConfig.ResetVectorToDefault(modConfig.ThreePlayer, modConfig.DefaultThreePlayer, 2);
                            break;
                        case "ALL":
                            modConfig.ResetVectorListToDefault(modConfig.ThreePlayer, modConfig.DefaultThreePlayer);
                            break;
                        default:
                            break;
                    }
                    break;
                case 4:
                    switch (toReset)
                    {
                        case "1":
                            modConfig.ResetVectorToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer, 0);
                            break;
                        case "2":
                            modConfig.ResetVectorToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer, 1);
                            break;
                        case "3":
                            modConfig.ResetVectorToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer, 2);
                            break;
                        case "4":
                            modConfig.ResetVectorToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer, 3);
                            break;
                        case "ALL":
                            modConfig.ResetVectorListToDefault(modConfig.FourPlayer, modConfig.DefaultFourPlayer);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            try
            {
/*                if (GameRunner.instance.gameInstances.Count != 0)
                {
                    int w = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferWidth : GameRunner.instance.Window.ClientBounds.Width);
                    int h = (Game1.graphics.IsFullScreen ? Game1.graphics.PreferredBackBufferHeight : GameRunner.instance.Window.ClientBounds.Height);
                    GameRunner.instance.ExecuteForInstances(instance => instance.SetWindowSize(w, h));
                    Game1.graphics.ApplyChanges();
                }*/
            }
            catch (Exception e)
            {
                monitor.Log($"Failed to refresh viewports: {e}", LogLevel.Error);
            }
        }

        public void AddButtonOption(IManifest mod, Func<string> leftText, Func<string> rightText, string fieldId = null)
        {
            if (configApi == null) return;

            var buttonOption = new ButtonOptions(leftText: leftText(), rightText: rightText(), fieldID: fieldId);

            configApi.AddComplexOption(
                mod: mod,
                name: () => "",
                draw: (batch, position) => buttonOption.Draw(batch, (int)position.X, (int)position.Y),
                height: () => buttonOption.rightTextHeight,
                beforeMenuOpened: () => { },
                beforeSave: () => { },
                afterReset: () => { },
                fieldId: fieldId
            );
        }

        public void AddSimpleHorizontalSeparator(IManifest mod)
        {
            if (configApi == null) return;

            var separatorOption = new SeparatorOptions();
            configApi.AddComplexOption(mod: mod, name: () => "", draw: separatorOption.Draw);
        }
    }
}

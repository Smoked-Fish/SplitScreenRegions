using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SplitScreenRegions
{

    internal class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool UpdateMainPlayer { get; set; } = false;
        public bool BackOutOnReset { get; set; } = false;
        public Vector4[] TwoPlayer { get; set; }
        public Vector4[] ThreePlayer { get; set; }
        public Vector4[] FourPlayer { get; set; }

        public Vector4[] DefaultTwoPlayer { get; }
        public Vector4[] DefaultThreePlayer { get; }
        public Vector4[] DefaultFourPlayer { get; }
        public KeybindList ResetKey { get; set; } = new KeybindList(SButton.F5);

        public ModConfig()
        {
            EnableMod = true;

            DefaultTwoPlayer =
            [
                new Vector4(0.00f, 0.00f, 0.50f, 1f),
                new Vector4(0.50f, 0.00f, 0.50f, 1f)
            ];
            DefaultThreePlayer =
            [
                new Vector4(0.00f, 0.00f, 1f, 0.50f),
                new Vector4(0.00f, 0.50f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.50f, 0.50f, 0.50f)
            ];
            DefaultFourPlayer =
            [
                new Vector4(0.00f, 0.00f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.00f, 0.50f, 0.50f),
                new Vector4(0.00f, 0.50f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.50f, 0.50f, 0.50f)
            ];


            TwoPlayer =
            [
                new Vector4(0.00f, 0.00f, 0.50f, 1f),
                new Vector4(0.50f, 0.00f, 0.50f, 1f)
            ];
            ThreePlayer =
            [
                new Vector4(0.00f, 0.00f, 1f, 0.50f),
                new Vector4(0.00f, 0.50f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.50f, 0.50f, 0.50f)
            ];
            FourPlayer =
            [
                new Vector4(0.00f, 0.00f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.00f, 0.50f, 0.50f),
                new Vector4(0.00f, 0.50f, 0.50f, 0.50f),
                new Vector4(0.50f, 0.50f, 0.50f, 0.50f)
            ];
        }

        // Method to reset any player configuration to its default value
        // Method to reset any player configuration to its default value
        public void ResetVectorListToDefault(Vector4[] playerConfig, Vector4[] defaultConfig)
        {
            Array.Clear(playerConfig, 0, playerConfig.Length);
            Array.Copy(defaultConfig, playerConfig, defaultConfig.Length);
        }

        // Method to reset a single vector inside a player configuration to its default value
        public void ResetVectorToDefault(Vector4[] playerConfig, Vector4[] defaultConfig, int index)
        {
            if (index >= 0 && index < playerConfig.Length)
            {
                playerConfig[index] = defaultConfig[index];
            }
            else
            {
                // Handle index out of range error
                throw new IndexOutOfRangeException("Index is out of range.");
            }
        }
    }
}

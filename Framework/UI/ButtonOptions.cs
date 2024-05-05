#nullable enable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SplitScreenRegions.Framework.Patches;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Threading;
using Color = Microsoft.Xna.Framework.Color;

namespace SplitScreenRegions.Framework.UI
{
    public class ButtonClickEventArgs
    {
        public string FieldID { get; }

        public ButtonClickEventArgs(string fieldID)
        {
            FieldID = fieldID;
        }
    }
    public class ButtonOptions
    {
        public static Action<ButtonClickEventArgs>? onClick;
        private readonly string leftText;
        private readonly string rightText;

        private bool isRightHovered = false;
        private bool wasRightHoveredPreviously = false;

        public int rightTextWidth { get; set; }
        public int rightTextHeight { get; set; }
        private int leftTextWidth;
        private int leftTextHeight;
        private string fieldID;

        public ButtonOptions(string leftText = "", string rightText = "", string? fieldID = null)
        {
            this.leftText = leftText;
            this.rightText = rightText;
            this.fieldID = fieldID ?? "";

            CalculateTextMeasurements();
        }

        private void CalculateTextMeasurements()
        {
            string parsedRightText = Game1.parseText(rightText, Game1.dialogueFont, 800);
            Vector2 rightTextSize = Game1.dialogueFont.MeasureString(parsedRightText) * 1f;
            rightTextWidth = (int)rightTextSize.X;
            rightTextHeight = (int)rightTextSize.Y;

            string parsedLeftText = Game1.parseText(leftText, Game1.dialogueFont, 800);
            Vector2 leftTextSize = MeasureString(parsedLeftText, true);
            leftTextWidth = (int)leftTextSize.X;
            leftTextHeight = (int)leftTextSize.Y;
        }

        private Vector2 MeasureString(string text, bool bold = false, float scale = 1f, SpriteFont? font = null)
        {
            return bold ? new Vector2((float)SpriteText.getWidthOfString(text) * scale, (float)SpriteText.getHeightOfString(text) * scale) : (font ?? Game1.dialogueFont).MeasureString(text) * scale;
        }

        protected void OnReset(string fieldId)
        {
            onClick?.Invoke(new ButtonClickEventArgs(fieldId));
        }


        private ButtonState lastButtonState = ButtonState.Released;

        protected void UpdateMouseState(int drawX, int drawY)
        {
            ButtonState buttonState = Mouse.GetState().LeftButton;
            int mouseX = Game1.getMouseX();
            int mouseY = Game1.getMouseY();

            if (buttonState == ButtonState.Pressed && drawX <= mouseX && mouseX <= drawX + rightTextWidth && drawY <= mouseY && mouseY <= drawY + rightTextHeight && lastButtonState == ButtonState.Released)
            {
                OnReset(fieldID);
            }

            lastButtonState = buttonState;

            int gmcmWidth = Math.Min(1200, Game1.uiViewport.Width - 200);
            int gmcmLeft = (Game1.uiViewport.Width - gmcmWidth) / 2;
            int top = (drawY);
            int left = gmcmLeft;

            bool isRightHoveredNow = IsHovered(drawX, drawY, rightTextWidth, rightTextHeight);

            if (isRightHoveredNow && !wasRightHoveredPreviously)
            {
                Game1.playSound("shiny4");
            }

            isRightHovered = isRightHoveredNow;
            wasRightHoveredPreviously = isRightHoveredNow;

            storedValues = (top, left);
        }

        public bool IsHovered(int drawX, int drawY, int width, int height)
        {
            int mouseX = Mouse.GetState().X;
            int mouseY = Mouse.GetState().Y;

            return drawX <= mouseX && mouseX <= drawX + width && drawY <= mouseY && mouseY <= drawY + height;
        }

        private (int top, int left) storedValues;

        public void Draw(SpriteBatch b, int posX, int posY)
        {
            try
            {
                // Retrieve stored calculated values
                UpdateMouseState(posX, posY);
                (int top, int left) = storedValues;

                // Draw rightText
                Color rightTextColor = isRightHovered ? Game1.unselectedOptionColor : Game1.textColor;
                Vector2 rightTextPosition = new Vector2(posX, posY);
                Utility.drawTextWithShadow(b, rightText, Game1.dialogueFont, rightTextPosition, rightTextColor);

                // Draw leftText
                Vector2 leftTextPosition = new Vector2(left - 8, top);
                SpriteText.drawString(b, leftText, (int)leftTextPosition.X, (int)leftTextPosition.Y, layerDepth: 1f, color: new Color?());

            }   
            catch (Exception e) 
            {
                ModEntry.monitor.Log($"Error in ButtonOptions Draw: {e}", LogLevel.Error);
            }
            
            
        }
    }
}


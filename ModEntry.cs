

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SButton = StardewModdingAPI.SButton;

namespace DesertEagle
{
    public class ModEntry : Mod
    {
        private const int MagazineSize = 7;
        private const int ReloadTicks = 90;
        private const int ShotCooldown = 18;

        private int ammo = MagazineSize;
        private int reloadTimer = 0;
        private int shotCooldown = 0;
        private int effectTimer = 0;

        private Vector2 shotStart;
        private Vector2 shotEnd;
        private int facingDirection;

        private Texture2D pixel;

        public override void Entry(IModHelper helper)
        {
            pixel = new Texture2D(
                Game1.graphics.GraphicsDevice, 1, 1
            );
            pixel.SetData(new[] { Color.White });

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;

            Monitor.Log("Desert Eagle loaded!", LogLevel.Info);
        }

        private bool HasDesertEagle()
        {
            return Context.IsWorldReady
                && Game1.player.CurrentItem != null
                && Game1.player.CurrentItem.Name == "Golden Desert Eagle";
        }

        private void OnButtonPressed(
            object sender,
            ButtonPressedEventArgs e)
        {
            if (!HasDesertEagle())
                return;

            if (e.Button == SButton.MouseLeft)
            {
                Helper.Input.Suppress(e.Button);
                Shoot();
            }

            if (e.Button == SButton.R)
            {
                Helper.Input.Suppress(e.Button);
                StartReload();
            }
        }

        private void Shoot()
        {
            if (reloadTimer > 0 || shotCooldown > 0)
                return;

            if (ammo <= 0)
            {
                Game1.playSound("dwop");
                return;
            }

            ammo--;
            shotCooldown = ShotCooldown;
            effectTimer = 8;

            var player = Game1.player;
            facingDirection = player.FacingDirection;

            shotStart = player.Position + new Vector2(32, 32);

            Vector2 direction = facingDirection switch
            {
                0 => new Vector2(0, -1),
                1 => new Vector2(1, 0),
                2 => new Vector2(0, 1),
                3 => new Vector2(-1, 0),
                _ => Vector2.Zero
            };

            shotEnd = shotStart + direction * 320f;

            Game1.playSound("slingshot");
        }

        private void StartReload()
        {
            if (reloadTimer > 0 || ammo == MagazineSize)
                return;

            reloadTimer = ReloadTicks;
            Game1.playSound("dwop");
        }

        private void OnUpdateTicked(
            object sender,
            UpdateTickedEventArgs e)
        {
            if (shotCooldown > 0)
                shotCooldown--;

            if (effectTimer > 0)
                effectTimer--;

            if (reloadTimer > 0)
            {
                reloadTimer--;

                if (reloadTimer == 0)
                {
                    ammo = MagazineSize;
                    Game1.playSound("dwop");
                }
            }
        }

        private void OnRenderedWorld(
            object sender,
            RenderedWorldEventArgs e)
        {
            if (!HasDesertEagle())
                return;

            var spriteBatch = e.SpriteBatch;

            string status = reloadTimer > 0
                ? "RELOADING..."
                : $"DESERT EAGLE  {ammo}/7";

            spriteBatch.DrawString(
                Game1.smallFont,
                status,
                new Vector2(16, 16),
                Color.Gold
            );

            if (effectTimer <= 0)
                return;

            Vector2 start = shotStart -
                new Vector2(Game1.viewport.X, Game1.viewport.Y);

            Vector2 end = shotEnd -
                new Vector2(Game1.viewport.X, Game1.viewport.Y);

            // Визуальная трасса выстрела
            if (facingDirection == 0 || facingDirection == 2)
            {
                spriteBatch.Draw(
                    pixel,
                    new Rectangle(
                        (int)start.X - 2,
                        (int)Math.Min(start.Y, end.Y),
                        4,
                        (int)Math.Abs(end.Y - start.Y)
                    ),
                    Color.OrangeRed * 0.65f
                );
            }
            else
            {
                spriteBatch.Draw(
                    pixel,
                    new Rectangle(
                        (int)Math.Min(start.X, end.X),
                        (int)start.Y - 2,
                        (int)Math.Abs(end.X - start.X),
                        4
                    ),
                    Color.OrangeRed * 0.65f
                );
            }

            // Вспышка у дула
            spriteBatch.Draw(
                pixel,
                new Rectangle(
                    (int)start.X - 7,
                    (int)start.Y - 7,
                    14,
                    14
                ),
                Color.Yellow
            );
        }
    }
}

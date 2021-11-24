using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.TP.Objects;
using Microsoft.Xna.Framework.Media;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System;
using System.Collections.Generic;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NumericVector3 = System.Numerics.Vector3;
namespace TGC.MonoGame.TP
{
    internal class GameRun
    {
        private TGCGame Game;
        private Simulation Simulation { get; set; }
        private BufferPool BufferPool { get; set; }
        private float time;
        private Matrix Projection { get; set; }
        private Matrix View { get; set; }
        public GameRun(TGCGame game)
        {
            Game = game;
            time = 0;
            View = Matrix.CreateLookAt(new Vector3(0,0,0), new Vector3(1,0,0), Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f,
                    1000f);
            
        }


        public void Draw(GameTime gameTime)
        {
            
            Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            
            
            var originalRasterizerState = Game.GraphicsDevice.RasterizerState;
            var rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            Game.GraphicsDevice.RasterizerState = rasterizerState;
            Game.SkyBox.Draw(View, Projection, new Vector3(0,0,0));
            Game.GraphicsDevice.RasterizerState = originalRasterizerState;
            
            
            
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            //Game.ocean.Draw(gameTime, Game.Camera.View, Game.Camera.Projection, Game);
            Game.terrain.Draw(Matrix.Identity, Game.Camera.View, Game.Camera.Projection,(float)gameTime.TotalGameTime.TotalSeconds);
            Game.MainShip.Draw();
            for (int eShip = 0; eShip < Game.CountEnemyShip; eShip++)
            {
                if (Game.EnemyShips[eShip].Life > 0)
                {
                    Game.EnemyShips[eShip].Draw();
                }
            }

            for (int isla = 0; isla < Game.cantIslas; isla++)
            {
                Game.Islas[isla].Draw();
            }
            
            if (Game.Camera.CanShoot)
            {
                Game.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                Game.spriteBatch.Draw(Game.Mira,
                    new Rectangle(Game.GraphicsDevice.Viewport.Width / 2 - 400, Game.GraphicsDevice.Viewport.Height / 2 - 300,
                        800, 600), Color.White);
                Game.spriteBatch.End();
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                Game.GraphicsDevice.BlendState = BlendState.Opaque;
            }
            
            
        }

        public void Update(GameTime gameTime)
        {
            Game.MainShip.Update(gameTime);
            //Game.Camera.SetPosition(Game.MainShip.Position + new Vector3((float)Math.Cos(Game.MainShip.anguloDeGiro+Game.MainShip.anguloInicial),0,(float)Math.Sin(Game.MainShip.anguloDeGiro+Game.MainShip.anguloInicial)));
            Game.Camera.SetPosition(Game.MainShip.Position);
            Game.Camera.Update(gameTime);
            
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Game.GameState = "PAUSE";
                Game.Exit();
            }
        }
    }
}
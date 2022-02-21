using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.TP.Objects;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using TGC.MonoGame.TP.Objects.Water;

namespace TGC.MonoGame.TP
{
    internal class Menu
    {
        private TGCGame Game;
        private Model Barco;
        private SpriteBatch spriteBatch;
        public Texture2D botonesOff;
        public Texture2D botonesOn;
        public Texture2D botonesCurrentPlay;
        public Texture2D botonesCurrentExit;
        private SpriteFont font;
        private Effect BarcoEffect;
        private Matrix Projection { get; set; }
        private Matrix View { get; set; }

        public Song Song { get; set; }
        private SoundEffect soundButtom { get; set; }
        private float KA = 0.3f;

        private float KD = 1f;

        private float KS = 0.5f;

        private float Shininess = 7;
        private Effect Effect;
        private Texture2D TextureShip;
        public Menu(TGCGame game)
        {
            Game = game;
            Barco = Game.Content.Load<Model>(TGCGame.ContentFolder3D + "Barco");
            botonesOff = Game.Content.Load<Texture2D>("Textures/" + "ButtonOff");
            botonesOn = Game.Content.Load<Texture2D>("Textures/" + "ButtonOn");
            TextureShip = Game.Content.Load<Texture2D>(TGCGame.ContentFolderTextures + "BarcoPrincipal2");
            botonesCurrentPlay = botonesOff;
            botonesCurrentExit = botonesOff;
            font = Game.Content.Load<SpriteFont>("SpriteFonts/Text");
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            soundButtom = Game.Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + "Button");
            Song = Game.Content.Load<Song>(TGCGame.ContentFolderMusic + "Menu");
            MediaPlayer.IsRepeating = true;
            View = Matrix.CreateLookAt(new Vector3(0,-200,0), new Vector3(1,0,0), Vector3.Up);
            Projection =
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, Game.GraphicsDevice.Viewport.AspectRatio, 0.1f,
                    8000f);
            Effect = Game.Content.Load<Effect>(TGCGame.ContentFolderEffects + "Ship");
            foreach (var modelMesh in Barco.Meshes)
            foreach (var meshPart in modelMesh.MeshParts)
                meshPart.Effect = Effect;
            Effect.Parameters["ambientColor"]?.SetValue(Game.KAColor);
            Effect.Parameters["diffuseColor"]?.SetValue(Game.KDColor);
            Effect.Parameters["specularColor"]?.SetValue(Game.KSColor);
            Effect.Parameters["baseTexture"]?.SetValue(TextureShip);
            Effect.Parameters["KAmbient"]?.SetValue(0.1f);
            Effect.Parameters["KDiffuse"]?.SetValue(0.7f);
            Effect.Parameters["KSpecular"]?.SetValue(0.2f);
            Effect.Parameters["shininess"]?.SetValue(100f);
        }

        private void DrawShip(Model model, float angle, String NameEffect)
        {
            Effect.CurrentTechnique = Effect.Techniques[NameEffect];
            var dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var tan = new Vector2(-MathF.Sin(angle), MathF.Cos(angle));
            var pos = new Vector2(2000, -100);
            var pos_ade = pos + dir * 100;
            var pos_der = pos + tan * 100;
            var PosAdelante = new Vector3(pos_ade.X, Game.terrain.Height(pos_ade.X, pos_ade.Y), pos_ade.Y);
            var PosDerecha = new Vector3(pos_der.X, Game.terrain.Height(pos_der.X, pos_der.Y), pos_der.Y);
            
            var matWorld = CalcularMatrizOrientacion(0.4f, new Vector3(pos.X, Game.terrain.Height(pos.X, pos.Y) +10, pos.Y), PosAdelante,
                PosDerecha);
            var modelMeshesBaseTransforms = new Matrix[Barco.Bones.Count];
            Barco.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var modelMesh in Barco.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index] * matWorld;
                Effect.Parameters["WorldViewProjectionSun"]?.SetValue(worldMatrix*Game.ViewSun*Game.ProjectionSun);
                // World is used to transform from model space to world space
                Effect.Parameters["World"]?.SetValue(worldMatrix);
                // InverseTransposeWorld is used to rotate normals
                Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // WorldViewProjection is used to transform from model space to clip space
                Effect.Parameters["WorldViewProjection"]?.SetValue(worldMatrix *Game.Camera.View * Game.Camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }
        }

        public void Draw(GameTime gameTime,String text, String NameEffect)
        {
            if (NameEffect == "ShadowMap")
            {
                //554Game.GraphicsDevice.Clear(Color.CornflowerBlue);
                var originalRasterizerState = Game.GraphicsDevice.RasterizerState;
                var rasterizerState = new RasterizerState();
                rasterizerState.CullMode = CullMode.None;
                Game.GraphicsDevice.RasterizerState = rasterizerState;
                Game.SunBox.Draw(Game.Camera.View, Game.Camera.Projection, Game.SunPosition);
                Game.SkyBox.Draw(View, Projection, new Vector3(0, -300, 0));
                Game.GraphicsDevice.RasterizerState = originalRasterizerState;
                Game.terrain.Draw(Matrix.Identity, Game.Camera.View, Game.Camera.Projection,(float)gameTime.TotalGameTime.TotalSeconds, NameEffect);
            }
            if (NameEffect == "EnviromentMap")
            {
                var originalRasterizerState = Game.GraphicsDevice.RasterizerState;
                var rasterizerState = new RasterizerState();
                rasterizerState.CullMode = CullMode.None;
                Game.GraphicsDevice.RasterizerState = rasterizerState;
                Game.SkyBox.Draw(View, Projection, new Vector3(0, -300, 0));
                Game.GraphicsDevice.RasterizerState = originalRasterizerState;
            }
            if (NameEffect != "EnviromentMap")
            {
                DrawShip(Barco, (float)gameTime.TotalGameTime.TotalSeconds, NameEffect);
            }
            if (NameEffect == "ShadowMap"){
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp,
                    DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                spriteBatch.Draw(botonesCurrentPlay,
                    new Rectangle(250, 50,
                        200, 100), Color.White);
                spriteBatch.DrawString(font, text, new Vector2(285, 60), Color.White);
                spriteBatch.Draw(botonesCurrentExit,
                    new Rectangle(800, 50,
                        200, 100), Color.White);
                spriteBatch.DrawString(font, "Exit", new Vector2(850, 60), Color.White);
                spriteBatch.End();
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                Game.GraphicsDevice.BlendState = BlendState.Opaque;
            }
        }

        public void Update(GameTime gameTime)
        {
            Effect.Parameters["lightPosition"]?.SetValue(Game.SunPosition);
            Effect.Parameters["eyePosition"]?.SetValue(Game.Camera.Position);
            var position = Mouse.GetState().Position;
            if (position.X > 800 && position.X < 1000 && position.Y > 50 && position.Y < 150)
            {
                botonesCurrentExit = botonesOn;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    soundButtom.Play();
                    Game.Exit();
                }
            }
            else
            {
                botonesCurrentExit = botonesOff;
            }
            if (position.X > 250 && position.X < 450 && position.Y > 50 && position.Y < 150)
            {
                botonesCurrentPlay = botonesOn;
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    MediaPlayer.Stop();
                    soundButtom.Play();
                    Game.GameState = "PLAY";
                    Game.Camera.Menu = false;
                }
            }
            else
            {
                botonesCurrentPlay = botonesOff;
            }
            Game.Camera.Update(gameTime);
        }
        public Matrix CalcularMatrizOrientacion(float scale, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var matWorld = Matrix.CreateScale(scale * 0.1f);

            // determino la orientacion
            var Dir = p1 - p0;
            Dir.Normalize();
            var Tan = p2 - p0;
            Tan.Normalize();
            var VUP = Vector3.Cross(Tan, Dir);
            VUP.Normalize();
            Tan = Vector3.Cross(VUP, Dir);
            Tan.Normalize();

            var V = VUP;
            var U = Tan;

            var Orientacion = new Matrix();
            Orientacion.M11 = U.X;
            Orientacion.M12 = U.Y;
            Orientacion.M13 = U.Z;
            Orientacion.M14 = 0;

            Orientacion.M21 = V.X;
            Orientacion.M22 = V.Y;
            Orientacion.M23 = V.Z;
            Orientacion.M24 = 0;

            Orientacion.M31 = Dir.X;
            Orientacion.M32 = Dir.Y;
            Orientacion.M33 = Dir.Z;
            Orientacion.M34 = 0;

            Orientacion.M41 = 0;
            Orientacion.M42 = 0;
            Orientacion.M43 = 0;
            Orientacion.M44 = 1;
            matWorld = matWorld * Orientacion;

            // traslado
            matWorld = matWorld * Matrix.CreateTranslation(p0);
            return matWorld;
        }
    }
}
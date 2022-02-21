using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.Samples.Cameras;
using TGC.MonoGame.TP.Objects;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Objects.Water;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal  del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = false;
        }

        private GraphicsDeviceManager Graphics { get; }
        private SpriteBatch SpriteBatch { get; set; }
        
        public Model Rock { get; set; }
        public Vector3 BarcoPositionCenter = new Vector3(-1000f, -10, 0);
        
        public Model[] islands { get; set; }
        public islas[] Islas { get; set; }

        public Vector3[] posicionesIslas;

        public int cantIslas;
        public Matrix World { get; set; }
        public Camera Camera { get; set; }
        
        public MainShip MainShip;

        public List<EnemyShip> EnemyShips;
        public int CountEnemyShip = 10;
        public float ElapsedTime = 0;
        private Song Song { get; set; }
        private string SongName { get; set; }
        
        public SpriteBatch spriteBatch ;
        private const int EnvironmentmapSize = 2048;
        public Texture2D Mira;
        public Texture2D Life;
        public Texture2D Life2;
        private GameRun gameRun;
        private Menu menu;
        public Water2 terrain;
        public SkyBox SkyBox;
        public Effect basicEffect;
        public Texture2D islasTexture;
        public string GameState = "START"; //posibles estados PLAY, RETRY, RESUME, END, PAUSE
        public Vector3 SunPosition = new Vector3(-200f, 1000, 100);
        public SunBox SunBox;
        public Vector2 LimitSpaceGame = new Vector2(5000, 7000);

        public Vector3 KAColor = new Vector3(0, 0, 0.4f);
        //public Vector3 KDColor = new Vector3(0, 0, 0.2f);
        public Vector3 KDColor = new Vector3(1, 1, 1);
        public Vector3 KSColor = new Vector3(1, 1, 1);
        public const int ShadowmapSize = 3048;
        public RenderTarget2D ShadowMapRenderTarget;
        public Matrix ViewSun;
        public Matrix ProjectionSun;
        private StaticCamera CubeMapCamera { get; set; }

        public RenderTargetCube EnvironmentMapRenderTarget { get; set; }
        //public Vector3 SunPosition = new Vector3(0f, 0, 1000000);

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.

            // Apago el backface culling.
            // Esto se hace por un problema en el diseno del modelo del logo de la materia.
            // Una vez que empiecen su juego, esto no es mas necesario y lo pueden sacar.
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            //Graphics.PreferredBackBufferWidth = 1280;
            //Graphics.PreferredBackBufferHeight = 720;
            Graphics.ApplyChanges();
            // Seria hasta aca.

            // Configuramos nuestras matrices de la escena.
            World = Matrix.CreateRotationY(MathHelper.Pi);
            var screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            MainShip = new MainShip(BarcoPositionCenter, new Vector3(0,0,0), 10, this );
            EnemyShips = new List<EnemyShip>();
            for (int eShip = 0; eShip < CountEnemyShip; eShip++)
            {
                EnemyShips.Add(new EnemyShip(new Vector3(600f * (Math.Abs(eShip/2)*2-1), 10f, eShip * 1300 -1300*CountEnemyShip/2), new Vector3(0,0,0),10,this));
            }
            Camera = new BuilderCamaras(GraphicsDevice.Viewport.AspectRatio , screenSize, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, MainShip, GameState == "START");
            gameRun = new GameRun(this);
            menu = new Menu(this);
            posicionesIslas = new[] { 
                new Vector3(2000f, -200f, 0f) , //isla gr
                new Vector3(-2000f, -200f, 0f) ,
                new Vector3(0f, 0f, -2000f) ,
                new Vector3(0f, -200f, 2000f) ,
                new Vector3(0f, -200f, 0f) ,
                new Vector3(-1500f, 0f, -4000f) ,
                new Vector3(1500f, 0f, -4000f) , 
                new Vector3(1500f, 0f, 4000f) , 
                new Vector3(-1500f, 0f, 4000f) };

            cantIslas = posicionesIslas.Length;
            var FrontDirection = Vector3.Normalize(Vector3.Zero - SunPosition);
            var RightDirection = Vector3.Normalize(Vector3.Cross(Vector3.Up, FrontDirection));
            var UpDirection = Vector3.Cross(FrontDirection, RightDirection);
            ViewSun = Matrix.CreateLookAt(SunPosition, SunPosition + FrontDirection, UpDirection);
            var LightCameraNearPlaneDistance = 5f;
            var LightCameraFarPlaneDistance = 3000f;
            ProjectionSun = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2,1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance);
            CubeMapCamera = new StaticCamera(1f, new Vector3(0,0,0), Vector3.UnitX, Vector3.Up);
            CubeMapCamera.BuildProjection(1f, 1f, 3000f, MathHelper.PiOver2);
            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            terrain = new Water2(GraphicsDevice,  this);
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            MainShip.LoadContent();
            for (int eShip = 0; eShip < CountEnemyShip; eShip++)
            {
                EnemyShips[eShip].LoadContent();
            }
            Rock = Content.Load<Model>(ContentFolder3D + "RockSet06-A");
            //islands = new Model[cantIslas];
            Mira = Content.Load<Texture2D>(ContentFolderTextures + "Mira");
            Life = Content.Load<Texture2D>(ContentFolderTextures + "Barra de vida");
            Life2 = Content.Load<Texture2D>(ContentFolderTextures + "Barra de vida 3");
            //basicEffect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");
            islasTexture = Content.Load<Texture2D>(ContentFolderTextures + "stones");
            Islas = new islas[cantIslas];
            for (int isla = 0; isla < cantIslas; isla++)
            {
                Islas[isla] = new islas(posicionesIslas[isla],this,"islands/isla" + (isla + 1),"Ship");
                Islas[isla].LoadContent();
            }


            SongName = "Game";
            Song = Content.Load<Song>(ContentFolderMusic + SongName);
            MediaPlayer.IsRepeating = true;

            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            var skyBox = Content.Load<Model>(ContentFolder3D + "cube");
            var skyBoxTexture = Content.Load<TextureCube>(ContentFolderTextures + "skyboxes/skybox/skybox");
            var skyBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SkyBox");
            var sunBoxEffect = Content.Load<Effect>(ContentFolderEffects + "SunBox");
            SkyBox = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect);
            SunBox = new SunBox(skyBox, sunBoxEffect,100);
            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
            EnvironmentMapRenderTarget = new RenderTargetCube(GraphicsDevice, EnvironmentmapSize, false,
                SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            base.LoadContent();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            ElapsedTime += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            SunPosition = new Vector3(MathF.Cos(ElapsedTime) * 5000f, 2000f, MathF.Sin(ElapsedTime) * 5000f);
            if (GameState == "START" || MainShip.Life <=0)
            {
                if (MediaPlayer.State != MediaState.Playing )
                {
                    MediaPlayer.Play(menu.Song, new TimeSpan(0,0,2));
                }
                IsMouseVisible = true;
                menu.Update(gameTime);
            }

            if (GameState == "PLAY" || GameState == "RESUME")
            {
                if (MediaPlayer.State != MediaState.Playing )
                {
                    MediaPlayer.Play(Song);
                }
                IsMouseVisible = false;
                gameRun.Update(gameTime);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && GameState == "END")
                //Salgo del juego.
                Exit();
            // Basado en el tiempo que paso se va generando una rotacion.
            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            #region Shadow Map
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            if (MainShip.Life <= 0)
                GameState = "GAMEOVER";
                menu.Draw(gameTime,"Retry","DepthMap" );
            if (GameState == "START")
                menu.Draw(gameTime,"Play", "DepthMap");
            if (GameState == "PLAY" || GameState == "RESUME")
                gameRun.Draw(gameTime, "DepthMap");
            #endregion 
            #region Enviroment Map
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            for (var face = CubeMapFace.PositiveX; face <= CubeMapFace.NegativeZ; face++)
            {
                // Set the render target as our cubemap face, we are drawing the scene in this texture
                GraphicsDevice.SetRenderTarget(EnvironmentMapRenderTarget, face);
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

                SetCubemapCameraForOrientation(face);
                CubeMapCamera.BuildView();

                // Draw our scene. Do not draw our tank as it would be occluded by itself 
                // (if it has backface culling on)
                if (MainShip.Life <= 0)
                    GameState = "GAMEOVER";
                menu.Draw(gameTime,"Retry","EnviromentMap" );
                if (GameState == "START")
                    menu.Draw(gameTime,"Play", "EnviromentMap");
                if (GameState == "PLAY" || GameState == "RESUME")
                    gameRun.Draw(gameTime,"EnviromentMap");
            }
            #endregion 
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
            if (MainShip.Life <= 0)
                GameState = "GAMEOVER";
                menu.Draw(gameTime,"Retry","ShadowMap" );
            if (GameState == "START")
                menu.Draw(gameTime,"Play", "ShadowMap");
            if (GameState == "PLAY" || GameState == "RESUME")
                gameRun.Draw(gameTime,"ShadowMap");

        }
        private void SetCubemapCameraForOrientation(CubeMapFace face)
        {
            switch (face)
            {
                default:
                case CubeMapFace.PositiveX:
                    CubeMapCamera.FrontDirection = -Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeX:
                    CubeMapCamera.FrontDirection = Vector3.UnitX;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.PositiveY:
                    CubeMapCamera.FrontDirection = Vector3.Down;
                    CubeMapCamera.UpDirection = Vector3.UnitZ;
                    break;

                case CubeMapFace.NegativeY:
                    CubeMapCamera.FrontDirection = Vector3.Up;
                    CubeMapCamera.UpDirection = -Vector3.UnitZ;
                    break;

                case CubeMapFace.PositiveZ:
                    CubeMapCamera.FrontDirection = -Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;

                case CubeMapFace.NegativeZ:
                    CubeMapCamera.FrontDirection = Vector3.UnitZ;
                    CubeMapCamera.UpDirection = Vector3.Down;
                    break;
            }
        }
        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }
    }
}
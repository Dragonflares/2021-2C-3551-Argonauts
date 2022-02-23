using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using static TGC.MonoGame.TP.Objects.CannonBall;

namespace TGC.MonoGame.TP.Objects
{
    public class EnemyShip
    {
        public Vector3 Position;
        public Vector3 PositionAnterior { get; set; }
        public float speed { get; set; }
        private float maxspeed { get; set; }
        private float maxacceleration { get; set; }
        public Model modelo { get; set; }
        public Vector3 orientacion { get; set; }
        public Vector3 orientacionSobreOla { get; set; }
        public float anguloDeGiro { get; set; }
        public float anguloInicial { get; set; }
        public float giroBase { get; set; }
        private float time;
        private Boolean pressedAccelerator { get; set; }
        private int currentGear { get; set; }
        private Boolean HandBrake { get; set; }
        private Boolean pressedReverse { get; set; }

        private TGCGame _game;

        public string ModelName;
        public string SoundShotName;
        private float CannonBallTime=0;
        private Boolean CanShoot { get; set; }

        private Model cannonBall { get; set; }
        private List<CannonBall> cannonBalls = new List<CannonBall>();

        private SoundEffect soundShot { get; set; }
        private Vector3 StartPositionCannon = new Vector3(0, 42, 80);
        public int Life = 100;
        private SpriteFont SpriteFont;
        private float initialScale;
        private float Kspecular;
        public OrientedBoundingBox ShipBox { get; set; }
        private bool Active;
        private Effect Effect;
        private String ModelTexture;
        private Texture2D texture;
        public EnemyShip(Vector3 initialPosition, Vector3 currentOrientation, float MaxSpeed, TGCGame game)
        {
            var rnd = new Random();
            var BarcoIndex = rnd.Next(2);
            if (BarcoIndex == 0)
            {
                //Barco azul
                ModelName = "Barco2/Barco2";
                initialScale = 1 ;
                anguloInicial = 0;
                ModelTexture = "Barco2";
                Kspecular = 1f;

            }
            else
            {   //Barco rojo
                ModelName = "Barco3";
                initialScale = 1 ;
                anguloInicial = (float)Math.PI/2;;
                ModelTexture = "Barco2";
                Kspecular = 0.1f;

            }
            speed = 0;
            Position = initialPosition;
            PositionAnterior = Position;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.1f;
            anguloDeGiro = 0f;
            giroBase = 0.003f;
            pressedAccelerator = false;
            currentGear = 0;
            HandBrake = false;
            pressedReverse = false;
            
            SoundShotName = "Shot";
            _game = game;
            SpriteFont = _game.Content.Load<SpriteFont>("SpriteFonts/Life");
        }

        public void clearVariable(Vector3 initialPosition, Vector3 currentOrientation, float MaxSpeed)
        {
            speed = 0;
            Position = initialPosition;
            PositionAnterior = Position;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.1f;
            anguloDeGiro = 0f;
            giroBase = 0.003f;
            pressedAccelerator = false;
            currentGear = 0;
            HandBrake = false;
            pressedReverse = false;
            Life = 100;
        }

        public void LoadContent()
        {
            modelo = _game.Content.Load<Model>(TGCGame.ContentFolder3D + ModelName);
            soundShot = _game.Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + SoundShotName);
            cannonBall = _game.Content.Load<Model>(TGCGame.ContentFolder3D + "sphere");
            texture = _game.Content.Load<Texture2D>(TGCGame.ContentFolderTextures + ModelTexture);
            
            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(modelo);
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, initialScale);
            // Create an Oriented Bounding Box from the AABB
            ShipBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            // Move the center
            ShipBox.Center = Position;
            // Then set its orientation!
            ShipBox.Orientation = Matrix.CreateRotationY(anguloInicial);
            
            Effect = _game.Content.Load<Effect>(TGCGame.ContentFolderEffects + "Ship");
            foreach (var modelMesh in modelo.Meshes)
            foreach (var meshPart in modelMesh.MeshParts)
                meshPart.Effect = Effect;

            Effect.Parameters["ambientColor"]?.SetValue(_game.KAColor);
            Effect.Parameters["diffuseColor"]?.SetValue(_game.KDColor);
            Effect.Parameters["specularColor"]?.SetValue(_game.KSColor);
            
            
        }
        private void DrawShip()
        {

            Position.Y = _game.terrain.Height(Position.X, Position.Z) + 10;
            var matWorld = Matrix.CreateRotationY(anguloInicial)*Matrix.CreateScale(initialScale)*
                           CalcularMatrizOrientacion( Position)
                           *Matrix.CreateTranslation(Position);
            
            var modelMeshesBaseTransforms = new Matrix[modelo.Bones.Count];
            modelo.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            Effect.Parameters["baseTexture"]?.SetValue(texture);
            Effect.Parameters["KAmbient"]?.SetValue(1f);
            Effect.Parameters["KDiffuse"]?.SetValue(1f);
            Effect.Parameters["KSpecular"]?.SetValue(Kspecular);
            Effect.Parameters["shininess"]?.SetValue(0.5f);
            foreach (var modelMesh in modelo.Meshes)
            {
                // We set the main matrices for each mesh to draw
                var worldMatrix = modelMeshesBaseTransforms[modelMesh.ParentBone.Index] * matWorld;
                // World is used to transform from model space to world space
                Effect.Parameters["World"].SetValue(worldMatrix);
                // InverseTransposeWorld is used to rotate normals
                Effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(worldMatrix)));
                // WorldViewProjection is used to transform from model space to clip space
                Effect.Parameters["WorldViewProjection"].SetValue(worldMatrix *_game.Camera.View * _game.Camera.Projection);

                // Once we set these matrices we draw
                modelMesh.Draw();
            }
        }
        public Matrix CalcularMatrizOrientacion(Vector3 p0)
        {
            var dir = new Vector2(MathF.Sin(anguloDeGiro), MathF.Cos(anguloDeGiro));
            var tan = new Vector2(-MathF.Cos(anguloDeGiro), MathF.Sin(anguloDeGiro));
            var pos = new Vector2(Position.X,  Position.Z);
            var pos_ade = pos + dir * 100;
            var pos_der = pos + tan * 100;
            var p1 = new Vector3(pos_ade.X, _game.terrain.Height(pos_ade.X, pos_ade.Y), pos_ade.Y);
            var p2 = new Vector3(pos_der.X, _game.terrain.Height(pos_der.X, pos_der.Y), pos_der.Y);

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
            return Orientacion;
        }

        public void Draw()
        {
            //modelo.Draw(
            //    Matrix.CreateRotationY(anguloDeGiro + anguloInicial) * Matrix.CreateScale(initialScale) *
            //    Matrix.CreateTranslation(Position), _game.Camera.View, _game.Camera.Projection);
            DrawShip();
            foreach (var cannon in cannonBalls)
            {
                if (cannon.Active)
                {
                    cannon.Draw();
                }
            }
        }

        public void Shoted()
        {
            Life-=10;
            if (Life<=0)
            Position.Y = -1000000000000;
        }
        public void Update(GameTime gameTime)
        {
            Position.Y = _game.terrain.Height(Position.X, Position.Z) + 10;
            Effect.Parameters["lightPosition"]?.SetValue(_game.SunPosition);
            Effect.Parameters["eyePosition"]?.SetValue(_game.Camera.Position);
            var shipsDistanceMin = 10000000000000f;
            var positionEnd = Position;
            if ((_game.MainShip.Position - Position).Length() < shipsDistanceMin)
            {
                shipsDistanceMin = (_game.MainShip.Position - Position).Length();
                positionEnd = _game.MainShip.Position;
            }
            foreach (var ship in  _game.EnemyShips)
            {
                if (ship!= this &&
                    (ship.Position - Position).Length() < shipsDistanceMin)
                {
                    shipsDistanceMin = (ship.Position - Position).Length();
                    positionEnd = ship.Position;
                }
            }

            if (shipsDistanceMin > 300)
            {
                MoveTo(positionEnd);
            }
            else
            {
                speed = 0;
                currentGear = 0;
            }
            UpdateMovementSpeed(gameTime);
            Move();
            ShipBox.Center = Position;
            ShipBox.Orientation = Matrix.CreateRotationY(anguloInicial) * CalcularMatrizOrientacion(Position);
            for (int ship = 0; ship < _game.CountEnemyShip; ship++)
                if (ShipBox.Intersects(_game.EnemyShips[ship].ShipBox)&& ShipBox !=_game.EnemyShips[ship].ShipBox)
                {
                    ShipBox.Center = PositionAnterior;
                    Position = PositionAnterior;
                    currentGear = 0;
                    HandBrake = false;
                    pressedReverse = false;
                    pressedAccelerator = false;
                    speed = 0;
                    break;
                }
            for (int isla = 0; isla < _game.cantIslas; isla++)
                if (ShipBox.Intersects(_game.Islas[isla].IslasBox))
                {
                    ShipBox.Center = PositionAnterior;
                    Position = PositionAnterior;
                    currentGear = 0;
                    HandBrake = false;
                    pressedReverse = false;
                    pressedAccelerator = false;
                    speed = 0;
                    break;
                }

            Position.X = Math.Min(Position.X,_game.LimitSpaceGame.X);
            Position.Z = Math.Min(Position.Z,_game.LimitSpaceGame.Y);
            Position.X = Math.Max(Position.X,-_game.LimitSpaceGame.X);
            Position.Z = Math.Max(Position.Z,-_game.LimitSpaceGame.Y);
            PositionAnterior = Position;
            List<CannonBall> removeCannonBalls = new List<CannonBall>();
            foreach (var cannon in cannonBalls)
            {
                if (cannon.Active)
                cannon.Update(gameTime);
                else
                {
                    removeCannonBalls.Add(cannon);
                }
            }

            foreach (var cannon in removeCannonBalls)
            {
                cannonBalls.Remove(cannon);
            }

            if (shipsDistanceMin < 2000 && CannonBallTime<=0)
            {
                var random = new Random();
                var CannonEnd = positionEnd;
                if (random.Next(0, 5) == 4)
                {
                    CannonEnd += new Vector3(500, 0, 0);
                }

                CannonBallTime = 3;
                cannonBalls.Add(new CannonBall(StartPositionCannon+Position,CannonEnd ,_game,cannonBall, null,this));;
            }
            else
            {
                CannonBallTime -= (float) gameTime.ElapsedGameTime.TotalSeconds;
            }
            /*
            ProcessKeyboard(_game.ElapsedTime);
            ProcessMouse(gameTime);
            UpdateMovementSpeed(gameTime);
            Move();*/
        }

        private void MoveTo(Vector3 position)
        {
            var Vectordestino = new Vector2(position.X-Position.X, position.Z-Position.Z);
            var angulodestino = Math.Atan(  Vectordestino.Y/Vectordestino.X);
            if (angulodestino > Math.PI / 2)
            {
                pressedReverse = true;
                angulodestino = angulodestino - Math.PI ;
            }
            else
            {
                if (angulodestino < -Math.PI / 2)
                {
                    pressedReverse = true;
                    angulodestino = Math.PI - angulodestino;
                }
            }
            if (speed != 0)
            {
                anguloDeGiro += (float)Math.Min(0.003f, angulodestino - anguloDeGiro);
            }

            if (pressedReverse)
            {
                currentGear = Math.Min(currentGear - 1, -2);
            }
            else
            {
                currentGear = Math.Max(currentGear + 1, 3);
            }
        }
        
        
        public void Move()
        {
            var newOrientacion = new Vector3((float) Math.Sin(anguloDeGiro), 0, (float) Math.Cos(anguloDeGiro));
            orientacion = newOrientacion;

            //TODO improve wave speed modification
            //var extraSpeed = 10;
            var extraSpeed = 0;
            if (speed <= float.Epsilon) extraSpeed = 0; //Asi no se lo lleva el agua cuando esta parado
            var speedMod = speed + extraSpeed * -Vector3.Dot(orientacionSobreOla, Vector3.Up);

            Position += orientacion * speed;
            if (PositionAnterior.Y < Position.Y)
            {
                Position -= orientacion * speed * (0.25f * (Position.Y - PositionAnterior.Y));
            }
            else
            {
                if (PositionAnterior.Y > Position.Y)
                {
                    Position += orientacion * speed * (0.25f * (PositionAnterior.Y - Position.Y));
                }
            }
        }

        private void UpdateMovementSpeed(GameTime gameTime)
        {
            float acceleration;
            time = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            if (HandBrake) acceleration = maxacceleration;
            else acceleration = maxacceleration * 8 * time;
            float GearMaxSpeed = (maxspeed * currentGear / 3);
            if (speed > GearMaxSpeed)
            {
                if (speed - acceleration < GearMaxSpeed)
                {
                    speed = GearMaxSpeed;
                }
                else
                {
                    speed -= acceleration;
                }
            }
            else if (speed < GearMaxSpeed)
            {
                if (speed + acceleration > GearMaxSpeed)
                {
                    speed = GearMaxSpeed;
                }
                else
                {
                    speed += acceleration;
                }
            }
        }

        private void ProcessMouse(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (mouseState.RightButton.Equals(ButtonState.Pressed) && CanShoot && _game.Camera.CanShoot)
            {
                CanShoot = false;
                soundShot.Play();
                var normal = (_game.Camera.LookAt - _game.Camera.Position);
                normal.Normalize();
                var aux = (float) 0;
                if (normal.Y >= 0)
                {
                    aux = (float) 10000000;
                }
                else
                {
                    aux = (float) -_game.Camera.Position.Y / normal.Y;
                }

                var endPosition = aux * normal + _game.Camera.Position;
                cannonBalls.Add(new CannonBall(StartPositionCannon + Position, endPosition, _game, cannonBall, null, this));

            }

            if (!mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                CanShoot = true;
            }
        }
    }
}
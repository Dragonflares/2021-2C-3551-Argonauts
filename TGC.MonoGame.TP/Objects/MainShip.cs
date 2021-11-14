﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using static TGC.MonoGame.TP.Objects.CannonBall;

namespace TGC.MonoGame.TP.Objects
{
    public class MainShip
    {
        public Vector3 Position { get; set; }
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
        private Boolean CanShoot { get; set; }
        
        private Model cannonBall { get; set; }
        private List <CannonBall> cannonBalls= new List <CannonBall>();

        private SoundEffect soundShot { get; set; }
        private Vector3 StartPositionCannon = new Vector3(0, 42, 80);
        private int Life = 100;
        private SpriteFont SpriteFont;
        public OrientedBoundingBox ShipBox { get; set; }
        private bool AreAABBsTouching { get; set; }
        private float initialScale;
        //private Vector3 PositionAnterior;
        public MainShip(Vector3 initialPosition, Vector3 currentOrientation, float MaxSpeed, TGCGame game)
        {
            speed = 0;
            Position = initialPosition;
            PositionAnterior = Position;
            orientacion = currentOrientation;
            maxspeed = MaxSpeed;
            maxacceleration = 0.1f;
            anguloDeGiro = 0f;
            anguloInicial = (float) (Math.PI/2);
            giroBase = 0.003f;
            pressedAccelerator = false;
            currentGear = 0;
            HandBrake = false;
            pressedReverse = false;
            ModelName = "Barco";
            SoundShotName = "Shot";
            _game = game;
            SpriteFont = _game.Content.Load<SpriteFont>("SpriteFonts/Life");
            initialScale = 0.01f;
        }

        public void LoadContent()
        {
            modelo = _game.Content.Load<Model>(TGCGame.ContentFolder3D + ModelName);
            soundShot = _game.Content.Load<SoundEffect>(TGCGame.ContentFolderSounds + SoundShotName);
            cannonBall = _game.Content.Load<Model>(TGCGame.ContentFolder3D + "sphere");
            //RobotTwoBox = new BoundingBox(RobotOneBox.Min + RobotTwoPosition, RobotOneBox.Max + RobotTwoPosition);
            
            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(modelo);
            // Scale it to match the model's transform
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, initialScale);
            // Create an Oriented Bounding Box from the AABB
            ShipBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            // Move the center
            ShipBox.Center = Position;
            // Then set its orientation!
            ShipBox.Orientation = Matrix.CreateRotationY(anguloInicial);
            
        }

        public void Draw()
        {
            _game.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.Default, RasterizerState.CullCounterClockwise);
            _game.spriteBatch.DrawString(SpriteFont, Life.ToString(), new Vector2(_game.GraphicsDevice.Viewport.Width - 110, 50), Color.White);
            _game.spriteBatch.Draw(_game.Life,
                new Rectangle(_game.GraphicsDevice.Viewport.Width - 210, 10,
                    200, 30), Color.White);
            _game.spriteBatch.Draw(_game.Life2,
                new Rectangle(_game.GraphicsDevice.Viewport.Width - 210, 10,
                    Life *2, 30), Color.Green);
            _game.spriteBatch.End();
            _game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            _game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            _game.GraphicsDevice.BlendState = BlendState.Opaque;
            modelo.Draw(
                Matrix.CreateRotationY(anguloDeGiro + anguloInicial) * Matrix.CreateScale(initialScale) *
                Matrix.CreateTranslation(Position), _game.Camera.View, _game.Camera.Projection);
            foreach (var cannon in cannonBalls)
            {
                cannon.Draw();
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var cannon in cannonBalls)
            {
                cannon.Update(gameTime);
            }

            ProcessKeyboard(_game.ElapsedTime);
            ProcessMouse(gameTime);
            UpdateMovementSpeed(gameTime);
            Move();
            
            ShipBox.Center = Position;
            for (int ship = 0; ship < _game.CountEnemyShip; ship++)
                    if (ShipBox.Intersects(_game.EnemyShips[ship].ShipBox))
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
                
            PositionAnterior = Position;
        }

        public void Move()
        {
            var newOrientacion = new Vector3((float)Math.Sin(anguloDeGiro), 0, (float)Math.Cos(anguloDeGiro));
            orientacion = newOrientacion;

            //TODO improve wave speed modification
            //var extraSpeed = 10;
            var extraSpeed=0;
            if (speed <= float.Epsilon) extraSpeed = 0; //Asi no se lo lleva el agua cuando esta parado
            var speedMod = speed + extraSpeed * -Vector3.Dot(orientacionSobreOla, Vector3.Up);
            
            Position += orientacion*speed ;
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
                    aux =(float) 10000000;
                }
                else
                {
                    aux = (float)-_game.Camera.Position.Y / normal.Y;
                }
                var endPosition = aux * normal + _game.Camera.Position;
                cannonBalls.Add(new CannonBall(StartPositionCannon+Position, endPosition,_game,cannonBall, this,null));
                
            }

            if (!mouseState.RightButton.Equals(ButtonState.Pressed))
            {
                CanShoot = true;
            }
        }

        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.D))
            {
                if (speed == 0)
                {
                }
                else
                {
                    if (anguloDeGiro + giroBase >= MathF.PI * 2)
                    {
                        anguloDeGiro +=  giroBase - MathF.PI * 2;
                    }
                    else
                    {
                        anguloDeGiro -= giroBase;
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (speed == 0)
                {
                }
                else
                {
                    if (anguloDeGiro + giroBase < 0)
                    {
                        anguloDeGiro += - giroBase + MathF.PI * 2;
                    }
                    else
                    {
                        anguloDeGiro += giroBase;
                    }
                }
            }

            if (this.pressedAccelerator == false && keyboardState.IsKeyDown(Keys.W) && currentGear < 3)
            {
                currentGear++;
                pressedAccelerator = true;
                if (HandBrake) HandBrake = false;
            }

            if (this.pressedAccelerator == true && keyboardState.IsKeyUp(Keys.W))
            {
                pressedAccelerator = false;
            }

            if (this.pressedReverse == false && keyboardState.IsKeyDown(Keys.S) && currentGear > -2)
            {
                currentGear--;
                pressedReverse = true;
                if (HandBrake) HandBrake = false;
            }

            if (this.pressedReverse == true && keyboardState.IsKeyUp(Keys.S))
            {
                pressedReverse = false;
            }

            if (HandBrake == false && keyboardState.IsKeyDown(Keys.Space))
            {
                HandBrake = true;
                currentGear = 0;
            }
        }

        public void Shoted()
        {
            Life--;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Objects;

namespace TGC.MonoGame.Samples.Cameras
{
    /// <summary>
    ///     The minimum behavior that a camera should have.
    /// </summary>
    internal class BuilderCamaras : Camera
    {
        public const float DefaultFieldOfViewDegrees = MathHelper.PiOver4;
        public const float DefaultNearPlaneDistance = 0.1f;
        public const float DefaultFarPlaneDistance = 200000000;
        //private static readonly Vector3 Position = new Vector3(-350, 50, 400);
        private Vector3 PositionBarco { get; set; }
        private Vector3 CenterPosition = new Vector3(0, 242,62);
        private Vector3 FrontPosition = new Vector3(0, 240,800);
        private static float Speed = 5;
        private static readonly Vector3 FromDirectionTarget = new Vector3(-350, 1000, 500);
        private static readonly Vector3 FromDirectionStatic = new Vector3(-200f, 15000, 0);
        private List<Camera> Cameras { get; set; }
        private Camera CurrentCamera { get; set; }
        public MainShip MainShip { get; set; }

        private float AspectRatio;
        public BuilderCamaras(float aspectRatio, Point screenCenter,float width, float height, MainShip BarcoPositionCenter, Boolean menu) : this(aspectRatio,screenCenter, width,height,BarcoPositionCenter, DefaultNearPlaneDistance, DefaultFarPlaneDistance, menu)
        {
        }

        public BuilderCamaras(float aspectRatio, Point screenCenter,float width, float height,MainShip BarcoPositionCenter, float nearPlaneDistance, float farPlaneDistance, Boolean menu) : this(aspectRatio,screenCenter, width,height, BarcoPositionCenter,
            nearPlaneDistance, farPlaneDistance, DefaultFieldOfViewDegrees, menu)
        {
        }

        public BuilderCamaras(float aspectRatio, Point screenCenter,float width, float height, MainShip BarcoPositionCenter, float nearPlaneDistance, float farPlaneDistance, float fieldOfViewDegrees, Boolean menu) : base(aspectRatio)
        {
            
            PositionBarco = BarcoPositionCenter.Position;
            MainShip = BarcoPositionCenter;
            AspectRatio = aspectRatio;
            Cameras = new List<Camera>()
            {
                //new FreeCamera(aspectRatio, Position+CenterPosition, screenCenter),
                new SimpleCamera(aspectRatio,PositionBarco+FrontPosition,Speed),
                new SimpleCamera(aspectRatio,PositionBarco+CenterPosition,Speed),
                new StaticCamera(aspectRatio, FromDirectionStatic, new Vector3(0,-950,0),new Vector3(1,1,0)), //Revisar para que quede para abajo mostrando todo el mapa
                new TargetCamera(aspectRatio, new Vector3(PositionBarco.X, PositionBarco.Y + 150, PositionBarco.Z - 250), PositionBarco, screenCenter, height, width),
                new StaticCamera(aspectRatio, new Vector3(0,0,0), new Vector3(1,0,0),Vector3.Up) // unicamente para menu
            };
            CurrentCamera = Cameras[0];
            CanShoot = CurrentCamera.CanShoot;
            Menu = menu;
        }

        public override void Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            if (!Menu)
            {
                
                if (keyboardState.IsKeyDown(Keys.D1))
                {
                    CurrentCamera = Cameras[0];
                }
                else
                {
                    if (keyboardState.IsKeyDown(Keys.D2))
                    {
                        CurrentCamera = Cameras[1];
                    }
                    else
                    {
                        if (keyboardState.IsKeyDown(Keys.D3))
                        {
                            CurrentCamera = Cameras[2];
                        }
                        else
                        {
                            if (keyboardState.IsKeyDown(Keys.D4))
                            {
                                CurrentCamera = Cameras[3];
                            }
                        }
                    }
                }

                if (CurrentCamera == Cameras[4])
                {
                    CurrentCamera = Cameras[0];
                }
            }
            else
            {
                CurrentCamera = Cameras[4];
            }

            CurrentCamera.Update(gameTime);
            Cameras[0].Position = MainShip.Position + CenterPosition;
            Cameras[1].Position = MainShip.Position + FrontPosition;
            Cameras[2].FrontDirection = -(FromDirectionStatic - MainShip.Position);
            Cameras[3].SetPosition(MainShip.Position);
            CanShoot = CurrentCamera.CanShoot;
            Position = CurrentCamera.Position;
            LookAt = CurrentCamera.LookAt;
            View = CurrentCamera.View;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, AspectRatio, 1, 1000000);;
        }

        public override void SetPosition(Vector3 position)
        {
            if (CurrentCamera == Cameras[0])
            {
                CurrentCamera.SetPosition(position+new Vector3(0,CenterPosition.Y,0) + (CenterPosition.X+ CenterPosition.Z)* new Vector3((float)Math.Sin(MainShip.anguloDeGiro),0,(float)Math.Cos(MainShip.anguloDeGiro)));
            }
            else
            {
                if (CurrentCamera == Cameras[1])
                {
                    CurrentCamera.SetPosition(position+new Vector3(0,FrontPosition.Y,0)+(FrontPosition.X+FrontPosition.Z) * new Vector3((float)Math.Sin(MainShip.anguloDeGiro),0,(float)Math.Cos(MainShip.anguloDeGiro)));
                }
                else
                {
                    CurrentCamera.SetPosition(position);
                }
            }
            
        }

        public override Matrix GetView()
        {
            return CurrentCamera.View;
        }
        public override Matrix GetProjection()
        {
            return CurrentCamera.Projection;
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using static TGC.MonoGame.TP.Objects.CannonBall;

namespace TGC.MonoGame.TP.Objects
{
    public class islas
    {
        private Vector3 Position;
        private TGCGame _game;
        private String ModelName;
        private Model modelo;
        private int initialScale;
        public OrientedBoundingBox IslasBox { get; set; }
        private String effectName;
        private Effect effect;

        public islas(Vector3 initialPosition, TGCGame game, String modelName, String effectName)
        {
            Position = initialPosition;
            _game = game;
            ModelName = modelName;
            initialScale = 50;
            this.effectName = effectName;
        }

        public void LoadContent()
        {
            modelo = _game.Content.Load<Model>(TGCGame.ContentFolder3D + ModelName);
            effect = _game.Content.Load<Effect>(TGCGame.ContentFolderEffects + effectName);
            var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(modelo);
            temporaryCubeAABB = BoundingVolumesExtensions.Scale(temporaryCubeAABB, initialScale);
            // Create an Oriented Bounding Box from the AABB
            IslasBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
            // Move the center
            IslasBox.Center = Position;
        }

        public void Draw()
        {
            var World =  Matrix.CreateRotationX(-(float)Math.PI/2)*Matrix.CreateScale(50f) *
                         Matrix.CreateTranslation(Position);
            foreach (var mesh in modelo.Meshes)
            {
                foreach (var part in mesh.MeshParts)
                {
                    part.Effect = effect;
                    effect.Parameters["World"].SetValue(World);
                    effect.Parameters["View"].SetValue(_game.Camera.View);
                    effect.Parameters["Projection"].SetValue(_game.Camera.Projection);
                    effect.Parameters["ModelTexture"].SetValue(_game.islasTexture);
                }
                mesh.Draw();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
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
            foreach (var modelMesh in modelo.Meshes)
            foreach (var meshPart in modelMesh.MeshParts)
                meshPart.Effect = effect;
        }

        public void Draw(String NameEffect )
        {
            var World =  Matrix.CreateRotationX(-(float)Math.PI/2)*Matrix.CreateScale(50f) *
                         Matrix.CreateTranslation(Position);
            effect.CurrentTechnique = effect.Techniques[NameEffect];
            effect.Parameters["ambientColor"]?.SetValue(_game.KAColor);
            effect.Parameters["diffuseColor"]?.SetValue(_game.KDColor);
            effect.Parameters["specularColor"]?.SetValue(_game.KSColor);
            effect.Parameters["KAmbient"]?.SetValue(0.1f);
            effect.Parameters["KDiffuse"]?.SetValue(1f);
            effect.Parameters["KSpecular"]?.SetValue(1f);
            effect.Parameters["shininess"]?.SetValue(0.6f);
            effect.Parameters["baseTexture"]?.SetValue(_game.islasTexture);
            effect.Parameters["lightPosition"]?.SetValue(_game.SunPosition);
            effect.Parameters["eyePosition"]?.SetValue(_game.Camera.Position);
            foreach (var mesh in modelo.Meshes)
            {
                // World is used to transform from model space to world space
                effect.Parameters["World"].SetValue(World);
                // InverseTransposeWorld is used to rotate normals
                effect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(World)));
                // WorldViewProjection is used to transform from model space to clip space
                effect.Parameters["WorldViewProjection"].SetValue(World *_game.Camera.View * _game.Camera.Projection);

                // Once we set these matrices we draw
                mesh.Draw();
            }
        }
    }
}
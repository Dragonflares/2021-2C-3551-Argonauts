﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TGC.MonoGame.TP
{
    /// <summary>
    /// A City Scene to be drawn
    /// </summary>
    public class Water
    {
        public const float WaterGrid = 5000f;
        private Model Model { get; set; }
        private List<Matrix> WorldWaterMatrix{ get; set; }
        private Effect Effect { get; set; }
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderTextures = "Textures/";
        private float time;
        private Texture texturaAgua { get; set; }

        /// <summary>
        /// Creates a City Scene with a content manager to load resources.
        /// </summary>
        /// <param name="content">The Content Manager to load resources</param>
        public Water(ContentManager content)
        {

            Model = content.Load<Model>(ContentFolder3D + "ocean");

            // Load an effect that will be used to draw the scene
            Effect = content.Load<Effect>(ContentFolderEffects + "waterShader");
            
            foreach (var mesh in Model.Meshes)
            {
                foreach (var meshPart in mesh.MeshParts)
                    meshPart.Effect = Effect;
            }
            texturaAgua = content.Load<Texture>(ContentFolderTextures + "Ocean2");
            WorldWaterMatrix = new List<Matrix>()
            {
                Matrix.Identity,
                Matrix.CreateTranslation(Vector3.Right * WaterGrid),
                Matrix.CreateTranslation(Vector3.Left * WaterGrid),
                Matrix.CreateTranslation(Vector3.Forward * WaterGrid),
                Matrix.CreateTranslation(Vector3.Backward * WaterGrid),
                Matrix.CreateTranslation((Vector3.Forward + Vector3.Right) * WaterGrid),
                Matrix.CreateTranslation((Vector3.Forward + Vector3.Left) * WaterGrid),
                Matrix.CreateTranslation((Vector3.Backward + Vector3.Right) * WaterGrid),
                Matrix.CreateTranslation((Vector3.Backward + Vector3.Left) * WaterGrid),
            };

        }

        public void Draw(GameTime gameTime, Matrix view, Matrix projection, TGCGame game)
        {
            time += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            Effect.Parameters["View"].SetValue(view);
            Effect.Parameters["Projection"].SetValue(projection);
            Effect.Parameters["World"].SetValue(game.World * Matrix.CreateScale(50.0f) * Matrix.CreateTranslation(0, -60f, 0));
            Effect.Parameters["Time"]?.SetValue(time);
            Effect.Parameters["baseTexture"].SetValue(texturaAgua);
            Effect.Parameters["foamTexture"]?.SetValue(texturaAgua);
            Effect.Parameters["environmentMap"]?.SetValue(texturaAgua);
            Effect.Parameters["normalTexture"]?.SetValue(texturaAgua);
            Effect.Parameters["sunPosition"]?.SetValue(new Vector3(-200f, 10000, 500));
            Effect.Parameters["cameraPosition"]?.SetValue(game.Camera.Position);
            Effect.Parameters["InverseTransposeWorld"]?.SetValue(Matrix.Invert(Matrix.Transpose(game.World * Matrix.CreateScale(50.0f) * Matrix.CreateTranslation(0, -60f, 0))));
            Effect.Parameters["KAmbient"]?.SetValue(0.4f);
            Effect.Parameters["ambientColor"]?.SetValue(new Vector3(0,0,1));
            Effect.Parameters["KDiffuse"]?.SetValue(0.2f);
            Effect.Parameters["diffuseColor"]?.SetValue(new Vector3(0,0,1));
            Effect.Parameters["KSpecular"]?.SetValue(0f);
            Effect.Parameters["shininess"]?.SetValue(0f);
            Effect.Parameters["KReflection"]?.SetValue(0.5f);
            Effect.Parameters["KFoam"]?.SetValue(0f);
            var modelMeshesBaseTransforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(modelMeshesBaseTransforms);
            foreach (var mesh in Model.Meshes)
            {
                var meshWorld = modelMeshesBaseTransforms[mesh.ParentBone.Index];
                foreach (var waterMatrix in WorldWaterMatrix)
                {
                    Effect.Parameters["World"].SetValue(meshWorld * waterMatrix);
                    mesh.Draw();
                }
            }

        }
    }
}

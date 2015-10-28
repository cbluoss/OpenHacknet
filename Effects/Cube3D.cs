// Decompiled with JetBrains decompiler
// Type: Hacknet.Effects.Cube3D
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet.Effects
{
    internal class Cube3D
    {
        private const int NUM_VERTICES = 36;
        private const int NUM_INDICIES = 14;
        private static VertexPositionNormalTexture[] verts;
        private static VertexBuffer vBuffer;
        private static IndexBuffer ib;
        private static BasicEffect wireframeEfect;
        private static RasterizerState wireframeRaster;

        public static void Initilize(GraphicsDevice gd)
        {
            ConstructCube();
            vBuffer = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, 36, BufferUsage.WriteOnly);
            vBuffer.SetData(verts);
            gd.SetVertexBuffer(vBuffer);
            ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, 14, BufferUsage.WriteOnly);
            ib.SetData(new short[14]
            {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9,
                10,
                11,
                12,
                13
            });
            gd.Indices = ib;
            wireframeRaster = new RasterizerState();
            wireframeRaster.FillMode = FillMode.WireFrame;
            wireframeRaster.CullMode = CullMode.None;
            wireframeEfect = new BasicEffect(gd);
            wireframeEfect.Projection = Matrix.CreatePerspectiveFieldOfView(0.7853982f,
                wireframeEfect.GraphicsDevice.Viewport.AspectRatio, 0.01f, 3000f);
        }

        private static void ResetBuffers()
        {
            var graphicsDevice = wireframeEfect.GraphicsDevice;

            graphicsDevice.SetVertexBuffer(vBuffer);
            graphicsDevice.Indices = ib;
        }

        public static void RenderWireframe(Vector3 position, float scale, Vector3 rotation, Color color)
        {
            RenderWireframe(position, scale, rotation, color, new Vector3(0.0f, 0.0f, 20f));
        }

        public static void RenderWireframe(Vector3 position, float scale, Vector3 rotation, Color color,
            Vector3 cameraOffset)
        {
            scale = Math.Max(1.0f/1000.0f, scale);
            wireframeEfect.DiffuseColor = Utils.ColorToVec3(color);
            wireframeEfect.GraphicsDevice.BlendState = BlendState.Opaque;
            wireframeEfect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            wireframeEfect.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            ResetBuffers();
            var rasterizerState = wireframeEfect.GraphicsDevice.RasterizerState;
            wireframeEfect.GraphicsDevice.RasterizerState = wireframeRaster;
            var matrix = Matrix.CreateTranslation(-new Vector3(0.0f, 0.0f, 0.0f))*Matrix.CreateScale(scale)*
                         Matrix.CreateRotationY(rotation.Y)*Matrix.CreateRotationX(rotation.X)*
                         Matrix.CreateRotationZ(rotation.Z)*Matrix.CreateTranslation(position);
            wireframeEfect.World = matrix;
            wireframeEfect.View = Matrix.CreateLookAt(cameraOffset, position, Vector3.Up);
            try
            {
                foreach (var effectPass in wireframeEfect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    wireframeEfect.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 36);
                }
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Not supported happened");
            }
            wireframeEfect.GraphicsDevice.RasterizerState = rasterizerState;
        }

        private static void ConstructCube()
        {
            verts = new VertexPositionNormalTexture[36];
            var position1 = new Vector3(-1f, 1f, -1f);
            var position2 = new Vector3(-1f, 1f, 1f);
            var position3 = new Vector3(1f, 1f, -1f);
            var position4 = new Vector3(1f, 1f, 1f);
            var position5 = new Vector3(-1f, -1f, -1f);
            var position6 = new Vector3(-1f, -1f, 1f);
            var position7 = new Vector3(1f, -1f, -1f);
            var position8 = new Vector3(1f, -1f, 1f);
            var normal1 = new Vector3(0.0f, 0.0f, 1f);
            var normal2 = new Vector3(0.0f, 0.0f, -1f);
            var normal3 = new Vector3(0.0f, 1f, 0.0f);
            var normal4 = new Vector3(0.0f, -1f, 0.0f);
            var normal5 = new Vector3(-1f, 0.0f, 0.0f);
            var normal6 = new Vector3(1f, 0.0f, 0.0f);
            var textureCoordinate1 = new Vector2(1f, 0.0f);
            var textureCoordinate2 = new Vector2(0.0f, 0.0f);
            var textureCoordinate3 = new Vector2(1f, 1f);
            var textureCoordinate4 = new Vector2(0.0f, 1f);
            verts[0] = new VertexPositionNormalTexture(position1, normal1, textureCoordinate1);
            verts[1] = new VertexPositionNormalTexture(position5, normal1, textureCoordinate3);
            verts[2] = new VertexPositionNormalTexture(position3, normal1, textureCoordinate2);
            verts[3] = new VertexPositionNormalTexture(position5, normal1, textureCoordinate3);
            verts[4] = new VertexPositionNormalTexture(position7, normal1, textureCoordinate4);
            verts[5] = new VertexPositionNormalTexture(position3, normal1, textureCoordinate2);
            verts[6] = new VertexPositionNormalTexture(position2, normal2, textureCoordinate2);
            verts[7] = new VertexPositionNormalTexture(position4, normal2, textureCoordinate1);
            verts[8] = new VertexPositionNormalTexture(position6, normal2, textureCoordinate4);
            verts[9] = new VertexPositionNormalTexture(position6, normal2, textureCoordinate4);
            verts[10] = new VertexPositionNormalTexture(position4, normal2, textureCoordinate1);
            verts[11] = new VertexPositionNormalTexture(position8, normal2, textureCoordinate3);
            verts[12] = new VertexPositionNormalTexture(position1, normal3, textureCoordinate3);
            verts[13] = new VertexPositionNormalTexture(position4, normal3, textureCoordinate2);
            verts[14] = new VertexPositionNormalTexture(position2, normal3, textureCoordinate1);
            verts[15] = new VertexPositionNormalTexture(position1, normal3, textureCoordinate3);
            verts[16] = new VertexPositionNormalTexture(position3, normal3, textureCoordinate4);
            verts[17] = new VertexPositionNormalTexture(position4, normal3, textureCoordinate2);
            verts[18] = new VertexPositionNormalTexture(position5, normal4, textureCoordinate1);
            verts[19] = new VertexPositionNormalTexture(position6, normal4, textureCoordinate3);
            verts[20] = new VertexPositionNormalTexture(position8, normal4, textureCoordinate4);
            verts[21] = new VertexPositionNormalTexture(position5, normal4, textureCoordinate1);
            verts[22] = new VertexPositionNormalTexture(position8, normal4, textureCoordinate4);
            verts[23] = new VertexPositionNormalTexture(position7, normal4, textureCoordinate2);
            verts[24] = new VertexPositionNormalTexture(position1, normal5, textureCoordinate2);
            verts[25] = new VertexPositionNormalTexture(position6, normal5, textureCoordinate3);
            verts[26] = new VertexPositionNormalTexture(position5, normal5, textureCoordinate4);
            verts[27] = new VertexPositionNormalTexture(position2, normal5, textureCoordinate1);
            verts[28] = new VertexPositionNormalTexture(position6, normal5, textureCoordinate3);
            verts[29] = new VertexPositionNormalTexture(position1, normal5, textureCoordinate2);
            verts[30] = new VertexPositionNormalTexture(position3, normal6, textureCoordinate1);
            verts[31] = new VertexPositionNormalTexture(position7, normal6, textureCoordinate3);
            verts[32] = new VertexPositionNormalTexture(position8, normal6, textureCoordinate4);
            verts[33] = new VertexPositionNormalTexture(position4, normal6, textureCoordinate2);
            verts[34] = new VertexPositionNormalTexture(position3, normal6, textureCoordinate1);
            verts[35] = new VertexPositionNormalTexture(position8, normal6, textureCoordinate4);
        }
    }
}
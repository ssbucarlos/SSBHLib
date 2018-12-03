﻿using SFGraphics.Cameras;
using SFGraphics.GLObjects.Shaders;
using System.IO;

namespace CrossMod.Rendering
{
    public class RTexture : IRenderable
    {
        public static Shader textureShader = null;
        private static ScreenTriangle triangle = null;

        public SFGraphics.GLObjects.Textures.Texture renderTexture = null;

        public void Render(Camera Camera)
        {
            // TODO: Render texture.
            if (triangle == null)
                triangle = new ScreenTriangle();

            if (textureShader == null)
            {
                textureShader = new Shader();
                textureShader.LoadShader(File.ReadAllText("Shaders/texture.frag"), OpenTK.Graphics.OpenGL.ShaderType.FragmentShader);
                textureShader.LoadShader(File.ReadAllText("Shaders/texture.vert"), OpenTK.Graphics.OpenGL.ShaderType.VertexShader);
            }

            // Texture unit 0 should be reserved for image preview.
            textureShader.UseProgram();
            if (renderTexture != null)
                textureShader.SetTexture("image", renderTexture, 0);

            triangle.Draw(textureShader, null);
        }
    }
}

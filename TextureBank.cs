using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hacknet
{
    public static class TextureBank
    {
        public static List<LoadedTexture> textures = new List<LoadedTexture>();

        public static Texture2D load(string filename, ContentManager content)
        {
            for (var index = 0; index < textures.Count; ++index)
            {
                if (textures[index].path.Equals(filename))
                {
                    if (textures[index].tex.IsDisposed)
                    {
                        textures.Remove(textures[index]);
                    }
                    else
                    {
                        var loadedTexture = textures[index];
                        ++loadedTexture.retainCount;
                        textures[index] = loadedTexture;
                        return loadedTexture.tex;
                    }
                }
            }
            try
            {
                var texture2D = content.Load<Texture2D>(filename);
                textures.Add(new LoadedTexture
                {
                    tex = texture2D,
                    path = filename,
                    retainCount = 1
                });
                return texture2D;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("File \"", filename, "\" Experienced an Error in Loading\n", ex));
                return null;
            }
        }

        public static Texture2D getIfLoaded(string filename)
        {
            foreach (var loadedTexture in textures)
            {
                if (loadedTexture.path.Equals(filename))
                {
                    if (!loadedTexture.tex.IsDisposed)
                        return loadedTexture.tex;
                    textures.Remove(loadedTexture);
                }
            }
            return null;
        }

        public static void unload(Texture2D tex)
        {
            unloadWithoutRemoval(tex);
        }

        public static void unloadWithoutRemoval(Texture2D tex)
        {
            for (var index = 0; index < textures.Count; ++index)
            {
                if (textures[index].tex.Equals(tex))
                {
                    if (textures[index].tex.IsDisposed)
                    {
                        textures.Remove(textures[index]);
                    }
                    else
                    {
                        var loadedTexture = textures[index];
                        --loadedTexture.retainCount;
                        textures[index] = loadedTexture;
                        break;
                    }
                }
            }
        }
    }
}
using System;
using GameCube.GX.Texture;

namespace Manifold.GFZCLI
{
    public record TplTextureName
    {
        public int TplIndex { get; init; }
        public int TextureLevel { get; init; }
        public TextureFormat TextureFormat { get; init; }
        public string Name { get; init; } = string.Empty;

        public TplTextureName()
        {
        }

        public TplTextureName(string name)
        {
            string[] components = name.Split('-');
            if (components.Length != 4)
            {
                string msg = $"error";
                throw new ArgumentException(msg);
            }

            TplIndex = int.Parse(components[0]);
            TextureLevel = int.Parse(components[1]);
            TextureFormat = Enum.Parse<TextureFormat>(components[2]);
            Name = components[3];
        }

        public override string ToString()
        {
            string value = $"{TplIndex}-{TextureLevel}-{TextureFormat}-{Name}";
            return value;
        }

        public static implicit operator string(TplTextureName textureName)
        {
            return textureName.ToString();
        }
    }
}

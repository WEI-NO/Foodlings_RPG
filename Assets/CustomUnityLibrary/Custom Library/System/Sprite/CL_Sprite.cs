using UnityEngine;

namespace CustomLibrary.SpriteExtra
{
    public struct SpriteExtra
    {
        public static Vector2 DynamicDimension(Sprite sprite, float normalizedSize)
        {
            var w = sprite.rect.width;
            var h = sprite.rect.height;
            var ratio = w / h;

            return new Vector2(ratio * normalizedSize, normalizedSize);
        }
    }
}
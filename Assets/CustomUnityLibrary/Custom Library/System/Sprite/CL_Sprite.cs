using UnityEngine;
using UnityEngine.UI;

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

        public static bool SetDynamicDimension(Image image,  float normalizedSize)
        {
            if (image == null || image.sprite == null)
            {
                return false;
            }

            image.rectTransform.sizeDelta = DynamicDimension(image.sprite, normalizedSize);
            return true;
        }
    }
}
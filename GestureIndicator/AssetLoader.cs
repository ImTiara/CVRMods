using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GestureIndicator
{
    internal class AssetLoader
    {
        public static Sprite openHand, _null, fist, thumbsUp, fingerGun, point, victory, rockAndRoll;
        public static GameObject template;

        public static void Load()
        {
            using var assetStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("GestureIndicator.Assets.gestureindicator");
            using var tempStream = new MemoryStream((int)assetStream.Length);
            assetStream.CopyTo(tempStream);

            AssetBundle assetBundle = AssetBundle.LoadFromMemory(tempStream.ToArray());

            openHand = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/openhand.png"));
            _null = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/null.png"));
            fist = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/fist.png"));
            thumbsUp = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/thumbsup.png"));
            fingerGun = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/fingergun.png"));
            point = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/point.png"));
            victory = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/victory.png"));
            rockAndRoll = ToSprite(assetBundle.LoadAsset("assets/mods/gestureindicator/textures/rockandroll.png"));

            template = (GameObject)assetBundle.LoadAsset("assets/mods/gestureindicator/gestureindicator.prefab");
            template.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        private static Sprite ToSprite(Object obj)
        {
            Texture2D texture = obj as Texture2D;
            texture.wrapMode = TextureWrapMode.Clamp;
            var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(texture.width / 2, texture.height / 2), 100.0f, 0, SpriteMeshType.Tight, Vector4.zero, false);
            sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
            return sprite;
        }
    }
}

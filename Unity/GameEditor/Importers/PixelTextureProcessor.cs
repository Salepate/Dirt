using UnityEditor;

namespace Dirt.GameEditor
{
    public class PixelTextureProcessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            TextureImporter imp = (TextureImporter)assetImporter;

            if (!imp.importSettingsMissing)
                return;

            bool isPixelTexture = assetPath.Contains("Environment");


            if (isPixelTexture)
            {
                imp.textureType = TextureImporterType.Sprite;

                if ( imp.spriteImportMode == SpriteImportMode.None )
                    imp.spriteImportMode = SpriteImportMode.Single;

                imp.textureCompression = TextureImporterCompression.Uncompressed;
                imp.filterMode = UnityEngine.FilterMode.Point;
                imp.wrapMode = UnityEngine.TextureWrapMode.Repeat;
            }
        }
    }
}
using UnityEditor;

namespace Dirt.GameEditor
{
    public class UITextureProcessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            TextureImporter imp = (TextureImporter)assetImporter;

            if (!imp.importSettingsMissing)
                return;

            bool isUiFolder = assetPath.Contains("/UI");

            if (assetPath.Contains("/DualityUI/"))
            {
                isUiFolder = true;
            }

            if (isUiFolder)
            {
                imp.textureType = TextureImporterType.Sprite;

                if ( imp.spriteImportMode == SpriteImportMode.None )
                    imp.spriteImportMode = SpriteImportMode.Single;

                imp.textureCompression = TextureImporterCompression.Uncompressed;
            }
        }
    }
}
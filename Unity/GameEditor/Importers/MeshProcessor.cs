using UnityEditor;
using UnityEngine;

namespace MOBA.GameEditor
{
    public class MeshProcessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            ModelImporter imp = (ModelImporter)assetImporter;

            
            if ( assetPath.Contains("/Models/"))
            {
                imp.useFileScale = false;
                imp.materialImportMode = ModelImporterMaterialImportMode.None;
            }

            // no compression
            //imp.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
}
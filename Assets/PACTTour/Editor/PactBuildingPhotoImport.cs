#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace Pact.VirtualTour
{
    public sealed class PactBuildingPhotoImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/PACTTour/Resources/PACTTour/Buildings/Photos/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.maxTextureSize = 4096;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Trilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.alphaSource = TextureImporterAlphaSource.None;
        }
        [MenuItem("PACT/Verificar fotos dos dois edificios")]
        static void Verify()
        {
            var json = Resources.Load<TextAsset>("PACTTour/Buildings/percurso");
            if (!json) { Debug.LogError("Falta percurso.json. Copia a pasta Assets da atualização."); return; }
            var route = JsonUtility.FromJson<PactBuildingExpansion.Route>(json.text);
            int missing = 0;
            if (route == null || route.nodes == null) { Debug.LogError("percurso.json inválido."); return; }
            foreach (var n in route.nodes)
            {
                string path = "Assets/PACTTour/Resources/" + n.resource + ".jpg";
                if (!AssetDatabase.LoadAssetAtPath<Texture2D>(path))
                { Debug.LogError("Fotografia em falta: " + path); missing++; }
            }
            Debug.Log(missing == 0 ? "PACT: todas as " + route.nodes.Length + " novas fotografias estão disponíveis." : "PACT: faltam " + missing + " fotografias.");
        }
    }
}
#endif

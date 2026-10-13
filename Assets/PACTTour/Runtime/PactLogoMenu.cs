using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pact.VirtualTour
{
    public class PactLogoMenu : MonoBehaviour
    {
        private Material materialLogo;
        private GameObject objetoLogo;
        private float proximaTentativa;
        private bool avisou;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Preparar()
        {
            SceneManager.sceneLoaded -= AoAbrirCena;
            SceneManager.sceneLoaded += AoAbrirCena;
        }

        private static void AoAbrirCena(
            Scene cena,
            LoadSceneMode modo)
        {
            if (Object.FindObjectOfType<PactLogoMenu>() != null)
                return;

            new GameObject("PACT_Logotipo_Menu")
                .AddComponent<PactLogoMenu>();
        }

        private void Update()
        {
            if (objetoLogo != null)
                return;

            if (Time.unscaledTime < proximaTentativa)
                return;

            proximaTentativa = Time.unscaledTime + 0.5f;

            PactMainMenu menu =
                Object.FindObjectOfType<PactMainMenu>();

            if (menu == null)
                return;

            Texture2D imagem =
                Resources.Load<Texture2D>("PACTTour/logo_pact");

            Shader shader = Shader.Find("Sprites/Default");

            if (imagem == null || shader == null)
            {
                if (!avisou)
                {
                    Debug.LogWarning(
                        "PACT: confirma que logo_pact.png está em " +
                        "Assets/PACTTour/Resources/PACTTour " +
                        "e foi importado como textura 2D.");

                    avisou = true;
                }

                return;
            }

            Transform marca = menu.transform.Find("Marca");

            if (marca == null)
                return;

            materialLogo = new Material(shader);
            materialLogo.name = "PACT_Logotipo_Branco";
            materialLogo.mainTexture = imagem;
            materialLogo.color = Color.white;

            objetoLogo = GameObject.CreatePrimitive(
                PrimitiveType.Quad);

            objetoLogo.name = "Logotipo_PACT";
            objetoLogo.transform.SetParent(menu.transform, false);

            // Acima do título, no lugar do texto P A C T.
            objetoLogo.transform.localPosition =
                new Vector3(0, 0.98f, 2.98f);

            float largura = 1.25f;

            float altura =
                largura * imagem.height /
                Mathf.Max(1f, imagem.width);

            objetoLogo.transform.localScale =
                new Vector3(largura, altura, 1);

            Renderer rendererLogo =
                objetoLogo.GetComponent<Renderer>();

            rendererLogo.sharedMaterial = materialLogo;

            rendererLogo.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            rendererLogo.receiveShadows = false;

            Collider colisor = objetoLogo.GetComponent<Collider>();

            if (colisor != null)
            {
                colisor.enabled = false;
                Destroy(colisor);
            }

            // Só esconder o texto depois de criar o logótipo.
            marca.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (objetoLogo != null)
                Destroy(objetoLogo);

            if (materialLogo != null)
                Destroy(materialLogo);
        }
    }
}
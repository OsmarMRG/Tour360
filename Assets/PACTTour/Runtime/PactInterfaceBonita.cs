using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pact.VirtualTour
{
    public class PactInterfaceBonita : MonoBehaviour
    {
        private static readonly Color Azul =
            new Color32(16, 24, 32, 255);

        private static readonly Color Laranja =
            new Color32(255, 81, 0, 255);

        private static readonly Color Branco =
            new Color32(245, 247, 250, 255);

        private static readonly Color Cinzento =
            new Color32(199, 210, 220, 255);

        private class Painel
        {
            public GameObject objeto;
            public Material material;
            public Mesh malha;
            public bool estavaVisivel;
            public float tempo;
        }

        private readonly List<Painel> paineis =
            new List<Painel>();

        private readonly HashSet<Renderer> tratados =
            new HashSet<Renderer>();

        private readonly List<Material> materiais =
            new List<Material>();

        private PactTourController visita;
        private float proximaVerificacao;

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
            if (Object.FindObjectOfType<PactInterfaceBonita>() != null)
                return;

            new GameObject("PACT_Interface_Bonita")
                .AddComponent<PactInterfaceBonita>();
        }

        private void Update()
        {
            if (Time.unscaledTime >= proximaVerificacao)
            {
                proximaVerificacao = Time.unscaledTime + 0.3f;
                AtualizarInterface();
            }

            AnimarPaineis();
        }

        private void AtualizarInterface()
        {
            if (visita == null)
                visita = Object.FindObjectOfType<PactTourController>();

            if (visita == null)
                return;

            tratados.RemoveWhere(item => item == null);

            Renderer[] elementos =
                visita.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer elemento in elementos)
            {
                if (elemento == null || tratados.Contains(elemento))
                    continue;

                TextMesh texto = elemento.GetComponent<TextMesh>();

                if (texto != null)
                {
                    EstilizarTexto(texto);
                    tratados.Add(elemento);
                    continue;
                }

                string nome = elemento.gameObject.name;

                if (EhPainel(nome))
                {
                    if (EstilizarPainel(elemento))
                        tratados.Add(elemento);
                }
                else if (EhMarcador(nome))
                {
                    PintarMarcador(elemento);
                    tratados.Add(elemento);
                }
            }
        }

        private bool EhPainel(string nome)
        {
            switch (nome)
            {
                case "Placa":
                case "Fundo":
                case "Repetir":
                case "Fechar guia":
                case "Menu":
                case "Inicio":
                case "Comecar":
                case "Cartao_sala":
                case "Painel_legendas":
                case "Local_capsula":
                case "Ponto_sala":
                    return true;

                default:
                    return false;
            }
        }

        private bool EhBotao(string nome)
        {
            return nome == "Repetir"
                || nome == "Fechar guia"
                || nome == "Menu"
                || nome == "Inicio"
                || nome == "Comecar"
                || nome == "Ponto_sala";
        }

        private bool EhMarcador(string nome)
        {
            return nome == "Anel"
                || nome == "Seta"
                || nome == "Apresentar_sala"
                || nome == "Detalhe"
                || nome == "Cursor"
                || nome == "Apontador";
        }

        private void PintarMarcador(Renderer elemento)
        {
            if (elemento.sharedMaterial == null)
                return;

            Material material =
                new Material(elemento.sharedMaterial);

            material.name = "PACT_Laranja";

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", Laranja);

            elemento.sharedMaterial = material;
            materiais.Add(material);

            LineRenderer linha = elemento as LineRenderer;

            if (linha != null)
            {
                linha.startColor = Color.white;
                linha.endColor = Color.white;
            }
        }

        private bool EstilizarPainel(Renderer elemento)
        {
            MeshFilter filtro = elemento.GetComponent<MeshFilter>();

            if (filtro == null)
                return false;

            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                Debug.LogWarning(
                    "PACT: shader Sprites/Default não encontrado.");

                return false;
            }

            Vector3 escala = elemento.transform.lossyScale;

            float proporcao =
                Mathf.Abs(escala.x) /
                Mathf.Max(0.001f, Mathf.Abs(escala.y));

            bool botao = EhBotao(elemento.gameObject.name);

            Material material = new Material(shader);
            material.name = "PACT_Painel";
            material.mainTexture = Texture2D.whiteTexture;
            material.color = Color.white;

            Mesh malha = CriarCartao(proporcao, botao);

            filtro.sharedMesh = malha;
            elemento.sharedMaterial = material;

            elemento.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            elemento.receiveShadows = false;

            paineis.Add(new Painel
            {
                objeto = elemento.gameObject,
                material = material,
                malha = malha
            });

            return true;
        }

        private Mesh CriarCartao(float proporcao, bool botao)
        {
            const int segmentos = 10;

            List<Vector3> vertices = new List<Vector3>();
            List<Color> cores = new List<Color>();
            List<int> triangulos = new List<int>();

            float raioY = 0.18f;
            float raioX = Mathf.Min(
                0.22f,
                raioY / Mathf.Max(0.1f, proporcao));

            Color centro = Azul;

            // Contorno laranja subtil nos painéis azuis.
            Color contorno = Color.Lerp(Azul, Laranja, 0.65f);
            
                vertices.Add(Vector3.zero);
            cores.Add(centro);

            List<Vector3> limite = new List<Vector3>();

            for (int canto = 0; canto < 4; canto++)
            {
                float centroX =
                    canto == 0 || canto == 3
                    ? 0.5f - raioX
                    : -0.5f + raioX;

                float centroY =
                    canto < 2
                    ? 0.5f - raioY
                    : -0.5f + raioY;

                for (int passo = 0; passo <= segmentos; passo++)
                {
                    float angulo =
                        (canto * 90f + passo * 90f / segmentos)
                        * Mathf.Deg2Rad;

                    limite.Add(new Vector3(
                        centroX + Mathf.Cos(angulo) * raioX,
                        centroY + Mathf.Sin(angulo) * raioY,
                        0));
                }
            }

            // Primeiro contorno: área interior do cartão.
            foreach (Vector3 ponto in limite)
            {
                vertices.Add(new Vector3(
                    ponto.x * 0.988f,
                    ponto.y * 0.965f,
                    0));

                cores.Add(centro);
            }

            // Segundo contorno: borda fina.
            foreach (Vector3 ponto in limite)
            {
                vertices.Add(ponto);
                cores.Add(contorno);
            }

            int quantidade = limite.Count;

            for (int i = 0; i < quantidade; i++)
            {
                int seguinte = (i + 1) % quantidade;

                int interiorA = 1 + i;
                int interiorB = 1 + seguinte;
                int exteriorA = 1 + quantidade + i;
                int exteriorB = 1 + quantidade + seguinte;

                triangulos.Add(0);
                triangulos.Add(interiorA);
                triangulos.Add(interiorB);

                triangulos.Add(interiorA);
                triangulos.Add(exteriorA);
                triangulos.Add(exteriorB);

                triangulos.Add(interiorA);
                triangulos.Add(exteriorB);
                triangulos.Add(interiorB);
            }

            Mesh malha = new Mesh();
            malha.name = "PACT_Cartao_Arredondado";
            malha.SetVertices(vertices);
            malha.SetColors(cores);
            malha.SetTriangles(triangulos, 0);

            Vector2[] uv = new Vector2[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                uv[i] = new Vector2(
                    vertices[i].x + 0.5f,
                    vertices[i].y + 0.5f);
            }

            malha.uv = uv;
            malha.RecalculateNormals();
            malha.RecalculateBounds();

            return malha;
        }

        private void EstilizarTexto(TextMesh texto)
        {
            switch (texto.gameObject.name)
            {
                case "Nome":
                    texto.color = Laranja;
                    texto.fontStyle = FontStyle.Bold;

                    PactVisuals.Fit(
                        texto, texto.text,
                        0.76f, 0.10f, 0.065f);
                    break;

                case "Acao":
                    texto.color = Branco;

                    PactVisuals.Fit(
                        texto,
                        "Descobrir este espaço",
                        0.77f, 0.05f, 0.029f);
                    break;

                case "Categoria":
                    texto.color = Cinzento;
                    break;

                case "Apresentacao":
                    texto.color = Laranja;
                    texto.fontStyle = FontStyle.Bold;
                    break;

                case "Legenda":
                    texto.color = Branco;
                    break;

                case "Simbolo":
                case "Repetir_texto":
                case "Fechar guia_texto":
                case "Menu_texto":
                case "Inicio_texto":
                case "Comecar_texto":
                    texto.color = Laranja;
                    texto.fontStyle = FontStyle.Bold;
                    break;

                case "Marca":
                case "Titulo":
                    texto.color = Laranja;
                    texto.fontStyle = FontStyle.Bold;
                    break;

                case "Subtitulo":
                case "Instrucoes":
                case "Localizacao":
                case "Destino":
                    texto.color = Branco;
                    break;

                case "Estado":
                    texto.color = Cinzento;
                    break;
            }
        }

        private void AnimarPaineis()
        {
            for (int i = paineis.Count - 1; i >= 0; i--)
            {
                Painel painel = paineis[i];

                if (painel.objeto == null)
                {
                    Libertar(painel);
                    paineis.RemoveAt(i);
                    continue;
                }

                if (!painel.objeto.activeInHierarchy)
                {
                    painel.estavaVisivel = false;
                    painel.tempo = 0;
                    continue;
                }

                if (!painel.estavaVisivel)
                {
                    painel.tempo = 0;
                    painel.estavaVisivel = true;
                }

                painel.tempo += Time.unscaledDeltaTime;

                float entrada = Mathf.SmoothStep(
                    0, 1,
                    Mathf.Clamp01(painel.tempo / 0.4f));

                painel.material.color =
                    new Color(1, 1, 1, entrada * 0.98f);
            }
        }

        private void Libertar(Painel painel)
        {
            if (painel.material != null)
                Destroy(painel.material);

            if (painel.malha != null)
                Destroy(painel.malha);
        }

        private void OnDestroy()
        {
            foreach (Painel painel in paineis)
                Libertar(painel);

            foreach (Material material in materiais)
            {
                if (material != null)
                    Destroy(material);
            }

            paineis.Clear();
            materiais.Clear();
        }
    }
}
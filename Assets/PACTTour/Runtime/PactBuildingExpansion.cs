using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pact.VirtualTour
{
    // Adição à visita existente. Não altera os ficheiros nem o asset original.
    [DefaultExecutionOrder(-100)]
    public sealed class PactBuildingExpansion : MonoBehaviour
    {
        [Serializable] public class Route { public Photo[] nodes; }
        [Serializable] public class Photo
        {
            public string title, resource;
            public float rotation;
            public Step[] links;
        }
        [Serializable] public class Step
        {
            public int target;
            public float yaw, distance = 2.1f;
            public string label;
        }
        PactTourController tour;
        PactTourData original, expanded;
        Shader newShader, oldShader;
        Material blue, orange;
        Transform toolbar;
        TextMesh destination;
        PactTarget switchTarget;
        PactMainMenu menu;
        int offset, lastOld, lastNew, observed = -1;
        readonly Color brandOrange = new Color(1f, 81f / 255f, 0f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            foreach (var controller in UnityEngine.Object.FindObjectsOfType<PactTourController>())
                if (!controller.GetComponent<PactBuildingExpansion>())
                    controller.gameObject.AddComponent<PactBuildingExpansion>();
        }

        void Awake()
        {
            tour = GetComponent<PactTourController>();
            var json = Resources.Load<TextAsset>("PACTTour/Buildings/percurso");
            newShader = Resources.Load<Shader>("PACTTour/Buildings/PactBuildingPanorama");
            if (!tour || !tour.data || !json || !newShader || !tour.panoramaTemplate)
            { Fail("Faltam os dados, o shader ou a visita original."); return; }
            original = tour.data;
            offset = original.nodes == null ? 0 : original.nodes.Length;
            if (offset != 3)
            { Fail("Esta atualização espera os 3 pontos originais. Encontrou " + offset + ". A visita original foi mantida."); return; }
            Route route;
            try { route = JsonUtility.FromJson<Route>(json.text); }
            catch (Exception e) { Fail("percurso.json inválido: " + e.Message); return; }
            if (route == null || route.nodes == null || route.nodes.Length != 54)
            { Fail("O percurso deve conter as 54 fotografias do novo edifício."); return; }
            // Validar antes de substituir a referência usada pelo controlador.
            for (int i = 0; i < route.nodes.Length; i++)
            {
                var n = route.nodes[i];
                if (n == null || string.IsNullOrEmpty(n.resource) || n.links == null)
                { Fail("Ponto inválido: " + i); return; }
                foreach (var l in n.links)
                    if (l == null || l.target < 0 || l.target >= route.nodes.Length || l.target == i)
                    { Fail("Ligação inválida no ponto " + i); return; }
            }
            oldShader = tour.panoramaTemplate.shader;
            expanded = Instantiate(original);
            expanded.name = "PACT — Dois edifícios (cópia em execução)";
            var all = new List<PactTourNode>(original.nodes);
            for (int i = 0; i < route.nodes.Length; i++)
            {
                var n = route.nodes[i];
                var links = new List<PactTourLink>();
                foreach (var l in n.links)
                    links.Add(new PactTourLink { targetNode = offset + l.target, yaw = l.yaw,
                        distance = Mathf.Clamp(l.distance, .8f, 5f), label = l.label });
                all.Add(new PactTourNode { title = n.title, roomId = "building-main-" + i,
                    panoramaResource = n.resource, panoramaRotation = n.rotation,
                    showGuide = false, narration = "", links = links.ToArray(),
                    roomSigns = Array.Empty<PactRoomInfo>() });
            }
            expanded.nodes = all.ToArray();
            expanded.startNode = offset;
            expanded.fadeSeconds = .38f;
            tour.data = expanded;
            lastOld = Mathf.Clamp(original.startNode, 0, offset - 1);
            lastNew = offset;
        }

        void Fail(string message)
        { Debug.LogError("PACT Expansão: " + message, this); enabled = false; }

        void Start()
        {
            if (!enabled || !tour.viewCamera) return;
            var shader = Shader.Find("PACT/Flat");
            if (!shader) shader = Shader.Find("Unlit/Color");
            if (!shader) { Fail("Falta um shader de cor para a interface."); return; }
            blue = new Material(shader) { color = new Color(16f / 255f, 24f / 255f, 32f / 255f) };
            orange = new Material(shader) { color = brandOrange };
            toolbar = new GameObject("PACT_TrocarEdificio").transform;
            toolbar.SetParent(tour.viewCamera.transform, false);
            // Abaixo do título de localização e acima do centro da imagem.
            toolbar.localPosition = new Vector3(0, .47f, 2.4f);
            var button = PactVisuals.Shape(PrimitiveType.Cube, toolbar, "Trocar_de_edificio",
                Vector3.zero, new Vector3(1.22f, .18f, .022f), blue, true);
            switchTarget = button.AddComponent<PactTarget>();
            switchTarget.tour = tour;
            switchTarget.action = PactAction.Navigate;
            var text = PactVisuals.Text(toolbar, "Trocar_texto", "Trocar de edifício",
                new Vector3(0, .018f, -.02f), .052f, brandOrange);
            PactVisuals.Fit(text, text.text, 1.08f, .09f, .052f);
            destination = PactVisuals.Text(toolbar, "Edificio_destino", "",
                new Vector3(0, -.046f, -.02f), .025f, brandOrange);
            toolbar.gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (!tour || !expanded || !toolbar) return;
            if (!menu) menu = tour.viewCamera.GetComponentInChildren<PactMainMenu>(true);
            bool inMenu = !menu || menu.gameObject.activeInHierarchy;
            int index = tour.CurrentNode;
            toolbar.gameObject.SetActive(!inMenu && index >= 0 && !tour.Busy);
            if (inMenu && menu)
            {
                var status = menu.transform.Find("Estado");
                var text = status ? status.GetComponent<TextMesh>() : null;
                if (text && (text.text == "3 pontos no corredor  /  Future Lab" || text.text == ""))
                    PactVisuals.Fit(text, "2 edifícios  ·  57 panorâmicas", 3f, .1f, .04f);
            }
            if (index < 0) { observed = -1; return; }
            if (index == observed) return;
            observed = index;
            bool main = index >= offset;
            if (main) lastNew = index; else lastOld = index;
            switchTarget.destination = main ? lastOld : lastNew;
            PactVisuals.Fit(destination, main ? "Destino: corredor do Future Lab" : "Destino: átrio, auditório e gabinetes",
                1.08f, .043f, .025f);
            // O controlador muda o ponto sob fade preto. LateUpdate ajusta o shader
            // antes do primeiro desenho; os 3 pontos antigos conservam a projeção original.
            var sky = RenderSettings.skybox;
            if (sky)
            {
                var texture = sky.GetTexture("_MainTex");
                sky.shader = main ? newShader : oldShader;
                sky.SetTexture("_MainTex", texture);
                sky.SetFloat("_Rotation", expanded.nodes[index].panoramaRotation);
            }
            StyleSteps(index);
        }

        void StyleSteps(int index)
        {
            var content = tour.transform.Find("Ponto_" + index);
            if (!content) return;
            foreach (var target in content.GetComponentsInChildren<PactTarget>(true))
            {
                if (target.action != PactAction.Navigate) continue;
                foreach (var line in target.GetComponentsInChildren<LineRenderer>())
                    line.sharedMaterial = orange;
                foreach (var text in target.GetComponentsInChildren<TextMesh>(true))
                    text.color = brandOrange;
                // Disco discreto, sem esfera e sem movimento de câmara.
                if (!target.transform.Find("Base_PACT"))
                    PactVisuals.Shape(PrimitiveType.Cylinder, target.transform, "Base_PACT",
                        new Vector3(0, .012f, 0), new Vector3(.43f, .01f, .43f), blue);
            }
        }

        void OnDestroy()
        {
            if (tour && tour.data == expanded) tour.data = original;
            if (toolbar) Destroy(toolbar.gameObject);
            if (blue) Destroy(blue);
            if (orange) Destroy(orange);
            if (expanded) Destroy(expanded);
        }
    }
}

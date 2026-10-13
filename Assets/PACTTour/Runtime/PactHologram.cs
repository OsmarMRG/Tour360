using UnityEngine;
namespace Pact.VirtualTour
{
    public class PactHologram : MonoBehaviour
    {
        AudioSource voice;
        Transform body, head, leftArm, rightArm, mouth;
        float[] samples = new float[128];
        public bool Speaking => voice && voice.isPlaying;
        public void Build(Material hologram, Material bright)
        {
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = .8f;
            voice.rolloffMode = AudioRolloffMode.Linear;
            voice.minDistance = 1.5f; voice.maxDistance = 9f;
            voice.dopplerLevel = 0;
            var pedestal = PactVisuals.Shape(PrimitiveType.Cylinder, transform, "Projetor",
                new Vector3(0, .06f, 0), new Vector3(.65f, .06f, .65f), bright);
            body = new GameObject("Avatar").transform; body.SetParent(transform, false);
            PactVisuals.Shape(PrimitiveType.Capsule, body, "Tronco", new Vector3(0, .92f, 0),
                new Vector3(.37f, .36f, .24f), hologram);
            head = PactVisuals.Shape(PrimitiveType.Sphere, body, "Cabeca", new Vector3(0, 1.48f, 0),
                new Vector3(.34f, .37f, .31f), hologram).transform;
            PactVisuals.Shape(PrimitiveType.Cube, body, "Visor", new Vector3(0, 1.50f, -.155f),
                new Vector3(.24f, .07f, .02f), bright);
            mouth = PactVisuals.Shape(PrimitiveType.Cube, body, "Indicador_de_voz", new Vector3(0, 1.40f, -.155f),
                new Vector3(.12f, .015f, .022f), bright).transform;
            leftArm = Arm("Braco_E", -.27f, hologram);
            rightArm = Arm("Braco_D", .27f, hologram);
            for (int side = -1; side <= 1; side += 2)
                PactVisuals.Shape(PrimitiveType.Capsule, body, "Perna", new Vector3(side * .105f, .4f, 0),
                    new Vector3(.13f, .22f, .13f), hologram);
            PactVisuals.Text(transform, "Identificacao", "PACT / GUIA DEMO", new Vector3(0, 1.85f, 0), .052f, Color.cyan);
        }
        Transform Arm(string label, float x, Material material)
        {
            var pivot = new GameObject(label).transform; pivot.SetParent(body, false);
            pivot.localPosition = new Vector3(x, 1.13f, 0);
            PactVisuals.Shape(PrimitiveType.Capsule, pivot, "Segmento", new Vector3(0, -.22f, 0),
                new Vector3(.12f, .23f, .12f), material);
            return pivot;
        }
        public void Speak(AudioClip clip) { Stop(); if (clip) { voice.clip = clip; voice.Play(); } }
        public void Stop() { if (voice) voice.Stop(); }
        void Update()
        {
            if (!body) return;
            float t = Time.unscaledTime;
            float energy = 0;
            if (Speaking)
            {
                voice.GetOutputData(samples, 0);
                foreach (float sample in samples) energy += sample * sample;
                energy = Mathf.Clamp01(Mathf.Sqrt(energy / samples.Length) * 12);
            }
            body.localPosition = new Vector3(0, .025f * Mathf.Sin(t * 1.5f), 0);
            leftArm.localRotation = Quaternion.Euler(Speaking ? -18 + 12 * Mathf.Sin(t * 2.4f) : 0, 0, -9);
            rightArm.localRotation = Quaternion.Euler(Speaking ? -30 + 18 * Mathf.Sin(t * 2) : 0, 0, 9);
            head.localRotation = Quaternion.Euler(0, 8 * Mathf.Sin(t), 0);
            mouth.localScale = new Vector3(.12f, .015f + energy * .07f, .022f);
        }
    }
}

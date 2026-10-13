using UnityEngine;
namespace Pact.VirtualTour
{
    public static class PactVisuals
    {
        static Font cachedFont;
        public static Font Font
        {
            get
            {
                if (!cachedFont)
                {
#if UNITY_2022_2_OR_NEWER
                    cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
                    cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
                }
                return cachedFont;
            }
        }
        public static GameObject Shape(PrimitiveType kind, Transform parent, string name,
            Vector3 position, Vector3 scale, Material material, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(kind); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position;
            go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) { go.GetComponent<Collider>().enabled = false; Object.Destroy(go.GetComponent<Collider>()); }
            return go;
        }
        // size is now a real line height in metres, independent of raster font size.
        public static TextMesh Text(Transform parent, string name, string value, Vector3 position, float size, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var text = go.AddComponent<TextMesh>(); text.font = Font;
            text.GetComponent<MeshRenderer>().sharedMaterial = Font.material;
            text.fontSize = 64; text.characterSize = size * 10f / 64f;
            text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center;
            text.color = color; text.richText = false; text.text = value;
            return text;
        }
        public static void Fit(TextMesh text, string value, float width, float height, float lineHeight)
        {
            text.text = value; text.transform.localScale = Vector3.one;
            text.characterSize = lineHeight * 10f / text.fontSize;
            Font.RequestCharactersInTexture(value, text.fontSize, FontStyle.Normal);
            float widest = 0;
            var lines = value.Split('\n');
            foreach (var line in lines)
            {
                float pixels = 0;
                foreach (char c in line)
                {
                    if (Font.GetCharacterInfo(c, out CharacterInfo info, text.fontSize, FontStyle.Normal)) pixels += info.advance;
                    else pixels += text.fontSize * .6f;
                }
                widest = Mathf.Max(widest, pixels * text.characterSize / 10f);
            }
            float totalHeight = lines.Length * lineHeight * 1.25f;
            Vector3 meshBounds = text.GetComponent<MeshRenderer>().localBounds.size;
            widest = Mathf.Max(widest, meshBounds.x);
            totalHeight = Mathf.Max(totalHeight, meshBounds.y);
            float scale = Mathf.Min(1, width / Mathf.Max(.001f, widest), height / Mathf.Max(.001f, totalHeight));
            text.transform.localScale = Vector3.one * scale;
        }
        public static string Wrap(string value, int width = 48)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var result = new System.Text.StringBuilder(); int column = 0;
            foreach (string word in value.Split(' '))
            {
                if (column + word.Length + 1 > width) { result.Append('\n'); column = 0; }
                if (column > 0) { result.Append(' '); column++; }
                result.Append(word); column += word.Length;
            }
            return result.ToString();
        }
        public static LineRenderer Line(Transform parent, string name, Material material, Vector3[] points, float width, bool loop=false)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>(); line.useWorldSpace=false;
            line.sharedMaterial=material; line.positionCount=points.Length; line.SetPositions(points);
            line.startWidth=width; line.endWidth=width; line.loop=loop;
            line.numCornerVertices=3; line.numCapVertices=3; return line;
        }
        public static void Ring(Transform parent, Material material, float radius)
        {
            var points=new Vector3[64];
            for(int i=0;i<points.Length;i++) { float a=i*Mathf.PI*2/points.Length; points[i]=new Vector3(Mathf.Cos(a)*radius,.025f,Mathf.Sin(a)*radius); }
            Line(parent,"Anel",material,points,.012f,true);
            Line(parent,"Seta",material,new[] {new Vector3(-.07f,.03f,-.025f),new Vector3(0,.03f,.05f),new Vector3(.07f,.03f,-.025f)},.016f);
        }
    }
}

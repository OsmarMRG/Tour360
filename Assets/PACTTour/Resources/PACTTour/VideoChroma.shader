Shader "PACT/VideoChroma"
{
 Properties {
  _MainTex("Video",2D)="black"{}
  _Alpha("Visibilidade",Range(0,1))=0
  _Threshold("Remover verde",Range(0,0.5))=.055
  _Softness("Suavidade",Range(.001,.3))=.065
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Blend SrcAlpha OneMinusSrcAlpha
  ZWrite Off Cull Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
   sampler2D _MainTex;float _Alpha,_Threshold,_Softness;
   v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
   fixed4 frag(v2f i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    fixed4 c=tex2D(_MainTex,i.uv);
    float excess=c.g-max(c.r,c.b);
    float key=1-smoothstep(_Threshold,_Threshold+_Softness,excess);
    // Atenuar reflexos verdes nas margens e dissolver o corte inferior do vídeo.
    c.g=lerp(c.g,min(c.g,max(c.r,c.b)),saturate(excess*5));
    float edge=smoothstep(0,.085,i.uv.y)*smoothstep(0,.012,i.uv.x)*smoothstep(0,.012,1-i.uv.x);
    return fixed4(c.rgb,c.a*key*edge*_Alpha);
   }
   ENDCG
  }
 }
}

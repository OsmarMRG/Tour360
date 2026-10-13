Shader "PACT/AvatarSurface"
{
 Properties {_Color("Cor",Color)=(.1,.12,.14,1) _Alpha("Presenca",Range(0,1))=1}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Back
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f{float4 vertex:SV_POSITION;float3 normal:TEXCOORD0;float3 world:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 fixed4 _Color;float _Alpha;
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.vertex=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 fixed4 frag(v2f i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);float light=.55+.45*saturate(dot(normalize(i.normal),normalize(float3(-.4,.8,-.5))));return fixed4(_Color.rgb*light,_Color.a*_Alpha);}
 ENDCG
 }}
}

Shader "PACT/MenuBackground"
{
 Properties { _TexA("Foto A",2D)="black"{} _TexB("Foto B",2D)="black"{} _Blend("Mistura",Range(0,1))=0 _AspectA("Aspecto A",Float)=1.7 _AspectB("Aspecto B",Float)=1.7 _PanelAspect("Painel",Float)=1.7 }
 SubShader { Tags {"Queue"="Transparent-100"} Pass {
 Cull Off ZWrite Off
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
 sampler2D _TexA,_TexB;float _Blend,_AspectA,_AspectB,_PanelAspect;
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 float2 cover(float2 uv,float a){float2 scale=float2(min(1,_PanelAspect/a),min(1,a/_PanelAspect));return (uv-.5)*scale+.5;}
 fixed4 frag(v2f i):SV_Target {UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);fixed3 a=tex2D(_TexA,cover(i.uv,_AspectA)).rgb;fixed3 b=tex2D(_TexB,cover(i.uv,_AspectB)).rgb;float shade=.32+.2*abs(i.uv.x-.5);return fixed4(lerp(a,b,_Blend)*shade,1);}
 ENDCG
 }}
}

Shader "PACT/AvatarHead"
{
 Properties {_MainTex("Referencia",2D)="white"{} _FaceRect("UV rosto",Vector)=(.400,.646,.223,.28) _Color("Pele",Color)=(.61,.40,.28,1) _Alpha("Presenca",Range(0,1))=1 _Talk("Boca",Range(0,1))=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Pass {
 Blend SrcAlpha OneMinusSrcAlpha ZWrite On Cull Back
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #include "UnityCG.cginc"
 struct appdata{float4 vertex:POSITION;float3 normal:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct v2f{float4 vertex:SV_POSITION;float3 local:TEXCOORD0;float3 normal:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
 sampler2D _MainTex;float4 _FaceRect;fixed4 _Color;float _Alpha,_Talk;
 v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.local=v.vertex.xyz;float jaw=saturate((-v.vertex.y-.04)*5)*saturate(-v.vertex.z*4);v.vertex.y-=_Talk*.012*jaw;o.vertex=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o;}
 fixed4 frag(v2f i):SV_Target{
 UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
 float2 uv=(i.local.xy+float2(.5,.5))*_FaceRect.zw+_FaceRect.xy;
 fixed4 face=tex2D(_MainTex,uv);
 float front=smoothstep(.01,.24,-i.local.z)*face.a;
 fixed3 color=lerp(_Color.rgb,face.rgb,front);
 float lighting=.75+.25*saturate(dot(normalize(i.normal),normalize(float3(-.4,.8,-.5))));
 // Abertura simplificada, guiada pelo volume; não representa fonemas.
 float2 mouth=(i.local.xy-float2(.04,-.24))/float2(.12,.007+_Talk*.016);
 float aperture=(1-smoothstep(.70,1,dot(mouth,mouth)))*front*_Talk;
 color=lerp(color,fixed3(.10,.035,.025),aperture);
 return fixed4(color*lighting,_Alpha);}
 ENDCG
 }}
}

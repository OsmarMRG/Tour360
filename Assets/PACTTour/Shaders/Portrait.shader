Shader "PACT/Portrait"
{
    Properties { _MainTex("Retrato",2D)="white"{} _Alpha("Opacidade",Range(0,1))=1 }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex; float _Alpha;
            v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 c=tex2D(_MainTex,i.uv);
                float scan=.96+.04*sin(i.uv.y*450-_Time.y*3);
                c.rgb=lerp(c.rgb,c.rgb*fixed3(.65,1,1.1),.25)*scan;
                c.a*= _Alpha*smoothstep(0,.10,i.uv.y);
                return c;
            }
            ENDCG
        }
    }
}

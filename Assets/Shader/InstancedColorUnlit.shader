Shader "Custom/InstancedUnlitBaseColor"
{
    Properties
    {
        _MainTex    ("Texture",     2D)   = "white" {}
        _BaseColor  ("Base Color",  Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;
            float4   _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv        : TEXCOORD0;
                float4 vertex    : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 texCol   = tex2D(_MainTex, i.uv);
                fixed4 instCol  = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                return texCol * instCol;
            }
            ENDCG
        }
    }
}
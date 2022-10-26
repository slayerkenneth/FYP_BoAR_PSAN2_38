Shader "Custom/SemanticShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SemanticTex("_SemanticTex", 2D) = "red" {}
        _OverlayTex("Overlay", 2D) = "black" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                //storage for our transformed depth uv
                float3 semantic_uv : TEXCOORD1;
                float2 overlay_uv : TEXCOORD2;
            };

            // Transforms used to sample the context awareness textures
            float4x4 _semanticTransform;

            //For translate to world
            float4x4 _WorldToUVMatrix;

            //our texture samplers
            sampler2D _MainTex;
            sampler2D _SemanticTex;
            sampler2D _OverlayTex;
            float4 _MainTex_ST;
            float4 _OverlayTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                //multiply the uv's by the depth transform to rotate them correctly.
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz;

                //Original uv mapping
                // o.overlay_uv = UnityWorldToViewPos(v.vertex).xy; //Add camera facing here
                //Alt
                o.overlay_uv = mul(unity_CameraToWorld, v.vertex).xy;
                
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                //unity scene
                float4 mainCol = tex2D(_MainTex, i.uv);
                //our semantic texture, we need to normalise the uv coords before using.
                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel
                float4 semanticCol = tex2D(_SemanticTex, semanticUV);

                fixed4 OverlayPix = tex2D(_OverlayTex, i.overlay_uv);

                return mainCol+semanticCol*OverlayPix; //Need to add camera rotation offset to OverlayPix (uv problem, plz go to vert() to change)
            }
            ENDCG
        }
    }
}
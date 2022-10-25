Shader "Custom/SemanticShader"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTex("_SemanticTex", 2D) = "red" {}
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
            };

            // Transforms used to sample the context awareness textures
            float4x4 _semanticTransform;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                //multiply the uv's by the depth transform to rotate them correctly.
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz;
                return o;
            }

            //our texture samplers
            sampler2D _MainTex;
            sampler2D _SemanticTex;


            fixed4 frag (v2f i) : SV_Target
            {
                //unity scene
                float4 mainCol = tex2D(_MainTex, i.uv);
                //our semantic texture, we need to normalise the uv coords before using.
                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z);
                //read the semantic texture pixel
                float4 semanticCol = tex2D(_SemanticTex, semanticUV);


                // //add some grid lines to the sky
                // semanticCol.g *= sin(i.uv.x* 100.0);
                // semanticCol.b *= cos(i.uv.y* 100.0);
                // //set alpha to blend rather than overight
                // semanticCol.a *= 0.1f;
                //
                // //mix the main color and the semantic layer
                // return lerp(mainCol,semanticCol, semanticCol.a);

                return mainCol+semanticCol;
            }
            ENDCG
        }
    }
}

Shader "Custom/SemanticShaderGPT2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SemanticTex("_SemanticTex", 2D) = "red" {}
        _OverlayTex("Overlay", 2D) = "black" {}
        _Intensity("Intensity", Range(0,1)) = 1
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

            float _Intensity; 
            float4 _MainTex_ST; 
            float4 _OverlayTex_ST; 

            float4x4 _ViewMatrix; 

            v2f vert (appdata v) 
            { 
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex); 
                o.uv = v.uv; 

                //multiply the uv's by the depth transform to rotate them correctly. 
                o.semantic_uv = mul(_semanticTransform, float4(v.uv, 1.0f, 1.0f)).xyz; 

                //transform vertex position to world space
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                //transform world space position to UV space using world-to-UV mapping matrix
                float4 uvPos = mul(unity_WorldToObject, worldPos);
                o.overlay_uv = uvPos.xy;

                return o; 
            } 

            #pragma shader_feature _requires_view_matrix 

            fixed4 frag (v2f i) : SV_Target 
            { 
                //unity scene 
                float4 mainCol = tex2D(_MainTex, i.uv); 
                //our semantic texture), we need to normalise the uv coords before using. 
                float2 semanticUV = float2(i.semantic_uv.x / i.semantic_uv.z, i.semantic_uv.y / i.semantic_uv.z); 
                //read the semantic texture pixel 
                float4 semanticCol = tex2D(_SemanticTex, semanticUV); 

                //sample the overlay texture using the new UV coordinates
                fixed4 OverlayPix = tex2D(_OverlayTex, i.overlay_uv); 

                #if defined(_requires_view_matrix) 
                    UNITY_SETUP_INSTANCE_ID(v);  
                    _ViewMatrix = UNITY_MATRIX_V;  
                #endif 

                return mainCol + semanticCol * OverlayPix * _Intensity; //Wanna dimmer the effects of the overlay texture: /2 
            } 
            ENDCG 
        } 
    } 
}
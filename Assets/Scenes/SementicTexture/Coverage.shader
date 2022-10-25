Shader "Custom/Coverage"
{
    Properties{
        _MainTex("Main Texture", 2D) = "black" {}
        _OverlayTex("Overlay Texture", 2D) = "black" {} 
        _Direction("Coverage Direction", Vector) = (0,1,0) //Can be x,y,z for different direction to be covered (Y: top)
        _Intensity("Intensity", Range(0,1)) = 1
    }
    
    SubShader{
        
        Pass{
            CGPROGRAM

            #pragma vertx vertexFunc
            #pragma fragment fragmentFunc
            #include "UnityCG.cginc"

            struct v2f //Defining what v2f properties for v2f Func
            {
                float4 pos: SV_POSITION;
                float3 normal: NORMAL;
                float2 uv_Main: TEXCOORD0;
                float2 uv_Overlay: TEXCOORD1; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST; //Scale and Translation
            sampler2D _OverlayTex;
            float4 _OverlayTex_ST;

            v2f vertexFunc (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv_Main = TRANSFORM_TEX(v.texcoord, _MainTex);        //Sampler
                o.uv_Overlay = TRANSFORM_TEX(v.texcoord, _OverlayTex);  //Sampler
                o.normal = mul(unity_ObjectToWorld, v.normal);    //Getting object to world position
                
                return o;
            }

            //Mapping tex

            float3 _Direction;
            fixed  _Intensity;

            //Fragment Shader - render
            fixed4 fragmentFunc (v2f i) : COLOR
            {
                fixed dir = dot(normalize(i.normal), _Direction); //Direction to be render in the object/Mat
                if (dir < 1 - _Intensity)
                {
                    dir = 0;
                }

                fixed4 tex1 = tex2D(_MainTex, i.uv_Main);       //Pixel rendering
                fixed4 tex2 = tex2D(_OverlayTex, i.uv_Overlay);

                return lerp(tex1, tex2, dir);
            }
            
            ENDCG
        }
    }
}

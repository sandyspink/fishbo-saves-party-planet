Shader "Custom/WorldCurvature"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _CurvatureX ("Curvature X", Range(0, 0.1)) = 0.01
        _CurvatureY ("Curvature Y", Range(0, 0.1)) = 0.01
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
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
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _CurvatureX;
            float _CurvatureY;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Convert to world space first
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                
                // Get distance from camera in X and Z
                float3 viewPos = worldPos.xyz - _WorldSpaceCameraPos.xyz;
                float distanceX = viewPos.x;
                float distanceZ = viewPos.z;
                
                // Apply curvature based on distance from camera
                // The further from camera, the more it curves down
                float curvePower = distanceZ * distanceZ * _CurvatureY + distanceX * distanceX * _CurvatureX;
                worldPos.y -= curvePower * 0.01;
                
                // Convert back to clip space
                o.vertex = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = worldPos.xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Simple lighting
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0, dot(i.normal, lightDir));
                
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Apply simple lighting
                col.rgb *= (ndotl * 0.5 + 0.5); // Half lambert for softer shadows
                
                // Add distance fog for atmosphere
                float distance = length(i.worldPos - _WorldSpaceCameraPos.xyz);
                float fog = 1.0 - saturate(distance / 100.0);
                col.rgb = lerp(float3(0.7, 0.7, 0.8), col.rgb, fog);
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}
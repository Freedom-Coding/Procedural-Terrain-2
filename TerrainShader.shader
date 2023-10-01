Shader "Custom/TerrainShader"
{
    Properties
    {
        _textureScale("Texture scale", float) = 1
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        #define MAX_TEXTURES 32

        float _textureScale;
        float minTerrainHeight;
        float maxTerrainHeight;

        float terrainHeights[MAX_TEXTURES];
        UNITY_DECLARE_TEX2DARRAY(terrainTextures);

        int numTextures;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 scaledWorldPos = IN.worldPos / _textureScale;
            float3 worldPosY = IN.worldPos.y;

            float heightValue = saturate((worldPosY - minTerrainHeight) / (maxTerrainHeight - minTerrainHeight));

            int layerIndex = -1;

            for (int i = 0; i < numTextures - 1; i++) 
            {
                if (heightValue >= terrainHeights[i] && heightValue <= terrainHeights[i + 1]) 
                {
                    layerIndex = i;
                    break;
                }
            }

            if (layerIndex == -1) 
            {
                layerIndex = numTextures - 1;
            }

            o.Albedo = UNITY_SAMPLE_TEX2DARRAY(terrainTextures, float3(scaledWorldPos.xz, layerIndex));
        }
        ENDCG
    }
    FallBack "Diffuse"
}

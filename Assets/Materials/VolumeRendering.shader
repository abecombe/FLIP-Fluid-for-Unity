Shader "VolumeRendering/VolumeRendering"
{
    CGINCLUDE
    #include "UnityCG.cginc"

    float3 _Color;
    sampler3D _VolumeTexture;
    float _SamplingDistance;
    uint _Iteration;

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 localPos : TEXCOORD0;
        float3 worldPos : TEXCOORD1;
    };

    // --------------------------------------------------------------------
    // Vertex Shader
    // --------------------------------------------------------------------
    v2f Vertex(appdata_full v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.localPos = v.vertex;
        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        return o;
    }

    // --------------------------------------------------------------------
    // Fragment Shader
    // --------------------------------------------------------------------
    half4 Fragment(v2f i) : SV_Target
    {
        const float3 world_dir = i.worldPos - _WorldSpaceCameraPos;
        const float3 local_dir = normalize(mul(unity_WorldToObject, world_dir));
        const float3 local_step = local_dir * _SamplingDistance;
        float3 local_pos = i.localPos;
        float alpha = 0;

        [loop]
        for (uint i = 0; i < _Iteration; i++)
        {
            const float val = tex3D(_VolumeTexture, local_pos + 0.5).r;
            alpha += (1 - alpha) * val;
            local_pos += local_step;
            if (any(abs(local_pos) > 0.5f)) break;
        }

        return float4(_Color, alpha);
    }
    ENDCG

    Properties {}

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Cull Back
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target   5.0
            #pragma vertex   Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
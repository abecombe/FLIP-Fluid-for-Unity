Shader "VolumeRendering/VolumeRendering"
{
    CGINCLUDE
    #include "UnityCG.cginc"

    float3 _Color;
    sampler3D _VolumeTexture;
    int _Iteration;

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float4 localPos : TEXCOORD0;
        float4 worldPos : TEXCOORD1;
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
        float3 wdir = i.worldPos - _WorldSpaceCameraPos;
        float3 ldir = normalize(mul(unity_WorldToObject, wdir));
        float3 lstep = ldir / _Iteration;
        float3 lpos = i.localPos;
        float alpha = 0;

        [loop]
        for (int i = 0; i < _Iteration; ++i)
        {
            float a = tex3D(_VolumeTexture, lpos + 0.5).r;
            alpha += (1 - alpha) * a;
            lpos += lstep;
            if (!all(max(0.5 - abs(lpos), 0.0))) break;
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
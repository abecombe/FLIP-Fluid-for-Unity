Shader "ParticleRendering/ParticleInstance"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    StructuredBuffer<float4> _ParticleRenderingBuffer;

    float _Radius;
    float _NearClipPlane;
    float _FarClipPlane;

    float2 _VelocityRange;
    float4 _SlowColor;
    float4 _FastColor;
    float _FresnelPower;

    struct v2g
    {
        float4 particle_data : TEXCOORD0;
    };

    struct g2f
    {
        float4 vertex : SV_POSITION;
        float3 camera_space_pos : TEXCOORD0;
        nointerpolation float3 camera_space_sphere_center_pos : TEXCOORD1;
        nointerpolation float sphere_radius : TEXCOORD2;
        nointerpolation float4 color : TEXCOORD3;
    };

    inline float Remap(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return out_min + saturate((x - in_min) / (in_max - in_min)) * (out_max - out_min);
    }

    // --------------------------------------------------------------------
    // Vertex Shader
    // --------------------------------------------------------------------
    v2g Vertex(uint id : SV_VertexID)
    {
        v2g o;

        o.particle_data = _ParticleRenderingBuffer[id];

        return o;
    }

    // --------------------------------------------------------------------
    // Geometry Shader
    // --------------------------------------------------------------------
    static const float quad_mesh_radius = 1.0f;
    static const float4 quad_mesh_vertices[4] = {
        float4(-1, -1, 0, 1),
        float4(-1, 1, 0, 1),
        float4(1, -1, 0, 1),
        float4(1, 1, 0, 1),
    };
    [maxvertexcount(4)]
    void Geometry(point v2g p[1], inout TriangleStream<g2f> stream)
    {
        const float3 world_space_sphere_center_pos = p[0].particle_data.xyz;
        const float sphere_radius = _Radius;

        const float3 world_space_camera_pos = _WorldSpaceCameraPos;
        const float3 view_vec = world_space_camera_pos - world_space_sphere_center_pos;
        const float sphere_cam_dist = length(view_vec);

        // Quadをカメラ方向に向けるための軸を計算
        // UNITY_MATRIX_V = Camera.worldToCameraMatrix: 右手座標系（z軸が手前）
        const float3 z_axis = normalize(-view_vec);
        const float3 x_axis = normalize(cross(UNITY_MATRIX_V._m10_m11_m12, z_axis));
        const float3 y_axis = normalize(cross(z_axis, x_axis));

        // 球の全範囲が描画できる最低限の半径を計算
        const float nessesary_radius = sphere_cam_dist * sphere_radius / sqrt(sphere_cam_dist * sphere_cam_dist - sphere_radius * sphere_radius);
        const float scale = nessesary_radius / quad_mesh_radius;

        // Quadの頂点のワールド座標を導出するための行列を計算
        float4x4 mat = 0;
        mat._m00_m10_m20 = x_axis;
        mat._m01_m11_m21 = y_axis;
        mat._m02_m12_m22 = z_axis;
        mat._m00_m10_m20 *= scale;
        mat._m01_m11_m21 *= scale;
        mat._m03_m13_m23 = world_space_sphere_center_pos;
        mat._m33 = 1;

        // ZTestで淘汰されないために、vertexは球の最前部に移動させる
        const float scale_multiplier_for_vertex = (sphere_cam_dist - sphere_radius) / sphere_cam_dist;
        float4x4 mat_for_vertex = mat;
        mat_for_vertex._m00_m10_m20 *= scale_multiplier_for_vertex;
        mat_for_vertex._m01_m11_m21 *= scale_multiplier_for_vertex;
        mat_for_vertex._m03_m13_m23 += view_vec * sphere_radius / sphere_cam_dist;

        const float remap_speed = Remap(p[0].particle_data.w, _VelocityRange.x, _VelocityRange.y, 0, 1);
        const float4 color = lerp(_SlowColor, _FastColor, remap_speed);

        for (int i = 0; i < 4; i++)
        {
            g2f o;

            o.sphere_radius = sphere_radius;

            const float3 world_space_pos = mul(mat, quad_mesh_vertices[i]).xyz;
            o.camera_space_pos = mul(UNITY_MATRIX_V, float4(world_space_pos, 1)).xyz;
            o.camera_space_sphere_center_pos = mul(UNITY_MATRIX_V, float4(world_space_sphere_center_pos, 1)).xyz;

            o.vertex = mul(UNITY_MATRIX_VP, float4(mul(mat_for_vertex, quad_mesh_vertices[i]).xyz, 1));

            o.color = color;

            stream.Append(o);
        }
        stream.RestartStrip();
    }

    // --------------------------------------------------------------------
    // Fragment Shader
    // --------------------------------------------------------------------
    float4 Fragment(g2f i, out float depth_test : SV_DepthLessEqual) : SV_Target
    {
        // カメラ空間での球と直線の交点から、深度を計算
        const float3 m = normalize(i.camera_space_pos);
        const float3 minus_a = -i.camera_space_sphere_center_pos;
        const float dot_m_minus_a = dot(m, minus_a);
        const float len_a = length(minus_a);
        const float r = i.sphere_radius;

        const float D = dot_m_minus_a * dot_m_minus_a - (len_a * len_a - r * r);
        // 交点がない場合は描画しない
        if (D < 0) discard;

        // カメラ空間のz座標、右手座標系なので画面内のz座標はマイナス
        const float3 camera_space_pos = (-dot_m_minus_a - sqrt(D)) * m;
        const float depth = camera_space_pos.z;

        if (-depth < _NearClipPlane) discard;

        // SV_DepthLessEqualにはNDC空間での深度を設定すればよさそう
        depth_test = (UNITY_MATRIX_P._m22 * depth + UNITY_MATRIX_P._m23) / (UNITY_MATRIX_P._m32 * depth + UNITY_MATRIX_P._m33);

        float4 color = i.color;

        // Fresnel反射
        const float3 camera_space_normal = normalize(camera_space_pos - i.camera_space_sphere_center_pos);
        const float3 camera_space_view_dir = normalize(-camera_space_pos);

        const float vdotn = dot(camera_space_view_dir, camera_space_normal);
        const float fresnel = 1 + (1.0 - _FresnelPower) * pow(1.0 - vdotn, 5.0) / _FresnelPower;

        color.rgb *= fresnel;

        return color;
    }

    ENDCG

    Properties
    {
    }

    SubShader
    {
        Tags{ "RenderType" = "Opaque" }

        Cull Back
        ZClip Off
        ZWrite On
        ZTest LEqual
        Blend One Zero
        BlendOp Add

        Pass
        {
            CGPROGRAM
            #pragma target   5.0
            #pragma vertex   Vertex
            #pragma geometry Geometry
            #pragma fragment Fragment
            ENDCG
        }
    }
}
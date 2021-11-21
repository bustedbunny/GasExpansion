Shader "Custom/InstancedIndirectColor" {
        Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            }; 

            struct MeshProperties {
                int index;
                float alpha;
                int color;
            };

            float4 RotateAroundYInDegrees (float4 vertex, float alpha) {
                    float sina, cosa;
                    sincos(alpha, sina, cosa);
                    float2x2 m = float2x2(cosa, -sina, sina, cosa);
                    return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            float _MaxAlpha;

            float _Angle;

            StructuredBuffer<MeshProperties> _MeshProperties;
            StructuredBuffer<float4x4> _Matrices;
            StructuredBuffer<float4> _Colors;


            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) { 
                v2f o;
                o.uv = i.uv;

                float4 rotated = RotateAroundYInDegrees(i.vertex, _Angle);
                float4 pos = mul(_Matrices[_MeshProperties[instanceID].index], rotated);
                pos.yw += _MeshProperties[instanceID].color * 0.001;
                o.vertex = UnityObjectToClipPos(pos);

                float alpha = _MeshProperties[instanceID].alpha;
                if (alpha > _MaxAlpha){
                    alpha = _MaxAlpha;
                }
                alpha /= _MaxAlpha;
                o.color = _Colors[_MeshProperties[instanceID].color];
                o.color.a = alpha;
                
                return o;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                return col;
            }


            ENDCG
        }
    }
}
Shader "Custom/SizedPointShader"
{
    Properties
    {
        _PointSize("Point Size", Range(0, 10)) = 1
        _PointColors("Point Colors", Color) = (1, 1, 1, 1)
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float size : POINT_SIZE;
                fixed4 color : COLOR;
            };

            float _PointSize;
            fixed4 _PointColors;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.size = _PointSize;
                o.color = v.color * _PointColors;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}

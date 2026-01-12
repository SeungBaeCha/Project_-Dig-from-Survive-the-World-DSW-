Shader "Custom/FoggedUI"
{
    Properties
    {
        // UI Shader에서 흔히 사용하는 프로퍼티들
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 100

        Pass
        {
            // UI를 위한 표준 블렌딩 모드
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // 이 지시문이 안개를 계산하는 여러 버전의 셰이더를 컴파일하도록 함
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            // 정점(Vertex) 데이터 구조
            struct appdata
            {
                float4 vertex : POSITION;    // 정점 위치
                float2 uv : TEXCOORD0;       // UV 좌표
                fixed4 color : COLOR;        // Image 컴포넌트의 Color 속성
            };

            // 정점에서 프래그먼트(픽셀) 셰이더로 전달될 데이터 구조
            struct v2f
            {
                float2 uv : TEXCOORD0;
                // 안개 좌표를 저장하기 위한 매크로
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; // 클립 공간에서의 정점 위치
                fixed4 color : COLOR;        // 최종적으로 계산될 색상
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            // 정점 셰이더
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Image 컴포넌트의 색상과 머티리얼의 Tint 색상을 곱함
                o.color = v.color * _Color;
                // 정점의 안개 데이터를 계산하는 매크로
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            // 프래그먼트(픽셀) 셰이더
            fixed4 frag (v2f i) : SV_Target
            {
                // 텍스처 색상과 조합된 색상을 곱하여 기본 색상을 계산
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // 계산된 안개 데이터를 기반으로 최종 색상에 안개를 적용
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}

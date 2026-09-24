Shader "MichaelManor/AngleRevealText"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("Writing Color", Color) = (0.45,0.9,1,1)
        _ObliqueStart ("Oblique Start", Range(0,1)) = 0.18
        _ObliqueFull ("Oblique Full", Range(0,1)) = 0.58
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_ST; fixed4 _FaceColor;
            float _ObliqueStart, _ObliqueFull;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 worldPos:TEXCOORD1; float3 worldNormal:TEXCOORD2; fixed4 color:COLOR; };
            v2f vert(appdata v)
            {
                v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.uv=TRANSFORM_TEX(v.uv,_MainTex);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex).xyz; o.worldNormal=UnityObjectToWorldNormal(v.normal); o.color=v.color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                float3 viewDir=normalize(_WorldSpaceCameraPos-i.worldPos);
                float oblique=1.0-abs(dot(normalize(i.worldNormal),viewDir));
                float reveal=smoothstep(_ObliqueStart,_ObliqueFull,oblique);
                float distanceField=tex2D(_MainTex,i.uv).a;
                float glyph=smoothstep(0.42,0.58,distanceField);
                fixed4 color=_FaceColor*i.color; color.a*=glyph*reveal; return color;
            }
            ENDCG
        }
    }
}

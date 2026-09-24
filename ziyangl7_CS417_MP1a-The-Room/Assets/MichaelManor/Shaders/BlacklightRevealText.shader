Shader "MichaelManor/BlacklightRevealText"
{
    Properties
    {
        _MainTex ("Font Atlas", 2D) = "white" {}
        _FaceColor ("UV Ink", Color) = (0.72,0.25,1,1)
        _ConeThreshold ("Beam Cone", Range(-1,1)) = 0.90
        _BlacklightRange ("Range", Float) = 7
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
            float4 _BlacklightPosition; float4 _BlacklightDirection; float _ConeThreshold; float _BlacklightRange;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; float3 worldPos:TEXCOORD1; fixed4 color:COLOR; };
            v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);o.worldPos=mul(unity_ObjectToWorld,v.vertex).xyz;o.color=v.color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float3 delta=i.worldPos-_BlacklightPosition.xyz; float distanceToLight=length(delta);
                float cone=dot(normalize(delta),normalize(_BlacklightDirection.xyz));
                float beam=smoothstep(_ConeThreshold,1.0,cone)*saturate(1.0-distanceToLight/_BlacklightRange);
                float glyph=smoothstep(0.42,0.58,tex2D(_MainTex,i.uv).a);
                fixed4 color=_FaceColor*i.color;color.a*=glyph*beam;return color;
            }
            ENDCG
        }
    }
}

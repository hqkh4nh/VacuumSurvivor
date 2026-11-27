Shader "VacuumSurvivor/WeaponContrast"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline", Color) = (0.055,0.12,0.2,1)
        _OutlinePixels ("Outline pixels", Float) = 10
        [Toggle] _Stroke ("Line effect", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Cull Off ZWrite Off Lighting Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color, _OutlineColor;
            float _OutlinePixels, _Stroke;
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct Output { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            Output vert(Input v)
            {
                Output o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color * _Color; return o;
            }
            fixed4 frag(Output i) : SV_Target
            {
                fixed4 result;
                if (_Stroke > 0.5)
                {
                    float distance = abs(i.uv.y - 0.5);
                    float core = 1 - smoothstep(0.28, 0.46, distance);
                    fixed3 light = lerp(i.color.rgb, fixed3(1,1,1), 0.35 * (1 - smoothstep(0.04,0.16,distance)));
                    result = fixed4(lerp(_OutlineColor.rgb, light, core), i.color.a * (1 - smoothstep(0.47,0.5,distance)));
                }
                else
                {
                    fixed4 sprite = tex2D(_MainTex,i.uv);
                    float2 d = _MainTex_TexelSize.xy * _OutlinePixels;
                    float edge = sprite.a;
                    edge = max(edge, tex2D(_MainTex,i.uv+float2(d.x,0)).a);
                    edge = max(edge, tex2D(_MainTex,i.uv-float2(d.x,0)).a);
                    edge = max(edge, tex2D(_MainTex,i.uv+float2(0,d.y)).a);
                    edge = max(edge, tex2D(_MainTex,i.uv-float2(0,d.y)).a);
                    edge = max(edge, tex2D(_MainTex,i.uv+d*0.707).a);
                    edge = max(edge, tex2D(_MainTex,i.uv-d*0.707).a);
                    edge = max(edge, tex2D(_MainTex,i.uv+float2(d.x,-d.y)*0.707).a);
                    edge = max(edge, tex2D(_MainTex,i.uv+float2(-d.x,d.y)*0.707).a);
                    result = fixed4(lerp(_OutlineColor.rgb, sprite.rgb*i.color.rgb, sprite.a),edge*i.color.a);
                }
                result.rgb *= result.a;
                return result;
            }
            ENDCG
        }
    }
}

Shader "Custom/Stencil/Diffuse NotEqualOne"
{

Properties
{
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader
{
	Tags { "RenderType"="Transparent" "Queue"="Geometry+1" }
	LOD 200
	Blend SrcAlpha OneMinusSrcAlpha
	ColorMask RGB

	Stencil
	{
		Ref 1
		Comp notequal
		Pass keep
	}

        Pass
        {
            Cull Back
//            ZTest Less
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            half4 frag(v2f i) : COLOR
            {
                return half4(0.3,0.3,0.8,0.5);
            }
            
            ENDCG
        }
}

Fallback "VertexLit"
}

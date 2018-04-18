// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/UIOutline"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _RenderTexture ("Render Texture", 2D) = "black" {}
		_Color ("Tint", Color) = (1,1,1,1)
		_OutlineStrength ("Outline Strength", float) = 0
		_OutlineColor ("Outline color", Color) = (1, 1, 1, 1)
		[MaterialToggle] _OutlineColorFollowMeshColor ("Outline color follow the mesh color", int) = 0
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]
		
		Pass
		{
		Name "OutlinePass"
		
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float3 normal   : NORMAL;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			uniform float _OutlineStrength;
			uniform fixed4 _OutlineColor;
			uniform bool _OutlineColorFollowMeshColor;
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
				if (_OutlineStrength > 0)
				{
				    OUT.vertex = UnityObjectToClipPos(IN.vertex);
				    
				    float3 normal = mul((float3x3)UNITY_MATRIX_IT_MV, IN.normal);
				    float2 offset = TransformViewToProjection(normal.xy);
				   
				    OUT.vertex.xy += offset * OUT.vertex.z * _OutlineStrength;
				    if (_OutlineColorFollowMeshColor)
				        OUT.color = _OutlineColor;
                    else
                        OUT.color = IN.color * _OutlineColor;
				}
				else
				{
				    OUT.color = IN.color;
				}
				
				return OUT;
			}
			
			fixed4 frag(v2f IN) : SV_Target
			{  
			    half4 color;
			    if (!_OutlineColorFollowMeshColor)
			    {
			        half4 mainTex = tex2D(_MainTex, IN.texcoord) * IN.color;
			        color = mainTex;
                }
                else
                {
			        half4 mainTex = tex2D(_MainTex, IN.texcoord) * IN.color;
			        color = IN.color;
			        color.a = mainTex.a;
                }
                if (color.a != 0 && _OutlineStrength > 0)
                {     
                    [unroll(16)]
                    for (int i = 1; i < _OutlineStrength + 1; i++)
                    {
                        fixed4 currentPixel = tex2D(_MainTex, IN.texcoord + fixed2(0, 0));
                        fixed4 pixelUp = tex2D(_MainTex, IN.texcoord + fixed2(0, i * _MainTex_TexelSize.y));
                        fixed4 pixelDown = tex2D(_MainTex, IN.texcoord - fixed2(0,i *  _MainTex_TexelSize.y));
                        fixed4 pixelRight = tex2D(_MainTex, IN.texcoord + fixed2(i * _MainTex_TexelSize.x, 0));
                        fixed4 pixelLeft = tex2D(_MainTex, IN.texcoord - fixed2(i * _MainTex_TexelSize.x, 0));

                        if (currentPixel.a <= 0.5)
                        {
                            if (pixelUp.a > 0 || pixelDown.a > 0 || pixelRight.a > 0 || pixelLeft.a > 0)
                                color.rgba = fixed4(1, 1, 1, 1) * _OutlineColor;
                        }
                    }
                }
                
                color.rgb *= color.a;
			    
				return color;
			}
		ENDCG
		}
	}
}

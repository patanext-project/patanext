Shader "Sprites/DefaultColorFlash"
{
	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1,1,1,1)
		_FlashColor("Flash Color", Color) = (1,1,1,1)
		_FlashAmount("Flash Amount",Range(0.0,1.0)) = 0.0
		[MaterialToggle] _FullAlpha("Full Alpha", Float) = 0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[MaterialToggle]_UseGrayScale("Use Gray Scale", Float) = 0
		_EffectAmount("Effect Amount", Range(0, 1)) = 1.0
		[MaterialToggle] UseGrad("Use Gradients", Float) = 0
		_GradLeftColor("Gradient Left Color", Color) = (0, 0, 0, 0)
		_GradRightColor("Gradient Right Color", Color) = (1, 1, 1, 1)
		[MaterialToggle] UseCustomPos("Use Custom Gradient Position", Float) = 0
		_Position("Custom Position", Vector) = (0, 0, 0)
		[MaterialToggle] _VerticalGradient("Vertical Gradient", Float) = 0
		[HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

	Pass
	{
		CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #include "UnitySprites.cginc"

		fixed4 _FlashColor;
		float _FlashAmount;
		fixed4 _GradLeftColor;
		fixed4 _GradRightColor;
		uniform bool _FullAlpha;
		uniform bool UseGrad;
		uniform bool UseCustomPos;
		float2 _Position;
		uniform bool _VerticalGradient;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			
                #ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
                #endif

			IN.vertex.xy *= _Flip.xy;

			OUT.vertex = UnityObjectToClipPos(IN.vertex);
			OUT.texcoord = IN.texcoord;
			OUT.color = IN.color * _Color * _RendererColor;

			float position = _Position;
			if (!UseCustomPos)
			{
				position = OUT.texcoord.x;
				if (_VerticalGradient)
					position = OUT.texcoord.y;
			}

			if (UseGrad)
			{
				OUT.color = IN.color * lerp(_GradLeftColor * _Color, _GradRightColor * _Color, position);
			}
	#ifdef PIXELSNAP_ON
			OUT.vertex = UnityPixelSnap(OUT.vertex);
	#endif

			return OUT;
		}

		uniform float _EffectAmount;
		uniform bool _UseGrayScale;

		fixed4 frag(v2f IN) : COLOR
		{
			if (!_UseGrayScale)
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				//c.a = pow(c.a + 0.055, 2.4) / 1.13711896582;
				//c.a = pow(c.a, 2.2);

				if (_FullAlpha)
				{
					if (c.a <= 0.8)
					{
					    c.a = 0;
					    c.a += ((_ScreenParams.zw+1.0) *float2(-1, 1));
					}
					else
						c.a += (_ScreenParams.zw-1.0) *float2(-1, 1);
				}
				c.rgb = lerp(c.rgb,_FlashColor.rgb,_FlashAmount);
				c.rgb *= c.a;

				return c;
			}
			half4 texcol = tex2D(_MainTex, IN.texcoord);

			if (_FullAlpha)
			{
				if (texcol.a <= 0.5)
					texcol.a = 0.1;
				else
					texcol.a = 1;
			}

			//texcol.a = pow(texcol.a + 0.055, 2.4) / 1.13711896582;

			texcol.rgb = lerp(texcol.rgb, dot(texcol.rgb, float3(0.3, 0.59, 0.11)) + _FlashAmount, _EffectAmount);
			texcol.rgb *= texcol.a;
			texcol = texcol * IN.color;

			//texcol.a = pow(texcol.a, 1/2.2);

			//texcol.a = _GradLeftColor.a + _GradRightColor.a;
			//texc.Alpha = 0;

			return texcol;
		}
		ENDCG
		}
	}
}
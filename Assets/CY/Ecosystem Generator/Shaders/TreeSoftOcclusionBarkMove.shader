Shader "Nature/Tree Soft Occlusion Bark Move" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,0)
		_MainTex ("Main Texture", 2D) = "white" {}
		_BaseLight ("Base Light", Range(0, 1)) = 0.35
		_AO ("Amb. Occlusion", Range(0, 10)) = 2.4
		
		// These are here only to provide default values
		_Scale ("Scale", Vector) = (1,1,1,1)
		_SquashAmount ("Squash", Float) = 1
	}
	
	SubShader {
		Tags {
			"IgnoreProjector"="True"
			"RenderType" = "TreeOpaque"
		}

		Pass {
			Lighting On
		
			CGPROGRAM
			#pragma vertex bark
			#pragma fragment frag 
			#include "SH_Vertex_Move.cginc"
			
			sampler2D _MainTex;
			
			fixed4 frag(v2f input) : COLOR
			{
				fixed4 col = input.color;
				col.rgb *= 2.0f * tex2D( _MainTex, input.uv.xy).rgb;
				return col;
			}
			ENDCG
		}
		
		Pass {
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
			
			Fog {Mode Off}
			ZWrite On ZTest Less Cull Off
			Offset 1, 1
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"
			
			struct v2f { 
				V2F_SHADOW_CASTER;
			};
			
			struct appdata {
			    float4 vertex : POSITION;
			    fixed4 color : COLOR;
			};
			v2f vert( appdata v )
			{
				v2f o;
float4 Split4=v.vertex;
float4 Multiply4=v.vertex * float4( 1000,1000,1000,1000);
float4 Split0=Multiply4;
float4 Multiply3=_Time * float4( 0.25,0.25,0.25,0.25 );
float4 Add4=float4( Split0.x, Split0.x, Split0.x, Split0.x) + Multiply3;
float4 Cos0=cos(Add4);
float4 Multiply0=Cos0 * float4( 0.01,0.01,0.01,0.01 );
float4 Add1=float4( Split4.x, Split4.x, Split4.x, Split4.x) + Multiply0;
float4 Add3=float4( Split0.y, Split0.y, Split0.y, Split0.y) + Multiply3;
float4 Cos1=cos(Add3);
float4 Multiply1=Cos1 * float4( 0.01,0.01,0.01,0.01 );
float4 Add2=float4( Split4.y, Split4.y, Split4.y, Split4.y) + Multiply1;
float4 Add5=float4( Split0.z, Split0.z, Split0.z, Split0.z) + Multiply3;
float4 Cos2=cos(Add5);
float4 Multiply2=Cos2 * float4( 0.01,0.01,0.01,0.01 );
float4 Add0=float4( Split4.z, Split4.z, Split4.z, Split4.z) + Multiply2;
float4 Assemble0=float4(Add1.x, Add2.y, Add0.z, float4( Split4.w, Split4.w, Split4.w, Split4.w).w);
				TerrainAnimateTree(Assemble0, v.color.w);
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}
			
			float4 frag( v2f i ) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG	
		}
	}
	
	SubShader {
		Tags {
			"IgnoreProjector"="True"
			"RenderType" = "TreeOpaque"
		}

		Pass {
			CGPROGRAM
			#pragma exclude_renderers gles xbox360 ps3
			#pragma vertex bark
			#include "SH_Vertex_Move.cginc"
			ENDCG
			
			Lighting On
						
			SetTexture [_MainTex] { combine primary * texture DOUBLE, constant }
		}
	}
	
	SubShader {
		Tags {
			"IgnoreProjector"="True"
			"RenderType" = "Opaque"
		}
		Pass {
			Tags { "LightMode" = "Vertex" }
			Lighting On
			Material {
				Diffuse [_Color]
				Ambient [_Color]
			}
			SetTexture [_MainTex] { combine primary * texture DOUBLE, constant }
		}		
	}
	
	Dependency "BillboardShader" = "Hidden/Nature/Tree Soft Occlusion Bark Rendertex Move"
	Fallback Off
}

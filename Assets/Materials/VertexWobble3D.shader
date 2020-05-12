// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Kardia/VertexWobble3D"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Power("Power", Int) = 10
		_NoiseScale("NoiseScale", Range( 0 , 100)) = 20
		_TimeScale("TimeScale", Range( 0 , 1)) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		LOD 100
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float _TimeScale;
		uniform float _NoiseScale;
		uniform int _Power;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Cutoff = 0.5;


		float3 mod3D289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod3D289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod3D289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod3D289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime4_g2 = _Time.y * _TimeScale;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 break7_g2 = ( mulTime4_g2 + mul( float4( ase_worldPos , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 appendResult11_g2 = (float3(0.0 , break7_g2.y , break7_g2.z));
			float temp_output_23_0_g2 = _NoiseScale;
			float simplePerlin3D15_g2 = snoise( appendResult11_g2*temp_output_23_0_g2 );
			int temp_output_24_0_g2 = _Power;
			float3 appendResult9_g2 = (float3(break7_g2.x , 0.0 , 0.0));
			float simplePerlin3D14_g2 = snoise( appendResult9_g2*temp_output_23_0_g2 );
			float3 appendResult8_g2 = (float3(break7_g2.x , 0.0 , 0.0));
			float simplePerlin3D13_g2 = snoise( appendResult8_g2*temp_output_23_0_g2 );
			float3 appendResult19_g2 = (float3(( simplePerlin3D15_g2 / temp_output_24_0_g2 ) , ( simplePerlin3D14_g2 / temp_output_24_0_g2 ) , ( simplePerlin3D13_g2 / temp_output_24_0_g2 )));
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz = ( appendResult19_g2 + ase_vertex3Pos );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode29 = tex2D( _MainTex, uv_MainTex );
			o.Albedo = tex2DNode29.rgb;
			o.Alpha = 1;
			clip( tex2DNode29.a - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
0;642;1349;359;1352.894;137.7716;1.487129;True;False
Node;AmplifyShaderEditor.RangedFloatNode;68;-488.4123,205.4461;Inherit;False;Property;_TimeScale;TimeScale;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-488.4123,285.446;Inherit;False;Property;_NoiseScale;NoiseScale;2;0;Create;True;0;0;False;0;20;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;39;-371.4123,125.4461;Inherit;False;Property;_Power;Power;1;0;Create;True;0;0;False;0;10;0;0;1;INT;0
Node;AmplifyShaderEditor.SamplerNode;29;-84.2363,-17.66653;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;81;-195.1266,188.6758;Inherit;False;VertexWobble;-1;;2;6d1231fd3090b2b4bac5f79907dd2d0f;0;3;24;INT;0;False;22;FLOAT;0;False;23;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;224.4219,-63.51244;Float;False;True;-1;4;ASEMaterialInspector;100;0;Lambert;Kardia/VertexWobble3D;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;20;0,0,0,1;VertexScale;True;False;Cylindrical;False;Absolute;100;;4;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;81;24;39;0
WireConnection;81;22;68;0
WireConnection;81;23;65;0
WireConnection;0;0;29;0
WireConnection;0;10;29;4
WireConnection;0;11;81;0
ASEEND*/
//CHKSM=970DB0970F0C1E1A125B9091CF43FD190C7F6A66
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _TimeScale;
		uniform float _NoiseScale;
		uniform int _Power;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float mulTime66 = _Time.y * _TimeScale;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 break52 = ( mulTime66 + mul( float4( ase_vertex3Pos , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 appendResult55 = (float3(0.0 , break52.y , break52.z));
			float simplePerlin2D20 = snoise( appendResult55.xy*_NoiseScale );
			float3 appendResult57 = (float3(break52.x , 0.0 , break52.z));
			float simplePerlin2D60 = snoise( appendResult57.xy*_NoiseScale );
			float3 appendResult58 = (float3(break52.x , break52.y , 0.0));
			float simplePerlin2D61 = snoise( appendResult58.xy*_NoiseScale );
			float3 appendResult64 = (float3(( simplePerlin2D20 / _Power ) , ( simplePerlin2D60 / _Power ) , ( simplePerlin2D61 / _Power )));
			v.vertex.xyz = ( appendResult64 + ase_vertex3Pos );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			o.Albedo = tex2D( _MainTex, uv_MainTex ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
0;505;1361;496;1250.725;410.8754;1.56984;True;True
Node;AmplifyShaderEditor.PosVertexDataNode;25;-1525.106,181.0491;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;68;-1323.617,125.5207;Inherit;False;Property;_TimeScale;TimeScale;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;26;-1626.688,380.7378;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1331.414,374.2906;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;66;-1230.155,249.7691;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-1028.75,192.2877;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;52;-1084.754,372.6391;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;58;-760.24,441.7305;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;55;-754.9849,197.0006;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;57;-763.729,323.165;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-999.7004,593.3939;Inherit;False;Property;_NoiseScale;NoiseScale;2;0;Create;True;0;0;False;0;20;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;39;-586.1264,575.8805;Inherit;False;Property;_Power;Power;1;0;Create;True;0;0;False;0;10;0;0;1;INT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;20;-598.7471,190.2489;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;60;-614.0475,312.5804;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;61;-612.7996,434.8903;Inherit;False;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;62;-331.9888,307.5881;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;63;-341.973,421.1616;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;24;-332.4805,195.8683;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;35;-96,544;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;64;-124.8105,252.6738;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;29;-917.1717,-84.73818;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;36;80,416;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;224.4219,-59.40508;Float;False;True;-1;2;ASEMaterialInspector;0;0;Lambert;Kardia/VertexWobble3D;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;20;0,0,0,1;VertexScale;True;False;Cylindrical;False;Absolute;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;66;0;68;0
WireConnection;67;0;66;0
WireConnection;67;1;27;0
WireConnection;52;0;67;0
WireConnection;58;0;52;0
WireConnection;58;1;52;1
WireConnection;55;1;52;1
WireConnection;55;2;52;2
WireConnection;57;0;52;0
WireConnection;57;2;52;2
WireConnection;20;0;55;0
WireConnection;20;1;65;0
WireConnection;60;0;57;0
WireConnection;60;1;65;0
WireConnection;61;0;58;0
WireConnection;61;1;65;0
WireConnection;62;0;60;0
WireConnection;62;1;39;0
WireConnection;63;0;61;0
WireConnection;63;1;39;0
WireConnection;24;0;20;0
WireConnection;24;1;39;0
WireConnection;64;0;24;0
WireConnection;64;1;62;0
WireConnection;64;2;63;0
WireConnection;36;0;64;0
WireConnection;36;1;35;0
WireConnection;0;0;29;0
WireConnection;0;11;36;0
ASEEND*/
//CHKSM=97F5EA78F8924A6CC518AF53FFC0249DE8360F02
// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Kardia/VertexWobble3D_Unlit"
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
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		LOD 100
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha , SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _TimeScale;
		uniform float _NoiseScale;
		uniform int _Power;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;


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
			float mulTime66 = _Time.y * _TimeScale;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float3 break52 = ( mulTime66 + mul( float4( ase_worldPos , 0.0 ), unity_ObjectToWorld ).xyz );
			float3 appendResult55 = (float3(0.0 , break52.y , break52.z));
			float simplePerlin3D20 = snoise( appendResult55*_NoiseScale );
			float3 appendResult57 = (float3(break52.x , 0.0 , break52.z));
			float simplePerlin3D60 = snoise( appendResult57*_NoiseScale );
			float3 appendResult58 = (float3(break52.x , break52.y , 0.0));
			float simplePerlin3D61 = snoise( appendResult58*_NoiseScale );
			float3 appendResult64 = (float3(( simplePerlin3D20 / _Power ) , ( simplePerlin3D60 / _Power ) , ( simplePerlin3D61 / _Power )));
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz = ( appendResult64 + ase_vertex3Pos );
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 color82 = IsGammaSpace() ? float4(0,0,0,1) : float4(0,0,0,1);
			float4 lerpResult79 = lerp( tex2D( _MainTex, uv_MainTex ) , color82 , ( 1.0 - i.vertexColor.a ));
			o.Emission = ( lerpResult79 * i.vertexColor ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17500
0;682;1361;319;2000.319;477.1862;2.078514;True;False
Node;AmplifyShaderEditor.RangedFloatNode;68;-1323.617,125.5207;Inherit;False;Property;_TimeScale;TimeScale;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ObjectToWorldMatrixNode;26;-1626.688,380.7378;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.WorldPosInputsNode;69;-1707.599,178.3067;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;66;-1230.155,249.7691;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-1331.414,374.2906;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4x4;1,0,0,0,1,1,0,0,1,0,1,0,0,0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-1028.75,192.2877;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;52;-1084.754,372.6391;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;58;-760.24,441.7305;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;57;-763.729,323.165;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-999.7004,593.3939;Inherit;False;Property;_NoiseScale;NoiseScale;2;0;Create;True;0;0;False;0;20;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;55;-754.9849,195.5502;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;60;-614.0475,312.5804;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;61;-612.7996,434.8903;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;39;-586.1264,575.8805;Inherit;False;Property;_Power;Power;1;0;Create;True;0;0;False;0;10;0;0;1;INT;0
Node;AmplifyShaderEditor.VertexColorNode;70;-1048.094,-124.0911;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;20;-598.7471,190.2489;Inherit;False;Simplex3D;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;24;-332.4805,195.8683;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;62;-331.9888,307.5881;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;63;-341.973,421.1616;Inherit;False;2;0;FLOAT;0;False;1;INT;20;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-1166.021,-498.7806;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;82;-1077.766,-302.0331;Inherit;False;Constant;_Color0;Color 0;4;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;84;-865.58,-29.22424;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;64;-124.8105,252.6738;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;35;-96,544;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;79;-627.5977,-424.3326;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;80,416;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-392.1061,-282.5172;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;224.4219,-61.15542;Float;False;True;-1;4;ASEMaterialInspector;100;0;Unlit;Kardia/VertexWobble3D_Unlit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;2;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;20;0,0,0,1;VertexScale;True;False;Cylindrical;False;Absolute;100;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;66;0;68;0
WireConnection;27;0;69;0
WireConnection;27;1;26;0
WireConnection;67;0;66;0
WireConnection;67;1;27;0
WireConnection;52;0;67;0
WireConnection;58;0;52;0
WireConnection;58;1;52;1
WireConnection;57;0;52;0
WireConnection;57;2;52;2
WireConnection;55;1;52;1
WireConnection;55;2;52;2
WireConnection;60;0;57;0
WireConnection;60;1;65;0
WireConnection;61;0;58;0
WireConnection;61;1;65;0
WireConnection;20;0;55;0
WireConnection;20;1;65;0
WireConnection;24;0;20;0
WireConnection;24;1;39;0
WireConnection;62;0;60;0
WireConnection;62;1;39;0
WireConnection;63;0;61;0
WireConnection;63;1;39;0
WireConnection;84;0;70;4
WireConnection;64;0;24;0
WireConnection;64;1;62;0
WireConnection;64;2;63;0
WireConnection;79;0;29;0
WireConnection;79;1;82;0
WireConnection;79;2;84;0
WireConnection;36;0;64;0
WireConnection;36;1;35;0
WireConnection;83;0;79;0
WireConnection;83;1;70;0
WireConnection;0;2;83;0
WireConnection;0;11;36;0
ASEEND*/
//CHKSM=9996BA397DAD62E6D98F54FB5C13FED31DE6AFA0
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Tutorial/015_vertex_manipulation" {
    //show values to edit in inspector
    Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", color) = (0,0,0)
        _Power("Power", Range(0, 20)) = 1

        
    }
    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        //the shader is a surface shader, meaning that it will be extended by unity in the background 
        //to have fancy lighting and other features
        //our surface shader function is called surf and we use our custom lighting model
        //fullforwardshadows makes sure unity adds the shadow passes the shader might need
        //vertex:vert makes the shader use vert as a vertex shader function
        //addshadows tells the surface shader to generate a new shadow pass based on out vertex shader
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        float _Power;

        

        float random (float2 uv)
        {
            return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
        }

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full data){
            float4 modifiedPos = data.vertex;
            float4 worldPos = mul(unity_ObjectToWorld, data.vertex);

            modifiedPos.x += (random(worldPos.z - worldPos.y) / 20) * _Power;
            modifiedPos.y += (random(worldPos.x - worldPos.z) / 20) * _Power;
            modifiedPos.z += (random(worldPos.y - worldPos.x) / 20) * _Power;

            data.vertex = modifiedPos;
        }

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
            //sample and tint albedo texture
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= _Color;
            o.Albedo = col.rgb;
            //just apply the values for metalness, smoothness and emission
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _Emission;
        }
        ENDCG
    }
    FallBack "Standard"
}
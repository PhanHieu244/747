Shader "Skybox/GradientSkybox" {
	Properties {
		_SkyColor1 ("Top Color", Vector) = (0.37,0.52,0.73,0)
		_SkyExponent1 ("Top Exponent", Float) = 8.5
		_SkyColor2 ("Horizon Color", Vector) = (0.89,0.96,1,0)
		_SkyColor3 ("Bottom Color", Vector) = (0.89,0.89,0.89,0)
		_SkyExponent2 ("Bottom Exponent", Float) = 3
		_SkyIntensity ("Sky Intensity", Float) = 1
		_SunColor ("Sun Color", Vector) = (1,0.99,0.87,1)
		_SunIntensity ("Sun Intensity", Float) = 2
		_SunAlpha ("Sun Alpha", Float) = 550
		_SunBeta ("Sun Beta", Float) = 1
		_SunVector ("Sun Vector", Vector) = (0.269,0.615,0.74,0)
		_SunAzimuth ("Sun Azimuth (editor only)", Float) = 20
		_SunAltitude ("Sun Altitude (editor only)", Float) = 38
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard
#pragma target 3.0

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			o.Albedo = 1;
		}
		ENDCG
	}
	//CustomEditor "HorizonWithSunSkyboxInspector"
}
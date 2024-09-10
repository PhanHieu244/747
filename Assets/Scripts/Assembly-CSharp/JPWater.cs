using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class JPWater : MonoBehaviour
{
	private Camera cam;

	public Material WaterMaterial;

	[SerializeField]
	private WaterQuality WaterQuality = WaterQuality.High;

	[SerializeField]
	private bool EdgeBlend = true;

	[SerializeField]
	private bool GerstnerDisplace = true;

	[SerializeField]
	private bool DisablePixelLights = true;

	[SerializeField]
	private int ReflectionSize = 256;

	[SerializeField]
	private float ClipPlaneOffset = 0.07f;

	[SerializeField]
	private LayerMask ReflectLayers = -1;

	public Light DirectionalLight;

	private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();

	private RenderTexture m_ReflectionTexture;

	private int m_OldReflectionTextureSize;

	private static bool s_InsideWater;

	[Header("UNDERWATER EFFECT")]
	[SerializeField]
	private bool UnderwaterEffect = true;

	public AudioSource Underwater;

	public Texture[] LightCookie;

	public Color32 defaultFogColor;

	public float defaultFogDensity;

	private Vector3 defaultLightDir = Vector3.zero;

	[SerializeField]
	private float UnderwaterDensity;

	private float screenWaterY;

	[Header("WATER PARTICLES FX")]
	[SerializeField]
	private bool ParticlesEffect = true;

	public ParticleSystem ripples;

	public ParticleSystem splash;

	public AudioClip Largesplash;

	private float count;

	private FlareLayer sunflare;

	private void Start()
	{
		cam = Camera.main;
		defaultLightDir = DirectionalLight.transform.forward;
		sunflare = cam.GetComponent<FlareLayer>();
	}

	private void Update()
	{
		if (!GetComponent<MeshRenderer>().isVisible | !WaterMaterial | !cam | s_InsideWater)
		{
			return;
		}
		if (WaterQuality > WaterQuality.Medium)
		{
			WaterMaterial.shader.maximumLOD = 501;
		}
		else if (WaterQuality > WaterQuality.Low)
		{
			WaterMaterial.shader.maximumLOD = 301;
		}
		else
		{
			WaterMaterial.shader.maximumLOD = 201;
		}
		if ((bool)DirectionalLight)
		{
			WaterMaterial.SetVector("_DirectionalLightDir", DirectionalLight.transform.forward);
		}
		if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) | !EdgeBlend)
		{
			Shader.EnableKeyword("WATER_EDGEBLEND_OFF");
			Shader.DisableKeyword("WATER_EDGEBLEND_ON");
		}
		else
		{
			Shader.EnableKeyword("WATER_EDGEBLEND_ON");
			Shader.DisableKeyword("WATER_EDGEBLEND_OFF");
			cam.depthTextureMode |= DepthTextureMode.Depth;
		}
		if (GerstnerDisplace)
		{
			Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
			Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
		}
		else
		{
			Shader.EnableKeyword("WATER_VERTEX_DISPLACEMENT_OFF");
			Shader.DisableKeyword("WATER_VERTEX_DISPLACEMENT_ON");
		}
		s_InsideWater = true;
		Camera reflectionCamera;
		CreateWaterObjects(cam, out reflectionCamera);
		Vector3 position = base.transform.position;
		Vector3 up = base.transform.up;
		int pixelLightCount = QualitySettings.pixelLightCount;
		if (DisablePixelLights)
		{
			QualitySettings.pixelLightCount = 0;
		}
		if (UpdateCameraModes(cam, reflectionCamera))
		{
			float w = 0f - Vector3.Dot(up, position) - ClipPlaneOffset;
			Vector4 plane = new Vector4(up.x, up.y, up.z, w);
			Matrix4x4 reflectionMat = Matrix4x4.zero;
			CalculateReflectionMatrix(ref reflectionMat, plane);
			Vector3 position2 = reflectionMat.MultiplyPoint(cam.transform.position);
			reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflectionMat;
			Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, up, 1f);
			reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
			reflectionCamera.cullingMask = -17 & ReflectLayers.value;
			reflectionCamera.targetTexture = m_ReflectionTexture;
			GL.invertCulling = true;
			Vector3 eulerAngles = cam.transform.eulerAngles;
			reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
			reflectionCamera.transform.position = position2;
			reflectionCamera.Render();
			GL.invertCulling = false;
			GetComponent<Renderer>().sharedMaterial.SetTexture("_ReflectionTex", m_ReflectionTexture);
			if (DisablePixelLights)
			{
				QualitySettings.pixelLightCount = pixelLightCount;
			}
			s_InsideWater = false;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_ReflectionTexture)
		{
			Object.DestroyImmediate(m_ReflectionTexture);
			m_ReflectionTexture = null;
		}
		foreach (KeyValuePair<Camera, Camera> reflectionCamera in m_ReflectionCameras)
		{
			Object.DestroyImmediate(reflectionCamera.Value.gameObject);
		}
		m_ReflectionCameras.Clear();
	}

	private bool UpdateCameraModes(Camera src, Camera dest)
	{
		if (dest == null)
		{
			return false;
		}
		dest.clearFlags = src.clearFlags;
		dest.backgroundColor = src.backgroundColor;
		dest.farClipPlane = src.farClipPlane;
		dest.nearClipPlane = src.nearClipPlane;
		dest.orthographic = src.orthographic;
		dest.fieldOfView = src.fieldOfView;
		dest.aspect = src.aspect;
		dest.orthographicSize = src.orthographicSize;
		return true;
	}

	private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera)
	{
		reflectionCamera = null;
		if (!m_ReflectionTexture | (m_OldReflectionTextureSize != ReflectionSize))
		{
			if ((bool)m_ReflectionTexture)
			{
				Object.DestroyImmediate(m_ReflectionTexture);
			}
			m_ReflectionTexture = new RenderTexture(ReflectionSize, ReflectionSize, 16)
			{
				name = "__WaterReflection" + GetInstanceID(),
				isPowerOfTwo = true,
				hideFlags = HideFlags.DontSave
			};
			m_OldReflectionTextureSize = ReflectionSize;
		}
		m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
		if (!reflectionCamera)
		{
			GameObject gameObject = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
			reflectionCamera = gameObject.GetComponent<Camera>();
			reflectionCamera.enabled = false;
			reflectionCamera.transform.position = base.transform.position;
			reflectionCamera.transform.rotation = base.transform.rotation;
			reflectionCamera.gameObject.AddComponent<FlareLayer>();
			gameObject.hideFlags = HideFlags.HideAndDontSave;
			m_ReflectionCameras[currentCamera] = reflectionCamera;
		}
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * ClipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}

	private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
	}

	private void OnGUI()
	{
		if (!Application.isPlaying | !UnderwaterEffect | !cam)
		{
			return;
		}
		float y = cam.ScreenToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane)).y;
		float y2 = cam.ScreenToWorldPoint(new Vector3(0f, Screen.height, cam.nearClipPlane)).y;
		float y3 = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0f, cam.nearClipPlane)).y;
		float y4 = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cam.nearClipPlane)).y;
		screenWaterY = Mathf.Clamp((Mathf.Min(y, y3) - base.transform.position.y) / (Mathf.Min(y, y3) - Mathf.Min(y2, y4)), -16f, 16f);
		Color32 b = Color32.Lerp(WaterMaterial.GetColor("_ReflectionColor"), WaterMaterial.GetColor("_BaseColor"), screenWaterY / 16f);
		RenderSettings.fogColor = Color32.Lerp(defaultFogColor, b, Mathf.Clamp01(screenWaterY));
		RenderSettings.fogDensity = Mathf.Lerp(defaultFogDensity, UnderwaterDensity, Mathf.Clamp01(screenWaterY));
		cam.backgroundColor = RenderSettings.fogColor;
		Color color = WaterMaterial.GetColor("_ReflectionColor");
		WaterMaterial.SetColor("_ReflectionColor", new Color(color.r, color.g, color.b, Mathf.Clamp(screenWaterY / 16f, 0.5f, 1f)));
		if (screenWaterY > 0.5f)
		{
			if (!Underwater.isPlaying)
			{
				Underwater.Play();
				sunflare.enabled = false;
				cam.clearFlags = CameraClearFlags.Color;
				DirectionalLight.transform.forward = -Vector3.up;
			}
			if (LightCookie.Length != 0)
			{
				DirectionalLight.cookie = LightCookie[Mathf.FloorToInt(Time.fixedTime * 16f % (float)LightCookie.Length)];
			}
		}
		else if (Underwater.isPlaying)
		{
			Underwater.Stop();
			sunflare.enabled = true;
			cam.clearFlags = CameraClearFlags.Skybox;
			DirectionalLight.transform.forward = defaultLightDir;
			DirectionalLight.cookie = null;
		}
	}

	private void OnTriggerStay(Collider col)
	{
		if (ParticlesEffect)
		{
			WaterParticleFX(col, ripples);
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (ParticlesEffect)
		{
			WaterParticleFX(col, splash);
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (ParticlesEffect)
		{
			WaterParticleFX(col, splash);
		}
	}

	private void WaterParticleFX(Collider col, ParticleSystem particleFx)
	{
		count += Time.fixedDeltaTime;
		Creature creature = null;
		if (!col.transform.root.GetComponent<Rigidbody>() || !(col.transform.root.tag == "Creature"))
		{
			return;
		}
		creature = col.transform.root.GetComponent<Creature>();
		creature.waterY = base.transform.position.y;
		if (!creature.IsVisible || (particleFx == ripples && count < (float)(creature.loop % 10)))
		{
			return;
		}
		SkinnedMeshRenderer skinnedMeshRenderer = creature.rend[0];
		if (skinnedMeshRenderer.bounds.Contains(new Vector3(col.transform.position.x, base.transform.position.y, col.transform.position.z)) && (!creature.anm.GetInteger("Move").Equals(0) | (creature.CanFly && creature.IsOnLevitation) | creature.OnJump | creature.OnAttack))
		{
			if (particleFx == splash && (!creature.IsOnGround | creature.OnJump))
			{
				col.transform.root.GetComponents<AudioSource>()[1].pitch = Random.Range(0.5f, 0.75f);
				col.transform.root.GetComponents<AudioSource>()[1].PlayOneShot(Largesplash, Random.Range(0.5f, 0.75f));
			}
			else
			{
				particleFx = ripples;
			}
			Vector2 vector = new Vector2(skinnedMeshRenderer.bounds.center.x, skinnedMeshRenderer.bounds.center.z);
			ParticleSystem particleSystem = Object.Instantiate(particleFx, new Vector3(vector.x, base.transform.position.y + 0.01f, vector.y), Quaternion.Euler(-90f, 0f, 0f));
			float num = skinnedMeshRenderer.bounds.size.magnitude / 10f;
			particleSystem.transform.localScale = new Vector3(num, num, num);
			Object.Destroy(particleSystem.gameObject, 3f);
			count = 0f;
		}
	}
}

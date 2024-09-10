using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
	private const string ManagerHelp = "Disable creatures management.\nCreatures A.I. still work, player inputs, camera behavior and GUI features are disabled.\nUseful if you want to use a third party asset e.g. fps controller. However, manager component still to be attached to the MainCam. ";

	[Header("JURASSIC PACK MANAGER")]
	[Tooltip("Disable creatures management.\nCreatures A.I. still work, player inputs, camera behavior and GUI features are disabled.\nUseful if you want to use a third party asset e.g. fps controller. However, manager component still to be attached to the MainCam. ")]
	public bool UseManager = true;

	[SerializeField]
	private bool ShowGUI = true;

	[SerializeField]
	private bool ShowFPS = true;

	public Texture2D helpscreen;

	public Texture2D icons;

	[SerializeField]
	private bool InvertYAxis;

	[SerializeField]
	[Range(0.1f, 10f)]
	private float sensivity = 2.5f;

	public AudioClip Wind;

	[Space(10f)]
	[Header("GLOBAL CREATURES SETTINGS")]
	[Tooltip("Add your creatures prefabs here, this will make it spawnable during game.")]
	public List<GameObject> CollectionList;

	private const string IKHelp = "Inverse Kinematics - Accurate feet placement on ground";

	[Tooltip("Inverse Kinematics - Accurate feet placement on ground")]
	public bool UseIK;

	[Tooltip("Creatures will be active even if they are no longer visible. (performance may be affected).")]
	public bool RealtimeGame;

	[Tooltip("Countdown to destroy the creature after his dead. Put 0 to cancels the countdown, the body will remain on the scene without disappearing.")]
	public int TimeAfterDead = 10000;

	private const string RaycastHelp = "ENABLED : allow creatures to walk on all kind of collider. (more expensive).\n\nDISABLED : creatures can only walk on Terrain collider (faster).\n";

	[Tooltip("ENABLED : allow creatures to walk on all kind of collider. (more expensive).\n\nDISABLED : creatures can only walk on Terrain collider (faster).\n")]
	public bool UseRaycast;

	[Tooltip("Layer used for water.")]
	public int waterLayer;

	[Tooltip("Unity terrain tree layer, the layer must be defined into tree model prefab")]
	public int treeLayer;

	[Tooltip("The maximium walkable slope before the creature start slipping.")]
	[Range(0.1f, 1f)]
	public float MaxSlope = 0.75f;

	[Tooltip("Water plane altitude")]
	public float WaterAlt = 55f;

	[Tooltip("Blood particle for creatures")]
	public ParticleSystem blood;

	[HideInInspector]
	public List<GameObject> creaturesList;

	[HideInInspector]
	public List<GameObject> playersList;

	[HideInInspector]
	public int selected;

	[HideInInspector]
	public int CameraMode = 1;

	[HideInInspector]
	public int message;

	[HideInInspector]
	public Terrain T;

	[HideInInspector]
	public TerrainData tdata;

	[HideInInspector]
	public Vector3 tpos = Vector3.zero;

	[HideInInspector]
	public float tres;

	[HideInInspector]
	public int toolBarTab = -1;

	[HideInInspector]
	public int addCreatureTab = -2;

	[HideInInspector]
	public int count;

	private bool browser;

	private Vector2 scroll1 = Vector2.zero;

	private Vector2 scroll2 = Vector2.zero;

	private float vx;

	private float vy;

	private float vz = 25f;

	private float timer;

	private float frame;

	private float fps;

	private Rigidbody body;

	private AudioSource source;

	private bool spawnAI;

	private bool rndSkin;

	private bool rndSize;

	private bool rndSetting;

	private int rndSizeSpan = 1;

	private void Awake()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Creature");
		GameObject[] array2 = GameObject.FindGameObjectsWithTag("Player");
		GameObject[] array3 = array;
		foreach (GameObject gameObject in array3)
		{
			if (!gameObject.name.EndsWith("(Clone)"))
			{
				creaturesList.Add(gameObject.gameObject);
			}
			else
			{
				Object.Destroy(gameObject.gameObject);
			}
		}
		array3 = array2;
		foreach (GameObject gameObject2 in array3)
		{
			playersList.Add(gameObject2.gameObject);
		}
		if (UseManager)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			body = base.transform.root.GetComponent<Rigidbody>();
			source = base.transform.root.GetComponent<AudioSource>();
		}
		if ((bool)Terrain.activeTerrain)
		{
			T = Terrain.activeTerrain;
			tdata = T.terrainData;
			tpos = T.GetPosition();
			tres = tdata.heightmapResolution;
		}
		treeLayer = 1 << treeLayer;
	}

	private void Update()
	{
		if (!UseManager)
		{
			return;
		}
		if (ShowFPS)
		{
			frame += 1f;
			timer += Time.deltaTime;
			if (timer > 1f)
			{
				fps = frame;
				timer = 0f;
				frame = 0f;
			}
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.Escape) && toolBarTab == -1)
			{
				Cursor.lockState = CursorLockMode.None;
				toolBarTab = 1;
			}
			else if (Input.GetKeyDown(KeyCode.Escape) && toolBarTab != -1)
			{
				Cursor.lockState = CursorLockMode.None;
				toolBarTab = -1;
			}
			else if (toolBarTab == -1)
			{
				Cursor.lockState = CursorLockMode.Locked;
			}
		}
		else if (Cursor.lockState == CursorLockMode.None && Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.Locked;
		}
		else if (Input.GetKeyDown(KeyCode.Escape))
		{
			Cursor.lockState = CursorLockMode.None;
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			if (selected > 0)
			{
				selected--;
			}
			else
			{
				selected = creaturesList.Count - 1;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Y))
		{
			if (selected < creaturesList.Count - 1)
			{
				selected++;
			}
			else
			{
				selected = 0;
			}
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (CameraMode == 2)
			{
				CameraMode = 0;
			}
			else
			{
				CameraMode++;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!UseManager)
		{
			return;
		}
		Creature creature = null;
		if (creaturesList.Count == 0)
		{
			CameraMode = 0;
		}
		else if (!creaturesList[selected] | !creaturesList[selected].activeInHierarchy)
		{
			CameraMode = 0;
		}
		else
		{
			creature = creaturesList[selected].GetComponent<Creature>();
		}
		if ((bool)T && T.SampleHeight(base.transform.root.position) + T.GetPosition().y > base.transform.root.position.y - 1f)
		{
			body.velocity = new Vector3(body.velocity.x, 0f, body.velocity.z);
			base.transform.root.position = new Vector3(base.transform.root.position.x, T.SampleHeight(base.transform.root.position) + T.GetPosition().y + 1f, base.transform.root.position.z);
		}
		switch (CameraMode)
		{
		case 0:
		{
			if (source.clip == null)
			{
				source.clip = Wind;
			}
			else if (source.clip == Wind)
			{
				if (source.isPlaying)
				{
					source.volume = body.velocity.magnitude / 128f;
					source.pitch = source.volume;
				}
				else
				{
					source.PlayOneShot(Wind);
				}
			}
			Vector3 zero = Vector3.zero;
			float num = 0f;
			if (Input.GetKey(KeyCode.LeftShift))
			{
				body.mass = 0.025f;
			}
			else
			{
				body.mass = 0.1f;
			}
			body.drag = 1f;
			if ((Cursor.lockState == CursorLockMode.Locked) | Input.GetKey(KeyCode.Mouse2))
			{
				vx += Input.GetAxis("Mouse X") * sensivity;
				vy = Mathf.Clamp(InvertYAxis ? (vy + Input.GetAxis("Mouse Y") * sensivity) : (vy - Input.GetAxis("Mouse Y") * sensivity), -89.9f, 89.9f);
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.AngleAxis(vx, Vector3.up) * Quaternion.AngleAxis(vy, Vector3.right), 0.1f);
			}
			num = (Input.GetKey(KeyCode.Space) ? 1f : ((!Input.GetKey(KeyCode.LeftControl)) ? 0f : (-1f)));
			zero = base.transform.rotation * new Vector3(Input.GetAxis("Horizontal"), num, Input.GetAxis("Vertical"));
			body.AddForce(zero * (base.transform.root.position - (base.transform.root.position + zero)).magnitude);
			break;
		}
		case 1:
		{
			body.mass = 1f;
			body.drag = 10f;
			float withersSize = creature.withersSize;
			if ((Cursor.lockState == CursorLockMode.Locked) | Input.GetKey(KeyCode.Mouse2))
			{
				if (Input.GetKey(KeyCode.Mouse1))
				{
					vx = creaturesList[selected].transform.eulerAngles.y;
					if (creature.IsOnLevitation)
					{
						vy = Mathf.Clamp(Mathf.Lerp(vy, creature.anm.GetFloat("Pitch") * 90f, 0.01f), -45f, 90f);
					}
					else
					{
						vy = Mathf.Clamp(InvertYAxis ? (vy - Input.GetAxis("Mouse Y") * sensivity) : (vy + Input.GetAxis("Mouse Y") * sensivity), -90f, 90f);
					}
				}
				else if (!Input.GetKey(KeyCode.Mouse2) | (Cursor.lockState != CursorLockMode.Locked))
				{
					vx += Input.GetAxis("Mouse X") * sensivity;
					vy = Mathf.Clamp(InvertYAxis ? (vy - Input.GetAxis("Mouse Y") * sensivity) : (vy + Input.GetAxis("Mouse Y") * sensivity), -90f, 90f);
				}
			}
			vz = Mathf.Clamp(vz - Input.GetAxis("Mouse ScrollWheel") * 10f, withersSize, withersSize * 32f);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(vy, vx, 0f), 0.1f);
			Vector3 vector = creaturesList[selected].transform.root.position + Vector3.up * withersSize * 1.5f - base.transform.root.position - base.transform.forward * vz;
			body.AddForce(vector * 128f);
			break;
		}
		case 2:
		{
			float withersSize = creature.withersSize;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.LookRotation(creaturesList[selected].transform.root.position + Vector3.up * withersSize * 1.5f - base.transform.root.position), 0.1f);
			break;
		}
		default:
			CameraMode = 0;
			break;
		}
	}

	private void OnGUI()
	{
		if (!UseManager)
		{
			return;
		}
		float num = Screen.width;
		float num2 = Screen.height;
		Creature creature = null;
		if (creaturesList.Count > 0 && (bool)creaturesList[selected] && creaturesList[selected].activeInHierarchy)
		{
			creature = creaturesList[selected].GetComponent<Creature>();
		}
		GUIStyle gUIStyle = new GUIStyle("box");
		gUIStyle.fontSize = 16;
		if (Cursor.lockState == CursorLockMode.None)
		{
			if ((bool)creature && CameraMode != 0)
			{
				GUI.Box(new Rect(0f, 0f, num, 50f), creaturesList[selected].name);
				GUI.color = Color.yellow;
				if (GUI.Button(new Rect(0f, 5f, num / 16f - 4f, 20f), "Free"))
				{
					CameraMode = 0;
				}
				if (CameraMode == 1)
				{
					GUI.color = Color.green;
				}
				if (GUI.Button(new Rect(num / 16f * 1.5f, 5f, num / 16f - 4f, 20f), "Follow"))
				{
					CameraMode = 1;
				}
				GUI.color = Color.yellow;
				if (CameraMode == 2)
				{
					GUI.color = Color.green;
				}
				if (GUI.Button(new Rect(num / 16f * 3f, 5f, num / 16f - 4f, 20f), "POV"))
				{
					CameraMode = 2;
				}
			}
			else
			{
				GUI.Box(new Rect(0f, 0f, num, 50f), "", gUIStyle);
				if ((bool)creature)
				{
					GUI.color = Color.green;
					GUI.Button(new Rect(0f, 5f, num / 16f - 4f, 20f), "Free");
					GUI.color = Color.yellow;
					if (GUI.Button(new Rect(num / 16f * 1.5f, 5f, num / 16f - 4f, 20f), "Follow"))
					{
						CameraMode = 1;
					}
					if (GUI.Button(new Rect(num / 16f * 3f, 5f, num / 16f - 4f, 20f), "POV"))
					{
						CameraMode = 2;
					}
				}
			}
			GUI.color = Color.white;
			Cursor.visible = true;
			if (!ShowGUI)
			{
				GUI.Box(new Rect(0f, 0f, num, 50f), "");
			}
			string[] texts = new string[4] { "File", "Creatures", "Options", "Help" };
			GUI.color = Color.yellow;
			toolBarTab = GUI.Toolbar(new Rect(0f, 30f, num, 20f), toolBarTab, texts);
			GUI.color = Color.white;
			switch (toolBarTab)
			{
			case 0:
				GUI.Box(new Rect(0f, 50f, num, num2 - 50f), "", gUIStyle);
				if (GUI.Button(new Rect(num / 2f - 60f, num2 / 2f - 35f, 120f, 30f), "Reset"))
				{
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
				if (GUI.Button(new Rect(num / 2f - 60f, num2 / 2f + 5f, 120f, 30f), "Quit"))
				{
					Application.Quit();
				}
				break;
			case 1:
				if ((bool)creature)
				{
					GUI.Box(new Rect(0f, 50f, num * 0.25f, num2 * 0.75f - 50f), "");
					string text = creaturesList[selected].name;
					creaturesList[selected].name = GUI.TextField(new Rect(25f, 50f, num * 0.25f - 25f, 25f), text, gUIStyle);
					if (GUI.Button(new Rect(0f, 50f, 25f, 25f), "X"))
					{
						Object.Destroy(creaturesList[selected].gameObject);
						creaturesList.RemoveAt(selected);
						if (selected > 0)
						{
							selected--;
						}
						else
						{
							if (creaturesList.Count <= 0)
							{
								return;
							}
							selected = creaturesList.Count - 1;
						}
					}
					if (browser)
					{
						if (GUI.Button(new Rect(0f, 75f, num * 0.25f, 25f), "Close Browser"))
						{
							browser = false;
						}
						GUI.Box(new Rect(0f, 100f, num * 0.25f, num2 * 0.75f - 100f), "Creatures : " + creaturesList.Count);
						scroll1 = GUI.BeginScrollView(new Rect(0f, 130f, num * 0.25f, num2 * 0.75f - 140f), scroll1, new Rect(0f, 0f, 270f, creaturesList.Count * 40), false, true);
						int value = creaturesList.Count;
						int num3 = Mathf.RoundToInt(scroll1.y / 40f);
						value = Mathf.Clamp(value, num3, num3 + Mathf.RoundToInt(num2 * 0.75f / 40f));
						for (int i = num3; i < value; i++)
						{
							float num4 = 40 * i;
							if (selected != i)
							{
								GUI.color = Color.gray;
							}
							else
							{
								GUI.color = Color.white;
							}
							GUI.Label(new Rect(5f, num4, num * 0.25f - 30f, 25f), i + 1 + ". ");
							if (GUI.Button(new Rect(30f, num4, 20f, 20f), "X"))
							{
								if (i <= selected && ((selected > 0) | (selected == creaturesList.Count - 1)))
								{
									selected--;
								}
								Object.Destroy(creaturesList[i].gameObject);
								creaturesList.RemoveAt(i);
								return;
							}
							if (GUI.Button(new Rect(50f, num4, 140f, 20f), creaturesList[i].name))
							{
								selected = i;
								CameraMode = 1;
							}
							if (GUI.Button(new Rect(190f, num4, 40f, 20f), "Edit"))
							{
								selected = i;
								browser = false;
							}
							Creature component = creaturesList[i].gameObject.GetComponent<Creature>();
							GUI.Label(new Rect(235f, num4, num * 0.25f - 30f, 25f), component.behavior + "  " + component.behaviorCount);
							Rect texCoords = new Rect(0f, 0f, 0.1f, 0.1f);
							GUI.color = Color.black;
							GUI.DrawTextureWithTexCoords(new Rect(30f, num4 + 22f, 50f, 4f), icons, texCoords, false);
							GUI.DrawTextureWithTexCoords(new Rect(85f, num4 + 22f, 50f, 4f), icons, texCoords, false);
							GUI.color = Color.green;
							GUI.DrawTextureWithTexCoords(new Rect(30f, num4 + 22f, component.Health / 2f, 4f), icons, texCoords, false);
							GUI.color = Color.yellow;
							GUI.DrawTextureWithTexCoords(new Rect(85f, num4 + 22f, component.Food / 2f, 4f), icons, texCoords, false);
							if (!component.CanSwim)
							{
								GUI.color = Color.black;
								GUI.DrawTextureWithTexCoords(new Rect(140f, num4 + 22f, 50f, 4f), icons, texCoords, false);
								GUI.DrawTextureWithTexCoords(new Rect(195f, num4 + 22f, 50f, 4f), icons, texCoords, false);
								GUI.color = Color.cyan;
								GUI.DrawTextureWithTexCoords(new Rect(140f, num4 + 22f, component.Water / 2f, 4f), icons, texCoords, false);
								GUI.color = Color.gray;
								GUI.DrawTextureWithTexCoords(new Rect(195f, num4 + 22f, component.Stamina / 2f, 4f), icons, texCoords, false);
							}
						}
						GUI.EndScrollView();
					}
					else
					{
						if (GUI.Button(new Rect(num * 0.25f / 4f, 75f, num * 0.25f / 2f, 20f), "Browse : " + (selected + 1) + "/" + creaturesList.Count))
						{
							browser = true;
						}
						if (GUI.Button(new Rect(0f, 75f, num * 0.25f / 4f, 20f), "<<"))
						{
							if (selected > 0)
							{
								selected--;
							}
							else
							{
								selected = creaturesList.Count - 1;
							}
						}
						if (GUI.Button(new Rect(num * 0.25f / 4f * 3f, 75f, num * 0.25f / 4f, 20f), ">>"))
						{
							if (selected < creaturesList.Count - 1)
							{
								selected++;
							}
							else
							{
								selected = 0;
							}
						}
						scroll1 = GUI.BeginScrollView(new Rect(0f, 110f, num * 0.25f, num2 * 0.75f - 110f), scroll1, new Rect(0f, 0f, 0f, 430f), false, true);
						if (creature.UseAI)
						{
							GUI.color = Color.gray;
							if (GUI.Button(new Rect(num * 0.25f / 2f, 0f, num * 0.25f / 2f - 20f, 25f), "Player"))
							{
								creature.SetAI(false);
							}
							GUI.color = Color.green;
							GUI.Box(new Rect(10f, 0f, num * 0.25f / 2f - 10f, 25f), "A.I. : " + creature.behavior);
						}
						else
						{
							GUI.color = Color.green;
							GUI.Box(new Rect(num * 0.25f / 2f, 0f, num * 0.25f / 2f - 20f, 25f), "Player");
							GUI.color = Color.gray;
							if (GUI.Button(new Rect(10f, 0f, num * 0.25f / 2f - 10f, 25f), "A.I."))
							{
								creature.SetAI(true);
							}
						}
						GUI.color = Color.white;
						int num5 = creature.bodyTexture.GetHashCode();
						int hashCode = creature.eyesTexture.GetHashCode();
						if (GUI.Button(new Rect(10f, 30f, num * 0.25f - 30f, 25f), "Body Skin : " + creature.bodyTexture))
						{
							num5 = ((num5 < 2) ? (num5 + 1) : 0);
							creature.SetMaterials(num5, hashCode);
						}
						if (GUI.Button(new Rect(10f, 60f, num * 0.25f - 30f, 25f), "Eyes Skin : " + creature.eyesTexture))
						{
							hashCode = ((hashCode < 15) ? (hashCode + 1) : 0);
							creature.SetMaterials(num5, hashCode);
						}
						float x = creaturesList[selected].transform.localScale.x;
						GUI.Box(new Rect(10f, 90f, num * 0.25f - 30f, 25f), "Scale : " + Mathf.Round(x * 100f) / 100f);
						x = GUI.HorizontalSlider(new Rect(10f, 110f, num * 0.25f - 30f, 25f), creaturesList[selected].transform.localScale.x, 0.1f, 1f);
						if (x != creaturesList[selected].transform.localScale.x)
						{
							creaturesList[selected].SendMessage("SetScale", Mathf.Round(x * 100f) / 100f);
						}
						GUI.Box(new Rect(10f, 125f, num * 0.25f - 30f, 25f), "Animation Speed : " + Mathf.Round(creature.AnimSpeed * 100f) / 100f);
						creature.AnimSpeed = GUI.HorizontalSlider(new Rect(10f, 145f, num * 0.25f - 30f, 25f), creature.AnimSpeed, 0f, 2f);
						GUI.Box(new Rect(10f, 160f, num * 0.25f - 30f, 25f), "Health : " + Mathf.Round(creature.Health * 10f) / 10f);
						creature.Health = GUI.HorizontalSlider(new Rect(10f, 180f, num * 0.25f - 30f, 25f), creature.Health, 0f, 100f);
						GUI.Box(new Rect(10f, 200f, num * 0.25f - 30f, 20f), "Food : " + Mathf.Round(creature.Food * 10f) / 10f);
						creature.Food = GUI.HorizontalSlider(new Rect(10f, 220f, num * 0.25f - 30f, 20f), creature.Food, 0f, 100f);
						GUI.Box(new Rect(10f, 240f, num * 0.25f - 30f, 20f), "Water : " + Mathf.Round(creature.Water * 10f) / 10f);
						creature.Water = GUI.HorizontalSlider(new Rect(10f, 260f, num * 0.25f - 30f, 20f), creature.Water, 0f, 100f);
						GUI.Box(new Rect(10f, 280f, num * 0.25f - 30f, 20f), "Stamina : " + Mathf.Round(creature.Stamina * 10f) / 10f);
						creature.Stamina = GUI.HorizontalSlider(new Rect(10f, 300f, num * 0.25f - 30f, 20f), creature.Stamina, 0f, 100f);
						GUI.Box(new Rect(10f, 320f, num * 0.25f - 30f, 20f), "Damages X" + Mathf.Round(creature.DamageMultiplier * 100f) / 100f);
						creature.DamageMultiplier = GUI.HorizontalSlider(new Rect(10f, 340f, num * 0.25f - 30f, 20f), creature.DamageMultiplier, 1f, 10f);
						GUI.Box(new Rect(10f, 360f, num * 0.25f - 30f, 20f), "Armor X" + Mathf.Round(creature.ArmorMultiplier * 100f) / 100f);
						creature.ArmorMultiplier = GUI.HorizontalSlider(new Rect(10f, 380f, num * 0.25f - 30f, 20f), creature.ArmorMultiplier, 1f, 10f);
						GUI.EndScrollView();
					}
				}
				else
				{
					GUI.Box(new Rect(0f, 50f, num * 0.25f, num2 * 0.75f - 50f), "None", gUIStyle);
				}
				GUI.color = Color.yellow;
				if (addCreatureTab == -2)
				{
					if (GUI.Button(new Rect(0f, num2 * 0.75f, num * 0.25f, 25f), ""))
					{
						addCreatureTab = -1;
					}
					GUI.Box(new Rect(0f, num2 * 0.75f, num / 4f, num2 / 4f), "Add a new creature", gUIStyle);
				}
				else
				{
					if (addCreatureTab != -1)
					{
						break;
					}
					if (GUI.Button(new Rect(num - 25f, 50f, 25f, 25f), "X"))
					{
						addCreatureTab = -2;
					}
					GUI.Box(new Rect(0f, num2 * 0.75f, num / 4f, num2 / 4f), "Spawn Settings", gUIStyle);
					GUI.color = Color.white;
					scroll2 = GUI.BeginScrollView(new Rect(0f, num2 * 0.75f + 40f, num * 0.25f, num2 * 0.25f - 40f), scroll2, new Rect(0f, 0f, 0f, 130f), false, true);
					GUI.Box(new Rect(10f, 0f, num * 0.25f - 30f, 25f), "");
					spawnAI = GUI.Toggle(new Rect(18f, 0f, 120f, 25f), spawnAI, " Spawn with AI ");
					GUI.Box(new Rect(10f, 30f, num * 0.25f - 30f, 25f), "");
					rndSkin = GUI.Toggle(new Rect(18f, 30f, 100f, 25f), rndSkin, " Random skin");
					GUI.Box(new Rect(10f, 60f, num * 0.25f - 30f, 25f), "");
					rndSize = GUI.Toggle(new Rect(18f, 60f, 100f, 25f), rndSize, " Random size");
					if (rndSize && GUI.Button(new Rect(130f, 60f, num * 0.25f - 150f, 25f), "Span : " + rndSizeSpan))
					{
						if (rndSizeSpan < 5)
						{
							rndSizeSpan++;
						}
						else
						{
							rndSizeSpan = 1;
						}
					}
					GUI.Box(new Rect(10f, 90f, num * 0.25f - 30f, 25f), "");
					rndSetting = GUI.Toggle(new Rect(18f, 90f, num * 0.25f - 30f, 25f), rndSetting, " Random status settings");
					GUI.EndScrollView();
					GUI.Box(new Rect(num / 4f, 50f, num * 0.75f, num2 - 50f), "Select a specie. " + CollectionList.Count + " creature(s) available.", gUIStyle);
					for (int j = 0; j < CollectionList.Count; j++)
					{
						int num6 = (int)num2 / 36;
						int num7 = j / num6;
						int num8 = j - num6 * num7;
						if (CollectionList[j].GetComponent<Creature>().Herbivorous)
						{
							GUI.color = Color.green;
						}
						else if (CollectionList[j].GetComponent<Creature>().CanFly)
						{
							GUI.color = Color.yellow;
						}
						else if (CollectionList[j].GetComponent<Creature>().CanSwim)
						{
							GUI.color = Color.cyan;
						}
						else
						{
							GUI.color = new Color(1f, 0.6f, 0f);
						}
						if (GUI.Button(new Rect(num / 4f + 15f + (float)(200 * num7), 100 + 30 * num8, 180f, 25f), CollectionList[j].name))
						{
							GameObject gameObject = Object.Instantiate(CollectionList[j], base.transform.position + base.transform.forward * 10f, Quaternion.identity);
							Creature component2 = gameObject.GetComponent<Creature>();
							if (!spawnAI)
							{
								CameraMode = 1;
							}
							component2.UseAI = spawnAI;
							if (rndSkin)
							{
								component2.SetMaterials(Random.Range(0, 3), Random.Range(0, 16));
							}
							if (rndSize)
							{
								component2.SetScale(0.5f + Random.Range((float)rndSizeSpan / -10f, (float)rndSizeSpan / 10f));
							}
							else
							{
								component2.SetScale(0.5f);
							}
							if (rndSetting)
							{
								component2.Health = 100f;
								component2.Stamina = Random.Range(0, 100);
								component2.Food = Random.Range(0, 100);
								component2.Water = Random.Range(0, 100);
							}
							gameObject.name = CollectionList[j].name;
							creaturesList.Add(gameObject.gameObject);
							selected = creaturesList.IndexOf(gameObject.gameObject);
						}
					}
					GUI.color = Color.white;
				}
				break;
			case 2:
			{
				GUI.Box(new Rect(0f, 50f, num, num2 - 50f), "Options", gUIStyle);
				GUI.Box(new Rect(num / 2f - 225f, num2 / 2f - 110f, 150f, 220f), "Screen", gUIStyle);
				bool fullScreen = Screen.fullScreen;
				fullScreen = GUI.Toggle(new Rect(num / 2f - 220f, num2 / 2f - 80f, 140f, 20f), fullScreen, " Fullscreen");
				if (fullScreen != Screen.fullScreen)
				{
					Screen.fullScreen = !Screen.fullScreen;
				}
				ShowFPS = GUI.Toggle(new Rect(num / 2f - 220f, num2 / 2f - 40f, 140f, 20f), ShowFPS, " Show Fps");
				ShowGUI = GUI.Toggle(new Rect(num / 2f - 220f, num2 / 2f, 140f, 20f), ShowGUI, " Show GUI");
				GUI.Box(new Rect(num / 2f - 75f, num2 / 2f - 110f, 150f, 220f), "Controls", gUIStyle);
				InvertYAxis = GUI.Toggle(new Rect(num / 2f - 70f, num2 / 2f - 80f, 140f, 20f), InvertYAxis, " Invert Y Axe");
				GUI.Label(new Rect(num / 2f - 70f, num2 / 2f - 40f, 140f, 20f), "Sensivity");
				sensivity = GUI.HorizontalSlider(new Rect(num / 2f - 70f, num2 / 2f, 140f, 20f), sensivity, 0.1f, 10f);
				GUI.Box(new Rect(num / 2f + 75f, num2 / 2f - 110f, 150f, 220f), "Creatures", gUIStyle);
				UseIK = GUI.Toggle(new Rect(num / 2f + 80f, num2 / 2f - 80f, 140f, 20f), UseIK, " Use IK");
				UseRaycast = GUI.Toggle(new Rect(num / 2f + 80f, num2 / 2f - 40f, 140f, 20f), UseRaycast, " Use Raycast");
				RealtimeGame = GUI.Toggle(new Rect(num / 2f + 80f, num2 / 2f, 140f, 20f), RealtimeGame, " Realtime Game");
				break;
			}
			case 3:
				GUI.Box(new Rect(0f, 50f, num, num2 - 50f), "Controls", gUIStyle);
				GUI.DrawTexture(new Rect(0f, 50f, num, num2 - 50f), helpscreen);
				break;
			}
		}
		else
		{
			Cursor.visible = false;
		}
		if ((bool)creature && ShowGUI && CameraMode == 1)
		{
			Rect texCoords2 = new Rect(0f, 0.5f, 0.5f, 0.5f);
			Rect texCoords3 = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
			Rect texCoords4 = new Rect(0.5f, 0f, 0.5f, 0.5f);
			Rect texCoords5 = new Rect(0f, 0f, 0.5f, 0.5f);
			Rect texCoords6 = new Rect(0f, 0f, 0.1f, 0.1f);
			GUI.color = Color.white;
			GUI.DrawTextureWithTexCoords(new Rect(num / 4f, num2 / 1.1f, num / 48f, num / 48f), icons, texCoords2, true);
			GUI.DrawTextureWithTexCoords(new Rect(num / 2f, num2 / 1.1f, num / 48f, num / 48f), icons, texCoords3, true);
			GUI.DrawTextureWithTexCoords(new Rect(num / 2f, num2 / 1.05f, num / 48f, num / 48f), icons, texCoords4, true);
			GUI.DrawTextureWithTexCoords(new Rect(num / 4f, num2 / 1.05f, num / 48f, num / 48f), icons, texCoords5, true);
			GUI.color = Color.black;
			GUI.DrawTextureWithTexCoords(new Rect(num / 3.5f, num2 / 1.09f, num * 0.002f * 100f, num2 / 100f), icons, texCoords6, false);
			GUI.DrawTextureWithTexCoords(new Rect(num / 1.85f, num2 / 1.09f, num * 0.002f * 100f, num2 / 100f), icons, texCoords6, false);
			GUI.DrawTextureWithTexCoords(new Rect(num / 1.85f, num2 / 1.04f, num * 0.002f * 100f, num2 / 100f), icons, texCoords6, false);
			GUI.DrawTextureWithTexCoords(new Rect(num / 3.5f, num2 / 1.04f, num * 0.002f * 100f, num2 / 100f), icons, texCoords6, false);
			if (((creature.Food == 0f) | (creature.Stamina == 0f) | (creature.Water == 0f)) && creature.loop <= 25)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.green;
			}
			GUI.DrawTextureWithTexCoords(new Rect(num / 3.5f, num2 / 1.09f, num * 0.002f * creature.Health, num2 / 100f), icons, texCoords6, false);
			if (creature.Food < 25f && creature.loop <= 25)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.yellow;
			}
			GUI.DrawTextureWithTexCoords(new Rect(num / 1.85f, num2 / 1.09f, num * 0.002f * creature.Food, num2 / 100f), icons, texCoords6, false);
			if (creature.Water < 25f && creature.loop <= 25)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.cyan;
			}
			GUI.DrawTextureWithTexCoords(new Rect(num / 1.85f, num2 / 1.04f, num * 0.002f * creature.Water, num2 / 100f), icons, texCoords6, false);
			if (creature.Stamina < 25f && creature.loop <= 25)
			{
				GUI.color = Color.red;
			}
			else
			{
				GUI.color = Color.gray;
			}
			GUI.DrawTextureWithTexCoords(new Rect(num / 3.5f, num2 / 1.04f, num * 0.002f * creature.Stamina, num2 / 100f), icons, texCoords6, false);
		}
		GUI.color = Color.white;
		if (ShowFPS)
		{
			GUI.Label(new Rect(num - 60f, 1f, 55f, 20f), "Fps : " + fps);
		}
		if (message == 0)
		{
			return;
		}
		count++;
		if (message == 1)
		{
			GUI.Box(new Rect(num / 2f - 120f, num2 / 2f, 240f, 25f), "Nothing to eat or drink...", gUIStyle);
		}
		else if (message == 2)
		{
			GUI.color = Color.yellow;
			GUI.Box(new Rect(num / 2f - 140f, num2 / 2f, 280f, 25f), "AI/IK Script Extension Asset Required", gUIStyle);
			GUI.color = Color.white;
			if (GUI.Button(new Rect(num / 2f - 140f, num2 / 2f + 25f, 280f, 25f), "Get it : www.assetstore.unity3d.com"))
			{
				Application.OpenURL("https://assetstore.unity.com/packages/3d/characters/animals/jp-script-extension-94813");
			}
		}
		if (count > 512)
		{
			count = 0;
			message = 0;
		}
	}
}

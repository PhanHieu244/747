using UnityEngine;

public class Particaleffect : MonoBehaviour
{
	public static Particaleffect instance;

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void playpop()
	{
		GetComponent<ParticleSystem>().Play();
	}
}

using UnityEngine;

public class grid : MonoBehaviour
{
	public float time;

	public float resettime;

	public bool a;

	public GameObject have;

	private void Awake()
	{
		base.gameObject.layer = 6;
	}

	private void Start()
	{
		resettime = 0.05f;
		time = resettime;
	}

	private void Update()
	{
		if (a)
		{
			time -= Time.deltaTime;
			if (time < 0f)
			{
				gridpanel();
			}
		}
	}

	public void greenpanle()
	{
		GetComponent<MeshRenderer>().material = BotsManager.instance.greengrid;
		time = resettime;
		a = true;
	}

	public void gridpanel()
	{
		a = false;
		time = resettime;
		GetComponent<MeshRenderer>().material = BotsManager.instance.planegrid;
	}
}

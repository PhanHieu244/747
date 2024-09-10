using System.Collections.Generic;
using UnityEngine;

public class BotsManager : MonoBehaviour
{
	public List<GameObject> enemybots;

	public List<GameObject> playerbots;

	public static BotsManager instance;

	public GameObject tempgameobject;

	public Material planegrid;

	public Material greengrid;

	public GameObject addthisdino;

	public int addhowmany;

	public Color PlayerColor;

	public Color BotColor;

	public bool over;

	public void Awake()
	{
		if (!instance)
		{
			instance = this;
		}
	}

	public void Start()
	{
		if (addhowmany == 0)
		{
			UiManager.Instance.removedinobutton();
		}
	}

	public GameObject returnatarget(GameObject needatarget)
	{
		tempgameobject = null;
		float num = 100f;
		if (needatarget.tag == "playerdino")
		{
			for (int i = 0; i < enemybots.Count; i++)
			{
				float num2 = Vector3.Distance(needatarget.transform.position, enemybots[i].transform.position);
				if (num2 < num)
				{
					num = num2;
					tempgameobject = enemybots[i];
				}
			}
		}
		if (needatarget.tag == "enemydino")
		{
			for (int j = 0; j < playerbots.Count; j++)
			{
				float num3 = Vector3.Distance(needatarget.transform.position, playerbots[j].transform.position);
				if (num3 < num)
				{
					num = num3;
					tempgameobject = playerbots[j];
				}
			}
		}
		return tempgameobject;
	}

	public void attack()
	{
		for (int i = 0; i < playerbots.Count; i++)
		{
			playerbots[i].GetComponent<Bots>().startkilling();
		}
		for (int j = 0; j < enemybots.Count; j++)
		{
			enemybots[j].GetComponent<Bots>().startkilling();
		}
	}

	public void checktodecide()
	{
		if (over)
		{
			return;
		}
		over = true;
		if (playerbots.Count > 0 && enemybots.Count > 0)
		{
			UiManager.Instance.StartCoroutine(UiManager.Instance.fail());
		}
		else if (playerbots.Count > 0 && enemybots.Count <= 0)
		{
			UiManager.Instance.StartCoroutine(UiManager.Instance.won());
			if ((bool)AudioManager.instance)
			{
				AudioManager.instance.Play("pop");
			}
			if ((bool)Particaleffect.instance)
			{
				Particaleffect.instance.playpop();
			}
		}
		else if (enemybots.Count > 0 && playerbots.Count <= 0)
		{
			UiManager.Instance.StartCoroutine(UiManager.Instance.fail());
		}
	}

	public void addadino()
	{
		if (addhowmany <= 0)
		{
			return;
		}
		for (int i = 0; i < Merge.Instance.gridsurface.transform.childCount; i++)
		{
			if (Merge.Instance.gridsurface.transform.GetChild(i).GetComponent<grid>().have == null)
			{
				Vector3 position = Merge.Instance.gridsurface.transform.GetChild(i).position;
				GameObject obj = Object.Instantiate(position: new Vector3(position.x, position.y + 0.25f, position.z), original: addthisdino, rotation: Quaternion.identity);
				addhowmany--;
				obj.SetActive(true);
				break;
			}
		}
		if (addhowmany == 0)
		{
			UiManager.Instance.removedinobutton();
		}
	}
}

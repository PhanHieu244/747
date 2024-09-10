using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Merge : MonoBehaviour
{
	private RaycastHit hit;

	private RaycastHit frompresent;

	public bool lift;

	public GameObject presentlift;

	private Vector3 screenPoint;

	private Vector3 offset;

	public Material gridpanle;

	public Material greenpanle;

	public GameObject a;

	public GameObject b;

	public Vector3 liftedfrom;

	public List<GameObject> dinos;

	public GameObject gridsurface;

	public static Merge Instance;

	public GameObject fromlift;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Debug.DrawRay(base.transform.position, Vector3.forward * 10000f, Color.yellow);
			if (Physics.Raycast(ray, out hit, float.PositiveInfinity) && hit.collider != null && hit.collider.tag == "playerdino")
			{
				presentlift = hit.collider.gameObject;
				liftedfrom = presentlift.transform.position;
				presentlift.GetComponent<NavMeshAgent>().enabled = false;
				presentlift.transform.position = new Vector3(presentlift.transform.position.x, presentlift.transform.position.y + 0.55f, presentlift.transform.position.z);
				screenPoint = Camera.main.WorldToScreenPoint(hit.transform.position);
				Physics.Raycast(presentlift.transform.position, Vector3.down * 5f, out frompresent, float.PositiveInfinity);
				Debug.DrawRay(presentlift.transform.position, Vector3.down * 5f, Color.yellow);
				if (frompresent.collider != null && frompresent.collider.tag == "grid")
				{
					fromlift = frompresent.collider.gameObject;
					frompresent.collider.gameObject.GetComponent<grid>().have = null;
				}
				offset = hit.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
			}
		}
		if (Input.GetMouseButton(0) && presentlift != null)
		{
			Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
			Vector3 vector = Camera.main.ScreenToWorldPoint(position) + offset;
			presentlift.transform.position = new Vector3(vector.x, presentlift.transform.position.y, vector.z);
			Physics.Raycast(presentlift.transform.position, Vector3.down * 5f, out frompresent);
			Debug.DrawRay(presentlift.transform.position, Vector3.down * 5f, Color.yellow);
			if (frompresent.collider != null && frompresent.collider.tag == "grid")
			{
				frompresent.collider.gameObject.GetComponent<grid>().greenpanle();
			}
		}
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}
		if (presentlift != null)
		{
			Physics.Raycast(presentlift.transform.position, Vector3.down * 5f, out frompresent);
			if (frompresent.collider != null)
			{
				if (frompresent.collider.tag == "grid")
				{
					if (frompresent.collider.GetComponent<grid>().have != null)
					{
						if (frompresent.collider.GetComponent<grid>().have.GetComponentInChildren<SkinnedMeshRenderer>().name == presentlift.GetComponentInChildren<SkinnedMeshRenderer>().name)
						{
							GameObject have = frompresent.collider.GetComponent<grid>().have;
							GameObject gameObject = getdinoandmerge(presentlift);
							if (gameObject != null)
							{
								Vector3 position2 = frompresent.collider.transform.position;
								position2 = new Vector3(position2.x, position2.y + 0.15f, position2.z);
								GameObject obj = Object.Instantiate(gameObject, position2, have.transform.rotation);
								obj.tag = "playerdino";
								presentlift.transform.position = frompresent.transform.position;
								frompresent.collider.GetComponent<grid>().have = presentlift;
								BotsManager.instance.playerbots.Remove(have);
								BotsManager.instance.playerbots.Remove(presentlift);
								Object.Destroy(have);
								obj.SetActive(true);
								Object.Destroy(presentlift);
							}
							else
							{
								sendbacktotile();
							}
						}
						else
						{
							sendbacktotile();
						}
					}
					else
					{
						presentlift.transform.position = frompresent.collider.gameObject.transform.position;
						frompresent.collider.GetComponent<grid>().have = presentlift;
					}
				}
				else
				{
					sendbacktotile();
				}
			}
			else
			{
				sendbacktotile();
			}
		}
		presentlift = null;
	}

	public void sendbacktotile()
	{
		presentlift.transform.position = liftedfrom;
		fromlift.GetComponent<grid>().have = presentlift;
	}

	public GameObject getdinoandmerge(GameObject thisdino)
	{
		for (int i = 0; i < dinos.Count; i++)
		{
			if (dinos[i] != thisdino && dinos[i].GetComponentInChildren<SkinnedMeshRenderer>().name == thisdino.GetComponentInChildren<SkinnedMeshRenderer>().name)
			{
				if (i + 1 == dinos.Count)
				{
					return null;
				}
				return dinos[i + 1];
			}
		}
		return null;
	}
}

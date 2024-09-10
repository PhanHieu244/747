using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Bots : MonoBehaviour
{
	public Animator animator;

	public bool move;

	public GameObject presenttarget;

	public NavMeshAgent navMesh;

	private RaycastHit downn;

	public float distance;

	public GameObject canvasformeter;

	public Image meter;

	private RaycastHit frompresent;

	public float power;

	private void Start()
	{
		base.gameObject.layer = 3;
		animator = GetComponent<Animator>();
		animator.enabled = false;
		navMesh = GetComponent<NavMeshAgent>();
		if (base.transform.tag == "playerdino")
		{
			BotsManager.instance.playerbots.Add(base.gameObject);
			meter.color = BotsManager.instance.PlayerColor;
		}
		else
		{
			BotsManager.instance.enemybots.Add(base.gameObject);
			meter.color = BotsManager.instance.BotColor;
		}
		Physics.Raycast(base.transform.position, Vector3.down * 5f, out frompresent, float.PositiveInfinity);
		if (frompresent.collider != null && frompresent.collider.tag == "grid")
		{
			base.transform.position = frompresent.transform.position;
			frompresent.collider.GetComponent<grid>().have = base.gameObject;
		}
	}

	private void Update()
	{
		if (!move)
		{
			return;
		}
		if (presenttarget != null)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, presenttarget.transform.position, 1f * Time.deltaTime);
			base.transform.LookAt(presenttarget.transform.position);
			if (Vector3.Distance(base.transform.position, presenttarget.transform.position) < 1f)
			{
				fightstarter();
			}
		}
		else
		{
			move = false;
			startkilling();
		}
	}

	public void startkilling()
	{
		presenttarget = BotsManager.instance.returnatarget(base.gameObject);
		if (presenttarget == null)
		{
			animator.SetBool("isWalking", false);
			animator.SetBool("isAttacking", false);
			BotsManager.instance.checktodecide();
		}
		else
		{
			animator.enabled = true;
			animator.SetBool("isWalking", true);
			move = true;
		}
	}

	public void fightstarter()
	{
		move = false;
		navMesh.isStopped = true;
		base.transform.LookAt(presenttarget.transform.position, Vector3.up);
		animator.enabled = true;
		StartCoroutine(attackcall());
	}

	public void meterchange()
	{
		meter.fillAmount -= 0.1f;
		if (meter.fillAmount <= 0f)
		{
			if (base.tag == "playerdino")
			{
				BotsManager.instance.playerbots.Remove(base.gameObject);
				Object.Destroy(base.gameObject);
			}
			else if (base.tag == "enemydino")
			{
				BotsManager.instance.enemybots.Remove(base.gameObject);
				Object.Destroy(base.gameObject);
			}
		}
	}

	public void attack()
	{
		if (presenttarget == null)
		{
			startkilling();
			return;
		}
		animator.SetBool("isWalking", false);
		animator.SetBool("isAttacking", true);
		presenttarget.GetComponent<Bots>().meterchange();
		StartCoroutine(attackcall());
	}

	private IEnumerator attackcall()
	{
		yield return new WaitForSeconds(power);
		attack();
	}
}

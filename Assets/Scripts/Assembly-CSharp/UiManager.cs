using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
	public GameObject winpanle;

	public GameObject losspanle;

	public GameObject attackbutton;

	public static UiManager Instance;

	public GameObject dinobutton;

	public Text levelnumber;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
	}

	private void Start()
	{
		levelnumber.text = "Level-" + PlayerPrefs.GetInt("level", 1);
		winpanle.SetActive(false);
		losspanle.SetActive(false);
	}

	public void attack()
	{
		BotsManager.instance.attack();
		attackbutton.SetActive(false);
		Merge.Instance.gridsurface.SetActive(false);
	}

	public IEnumerator won()
	{
		yield return new WaitForSeconds(0.6f);
		winpanle.SetActive(true);
	}

	public IEnumerator fail()
	{
		yield return new WaitForSeconds(0.6f);
		losspanle.SetActive(true);
	}

	public void addadino()
	{
		BotsManager.instance.addadino();
	}

	public void nextlevel()
	{
		if (PlayerPrefs.GetInt("level") >= SceneManager.sceneCountInBuildSettings - 1)
		{
			PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level", 1) + 1);
			int num = Random.Range(1, SceneManager.sceneCountInBuildSettings);
			PlayerPrefs.SetInt("THISLEVEL", num);
			SceneManager.LoadScene(num);
		}
		else
		{
			PlayerPrefs.SetInt("level", SceneManager.GetActiveScene().buildIndex + 1);
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}

	public void restartlevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void removedinobutton()
	{
		dinobutton.SetActive(false);
	}
}

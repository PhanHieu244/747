using UnityEngine;
using UnityEngine.SceneManagement;

public class levelloder : MonoBehaviour
{
	private void Start()
	{
		if (PlayerPrefs.GetInt("level") >= SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(PlayerPrefs.GetInt("THISLEVEL"));
		}
		else
		{
			SceneManager.LoadScene(PlayerPrefs.GetInt("level", 1));
		}
	}
}

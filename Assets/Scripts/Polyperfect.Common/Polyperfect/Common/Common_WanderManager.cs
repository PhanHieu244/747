using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_WanderManager : MonoBehaviour
	{
		[SerializeField]
		private bool peaceTime;

		private static Common_WanderManager instance;

		public bool PeaceTime
		{
			get
			{
				return peaceTime;
			}
			set
			{
				SwitchPeaceTime(value);
			}
		}

		public static Common_WanderManager Instance
		{
			get
			{
				return instance;
			}
		}

		private void Awake()
		{
			if (instance != null && instance != this)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				instance = this;
			}
		}

		private void Start()
		{
			if (peaceTime)
			{
				Debug.Log("AnimalManager: Peacetime is enabled, all animals are non-agressive.");
				SwitchPeaceTime(true);
			}
		}

		public void SwitchPeaceTime(bool enabled)
		{
			if (enabled == peaceTime)
			{
				return;
			}
			peaceTime = enabled;
			Debug.Log(string.Format("AnimalManager: Peace time is now {0}.", enabled ? "On" : "Off"));
			foreach (Common_WanderScript allAnimal in Common_WanderScript.AllAnimals)
			{
				allAnimal.SetPeaceTime(enabled);
			}
		}

		public void Nuke()
		{
			foreach (Common_WanderScript allAnimal in Common_WanderScript.AllAnimals)
			{
				allAnimal.Die();
			}
		}
	}
}

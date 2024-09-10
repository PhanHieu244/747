using UnityEngine;
using UnityEngine.AI;

namespace Polyperfect.Common
{
	[ExecuteInEditMode]
	public class RandomCharacterPlacer : MonoBehaviour
	{
		[SerializeField]
		private float spawnSize;

		[SerializeField]
		private int spawnAmmount;

		[SerializeField]
		private GameObject[] characters;

		[ContextMenu("Spawn Characters")]
		private void SpawnAnimals()
		{
			GameObject gameObject = new GameObject("SpawnedCharacters");
			for (int i = 0; i < spawnAmmount; i++)
			{
				int num = Random.Range(0, characters.Length);
				Object.Instantiate(characters[num], RandomNavmeshLocation(spawnSize), Quaternion.identity, gameObject.transform);
			}
		}

		public Vector3 RandomNavmeshLocation(float radius)
		{
			Vector3 sourcePosition = Random.insideUnitSphere * radius + base.transform.position;
			Vector3 result = Vector3.zero;
			NavMeshHit hit;
			if (NavMesh.SamplePosition(sourcePosition, out hit, radius, 1))
			{
				result = hit.position;
			}
			return result;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(base.transform.position, spawnSize);
		}
	}
}

using UnityEngine;

namespace NewBezier
{
	public class MyPathCreator : MonoBehaviour
	{
		[SerializeField] public MyPath path;

		public void CreatePath()
		{
			path = new MyPath(transform.position, transform);
		}
	}
}
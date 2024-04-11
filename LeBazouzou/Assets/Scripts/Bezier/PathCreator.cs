using UnityEngine;

namespace Bezier
{
    public class PathCreator : MonoBehaviour
    {
        [SerializeField]
        public Path path;

        public void CreatePath()
        {
            path = new Path(transform.position);
        }
    }
}

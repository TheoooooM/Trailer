using System.Collections.Generic;
using UnityEngine;

namespace Bezier
{
    [System.Serializable]
    public class Path
    {
        [SerializeField]
        private List<Vector3> points;

        public Path(Vector3 center)
        {
            points = new List<Vector3>()
            {
                center+Vector3.left,
                center+(Vector3.left + Vector3.up),
                center+(Vector3.right + Vector3.down),
                center+Vector3.right
            };
        }

        public Vector3 this[int i] => points[i];
        
        public int PointAmount => points.Count;
        public int SegmentAmount => (points.Count - 4)/3 + 1;

        public void AddSegment(Vector3 anchorPoint)
        {
            points.Add(points[^1]*2-points[^2]);
            points.Add((points[^1] + anchorPoint)/2);
            points.Add(anchorPoint);
        }

        public void SplitSegment(Vector3 anchorPoint, int segmentIndex)
        {
            var startSPoint = segmentIndex * 3;
            var previousTangent = ((points[startSPoint] + points[startSPoint + 1]) / 2 + anchorPoint)/2;
            var nextTangent = ((points[startSPoint+2] + points[startSPoint + 3]) / 2 + anchorPoint)/2;
            points.InsertRange(segmentIndex*3+2, new []{previousTangent, anchorPoint,nextTangent});
        }

        public void SetSegmentStraight(int segmentIndex)
        {
            var startSPoint = segmentIndex * 3;
            points[startSPoint + 1] = (points[startSPoint + 3] - points[startSPoint])*.25f + points[startSPoint];
            points[startSPoint + 2] = (points[startSPoint ] - points[startSPoint + 3])*.25f + points[startSPoint + 3];
        }

        public void RemovePoint(int i)
        {
            if (SegmentAmount < 2) return;
            int startIndex = i-1;
            if (i == 0) startIndex = 0;
            else if (i == points.Count - 1) startIndex = i - 2;
            points.RemoveRange(startIndex,3);
        }

        public Vector3[] GetSegmentPoints(int i)
        {
            return new[]
            {
                points[i * 3],
                points[i * 3 + 1],
                points[i * 3 + 2],
                points[i * 3 + 3],
            };
        }

        public void MovePoint(int i, Vector3 newPos, bool moveMirrorTangent = false)
        {
            var deltaPos = newPos - points[i];
            points[i] = newPos;
            if (!IsPointTangent(i))
            { 
                if(i!=0)points[i-1] += deltaPos;
                if(i!=points.Count-1)points[i+1] += deltaPos;
            }
            else if (moveMirrorTangent && !(i == 1 || i == points.Count-2))
            {
                var center = Vector3.zero;
                var mirrorIndex = 0;
                //check if is startTangent
                if (i % 3 == 1)
                {
                    center = points[i - 1];
                    mirrorIndex = i - 2;
                }
                else
                {
                    center = points[i + 1];
                    mirrorIndex = i + 2;
                }

                points[mirrorIndex] = center*2-newPos;
            }
        }

        public bool IsPointTangent(int i)
        {
           return i%3 != 0;
        }

        public bool isNull()
        {
            return points == null;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace NewBezier
{

    public class MyPath
    {
        [Serializable]
        public class Point
        {
            private Transform _transform;

            public Transform parent
            {
                get => _transform.parent;
                set => _transform.parent = value;
            }
            
            public Vector3 Position => _transform.position;
            private Vector3 previousTangent;

            public Vector3 PreviousTangent
            {
                get => previousTangent + Position;
                set => SetTangeantValue(value, true);
            }

            private Vector3 nextTangent;
            public Vector3 NextTangent
            {
                get => nextTangent + Position;
                set => SetTangeantValue(value, false);
            }
            public UnityEvent unityEvent;
            
            public Point(Vector3 position)
            {
                _transform = Object.Instantiate(new GameObject(), position, Quaternion.identity).transform;
            }

            public Point(Vector3 position, Vector3 previousTangent) : this(position)
            {
                this.previousTangent = previousTangent;
            }

            public Point(Vector3 position, Vector3 previousTangent, Vector3 nextTangent) : this(position,
                previousTangent)
            {
                this.nextTangent = nextTangent;
            }

            private void SetTangeantValue(Vector3 value, bool isPrevious)
            {
                if (isPrevious) previousTangent = value - Position;
                else nextTangent = value - Position;
            }
            
            public static explicit operator Vector3(Point p) => p.Position;

            ~Point()
            {
                Destroy();
            }

            public void Destroy()
            {
                Object.Destroy(_transform.gameObject);
            }
        }

        private List<Point> _points;

        private Transform _positionParent;

        public MyPath(Vector3 center, Transform posParent)
        {
            Point point1 = new(center + Vector3.left, Vector3.zero, center + (Vector3.left + Vector3.up));
            Point point2 = new(Vector3.right, center + (Vector3.right + Vector3.down));
            point1.parent = point2.parent = _positionParent = posParent;
            _points = new List<Point>()
            {
                point1,
                point2
            };
        }

        public Point this[int i] => _points[i];

        public int PointAmount => _points.Count;
        public int SegmentAmount => _points.Count - 1;


        public void AddSegment(Vector3 newAnchorPoint)
        {
            _points[^1].NextTangent = _points[^1].Position * 2 - _points[^1].PreviousTangent;
            _points.Add(new(newAnchorPoint, _points[^1].NextTangent + newAnchorPoint));
            _points[^1].parent = _positionParent;
        }

        public void SplitSegment(Vector3 pos, int index)
        {
            var previousPos = ((_points[index].Position + _points[index].NextTangent) / 2 + pos) / 2;
            var nextPos = ((_points[index + 1].Position + _points[index + 1].NextTangent) / 2 + pos) / 2;
            var point = new Point(pos, previousPos, nextPos);
            point.parent = _positionParent;
            _points.Insert(index, point);
        }

        public void SetSegmentStraight(int index)
        {
            _points[index].NextTangent = (_points[index + 1].Position - _points[index].Position) * .25f +
                                         _points[index].Position;
            _points[index + 1].PreviousTangent = (_points[index].Position - _points[index + 1].Position) * .25f +
                                                 _points[index + 1].Position;
        }

        public void RemovePoint(int i)
        {
            if (_points.Count > 2)
            {
                var point = _points[i];
                _points.RemoveAt(i);
                point.Destroy();
            }
        }

        public Vector3[] GetSegmentPoints(int i) => new[]
        {
            _points[i].Position,
            _points[i].NextTangent,
            _points[i + 1].PreviousTangent,
            _points[i + 1].Position
        };

        public void MoveTangeant(int i, bool isPrevious, Vector3 pos, bool moveMirrored = false)
        {
            if (isPrevious || moveMirrored) _points[i].PreviousTangent = pos;
            if(!isPrevious || moveMirrored) _points[i].NextTangent = pos;
        }
    }
}

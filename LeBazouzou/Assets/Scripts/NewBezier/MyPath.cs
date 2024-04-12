using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace NewBezier
{

    [Serializable]
    public class MyPath
    {
        [Serializable]
        public class Point
        {
            [SerializeField]private Transform _transform;
            private Transform Transform
            {
                get
                {
                    if (!_transform) _transform = CreateTransform();
                    return _transform;
                }
                set
                {
                    _transform = value;
                    _transform.parent = value.parent;
                }
            }

            [HideInInspector]public Transform parent;
            
            public string Name
            {
                get => Transform.name;
                set => Transform.name = value;
            }

            private Vector3 _position;
            public Vector3 Position
            {
                get => _position;
                set{
                    _position = value;
                    Transform.position = value;
                }
            }

            private Vector3 _previousTangent;
            public Vector3 PreviousTangent
            {
                get => _previousTangent + Position;
                set=>SetTangeantValue(value, true);
                
            }


            private Vector3 _nextTangent;
            public Vector3 NextTangent
            {
                get => _nextTangent + Position;
                set => SetTangeantValue(value, false);
            }
            public UnityEvent unityEvent;
            
            public Point(Transform parent, Vector3 position)
            {
                this.parent = parent;
                Position = position;
            }

            public Point(Transform parent,Vector3 position, Vector3 previousTangent) : this(parent,position)
            {
                if(previousTangent != Vector3.zero)PreviousTangent = previousTangent;
            }

            public Point(Transform parent,Vector3 position, Vector3 previousTangent, Vector3 nextTangent) : this(parent, position,
                previousTangent)
            {
                if(nextTangent != Vector3.zero)NextTangent = nextTangent;
            }

            private void SetTangeantValue(Vector3 value, bool isPrevious)
            {
                if (isPrevious) _previousTangent = value - Position;
                else _nextTangent = value - Position;
            }
            
            public static explicit operator Vector3(Point p) => p.Position;

            ~Point()
            {
                Destroy();
            }

            Transform CreateTransform()
            { 
                var transform = Object.Instantiate(new GameObject(), Position, Quaternion.identity).transform;
                transform.parent = parent;
                return transform;
            }
            
            public void Destroy()
            {
                GameObject.Destroy(Transform.gameObject);
            }
        }

        public List<Point> _points;

        private Transform _parent;

        public MyPath(Vector3 center, Transform parent)
        {
            _parent = parent;
            Point point1 = new(parent, center + Vector3.left, Vector3.zero, center + (Vector3.left + Vector3.up));
            Point point2 = new(parent, center + Vector3.right, center + (Vector3.right + Vector3.down));
            _points = new List<Point>()
            {
                point1,
                point2
            };
            UdpateNames();
        }

        public Point this[int i] => _points[i];

        public int PointAmount => _points.Count;
        public int SegmentAmount => _points.Count - 1;


        public Point NewPoint(Vector3 pos, Vector3 previousPos, Vector3 nextPos) => new(_parent, pos, previousPos,nextPos);
        
        public void AddSegment(Vector3 newAnchorPoint)
        {
            _points[^1].NextTangent = _points[^1].Position * 2 - _points[^1].PreviousTangent;
            _points.Add(new(_parent, newAnchorPoint, _points[^1].NextTangent + newAnchorPoint));
            _points[^1].parent = _parent;
            UdpateNames();
        }

        public void SplitSegment(Vector3 pos, int index)
        {
            var previousPos = ((_points[index].Position + _points[index].NextTangent) / 2 + pos) / 2;
            var nextPos = ((_points[index + 1].Position + _points[index + 1].NextTangent) / 2 + pos) / 2;
            var point = new Point(_parent,pos, previousPos, nextPos);
            point.parent = _parent;
            _points.Insert(index, point);
            UdpateNames();
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
            UdpateNames();
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

        public void UdpateNames()
        {
            for (int i = 0; i < _points.Count; i++)
            {
                _points[i].Name = $"Point {i}";
            }
        }
        public bool isNull()
        {
            return _points == null;
        }
    }
}

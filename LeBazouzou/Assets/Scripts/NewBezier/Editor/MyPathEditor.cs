using System;
using UnityEditor;
using UnityEngine;

namespace NewBezier.Editor
{
	[CustomEditor(typeof(MyPathCreator))]
	public class MyPathEditor : UnityEditor.Editor
	{
		private MyPathCreator creator;
		private MyPath path;


		private const float DetectSegmentRange = .5f;
		private int _selectedSegmentIndex = -1;
		private float _currentMouseDepth;

		private Vector3 _lastCreatorPos;

		private void OnSceneGUI()
		{
			Draw();
			Input();
		}


		void Input()
		{
			Event guiEvent = Event.current;
			Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).GetPoint(Vector3.Distance(Camera.current.transform.position, path[path.PointAmount-1].Position));

			if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
			{
				if (_selectedSegmentIndex != -1)
				{
					Undo.RecordObject(creator, "Split Segment");
					var pos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).GetPoint(_currentMouseDepth);
					path.SplitSegment(pos, _selectedSegmentIndex);
				}
				else
				{
					Undo.RecordObject(creator, "Add Point");
					path.AddSegment(mousePos);
				}
			}

			if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 )
			{
				if (guiEvent.alt)
				{
					if (_selectedSegmentIndex != -1)
					{
						Undo.RecordObject(creator, "Straight Segment");
						path.SetSegmentStraight(_selectedSegmentIndex);
					}
				}
				else
				{
					float minDst = .1f;
					float minDepth = 100;
					int nearPointIndex = -1;
					for (int i = 0; i < path.PointAmount; i++)
					{
						var pointDepth = Vector3.Distance(Camera.current.transform.position, path[i].Position);
						var currentMousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition)
							.GetPoint(pointDepth);
						var dist = Vector3.Distance(currentMousePos, path[i].Position);
						if (dist < minDst && pointDepth < minDepth)
						{
							minDepth = pointDepth;
							nearPointIndex = i;
						}
					}

					if (nearPointIndex != -1)
					{
						Undo.RecordObject(creator, "Remove Point");
						path.RemovePoint(nearPointIndex);
					}
				}
			}
			
			float minSegmentDepth = 100;
			int newSelectedSegmentIndex = -1;
			for (int i = 0; i < path.SegmentAmount; i++)
			{
				Vector3[] segmentPoints = path.GetSegmentPoints(i);
				var averagePoint = (segmentPoints[0] + segmentPoints[1] + segmentPoints[2] + segmentPoints[3])/4;
				var pointDepth = Vector3.Distance(Camera.current.transform.position, averagePoint);
				var currentMousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).GetPoint(pointDepth);
				float dst = HandleUtility.DistancePointBezier(currentMousePos, segmentPoints[0], segmentPoints[3], segmentPoints[1], segmentPoints[2]);
				if (dst < DetectSegmentRange && pointDepth < minSegmentDepth)
				{
					minSegmentDepth = pointDepth;
					newSelectedSegmentIndex = i;
				}
			}

			if (newSelectedSegmentIndex != _selectedSegmentIndex)
			{
				_selectedSegmentIndex = newSelectedSegmentIndex;
				_currentMouseDepth = minSegmentDepth;
				HandleUtility.Repaint(); 
			}

			var creatorPos = creator.transform.position;
			if (creatorPos != _lastCreatorPos)
			{
				var delta = creatorPos - _lastCreatorPos;
				for (int i = 0; i < path.PointAmount; i++)
				{
					path[i].Position += delta;
				}
				_lastCreatorPos = creatorPos;
			}
		}

		void Draw()
		{
			for (int i = 0; i < path.PointAmount-1; i++)
			{
				Vector3[] points = path.GetSegmentPoints(i);
				
				Handles.color = Color.white;
				Handles.DrawBezier(points[0],points[3],points[1],points[2],Color.white,null, 3f); 
				Handles.color = Color.green;
				Handles.DrawLine(points[0],points[1]);
				Handles.DrawLine(points[2],points[3]);
			}
			
			for (int i = 0; i < path.PointAmount; i++)
			{
				Handles.color = Color.red;
				Handles.FreeMoveHandle(path[i].Position,.1f,Vector3.zero, Handles.CylinderHandleCap);
				Handles.color = Color.yellow;
				if (path[i].PreviousTangent != path[i].Position)
				{
					Vector3 newPreviousPos = Handles.FreeMoveHandle(path[i].PreviousTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
					if (path[i].PreviousTangent != newPreviousPos)
					{
						Undo.RecordObject(creator, "Move Tangent");
						path.MoveTangeant(i, true, newPreviousPos, Event.current.alt);
					}
				}

				if (path[i].NextTangent != path[i].Position)
				{
					Vector3 newNextPos = Handles.FreeMoveHandle(path[i].NextTangent, .1f, Vector3.zero, Handles.CylinderHandleCap);
					if (path[i].NextTangent != newNextPos)
					{
						Undo.RecordObject(creator, "Move Tangent");
						path.MoveTangeant(i, false, newNextPos, Event.current.alt);
					}
				}
			}
		}

		private void OnEnable()
		{
			creator = (MyPathCreator)target;
			_lastCreatorPos = creator.transform.position;
			if (creator.path == null || creator.path.isNull())
			{
				creator.CreatePath();
			}
			path = creator.path;
		}
	}
}
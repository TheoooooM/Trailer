using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Bezier.Editor
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : UnityEditor.Editor
    {
        private PathCreator creator;
        private Path path;

        private const float detectSegmentRange = .5f;
        private float currentMouseDepth = 0;
        private int selectedSegmentIndex = -1;

        private Vector3 lastCreatorPos;

        private void OnSceneGUI()
        {
            Input();
            Draw();
        }

        void Input()
        {
            Event guiEvent = Event.current;
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).GetPoint(Vector3.Distance(Camera.current.transform.position, path[path.PointAmount-1]));

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)// Add Point
            {
                if (selectedSegmentIndex != -1)
                { 
                    Undo.RecordObject(creator,"Split Segment");
                    var pos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).GetPoint(currentMouseDepth);
                    path.SplitSegment(pos, selectedSegmentIndex);
                }
                else
                {
                    Undo.RecordObject(creator, "Add Point");
                    EditorUtility.SetDirty(creator);
                    path.AddSegment(mousePos);
                }
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1) // Delete Point
            {
                if (guiEvent.alt)
                {
                    if (selectedSegmentIndex != -1)
                    {
                        Undo.RecordObject(creator, "Straight Segment");
                        path.SetSegmentStraight(selectedSegmentIndex);
                    }
                }
                else
                {
                    float minDst = .1f;
                    float minDepth = 100;
                    int nearPointIndex = -1;
                    for (int i = 0; i < path.PointAmount; i++)
                    {
                        var pointDepth = Vector3.Distance(Camera.current.transform.position, path[i]);
                        var currentMousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition)
                            .GetPoint(pointDepth);
                        var dist = Vector3.Distance(currentMousePos, path[i]);
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
                if (dst < detectSegmentRange && pointDepth < minSegmentDepth)
                {
                    minSegmentDepth = pointDepth;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                currentMouseDepth = minSegmentDepth;
                HandleUtility.Repaint();     
            }

            var creatorPos = creator.transform.position;
            if (lastCreatorPos != creatorPos)
            {
                var deltaPos = creatorPos - lastCreatorPos;
                for (int i = 0; i < path.PointAmount; i+=3)
                {
                    path.MovePoint(i, path[i]+deltaPos); 
                }

                lastCreatorPos = creatorPos;
            }
        }

        void Draw()
        {
            for (int i = 0; i < path.SegmentAmount; i++)
            {
                Vector3[] points = path.GetSegmentPoints(i);
                
                Handles.color = i == selectedSegmentIndex? Color.yellow : Color.white;
                Handles.DrawBezier(points[0],points[3],points[1],points[2],Color.white,null, 3f); 
                Handles.color = Color.green;
                Handles.DrawLine(points[0],points[1]);
                Handles.DrawLine(points[2],points[3]);
            }
            
            for (int i = 0; i < path.PointAmount; i++)
            {
                Handles.color = path.IsPointTangent(i)? Color.yellow : Color.red;
                Vector3 newPosition = Handles.FreeMoveHandle(path[i], .1f, Vector3.zero, Handles.CylinderHandleCap);
                if (path[i] != newPosition)
                {
                    Undo.RecordObject(creator,"Move Point");
                    EditorUtility.SetDirty(creator);
                    path.MovePoint(i,newPosition, Event.current.alt);
                }
            }
        }
        
        private void OnEnable()
        {
            creator = (PathCreator)target;
            lastCreatorPos = creator.transform.position;
            if (creator.path == null || creator.path.isNull())
            {
                creator.CreatePath();
                EditorUtility.SetDirty(creator);
            }

            path = creator.path;
        }
    }
}

using Bezier;
using DG.Tweening;
using UnityEngine;

public class CameraTrail : MonoBehaviour
{
    [SerializeField] private PathCreator _creator;
    [Space]
    [SerializeField]
    public int index = 0;
    [SerializeField] private float moveDuration = 1;

    private Path path;

    void Start()
    {
        Init();
        
    }

    public void Init()
    {
        path = _creator.path;
        GoToPoint(index);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) GoNextPoint();
    }

    private void GoNextPoint()
    {
        index++;
        var sequence = DOTween.Sequence();
        float lerpValue = 0;

        sequence.Append(DOVirtual.Float(0, 1, moveDuration, t => MoveAlongPath(index,t)));
        
    }

    public void GoToPoint(int index)
    {
        if(index>path.PointAmount-1 || index<0)return;
        transform.position = path[index * 3];
    }

    void MoveAlongPath(int segment, float t)
    {
        var points = path.GetSegmentPoints(segment-1);
        var newPos = GetBezierPosition(points[0], points[1], points[2], points[3], t);
        transform.position = newPos;
    }

    Vector3 GetBezierPosition(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
    {
        var p0 = Vector3.Lerp(a, b,t);
        var p1 = Vector3.Lerp(b, c,t);
        var p2 = Vector3.Lerp(c, d,t);

        var p3 = Vector3.Lerp(p0, p1, t);
        var p4 = Vector3.Lerp(p1, p2, t);

        return Vector3.Lerp(p3, p4, t);
    }

    
}

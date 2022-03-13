using UnityEngine;

namespace EC.Core {
    
    [ExecuteAlways]
    public class CameraLockTargetTest : MonoBehaviour {

    public Transform PointA;
    public Transform PointB;
    public Transform PointC;
    public Transform PointD;
    public Transform PointE;

    public int CircleSamples = 128;

    public float dVectorMagnitude = 10f;
    public float dcLength = 1f;

    private LineRenderer _lineRenderer;
    
    // Cache
    private int _lastCircleSamples;
    private Vector2 _lastPointAPos;
    private Vector2 _lastPointBPos;
    private Vector2 _lastPointCPos;
    private float _dVectorMag;
    private float _dcLength;

    private void OnEnable() {
        _lineRenderer = GetComponent<LineRenderer>();
        Draw();
    }

    private void Update() {
        Draw();
        if (ShouldRedraw()) {
        }
    }

    private void Draw() {
        Vector3 aPos = PointA.position;
        Vector3 bPos = PointB.position;
        Vector3 cPos = PointC.position;
        
        PointD.gameObject.SetActive(false);
        
        float radius = (aPos - bPos).magnitude;

        #region Draw Circle
        Vector3[] circlePositions = new Vector3[CircleSamples];
        var degreeIncrements = 360f / (float)CircleSamples;

        for (int i = 0; i < CircleSamples; i++) {
            float radians = (degreeIncrements * i) * Mathf.Deg2Rad;
            var pos = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * radius;
            circlePositions[i] = pos + aPos;
        }
        _lineRenderer.positionCount = CircleSamples;
        _lineRenderer.SetPositions(circlePositions);
        #endregion
        
        #region Connect Points

        // SetPointData(PointA);
        // SetPointData(PointB);
        // SetPointData(PointC);

        Connect(PointA, PointB);
        Connect(PointB, PointC);
        Connect(PointC, PointA);
        
        #endregion

        #region Important
        var bcVector = bPos - cPos;
        var acVector = aPos - cPos;

        var dVectorDir = (bcVector + acVector).normalized;
        Debug.DrawLine(cPos, cPos + (dVectorDir * dVectorMagnitude), Color.blue);
        
        var dVectorNormal = Vector3.Cross(bcVector, acVector).normalized;
        Debug.DrawLine(cPos, cPos + (dVectorNormal * dVectorMagnitude), Color.green);
        
        var dVectorPerpToA = Vector3.Cross(dVectorDir, dVectorNormal).normalized;
        Debug.DrawLine(cPos, cPos + (dVectorPerpToA * radius), Color.red);
        // Debug.DrawLine(aPos, aPos + (dVectorPerpToA * radius), Color.yellow);
        
        var acVectorReflectDir = Vector3.Reflect(acVector.normalized, dVectorPerpToA);
        Debug.DrawLine(aPos, aPos + (acVectorReflectDir * radius), Color.yellow);
        #endregion

        
        // PointD.gameObject.SetActive(true);
        // PointD.position = cPos + (dVectorDir * dVectorMagnitude);
        
        // var dVectorNormal = Vector2.Perpendicular(dVectorDir);
        //
        // var cLR = PointC.Find("Line").GetComponent<LineRenderer>();
        // cLR.positionCount = 2;
        // cLR.SetPosition(0, aPos);
        // cLR.SetPosition(1, aPos + (dVectorNormal * dcLength));
        // cLR.useWorldSpace = true;
        //
        // var acVectorDirReflect = Vector2.Reflect(acVector.normalized, dVectorNormal);
        //
        // var aLR = PointC.Find("Line").GetComponent<LineRenderer>();
        // aLR.positionCount = 2;
        // aLR.SetPosition(0, aPos);
        // aLR.SetPosition(1, aPos + (acVectorDirReflect * dcLength));
        // aLR.useWorldSpace = true;
        //
        // PointD.position = aPos + (acVectorDirReflect * radius);
        // Connect(PointD, PointC, true, true);
        
        #region Cache

        _lastCircleSamples = CircleSamples;
        _lastPointAPos = aPos;
        _lastPointBPos = bPos;
        _lastPointCPos = cPos;
        _dVectorMag = dVectorMagnitude;
        _dcLength = dcLength;

        #endregion
    }

    private void Connect(Transform point1, Transform point2, bool useWorldSpace = true, bool isD = false) {
        LineRenderer lr = point1.GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, point1.position);
        lr.SetPosition(1, point2.position);
        lr.useWorldSpace = useWorldSpace;
    }

    private bool ShouldRedraw() {
        if (!PointA || !PointB || !PointC || !PointD) return false;
        
        return (
            _lastCircleSamples != CircleSamples ||
            _lastPointAPos != (Vector2)PointA.position ||
            _lastPointBPos != (Vector2)PointB.position ||
            _lastPointCPos != (Vector2)PointC.position ||
            Mathf.Abs(_dVectorMag - dVectorMagnitude) > 0.001 ||
            Mathf.Abs(_dcLength - dcLength) > 0.001
        );
    }
    }
}
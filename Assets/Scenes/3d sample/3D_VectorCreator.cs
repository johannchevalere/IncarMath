using System.Collections;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class ThreeDVectorCreator : MonoBehaviour
{
    public Color ColorVec1 = Color.white;
    public Color ColorVec2 = Color.white;
    public Color ColorVec3 = Color.white;
    public MathManager manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 v1 = -1 * Vector3.one;
        Vector3 v2 = Vector3.down + Vector3.right + Vector3.back;
        manager.InstantiateVector(Vector3.zero, v1, transform).GetComponent<Vector>().ChangeColor(ColorVec1);
        manager.InstantiateVector(Vector3.zero, v2, transform).GetComponent<Vector>().ChangeColor(ColorVec2);
        manager.InstantiateVector(Vector3.zero, new Vector3( 0, 1, -1), transform).GetComponent<Vector>().ChangeColor(ColorVec3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

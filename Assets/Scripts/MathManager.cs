using UnityEngine;

public class MathManager : MonoBehaviour
{
    static public MathManager instance;
    public GameObject MathPoint;
    public GameObject MathVector;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        instance = this;
    }
    public GameObject InstantiatePoint(Vector3 position, Transform parent, float scale =1)
    {
        GameObject newPoint = Instantiate(MathPoint, parent, false);
        if (newPoint.GetComponent<Point>() == null) newPoint.AddComponent<Point>();
        newPoint.transform.localPosition = position;
        newPoint.transform.localScale *= scale;
        return newPoint;
    }
    public GameObject InstantiateVector(Vector3 initialPosition, Vector3 terminalPosition, Transform parent,  float scale =1, bool isMoving = true)
    {
        GameObject newVector = Instantiate(MathVector, parent, false);
        
        if (newVector.GetComponent<Vector>() == null) newVector.AddComponent<Vector>();
        Vector v = newVector.GetComponent<Vector>();
        if (v == null) v = newVector.AddComponent<Vector>();
        v.transform.localScale *= scale;
        v.changePointPosition(initialPosition, false);
        v.changePointPosition(terminalPosition, true);
        v.IsMoving = isMoving;
        return newVector;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

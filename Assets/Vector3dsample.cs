using UnityEngine;

public class Vector3dsample : MonoBehaviour
{

    public Color Vector1Color;
    public Color Vector2Color;
    public Color Vector3Color;
    public MathManager mathManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mathManager.InstantiateVector(Vector3.zero, new Vector3(1.5f, -0.2f, 0.5f), transform).GetComponent<Vector>().ChangeColor(Vector1Color);
        mathManager.InstantiateVector(Vector3.zero, new Vector3(-0.6f, -0.2f, 0.8f), transform).GetComponent<Vector>().ChangeColor(Vector2Color);
        mathManager.InstantiateVector(Vector3.zero, new Vector3(0.06f, 1.5f, 0.42f), transform).GetComponent<Vector>().ChangeColor(Vector3Color);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

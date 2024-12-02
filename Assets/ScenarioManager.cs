using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    enum State { Exercise, Explanation}
    public static ScenarioManager instance;
    public Repere exerciseGrid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null && instance != this) Destroy(this.gameObject);
        instance = this;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkVectorByCoordinates()
    {
        




    }
}

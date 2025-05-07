using TMPro;
using UnityEngine;

public class SolePointUI : MonoBehaviour
{
    public TMP_Text pointNameObject;
    public TMP_Text pointCoordOject;
    public string pointName = "";
    public string pointCoord = "";
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pointNameObject.text = pointName;
        pointCoordOject.text = pointCoord;
    }
}

using UnityEngine;
using TMPro;
public class SoleVectorUI : MonoBehaviour
{
    public TMP_Text vectorNameObject;
    public TMP_Text vectorCoordObject;
    public TMP_Text vectorArrow;
    public string vectorName;
    public string vectorCoord;
    public Color ArrowColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        vectorNameObject.text = vectorName;
        vectorCoordObject.text = vectorCoord;
        if (string.IsNullOrWhiteSpace(vectorName)) vectorArrow.gameObject.SetActive(false);
        else vectorArrow.gameObject.SetActive(true);
    }
}

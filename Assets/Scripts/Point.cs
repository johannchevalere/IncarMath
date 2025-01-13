using System.Collections;
using UnityEngine;
using TMPro;

public class Point : MonoBehaviour
{
    public TMP_Text pointName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void setName(string name)
    {
        pointName.text = name;
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeColor(Color color)
    {
        GetComponent<Renderer>().material.color = color;
    }

    IEnumerator changeColorTemporary(Color color, float seconds)
    {
        Color currentColor = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = color;
        yield return new WaitForSeconds(seconds);
        GetComponent<Renderer>().material.color = currentColor;
    }
}

using System.Collections;
using UnityEngine;

public class Point : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    public float duration = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        StartCoroutine("fadeIn");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator fadeIn()
    {
        var time = Time.time;
        var rends = GetComponentsInChildren<Renderer>();
        while (Time.time - time < duration)
        {
            foreach (var r in rends)
            {
                var tempColor = r.material.color;
                tempColor.a -= (Time.deltaTime / duration);
                r.material.color = tempColor;
            }
            yield return null;
        }
        Destroy(this);
    }
}

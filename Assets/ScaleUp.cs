using System.Collections;
using UnityEngine;

public class ScaleUp : MonoBehaviour
{
    public float scaleFactor = 1.1f;
    public float fullZoomDurationInSeconds = 0.6f;
    public float transitionDurationInSeconds = 0.2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        StartCoroutine(Zoom());
    }

    // Update is called once per frame
    IEnumerator Zoom()
    {
        //transition phase
        float t = 0;
        Vector3 initialScale = transform.localScale;
        while (t < transitionDurationInSeconds)
        {
            transform.localScale = initialScale * Mathf.SmoothStep(1, scaleFactor, t/transitionDurationInSeconds);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForSeconds(fullZoomDurationInSeconds);
        t = 0;
        while (t < transitionDurationInSeconds)
        {
            transform.localScale = initialScale * Mathf.SmoothStep(scaleFactor, 1, t/transitionDurationInSeconds);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        EndZoom();
            
    }

    void EndZoom()
    {
        Destroy(this);
    }
}

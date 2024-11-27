using UnityEngine;

public class Vector : MonoBehaviour
{
    private bool isOnGrid = false;
    public Vector3 initialPoint = Vector3.zero;
    public Vector3 terminalPoint = Vector3.up;
    public bool IsMoving { get; set; }// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    //Modify the size of the Vector
    public void changePointPosition(Vector3 newPosition, bool terminalPointMoving = true)
    {
        Transform shaft = transform.Find("Shaft");
        Transform arrow = transform.Find("Arrow");
        float oldMagnitude = Vector3.Distance(initialPoint, terminalPoint);
        if (terminalPointMoving) terminalPoint = newPosition;
        else initialPoint = newPosition;

        Vector3 heading = terminalPoint - initialPoint;
        if (shaft != null)
        {
            shaft.localScale = new Vector3(shaft.localScale.x,0.5f * heading.magnitude, shaft.localScale.z);
        }
        if (arrow != null)
        {
            arrow.localPosition += Vector3.up * 0.5f * (heading.magnitude - oldMagnitude);
        }
        transform.localPosition = transform.localScale.y * (initialPoint + heading / 2);

        if (heading.magnitude > 0) transform.up = heading/heading.magnitude;
            
    }
    // Update is called once per frame
    private float magnitude()
    {
        return Vector3.Distance(initialPoint, terminalPoint);
    }
    void Update()
    {
        {
            //if the vector is very small
            if (magnitude() < 0.1 * transform.localScale.x)
            {
                //Make the arrow disappear if vector is too small
                if (transform.Find("Arrow").GetComponent<Renderer>().enabled) transform.Find("Arrow").GetComponent<Renderer>().enabled = false;
                //Behaviour when vector is small and is moving
                if (IsMoving) { }
                //Behaviour when vector is small and is not moving
                else { Destroy(this); }
            }
            else
            {

                if (!transform.Find("Arrow").GetComponent<Renderer>().enabled) transform.Find("Arrow").GetComponent<Renderer>().enabled = true;
            }
        }
        }
    }

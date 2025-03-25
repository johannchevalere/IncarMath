using System.Runtime.ExceptionServices;
using UnityEngine;

public class ChangeHand : MonoBehaviour
{

    public GameObject RayInteractor;
    public GameObject LeftHand;
    public GameObject RightHand;
    private int HandState = 0;


    public void changeHand()
    {
        if (HandState == 0)
        {
            RayInteractor.transform.parent = LeftHand.transform;
            HandState = 1;
            return;
        }
        if (HandState == 1)
        {
            RayInteractor.transform.parent = RightHand.transform;
            HandState = 0;
            return;


            int a = 27;
        }
    }
        
    
}

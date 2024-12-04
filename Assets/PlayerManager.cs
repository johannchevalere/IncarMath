using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    private int state = 0;
    [SerializeField] Repere grid;
    [SerializeField] XRRayInteractor rayInteractor;
    [SerializeField] Transform pointer;
    [SerializeField] InputActionAsset gridActionAsset;

    [Header("UI")]
    [SerializeField] TMP_Text text;

    Vector3 pointerPositionOnGrid;
    int currentPointID;


    private void Update()
    {
        RaycastHit hit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            pointerPositionOnGrid = hit.point;
            pointer.position = grid.CoordToPos(grid.posToRoundCoord(hit.point));
        }   
    }
    private void OnSelect()
    {
        RaycastHit hit;
        //If we are on grid
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            Debug.Log("Hit");
            if (grid.trySelectPointByPos(hit.point, out int id))
            {
                currentPointID = id;
                //Todo afficher les informations du point
                return;
            }//if a point exists at current coordinates, we select it and end method
            currentPointID = grid.CreatePoint(hit.point); //else we create a point. Storing point id in case new vector is created
        }
    }
    private void OnHoldSelect()
    {
        //Create Vector by holding trigger
        RaycastHit hit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            text.text = "Pointer at " + grid.posToCoordinates(hit.point).ToString();
            StartCoroutine(createVectorHold());
        }
        Debug.Log("Hold");
    }
    private void OnGrab()
    {
        RaycastHit hit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        {
            if (grid.tryGetPointIdByPos(hit.point, out int id))
                StartCoroutine(movePoint(hit.point));
        }
    }
    IEnumerator movePoint(Vector3 pos)
    {
        InputAction grab = gridActionAsset.FindActionMap("Grid").FindAction("Grab");
        if (grab != null) {
            int id = -2;
            grid.tryGetPointIdByPos(pos, out id);
            while (grab.ReadValue<float>() > 0f)
            {
                grid.movePointToPos(id, grid.CoordToPos(grid.posToRoundCoord(pointerPositionOnGrid)), false);
                yield return new WaitForSeconds(0.1f);
            }
            //Definitive move to change dictionnaries
            grid.movePointToPos(id, grid.CoordToPos(grid.posToRoundCoord(pointerPositionOnGrid)), true);
        }
    }
    IEnumerator createVectorHold()
    {
        InputAction hold = gridActionAsset.FindActionMap("Grid").FindAction("Hold Select");
         
        int terminalPointID = grid.CreatePoint(pointerPositionOnGrid);
        Vector v = grid.CreateVectorWithTwoPoints(currentPointID, terminalPointID);
        
        
        
        if (hold != null)
        {
            print("Find Select");
            while(hold.ReadValue<float>() > 0f)
            {
                //CHANGER LA VALEUR DE POINTERPOSITIONON GRID DANS CETTE FONCTION POUR CHANGER LES CONDITIONS DE CONGRUENCES
                grid.movePointToPos(terminalPointID, grid.CoordToPos(grid.posToRoundCoord(pointerPositionOnGrid)), false);
                yield return new WaitForSeconds(0.1f);
            }

            grid.movePointToPos(terminalPointID,grid.CoordToPos(grid.posToRoundCoord(pointerPositionOnGrid)), true);
        }
    }
}

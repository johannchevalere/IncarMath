using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    private int state = 0;
    [SerializeField] Grid2 grid;
    [SerializeField] XRRayInteractor rayInteractor;
    [SerializeField] Transform pointer;
    [SerializeField] InputActionAsset gridActionAsset;
    [SerializeField] bool lowCongruence;

    [Header("UI")]

    Vector3 pointerPositionOnGrid;
    Vector3 pointerRoundCoords;
    int currentPointID;

    private void OnEnable()
    {
        StartCoroutine(pointerPosition( 0.05f));
    }
    private void Update()
    {
            }
    IEnumerator pointerPosition(float refreshrate)
    {
        while (true)
        {

            yield return new WaitForSeconds(refreshrate);
            //text.text = grid.NbPoints().ToString();
            if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                pointerPositionOnGrid = hit.point;
                pointer.position = grid.CoordToPos(pointerRoundCoords);
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
                {
                    pointerPositionOnGrid.z = grid.transform.position.z;
                }
                pointerRoundCoords = grid.PosToRoundCoord(pointerPositionOnGrid);
            }
        }
    }
    private void OnSelect()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
            {
                //If we are in the display mode for vector sums, and a different vector than the one we hit was selected, we display the vectorial sum
                int vectorID = grid.GetVectorId(hit.collider.transform.parent.GetComponent<Vector>());
                if (grid.isVectorSelected(out int oldVectorID))
                {
                    if (vectorID != oldVectorID)
                    {
                        grid.VectorSum(oldVectorID, vectorID);
                    }
                }
                grid.SelectVector(vectorID);
                return;
            }

            if (grid.TryGetPointByCoordinates(pointerRoundCoords, out int id))
            {
                grid.SelectPoint(id);
                currentPointID = id;
                //Can create vector if another point was already selected

                return;
            }//if a point exists at current coordinates, we select it and end method
            currentPointID = grid.CreatePoint(pointerRoundCoords); //else we create a point. Storing point id in case new vector is created
            if (grid.TryGetPointByCoordinates(pointerRoundCoords, out int testPointID)) {
            } 
        }
    }
    private void OnHoldSelect()
    {
        //Create Vector by holding trigger
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            StartCoroutine(CreateVectorHold());
        }
    }
    private void OnGrab()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (grid.TryGetPointByCoordinates(pointerRoundCoords, out int id))
            {
                StartCoroutine(MovePoint(id));
                return;
            }
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
            {
                StartCoroutine(MoveVector(hit.collider.transform.parent.GetComponent<Vector>()));
            }
        }

    }
    IEnumerator MovePoint(int pointID)
    {
        InputAction grab = gridActionAsset.FindActionMap("Grid").FindAction("Grab");
        if (grab != null) {
            while (grab.ReadValue<float>() > 0f)
            {
                grid.MovePoint(pointID, pointerRoundCoords,false);
                yield return new WaitForSeconds(0.1f);
            }
            grid.MovePoint(pointID, pointerRoundCoords, true);
        }
    }
    IEnumerator CreateVectorHold()
    {
        InputAction hold = gridActionAsset.FindActionMap("Grid").FindAction("Hold Select");
        int terminalPointID = grid.CreatePoint(pointerRoundCoords, fusePoint : false);
        int v = grid.CreateVector(currentPointID, terminalPointID);
        grid.SelectVector(v);
        Vector3 nextPointCoord = pointerRoundCoords;
        if (hold != null)
        {
            while(hold.ReadValue<float>() > 0f)
            {
                nextPointCoord = pointerRoundCoords;
                //CHANGER LA VALEUR DE POINTERPOSITIONON GRID DANS CETTE FONCTION POUR CHANGER LES CONDITIONS DE CONGRUENCES
                if (lowCongruence)
                {
                    
                }

                grid.MovePoint(terminalPointID, nextPointCoord, false);
                yield return new WaitForSeconds(0.1f);
            }
            
            grid.MovePoint(terminalPointID, nextPointCoord, true);
        }
    }
    IEnumerator MoveVector(Vector vector)
    {
        //Todo: Vector can be placed out of grid then crash
        //TODO: use specific colors designed in grid (Or in vector ?? Maybe do a mathObject ?) for vector when displaced/selected
        //Todo : revisit all this crappy old code
        InputAction grab = gridActionAsset.FindActionMap("Grid").FindAction("Grab");
        Vector3 initialPointerPos = pointerPositionOnGrid;
        vector.transform.Find("Shaft").GetComponent<CapsuleCollider>().enabled = false;

        int vectorID = grid.GetVectorId(vector);
        grid.SelectVector(vectorID);
        Vector3 initialPointOldPos = grid.CoordToPos(vector.initialPoint);
        Vector3 terminalPointOldPos =grid.CoordToPos(vector.terminalPoint);

        if (grab != null) {          
            vector.ChangeColor(Color.green);
            while (grab.ReadValue<float>() > 0f)
            {
                Vector3 deltaPos = pointerPositionOnGrid - initialPointerPos;
                Vector3 newInitialPointPos = grid.PosToRoundCoord(initialPointOldPos + deltaPos) - 0.3f * Vector3.forward;
                Vector3 newTerminalPointPos = grid.PosToRoundCoord(terminalPointOldPos + deltaPos) - 0.3f * Vector3.forward;
                vector.ChangePointPosition(newInitialPointPos, false);
                vector.ChangePointPosition(newTerminalPointPos, true);
                
                
                yield return new WaitForSeconds(0.1f);
            }

            vector.transform.Find("Shaft").GetComponent<CapsuleCollider>().enabled = true;
            vector.ChangeColor(Color.gray);
            //Definitive move and changing dictionnaries
            vector.ChangePointPosition(vector.initialPoint + 0.3f * grid.transform.forward, false);
            vector.ChangePointPosition(vector.terminalPoint + 0.3f * grid.transform.forward, true);
            
            grid.MoveVector(vectorID, vector.initialPoint, vector.terminalPoint);

        }
    }
}

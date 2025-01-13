using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour
{
    private int state = 0;
    [SerializeField] Repere grid;
    [SerializeField] XRRayInteractor rayInteractor;
    [SerializeField] Transform pointer;
    [SerializeField] InputActionAsset gridActionAsset;
    [SerializeField] bool lowCongruence;

    [Header("UI")]
    [SerializeField] TMP_Text text;

    Vector3 pointerPositionOnGrid;
    int currentPointID;


    private void Update()
    {
        text.text = grid.NbPoints().ToString();
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            
            pointerPositionOnGrid = hit.point;
            pointer.position = grid.CoordToPos(grid.PosToRoundCoord(hit.point));
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
            {
                pointerPositionOnGrid.z = grid.transform.position.z;
            }
        }   

        if (!grid.SelectedToString().Equals(""))
        {
            text.text = grid.SelectedToString();
        }
    }
    private void OnSelect()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
            {
                grid.SelectVector(grid.GetVectorId(hit.collider.transform.parent.GetComponent<Vector>()));
                return;
            }

            if (grid.TrySelectPointByPos(hit.point, out int id))
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
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            text.text = "Pointer at " + grid.PosToCoord(hit.point).ToString();
            StartCoroutine(CreateVectorHold());
        }
    }
    private void OnGrab()
    {
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (grid.TryGetPointIdByPos(hit.point, out int id))
            {
                Debug.Log("Moving point with grab");
                StartCoroutine(MovePoint(hit.point));
                return;
            }
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Vector"))
            {
                StartCoroutine(MoveVector(hit.collider.transform.parent.GetComponent<Vector>()));
            }
        }

    }
    IEnumerator MovePoint(Vector3 pos)
    {
        InputAction grab = gridActionAsset.FindActionMap("Grid").FindAction("Grab");
        if (grab != null) {
            grid.TryGetPointIdByPos(pos, out int id);
            while (grab.ReadValue<float>() > 0f)
            {
                grid.MovePointToPos(id, grid.CoordToPos(grid.PosToRoundCoord(pointerPositionOnGrid)), false);
                yield return new WaitForSeconds(0.1f);
            }
            //Definitive move to change dictionnaries
            grid.MovePointToPos(id, grid.CoordToPos(grid.PosToRoundCoord(pointerPositionOnGrid)), true);
        }
    }
    IEnumerator CreateVectorHold()
    {
        InputAction hold = gridActionAsset.FindActionMap("Grid").FindAction("Hold Select");
         
        //There is the issue... terminal point is created on top of initial point and new merge makes it go boom (doesn't even crash
        //Todo Fix this sh... i have no idea how to elegantly solve this. Might need a temporary point
        int terminalPointID = grid.CreateTempPoint(pointerPositionOnGrid);
        grid.CreateVectorWithTwoPoints(currentPointID, terminalPointID);
        if (hold != null)
        {
            while(hold.ReadValue<float>() > 0f)
            {
                Debug.Log("Create vector hold");
                Vector3 nextPointPosition = pointerPositionOnGrid;
                //CHANGER LA VALEUR DE POINTERPOSITIONON GRID DANS CETTE FONCTION POUR CHANGER LES CONDITIONS DE CONGRUENCES
                if (lowCongruence)
                {
                    
                }

                grid.MovePointToPos(terminalPointID, grid.CoordToPos(grid.PosToRoundCoord(nextPointPosition)), false);
                yield return new WaitForSeconds(0.1f);
            }

            grid.MovePointToPos(terminalPointID,grid.CoordToPos(grid.PosToRoundCoord(pointerPositionOnGrid)), true);
        }
    }
    IEnumerator MoveVector(Vector vector)
    {
        //Todo: Vector can be placed out of grid then crash
        //TODO: use specific colors designed in grid (Or in vector ?? Maybe do a mathObject ?) for vector when displaced/selected
        InputAction grab = gridActionAsset.FindActionMap("Grid").FindAction("Grab");
        Vector3 initialPointerPos = pointerPositionOnGrid;
        vector.transform.Find("Shaft").GetComponent<CapsuleCollider>().enabled = false;
        
        Vector3 initialPointOldPos = grid.CoordToPos(vector.initialPoint);
        Vector3 terminalPointOldPos =grid.CoordToPos(vector.terminalPoint);
        if (!grid.TryGetPointIdByPos(grid.CoordToPos(grid.PosToRoundCoord(initialPointOldPos)), out int oldInitialPointID)) {
            Debug.LogWarning("No initialPoint associated with vector on grid");
            yield break;
        }
        if (!grid.TryGetPointIdByPos(grid.CoordToPos(grid.PosToRoundCoord(initialPointOldPos)), out int oldTerminalPointID)) {
            Debug.LogWarning("No terminalPoint associated with vector on grid");
            yield break;
        }


        if (grab != null) {          
            vector.ChangeColor(Color.green);
            while (grab.ReadValue<float>() > 0f)
            {
                Vector3 deltaPos = pointerPositionOnGrid - initialPointerPos;
                text.text = deltaPos.ToString();
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
            //Deleting old points that aren't linked to any vectors
            //Keeping that here for now but will remove it when i'm sure the other version works with 0 problems
            /*
            if (!grid.IsPointLinkedToVectors(oldTerminalPointID)) grid.DeletePoint(oldTerminalPointID);
            if (!grid.IsPointLinkedToVectors(oldInitialPointID)) grid.DeletePoint(oldInitialPointID);

            int initialPointID = grid.CreatePointByCoordinates(vector.initialPoint);
            int terminalPointID = grid.CreatePointByCoordinates(vector.terminalPoint);
            int vectorID = grid.GetVectorId(vector);
            grid.ChangeVectorInitialPoint(vectorID, initialPointID);
            grid.ChangeVectorTerminalPoint(vectorID, terminalPointID);
            */
            int vectorID = grid.GetVectorId(vector);

            grid.VectorMoveWithPointDeletion(vectorID, vector.initialPoint, vector.terminalPoint);

        }
    }
}

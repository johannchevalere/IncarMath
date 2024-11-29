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
            if (grid.trySelectPointByPos(hit.point)) return; //if a point exists at current coordinates, we select it and end method
            grid.CreatePoint(hit.point); //else we create a point
        }
    }
    private void OnHoldSelect()
    {
        //Create Vector (need to use a coroutine)
        RaycastHit hit;
        if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
        text.text = "Pointer at " + grid.posToCoordinates(hit.point).ToString();
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
            Debug.Log("Found It");
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
}

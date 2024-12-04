using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System.Collections;
using NUnit.Framework.Internal;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Linq;
using UnityEngine.Events;
public class Repere : MonoBehaviour
{

    [Header("XR Interaction")]
    public XRRayInteractor rayInteractor;
    public InputAction leftTrigger;
    public InputAction rightTrigger;
    [Header("Grid Settings")]
    public bool is2d = true; //True if the grid is 2 dimensional, False if it is 3 dimensional
    public int width = 5;
    public int height = 5;
    public int depth = 0; //Used in 3d grids
    public int precision = 1; //Might change name, numbers of sublines between two integers 
    public float scale = 0.10f;
    [Header("Prefabs")]
    [SerializeField] private GameObject gridLine;
    [Header("Points")]
    private Dictionary<int, GameObject> points = new Dictionary<int, GameObject>(); //key = button id
    private Dictionary<int, Vector3> pointsCoordinate = new Dictionary<int, Vector3>();
    private Dictionary<Vector3, List<int>> coordinatePoints = new Dictionary<Vector3, List<int>>();
    private bool isPointSelected = false;
    private int selectedPointID = -1;
    [SerializeField] private Color pointBaseColor = Color.gray;
    [SerializeField] private Color pointSelectedColor = Color.yellow;



    [Header("Vectors")]
    private Dictionary<int, Vector> vectors = new Dictionary<int, Vector>();
    private Dictionary<int, int> initialPoints = new Dictionary<int, int>(); //Key = VectorID, Value = InitialPointID
    private Dictionary<int, int> terminalPoints = new Dictionary<int, int>();//Key = VectorID, Value = TerminalPointID
    private bool vectorCreation = false;
    [Header("Events")]
    public UnityEvent GridModification;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateRepere();
        GameObject O = points[CreatePointByCoordinates(Vector3.zero, id: 15)];
        GameObject I = points[CreatePointByCoordinates(Vector3.right, id: 9)];
        GameObject J = points[CreatePointByCoordinates(Vector3.up, id: 10)];
        O.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1);
        O.transform.localScale += 0.05f * scale * Vector3.one;
        I.GetComponent<Renderer>().material.color = Color.grey;
        I.transform.localScale += 0.025f * scale * Vector3.one;
        J.GetComponent<Renderer>().material.color = Color.grey;
        J.transform.localScale += 0.025f * scale * Vector3.one;

        leftTrigger.Enable();
        rightTrigger.Enable();
    }

    void CreateRepere()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(transform);

        plane.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        plane.transform.localScale = new Vector3(scale * (1 + 2 * width) / 10, 1, scale * (1 + 2 * height) / 10);
        plane.transform.localPosition = Vector3.forward * 0.05f;
        plane.layer = 6;
        plane.GetComponent<Renderer>().material.color = Color.white;
        for (int line = -width; line <= width; line++)
        {
            GameObject Line = Instantiate(gridLine, transform, false);
            Line.transform.localScale = new Vector3(Line.transform.localScale.x, scale * height, Line.transform.localScale.z);
            Line.transform.localPosition = new Vector3(scale * (line), 0, 0);
            if (line == 0)
            {
                Line.transform.localScale += 0.025f * scale * new Vector3(1, 0, 1);
                Line.GetComponent<Renderer>().material.color = Color.grey;
            }
        }

        for (int column = -height; column <= height; column++)
        {
            GameObject Line = Instantiate(gridLine, transform, false);
            Line.transform.localScale = new Vector3(Line.transform.localScale.x, scale * width, Line.transform.localScale.z);
            Line.transform.localPosition = new Vector3(0, scale * column, 0);
            Line.transform.localRotation = Quaternion.Euler(0, 0, 90);
            if (column == 0)
            {
                Line.transform.localScale += 0.025f * scale * new Vector3(1, 0, 1);
                Line.GetComponent<Renderer>().material.color = Color.grey;
            }
        }
    }


    //Destroy all points and vectors on the grid and reset the dictionnaries
    public void ClearGrid()
    {
        foreach (int pointID in points.Keys)
        {
            //Si on doit itérer points par points pour faire quelque chose lors de la destruction
            Destroy(points[pointID]);
        }
        foreach (int vectorID in vectors.Keys)
        {
            Destroy(vectors[vectorID].gameObject);
        }
        points = new Dictionary<int, GameObject>(); //key = button id
        pointsCoordinate = new Dictionary<int, Vector3>();
        coordinatePoints = new Dictionary<Vector3, List<int>>();
        vectors = new Dictionary<int, Vector>();
        initialPoints = new Dictionary<int, int>(); //Key = VectorID, Value = InitialPointID
        terminalPoints = new Dictionary<int, int>();//Key = VectorID, Value = TerminalPointID
        GridModification.Invoke();
    }
    //Create a point on the grid and generate the first available id for the point
    //Only int positioning for now, might reconsider for decimal using precision later
    public int CreatePointByCoordinates(Vector3 pointCoord, int id = -1)
    {

        Assert.IsFalse(points.ContainsKey(id));
        Assert.IsTrue(Mathf.Abs(pointCoord.x) <= width && Mathf.Abs(pointCoord.y) <= height && Mathf.Abs(pointCoord.z) <= depth);
        Vector3 pointPos = new Vector3(pointCoord.x * scale, pointCoord.y * scale, pointCoord.z * scale);
        if (id == -1)
        {
            id = 0;
            while (points.ContainsKey(id)) id++;
        }
        GameObject point = MathManager.instance.InstantiatePoint(pointPos, transform, scale);
        points[id] = point;
        pointsCoordinate[id] = pointCoord;
        if (!coordinatePoints.ContainsKey(pointCoord)) coordinatePoints[pointCoord] = new List<int>();
        coordinatePoints[pointCoord].Add(id);
        GridModification.Invoke();
        return id;
    }
    public Vector3 CoordToPos(Vector3 coord)
    {
        return (scale * coord) + transform.position;
    }
    public Vector3 posToCoordinates(Vector3 pos)
    {
        return (pos - transform.position) / scale;
    }
    public Vector3 posToRoundCoord(Vector3 pos)
    {
        Vector3 coords = posToCoordinates(pos);
        return new Vector3(Mathf.Round(coords.x), Mathf.Round(coords.y), Mathf.Round(coords.z));
    }
    private Vector3 coordToRoundCoord(Vector3 coords)
    {
        return new Vector3(Mathf.Round(coords.x), Mathf.Round(coords.y), Mathf.Round(coords.z));
    }
    void DestroyPoint(int id)
    {
        GameObject point = points[id];
        points.Remove(id);
        coordinatePoints[pointsCoordinate[id]].Remove(id);
        pointsCoordinate.Remove(id);
        Destroy(point);
        GridModification.Invoke();
    }
    public int CreatePoint(Vector3 pos)
    {
        return CreatePointByCoordinates(posToRoundCoord(pos));
    }
    public Vector CreateVectorWithTwoPoints(int id1, int id2)
    {
        var offset = new Vector3(0, 0, 0);
        if (is2d)
        {
            offset = Vector3.back * 0.01f;
        }

        GameObject v = MathManager.instance.InstantiateVector(pointsCoordinate[id1] + offset, pointsCoordinate[id2] + offset, transform, scale);
        int id = newVectorId();
        vectors[id] = v.GetComponent<Vector>();
        initialPoints[id] = id1;
        terminalPoints[id] = id2;
        GridModification.Invoke();
        return v.GetComponent<Vector>();
    }

    private int newVectorId()
    {
        int id = 0;
        while (vectors.ContainsKey(id)) id++;
        return id;
    }
    /*IEnumerator CreateVectorWithOnePoint()
    {
        int id = selectedPointID;
        Vector v = CreateVectorWithTwoPoints(id, id);
        isPointSelected = false;
        selectedPointID = -1;
        yield return new WaitForSeconds(0.5f);
        for(;;)
        {
            Vector3 nextVectorPos = (pointerCoordinates());
            nextVectorPos.z = v.transform.localPosition.z;
           
            v.changePointPosition(nextVectorPos);
            if (leftTrigger.ReadValue<float>() > 0f)
            {
                var finalCoords = coordToRoundCoord(pointerCoordinates());
                v.changePointPosition(finalCoords);
                v.IsMoving = false;
                CreatePointByCoordinates(finalCoords);
                break;
            }
            yield return null;
        }
    } */
    //TODO: Needs works for moving the vectors associated to the point etc...
    private void selectPoint(int id)
    {
        if (!isPointSelected)
        {
            isPointSelected = true;
            selectedPointID = id;
            points[id].gameObject.GetComponent<Renderer>().material.color = pointSelectedColor;
            return;
        }
        CreateVectorWithTwoPoints(selectedPointID, id);
        DeselectPoint(selectedPointID);
        GridModification.Invoke();
    }

    public void DeselectAllPoints()
    {
        foreach (int id in points.Keys)
        {
            DeselectPoint(id);
        }
        selectedPointID = -1;
        isPointSelected=false;
    }
    public void DeselectPoint(int id)
    {
        if (selectedPointID == id)
        {
            selectedPointID = -1;
            isPointSelected = false;
        }

        points[id].gameObject.GetComponent<Renderer>().material.color = pointBaseColor;
        
    }
    public bool trySelectPointByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = posToRoundCoord(pos);
        if (coordinatePoints.ContainsKey(roundCoord) && (coordinatePoints[roundCoord].Count > 0))
        {
            id = coordinatePoints[roundCoord][^1];
            selectPoint(id);
            return true;
        }
        id = -1;
        return false;
    }
    public bool tryGetPointIdByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = posToRoundCoord(pos);
        if (!coordinatePoints.ContainsKey(roundCoord) || coordinatePoints[roundCoord].Count == 0)
        {
            id = -1;
            return false;
        }
        else
        {
            id = coordinatePoints[roundCoord][^1];
            return true;
        }
    }
    public void movePointToPos(int pointId, Vector3 pos, bool definitive = true)
    {
        //Move point
        points[pointId].transform.position = pos;
        //Move vectors associated with point
        //Reverse search in dict isn't great, but best solution atm
        foreach (KeyValuePair<int, int> pair in initialPoints)
        {
            if (pair.Value == pointId)
                vectors[pair.Key].changePointPosition((posToRoundCoord(pos)), false);
        }
        foreach (KeyValuePair<int, int> pair in terminalPoints)
        {
            if (pair.Value == pointId)
                vectors[pair.Key].changePointPosition((posToRoundCoord(pos)), true);
        }
        Vector3 roundCoords = posToRoundCoord(pos);
        //If move is definitive then modify point coordinates dicts 
        if (definitive)
        {
            Debug.Log("Definitive move of point");
            Vector3 oldPos = pointsCoordinate[pointId];
            //In case it was a point stacked with other points
            if (coordinatePoints.ContainsKey(oldPos) && coordinatePoints[oldPos].Count > 1)
            {
                coordinatePoints[oldPos].Remove(pointId);
            }
            else
            {
                coordinatePoints.Remove(oldPos);
            }
            if (coordinatePoints.ContainsKey(roundCoords))
            {
                Debug.Log("Point has been moved in top of another point");
                coordinatePoints[roundCoords].Add(pointId);
            }
            else
            {
                coordinatePoints[roundCoords] = new List<int>();
                coordinatePoints[roundCoords].Add(pointId);
            }//If we land on another point CHANGE HERE IF WE MERGE POINTS ON THE SAME PLACE
            pointsCoordinate[pointId] = roundCoords;
            GridModification.Invoke();
        }
    }


    //Functions to check the instructions

    public bool isThereNPoints(int n)
    {

        return points.Keys.Count >= n;
    }

    public bool isThereNVectors(int n)
    {
        return vectors.Keys.Count >= n;
    }

    public bool isTherePointAtCoordinates(Vector3 coords)
    {
        return coordinatePoints.ContainsKey(coords);
    }
}




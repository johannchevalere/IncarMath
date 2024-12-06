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
    private Dictionary<int, GameObject> points = new (); //key = button id
    private Dictionary<int, Vector3> pointsCoordinate = new ();
    private Dictionary<Vector3, List<int>> coordinatePoints = new ();
    private bool isPointSelected = false;
    private int selectedPointID = -1;
    [SerializeField] private Color pointBaseColor = Color.gray;
    [SerializeField] private Color pointSelectedColor = Color.yellow;



    [Header("Vectors")]
    private Dictionary<int, Vector> vectors = new ();
    private Dictionary<int, int> initialPoints = new (); //Key = VectorID, Value = InitialPointID
    private Dictionary<int, int> terminalPoints = new ();//Key = VectorID, Value = TerminalPointID
    private bool isVectorSelected = false;
    private int selectedVectorID = -1;
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
            Line.transform.SetLocalPositionAndRotation(new Vector3(0, scale * column, 0), Quaternion.Euler(0, 0, 90));
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
    public Vector3 PosToCoord(Vector3 pos)
    {
        return (pos - transform.position) / scale;
    }
    public Vector3 PosToRoundCoord(Vector3 pos)
    {
        Vector3 coords = PosToCoord(pos);
        return new Vector3(Mathf.Round(coords.x), Mathf.Round(coords.y), Mathf.Round(coords.z));
    }
    private Vector3 CoordToRoundCoord(Vector3 coords)
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
        return CreatePointByCoordinates(PosToRoundCoord(pos));
    }
    public Vector CreateVectorWithTwoPoints(int id1, int id2)
    {
        var offset = new Vector3(0, 0, 0);
        if (is2d)
        {
            offset = Vector3.back * 0.01f;
        }

        GameObject v = MathManager.instance.InstantiateVector(pointsCoordinate[id1] + offset, pointsCoordinate[id2] + offset, transform, scale);
        int id = NewVectorId();
        vectors[id] = v.GetComponent<Vector>();
        initialPoints[id] = id1;
        terminalPoints[id] = id2;
        GridModification.Invoke();
        return v.GetComponent<Vector>();
    }

    private int NewVectorId()
    {
        int id = 0;
        while (vectors.ContainsKey(id)) id++;
        return id;
    }
    private void SelectPoint(int id)
    {
        if (!isPointSelected)
        {
            isPointSelected = true;
            selectedPointID = id;
            points[id].GetComponent<Renderer>().material.color = pointSelectedColor;
            return;
        }
        CreateVectorWithTwoPoints(selectedPointID, id);
        DeselectPoint(selectedPointID);
        GridModification.Invoke();
    }

    public void SelectVector(int id)
    {
        DeselectAllPoints();

    }
    public string SelectedToString()
    {
        if (isPointSelected)
        {
            return "Point selected : ";
        }
        if (isVectorSelected)
        {
            return "Vector selected : ";
        }
        return "";

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

        points[id].GetComponent<Renderer>().material.color = pointBaseColor;
        
    }
    public bool TrySelectPointByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = PosToRoundCoord(pos);
        if (coordinatePoints.ContainsKey(roundCoord) && (coordinatePoints[roundCoord].Count > 0))
        {
            id = coordinatePoints[roundCoord][^1];
            SelectPoint(id);
            return true;
        }
        id = -1;
        return false;
    }
    public bool TryGetPointIdByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = PosToRoundCoord(pos);
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
    public int GetVectorId(Vector vector)
    {
        foreach (KeyValuePair<int, Vector> pair in vectors)
        {
            if (pair.Value.Equals(vector)) return pair.Key;
        }
        Debug.LogWarning("could not find vector in vectors list on the grid");
        return -1;
    }

    public void ChangeVectorInitialPoint(int vectorID, int newInitialPointID)
    {
        Assert.IsTrue(vectors.ContainsKey(vectorID));
        Assert.IsTrue(points.ContainsKey(newInitialPointID));

        initialPoints[vectorID] = newInitialPointID;
    }
    public void ChangeVectorTerminalPoint(int vectorID, int newTerminalPointID)
    {
        Assert.IsTrue(vectors.ContainsKey(vectorID));
        Assert.IsTrue(points.ContainsKey(newTerminalPointID));

        terminalPoints[vectorID] = newTerminalPointID;
    }
    public void MovePointToPos(int pointId, Vector3 pos, bool definitive = true)
    {

        if (!points.ContainsKey(pointId)) { return; }
        //Move point
        points[pointId].transform.position = pos;

        //Move vectors associated with point
        //Reverse search in dict isn't great, but best solution atm
        foreach (KeyValuePair<int, int> pair in initialPoints)
        {
            if (pair.Value == pointId)
                vectors[pair.Key].ChangePointPosition((PosToRoundCoord(pos)), false);
        }
        foreach (KeyValuePair<int, int> pair in terminalPoints)
        {
            if (pair.Value == pointId)
                vectors[pair.Key].ChangePointPosition((PosToRoundCoord(pos)), true);
        }
        Vector3 roundCoords = PosToRoundCoord(pos);
        //If move is definitive then modify point coordinates dicts 
        if (definitive)
        {
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
                coordinatePoints[roundCoords] = new (pointId);
            }//If we land on another point CHANGE HERE IF WE MERGE POINTS ON THE SAME PLACE
            pointsCoordinate[pointId] = roundCoords;
            GridModification.Invoke();
        }
    }


    //Functions to check the instructions

    public bool IsThereNPoints(int n)
    {

        return points.Keys.Count >= n;
    }

    public bool IsThereNVectors(int n)
    {
        return vectors.Keys.Count >= n;
    }

    public bool IsTherePointAtCoordinates(Vector3 coords)
    {
        return coordinatePoints.ContainsKey(coords);
    }
}




using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using System.Collections;
using NUnit.Framework.Internal;
using UnityEngine.Rendering;
public class Repere : MonoBehaviour
{
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



    [Header("Vectors")]
    private Dictionary<int, Vector> vectors = new Dictionary<int, Vector>();
    private Dictionary<int, int> initialPoints = new Dictionary<int, int>(); //Key = VectorID, Value = InitialPointID
    private Dictionary<int, int> terminalPoints = new Dictionary<int, int>();//Key = VectorID, Value = TerminalPointID
    private bool vectorCreation = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateRepere();
        GameObject O = CreatePointByCoordinates(Vector3.zero, id: 15);
        GameObject I = CreatePointByCoordinates(Vector3.right, id: 9);
        GameObject J = CreatePointByCoordinates(Vector3.up, id: 10);
        O.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1);
        O.transform.localScale += 0.05f * scale * Vector3.one;
        I.GetComponent<Renderer>().material.color = Color.grey;
        I.transform.localScale += 0.025f * scale * Vector3.one;
        J.GetComponent<Renderer>().material.color = Color.grey;
        J.transform.localScale += 0.025f * scale * Vector3.one;
    }

    void CreateRepere()
    {
        for (int line = -width; line <= width; line++)
        {
            GameObject Line = Instantiate(gridLine, transform, false);
            Line.transform.localScale = new Vector3(Line.transform.localScale.x, scale * height, Line.transform.localScale.z);
            Line.transform.localPosition = new Vector3(scale * (line), 0, 0);
            if (line == 0) {
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
    //Create a point on the grid and generate the first available id for the point
    //Only int positioning for now, might reconsider for decimal using precision later
    public GameObject CreatePointByCoordinates(Vector3 pointCoord, int id = -1)
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
        return point;
    }
    private Vector3 CoordToPos(Vector3 coord)
    {
        return (scale * coord) + transform.position;
    }
    private Vector3 posToCoordinates(Vector3 pos)
    {
        return (pos - transform.position) / scale;
    }
    private Vector3 posToRoundCoord(Vector3 pos)
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
    }
    void Update()
    {
        var mouse = Mouse.current;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, transform.position.z));
            mousePos.z = transform.position.z;
            Debug.Log(posToCoordinates(mousePos));
            Debug.Log(posToRoundCoord(mousePos));
            if (coordinatePoints.ContainsKey(posToRoundCoord(mousePos)) && coordinatePoints[posToRoundCoord(mousePos)].Count > 0)
            {
                selectPoint(coordinatePoints[posToRoundCoord(mousePos)][^1]);
            }
        }
        if (mouse.rightButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, transform.position.z));
            if(coordinatePoints.ContainsKey(posToRoundCoord(mousePos)))
            {
                selectPoint(coordinatePoints[posToRoundCoord(mousePos)][^1]);
                StartCoroutine("movePointToPointer");
            }
            mousePos.z = transform.position.z;
            CreatePoint(mousePos);
        }
    }

    public void CreatePoint(Vector3 pos)
    {
        CreatePointByCoordinates(posToRoundCoord(pos));
    }
    public Vector CreateVectorWithTwoPoints(int id1, int id2)
    {
        var offset = new Vector3(0, 0, 0);
        if (is2d)
        {
            offset = Vector3.back * 0.01f;
        }

        GameObject v = MathManager.instance.InstantiateVector(pointsCoordinate[id1] + offset, pointsCoordinate[id2] + offset, transform, scale);
        return v.GetComponent<Vector>();
    }
    
    IEnumerator CreateVectorWithOnePoint()
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
            if (Mouse.current.leftButton.isPressed)
            {
                var finalCoords = coordToRoundCoord(pointerCoordinates());
                v.changePointPosition(finalCoords);
                v.IsMoving = false;
                CreatePointByCoordinates(finalCoords);
                break;
            }
            yield return null;
        }
    }
    //TODO: Needs works for moving the vectors associated to the point etc...
    IEnumerator movePointToPointer()
    {
        Debug.Log(selectedPointID);
                for (; ; )
        {
            GameObject pointToMove = points[selectedPointID];
            pointToMove.transform.position = CoordToPos(coordToRoundCoord(pointerCoordinates()));
            yield return null;
        }
    }
    private void selectPoint(int id) { 
        if (!isPointSelected) { 
            isPointSelected = true; 
            selectedPointID = id; 
            points[id].gameObject.GetComponent<Renderer>().material.color = Color.yellow * 3; 
            return; 
        }

        if (id == selectedPointID)
        {            
            StartCoroutine(CreateVectorWithOnePoint());
            return;
        }
        isPointSelected = false; 
        CreateVectorWithTwoPoints(selectedPointID, id); 
        points[selectedPointID].gameObject.GetComponent<Renderer>().material.color = Color.white; 
        selectedPointID = -1; 
    }


    private Vector3 pointerCoordinates()
    {
        var mouse = Mouse.current;
        Vector3 pointerCoordinates = new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, transform.position.z);
        pointerCoordinates = Camera.main.ScreenToWorldPoint(pointerCoordinates);
        return posToCoordinates(pointerCoordinates);
    }
}




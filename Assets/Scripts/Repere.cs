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
using UnityEditor.Experimental.GraphView;
public class Repere : MonoBehaviour
{
    struct GridPoint
    {
        public Vector3 coordinates;
        public GameObject pointObject;
        public int id;
        public GridPoint(Vector3 coordinates, GameObject pointObject, int id = -1)
        {
            this.coordinates = coordinates;
            this.pointObject = pointObject;
            this.id = id;
        }
    }


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
    private List<GridPoint> points = new();
    private List<bool> idExist = new();
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
        CreatePointByCoordinates(Vector3.down);
        GameObject O = GetGridPointByID(CreatePointByCoordinates(Vector3.zero, id: 14)).pointObject;
        GameObject I = GetGridPointByID(CreatePointByCoordinates(Vector3.right, id: 8)).pointObject;
        GameObject J = GetGridPointByID(CreatePointByCoordinates(Vector3.up, id: 9)).pointObject;
        O.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f, 1);
        O.transform.localScale += 0.05f * scale * Vector3.one;
        I.GetComponent<Renderer>().material.color = Color.grey;
        I.transform.localScale += 0.025f * scale * Vector3.one;
        J.GetComponent<Renderer>().material.color = Color.grey;
        J.transform.localScale += 0.025f * scale * Vector3.one;

        CreateVectorWithTwoPoints(14, 8);
        CreateVectorWithTwoPoints(9, 8);
        SelectPoint(14);
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
                Line.transform.localScale += 0.025f * scale * new Vector3(1, 10, 1);
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
                Line.transform.localScale += 0.025f * scale * new Vector3(1, 10, 1);
                Line.GetComponent<Renderer>().material.color = Color.grey;
            }
        }
    }
    //Destroy all points and vectors on the grid and reset the dictionnaries
    public void ClearGrid()
    {
        foreach(GridPoint point in existingPoints())
        {
            //Si on doit itérer points par points pour faire quelque chose lors de la destruction
            Destroy(point.pointObject);
        }
        foreach (int vectorID in vectors.Keys)
        {
            Destroy(vectors[vectorID].gameObject);
        }
        points = new List<GridPoint>(); //key = button id
        idExist = new List<bool>();
        vectors = new Dictionary<int, Vector>();
        initialPoints = new Dictionary<int, int>(); //Key = VectorID, Value = InitialPointID
        terminalPoints = new Dictionary<int, int>();//Key = VectorID, Value = TerminalPointID
        DeselectAllObjects();
        GridModification.Invoke();
    }
    private GridPoint getGridPointByID(int id)
    {
        Assert.IsTrue(id >= 0 && id < points.Count);
        Assert.IsTrue(idExist[id]);
        return points[id];
    }
    //Create a point on the grid and generate the first available id for the point
    //Only int positioning for now, might reconsider for decimal using precision later
    public int CreatePointByCoordinates(Vector3 pointCoord, int id = -1)
    {

        if (id != -1 && id < idExist.Count)
        {
            Assert.IsTrue(!idExist[id]);
        }
        Assert.IsTrue(Mathf.Abs(pointCoord.x) <= width && Mathf.Abs(pointCoord.y) <= height && Mathf.Abs(pointCoord.z) <= depth);
        Vector3 pointPos = new Vector3(pointCoord.x * scale, pointCoord.y * scale, pointCoord.z * scale);
        if (id == -1)
        {
            id = 0;
            while (id < idExist.Count() && idExist[id]) id++;
        }
        if (!pointExistAtCoordinates(pointCoord, out int existingPointID)) {
            while (idExist.Count <= id)
            {
                idExist.Add(false);
            }
            idExist[id] = true;
            while (points.Count <= id)
            {
                points.Add(new GridPoint());
            }
            GameObject point = MathManager.instance.InstantiatePoint(pointPos, transform, scale);
            points[id] = new GridPoint(pointCoord, point, id);
            point.GetComponent<Point>().setName(PointIDToString(id).ToString());
        }
        else
        {
            id = existingPointID;
        }
        GridModification.Invoke();
        return id;
    }

    bool pointExistAtCoordinates(Vector3 coords, out int id)
    {
        for (int pointID = 0; pointID < idExist.Count; pointID++)
        {
            if (!idExist[pointID])
            {
                continue;
            }
            GridPoint point = GetGridPointByID(pointID);
            if (point.coordinates.Equals(coords))
            {
                id = point.id;
                return true;
            }
        }
        id = -1;
        return false;
    }
    //Used to create vectors with two points, shouldn't be used to create points in general. Use CreatePointByCoordinates instead
    private int CreateTempPointByCoodinates(Vector3 pointCoord, int id = -1)
    {
        if (id != -1 || id < idExist.Count)
        {
            Assert.IsTrue(!idExist[id]);
        }
        Assert.IsTrue(Mathf.Abs(pointCoord.x) <= width && Mathf.Abs(pointCoord.y) <= height && Mathf.Abs(pointCoord.z) <= depth);
        Vector3 pointPos = new Vector3(pointCoord.x * scale, pointCoord.y * scale, pointCoord.z * scale);
        if (id == -1)
        {
            id = 0;
            while (id <= idExist.Count && idExist[id] ) id++;
        }
        while (idExist.Count <= id)
        {
            idExist.Add(false);
        }
        idExist[id] = true;

        GameObject point = MathManager.instance.InstantiatePoint(pointPos, transform, scale);
        points.Add(new GridPoint(pointCoord, point, id));

        point.GetComponent<Point>().setName(PointIDToString(id).ToString());
        return id;
    }
    private void RemoveVectorPoints(int vectorID)
    {
        initialPoints[vectorID] = -1;
        terminalPoints[vectorID] = -1;
    }
    private bool IsPointLinkedToVectors(int id)
    {
        return initialPoints.ContainsValue(id) || terminalPoints.ContainsValue(id);
    }
    private GridPoint GetGridPointByID(int id)
    {
        Assert.IsTrue(id < idExist.Count && idExist[id]);
        foreach (GridPoint point in points)
        {
            if (point.id == id) return point;
        }
        Debug.LogError("Trying to get point while ID doesn't exist");
        return new GridPoint();
    }

    public void VectorMoveWithPointDeletion(int vectorID, Vector3 newInitialPointCoord, Vector3 newTerminalPointCoord)
    {
        GridPoint oldInitialPoint = GetGridPointByID(initialPoints[vectorID]);
        GridPoint oldTerminalPoint = GetGridPointByID(terminalPoints[vectorID]);
        int newInitialPointID;
        int newTerminalPointID;
        RemoveVectorPoints(vectorID);
        if (!(IsPointLinkedToVectors(oldInitialPoint.id) || oldInitialPoint.coordinates == newTerminalPointCoord || oldInitialPoint.coordinates == newInitialPointCoord)) {
            MovePointToPos(oldInitialPoint.id, CoordToPos(newInitialPointCoord));
            newInitialPointID = oldInitialPoint.id;
        }
        else
        {
            newInitialPointID = CreatePointByCoordinates(newInitialPointCoord);

        }
        if (!(IsPointLinkedToVectors(oldTerminalPoint.id) || oldTerminalPoint.coordinates == newInitialPointCoord || oldTerminalPoint.coordinates == newTerminalPointCoord))
        {
            MovePointToPos(oldTerminalPoint.id, CoordToPos(newTerminalPointCoord));
            newTerminalPointID = oldTerminalPoint.id;
        }
        else
        {
        newTerminalPointID = CreatePointByCoordinates(newTerminalPointCoord);
        }
        ChangeVectorInitialPoint(vectorID, newInitialPointID);
        ChangeVectorTerminalPoint(vectorID, newTerminalPointID);
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
    public void DeletePoint(int id)
    {

        if (isPointSelected && selectedPointID == id)
        {
            isPointSelected = false;
            selectedPointID = -1;
        }
        GridPoint point = getGridPointByID(id);
        points.Remove(point);
        Destroy(point.pointObject);
        idExist[id] = false;
        GridModification.Invoke();
    }
    public int CreatePoint(Vector3 pos)
    {
        return CreatePointByCoordinates(PosToRoundCoord(pos));
    }

    //Todo : make a public function CreateVectorFromPoint(id, pos) and private CreateTempPoint
    //Should only be used to create vectors
    public int CreateTempPoint(Vector3 pos)
    {
        return CreateTempPointByCoodinates(PosToRoundCoord(pos));
    }
    public Vector CreateVectorWithTwoPoints(int id1, int id2)
    {
        var offset = new Vector3(0, 0, 0);
        if (is2d)
        {
            offset = Vector3.back * 0.01f;
        }

        GameObject v = MathManager.instance.InstantiateVector(GetGridPointByID(id1).coordinates + offset, GetGridPointByID(id2).coordinates + offset, transform, scale);
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

    //Might private this
    public string PointIDToString(int id)
    {
        char c =  (char)(id);
        c += 'A';
        return c.ToString();
    }
    private void SelectPoint(int id)
    {
        if (!isPointSelected)
        {
            DeselectAllObjects();

            isPointSelected = true;
            selectedPointID = id;
            getGridPointByID(id).pointObject.GetComponent<Renderer>().material.color = pointSelectedColor;
            return;
        }
        CreateVectorWithTwoPoints(selectedPointID, id);
        DeselectAllObjects();
        GridModification.Invoke();
    }
    public void SelectVector(int id)
    {
        DeselectAllObjects();
        vectors[id].ChangeColor(pointSelectedColor);
        selectedVectorID = id;
        isVectorSelected = true;
    }
    public string SelectedToString()
    {
        if (isPointSelected)
            return PointIDToString(selectedPointID) + " " + PointToString(selectedPointID);
        if (isVectorSelected)
            return PointIDToString(initialPoints[selectedVectorID]) + PointIDToString(terminalPoints[selectedVectorID]) + " " + VectorToString(selectedVectorID);
        return "";
    }
    public string PointToString(int id)
    {
        return string.Format("({0}; {1})", getGridPointByID(id).coordinates.x, getGridPointByID(id).coordinates.y);
    }
    public string VectorToString(int id)
    {
        Vector3 vectorCoordinates = VectorCoordinates(id);
        return string.Format("({0}; {1})", (int) vectorCoordinates.x, (int) vectorCoordinates.y);
    }
    private Vector3 VectorCoordinates(int vectorID)
    {
        return getGridPointByID(terminalPoints[vectorID]).coordinates - getGridPointByID(initialPoints[vectorID]).coordinates;
    }
    public void DeselectAllObjects()
    {
        DeselectAllPoints();
        DeselectAllVectors();
    }

    private GridPoint[] existingPoints()
    {
        int count = 0;
        for (int id = 0; id < idExist.Count; id++)
        {
            if (idExist[id]) count++;
        }
        GridPoint[] gridPoints = new GridPoint[count];
        count = 0;
        for (int id = 0;id < idExist.Count; id++)
        {
            if (idExist[id])
            {
                Debug.Log(id);
                gridPoints[count] = getGridPointByID(id);
                count++;
            }
        }
        return gridPoints;

    }
    public void DeselectAllPoints()
    {
        foreach (GridPoint point in existingPoints())
        {
            DeselectPoint(point.id);
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

        getGridPointByID(id).pointObject.GetComponent<Renderer>().material.color = pointBaseColor;
        
    }
    void DeselectAllVectors()
    {
        foreach (int id in vectors.Keys)
        {
            DeselectVector(id);
        }
        selectedVectorID = -1;
        isVectorSelected=false;
    }
    void DeselectVector(int id)
    {
        if (selectedVectorID == id)
        {
            selectedVectorID = -1;
            isVectorSelected = false;
        }

        vectors[id].ChangeColor(pointBaseColor);
    }
    public bool TrySelectPointByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = PosToRoundCoord(pos);
        if (pointExistAtCoordinates(roundCoord, out id))
        {
            SelectPoint(id);
            return true;
        }
        DeselectAllPoints();
        id = -1;
        return false;
    }
    public bool TryGetPointIdByPos(Vector3 pos, out int id)
    {
        Vector3 roundCoord = PosToRoundCoord(pos);
        if (pointExistAtCoordinates(roundCoord, out id))
        {
            return true;
        }
        id = -1;
        return false;
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
        Assert.IsTrue(idExist[newInitialPointID]);
        initialPoints[vectorID] = newInitialPointID;
    }
    public void ChangeVectorTerminalPoint(int vectorID, int newTerminalPointID)
    {
        Assert.IsTrue(vectors.ContainsKey(vectorID));
        Assert.IsTrue(idExist[newTerminalPointID]);
        terminalPoints[vectorID] = newTerminalPointID;
    }
    public void MovePointToPos(int pointID, Vector3 pos, bool definitive = true)
    {
        if (!idExist[pointID]) { return; }

        GridPoint point = getGridPointByID(pointID);
        //Move point
        point.pointObject.transform.position = pos;

        //Move vectors associated with point
        //Reverse search in dict isn't great, but best solution atm
        foreach (KeyValuePair<int, int> pair in initialPoints)
        {
            if (pair.Value == pointID)
                vectors[pair.Key].ChangePointPosition((PosToRoundCoord(pos)), false);
        }
        foreach (KeyValuePair<int, int> pair in terminalPoints)
        {
            if (pair.Value == pointID)
                vectors[pair.Key].ChangePointPosition((PosToRoundCoord(pos)), true);
        }
        Vector3 roundCoords = PosToRoundCoord(pos);
        //Todo: would love to add this, but there is a huge problem when using this. It fixes everything for the selection issue but the logic is destroyed

        //If move is definitive then fuse point with points stacked on it 
        if (definitive)
        {
            //In case it was a point stacked with other points
            if (pointExistAtCoordinates(roundCoords, out int stackedPointID))
            {
                Debug.Log("Point has been moved in top of another point");
                FusePoints(point.id, stackedPointID);
            }
            GridModification.Invoke();
        }
        point.coordinates = roundCoords;
    }
    
    //id1 is the point that stays, id2 get fused into id1
    private void FusePoints(int id1, int id2)
    {
        if (id1 == id2) return;

        foreach (int vectorID in vectors.Keys)
        {
            if (initialPoints[vectorID] == id2)
            {
                initialPoints[vectorID] = id1;
            }
            if (terminalPoints[vectorID] == id2)
            {
                terminalPoints[vectorID] = id1;
            }
        }
        if (selectedPointID == id2) selectedPointID = id1;
        Assert.IsFalse(IsPointLinkedToVectors(id2));
        DeletePoint(id2);
    }
    //Functions to check the instructions
    public int NbPoints()
    {
        return points.Count;
    }
    public int NbVectors()
    {
        return vectors.Keys.Count;
    }
        public int NbVectorDirection(Vector3 direction)
    {
        int nb = 0;
        foreach (int vectorID in vectors.Keys)
        {
            int initialPointID = initialPoints[vectorID];
            int terminalPointID = terminalPoints[vectorID];
            Vector3 vDirection = getGridPointByID(terminalPointID).coordinates - getGridPointByID(initialPointID).coordinates;
            Vector3 xProduct = Vector3.Cross(vDirection, direction);
            if (xProduct.x <= Vector3.kEpsilon && xProduct.y <= Vector3.kEpsilon && xProduct.z <= Vector3.kEpsilon)
            {
                nb++;
            }
        }
        return nb;
    }
    public int NbVectorMagnitude(float magnitude)
    {
        int nb = 0;
        foreach (int vectorID in vectors.Keys)
        {
            int initialPointID = initialPoints[vectorID];
            int terminalPointID = terminalPoints[vectorID];
            float vMagnitude = Vector3.Magnitude(getGridPointByID(terminalPointID).coordinates - getGridPointByID(initialPointID).coordinates);
            if (Mathf.Abs(vMagnitude - magnitude) <= Vector3.kEpsilon)
            {
                
                nb++;
            }  
        }
        return nb;

    }
    public bool pointAtCoordinates(Vector3 coords)
    {
        return pointExistAtCoordinates(coords, out int id);
    }
    public bool vectorAtCoordinates(Vector3 initialPointCoords, Vector3 terminalPointCoords)
    {
        foreach (int vectorID in vectors.Keys)
        {
            if (getGridPointByID(initialPoints[vectorID]).coordinates == initialPointCoords && getGridPointByID(terminalPoints[vectorID]).coordinates == terminalPointCoords)
            {
                return true;
            }
        }
        return false;
    }

    public string GridContentToString()
    {
        string s = "Points :\n";
        for (int pointID = 0; pointID < idExist.Count; pointID++)
        {
            if (!idExist[pointID])
            {
                continue;
            }
            if (isPointSelected && selectedPointID == pointID)
            {
                s += "<color=#" + ColorUtility.ToHtmlStringRGB(pointSelectedColor) + ">";
            }
            s +="\t" + PointIDToString(pointID) + " : " + PointToString(pointID) + "\n";
            s += "</color>";
        }
        s += "\nVecteurs : \n";
        foreach (int vectorID in vectors.Keys )
        {
            if (isVectorSelected && selectedVectorID == vectorID)
            {
                s += "<color=#" + ColorUtility.ToHtmlStringRGB(pointSelectedColor) + ">";
            }
            s += "\t" + VectorName(vectorID) + " : " + VectorToString(vectorID) + "\n";
            s += "</color>";
        }
        return s;
    }

    public string VectorName(int vectorID)
    {
        //Todo: use vector arrow with character sprite on TMPro after making each and every sprite
        return PointIDToString(initialPoints[vectorID]) + PointIDToString(terminalPoints[vectorID]);
    }
}


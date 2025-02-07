using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Grid2 : MonoBehaviour
{
    [Header("Grid parameters")]
    public int width = 5;
    public int height = 5;
    public int depth =0;
    public float scale = 0.1f;
    [SerializeField] private GameObject gridLine;

    [Header("Points")]
    private Dictionary<int, GridPoint> points = new Dictionary<int,GridPoint>();
    private List<bool> isNameIDExisting = new List<bool>();
    int pointNextID = 0;

    [Header("Vectors")]
    private Dictionary<int, GridVector> vectors = new Dictionary<int, GridVector>();
    int vectorNextID = 0;
    private struct GridPoint
    {
        public Vector3 coordinates;
        public readonly int nameID;
        public readonly int id;
        public GameObject pointObject;

        public GridPoint(Vector3 coord, int nameID, int id, GameObject pointObject)
        {
            this.coordinates = coord;
            this.nameID = nameID;
            this.id = id;
            this.pointObject = pointObject;
        }
    }
    private struct GridVector
    {
        public GridPoint initialPoint;
        public GridPoint terminalPoint;
        public Vector vector;
        public readonly int id;

        public GridVector(int id, GridPoint initialPoint, GridPoint terminalPoint, Vector vector)
        {
            this.id = id;
            this.initialPoint = initialPoint;
            this.terminalPoint = terminalPoint;
            this.vector = vector;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateGrid();
        int id0 = CreatePoint(Vector3.zero);
        int id1 = CreatePoint(Vector3.up + Vector3.right);
        CreateVector(id0, id1);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void CreateGrid()
    {
        gameObject.transform.localScale = new Vector3(scale, scale, scale);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(transform);

        plane.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        plane.transform.localScale = new Vector3((1 + 2 * width) / 10f, 1, (1 + 2 * height) / 10f);
        plane.transform.localPosition = Vector3.forward * 0.05f;
        plane.layer = 6;
        plane.GetComponent<Renderer>().material.color = Color.white;
        for (int line = -width; line <= width; line++)
        {
            GameObject Line = Instantiate(gridLine,transform,false);
            Line.transform.SetLocalPositionAndRotation(new Vector3(line, 0, 0), Quaternion.identity);
            Line.transform.localScale = new Vector3(Line.transform.localScale.x, Line.transform.localScale.y * 2 * height, Line.transform.localScale.z);
            if (line == 0)
            {
                Line.GetComponent<Renderer>().material.color = Color.grey;
                float middleScaleFactor = 1 + 1f / (3f * height);
                Line.transform.localScale = new Vector3(Line.transform.localScale.x * 2f, Line.transform.localScale.y * middleScaleFactor , Line.transform.localScale.z * 2f); 
            }
        }

        for (int column = -height; column <= height; column++)
        {
            GameObject Line = Instantiate(gridLine, transform, false);
            Line.transform.SetLocalPositionAndRotation(new Vector3(0, column, 0), Quaternion.Euler(0, 0, 90));
            Line.transform.localScale = new Vector3( Line.transform.localScale.x, Line.transform.localScale.y * 2 * width, Line.transform.localScale.z);
            if (column == 0)
            {
                Line.GetComponent<Renderer>().material.color = Color.grey;
                float middleScaleFactor = 1 + 1f / (3*width);
                Line.transform.localScale = new Vector3(Line.transform.localScale.x * 1.5f, Line.transform.localScale.y * middleScaleFactor, Line.transform.localScale.z * 1.5f);
            }
        }
    }
    private GridPoint getGridPoint(int id)
    {
        if (!points.ContainsKey(id))
        {
            Debug.LogError(string.Format("ERROR::POINT::DOESNT::EXIST id = {0}", id));
            Assert.Fail();
        }
        return points[id];
    }
    private bool TryGetPointByCoordinates(Vector3 coordinates, out int existingPointID)
    {
        foreach (GridPoint point in points.Values)
        {
            if (Vector3.Equals(point.coordinates, coordinates))
            {
                existingPointID = point.id;
                return true;
            }
        }
        existingPointID= -1;
        return false;
    }
    public int CreatePoint(Vector3 coordinates, int nameID = -1, bool fusePoint = true)
    {
        //Todo: Assert coordinates are in bound
        
        //If we don't want stacking points
        if (fusePoint)
        {
            //Searching for existing point at coordinates
            if (TryGetPointByCoordinates(coordinates, out int pointToBeFusedID))
            {
                //Don't create new point at same place, just return id of existing point
                return pointToBeFusedID;
            }
        }
        //Creation of a new point
        GameObject point = MathManager.instance.InstantiatePoint(coordinates, transform);
        int newID = pointNextID;
        pointNextID++;
        int pointNameID = GenerateNewNameID(nameID);
        points[newID] = new GridPoint(coordinates, pointNameID, newID, point);

        //return new point id
        return newID;
    }

    public void MovePoint(int id, Vector3 coordinates, bool fusePoint = true)
    {
        //Todo: Assert coordinates are in grid Space
        GridPoint point = getGridPoint(id);
        //Fuse point if asked to do it and point already exist in coordinates
        if (fusePoint && TryGetPointByCoordinates(coordinates, out int existingPointID)) 
        {
            FusePoint(id, existingPointID);
            //Might need to return here if Fusing destroy the moved point
        }

        //Updating point coordinates
        point.coordinates = coordinates;
        point.pointObject.transform.localPosition = coordinates;
        
        //Todo: Move vectors associated with point

        return;
    }
    public void DeletePoint(int id)
    {
        GridPoint point = getGridPoint(id);

        //Deleting Vectors linked to point
        foreach(GridVector vector in VectorsWithInitialPoint(id))
        {
            DeleteVector(vector.id);
        }
        foreach(GridVector vector in VectorsWithTerminalPoint(id))
        {
            DeleteVector(vector.id);
        }
        isNameIDExisting[point.nameID] = false;
        points.Remove(id);
        Destroy(point.pointObject);
        return;
    }
    private void DeletePoint(GridPoint gridPoint)
    {
        DeletePoint(gridPoint.id);
    }
    public void DeleteVector(int id)
    {
        //Todo implement this
    }

    public Vector3 AboslutePositionToCoordinates(Vector3 position)
    {
        Vector3 coordinates = Vector3.zero;
        coordinates.x = (position.x - transform.position.x) / transform.localScale.x;
        coordinates.y = (position.y - transform.position.y) / transform.localScale.y;
        coordinates.z = (position.z - transform.position.z) / transform.localScale.z;
        return coordinates;
    }

    //Check if name is available and update nameIDs list (if nameID = -1, generate the lowest available nameID) 
    private int GenerateNewNameID(int nameID = -1)
    {
        Assert.IsTrue(nameID >= -1);
        //Generating first available point name (default behaviour)
        if (nameID == -1)
        {
            for (int i = 0; i < isNameIDExisting.Count; i++)
            {
                if (!isNameIDExisting[i])
                {
                    return i;
                }
            }
            isNameIDExisting.Add(true);
            return isNameIDExisting.Count - 1;
        }
        //Generating name specified by the user
        //if nameID is greater than all pre existing nameIDs generated
        while (isNameIDExisting.Count <= nameID)
        {
            isNameIDExisting.Add(false);
        }
        //if name is already taken
        if (isNameIDExisting[nameID])
        {
            Debug.LogWarning("Generating a point whose name is already taken. Generating another name instead");
            return GenerateNewNameID();
        }
        isNameIDExisting[nameID] = true;
        return nameID;
    }

    private void FusePoint(int id1, int id2)
    {
        GridPoint point1 = getGridPoint(id1);
        GridPoint point2 = getGridPoint(id2);
        
        //Updating vectors linked to point
        foreach (GridVector vector in VectorsWithInitialPoint(id2))
        {
            ChangeInitialPoint(vector,id1);
        }
        foreach (GridVector vector in VectorsWithTerminalPoint(id2))
        {
            ChangeTerminalPoint(vector,id1);
        }

        DeletePoint(id2);
        return;
    }

    private List<GridVector> VectorsWithInitialPoint(int initialPointID)
    {
        Assert.IsTrue (initialPointID >= 0 && initialPointID < points.Count);
        List<GridVector> vectors = new List<GridVector>();
        foreach(GridVector vector in vectors)
        {
            if (vector.initialPoint.id == initialPointID)
            {
                vectors.Add(vector);
            }
        }
        return vectors;
    }
    private List<GridVector> VectorsWithTerminalPoint(int terminalPointID)
    {
        Assert.IsTrue (terminalPointID >= 0 && terminalPointID < points.Count);
        List<GridVector> vectors = new List<GridVector>();
        foreach(GridVector vector in vectors)
        {
            if (vector.terminalPoint.id == terminalPointID)
            {
                vectors.Add(vector);
            }
        }
        return vectors;
    }

    private List<GridVector> VectorsAssociatedWithPoint(int pointID)
    {
        List<GridVector> vectorsAssociated = VectorsWithInitialPoint(pointID);
        vectorsAssociated.AddRange(VectorsWithTerminalPoint(pointID));
        return vectorsAssociated;
    }
    private void ChangeInitialPoint(GridVector vector, int id)
    {
        //Todo: Asserts
        GridPoint point = getGridPoint(id);
        vector.initialPoint = point;
        //Todo: change vector position
    }
    private void ChangeTerminalPoint(GridVector vector, int id)
    {
        GridPoint terminalPoint = getGridPoint(id);
        vector.terminalPoint = terminalPoint;
        //Todo: Change vector position
    }

    public int CreateVector(int initialPointID, int terminalPointID)
    {
        //We don't want vectors AA to be possible at creation
        Assert.IsFalse(initialPointID == terminalPointID);

        GridPoint initialPoint = getGridPoint(initialPointID);
        GridPoint terminalPoint = getGridPoint(terminalPointID);

        //Todo: Check if vector associated with these two points already exist.
        int newID = vectorNextID;
        vectorNextID++;
        Vector vector = MathManager.instance.InstantiateVector(initialPoint.coordinates, terminalPoint.coordinates, transform).GetComponent<Vector>();
        vectors[newID] = new GridVector(newID, initialPoint, terminalPoint, vector);

        return newID;
    }

    public void MoveVector(int vectorID, Vector3 newInitialPointCoord, Vector3 newTerminalPointCoord)
    {
        //Todo: Assert coords are in bound
        GridVector vector = getGridVector(vectorID);

        //Using getGridPoint for the assertions, not really needed
        GridPoint oldInitialPoint = getGridPoint(vector.initialPoint.id);
        bool replaceInitialPoint = false;
        GridPoint oldTerminalPoint = getGridPoint(vector.terminalPoint.id);
        bool replaceTerminalPoint = false;

        //Checking if oldInitialPoints and oldTerminalPoints are linked to other vectors and creating new points if needed
        foreach (GridVector associatedVector in VectorsAssociatedWithPoint(oldInitialPoint.id))
        {
            if (associatedVector.id != vectorID)
            {
                replaceInitialPoint = true;
                int newInitialPointID = CreatePoint(newInitialPointCoord);
                ChangeInitialPoint(vector, newInitialPointID);
                break;
            }
        }
        foreach (GridVector associatedVector in VectorsAssociatedWithPoint(oldTerminalPoint.id))
        {
            if (associatedVector.id != vectorID)
            {
                replaceTerminalPoint = true;
                int newTerminalPointID = CreatePoint(newTerminalPointCoord);
                ChangeTerminalPoint(vector, newTerminalPointID);
                break;
            }
        }
        if (!replaceInitialPoint)
        {
            MovePoint(oldInitialPoint.id, newInitialPointCoord);
        }
        if (!replaceTerminalPoint)
        {
            MovePoint(oldTerminalPoint.id, newTerminalPointCoord);
        }
        vector.vector.ChangePointPosition(newInitialPointCoord, false);
        vector.vector.ChangePointPosition(newTerminalPointCoord, true);
    }
    
    private GridVector getGridVector(int id)
    {
        //Verifications and assertions
        Assert.IsTrue(vectors.ContainsKey(id));
        GridVector vector = vectors[id];
        getGridPoint(vector.initialPoint.id);
        getGridPoint(vector.terminalPoint.id);

        return vector;
    }

}

using UnityEngine.Assertions;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.Events;
using UnityEditor.Experimental.GraphView;

public class Grid2 : MonoBehaviour
{
    [Header("Grid parameters")]
    public int width = 5;
    public int height = 5;
    public int depth =0;
    public float scale = 0.1f;
    [SerializeField] private GameObject gridLine;
    [SerializeField] private Material planeMaterial;
    [Header("Points")]
    private Dictionary<int, GridPoint> points = new Dictionary<int,GridPoint>();
    private List<bool> isNameIDExisting = new List<bool>();
    int pointNextID = 0;

    [Header("Vectors")]
    private Dictionary<int, GridVector> vectors = new Dictionary<int, GridVector>();
    int vectorNextID = 0;

    [Header("Displayed Information")]
    private GridSingleObject exhibitedObject = new GridSingleObject();
    private GridSingleObject selectedObject = new GridSingleObject();
    public Color basicColor = Color.gray;
    public Color selectedObjectColor = Color.blue;
    private enum gridObjectType { Point, Vector, Null }

    [Header("Vectorial Sum")]
    public bool displayVectorSum = true;
    public Color vector1_Color;
    public Color vector2_Color;
    public Color vectorSumColor = Color.green;
    private string vectorSumText = "";
    private GridSingleObject vectorSumv1 = new GridSingleObject();
    private GridSingleObject vectorSumv2 = new GridSingleObject();
    private Vector3 vectorSumCoords = new Vector3();
    public bool b_showVectorSum = false;

    [Header("Still in dev")]
    public TMP_Text vectorTestText;

    [Header("Events")]
    public UnityEvent GridAddPoint;
    public UnityEvent GridDeletePoint;
    public UnityEvent GridVectorSum;
    public UnityEvent GridSelectPoint;
    public UnityEvent GridSelectVector;
    private class GridSingleObject
    {
        public gridObjectType type;
        public int id;

        public GridSingleObject(gridObjectType type = gridObjectType.Null, int exhibitedObjectID = -1)
        {
            this.type = type;
            this.id = exhibitedObjectID;
        }
        
        public void changeObject(gridObjectType type, int exhibitedObjectID)
        {
            this.type = type;
            this.id = exhibitedObjectID;
        }
    }
    private class GridPoint
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
    private class GridVector
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
        public void changeInitialPoint(GridPoint newInitialPoint)
        {
            this.initialPoint = newInitialPoint;
        }
        public void changeTerminalPoint(GridPoint newTerminalPoint)
        {
            this.terminalPoint = newTerminalPoint;
        }
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateGrid();
    }
    public IEnumerator showVectorSum(int vector1, int vector2, string vectorSumName = "v")
    {
        b_showVectorSum = true;
        if (!displayVectorSum)
        {
            Debug.LogError("ERROR::SHOWVECTORSUM::ACCESSED::WHILE::NOT::IN::VECTORSUM::MODE");
            StopCoroutine("showVectorSum");
        }

        //Todo change colors of v1 and v2
        vectorSumv1.changeObject(gridObjectType.Vector,vector1);
        vectorSumv2.changeObject(gridObjectType.Vector,vector2);
        
        GridVector v1 = getGridVector(vector1);
        GridVector v2 = getGridVector(vector2);

        
        GameObject vsum = MathManager.instance.InstantiateVector(v1.initialPoint.coordinates, (v1.terminalPoint.coordinates + (v2.terminalPoint.coordinates - v2.initialPoint.coordinates)), transform);
        while (displayVectorSum && b_showVectorSum)
        {
            v1 = getGridVector(vectorSumv1.id);
            v2 = getGridVector(vectorSumv2.id);
            ChangeVectorColor(v1, vector1_Color);
            ChangeVectorColor(v2, vector2_Color);
            vsum.GetComponent<Vector>().ChangeColor(vectorSumColor);
            vsum.GetComponent<Vector>().ChangePointPosition(v1.initialPoint.coordinates, false);
            vsum.GetComponent<Vector>().ChangePointPosition(v1.terminalPoint.coordinates + (v2.terminalPoint.coordinates - v2.initialPoint.coordinates), true);
            Vector3 vCoords = v1.terminalPoint.coordinates + v2.terminalPoint.coordinates - v1.initialPoint.coordinates - v2.initialPoint.coordinates;
            vectorSumCoords = vCoords;
            string vSumColorString = "<color=#" + ColorUtility.ToHtmlStringRGBA(vsum.GetComponent<Vector>().color) + ">";
            //Change exhibited text
            vectorSumText = VectorColorToString(vector1) + VectorName(vector1) + "<color=#000000> + ";
            vectorSumText += VectorColorToString(vector2) + VectorName(vector2) + "<color=#000000> = ";
            vectorSumText += vSumColorString + vectorSumName + "\n";
            vectorSumText += VectorColorToString(vector1) + VectorPositionToString(vector1) + "<color=#000000> + ";
            vectorSumText += VectorColorToString(vector2) + VectorPositionToString(vector2) + "<color=#000000> = ";
            vectorSumText += vSumColorString + string.Format("({0}; {1})", vCoords.x, vCoords.y);

            yield return null;
        }
        Destroy(vsum);
    }


    public Vector3 VectorSumCoords()
    {
        return vectorSumCoords;
    }


    public bool TryGetVectorByCoordinates(Vector3 coordinates, out int vectorID)
    {
        
        foreach (GridVector vector in vectors.Values)
        {
            Vector3 vCoords= vector.terminalPoint.coordinates - vector.initialPoint.coordinates;
               
            if (Vector3.Equals(vCoords, coordinates))
            {
                vectorID = vector.id;
                return true;
            }
        }
        vectorID = -1;
        return false;
    }
    public string VectorColorToString(int id)
    {
        GridVector vector = getGridVector(id);
        return "<color=#" + ColorUtility.ToHtmlStringRGBA(vector.vector.GetComponent<Vector>().color) + ">";
    }
    public void changeDisplayVectorSum()
    {
        displayVectorSum = !displayVectorSum;
    }
    IEnumerator TestGrid2()
    {
        yield return new WaitForEndOfFrame();
        ClearGrid();
        yield return new WaitForEndOfFrame();
        int A = CreatePoint(Vector3.zero,fusePoint:false);
        int B = CreatePoint(Vector3.zero,fusePoint:false);
        int C = CreatePoint(new Vector3(2, 2, 0));
        int D = CreatePoint(new Vector3(1,3,0));
        int v2 = CreateVector(B, C);
        int v1 = CreateVector(A, B);
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(1f);
        MovePoint(B, Vector3.right,fusePoint:false);
        showVectorSum(v2,v1);
        yield return new WaitForSeconds(3f);

    }

    public void AddToVector(int id, Vector3 coordsToAdd)
    {
        GridVector v = getGridVector(id);
        Vector3 oldTerminalPointCoords = v.terminalPoint.coordinates;
        MovePoint(v.terminalPoint.id, oldTerminalPointCoords + coordsToAdd, fusePoint:false);
    }
    public void AddToSelectedVector(Vector3 coordsToAdd)
    {
        Assert.IsTrue(selectedObject.type == gridObjectType.Vector, "Trying to modify selected vector coords when selected object isn't vector");
        AddToVector(selectedObject.id, coordsToAdd);
    }
    public void SelectedVectorXMinusOne()
    {
        AddToSelectedVector(Vector3.left);
    }
    public void SelectedVectorXPlusOne()
    {
        AddToSelectedVector(Vector3.right);
    }
    public void SelectedVectorYMinusOne()
    {
        AddToSelectedVector(Vector3.down);
    }
    public void SelectedVectorYPlusOne()
    {
        AddToSelectedVector(Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObject.type == gridObjectType.Vector)
        vectorTestText.text = VectorColorToString(selectedObject.id) + VectorPositionToString(selectedObject.id);
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
        plane.GetComponent<Renderer>().material = planeMaterial;
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
    public void ClearGrid()
    {
        UnSelect();
        UnExhibit();
        List<int> pointToDelete = new List<int>();
        List<int> vectorsToDelete = new List<int>();

        foreach (int pointID in points.Keys)
        {
            pointToDelete.Add(pointID);
        }
        foreach (int vectorID in vectors.Keys)
        {
            vectorsToDelete.Add(vectorID);
        }
        foreach (var vectorID in vectorsToDelete)
        {
            DeleteVector(vectorID); 
        }
        foreach (var pointID in pointToDelete)
        {
            DeletePoint(pointID);
        }
        vectors = new Dictionary<int, GridVector>();
        points = new Dictionary<int, GridPoint>();

        b_showVectorSum = false;
    }
    private GridPoint getGridPoint(int id)
    {
        if (!points.ContainsKey(id))
        {
            Debug.LogError(string.Format("ERROR::POINT::DOESNT::EXIST id = {0}", id));
        }
        return points[id];
    }
    public bool TryGetPointByCoordinates(Vector3 coordinates, out int existingPointID)
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
        point.GetComponent<Point>().setName(NameIDToString(pointNameID));
        points[newID] = new GridPoint(coordinates, pointNameID, newID, point);

        //Invoke AddPoint event for potential listeners
        GridAddPoint.Invoke();
        //return new point id
        return newID;
    }

    public void MovePoint(int id, Vector3 coordinates, bool fusePoint = true)
    {
        //Todo: Assert coordinates are in grid Space
        GridPoint point = getGridPoint(id);

        //Todo: Move vectors associated with point
        foreach ( GridVector v in VectorsWithInitialPoint(id))
        {
            v.vector.ChangePointPosition(coordinates,false);
        }
        foreach ( GridVector v in VectorsWithTerminalPoint(id))
        {
            v.vector.ChangePointPosition(coordinates, true);
        }

                //Fuse point if asked to do it and point already exist in coordinates
        if (fusePoint && TryGetPointByCoordinates(coordinates, out int existingPointID)) 
        {
            if (!(existingPointID == id))
                FusePoint(id, existingPointID);
            //Might need to return here if Fusing destroy the moved point
        }

        //Updating point coordinates
        point.coordinates = coordinates;
        point.pointObject.transform.localPosition = coordinates;
        
        return;
    }
    public void DeletePoint(int id)
    {
        if (selectedObject.id == id && selectedObject.type == gridObjectType.Point) UnSelect();
        if (exhibitedObject.id == id && exhibitedObject.type == gridObjectType.Point) UnExhibit();

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
        GridDeletePoint.Invoke();
        return;
    }
    private void DeletePoint(GridPoint gridPoint)
    {
        DeletePoint(gridPoint.id);
    }
    private void DeleteVector(GridVector vector)
    {
        DeleteVector(vector.id);
    }
    public void DeleteVector(int id)
    {
        if (selectedObject.id == id && selectedObject.type == gridObjectType.Vector) UnSelect();
        if (exhibitedObject.id == id && exhibitedObject.type == gridObjectType.Vector) UnExhibit();
        GridVector vector = getGridVector(id);
        vectors.Remove(id);
        Destroy(vector.vector.gameObject);
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
                    isNameIDExisting[i] = true;
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

        Assert.IsTrue(points.ContainsKey(initialPointID));
        List<GridVector > result = new List<GridVector>();
        foreach(int vectorID in vectors.Keys)
        {
            GridVector vector = getGridVector(vectorID);
            if (vector.initialPoint.id == initialPointID)
            {
                result.Add(vector);
            }
        }
        return result;
    }
    private List<GridVector> VectorsWithTerminalPoint(int terminalPointID)
    {
        Assert.IsTrue(points.ContainsKey(terminalPointID));
        List<GridVector> result = new List<GridVector>();
        foreach(int vectorID in vectors.Keys)
        {
            GridVector vector = getGridVector(vectorID);
            if (vector.terminalPoint.id == terminalPointID)
            {
                result.Add(vector);
            }
        }
        return result;
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
        GridPoint initialPoint = getGridPoint(id);
        vector.changeInitialPoint(initialPoint);
    }
    private void ChangeTerminalPoint(GridVector vector, int id)
    {
        GridPoint terminalPoint = getGridPoint(id);
        vector.changeTerminalPoint(terminalPoint);
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
        Assert.IsTrue(Mathf.Abs(newInitialPointCoord.x) <= width && Mathf.Abs(newTerminalPointCoord.y) <= height, "Vector moved out of bounds"); 
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
        
        GridVector vector = vectors[id];
        getGridPoint(vector.initialPoint.id);
        getGridPoint(vector.terminalPoint.id);

        return vector;
    }

    private string NameIDToString(int nameID)
    {
        char c =  (char)(nameID);
        c += 'A';
        return c.ToString();
    }
    public int GetPointIDByName(string name)
    {
        foreach (GridPoint point in points.Values)
        {
            if (PointName(point).Equals(name))
            {
                return point.id;
            }
        }
        return -1;
    }
    private string PointName(GridPoint point)
    {
        return NameIDToString(point.nameID);
    }
    private string PointName(int pointID)
    {
        return PointName(getGridPoint(pointID));
    }
    private string VectorName(GridVector vector)
    {
        return PointName(vector.initialPoint) + PointName(vector.terminalPoint);
    }
    private string VectorName(int vectorID)
    {
        return VectorName(getGridVector(vectorID));
    }
    private string PointPositionToString(GridPoint point)
    {
        return string.Format("({0};{1})", point.coordinates.x, point.coordinates.y);
    }
    private string PointPositionToString(int pointID)
    {
        return PointPositionToString(getGridPoint(pointID));
    }
    private string VectorPositionToString(GridVector vector)
    {
        Vector3 vectorCoordinates = vector.terminalPoint.coordinates - vector.initialPoint.coordinates;
        return string.Format("({0};{1})", vectorCoordinates.x, vectorCoordinates.y);
    }
    public string VectorPositionToString(int id)
    {
        return VectorPositionToString(getGridVector(id));
    }
    public string VectorPositionAndColorToString(int id)
    {
        return VectorColorToString(id) + VectorPositionToString(id);
    }
    public string ExhibitText()
    {
        string s = "";
        //In case of vector sum, the exhibited text should be the vectorial sum
        if (displayVectorSum && !vectorSumText.Equals(""))
        {
            return vectorSumText;
        }


        switch (exhibitedObject.type)
        {
            case gridObjectType.Null:
                return "";
                
            case gridObjectType.Point:
                //Change text color to match point color
                s = "<color=#" + ColorUtility.ToHtmlStringRGB(getGridPoint(exhibitedObject.id).pointObject.GetComponent<Point>().color) + ">";
                //Display point name and position
                return s + PointName(exhibitedObject.id) + " : " + PointPositionToString(exhibitedObject.id);
            case gridObjectType.Vector:
                s = "<color=#" + ColorUtility.ToHtmlStringRGB(getGridVector(exhibitedObject.id).vector.GetComponent<Vector>().color) + ">";
//Todo: do this you lazy dev
                return s + VectorName(exhibitedObject.id) + " : " + VectorPositionToString(exhibitedObject.id);
            default:
                Debug.LogError("exhibitedObject.type isn't supported");
                return "";
        }
    }
    private void ChangePointColor(GridPoint point, Color color)
    {
        point.pointObject.GetComponent<Point>().changeColor(color);
    }
    public void ChangePointColor(int pointID, Color color)
    {
        ChangePointColor(getGridPoint(pointID), color);
    }
    private void ChangeVectorColor(GridVector vector, Color color)
    {
        vector.vector.GetComponent<Vector>().ChangeColor(color);
    }
    public void ChangeVectorColor(int vectorID, Color color)
    {
        ChangeVectorColor(getGridVector(vectorID), color);
    }
    public void ExhibitPoint(int id)
    {
        exhibitedObject.changeObject(gridObjectType.Point, id);
    }
    public void ExhibitVector(int id)
    {
        exhibitedObject.changeObject(gridObjectType.Vector, id);
    }
    public void UnExhibit()
    {
        exhibitedObject.changeObject(gridObjectType.Null, -1);
    }
    public void SelectPoint(int id)
    {
        UnSelect();
        GridPoint point = getGridPoint(id);
        point.pointObject.GetComponent<Point>().changeColor(selectedObjectColor);
        selectedObject.changeObject(gridObjectType.Point, id);
        ExhibitPoint(id);
    }
    public void SelectVector(int id)
    {
        UnSelect();
        GridVector vector = getGridVector(id);
        vector.vector.GetComponent<Vector>().ChangeColor(selectedObjectColor);
        selectedObject.changeObject(gridObjectType.Vector, id);
        ExhibitVector(id);
    }
    public void UnSelect()
    {
        //Changing back old selected object to old color
        switch(selectedObject.type)
        {
            case (gridObjectType.Point):
                ChangePointColor(selectedObject.id,basicColor);
                break;
            case (gridObjectType.Vector):
                ChangeVectorColor(selectedObject.id,basicColor);
                break;
            default:
                break;
        }
        //Changing selected object to nothing
        selectedObject.changeObject(gridObjectType.Null, -1);
    }
    public string GridContent()
    {

        string s = "";
        s += "Points\n\n";
        foreach (int id in points.Keys)
        {
            GridPoint point = getGridPoint(id);
            s += "<color=#" + ColorUtility.ToHtmlStringRGBA(point.pointObject.GetComponent<Point>().color) + ">";
            s += PointName(point) + " : " + PointPositionToString(point);
            s += "\n";
        }
        s += "Vectors\n\n";
        foreach (int id in vectors.Keys)
        {
            GridVector vector = vectors[id];
            s += "<color=#" + ColorUtility.ToHtmlStringRGBA(vector.vector.GetComponent<Vector>().color) + ">";
            s += VectorName(vector) + " : " + VectorPositionToString(vector);
            s += "\n";
        }
        //Todo: display vectors too
        return s;
    }
    public string VectorIDS(int id)
    {
        return string.Format("initial point = {0}; terminal point = {1}", vectors[id].initialPoint.id, vectors[id].terminalPoint.id);
    }
    public Vector3 PosToCoord(Vector3 pos)
    {
        return (pos - transform.position) / transform.localScale.x;
    }
    public Vector3 PosToRoundCoord(Vector3 pos)
    {
        Vector3 coords = PosToCoord(pos);
        return new Vector3(Mathf.Round(coords.x), Mathf.Round(coords.y), Mathf.Round(coords.z));
    }
    public Vector3 CoordToPos(Vector3 coord)
    {
        return (scale * coord) + transform.position;
    }

    public int GetVectorId(Vector vector)
    {
        foreach (KeyValuePair<int, GridVector> pair in vectors)
        {
            if (pair.Value.vector.Equals(vector)) return pair.Key;
        }
        Debug.LogWarning("could not find vector in vectors list on the grid");
        return -1;
    }
    public bool isVectorSelected(out int selectedVectorID)
    {
        if (selectedObject.type == gridObjectType.Vector)
        {
            selectedVectorID = selectedObject.id;
            return true;
        }
        selectedVectorID = -1;
        return false;
    }
    public void VectorSum(int v1, int v2)
    {
        StartCoroutine(showVectorSum(v1, v2));
    }

    public Dictionary<string, int> getPointsNameDict()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();  
        foreach(GridPoint point in points.Values)
        {
            dict[PointName(point)] = point.id;
        }
        return dict;
    }
    public Dictionary<string, int> getVectorsNameDict()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();
        foreach (GridVector vector in vectors.Values)
        {
            dict[VectorColorToString(vector.id) + VectorName(vector)] = vector.id;
        }
        return dict;
    }
    public string SelectedPointName()
    {
        if (selectedObject.type == gridObjectType.Point)
        {
            return PointName(selectedObject.id);
        }
        return "";
    }

    //Event grid modification

    public (int, int) GetVectorPoints(int vectorID)
    {
        return (vectors[vectorID].initialPoint.id, vectors[vectorID].terminalPoint.id);
    }
    public ((int, int), (int, int)) GetVectorSumPointIDS()
    {
        return (GetVectorPoints(vectorSumv1.id), GetVectorPoints(vectorSumv2.id));
    }
    public bool GetVectorByName(string name, out int id)
    {
        id = -1;
        foreach(GridVector vector in vectors.Values)
        {
            if (Equals(VectorName(vector), name) || Equals(VectorColorToString(vector.id) + VectorName(vector), name))
            {
                id = vector.id;
                return true;
            }
        }
        return false;
    }
    public void changeVectorSum(int vectorNumber, int vectorID)
    {
        if (vectorNumber == 0)
        {
            ChangeVectorColor(vectorSumv1.id, basicColor);
            vectorSumv1.changeObject(gridObjectType.Vector, vectorID);
        }

        if (vectorNumber == 1)
        {
            vectorSumv2.changeObject(gridObjectType.Vector, vectorID);
        }
    }
    public (int, int) GetVectorSumIDS()
    {
        return (vectorSumv1.id, vectorSumv2.id);
    }


    public float VectorNorm(int id)
    {
        GridVector vector = getGridVector(id);

        float x = vector.terminalPoint.coordinates.x -vector.initialPoint.coordinates.x;
        float y = vector.terminalPoint.coordinates.y - vector.initialPoint.coordinates.y;
        return Mathf.Sqrt(x * x + y * y);
    }
    public Vector3 VectorCoords(int id)
    {
        GridVector vector = getGridVector(id);
        
        float x = vector.terminalPoint.coordinates.x -vector.initialPoint.coordinates.x;
        float y = vector.terminalPoint.coordinates.y - vector.initialPoint.coordinates.y;
        return new Vector3(x, y);
    }
    public Vector3 PointCoords(int id)
    {
        return getGridPoint(id).coordinates;
    }

    public void VectorZoom(int vectorID, float scaleFactor, float zoomDuration, float transitionDuration = 0.2f)
    {
        Vector v = vectors[vectorID].vector;
        ScaleUp su = v.gameObject.AddComponent<ScaleUp>();
        su.scaleFactor = scaleFactor;
        su.fullZoomDurationInSeconds = zoomDuration;
        su.transitionDurationInSeconds = transitionDuration;
    }
}

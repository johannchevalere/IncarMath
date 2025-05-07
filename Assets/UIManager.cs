using UnityEngine.Assertions;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class UIManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid2 grid;
    [Header("UI Elements")]
    [Header("Sole Point")]
    public GameObject SolePointUI;
    private DropdownPoint solePointDropdown;
    public int solePointID = 0;
    [Header("Sole Vector")]
    public GameObject SoleVectorUI;
    private DropdownVector soleVectorDropDown;
    public int soleVectorID = 0;
    [Header("Vector Sum")]
    public VectorSumUI VectorSumUI;
    public string vectorSumString = "_";
    [Header("Scalar Vector")]
    public GameObject ScalarVectorUI;
    [Header("Pop Up")]
    public GameObject PopUpUI;
    public enum UIConfig
    {
        SolePoint, SoleVector, VectorSum, ScalarVector, None
    }
    public UIConfig config;
    public UIConfig lastState = UIConfig.None;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(SolePointUI, "No Sole Point UI Object detected");
        if (!(SolePointUI.transform.Find("PointDropDown").TryGetComponent<DropdownPoint>(out solePointDropdown)))
        {
            Debug.LogError("Can't find PointDropDown on SolePointUI");
        }
        Assert.IsNotNull(SoleVectorUI, "No Sole Vector UI Object detected");
        if (!(SoleVectorUI.transform.Find("VectorDropDown")))
        Assert.IsNotNull(VectorSumUI, "No VectorSum UI Object detected");
        Assert.IsNotNull(ScalarVectorUI, "No ScalarVector UI Object detected");
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateConfig();
    }

    void UpdateConfig()
    {
        if (lastState != config)
            DisableUI();

        switch (config)
        {
            case UIConfig.SolePoint:
                Debug.Log("This should appear");
                if (lastState != config)
                {
                    Debug.Log("This should appear once");
                    SolePointUI.SetActive(true);
                }
                    UpdateSolePoint();
                break;
            case UIConfig.SoleVector:
                
                if(!lastState.Equals(config))
                    SoleVectorUI.SetActive(true);
                UpdateSoleVector();
                break;
            case UIConfig.ScalarVector:
                if(!lastState.Equals(config))
                    ScalarVectorUI.SetActive(true);
                break;
            case UIConfig.VectorSum:
                if (!lastState.Equals(config))
                { 
                    VectorSumUI.gameObject.SetActive(true);
                    ActualizeVectorSumNames();
                }
                UpdateVectorSum();
                break;
            case UIConfig.None:
                break;
            default:
                Debug.LogError("No UI configuration selected");
                break;
        }
        lastState = config;
    }
    void DisableUI()
        {
            Debug.Log("This should appear once too");
            SolePointUI.SetActive(false);
            SoleVectorUI.SetActive(false);
            VectorSumUI.gameObject.SetActive(false);
            ScalarVectorUI.SetActive(false);
        }
    void UpdateSolePoint() {
        SolePointUI pointUIObject = SolePointUI.GetComponent<SolePointUI>();
        if (grid.ContainsPointID(solePointID))
        {
            Debug.Log("I'm in");
            string pointName = grid.PointName(solePointID);
            string pointCoords = "(" + grid.PointCoords(solePointID).x.ToString() + "; " + grid.PointCoords(solePointID).y.ToString() + ")";
            pointUIObject.pointName = pointName;
            pointUIObject.pointCoord = pointCoords;
        }
    }
    void UpdateSoleVector() {
        SoleVectorUI vectorUIObject = SoleVectorUI.GetComponent<SoleVectorUI>();
        if (grid.ContainsVectorID(soleVectorID))
        {
            string vectorName = grid.VectorColorToString(soleVectorID) + grid.VectorName(soleVectorID);
            string vectorCoords = grid.VectorPositionAndColorToString(soleVectorID);
            vectorUIObject.vectorName = vectorName;
            vectorUIObject.vectorCoord = vectorCoords;
        }
    }
    public void setID(int id)
    {
        solePointID = id;
        soleVectorID = id;
    }
    void UpdateVectorSum()
    {
       (int, int) vectorSum = grid.GetVectorSumIDS();
        VectorSumUI.UpdateCoords(0, grid.VectorPositionAndColorToString(vectorSum.Item1));
        VectorSumUI.UpdateCoords(1, grid.VectorPositionAndColorToString(vectorSum.Item2));
        VectorSumUI.UpdateCoords(2, VectorSumString());
    }
    string VectorSumString()
    {
        if (vectorSumString == "_")
        {
            (int, int) vectorSum = grid.GetVectorSumIDS();
            Vector3 sum = grid.VectorCoords(vectorSum.Item1) + grid.VectorCoords(vectorSum.Item2);
            string r = string.Format("<color=#C9A781>({0};{1})", sum.x, sum.y);
            return r;
        }
        else return vectorSumString;
    }


    void UpdateScalarVector()
    {
        
    }

    //Vector Sums Function

    public void modifyVector(string vectorName, Vector3 coordsToAdd)
    {
        if (!grid.GetVectorByName(vectorName, out int id))
        {
            Debug.LogError("Could not find vector name" + vectorName);
            return;
        }
        grid.AddToVector(id, coordsToAdd);
    }
    public void changeVectorSum(int vectorNumber, string vectorName)
    {
        if (!grid.GetVectorByName(vectorName, out int id))
        {
            Debug.LogError("Could not find vector name : " + vectorName);
            return;
        }
        grid.changeVectorSum(vectorNumber, id);
    }
    public void ActualizeVectorSumNames()
    {
        Dictionary<string, int> names = grid.getVectorsNameDict();
        List<string> options = new List<string>();
        int id1 = -1;
        int id2 = -1;
        (int, int) vectorSum = grid.GetVectorSumIDS();
        int i = 0;
        foreach (string name in names.Keys)
        {
            options.Add(name);
            if (names[name] == vectorSum.Item1) id1 = i;
            if (names[name] == vectorSum.Item2) id2 = i;
            i++;
        }
        VectorSumUI.ActualiseVectorOptions(0, options, id1);
        VectorSumUI.ActualiseVectorOptions(1, options, id2);
    }
}

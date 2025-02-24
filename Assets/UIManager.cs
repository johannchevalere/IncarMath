using NUnit.Framework;
using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    [Header("Grid")]
    public Grid2 grid;
    [Header("UI Elements")]
    [Header("Sole Point")]
    public GameObject SolePointUI;
    private DropdownPoint solePointDropdown;
    [Header("Sole Vector")]
    
    public GameObject SoleVectorUI;
    [Header("Vector Sum")]
    public GameObject VectorSumUI;
    [Header("Scalar Vector")]
    public GameObject ScalarVectorUI;
    public enum UIConfig
    {
        SolePoint, SoleVector, VectorSum, ScalarVector
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(SolePointUI, "No Sole Point UI Object detected");
        SolePointUI.transform.Find("PointDropDown");
        Assert.IsNotNull(SoleVectorUI, "No Sole Vector UI Object detected");
        Assert.IsNotNull(VectorSumUI, "No VectorSum UI Object detected");
        Assert.IsNotNull(ScalarVectorUI, "No ScalarVector UI Object detected");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}

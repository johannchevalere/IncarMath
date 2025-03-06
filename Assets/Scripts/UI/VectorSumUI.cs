using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;

public class VectorSumUI : MonoBehaviour
{
    [Header("UI Manager")]
    public UIManager uiManager;
    [Header("Vector Names")]
    public TMP_Dropdown v1DropDown;
    public TMP_Dropdown v2DropDown;
    public TMP_Text sumName;

    [Header("Coordinates")]
    public Button v1XPlus;
    public Button v1XMinus;
    public Button v1YPlus;
    public Button v1YMinus;
    public Button v2XPlus;
    public Button v2XMinus;
    public Button v2YPlus;
    public Button v2YMinus;
    public TMP_Text v1Coords;
    public TMP_Text v2Coords;
    public TMP_Text vSumCoords;


    //==============Dropdown functions===============
    public void ActualiseVectorOptions(int vectorNumber, List<string> VectorNames, int VectorIndexInList = 0)
    {
        TMP_Dropdown dropdown = vectorNumber == 0 ? v1DropDown : v2DropDown;
        dropdown.ClearOptions();
        dropdown.AddOptions(VectorNames);
        dropdown.value = VectorIndexInList;
    }
    public void ChangeVector(int vectorNumber)
    {
        string vectorName = vectorNumber ==0 ? v1DropDown.options[v1DropDown.value].text : v2DropDown.options[v2DropDown.value].text;
        uiManager.changeVectorSum(vectorNumber, vectorName);
    }
    public void ChangeSumName(string name)
    {
        sumName.text = name;
    }
    

    //===============Coords Functions================
    public void UpdateCoords(int vectorNumber, string coords)
    {
        if (vectorNumber == 0) v1Coords.text = coords;
        if (vectorNumber == 1) v2Coords.text = coords;
        if (vectorNumber == 2) vSumCoords.text = coords;
    }
    

    public void modifyCoords(int vectorNumber, Vector3 coordsToAdd)
    {
        string vectorName = vectorNumber == 0 ? v1DropDown.options[v1DropDown.value].text : v2DropDown.options[v2DropDown.value].text;
        uiManager.modifyVector(vectorName, coordsToAdd);
    }

}


using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;
public class DropdownVector : MonoBehaviour
{
    public TMP_Dropdown firstLetter;
    public TMP_Dropdown secondLetter;
    public Grid2 grid;
    List<string> pointNameList = new List<string>();
    Dictionary<(string, string), (int,int)> namesToID = new Dictionary<(string, string), (int, int)> ();
    Dictionary<string, List<string>> dictInitialTerminalPoint = new Dictionary<string, List<string>>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Sign up to grid events
        grid.GridAddPoint.AddListener(actualizeVectorName);
        grid.GridDeletePoint.AddListener(actualizeVectorName);
        firstLetter.onValueChanged.AddListener(delegate { actualizeSecondPointOptions(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void actualizeVectorName()
    {
        namesToID = grid.getVectorsNameDict();
        foreach ((string initialPointName, string terminalPointName) in namesToID.Keys)
        {
            if (!dictInitialTerminalPoint.ContainsKey(initialPointName))
            {
                dictInitialTerminalPoint[initialPointName] = new List<string>();
                dictInitialTerminalPoint[initialPointName].Add("_");
            }
            dictInitialTerminalPoint[initialPointName].Add(terminalPointName);
            pointNameList.Add(initialPointName);

        }
        pointNameList.Sort();
        firstLetter.ClearOptions();
        firstLetter.AddOptions(pointNameList);
        actualizeSecondPointOptions();
    }
    public void actualizeSecondPointOptions()
    {
        Debug.Log(firstLetter.value);
        secondLetter.ClearOptions();
        string letter = firstLetter.options[firstLetter.value].text;
        dictInitialTerminalPoint[letter].Sort();
        secondLetter.AddOptions(dictInitialTerminalPoint[firstLetter.options[firstLetter.value].text]);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownPoint : MonoBehaviour
{

    public Grid2 grid;
    public Dictionary<string, int> nameToID = new Dictionary<string, int>();
    public List<string> names = new List<string>();
    private TMP_Dropdown dropdownComponent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!gameObject.TryGetComponent<TMP_Dropdown>(out dropdownComponent))
        {
            Debug.LogError("DropDownPoint has been added to an object without a dropdown component");
        }
        grid.GridAddPoint.AddListener(actualizePointNames);
        grid.GridDeletePoint.AddListener(actualizePointNames);
        grid.GridSelectPoint.AddListener(changeSelectedOnGrid);
        actualizePointNames();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void actualizePointNames() {
        nameToID = grid.getPointsNameDict();
        names = new List<string>();
        string selectedName = grid.SelectedPointName();
        foreach (string name in nameToID.Keys) {
            names.Add(name);
        }
        names.Add("_");
        names.Sort();
        dropdownComponent.ClearOptions();
        dropdownComponent.AddOptions(names);
        for (int i = 0; i < names.Count; i++)
        {
            if (names[i] == selectedName)
            {
                dropdownComponent.value = i; break;
            }
        }
    }

    public void changeSelectedOnGrid()
    {
        string value = dropdownComponent.options[dropdownComponent.value].text;
        if (value != "_")
            grid.SelectPoint(nameToID[value]);
    }
    void changeSelectedOnText()
    {
        string selectedName = grid.SelectedPointName();

    }
}

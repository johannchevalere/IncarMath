using NUnit.Framework;
using UnityEngine;

using System.Collections.Generic;
using System.Globalization;
using TMPro;
public class ScenarioManager : MonoBehaviour
{
    enum State { Exercise, Explanation}
    public static ScenarioManager instance;
    public Repere exerciseGrid;
    public Grid2 grid2;
    public TMP_Text instructionText;
    public GameObject popUpUI;
    public GameObject playerManager;
    public TMP_Text gridContentText;
    enum Condition
    {
        VectorAtCoordinates,
        PointAtCoordinates,
        VectorMagnitude,
        VectorDirection,
        VectorExistence,
        PointExistence
    }
    private List<List<Instruction>> instructions = new();
    private int instructionIndex = 0;
    struct Instruction
    {
        Condition condition;
        Vector3? coords1;
        Vector3? coords2;
        float? value;
        int count;
        Repere grid;
        public readonly string instructionText;

        //Instruction is created with lots of differents parameters.
        //Condition is the rule to pass the exercice
        //coords1 is used as the value of a needed coordinate in the instruction (eg: the position of a point)
        //coords2 is used as the value of a 2nd needed coordinate in the instruction (eg: the position of the terminal point of a vector)
        //value is used when an int argument is needed (might need to change that to float)
        //count is the number of object that needs to have the condition to validate it.
        public Instruction(Condition condition, Repere grid, string instructionText, Vector3? coords1 = null, Vector3? coords2 = null, float? value = null, int count = 1 )
        {
            switch (condition)
            {
                case Condition.VectorAtCoordinates:
                    Assert.IsNotNull(coords1);
                    Assert.IsNotNull(coords2);
                    Assert.IsNull(value);
                    break;
                case Condition.PointAtCoordinates:
                    Assert.IsNotNull(coords1);
                    Assert.IsNull(coords2);
                    Assert.IsNull(value);
                    break;
                case Condition.VectorMagnitude:
                    Assert.IsNull(coords1);
                    Assert.IsNull(coords2);
                    Assert.IsNotNull(value);
                    break;
                case Condition.VectorDirection:
                    Assert.IsNotNull(coords1);
                    Assert.IsNull(coords2);
                    Assert.IsNull(value);
                    break;
                case Condition.VectorExistence:
                    Assert.IsNull (coords1);
                    Assert.IsNull(coords2);
                    Assert.IsNull (value);
                    break;
                case Condition.PointExistence:
                    Assert.IsNull (coords1);
                    Assert.IsNull(coords2);
                    Assert.IsNull(value);
                    break;
            }

            this.condition = condition;
            this.value = value;
            this.coords1 = coords1;
            this.coords2 = coords2;
            this.grid = grid;
            this.count = count;
            this.instructionText = instructionText;
    }
        //Todo: ToString
        public bool IsCompleted()
        {
            switch (condition)
            {
                case Condition.VectorAtCoordinates:
                    return grid.vectorAtCoordinates((Vector3)coords1, (Vector3)coords2);
                case Condition.PointAtCoordinates:
                    //TODO Implement verification functions for conditions
                    return grid.pointAtCoordinates((Vector3) coords1);
                case Condition.VectorMagnitude:
                    Assert.IsNotNull(value);
                    return grid.NbVectorMagnitude((float) value) >= count;
                case Condition.VectorDirection:
                    Assert.IsNotNull(coords1);
                    return grid.NbVectorDirection((Vector3) coords1) >= count;
                case Condition.VectorExistence:
                    Assert.IsNull(value);

                    return grid.NbVectors() >= count;

                case Condition.PointExistence:
                    Assert.IsNotNull(value);
                    return grid.NbPoints() >= count;
                default:
                    Debug.LogWarning("Condition not supported");
                    return false;

            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null && instance != this) Destroy(this.gameObject);
        instance = this;

    }
    private void Update()
    {
        DisplayGridContent();
    }
    public void CheckConditions()
    {
        bool b = true;
        foreach (Instruction instruction in instructions[instructionIndex])
        {
            b &= instruction.IsCompleted();
        }
        if (b) 
        {
            instructionIndex++;

            //Fin de séquence
            if (instructionIndex >= instructions.Count)
            {
                
                popUp("Séquence finie !");
                //Return to menu
                instructionIndex--;
            }
            else
            {

                popUp(instructions[instructionIndex][0].instructionText);
            }
        }
        else
        {
            popUp("Ce n'est pas la bonne réponse !");
        }
    }
    public void popUp(string text)
    {
        playerManager.SetActive(false);
        popUpUI.SetActive(true);
        Transform child = popUpUI.transform.Find("Panel/PopUpText");
        if (child != null && child.TryGetComponent<TMP_Text>(out var t))
        {
            t.text = text;
        }
    }
    private void DisplayGridContent()
    {
        gridContentText.text = grid2.GridContent();
    }
}

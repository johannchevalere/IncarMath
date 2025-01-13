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
    public TMP_Text selectedText;
    public TMP_Text instructionText;
    enum Condition
    {
        VectorAtCoordinates,
        PointAtCoordinates,
        VectorMagnitude,
        VectorDirection,
        VectorExistence,
        PointExistence
    }
    private List<Instruction> instructions = new();
    struct Instruction
    {
        Condition condition;
        Vector3? coords1;
        Vector3? coords2;
        float? value;
        int count;
        Repere grid;

        //Instruction is created with lots of differents parameters.
        //Condition is the rule to pass the exercice
        //coords1 is used as the value of a needed coordinate in the instruction (eg: the position of a point)
        //coords2 is used as the value of a 2nd needed coordinate in the instruction (eg: the position of the terminal point of a vector)
        //value is used when an int argument is needed (might need to change that to float)
        //count is the number of object that needs to have the condition to validate it.
        public Instruction(Condition condition, Repere grid, Vector3? coords1 = null, Vector3? coords2 = null, float? value = null, int count = 1 )
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
    }
        //Todo: ToString
        public bool IsCompleted()
        {
            switch (condition)
            {
                case Condition.VectorAtCoordinates:
                    
                    return false;
                case Condition.PointAtCoordinates:
                    //TODO Implement verification functions for conditions
                    return false;
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

        Instruction ins = new (Condition.VectorExistence, exerciseGrid, count: 5);
        instructions.Add(ins);
    }
    private void Update()
    {
        selectedText.text = exerciseGrid.SelectedToString();
    }
    public void CheckConditions()
    {
        bool b = true;
        foreach (Instruction instruction in instructions)
        {
            b &= instruction.IsCompleted();
        }
        if (b) 
        {
            selectedText.text = "Exercice réussi";            
            //Todo: Finish the exercise and go to the next one
        }
    }
}

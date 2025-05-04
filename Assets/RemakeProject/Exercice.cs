using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class TimedEvent
{
    public float timestamp;
    public ActionType actionType;
    
    public string actionParam;
    public float duration;

    public GridConfig gridConfig;
}
public enum ActionType {
    
    HighlightGrid,
    GrowVector,
    ChangeGridConfig,
    //CHANGEUI Config
    //ChangeUI information
}
public enum Condition
{
    NoCondition, PointAtCoord, VectorAtCoord, VectorialSumCoord
}
[System.Serializable] 
public class GridConfig
{
    public List<Vector2> points;
    public List<char> pointNames;
    public List<(string, string)> vectors;
    public bool DisplayVectorSum;
    public (string, string) vectorSum;
}

[System.Serializable]
public class ExerciceStep
{
    public Condition ExerciceSucessCondition;
    public Vector2 ExerciceSucessParam;
    public string exerciceTitle;
    public string exerciceDescription;
    public AudioClip audioConsigne;
    public AudioClip audioSucess;
    public AudioClip audioFailure;
    public bool changeGridConfig;
    public GridConfig gridConfig;
    public List<TimedEvent> events;
    [Header("UI")]
    public UIManager.UIConfig exerciceUIConfig;
}
public class Exercice : MonoBehaviour
{
    public Grid2 grid;

    public string scenarioTitle;
    public List<ExerciceStep> steps;
    public int ExerciceStepIndex = 0;
    public AudioSource audioSource;
    public bool conditionMet = false;

    private void Start()
    {
        StartCoroutine(playExerciceStep());
    }

    bool CheckCondition(Vector3 coordinates, Condition condition)
    {
        switch (condition)
        {
            case Condition.PointAtCoord:
                return grid.TryGetPointByCoordinates(coordinates, out int pointID);
            case Condition.VectorAtCoord:
                return grid.TryGetVectorByCoordinates(coordinates, out int vectorID);
            case Condition.VectorialSumCoord:
                return (grid.VectorSumCoords() == coordinates);
            default: return false;
        }
    }

    IEnumerator playExerciceStep()
    {
        ExerciceStep step = steps[ExerciceStepIndex];
        audioSource.PlayOneShot(step.audioConsigne);
        float t0 = 0f;
        float t1 = 0f;
        while (!conditionMet)
        {
            yield return new WaitForEndOfFrame();
            t1 += Time.deltaTime;

            foreach (TimedEvent ev in step.events)
            {
                if (ev.timestamp >= t0 && ev.timestamp < t1)
                {
                    StartCoroutine(solveTimedEvent(ev));
                }
            }
        }
    }


    IEnumerator solveTimedEvent(TimedEvent timedEvent)
    {
        yield return new WaitForEndOfFrame();

        switch (timedEvent.actionType) {
            case ActionType.HighlightGrid:
                //Show Highlight on coordinates at param

                yield return new WaitForSeconds(timedEvent.duration); break;
                //Delete Highlight on coordinates at param
            case ActionType.GrowVector:
                bool vectorRetrieved = grid.GetVectorByName(timedEvent.actionParam, out int vectorID);
                grid.VectorZoom(vectorID, 1.1f, timedEvent.duration);
                break;

            case ActionType.ChangeGridConfig:
                SetUpGrid(timedEvent.gridConfig); break;
            
        }
    }

    void SetUpGrid(GridConfig config)
    {
        int i = 0;
        foreach (Vector2 point in config.points) {
            int nameID = -1;
            if (config.pointNames.Count > i)
            {
                nameID = config.pointNames[i] + 'A';
            }
            grid.CreatePoint(point, nameID);

        }

        foreach ((string, string) vector in config.vectors)
        {
            int point1ID = grid.GetPointIDByName(vector.Item1);
            int point2ID = grid.GetPointIDByName(vector.Item2);
            if (point1ID == -1 || point2ID == -1) Debug.LogWarning("A point in your vector pair doesn't exist");
            else grid.CreateVector(point1ID, point2ID);
        }

        if (config.DisplayVectorSum)
        {
            int point1ID = grid.GetPointIDByName(config.vectorSum.Item1);
            int point2ID = grid.GetPointIDByName(config.vectorSum.Item2);
        }
    }

    void CheckConditions()
    {
        Condition condition = steps[ExerciceStepIndex].ExerciceSucessCondition;
        Vector2 param = steps[ExerciceStepIndex].ExerciceSucessParam;

        switch (condition) {
            case Condition.NoCondition:
                conditionMet = true;
                break;
            case Condition.PointAtCoord:
                conditionMet = grid.TryGetPointByCoordinates(param, out int i);
                break;
            case Condition.VectorAtCoord:
                conditionMet = grid.TryGetVectorByCoordinates(param, out int vectorID);
                break;
            case Condition.VectorialSumCoord:
                conditionMet = (grid.VectorSumCoords().Equals((Vector3)param));
                break;
        }
        if (conditionMet)
        {
            audioSource.PlayOneShot(steps[ExerciceStepIndex].audioSucess);
            EndExercice();
        }
        else
        {
            audioSource.PlayOneShot(steps[ExerciceStepIndex].audioFailure);
        }
    }
    void EndExercice()
    {
        ExerciceStepIndex++;
        conditionMet = false;
        if(ExerciceStepIndex < steps.Count) { StartCoroutine(playExerciceStep()); }
    }
}

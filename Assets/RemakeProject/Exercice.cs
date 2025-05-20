using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;




[System.Serializable]
public class TimedEvent
{
    public float timestamp;
    public ActionType actionType;
    
    public string actionParam;
    public float duration;

    public GridConfig gridConfig;
    public UIConfig uiConfig;
}
public enum ActionType {
    
    HighlightGrid,
    GrowVector,
    ChangeGridConfig,
    ChangeUIConfig,
    //CHANGEUI Config
    //ChangeUI information
}
public enum Condition
{
    NoCondition, PointAtCoord, VectorAtCoord, VectorialSumCoord, ChooseVector
}

[System.Serializable]
public class UIConfig
{
    public UIManager.UIConfig uiConfig;
    public int uiID = 0;
}
[System.Serializable] 
public class GridConfig
{
    public List<Vector2> points;
    public List<char> pointNames;
    public List<ExerciceVector> vectors;
    public bool DisplayVectorSum;
    public string vectorSum1;
    public string vectorSum2;
}
[System.Serializable]
public class ExerciceVector
{
    public string InitialPoint;
    public string TerminalPoint;
}

[System.Serializable]
public class ExerciceStep
{
    public Condition ExerciceSucessCondition;
    public Vector2 ExerciceSucessParam;
    public string SucessParamString = "";
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
    public int uiID = 0;
}
public class Exercice : MonoBehaviour
{
    public Grid2 grid;
    public TMP_Dropdown dropdown;
    public UIManager uiManager;

    public string scenarioTitle;
    public List<ExerciceStep> steps;
    public int ExerciceStepIndex = 0;
    public AudioSource audioSource;
    public bool conditionMet = false;

    private void Start()
    {
        StartCoroutine(StartSequence());
    }
    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(playExerciceStep());
    }
    bool CheckCondition(Vector3 coordinates, Condition condition, string param)
    {
        switch (condition)
        {
            case Condition.PointAtCoord:
                return grid.TryGetPointByCoordinates(coordinates, out int pointID);
            case Condition.VectorAtCoord:
                return grid.TryGetVectorByCoordinates(coordinates, out int vectorID);
            case Condition.VectorialSumCoord:
                return (grid.VectorSumCoords() == coordinates);
            case Condition.ChooseVector:
                return (dropdown.options[dropdown.value].text.Equals(param));
            default: return false;
        }
    }

    IEnumerator playExerciceStep()
    {
        ExerciceStep step = steps[ExerciceStepIndex];
        audioSource.PlayOneShot(step.audioConsigne);
        float t0 = 0f;
        float t1 = 0f;
        uiManager.config = step.exerciceUIConfig;
        uiManager.setID(step.uiID);
        if (step.changeGridConfig)
        {
            SetUpGrid(step.gridConfig);
        }
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
            t0 += Time.deltaTime;
        }
        if (step.audioSucess != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(step.audioSucess);
            yield return new WaitForSeconds(step.audioSucess.length);
        }
        EndExercice();
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
                Debug.Log("Grow Vector");
                bool vectorRetrieved = grid.GetVectorByName(timedEvent.actionParam, out int vectorID);
                grid.VectorZoom(vectorID, 1.5f, timedEvent.duration, 0.1f);
                break;

            case ActionType.ChangeGridConfig:
                SetUpGrid(timedEvent.gridConfig); break;
            case ActionType.ChangeUIConfig:
                SetUpUI(timedEvent.uiConfig); break;

        }
    }
    void SetUpUI(UIConfig uiConfig)
    {
        uiManager.config = uiConfig.uiConfig;
        uiManager.setID(uiConfig.uiID);
    }
    void SetUpGrid(GridConfig config)
    {
        grid.ClearGrid();
        if (config.points.Count > 0)
        {
            int i = 0;
            foreach (Vector2 point in config.points)
            {
                int nameID = -1;
                if (config.pointNames.Count > i)
                {
                    nameID = config.pointNames[i] - 'A';
                }
                grid.CreatePoint(point, nameID:nameID);
                i++;
            }
        }

        if (config.vectors.Count > 0)
        {
            foreach (ExerciceVector vector in config.vectors)
            {
                int point1ID = grid.GetPointIDByName(vector.InitialPoint);
                int point2ID = grid.GetPointIDByName(vector.TerminalPoint);
                if (point1ID == -1 || point2ID == -1) Debug.LogWarning("A point in your vector pair doesn't exist");
                else grid.CreateVector(point1ID, point2ID);
            }
        }

        if (config.DisplayVectorSum)
        {
            int point1ID = grid.GetPointIDByName(config.vectorSum1[0].ToString());
            int point2ID = grid.GetPointIDByName(config.vectorSum2[1].ToString());
            if (grid.GetVectorByName(config.vectorSum1, out int id1) && grid.GetVectorByName(config.vectorSum2, out int id2))
            {
                grid.VectorSum(id1, id2);
            }
            else
            {
                Debug.Log("Je ne trouve pas les vecteurs");
            }
        }
    }

    public void CheckConditions()
    {
        Condition condition = steps[ExerciceStepIndex].ExerciceSucessCondition;
        Vector2 param = steps[ExerciceStepIndex].ExerciceSucessParam;
        string stringParam = steps[ExerciceStepIndex].SucessParamString;

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
                case Condition.ChooseVector:
                conditionMet = CheckCondition(Vector3.zero, condition, stringParam);
                break;
        }
        if (conditionMet)
        {

        }
        else
        {
            audioSource.Stop();
            audioSource.PlayOneShot(steps[ExerciceStepIndex].audioFailure);
        }
    }
    void EndExercice()
    {
        ExerciceStepIndex++;
        if (ExerciceStepIndex >= 5 && dropdown != null) dropdown.gameObject.SetActive(true);
        conditionMet = false;
        audioSource.Stop();
        if(ExerciceStepIndex < steps.Count) { StartCoroutine(playExerciceStep()); }
    }

    public void ReplayExercice()
    {
        StopAllCoroutines();
        audioSource.Stop();
        StartCoroutine(playExerciceStep());
    }
    public void ReplayAudioConsigne()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(steps[ExerciceStepIndex].audioConsigne);
    }
}

using UnityEngine;
using System.Collections;
using TMPro;

public class DemoCreatePoint : MonoBehaviour
{
    int indexConsigne = -1;
    public Grid2 grid;
    public UIManager uiManager;
    public TMP_Text instructionText;
    public GameObject RightControllerTrigger;
    public GameObject RightControllerGrip;
    private string textExercice2 = "Tu peux déplacer et modifier les vecteurs et les points à l'aide du \"Grip\" qui se trouve sous ton majeur.\n" +
        "Essaie de déplacer le point B pour faire un vecteur plus grand";
    private string textExercice3 = "Dans cette démonstration, les consignes sont écrites sur ces panneaux, mais elles seront énoncés à l'oral dans la version finale.\n" +
        "Dans le prochain exercice, le vecteur vert est la somme du vecteur rouge et du vecteur bleu. Essaie de modifier les vecteurs rouges et bleus pour que la somme vectorielle soit (3;4)";
    private string currentConsigne = "";

    public IEnumerator exercice0()
    {
        yield return null;
    }
    public IEnumerator exercice1()
    {
        grid.ClearGrid();
        instructionText.text = "Creation de vecteur";
        RightControllerTrigger.SetActive(true);
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (grid.getVectorsNameDict().Count > 0)
            {
                yield return new WaitForSeconds(1.0f);
                StartConsigne(textExercice2);
                RightControllerTrigger.SetActive(false);
                break;
            }
        }
    }
    public IEnumerator exercice2()
    {
        grid.ClearGrid();
        instructionText.text = "Agrandir un vecteur";
        RightControllerGrip.SetActive(true);
        int A = grid.CreatePoint(Vector3.zero);
        int B = grid.CreatePoint(Vector3.right);
        int v1 = grid.CreateVector(A, B);
        while (true) {
            yield return new WaitForSeconds(0.1f);
            if(grid.VectorNorm(v1) > 1.5f)
            {
                RightControllerGrip.SetActive(false);
                yield return new WaitForSeconds(1.0f);
                StartConsigne(textExercice3);
                break;
            }
        }
    }
    public IEnumerator exercice3()
    {
        grid.ClearGrid();
        instructionText.text = "Somme Vectorielle";
        RightControllerGrip.SetActive(true);
        int A = grid.CreatePoint(Vector3.zero);
        int B = grid.CreatePoint(Vector3.right);
        int C = grid.CreatePoint(Vector3.up);
        int v1 = grid.CreateVector(A, B);
        int v2 = grid.CreateVector(B, C);
        yield return new WaitForEndOfFrame();
        grid.VectorSum(v1, v2);
        yield return new WaitForEndOfFrame();
        uiManager.config = UIManager.UIConfig.VectorSum;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            (int, int) vsums = grid.GetVectorSumIDS();
            if (Vector3.Equals(grid.VectorCoords(vsums.Item1) + grid.VectorCoords(vsums.Item2), new Vector3(3,4,0)))
            {
                RightControllerGrip.SetActive(false);
                yield return new WaitForSeconds(0.5f);
                StartConsigne("Félicitation, vous êtes arrivés au bout de la démo. Il y a encore d'autres fonctionnalités que ce tutoriel n'a pas couvert.\n" +
                    "Merci d'avoir pris le temps de participer !");
                break;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string text = "Bonjour, bienvenue dans la démonstration d'Incarmath !\nIncarmath est un outils d'apprentissage des mathématiques à l'aide de la réalité virtuelle." +
            "\nUtilise la gâchette pour passer à l'étape suivante !";
        StartConsigne(text);
    }
    

    void StartConsigne(string text)
    {
        indexConsigne++;
        ScenarioManager.instance.popUp(text);
    }

    public void EndConsigne()
    {
        int i = indexConsigne;
        switch (i)
        {
            case 0:
                StartConsigne("Le but est d'apprendre les vecteurs. Les vecteurs sont des objets représentés par des flèches et des nombres.\n" +
                    "Essaie de créer un vecteur en maintenant la gâchette appuyée et en bougeant ton curseur sur la grille.");
                break;
            case 1:

                StartCoroutine(exercice1());
                break;
            case 2:

                StartCoroutine(exercice2());
                break;
            case 3:

                StartCoroutine(exercice3());
                break;
            default:
                instructionText.text = "Mode Libre";
                StartCoroutine(freeMode());
                break;
        }
    }

    IEnumerator freeMode()
    {
        uiManager.config = UIManager.UIConfig.SolePoint;
        grid.ClearGrid();
        yield return new WaitForEndOfFrame();
        int A = grid.CreatePoint(Vector3.zero);
        int B = grid.CreatePoint(Vector3.right);
        int C = grid.CreatePoint(Vector3.up);
        int v1 = grid.CreateVector(A, B);
        int v2 = grid.CreateVector(B, C);
        yield return new WaitForEndOfFrame();
        grid.VectorSum(v1, v2);
        yield return new WaitForEndOfFrame();
        uiManager.config = UIManager.UIConfig.VectorSum;
        indexConsigne++;
    }
    public void restartExercice()
    {
        StopAllCoroutines();
        indexConsigne--;
        EndConsigne();
    }
    // Update is called once per frame
    void Update()
    {

    }
}

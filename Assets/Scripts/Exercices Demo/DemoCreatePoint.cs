using UnityEngine;
using System.Collections;

public class DemoCreatePoint : MonoBehaviour
{
    int indexConsigne = -1;
    public Grid2 grid;
    public UIManager uiManager;
    private string textExercice2 = "Tu peux déplacer et modifier les vecteurs et les points à l'aide du \"Grip\" qui se trouve sous ton majeur.\n" +
        "Essaie de déplacer le point B pour faire un vecteur plus grand";
    private string textExercice3 = "On peut voir les vecteurs comme des déplacements ou des trajets. Ils vont du début de la flèche à la fin de la flèche.\n" +
        "La somme vectorielle correspondrait au trajet du premier vecteur suivi du trajet du deuxième vecteur.\n" +
        "Dans le prochain exercice, le vecteur vert est la somme du vecteur rouge et du vecteur bleu. Essaie de modifier les vecteurs rouges et bleus pour que la somme vectorielle soit (3;4)";


    public IEnumerator exercice0()
    {
        yield return null;
    }
    public IEnumerator exercice1()
    {
        grid.ClearGrid();
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (grid.getVectorsNameDict().Count > 0)
            {
                StartConsigne(textExercice2);
                break;
            }
        }
    }
    public IEnumerator exercice2()
    {
        grid.ClearGrid();
        int A = grid.CreatePoint(Vector3.zero);
        int B = grid.CreatePoint(Vector3.right);
        int v1 = grid.CreateVector(A, B);
        while (true) {
            yield return new WaitForSeconds(0.1f);
            if(grid.VectorNorm(v1) > 1.5f)
            {
                yield return new WaitForSeconds(0.5f);
                StartConsigne(textExercice3);
                break;
            }
        }
    }
    public IEnumerator exercice3()
    {
        grid.ClearGrid();
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
                indexConsigne++;
                break;
            default:
                indexConsigne++;
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}

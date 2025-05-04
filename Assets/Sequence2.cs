using UnityEngine;

public class Sequence2 : MonoBehaviour
{

    int exerciseNumber = -1;
    int exercise0Index = 0;
    public Grid2 grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NextExercise();
    }


    public void NextExercise()
    {
        exerciseNumber++;
        StartExerciseByNumber(exerciseNumber);
    }

    void StartExerciseByNumber(int exerciseNumber)
    {
        switch (exerciseNumber)
        {
            case 0:
                StartExercise0();
                break;
            case 2:
                StartExercise2(); break;
            case 3:
                StartExercise3(); break;
            default:
                break;
        }

    }

    void StartExercise0()
    {
        grid.ClearGrid();
        int A = grid.CreatePoint(Vector3.zero);
        int B = grid.CreatePoint(Vector3.right + 2 * Vector3.up);
        int C = grid.CreatePoint(2* Vector3.left + Vector3.up);

        int AB = grid.CreateVector(A, B);
        int BC = grid.CreateVector(A, C);

        grid.VectorSum(AB, BC);
    }

    void StartExercise2()
    {

        //Change la création du prochain vecteur pour que la tête de flèche n'apparaisse pas
        //Fais apparaître un vecteur où tu veux dans le plan

        //Le segment AB qui est apparu défini une direction. C'est la direction du vecteur AB. Le segment AB possède une longueur, c'est la Norme du vecteur AB

        //Fait apparaître la tête du vecteur
        //Il existe deux vecteurs différents avec la même direction et la même norme.
        //Si la flèche va dans le sens de A vers B, il s'agit du vecteur AB

        //Tourne le vecteur à 180°
        //Si la flèche va dans le sens de B vers A, il s'agit du vecteur BA
        
        //En résumé, le vecteur possède 4 caractéristiques : son origine, sa norme, sa direction et son sens
        
    }

    void StartExercise3()
    {
        //Coordonnées


    }

    void FinishExercise()
    {
        NextExercise();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

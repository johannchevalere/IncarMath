using UnityEngine;
using System.Collections;

public class Sequence1 : MonoBehaviour
{

    enum Condition
    {
        PointAtCoord, VectorAtCoord, VectorialSumCoord
    }

    Condition condition;
    Vector3 coordinates = Vector3.zero;
    bool nextExercice = false;
    public AudioClip[] audioClips;
    int audioClipIndex = 0;
    public Grid2 grid;
    public AudioSource source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Exercise0());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckEndExercice()
    {

        
        if (CheckCondition())
        {
            nextExercice = true;
            source.PlayOneShot(audioClips[audioClipIndex]);
            audioClipIndex+= 2;
        }
        else
        {
            source.PlayOneShot(audioClips[audioClipIndex + 1]);
        }
    }
    bool CheckCondition() {
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

    void PlayClip()
    {
        source.PlayOneShot(audioClips[audioClipIndex]);
        audioClipIndex++;
    }

    IEnumerator Exercise0()
    {

        yield return new WaitForSeconds(2f);

        grid.ClearGrid();
        int O = grid.CreatePoint(Vector3.zero);
        int A = grid.CreatePoint(new Vector3(3, 3, 0));
        int B = grid.CreatePoint(new Vector3(2, -2, 0));
        int v1 = grid.CreateVector(O, A);
        int v2 = grid.CreateVector(O, B);
        grid.VectorSum(v1, v2);
        PlayClip();
        //Vecteur OA
        yield return new WaitForSeconds(1.5f);
        grid.VectorZoom(v1, 1.1f, 1f, 0.1f);
        yield return new WaitForSeconds(5.5f);
        grid.VectorZoom(v2, 1.1f, 1f, 0.1f);
        yield return new WaitForSeconds(17f);
        grid.ClearGrid();
        
        PlayClip();
        yield return new WaitForSeconds(8f);
        //Place le point O d'abscisse 0 et d'ordonnée 0
        nextExercice = false;
        condition = Condition.PointAtCoord;
        coordinates = Vector3.zero;
        yield return new WaitForSeconds(0.1f);
        while (!nextExercice) {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(7.5f);
        //Place le point M d'abscisse 3 et d'ordonnée 3
        coordinates = new Vector3(3, 3, 0);

        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }


        //Trace le vecteur OM
        yield return new WaitForSeconds(2f);
        nextExercice = false;
        condition = Condition.VectorAtCoord;
        coordinates = new Vector3(3, 3, 0);
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
        //Trace le vecteur OA
        nextExercice = false;

        condition = Condition.VectorAtCoord;
        coordinates = new Vector3(2, 2, 0);
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
        //Les deux vecteurs ont étés tracés

        //Faire apparaitre la somme vectorielle
        grid.ClearGrid();
        O = grid.CreatePoint(Vector3.zero);
        A = grid.CreatePoint(new Vector3(3, 3, 0));
        B = grid.CreatePoint(new Vector3(2, -2, 0));
        v1 = grid.CreateVector(O, A);
        v2 = grid.CreateVector(O, B);
        grid.VectorSum(v1, v2);


        //Vecteur OA
        yield return new WaitForSeconds(1f);
        grid.VectorZoom(v1, 1.1f, 0.5f, 0.1f);
        yield return new WaitForSeconds(2f);
        grid.VectorZoom(v2, 1.1f, 0.5f, 0.1f);
        yield return new WaitForSeconds(2f);
        //On souhaite maintenant que la somme vectorielle soit égale à 8, 6. Je te laisse manipuler les vecteurs OM et OA ...
        condition = Condition.VectorialSumCoord;
        coordinates = new Vector3(8, 6, 0);
        nextExercice = false;
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
        //
        coordinates = new Vector3(-9, -3, 0);
        nextExercice = false;
        while (!nextExercice)
        {
            //Exercice
        }
        //Maintenant la consigne va changer. Le vecteur OM est fixé à 3; 3
        grid.ClearGrid();
        O = grid.CreatePoint(Vector3.zero);
        A = grid.CreatePoint(new Vector3(3, 3, 0));
        B = grid.CreatePoint(new Vector3(2, -2, 0));
        v1 = grid.CreateVector(O, A);
        v2 = grid.CreateVector(O, B);
        grid.VectorSum(v1, v2);
        //grid.FixVector(v1)


        coordinates = new Vector3(-1, 5, 0);
        nextExercice = false;
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
        //
        coordinates = new Vector3(6, 1, 0);
        nextExercice = false;
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
        //Tu as face à toi 2 vecteurs et tu dois tracer la somme vectorielle de ces deux vecteurs à la main
        grid.ClearGrid();
        O = grid.CreatePoint(Vector3.zero);
        A = grid.CreatePoint(new Vector3(-4, -5, 0));
        B = grid.CreatePoint(new Vector3(7, -3, 0));
        v1 = grid.CreateVector(O, A);
        v2 = grid.CreateVector(O, B);
        //grid.FixVector(v1);
        //grid.FixVector(v2);
        nextExercice = false;
        while (!nextExercice)
        {
            //Exercice
            yield return new WaitForSeconds(0.1f);
        }
    }
}

using UnityEngine;

public class Sequence0 : MonoBehaviour
{

    int exerciseNumber = -1;
    int exercise0Index = 0;
    public GameObject intersectionBleue;
    public GameObject intersectionRouge;
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
            case 1:
                StartExercise1(); 
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
        //Introduction
        //Désactive la plupart des fonctionnalité d'interactions et des objets de la scène (ils apparaitront progressivement)
        
        //Faire apparaite le mur blanc
        
        //Faire apparaitre les boutons

        //Faire apparaite l'UI

        //Tu vas maintenant réaliser une série de tâche pour te familiariser avec l'environnement

        //Exercice suivant
        NextExercise();
    }
    void StartExercise1()
    {
        //Passe le pointeur sur la grille

        //Débloque la fonctionalité de créer des points
        //Enable intersection en bleu
        intersectionBleue.SetActive(true);
        //"Fait apparaitre un point sur l'intersection en bleu"

        //Boucle while tant que l'élève n'a pas mis le point en bleu. Après un certain temps, relance la consigne à l'audio ?

        intersectionBleue.SetActive(false);
        //Next step : Créer un vecteur
        //Désactive la possibilité de créer des points
        //Fait apparaître une intersection en rouge en D5
        intersectionRouge.SetActive(true);
        //"Pars du point que tu viens de créer et fais glisser le pointeur tout en maintenant la gâchette enfoncée, jusqu'à l'intersection rouge."

        //Boucle while. Si mauvaise position, on supprime le vecteur et on lance une autre formule audio "Réessaye"

        //"Tu viens de réaliser la translation qui transforme A en B. On dit que B est l'image de A par la translation du vecteur AB qui s'étire du point ..."

        //Redonne la possibilité de créer des points
        //"Tu peux supprimer un vecteur avec le bouton A et déplacer/étirer les vecteur avec le Grip"


        //Fait apparaitre le bouton continuer
        //30 secondes de disponibles/Si l'élève appuie sur le bouton continuer

        //Fin de l'exercice
        NextExercise();


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

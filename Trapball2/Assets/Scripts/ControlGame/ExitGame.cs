using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        // Cierra la aplicaci�n
        Application.Quit();
        // Si est�s en el editor de Unity, esto detendr� la ejecuci�n del juego
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

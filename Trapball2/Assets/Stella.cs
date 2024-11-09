using UnityEngine;
using System.Collections;

public class Stella : MonoBehaviour
{
    private GameObject player;
    private float bottomOffset = 0.1f;  // Distancia desde el centro hasta la parte inferior (aj�stala seg�n tu personaje)
    private float scaleSpeed = 9f; // Velocidad de escalado
    private Vector3 targetScale = new Vector3(1, 1, 1); // Escala final
    private Vector3 startScale = new Vector3(1, 0, 1); // Escala inicial
    private bool isDeactivating = false; // Controla si est� en proceso de desactivarse
    private float rotationSmoothing = 5f; // Velocidad de interpolaci�n para la rotaci�n

    private void Awake()
    {
        player = transform.parent.gameObject;
        transform.localScale = startScale; // Empieza con escala m�nima
    }

    private void Update()
    {
        // Solo ejecuta la l�gica de crecimiento si no est� en desactivaci�n
        if (!isDeactivating)
        {
            // Ajustar la rotaci�n para que siga la rotaci�n del jugador en el eje Y
            Quaternion targetRotation = Quaternion.Euler(0, player.transform.rotation.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);

            // Mant�n la posici�n del plano ligeramente por debajo del jugador
            transform.position = new Vector3(
                player.transform.position.x,
                player.transform.position.y - bottomOffset,
                player.transform.position.z
            );

            // Incremento gradual de la escala con MoveTowards
            transform.localScale = new Vector3(
                transform.localScale.x,
                Mathf.MoveTowards(transform.localScale.y, targetScale.y, Time.deltaTime * scaleSpeed),
                transform.localScale.z
            );
        }
    }

    public void active()
    {
        gameObject.SetActive(true);
        isDeactivating = false; // Asegura que pueda crecer
        transform.SetParent(null);
        transform.localScale = startScale; // Resetea la escala al activarse
        transform.rotation = Quaternion.Euler(0, player.transform.rotation.eulerAngles.y, 0);
        transform.position = new Vector3(
            player.transform.position.x,
            player.transform.position.y - bottomOffset,
            player.transform.position.z
        );
    }

    public void desActive()
    {
        // Inicia la corrutina para el escalado inverso y marca como desactivando
        isDeactivating = true;
        StartCoroutine(ScaleDownAndDeactivate());
    }

    private IEnumerator ScaleDownAndDeactivate()
    {
        // Reduce la escala gradualmente hacia startScale
        while (transform.localScale.y > startScale.y)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                Mathf.MoveTowards(transform.localScale.y, startScale.y, Time.deltaTime * scaleSpeed),
                transform.localScale.z
            );

            yield return null; // Espera al siguiente frame
        }

        // Forzar la escala a cero en Y si se queda en un valor cercano
        transform.localScale = startScale;

        // Finalmente, desactiva el objeto
        gameObject.SetActive(false);
        transform.SetParent(player.transform); // Vuelve a asignar el padre si es necesario
    }
}

using UnityEngine;
using System.Collections;

public class HydraulicLift : MonoBehaviour
{
    public Transform piston; // El objeto cil�ndrico que act�a como pist�n.
    public float speed = 0.5f; // Velocidad de expansi�n y contracci�n.
    public float speedUp = 0.5f; // Velocidad de expansi�n y contracci�n.
    public float maxHeight = 2.54f; // Altura m�xima del pist�n.
    public float minHeight = 1f; // Altura m�nima (inicial) del pist�n.

    public bool isExpanding = true; // Para alternar entre expansi�n y contracci�n.
    public bool velocityNormal = true;
    public bool isDown = false;
    public bool isUp = false;
    public bool isHold = false;
    public float timeSecondsHold = 5f;

    private void Start()
    {
        piston = transform;
    }
    void Update()
    {
        float newYScale = piston.localScale.y;

        if (isExpanding)
        {
            newYScale += (speedUp * (velocityNormal ? 1 : 0.25f)) * Time.deltaTime;
            isDown = false;
            if (newYScale >= maxHeight)
            {
                newYScale = maxHeight;
                isUp = true;
            }
        }
        else
        {
            newYScale -= speed * Time.deltaTime;
            isUp = false;
            if (!isDown && newYScale <= minHeight)
            {
                newYScale = minHeight;
                isHold = true;
                StartCoroutine(delayHold());
            }
        }

        piston.localScale = new Vector3(piston.localScale.x, newYScale, piston.localScale.z);
    }
    public bool isNearDownHydraulic()
    {
        return !isExpanding && (piston.localScale.y <= minHeight + 0.65f);
    }

    public bool isMinUpHydraulic()
    {
        return isExpanding && (piston.localScale.y >= minHeight + 0.65f);
    }

    IEnumerator delayHold()
    {
        yield return new WaitForSeconds(timeSecondsHold);
        isHold = false;
        isDown = true;
    }
}


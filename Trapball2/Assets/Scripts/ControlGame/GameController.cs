using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    private Gamepad gamepad;
    public float lowFrequency = 0.5f; // Frecuencia baja
    public float highFrequency = 0.5f; // Frecuencia alta
    public float duration = 1.0f; // Duraci�n de la vibraci�n
    public float minRumbleIntensity = 0.1f; // Intensidad m�nima de vibraci�n
    public float maxRumbleIntensity = 0.5f; // Intensidad m�xima de vibraci�n


    void Start()
    {
        if (Gamepad.all.Count > 0)
        {
            gamepad = Gamepad.all[0];
            OnEnable();
        }
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos de conexi�n y desconexi�n del mando.
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        // Desuscribirse de los eventos para evitar llamadas a funciones ya destruidas.
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Disconnected:
                if (device is Gamepad)
                {
                    gamepad = null;
                    // Aqu� puedes agregar l�gica adicional, como pausar el juego, mostrar un mensaje, etc.
                }
                break;

            case InputDeviceChange.Reconnected:
                if (device is Gamepad)
                {
                    Debug.Log("Un mando se ha reconectado");
                    gamepad = Gamepad.all[0];
                    // Aqu� puedes agregar l�gica adicional, como reanudar el juego si estaba pausado.
                }
                break;

                // Puedes manejar otros eventos como InputDeviceChange.Added, InputDeviceChange.Removed, etc.
        }
    }
    public void StartRumble()
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);
            Invoke(nameof(StopRumble), duration);
        }
    }

    public void StopRumble()
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0, 0);
        }
    }

    public float NormalizeValue(float currentValue, float minValue, float maxValue)
    {
        float normalizedValue = (currentValue - minValue) / (maxValue - minValue);
        return Mathf.Clamp(normalizedValue, minRumbleIntensity, maxRumbleIntensity);
    }


    public void ApplyRumble(float currentValue, float duration)
    {
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(currentValue, currentValue);
            if (duration > 0)
            {
                Invoke(nameof(StopRumble), duration);
            }
        }
    }


}


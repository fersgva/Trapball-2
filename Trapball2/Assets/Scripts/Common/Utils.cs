using UnityEngine;

public class Utils 
{
    public static bool IsCollisionAbove(Collision collision, float positionY)
    {
        // Obtener la posici�n en Y de tu objeto
        float myYPosition = positionY;

        // Obtener la posici�n en Y de la caja
        float otherObjectYPosition = collision.gameObject.transform.position.y + 0.35f;

        // Comparar las posiciones en Y
        return myYPosition > otherObjectYPosition;
    }
    public static bool IsCollisionAboveEnemies(Collision collision, float positionY)
    {
        // Obtener la posici�n en Y de tu objeto
        float myYPosition = positionY;

        // Obtener la posici�n en Y de la caja
        float otherObjectYPosition = collision.gameObject.transform.position.y;

        // Comparar las posiciones en Y
        return myYPosition > otherObjectYPosition;
    }

    public static float limitValue(float value, float maxValue)
    {
        if (value > maxValue)
        {
            value = maxValue;
        }
        return value;
    }
}

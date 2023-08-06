using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class BallHud : MonoBehaviour
{
    private Image targetImage;
    public Sprite initialSprite;
    public Sprite damageSprite;
    public Sprite limitLiveSprite;
    public Sprite deadSprite;
    public Sprite attackSprite;
    public Color flashColor = Color.red;
    public float flashDuration = 0.5f;
    public int flashCount = 2;
    private Color originalColor;
    public GameObject live;
    public GameObject energy;


    private Player player;
    private StatePlayer statePlayer;
    private Vector3 initialPosition;
    private Transform transform;
    private Image imageLive;
    private Image imageEnergy;

    public Sprite[] spritesLive;
    public Sprite[] spritesEnergy;

    private float limitEnergy = 0;
    private float lowLimitEnergy = 0;
    private float limitBombJump = 0;


    private int limitLive = 0;
    private int limitMaxLive = 0;
    private int bufferLive = 9;

    private bool damage = false;
    Coroutine myCoroutineDamage;


    void Start()
    {
        transform = GetComponent<Transform>();
        initialPosition = transform.position;
        targetImage = GetComponent<Image>();
        originalColor = targetImage.color;
        imageLive = live.GetComponent<Image>();
        imageEnergy = energy.GetComponent<Image>();    
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.GetComponent<Player>();
                statePlayer = player.state;
                limitMaxLive = player.live;
                bufferLive = limitMaxLive;
            }
        }
        if (player != null)
        {
            setPlayerStateImage(player.state, player.getJumpForce());
            if (limitEnergy == 0)
            {
                limitEnergy = player.getJumpLimit();
                lowLimitEnergy = player.getJumpLowLimit() - (limitEnergy * 0.05f);
                limitBombJump = player.getJumpBombLimit();
            }
            setPlayerEnergy(player.getJumpForce());
            setPlayerLive(player.live);

        }

    }

    private void setPlayerEnergy(float jumpForce)
    {
        // Primero, normalizamos el valor de energ�a entre 0 y 1
        float normalizedEnergy = (jumpForce - lowLimitEnergy) / (limitEnergy - lowLimitEnergy);

        // Luego, lo escalamos al rango de �ndices de nuestros gr�ficos (0 a 8)
        int spriteIndex = Mathf.RoundToInt(normalizedEnergy * (spritesEnergy.Length - 1));

        // Nos aseguramos de que el �ndice est� en el rango correcto
        spriteIndex = Mathf.Clamp(spriteIndex, 0, spritesEnergy.Length - 1);

        // Finalmente, establecemos el sprite de la barra de energ�a
        imageEnergy.sprite = spritesEnergy[spriteIndex];
    }
    private void setPlayerLive(float live)
    {
        // Primero, normalizamos el valor de energ�a entre 0 y 1
        float normalizedLive = 1 - ((live - limitLive) / (limitMaxLive - limitLive));

        // Luego, lo escalamos al rango de �ndices de nuestros gr�ficos (0 a 8)
        int spriteIndex = Mathf.RoundToInt(normalizedLive * (spritesLive.Length - 1));

        // Nos aseguramos de que el �ndice est� en el rango correcto
        spriteIndex = Mathf.Clamp(spriteIndex, 0, spritesLive.Length - 1);

        // Finalmente, establecemos el sprite de la barra de energ�a
        imageLive.sprite = spritesLive[spriteIndex];
    }

    private void setPlayerStateImage(StatePlayer state, float jumpForce)
    {
        bool damagePlayer = bufferLive != player.live && player.live > 0;
        if (state == StatePlayer.DEAD)
        {
            StopCoroutine(myCoroutineDamage);
            damage = false;
        }
        if (damagePlayer) {
            bufferLive = player.live;
            damage = true;
            setImageDamage();
        } else if (statePlayer != state && !damage)
        {
            switch (state)
            {
                case StatePlayer.NORMAL:
                    setStateNormal();
                    break;
                case StatePlayer.JUMP:
                    setHiddenLiveEnergy(true);
                    if (jumpForce > limitBombJump)
                    {
                        targetImage.color = originalColor;
                        targetImage.sprite = attackSprite;
                    }
                    break;
                case StatePlayer.END_BOMB_JUMP:
                case StatePlayer.BOMBJUMP:
                    targetImage.color = originalColor;
                    targetImage.sprite = attackSprite;
                    break;
                case StatePlayer.DEAD:
                    setHiddenLiveEnergy(false);
                    targetImage.color = originalColor;
                    targetImage.sprite = deadSprite;
                    transform.position = new Vector3(initialPosition.x, initialPosition.y - 20);
                    StartCoroutine(MoveDiagonally());
                    break;

            }
            statePlayer = state;
        }
    }

    private Sprite imageDependLive()
    {
        if (player != null && player.live < 3)
        {
            return limitLiveSprite;
        } else
        {
            return initialSprite;
        }   
    }

    private void setStateNormal()
    {
        setHiddenLiveEnergy(true);
        targetImage.color = originalColor;
        targetImage.sprite = imageDependLive();
        transform.position = initialPosition;
    }

    private void setHiddenLiveEnergy(bool value)
    {
        live.SetActive(value);
        energy.SetActive(value);
    }


    IEnumerator MoveDiagonally()
    {
        // Definimos la direcci�n de movimiento y la distancia
        Vector3 moveDirection = new Vector3(1, 1, 0);
        float moveDistance = 100f;

        // Calculamos la posici�n final
        Vector3 finalPosition = transform.position + moveDirection * moveDistance;

        // Duraci�n de la animaci�n en segundos
        float animationDuration = 1f;

        // Guardamos la posici�n inicial
        Vector3 initialPosition = transform.position;

        Color initialColor = targetImage.color;
        Color finalColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0); // Alpha 0 para transparencia total

        float timer = 0;

        while (timer < animationDuration)
        {
            // Interpolamos entre la posici�n inicial y la final
            transform.position = Vector3.Lerp(initialPosition, finalPosition, timer / animationDuration);

            // Interpolamos entre el color inicial y el final
            targetImage.color = Color.Lerp(initialColor, finalColor, timer / animationDuration);

            // Avanzamos el tiempo
            timer += Time.deltaTime;

            // Esperamos hasta el pr�ximo frame
            yield return null;
        }

        // Nos aseguramos de que la posici�n final es la correcta y que el color es el final
        transform.position = finalPosition;
        targetImage.color = finalColor;
    }



    private void setImageDamage()
    {
        targetImage.sprite = damageSprite;
        myCoroutineDamage = StartCoroutine(FlashColor());
    }

    IEnumerator FlashColor()
    {
        float timer = 0;
        foreach (int i in Enumerable.Range(0, flashCount))
        {
            timer = 0;

            while (timer < flashDuration)
            {
                // Cambia el color suavemente al originalColor
                targetImage.color = Color.Lerp(flashColor, originalColor, timer / flashDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0;
            while (timer < flashDuration)
            {
                // Cambia el color suavemente al flashColor
                targetImage.color = Color.Lerp(originalColor, flashColor, timer / flashDuration);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        damage = false;
        setStateNormal();
    }
}



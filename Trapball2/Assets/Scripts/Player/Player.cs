﻿using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IResettable
{
    // PUBLIC 
    public const string TAG = "Player";
    [HideInInspector] public Rigidbody rb;

    public float jumpForce;
    public Vector2 velocityBall;
    public int live = 8;
    public int valor = 0;
    private int liveInit;

    public StatePlayer state = StatePlayer.NONE;
    private StateSoundImpactPlayer stateImpactTerrain = StateSoundImpactPlayer.NONE;


    public bool isRumbleActive = false;
    public GameController gameController;
    public bool especialStage = false;

    public float positionInitial;
    public float positionFinal;
    public bool ejeX;
    public float percentStage;

    // PRIVATE
    [SerializeField] private PhysicMaterial BOUNCY;
    [SerializeField] private const float JUMP_DELTA = 5; //Define cuánto de rápido se alcanza el límite de fuerza de salto.
    [SerializeField] private const float JUMP_LIMIT = 10; //Define la mayor fuerza de salto posible a aplicar.
    [SerializeField] private float initGravityFactor;
    [SerializeField] private LayerMask jumpable;

    private float movementPlayer;
    private float movementPlayerExpecialZ;
    private const float SPEED_VELOCITY_LIMIT = 5f;
    private const float MOVEMENT_FORCE = 10f;
    private ParticlesExplosion particles;
    private SphereCollider coll;

    private FMOD.Studio.EventInstance playerSoundroll;
    private FMOD.Studio.EventInstance impactFloor;
    private FMOD.Studio.EventInstance exitTerrain;
    private FMOD.Studio.EventInstance impactBombFloor;
    private FMOD.Studio.EventInstance impactObjetc;
    private FMOD.Studio.EventInstance underWater;
    private FMOD.Studio.EventInstance soundCourage;

    private const float JUMP_LOW_PERCENT = 0.70f;
    private const float JUMP_LOW_BOMB_PERCENT = 0.85f;
    private const float JUMP_LOW_LIMIT = JUMP_LIMIT * JUMP_LOW_PERCENT;
    private const float JUMP_LOW_LIMIT_BOMB = JUMP_LIMIT * JUMP_LOW_BOMB_PERCENT;

    private bool jumpCharge = false;

    private bool debugger = false;

    private bool isBalancin = false;
    private bool jumpBombEnabled = false;

    private FMODUnity.StudioEventEmitter emitter;

    private float currentGravityFactor; //Añade un extra de gravedad para saltos más fluidos y rápidos. Tener en cuenta: A mayor factor, más nos costará saltar --> Incrementar jumpLimit
    private bool freeFall;
    private CameraShake camShakeScript;
    private FMODConstants.MATERIAL materialActual = FMODConstants.MATERIAL.NONE;


    void Awake()
    {
        liveInit = live;
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<SphereCollider>();
        particles = transform.GetChild(0).GetComponent<ParticlesExplosion>();
        currentGravityFactor = initGravityFactor;
        resetJumpForce();
    }

    void Start()
    {
        state = StatePlayer.JUMP;
        camShakeScript = GameManager.gM.cam.GetComponent<CameraShake>();
        
        playerSoundroll = FMODUtils.createInstance(FMODConstants.MOVE.PLAYER_ROLL);
        impactFloor = FMODUtils.createInstance(FMODConstants.JUMPS.IMPACT_TERRAIN_PLAYER);
        impactBombFloor = FMODUtils.createInstance(FMODConstants.JUMPS.IMPACT_TERRAIN_BOMB);
        exitTerrain = FMODUtils.createInstance(FMODConstants.JUMPS.EXIT_TERRAIN_PLAYER);
        impactObjetc = FMODUtils.createInstance(FMODConstants.OBJECTS.IMPACT_OBJECT_PLAYER);
        underWater = FMODUtils.createInstance(FMODConstants.AMBIENT.UNDER_WATER);
        soundCourage = FMODUtils.createInstance(FMODConstants.OBJECTS.GRAB_MOON);
        emitter = Camera.main.GetComponent<FMODUnity.StudioEventEmitter>();
        playerSoundroll.start();
    }

    void FixedUpdate() {
        movementPlayer = especialStage ? 60 : movementPlayer;
        rb.AddForce(new Vector3(movementPlayer, 0, movementPlayerExpecialZ) * MOVEMENT_FORCE, ForceMode.Force); //Para movimiento.
        manageExtraGravity();
        if (!freeFall) {
            ManageBallSpeed();
        }
        velocityBall = new Vector2(rb.velocity.x, rb.velocity.y);
        playerSoundroll.setParameterByName(FMODConstants.SPEED, velocityBall.x);
    }

    void Update() {
        if (state != StatePlayer.DEAD && state != StatePlayer.FINISH)
        {
            processJumpForce();
            checkTouchingFloor();
            checkSoundRoll();
            checkRotations();
            percentStage = getPercentPositionPlayer();
            emitter.SetParameter(FMODConstants.PERCENT_STAGE, percentStage);
        }
    }

    private void checkRotations()
    {
        if (!especialStage && (rb.rotation.x != 0.000f || rb.rotation.y != 0.000f))
        {
            rb.rotation = new Quaternion(0.0f, 0.0f, rb.rotation.z, rb.rotation.w);
        }
    }
    void checkTouchingFloor()
    {
        if (rb.velocity.y <= 0 && state != StatePlayer.INIT_JUMP)
        {
            if (isColliderPlatforms())
            {
                collisionFloor();
            }
            else if (state == StatePlayer.NORMAL)
            {
                state = StatePlayer.INIT_FALL;
            } else if (state == StatePlayer.INIT_FALL)
            {
                StartCoroutine(setStateFallThreadDelay());
            }
            else if (state == StatePlayer.FALL)
            {
                state = StatePlayer.JUMP;
            }
        }
        else if (rb.velocity.y > 3 && state == StatePlayer.NORMAL && !isColliderPlatforms())
        {
            resetJumpForce();
            state = StatePlayer.JUMP;
        }
    }

    private bool isColliderPlatforms()
    {
        Vector3 centralOffset = new(0, -0.35f, 0);
        Vector3 leftOffset = new(-0.35f, -0.35f, 0);
        Vector3 rightOffset = new(0.35f, -0.35f, 0);
        Collider[] centralColls = Physics.OverlapSphere(transform.position + centralOffset, 0.1f, jumpable.value);
        bool result = centralColls.Length > 0;
        if (!result)
        {
            // Si no hay colisión en el centro, entonces verificamos los lados
            Collider[] leftColls = Physics.OverlapSphere(transform.position + leftOffset, 0.1f, jumpable.value);
            Collider[] rightColls = Physics.OverlapSphere(transform.position + rightOffset, 0.1f, jumpable.value);
            result = leftColls.Length > 0 && rightColls.Length > 0;
        }
        return result;
    }

    private void collisionFloor()
    {
        if (state == StatePlayer.BOMBJUMP)
        {
            setStateEndBombJump();
        }
        else if (state != StatePlayer.BOMBJUMP && state != StatePlayer.NORMAL)
        {
            setStateNormal();
        }
    }

    private void setStateImpactTerrain(StateSoundImpactPlayer state)
    {
        if (stateImpactTerrain == StateSoundImpactPlayer.NONE)
        {
            stateImpactTerrain = state;
        }
    }

    void processJumpForce() {
        switch (state)
        {
            case StatePlayer.NORMAL:
                if (jumpCharge && jumpForce < JUMP_LIMIT)
                {
                    jumpForce += JUMP_DELTA * Time.deltaTime;
                    if (jumpForce > JUMP_LIMIT)
                    {
                        jumpForce = JUMP_LIMIT;
                    }
                }
                break;
        }
    }

    void handleButtonDown()
    {
        jumpCharge = true;
        switch (state)
        {
            case StatePlayer.NORMAL:
                
                break;
            case StatePlayer.JUMP:
                if (jumpBombEnabled)
                {
                    setStateBombJump();
                }
                break;
        }
    }
    void handleButtonUp() {
        jumpCharge = false;
        switch (state) {
            case StatePlayer.INIT_FALL:
            case StatePlayer.FALL:
            case StatePlayer.NORMAL:
                adjustJumpForce();
                simpleJump();
                break;
            case StatePlayer.JUMP:
                resetJumpForce();
                break;
        }
    }

    void adjustJumpForce() {
        if (jumpForce > 0) {
            jumpForce = Mathf.Clamp(jumpForce, JUMP_LOW_LIMIT, JUMP_LIMIT);
        }
    }

    public void resetJumpForce()
    {
        jumpForce = JUMP_LOW_LIMIT - 1;
    }

    void simpleJump() {
        state = StatePlayer.INIT_JUMP;
        if (jumpForce < JUMP_LOW_LIMIT)
        {
            jumpForce = JUMP_LOW_LIMIT;
        }
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse); //Para salto.
        playerSoundroll.setVolume(0);
        float playSoundProbability = 0.65f;  // 80%

        if (Random.Range(0f, 1f) < playSoundProbability)
        {
            if (jumpForce > JUMP_LOW_LIMIT_BOMB)
            {
                FMODUtils.playOneShot(FMODConstants.JUMPS.HIGH, transform.position);
            }
            else
            { 
                FMODUtils.playOneShot(FMODConstants.JUMPS.LOW, transform.position);
            }
        }
        StartCoroutine(setStateJumpThreadDelay());
    }

    void setStateBombJump() {
        state = StatePlayer.BOMBJUMP;
        jumpBombEnabled = false;
        coll.material = BOUNCY; //Le ponemos un material rebotante.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; //Cambiamos a dinámico por si atraviesa.
        rb.AddForce(Vector3.down * 15f, ForceMode.Impulse);
        FMODUtils.playOneShot(FMODConstants.JUMPS.JUMP_BOMB, transform.position);
        playerSoundroll.setVolume(0);
    }
    private void setStateEndBombJump()
    {
        if (isRumbleActive)
        {
            gameController.ApplyRumble(1f, 0.15f);
        }
        StartCoroutine(camShakeScript.Shake(0.15f, 0.15f));
        coll.material = null;
        setStateImpactTerrain(StateSoundImpactPlayer.IMPACT_BOMB_TERRAIN);
        playerSoundroll.setVolume(1);
        state = StatePlayer.END_BOMB_JUMP;
        StartCoroutine(stateNormalThreadDelay());
    }

    private void setStateNormal()
    {
        state = StatePlayer.NORMAL;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete; //Volvemos a discreto para consumir menos recursos.
        playerSoundroll.setVolume(1);
        jumpBombEnabled = false;
    }

    IEnumerator setStateJumpThreadDelay() {
        yield return new WaitForSeconds(0.15f);
        state = StatePlayer.JUMP;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; //Volvemos a discreto para consumir menos recursos.
        if (jumpForce > JUMP_LOW_LIMIT_BOMB)
        {
            jumpBombEnabled = true;
        }
        resetJumpForce();
    }
    IEnumerator setStateFallThreadDelay()
    {
        yield return new WaitForSeconds(0.1f);
        state = StatePlayer.FALL;
    }
    IEnumerator stateNormalThreadDelay()
    {
        yield return new WaitForSeconds(0.5f);
        setStateNormal();
    }

    private void checkSoundRoll()
    {
        float valueSoundRoll = 0;
        playerSoundroll.getParameterByName("onTheFloor", out valueSoundRoll);
        if (state == StatePlayer.NORMAL && valueSoundRoll == 0)
        {
            playerSoundroll.setParameterByName("onTheFloor", 1);
        }
        else if (state != StatePlayer.NORMAL && valueSoundRoll == 1)
        {
            playerSoundroll.setParameterByName("onTheFloor", 0);
        }
    }

    void manageExtraGravity() {
        if (state != StatePlayer.NORMAL) {
            Vector3 vel = rb.velocity;
            vel.y -= currentGravityFactor * Time.fixedDeltaTime;
            rb.velocity = vel;
        }
    }
    void ManageBallSpeed() {
        //Límite de velocidad
        if (Mathf.Abs(rb.velocity.x) > SPEED_VELOCITY_LIMIT) {
            rb.velocity = new Vector3(SPEED_VELOCITY_LIMIT * Mathf.Sign(rb.velocity.x), rb.velocity.y, rb.velocity.z);
        }
        //Ayuda para que no cueste tanto dejar la bola quieta
        //Si la bola a penas se mueve, no hay input de usuario y no está en una rampa (y == 0 + TouchingFloor) se parará por completo.
        else if (Mathf.Abs(rb.velocity.x) < 0.3f && movementPlayer == 0 && rb.velocity.y == 0) {
            rb.velocity = new Vector3(0, rb.velocity.y, rb.velocity.z);
        }
    }

    float getPercentPositionPlayer()
    {
        // Obtén la posición actual del jugador
        float playerPosition;
        if (ejeX)
        {
            playerPosition = transform.position.x;
        }
        else
        {
            playerPosition = transform.position.y;
        }

        // Calcula el porcentaje
        float totalDistance = positionFinal - positionInitial;
        float playerProgress = playerPosition - positionInitial;

        // Asegúrate de que el valor esté entre 0 y 100
        float percent = (playerProgress / totalDistance) * 100;
        return Mathf.Clamp(percent, 0f, 100f);
    }


    private void OnCollisionEnter(Collision collision) {
        string tag = collision.gameObject.tag;
        float yVelocity = collision.relativeVelocity.y;
        impactFloor.setParameterByName(FMODConstants.SPEED, yVelocity);
        switch (tag) {
            case "SueloPantanoso":
                if (yVelocity > 1)
                {
                    setStateImpactTerrain(StateSoundImpactPlayer.IMPACT_TERRAIN);
                }
                setTerrainParametersAndStart(FMODConstants.MATERIAL.MUD);
                break;
            case "Balancin":
            case "SueloMadera":
            case "Box":
                if (Utils.IsCollisionAbove(collision, transform.position.y))
                {
                    if (yVelocity > 1)
                    {
                        setStateImpactTerrain(StateSoundImpactPlayer.IMPACT_TERRAIN);
                    }

                    setTerrainParametersAndStart(FMODConstants.MATERIAL.WOOD);
                } else
                {
                    float collisionForce = Utils.limitValue(collision.relativeVelocity.magnitude + 3, FMODConstants.LIMIT_SOUND_VALUE);
                    impactObjetc.setParameterByName(FMODConstants.SPEED, collisionForce);
                    impactObjetc.start();
                    impactObjetc.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
                break;
            case "SueloPiedra":
                setTerrainParametersAndStart(FMODConstants.MATERIAL.STONE);
                break;
            default:
                // Handle other cases or do nothing
                break;
        }

        switch (tag)
        {
            case "Balancin":
                isBalancin = true;
                break;
            default:
                // Handle other cases or do nothing
                break;
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        string tag = collision.gameObject.tag;

        switch (tag)
        {
            case "Balancin":
            case "SueloMadera":
            case "Box":
                if (materialActual != FMODConstants.MATERIAL.WOOD && Utils.IsCollisionAbove(collision, transform.position.y))
                {
                    setTerrainParametersAndStart(FMODConstants.MATERIAL.WOOD);
                }

                break;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        string tag = collision.gameObject.tag;

        switch (tag)
        {
            case "Balancin":
                isBalancin = false;
                break;
            default:
                // Handle other cases or do nothing
                break;
        }
    }

    private void setTerrainParametersAndStart(FMODConstants.MATERIAL material) {
        playerSoundroll.setParameterByName(FMODConstants.TERRAIN, (int) material);
        if (materialActual != material)
        {
            materialActual = material;
            playerSoundroll.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            playerSoundroll.start();
        }
        if (stateImpactTerrain == StateSoundImpactPlayer.IMPACT_BOMB_TERRAIN)
        {
            impactBombFloor.setParameterByName(FMODConstants.TERRAIN, (int) material);
            impactBombFloor.start();
            stateImpactTerrain = StateSoundImpactPlayer.NONE;
        }
        else if (stateImpactTerrain == StateSoundImpactPlayer.IMPACT_TERRAIN)
        {
            impactFloor.setParameterByName(FMODConstants.TERRAIN, (int) material);
            impactFloor.start();
            stateImpactTerrain = StateSoundImpactPlayer.NONE;
        }
    }
    
    private void setTerrainParametersToExitMaterialAndStart(FMODConstants.MATERIAL material) {
        underWater.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        exitTerrain.setParameterByName(FMODConstants.TERRAIN, (int) material);
        exitTerrain.start();
    }


    private void OnTriggerEnter(Collider other) {
        string tag = other.tag;
        switch (tag) {
            case "Water":
                collisionFloor();
                setStateImpactTerrain(StateSoundImpactPlayer.IMPACT_TERRAIN);
                rb.velocity = new Vector3(rb.velocity.x, -0.5f, rb.velocity.z);
                setTerrainParametersAndStart(FMODConstants.MATERIAL.WATER);
                underWater.start();
                break;
            case "TubeEnter":
                freeFall = true;
                break;
            case "TubeExit":
                freeFall = false;
                break;
            case "Courage":
                valor++;
                soundCourage.start();
                break;
            case "Exit":
                state = StatePlayer.FINISH;
                rb.velocity = new Vector3(0, 0, 0);
                playerSoundroll.setParameterByName(FMODConstants.SPEED, 0);
                playerSoundroll.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                break;
            default:
                // Handle other cases or do nothing
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Water")) 
        {
            currentGravityFactor = -5f;
            rb.angularDrag = 4;
            rb.drag = 2.2f;
        }
    }
    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Water") && state != StatePlayer.NONE) {
            currentGravityFactor = initGravityFactor;
            rb.angularDrag = 0.05f;
            rb.drag = 0;
            setTerrainParametersToExitMaterialAndStart(FMODConstants.MATERIAL.WATER);
        }
    }
    public void die()
    {
        GameManager.gM.ChangeGravityScale(-9.81f); //También cambio la gravedad aquí porque si no se nota más gravedad en las partículas.
        particles.Explode();
        StartCoroutine(delayDead());
        FMODUtils.playOneShot(FMODConstants.DAMAGE.DEATH_VOICE  , transform.position);
        rb.isKinematic = true;
        GetComponent<Renderer>().enabled = false;
        state = StatePlayer.DEAD;
    }

    IEnumerator delayDead()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.gM.InstantiateNewBall(2);
    }

    public void resetObject()
    {
        rb.isKinematic = false;
        GetComponent<Renderer>().enabled = true;
        state = StatePlayer.NONE;
        stateImpactTerrain = StateSoundImpactPlayer.NONE;
        materialActual = FMODConstants.MATERIAL.NONE;
        jumpBombEnabled = false;
        particles.resetObject();
        currentGravityFactor = initGravityFactor;
        rb.angularDrag = 0.05f;
        rb.drag = 0;
        int valorResult = valor - 10;
        valor = (valorResult > 0) ? valorResult : 0;
        live = liveInit;

        underWater.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public bool isTouchFloor() {
        return state == StatePlayer.NORMAL;
    }

    public bool isOnBalancin()
    {
        return state == StatePlayer.NORMAL && isBalancin;
    }

    public float getJumpForce()
    {
        return jumpForce;
    }

    public bool isJumpBombEnabled()
    {
        return jumpBombEnabled;
    }

    public float getJumpLimit()
    {
        return JUMP_LIMIT;
    }
    public float getJumpLowLimit()
    {
        return JUMP_LOW_LIMIT;
    }

    public void addDamage()
    {
        live--;
        if (live<=0)
        {
            die();
        }
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            handleButtonDown();
        } else
        {
            handleButtonUp();
        }
    }

    public void OnMove(InputValue value)
    {
        if (state != StatePlayer.DEAD && state != StatePlayer.FINISH)
        {
#if UNITY_STANDALONE

#endif
#if UNITY_ANDROID
                                //movementPlayer = Input.acceleration.x * 2;
#endif

            movementPlayer = especialStage ? 0 : value.Get<Vector2>().x;
            movementPlayerExpecialZ = especialStage ? value.Get<Vector2>().x * -2 : 0;
        }
    }

    public void OnMenu()
    {
        StartCoroutine(delayChangeScene());
    }
    IEnumerator delayChangeScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadSceneAsync("Menu");
    }
    public void OnDetectController(InputValue value)
    {
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }
        isRumbleActive = true;
    }
    public void OnDetectKeyboard(InputValue value)
    {
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }
        isRumbleActive = false;
    }
    public void OnDetectMouse(InputValue value)
    {
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
        isRumbleActive = false;
    }

    void OnGUI()
    {
        if (debugger)
        {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(50, 50, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = h * 4 / 100;
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            string text = string.Format("X: {0:0.000} Y:{1:0.000}", velocityBall.x, velocityBall.y);
            GUI.Label(rect, text, style);

            Rect rect2 = new Rect(50, 250, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = h * 4 / 100;
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            string text2 = string.Format("State =" + state);
            GUI.Label(rect2, text2, style);

            Rect rect3 = new Rect(50, 350, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperCenter;
            style.fontSize = h * 4 / 100;
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            string text3 = string.Format("EjeX = " + movementPlayerExpecialZ);
            GUI.Label(rect3, text3, style);
        }
    }

    private enum StateSoundImpactPlayer
    {
        NONE,
        IMPACT_TERRAIN,
        IMPACT_BOMB_TERRAIN
    }

}

public enum StatePlayer
{
    NORMAL,
    INIT_JUMP,
    JUMP,
    BOMBJUMP,
    END_BOMB_JUMP,
    INIT_FALL,
    FALL,
    DEAD,
    FINISH,
    NONE
}
using UnityEngine;

public class Balancin : MonoBehaviour
{
    [SerializeField] float depthBeforeSumerged;
    [SerializeField] float displacementAmount;
    bool floating;
    Rigidbody rb;
    float waterYPos;
    [SerializeField] float torque;
    float offset = 0.4f;
    float initDisplacement;
    GameObject player;
    float energyImpactMouse = 0f;
    float energyImpactBall = 0f;
    string mouseBall = "MouseBall";
    GameObject mouse;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }
    private void FixedUpdate()
    {
        float zRotation = transform.eulerAngles.z;
        //Debug.Log(zRotation);
        int turnDirection;
        if (floating)
        {
            //Si la caja est� por encima del agua, entonces la variable quedar� como negativa y positiva al contrario
            float displacementMultiplier = Mathf.Clamp01((waterYPos + offset - transform.position.y) / depthBeforeSumerged) * displacementAmount;
            //As�, se va aplicando una fuerza en ambas direcciones (arriba y abajo) en funci�n de la posici�n de la caja respecto al agua.
            //Finalmente, se acabar� cancelando la fuerza aplicada ya que las posiciones se igualar�n.
            rb.AddForce(new Vector3(0, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0), ForceMode.Acceleration);

            //1er cuadrante. Caja entra recta.
            //if(initDisplacement <= 45 || initDisplacement > 315)
            //{
            if (zRotation > 0.1f || zRotation < 359.9f)
            {
                turnDirection = zRotation > 0.5f && zRotation < 180 ? -1 : 1;
                rb.AddTorque(transform.forward * torque * turnDirection, ForceMode.Acceleration);
            }

            //}
            ////2o cuadrante
            //else if(initDisplacement > 45 && initDisplacement < 135)
            //{
            //    if (zRotation > 90.5f || zRotation < 89.5f)
            //    {
            //        Debug.Log("eee");
            //        turnDirection = zRotation > 90.5f ? 1 : -1;
            //        rb.AddTorque(transform.forward * torque * turnDirection, ForceMode.Acceleration);
            //    }
            //}



        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player)
        {
            energyImpactBall = player.GetComponent<Rigidbody>().velocity.y * -1;
            if (mouse != null && energyImpactBall > 2.5f)
            {
                Rigidbody mouseRB = mouse.GetComponent<Rigidbody>();
                mouseRB.AddForce(new Vector3(0, energyImpactBall * 2.5f, 0), ForceMode.Impulse);
                energyImpactMouse = energyImpactBall;
                energyImpactBall = 0;
            }
        }
        if (collision.gameObject.name == mouseBall)
        {
            mouse = collision.gameObject;
            if (player != null && energyImpactMouse > 2.5f)
            {
                Rigidbody playerRB = player.GetComponent<Rigidbody>();
                playerRB.AddForce(new Vector3(0, energyImpactMouse * 2.5f, 0), ForceMode.Impulse);
                energyImpactMouse = 0;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == mouseBall)
        {
            mouse = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSurface"))
        {
            initDisplacement = transform.eulerAngles.z;
            rb.mass = 10; //Para mejorar comportamiento cuando la bola se pone encima de una caja en el agua.
            waterYPos = other.bounds.center.y;
            //rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
            floating = true;
        }
    }
}


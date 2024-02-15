using UnityEngine;

public class ActiveBocadillo : MonoBehaviour
{
    public GameObject bocadilloObject;
    public bool isUsed = false;
    public string text = "";
    private Bocadillo bocadilloScript;
    // Start is called before the first frame update
    void Start()
    {
        bocadilloScript = bocadilloObject.GetComponent<Bocadillo>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;
        switch (tag)
        {
            case Player.TAG:
                if (!isUsed && bocadilloScript != null)
                {
                    // Llama al m�todo "ActiveBocadillo" del script
                    bocadilloScript.ActiveBocadillo(text);
                    isUsed = true;
                }
                else
                {
                    Debug.LogError("Usado o bien El componente Bocadillo no se encontr� en BocadilloObject");
                }
                break;
        }
    }
}

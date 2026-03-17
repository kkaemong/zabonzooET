using UnityEngine;

public class destroyer : MonoBehaviour
{
    public float destroyX = -15f; 

    void Update()
    {
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}

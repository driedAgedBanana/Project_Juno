using UnityEngine;

public class Player_health : MonoBehaviour
{
    public static Player_health Instance;

    [HideInInspector] public bool isAlive = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

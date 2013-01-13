using UnityEngine;
using System.Collections;

public class PersistentMonoBehaviour : MonoBehaviour
{
    public bool Singleton = true;

    protected bool IsDuplicate()
    {
        if (Singleton)
        {
            var objects = this.FindAll(this.gameObject.name);
            if (objects.Length > 1)
            {
                Destroy(this.gameObject);
                return true;
            }
        }

        DontDestroyOnLoad(this.gameObject);
        return false;
    }

    void Awake()
    {
        IsDuplicate();
    }
}

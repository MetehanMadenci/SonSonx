using UnityEngine;

public class SimpleObjectDeleter : MonoBehaviour
{
    [Tooltip("Sahneye eklenen AR objesi (Instantiate edilen model)")]
    public GameObject arSpawnedObject;

    /// <summary>
    /// Sahnedeki AR modelini siler.
    /// </summary>
    public void DeleteARObject()
    {
        if (arSpawnedObject != null)
        {
            Destroy(arSpawnedObject);
            arSpawnedObject = null;
        }
    }
}

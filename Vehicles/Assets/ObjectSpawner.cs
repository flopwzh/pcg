using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject obj;
    public int count = 10;
    public int seed = 0;
    void Awake()
    {
        Random.InitState(seed);
        for (int i = 0; i < count; i++)
        {
            GameObject copy = Instantiate(obj);

            copy.transform.position = new Vector3(i * 3.0f, 0, 0);
            copy.name = obj.name + "_" + i;
            ShipGenerator sg = copy.GetComponent<ShipGenerator>();
            if (sg != null)
            {
                sg.seed = Random.Range(0, 100000);
            }
        }

    }
}

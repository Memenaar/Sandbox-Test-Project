using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
         transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothSpeed * Time.deltaTime);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationInformation : MonoBehaviour
{
    public int m;
    public float yAngle; //new angle
    private float yAngleLast; //angle last update
    private float deltaX; // rate of change of rotationin degrees per second 
    public float degreesPerSecond;
    public int headRotSpeed; // (the aim speed to be compared to)
    public bool revFloor; // (given by the floor object, if reverberant =1)
    public bool revRight; // (given by the right wall object, if reverberant =1)
    public bool revLeft; // (given by the left wall object, if reverberant =1)
    private bool sourceStatic; // (given by buttons yes = 1, no = 0)
    float[,] responses = new float[5, 30]; // not sure about this but should be for all responses before they get written to CSV
    List<float[]> movements = new List<float[]>(); // i can't think of another way of stoinrg what is effectively an unknonw array size. It should store deltaX against scenrario time. THis may be toom much and it way need smoothing and say 5 values a second taken instead. 
    
        
        
        // Start is called before the first frame update
    void Start()
    {
        yAngle = 0; // var cameraRotation = GameObject.Find("TrackerAnchor").transform.rotation;
        deltaX = 0;
        yAngleLast = 0;// BOO SAID THE GOOSE
    }

    // Update is called once per frame
    void Update()
    {
        yAngle = ((transform.rotation.y) * Mathf.PI) / 180;
        deltaX += ((Mathf.Abs(yAngle - yAngleLast)) / Time.deltaTime);
        degreesPerSecond = deltaX / Time.fixedTime;
        Debug.Log("Y Angle" + yAngle);
        Debug.Log("Rotation Speed" + degreesPerSecond);
        yAngleLast = yAngle;
    }

}

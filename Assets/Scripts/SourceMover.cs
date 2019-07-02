using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceMover : MonoBehaviour
{
    public float speed;
    // public Vector3 cubeLocation;
    public Rigidbody rb;
    private float xPos;
    private float yPos;
    private float angle;
    private float m; // angle multiplier;
    public double radius;


    // Start is called before the first frame update
    void Start()
    {
        // Vector3 position = GetComponent<Rigidbody>().position;
        rb = GetComponent<Rigidbody>();
        // var cameraRotation = GameObject.Find("CenterEyeAnchor").transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject go = GameObject.Find("CenterEyeAnchor"); // Find the game object with the script
        RotationInformation cs = go.GetComponent<RotationInformation>(); // identify the script
        angle = cs.yAngle;    // get the variable I want
        m = cs.m; // get multiplier from central script

        float yAngleRad = (Mathf.PI / 180) * angle * m;
        double sinAngle = Mathf.Sin(angle);
        double cosAngle = Mathf.Cos(angle);
        xPos = (float) (radius * sinAngle);
        yPos = (float) (radius * cosAngle);
        Vector3 tempVect = new Vector3(xPos, yPos, 0);
        tempVect = tempVect.normalized * Time.deltaTime;
        rb.MovePosition(transform.position + tempVect);
    }
}


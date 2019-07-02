using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class mainScript : MonoBehaviour
{

    const int BUTTON_START_TEST1 = 0;
    const int BUTTON_START_TEST2 = 1;
    const int BUTTON_TEST2_YES = 2;
    const int BUTTON_TEST2_NO = 3;
    const int BUTTON_FINAL_YES = 4;
    const int BUTTON_FINAL_NO = 5;

    const int STAGE_TEST1_SCREEN = 0;
    const int STAGE_TEST1 = 1;
    const int STAGE_TEST2_SCREEN = 2;
    const int STAGE_TEST2 = 3;
    const int STAGE_TEST3_SCREEN = 4;
    const int STAGE_TEST3 = 5;
    const int STAGE_FINAL_SCREEN = 6;

    private int appStage = 0;

    public GameObject[] UIpanels = new GameObject[4];

    public GameObject mainObject;

    public GameObject participantTextfield;

    public int participantId;

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
        restartExperience();
        // Vector3 position = GetComponent<Rigidbody>().position;
        rb = GetComponent<Rigidbody>();
        // var cameraRotation = GameObject.Find("CenterEyeAnchor").transform.rotation;
    }

    void restartExperience()
    {

        appStage = STAGE_TEST1_SCREEN;
        mainObject.transform.localScale = new Vector3(0, 0, 0);
        int i;
        for (i = 1; i < 4; i++)
        {
            UIpanels[i].GetComponent<CanvasGroup>().alpha = 0.0f;
        }
        participantTextfield.GetComponent<Text>().text = participantId.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        /*
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
        */
    }

    public void ClickButton(int whichOne)
    {
        switch (whichOne)
        {
            case (BUTTON_START_TEST1):
                {
                    changeStage(STAGE_TEST1);
                    break;
                }

            case (BUTTON_START_TEST2):
                {
                    changeStage(STAGE_TEST2);
                    break;
                }

            case (BUTTON_TEST2_YES):
                {
                    
                    break;
                }

            case (BUTTON_TEST2_NO):
                {
                    break;
                }

            case (BUTTON_FINAL_YES):
                {
                    break;
                }

            case (BUTTON_FINAL_NO):
                {

                    break;
                }

        }
    }

    void changeStage(int whichStage)
    {
        switch (whichStage)
        {

            case (STAGE_TEST1):
                {
                    LeanTween.alphaCanvas(UIpanels[0].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    break;
                }

            case (STAGE_TEST2_SCREEN):
                {
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    break;
                }


            case (STAGE_TEST2):
                {
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    break;
                }


            case (STAGE_TEST3_SCREEN):
                {
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    break;
                }

            case (STAGE_TEST3):
                {
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    break;
                }

            case (STAGE_FINAL_SCREEN):
                {
                    LeanTween.alphaCanvas(UIpanels[3].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    break;
                }


        }
    }

}


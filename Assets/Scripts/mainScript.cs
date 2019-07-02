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

    public float[] test1Times = new float[3];
    public float[] test1Speed = new float[3];
    public int test1Number = 0;


    private Vector3 playerOrigin;
    private Vector3 playerRotation;

    public GameObject mainObject;

    public GameObject OculusCenterEyes;
    public GameObject participantTextfield;

    public GameObject Canvas;

    public int participantId;

    public float ballSpeed;
    public float ballDistance;

    public float gValue = .5f;

    public bool StartPart;

    private float timeStart;

    // Start is called before the first frame update
    void Start()
    {

        restartExperience();
        // Vector3 position = GetComponent<Rigidbody>().position;
     //   rb = GetComponent<Rigidbody>();
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
            UIpanels[i].SetActive(false);
        }
        participantTextfield.GetComponent<Text>().text = participantId.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(StartPart==true)
        {
            changeStage(STAGE_TEST1);
            StartPart = false;
        }
        float deltaTime = Time.time - timeStart;
        switch (appStage)
        {
            case (STAGE_TEST1):
                {
                    Vector3 startRotation = new Vector3(((Mathf.Sin((deltaTime) * Mathf.PI / 180f * test1Speed[test1Number]))) * ballDistance, playerOrigin.y, Mathf.Abs(Mathf.Cos((deltaTime) * Mathf.PI / 180f * test1Speed[test1Number])) * ballDistance);
                    mainObject.transform.position = startRotation;

                    RaycastHit seen = new RaycastHit();
                    Ray raydirection = new Ray(OculusCenterEyes.transform.position, OculusCenterEyes.transform.forward);
                    mainObject.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
                    if (Physics.Raycast(raydirection, out seen, 25.0f))
                    {
                      
                        if (seen.collider.tag == "matterObject")
                        {
                            mainObject.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
                        }
                    }
                    break;
                }

            case (STAGE_TEST2):
                {
                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;
                    rotation.y = rotation.y * gValue;
                  //  SoundGameObject.transform.position = new Vector3(ballDistance * , playerOrigin.y, ballDistance *);


                    break;
                }

        }

    }


    void FixedUpdate()
    {
        if (appStage == STAGE_TEST1)
        {
            
        }

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

    public void killCanvas(object number)
    {
      /*  int ilonczyn = number as int;
        timeStart = Time.fixedTime;
        if (ilonczyn == 0)
        {
            appStage = STAGE_TEST1;
            UIpanels[0].SetActive(false);
        }*/
    }

    void changeStage(int whichStage)
    {
        switch (whichStage)
        {

            case (STAGE_TEST1):
                {
                    LeanTween.alphaCanvas(UIpanels[0].GetComponent<CanvasGroup>(), 0.0f, 0.5f);// setOnComplete(killCanvas).setOnCompleteParam(STAGE_TEST1);
                    LeanTween.scale(mainObject, new Vector3(1.0f, 1.0f, 1.0f), .5f);
                    UIpanels[0].SetActive(false);
                    appStage = STAGE_TEST1;
                    timeStart = Time.fixedTime;
                    playerOrigin = OculusCenterEyes.transform.position;
                    playerRotation = OculusCenterEyes.transform.eulerAngles;
                    Canvas.SetActive(false);
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



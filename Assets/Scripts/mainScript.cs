using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class testInstance
{
    public float gValue;
    public bool floorReflection;
    public bool RightwallReflection;
    public bool leftWallReflection;

    // turn to private
    public bool[] finished = new bool[3] { false, false, false };
}

public class mainScript : MonoBehaviour
{

    const int BUTTON_START_TRAINING = 0;
    const int BUTTON_START_TEST = 1;
    const int BUTTON_TEST_YES = 2;
    const int BUTTON_TEST_NO = 3;

    const int BUTTON_FINAL_YES = 4;
    const int BUTTON_FINAL_NO = 5;

    const int STAGE_TRAINING_SCREEN = 0;
    const int STAGE_TRAINING = 1;
    const int STAGE_TEST_SCREEN = 2;
    const int STAGE_TEST = 3;
    const int STAGE_FINAL_SCREEN = 4;
    //   const int STAGE_TEST3 = 5;
    //  const int STAGE_FINAL_SCREEN = 6;


    private int appStage = 0;

    public int testResult = 0;

    public int testRange;

    List<bool> testSuccessfull = new List<bool>();

    public List<testInstance> tests = new List<testInstance>();
    
    public float[] trainingSpeed = new float[3];
    private bool[] trainingFinished = new bool[3];

    public float trainingTime;


    private Vector3 playerOrigin;
    private Vector3 playerRotation;

    public GameObject testID;
    public GameObject floorReverb;
    public GameObject LeftWallReverb;
    public GameObject RightWallReverb;

    public GameObject[] UIpanels = new GameObject[4];

    public GameObject mainObject;

    public GameObject OculusCenterEyes;
    public GameObject participantTextfield;

    public GameObject Canvas;

    public int participantId;
    public int _AmountOfTests = 5;
    public float ballDistance;

    // to remove
    public float gValue = .5f;
    public int trainingNumber = 0;
    public bool StartPart;

    private float timeStart;

    public bool audioTest;

    public int chosenTest;

    // Start is called before the first frame update
    void Start()
    {
        mainObject.GetComponent<AudioSource>().Play();
        newSession();
    }


    void newSession()
    {
        restartExperience();
    }

    void restartExperience()
    {
        int i;
        appStage = STAGE_TRAINING_SCREEN;
        for(i=0;i<_AmountOfTests; i++)
        {
            tests[i].finished = new bool[3];
            tests[i].finished[0] = false;
            tests[i].finished[1] = false;
            tests[i].finished[2] = false;
        }
        for(i=0;i<trainingFinished.Length; i++)
        {
            trainingFinished[i] = false;
        }
        mainObject.transform.localScale = new Vector3(0, 0, 0);
   
        for (i = 1; i < 4; i++)
        {
            UIpanels[i].GetComponent<CanvasGroup>().alpha = 0.0f;
            UIpanels[i].SetActive(false);
        }
        UIpanels[0].SetActive(true);
        UIpanels[0].GetComponent<CanvasGroup>().alpha = 1.0f;

        Canvas.SetActive(true);

        participantTextfield.GetComponent<Text>().text = participantId.ToString();
        mainObject.GetComponent<AudioSource>().mute = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(audioTest)
        {
            changeStage(STAGE_TEST);
            audioTest = false;
        }
        if(StartPart==true)
        {
            changeStage(STAGE_TRAINING);
            StartPart = false;
        }
        float deltaTime = Time.fixedTime - timeStart;

        switch (appStage)
        {
            case (STAGE_TRAINING):
                {
                    if (deltaTime >= trainingTime)
                    {
                        trainingFinished[trainingNumber] = true;
                        changeStage(STAGE_TEST_SCREEN);
                    }
                    else
                    {
                        Vector3 startRotation = new Vector3(((Mathf.Sin((deltaTime) * Mathf.PI / 180f * trainingSpeed[trainingNumber]))) * ballDistance, playerOrigin.y, Mathf.Abs(Mathf.Cos((deltaTime) * Mathf.PI / 180f * trainingSpeed[trainingNumber])) * ballDistance);
                        mainObject.transform.position = startRotation;

                        RaycastHit seen = new RaycastHit();
                        Ray raydirection = new Ray(OculusCenterEyes.transform.position, OculusCenterEyes.transform.forward);
                        mainObject.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
                        bool trying = false;
                        if (Physics.Raycast(raydirection, out seen, 25.0f))
                        {

                            if (seen.collider.tag == "matterObject")
                            {
                                mainObject.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
                                trying = true;
                            }

                        }
                        testSuccessfull.Add(trying);
                    }
                    break;
                }

            case (STAGE_TEST):
                {
                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;
                    Vector3 startRotation = new Vector3((Mathf.Sin( Mathf.PI / 180f *  gValue * rotation.y) * ballDistance), playerOrigin.y, Mathf.Abs(Mathf.Cos(Mathf.PI / 180f*gValue*rotation.y)) * ballDistance);
                    Debug.Log(rotation);
                    if((rotation.y>180)||(rotation.y<0))
                    {
                        Debug.Log("rotation is negative - flip the x coordinate");
                        startRotation.x = startRotation.x * -1;
                    }

                    mainObject.transform.position = startRotation;
                //    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;
                  //  rotation.y = rotation.y * gValue;
                  //  SoundGameObject.transform.position = new Vector3(ballDistance * , playerOrigin.y, ballDistance *);


                    break;
                }

        }

    }


    bool chooseRandomTest()
    {
        bool sortedOut = false;
        bool outcome = true;
        bool[] testingAllVariants = new bool[_AmountOfTests];
        while(sortedOut==false)
        {
            int radom = Random.Range(0, testRange);
            testingAllVariants[radom] = true;
            if(tests[radom].finished[trainingNumber]==false)
            {
                chosenTest = radom;
                sortedOut = true;
            }
            int i;
            outcome = true;
            for (i=0;i<_AmountOfTests; i++)
            {
                if(tests[i].finished[trainingNumber]==false)
                {
                    outcome = false;
                }
            }
            if(outcome)
            {
                sortedOut = true;
            }
        }
        if(outcome)
        {
            return (true);
        }
        floorReverb.SetActive(tests[chosenTest].floorReflection);
        LeftWallReverb.SetActive(tests[chosenTest].leftWallReflection);
        RightWallReverb.SetActive(tests[chosenTest].RightwallReflection);
        gValue = tests[chosenTest].gValue;
        return (false);
    }



    void FixedUpdate()
    {
        if (appStage == STAGE_TRAINING)
        {
            
        }

    }

    public void ClickButton(int whichOne)
    {
        switch (whichOne)
        {
            case (BUTTON_START_TRAINING):
                {
                    changeStage(STAGE_TRAINING);
                    break;
                }

            case (BUTTON_START_TEST):
                {
                    changeStage(STAGE_TEST);
                    break;
                }

            case (BUTTON_TEST_YES):
                {

                    break;
                }

            case (BUTTON_TEST_NO):
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

    public void finishScreenTransition(object number)
    {
        appStage = (int)number;
        Debug.Log(appStage);
        timeStart = Time.fixedTime;
        playerOrigin = OculusCenterEyes.transform.position;
        playerRotation = OculusCenterEyes.transform.eulerAngles;
        Canvas.SetActive(false);
    }

    void changeStage(int whichStage)
    {
        switch (whichStage)
        {

            case (STAGE_TRAINING):
                {
                    LeanTween.alphaCanvas(UIpanels[0].GetComponent<CanvasGroup>(), 0.0f, 0.7f).setOnComplete(finishScreenTransition).setOnCompleteParam(STAGE_TRAINING);
                    LeanTween.scale(mainObject, new Vector3(1.0f, 1.0f, 1.0f), 1.0f);
                    break;
                }

            case (STAGE_TEST_SCREEN):
                {
                    appStage = STAGE_TEST_SCREEN;
                    Canvas.SetActive(true);
                    UIpanels[0].SetActive(false);
                    UIpanels[1].SetActive(true);
                    LeanTween.scale(mainObject, new Vector3(0, 0, 0), .5f);
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    chooseRandomTest();
                    break;
                }


            case (STAGE_TEST):
                {
                    mainObject.GetComponent<AudioSource>().mute = false;
                    LeanTween.scale(mainObject, new Vector3(1, 1, 1), .5f); // TODO: Debug
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    playerOrigin = OculusCenterEyes.transform.position;
                    playerRotation = OculusCenterEyes.transform.eulerAngles;
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 1.0f, .5f);
                    appStage = STAGE_TEST;
                    break;
                }


            case (STAGE_FINAL_SCREEN):
                {
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    break;
                }

        }

    }

}



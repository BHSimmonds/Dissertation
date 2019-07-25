using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

[System.Serializable]
public class testInstance
{
    public float gValue;
    public uint reflections; // bitwise boolean value = refFloor, refRWall, refLWall
    public bool finished;
    public bool result;

    public List<float> headRotationx;
    public List<float> headRotationy;
    public List<float> headRotationz;
    public List<float> timestamp;
}

public class mainScript : MonoBehaviour
{

    const uint _RefFloor = 1;
    const uint _RefRWall = 2;
    const uint _refLWall = 4;

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

    List<bool> testSuccessfull = new List<bool>();

    public testInstance[,] tests;
    
    public float[] trainingSpeed = new float[3];
    public float[] trainingTime = new float[3];
    private bool[] trainingFinished = new bool[3];
    public int firstTrainingToPerform;
    public bool ReverbVersion = false;
    public bool recordXandZ = false;
 
    private int _variantsG = 6;  // amount of G variants
    private int _variantsTests = 4; // amount of test variations - if sound is reflective or not? 
    private int _variantsTraining = 3; // amount of trainings

    // ______________ BINDINGS TO GAMEOBJECTS

    private Vector3 playerOrigin;
    private Vector3 playerRotation;
    private Vector3 origFloorPos;
    private Vector3 origLWallPos;
    private Vector3 origRWallPos;
    private Vector3 farFloorPos;
    private Vector3 farLWallPos;
    private Vector3 farRWallPos;
    private float sourceDirection;
   

    [HideInInspector]
    public GameObject worldAnchor;
    [HideInInspector]
    public GameObject OculusCenterEyes;
    [HideInInspector]
    public GameObject Canvas;
    [HideInInspector]
    public GameObject[] UIpanels = new GameObject[4];
    [HideInInspector]
    public GameObject participantTextfield;
    [HideInInspector]
    public GameObject yesButton;
    [HideInInspector]
    public GameObject noButton;
    [HideInInspector]
    public GameObject testID;
      
    public GameObject objectFloor;
    public GameObject objectLeftWall;
    public GameObject objectRightWall;




    public GameObject mainObject;
    public GameObject soundObject;
    public GameObject limitRightSide;
    public GameObject limitLeftSide;



    //______________________________________________ IMPORTANT VALUES

    private float[] gValues = new float[6] { -.5f, -.25f, -0f, .25f, .5f, .75f };

    private int participantId;
    public int _amountTestsToPerform;    // during launch should be 0, if you put other value it conduct only _AmountOfTests from each training
    public float ballDistance;

    // to remove
    public float gValue = .5f;
    public int trainingNumber = 0;   // 0, 1, 2


    private float timeStart;   

    // public AudioMixerSnapshot volumeUp; // to control audio mixer snapshots
    // public AudioMixerSnapshot volumeDown; //

    public int chosenTest;

    private int testCount = 0;

    public bool StartPart;   // debug only, maybe remove? 
    public bool audioTest;   // debuug only, maybe remove?
    public bool debugObjects = false;   // shows or hids the graphics object

    // Start is called before the first frame update
    void Start()
    {

        origFloorPos = objectFloor.transform.position; // Get position of reverberant surfaces and make far positions
        origLWallPos = objectLeftWall.transform.position;
        origRWallPos = objectRightWall.transform.position;
        farFloorPos = origFloorPos;
        farLWallPos = origFloorPos;
        farRWallPos = origFloorPos;
        farFloorPos.y -= 10000;
        farLWallPos.x -= 10000;
        farRWallPos.x += 10000;

        if (ReverbVersion)
        {
            // TEST 2
            _variantsTests = 4;
            firstTrainingToPerform = 0;
        } else
        {
            // TEST 1
            _variantsTests = 1; // THIS ONE??? THis was originally 3 but now is 1 as there is only the anechoic condition in the reverb free test
        }

        tests = new testInstance[_variantsTraining, _variantsG * _variantsTests];

        if (_amountTestsToPerform == 0)
        {
            _amountTestsToPerform = _variantsG * _variantsTests;
        }

        Debug.Log("AMOUNT: "+ _amountTestsToPerform);
        soundObject.GetComponent<AudioSource>().Play();

        // loading the index file to get the most recent participant ID
        try
        {
            string index = System.IO.File.ReadAllText("index");
            participantId = int.Parse(index);
        }
        catch (System.Exception e)
        {

        }

        // this is the part where you generate all the test variants

        int i;
        int j;
        uint m;
        for(j=0; j<3; j++) // speeds??
        {
            int k;
            i = 0;
            k = 0;
            for (k = 0; k < _variantsG; k++)
            {
                for (m = 0; m < _variantsTests; m++) // reverb variants for each g value
                {
                    tests[j, i] = new testInstance();
                    tests[j, i].gValue = gValues[k];
                    if (ReverbVersion)
                    {

                        switch (m)
                        {
                            case (0):
                                {
                                    tests[j, i].reflections = 0;
                                    break;
                                }

                            case (1):
                                {
                                    tests[j, i].reflections = _RefFloor;
                                    break;
                                }

                            case (2):
                                {
                                    tests[j, i].reflections = _RefRWall;
                                    break;
                                }

                            case (3):
                                {
                                    tests[j, i].reflections = _refLWall;
                                    break;
                                }
                        }
                    } else
                    {
                        tests[j, i].reflections = 0;
                    }
                    tests[j, i].finished = false;
                    tests[j, i].result = false;
                    tests[j, i].timestamp = new List<float>();
                    tests[j, i].headRotationy = new List<float>();
                    if (recordXandZ)
                    {
                        tests[j, i].headRotationx = new List<float>();
                        tests[j, i].headRotationz = new List<float>();

                    }
                    
                    i++;
                }
                
            }

        }

        newSession();
    }


    void newSession()
    {
        restartExperience();
    }

    void restartExperience()
    {
        testCount = 0;
        trainingNumber =firstTrainingToPerform;
        int i;
        int j;
        for (j = 0; j < 3; j++)
        {
            for (i = 0; i < _variantsG * _variantsTests; i++)
            {
                tests[j, i].finished = false;
                tests[j, i].result = false;
            }
        }
        for(i=0;i<trainingFinished.Length; i++)
        {
            trainingFinished[i] = false;
        }
        changeStage(STAGE_TRAINING_SCREEN);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
        if (audioTest)
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
            case (STAGE_TRAINING_SCREEN):
                {
                    Vector3 murcki = worldAnchor.transform.position;
                    murcki.x = OculusCenterEyes.transform.position.x;
                    murcki.z = OculusCenterEyes.transform.position.z;
                    worldAnchor.transform.position = murcki;
                    break;
                }
            case (STAGE_TRAINING):
                {

                    float alpha = (deltaTime) * trainingSpeed[trainingNumber];
                    Vector3 startRotation = new Vector3((Mathf.Sin(alpha * Mathf.PI / 180f)) * ballDistance, playerOrigin.y, Mathf.Abs(Mathf.Cos(alpha * Mathf.PI / 180f)) * ballDistance);
                    mainObject.transform.localPosition = startRotation;
                   
                    if (alpha>180)
                    {
                        alpha = 180 - alpha;
                    }


                     if(deltaTime>trainingTime[trainingNumber]) {
                        
                            trainingFinished[trainingNumber] = true;
                            changeStage(STAGE_TEST_SCREEN);
                        }
                        

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
                   // }
                    break;
                }

            case (STAGE_TEST):
                {

                    float alpha = (deltaTime) * trainingSpeed[trainingNumber];
                    Vector3 startRotation = new Vector3((Mathf.Sin(alpha * Mathf.PI / 180f)) * ballDistance, playerOrigin.y, Mathf.Abs(Mathf.Cos(alpha * Mathf.PI / 180f)) * ballDistance);
                    mainObject.transform.localPosition = startRotation;

                    if (alpha > 180)
                    {
                        alpha = 180 - alpha;
                    }


                    //if (deltaTime > trainingTime[trainingNumber])
                    //{
                    //    trainingFinished[trainingNumber] = true;
                    //    changeStage(STAGE_TEST_SCREEN);
                    //}


                    // RaycastHit seen = new RaycastHit();
                    // Ray raydirection = new Ray(OculusCenterEyes.transform.position, OculusCenterEyes.transform.forward);
                    // mainObject.GetComponent<Renderer>().material.color = new Color32(255, 0, 0, 255);
                    // bool trying = false;
                    // if (Physics.Raycast(raydirection, out seen, 25.0f))
                    // {
                    // 
                    //     if (seen.collider.tag == "matterObject")
                    //     {
                    //         mainObject.GetComponent<Renderer>().material.color = new Color32(0, 255, 0, 255);
                    //         trying = true;
                    //     }
                    // 
                    // }


                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;

                   
                    if (rotation.y > 180)
                    {
                        rotation.y = rotation.y - 360;
                    }

                    Vector3 soundRotation = new Vector3((Mathf.Sin( Mathf.PI / 180f *  gValue * (rotation.y)) * ballDistance), playerOrigin.y, Mathf.Abs(Mathf.Cos(Mathf.PI / 180f*gValue * (rotation.y))) * ballDistance);
                    soundObject.transform.localPosition = soundRotation;
                    
                    sourceDirection = (gValue * (rotation.y)) + 180; // makes the source dirtectional
                    soundObject.transform.eulerAngles = new Vector3(0, sourceDirection, 0);
                    

                    tests[trainingNumber, chosenTest].timestamp.Add(deltaTime);
                    tests[trainingNumber, chosenTest].headRotationy.Add(rotation.y);
                    if (recordXandZ)
                    {
                        tests[trainingNumber, chosenTest].headRotationx.Add(rotation.x);
                        tests[trainingNumber, chosenTest].headRotationz.Add(rotation.z);

                    }

                    break;
                }

        }

    }


    bool chooseRandomTest()
    {
        bool sortedOut = false;
        bool outcome = true;
        bool[] testingAllVariants = new bool[_amountTestsToPerform];

        Debug.Log(testingAllVariants.Length);

        //     Random.state = Time.time;

        timeStart = Time.fixedTime;
        while (sortedOut==false)
        {
            int radom = UnityEngine.Random.Range(0, _amountTestsToPerform);
            if(radom== _amountTestsToPerform)
            {
                radom--;
            }
            testingAllVariants[radom] = true;
            if(tests[trainingNumber,radom].finished==false)
            {
                chosenTest = radom;
                sortedOut = true;
            }
            int i;
            outcome = true;
            for (i=0;i< _amountTestsToPerform; i++)
            {
                if(tests[trainingNumber,i].finished==false)
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
        testCount += 1;
        if (ReverbVersion)
        {
            testID.GetComponent<Text>().text = testCount + "/" + _amountTestsToPerform;
        } else
        {
            testID.GetComponent<Text>().text = testCount + "/" + _amountTestsToPerform * _variantsTraining;

        }
        //   tests[trainingNumber, chosenTest].reflections

        Debug.Log("chosenTest: "+ chosenTest + ": at speed: " + trainingSpeed);// tests[trainingNumber, chosenTest].reflections & 0x1);

        // floorReverb.SetActive((tests[trainingNumber, chosenTest].reflections & _RefFloor) == _RefFloor); 
        // LeftWallReverb.SetActive((tests[trainingNumber, chosenTest].reflections & _refLWall) == _refLWall);
        // RightWallReverb.SetActive((tests[trainingNumber, chosenTest].reflections & _RefRWall) == _RefRWall);

        if ((tests[trainingNumber, chosenTest].reflections & _RefFloor) == _RefFloor) // moves the walls away instead of deactivating
        {
            objectFloor.transform.position = origFloorPos;
        }
        else
        {
            
            objectFloor.transform.position = farFloorPos;
        }



        if ((tests[trainingNumber, chosenTest].reflections & _refLWall) == _refLWall)
        {
            objectLeftWall.transform.position = origLWallPos;
        }
        else
        {
            // Vector3 lWallPos = new Vector3(-10000, 1.1f, 0);
            objectLeftWall.transform.position = farLWallPos;
        }


        if ((tests[trainingNumber, chosenTest].reflections & _RefRWall) == _RefRWall)
        {
            // Vector3 rWallPos = new Vector3(10, 1.63f, 0);
            objectRightWall.transform.position = origRWallPos;
        }
        else
        {
            // Vector3 rWallPos = new Vector3(10000, 1.63f, 0);
            objectRightWall.transform.position = farRWallPos;
        }



        // if(debugObjects)
        //{
        //   objectFloor.SetActive((tests[trainingNumber, chosenTest].reflections & _RefFloor) == _RefFloor);
        //   objectLeftWall.SetActive((tests[trainingNumber, chosenTest].reflections & _refLWall) == _refLWall);
        //   objectRightWall.SetActive((tests[trainingNumber, chosenTest].reflections & _RefRWall) == _RefRWall);
        //}

        gValue = tests[trainingNumber, chosenTest].gValue;
        Debug.Log("G Value: " + gValue + ":Floor:" + objectFloor.active + ":LeftWall:" + objectLeftWall.active + ":RightWall:" + objectRightWall.active);
        return (false);
    }

    public void acknowledgeTests(bool valjusz)
    {
        if(valjusz)
        {
            LeanTween.scale(yesButton, new Vector3(0,0,0), .15f);
            LeanTween.scale(yesButton, new Vector3(0.002f, 0.002f, 0.002f), .15f).setDelay(.15f);

        } else
        {
            LeanTween.scale(noButton, new Vector3(0, 0, 0), .15f);
            LeanTween.scale(noButton, new Vector3(0.002f, 0.002f, 0.002f), .15f).setDelay(.15f);
            
        }
        if(chooseRandomTest())
        {
            if (ReverbVersion)
            {
                trainingNumber = _variantsTraining;
            }
            else
            {
                trainingNumber++;
            }
            if(trainingNumber==_variantsTraining)
            {
                changeStage(STAGE_FINAL_SCREEN);
            } else
            {
                changeStage(STAGE_TRAINING_SCREEN);
            }
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
                   tests[trainingNumber,chosenTest].result = true;
                   tests[trainingNumber,chosenTest].finished = true;
                    acknowledgeTests(true);
                    // volumeDown.TransitionTo(.5f);
                    break;
                }

            case (BUTTON_TEST_NO):
                {
                    tests[trainingNumber,chosenTest].result = false;
                    tests[trainingNumber,chosenTest].finished = true;
                    acknowledgeTests(false);
                    // volumeDown.TransitionTo(.5f);
                    break;
                }

            case (BUTTON_FINAL_YES):
                {

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("\"id\", \"speed\", \"floor\", \"rwall\", \"lwall\", \"g\", \"result\", \"timestamp\", \"rotation x\", \"rotation y\", \"rotation x\"");
                  //  builder.Append("\"id");
                    int i;
                    int j;
                    int k;
                    for (j = 0; j < 3; j++) {
                        for (i = 0; i < _amountTestsToPerform; i++)
                        {
                         
                            for (k = 0; k < tests[j, i].timestamp.Count; k++)
                            {
                                builder.Append("\"");
                                builder.Append(participantId);
                                builder.Append("\",\"");
                                builder.Append(trainingSpeed[j]);
                                builder.Append("\",\"");
                                builder.Append(Convert.ToInt32((tests[j, i].reflections & 0x01) == 1));
                                builder.Append("\",\"");
                                builder.Append(Convert.ToInt32((tests[j, i].reflections & 0x02) == 2));
                                builder.Append("\",\"");
                                builder.Append(Convert.ToInt32((tests[j, i].reflections & 0x04) == 4));
                                builder.Append("\",\"");
                                builder.Append(tests[j, i].gValue);
                                builder.Append("\",\"");
                                builder.Append(Convert.ToInt32(tests[j, i].result));
                                builder.Append("\",\"");
                                builder.Append(tests[j, i].timestamp[k]);
                                if (recordXandZ)
                                {
                                    builder.Append("\",\"");
                                    builder.Append(tests[j, i].headRotationx[k]);
                                }
                                builder.Append("\",\"");
                                builder.Append(tests[j, i].headRotationy[k]);
                                if (recordXandZ)
                                {
                                    builder.Append("\",\"");
                                    builder.Append(tests[j, i].headRotationz[k]);
                                }
                                builder.Append("\"");
                                builder.Append("\r\n");
                            }
                            builder.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");

                        }
                    }
                    //   FileUtil.
                    System.IO.File.AppendAllText("data" + participantId + ".csv", builder.ToString());
                    // 
                    participantId++;

                    System.IO.File.WriteAllText("index", participantId.ToString());
                    restartExperience();
                    break;
                }

            case (BUTTON_FINAL_NO):
                {
                    restartExperience();
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

    void changeStage(int whichStage) // Building the scenarios and stages 
    {
        switch (whichStage)
        {

            case (STAGE_TRAINING_SCREEN):
                {
                    soundObject.GetComponent<MeshRenderer>().enabled = false;
                    objectFloor.transform.position = farFloorPos;
                    objectLeftWall.transform.position = farLWallPos;
                    objectRightWall.transform.position = farRWallPos;
                    // objectFloor.SetActive(false);
                    // objectLeftWall.SetActive(false);
                    // objectRightWall.SetActive(false);

                    mainObject.transform.localScale = new Vector3(0, 0, 0);
                    int i;
                    for (i = 1; i < 4; i++)
                    {
                        UIpanels[i].GetComponent<CanvasGroup>().alpha = 0.0f;
                        UIpanels[i].SetActive(false);
                    }
                    UIpanels[0].SetActive(true);
                    UIpanels[0].GetComponent<CanvasGroup>().alpha = 1.0f;

                    Canvas.SetActive(true);
                    participantTextfield.GetComponent<Text>().text = participantId.ToString();
                    soundObject.GetComponent<AudioSource>().mute = true;

                    limitLeftSide.SetActive(false);
                    limitRightSide.SetActive(false);

                    appStage = STAGE_TRAINING_SCREEN;
                    break;
                }

            case (STAGE_TRAINING):
                {
                    mainObject.GetComponent<MeshRenderer>().enabled = true;
                    timeStart = Time.fixedTime;
                    LeanTween.alphaCanvas(UIpanels[0].GetComponent<CanvasGroup>(), 0.0f, 0.7f).setOnComplete(finishScreenTransition).setOnCompleteParam(STAGE_TRAINING);
                    LeanTween.scale(mainObject, new Vector3(1.0f, 1.0f, 1.0f), 1.0f);
                    objectFloor.transform.position = farFloorPos;
                    objectLeftWall.transform.position = farLWallPos;
                    objectRightWall.transform.position = farRWallPos;
                    limitLeftSide.SetActive(true);
                    limitRightSide.SetActive(true);
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
                    // limitLeftSide.SetActive(false);
                    // limitRightSide.SetActive(false);
                    break;
                }


            case (STAGE_TEST):
                {
                    objectFloor.transform.position = origFloorPos;
                    objectLeftWall.transform.position = origLWallPos;
                    objectRightWall.transform.position = origRWallPos;
                    soundObject.GetComponent<AudioSource>().mute = false;
                    LeanTween.scale(mainObject, new Vector3(1, 1, 1), .5f); // TODO: Debug
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    playerOrigin = OculusCenterEyes.transform.position;
                    playerRotation = OculusCenterEyes.transform.eulerAngles;
                    UIpanels[0].SetActive(false);
                    UIpanels[2].SetActive(true);
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 1.0f, .5f);
                    LeanTween.scale(mainObject, new Vector3(1f, 1f, 1f), .5f);
                    if (debugObjects == false)
                    {
                        soundObject.GetComponent<MeshRenderer>().enabled = false;
                    } else
                    {
                        soundObject.GetComponent<MeshRenderer>().enabled = true;
                    }
                    chooseRandomTest();
                    appStage = STAGE_TEST;
                    break;
                }


            case (STAGE_FINAL_SCREEN):
                {
                    appStage = STAGE_FINAL_SCREEN;
                    soundObject.GetComponent<AudioSource>().mute = true;
                    LeanTween.alphaCanvas(UIpanels[2].GetComponent<CanvasGroup>(), 0.0f, 0.5f);
                    UIpanels[3].SetActive(true);
                    LeanTween.alphaCanvas(UIpanels[3].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    limitLeftSide.SetActive(false);
                    limitRightSide.SetActive(false);
                    break;
                }

        }

    }

}



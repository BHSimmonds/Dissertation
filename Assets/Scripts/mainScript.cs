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
    public int headSpeed;
    public uint reflections; // bitwise boolean value = refFloor, refRWall, refLWall
    public int soundSource;
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




    public AudioClip[] soundSource;
    private int appStage = 0;

    List<bool> testSuccessfull = new List<bool>(); // I don't know what this is

    private testInstance[] tests;

    public int[] headRotSpeed = new int[3];
    public float trainingTime;
    private bool trainingFinished;
    public float arcAngle; // how long is the arc for any scenario
    private float a1, a2, alpha, deltaAngle; // a1=previous angle value, a2 is new angle value, alpha is scene angle for ball location, deltaAngle is the change in angle for the frame
    public int firstTrainingToPerform;
    public bool recordXandZ = false;
    // private int _variantsSpeeds = 3; // NEED TO LINK THIS TO SIZE OF headRotSpeed array!! ALso NEED TO LINK trainingTime array size to this. THIS NUMBER SHOULD MATCH "public float[] headRotSpeed = new float[3];"
    // private int _variantsG = 6;  // amount of G variants
    private int _variantsRevTests = 0; // amount of test variations - if sound is reflective or not? 
    // private int _variantsTests = 0; // amount of tests at each speed
    // private int _variantsTraining = 3; // amount of trainings GET RID OF THIS ******************************************************
    private int _variantsSources = 1; // need to find out how to  automate this **********************************

    // ______________ BINDINGS TO GAMEOBJECTS

    private Vector3 playerOrigin;
    private Vector3 playerRotation;
    private Vector3 origFloorPos;
    private Vector3 origLWallPos;
    private Vector3 origRWallPos;
    private Vector3 farFloorPos;
    private Vector3 farLWallPos;
    private Vector3 farRWallPos;
    private Vector3 rLimitPos;
    private Vector3 lLimitPos;
    // private Vector3 rLimitRot;
    // private Vector3 lLimitRot;
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
    public bool inclFloor; // Is the floor included in the test
    public GameObject objectLeftWall;
    public bool inclLeftWall; // Is the left wall included in the test
    public GameObject objectRightWall;
    public bool inclRightWall; // Is the right wall included in the test

    public GameObject mainObject;
    public GameObject soundObject;
    public GameObject limitRightSide;
    public GameObject limitLeftSide;



    //______________________________________________ IMPORTANT VALUES

    public float[] gValues = new float[6] { -.5f, -.25f, -0f, .25f, .5f, .75f };

    private int participantId;
    public int _amountTestsToPerform;    // during launch should be 0, if you put other value it conduct only _AmountOfTests from each training
    public float ballDistance;

    // to remove
    public float gValue = .5f;
    private int headSpeed;
    // public int trainingNumber = 0;   // 0, 1, 2


    private float timeStart;

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

        // set limit positions
        rLimitPos = limitRightSide.transform.position;
        lLimitPos = limitLeftSide.transform.position;

        float limitAngle = arcAngle + 11.537f; // what is the angle of the limits with the adjustment 11.537
        rLimitPos.x = Mathf.Sin((limitAngle * Mathf.PI) / 180) * ballDistance;
        rLimitPos.z = Mathf.Cos((limitAngle * Mathf.PI) / 180) * ballDistance;
        lLimitPos.x = Mathf.Sin(((360 - limitAngle) * Mathf.PI) / 180) * ballDistance;
        lLimitPos.z = Mathf.Cos(((360 - limitAngle) * Mathf.PI) / 180) * ballDistance;
        limitRightSide.transform.position = rLimitPos;
        limitLeftSide.transform.position = lLimitPos;

        limitRightSide.transform.Rotate(0, (-(90 - arcAngle)), 0, Space.World); // rotation of ball limits
        limitLeftSide.transform.Rotate(0, (90 - arcAngle), 0, Space.World);




        _variantsRevTests = 1 + Convert.ToInt32(inclFloor) + Convert.ToInt32(inclLeftWall) + Convert.ToInt32(inclRightWall);

        if (_amountTestsToPerform == 0)
        {
            _amountTestsToPerform = (gValues.Length) * _variantsRevTests * (headRotSpeed.Length) * _variantsSources;
        }

        tests = new testInstance[_amountTestsToPerform];



        Debug.Log("AMOUNT: " + _amountTestsToPerform);
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

        int i = 0;
        int j;
        uint m;
        int l;
        for (j = 0; j < (headRotSpeed.Length); j++) //speeds
        {
            int k;
            // i = 0;
            k = 0;
            for (k = 0; k < (gValues.Length); k++) // variants for each g value
            {
                for (l = 0; l < _variantsSources; l++) //variants for each sound source
                {

                    for (m = 0; m < _variantsRevTests; m++) // reverb variants for each g value
                    {
                        tests[i] = new testInstance();
                        tests[i].gValue = gValues[k];

                        switch (m)
                        {
                            case (0):
                                {
                                    tests[i].reflections = 0;
                                    break;
                                }

                            case (1):
                                {
                                    tests[i].reflections = _RefFloor;
                                    break;
                                }

                            case (2):
                                {
                                    tests[i].reflections = _RefRWall;
                                    break;
                                }

                            case (3):
                                {
                                    tests[i].reflections = _refLWall;
                                    break;


                                }

                            default:
                                tests[i].reflections = 0;
                                break;

                        }


                        tests[i].soundSource = l;
                        tests[i].gValue = gValues[k];
                        tests[i].headSpeed = headRotSpeed[j];
                        // tests[i].finished = false;
                        // tests[i].result = false;
                        tests[i].timestamp = new List<float>();
                        tests[i].headRotationy = new List<float>();
                        if (recordXandZ)
                        {
                            tests[i].headRotationx = new List<float>();
                            tests[i].headRotationz = new List<float>();

                        }
                        i++;



                    }

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

        int i;

        for (i = 0; i < _amountTestsToPerform; i++)
        {

            tests[i].finished = false;
            tests[i].result = false;

        }

        trainingFinished = false;
        changeStage(STAGE_TRAINING_SCREEN);
        Debug.Log("BUILDING TRAINING SCREEN");
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
        if (StartPart == true)
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
                    Debug.Log("Stage_Training_Screen");
                    break;
                }
            case (STAGE_TRAINING):
                {

                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;


                    if (rotation.y > 180)
                    {
                        rotation.y = rotation.y - 360;
                    }

                    Vector3 soundRotation = new Vector3((Mathf.Sin(Mathf.PI / 180f * (rotation.y)) * ballDistance), playerOrigin.y, Mathf.Abs(Mathf.Cos(Mathf.PI / 180f * (rotation.y))) * ballDistance);
                    soundObject.transform.localPosition = soundRotation;
                    mainObject.transform.localPosition = soundRotation;

                    sourceDirection = rotation.y + 180; // makes the source dirtectional
                    soundObject.transform.eulerAngles = new Vector3(0, sourceDirection, 0);




                    if (deltaTime > trainingTime)
                    {

                        trainingFinished = true;
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

                    gValue = (tests[chosenTest].gValue);
                    a1 = a2;
                    a2 = (deltaTime) * (tests[chosenTest].headSpeed);
                    deltaAngle = a2 - a1;
                    alpha += deltaAngle;

                    if ((alpha > arcAngle) && (alpha < (180 - arcAngle)))
                        alpha += (2 * (90 - arcAngle));
                    else if ((alpha > (180 + arcAngle)) && (alpha < (360 - arcAngle)))
                        alpha += (2 * (90 - arcAngle));

                    if (alpha > 360)
                        alpha -= 360;

                    Vector3 startRotation = new Vector3((Mathf.Sin(alpha * Mathf.PI / 180f)) * ballDistance, playerOrigin.y, Mathf.Abs(Mathf.Cos(alpha * Mathf.PI / 180f)) * ballDistance);
                    mainObject.transform.localPosition = startRotation;

                    //if (alpha > 180)
                    //{
                    //    alpha = 180 - alpha;
                    //}

                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;


                    if (rotation.y > 180)
                    {
                        rotation.y = rotation.y - 360;
                    }

                    Vector3 soundRotation = new Vector3((Mathf.Sin(Mathf.PI / 180f * gValue * (rotation.y)) * ballDistance), playerOrigin.y, Mathf.Abs(Mathf.Cos(Mathf.PI / 180f * gValue * (rotation.y))) * ballDistance);
                    soundObject.transform.localPosition = soundRotation;

                    sourceDirection = (gValue * (rotation.y)) + 180; // makes the source dirtectional
                    soundObject.transform.eulerAngles = new Vector3(0, sourceDirection, 0);


                    tests[chosenTest].timestamp.Add(deltaTime);
                    tests[chosenTest].headRotationy.Add(rotation.y);

                    if (recordXandZ)
                    {
                        tests[chosenTest].headRotationx.Add(rotation.x);
                        tests[chosenTest].headRotationz.Add(rotation.z);

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
        while (sortedOut == false)
        {
            int radom = UnityEngine.Random.Range(0, _amountTestsToPerform);
            if (radom == _amountTestsToPerform)
            {
                radom--;
            }
            testingAllVariants[radom] = true;
            if (tests[radom].finished == false)
            {
                chosenTest = radom;
                sortedOut = true;
            }
            int i;
            outcome = true;
            for (i = 0; i < _amountTestsToPerform; i++)
            {
                if (tests[i].finished == false)
                {
                    outcome = false;
                }
            }
            if (outcome)
            {
                sortedOut = true;
            }
        }
        if (outcome)
        {
            return (true);
        }
        testCount += 1;

        headSpeed = tests[chosenTest].headSpeed;
        gValue = tests[chosenTest].gValue;

        if (debugObjects == true)
        {
            testID.GetComponent<Text>().text = testCount + "/" + _amountTestsToPerform + "/" + gValue + " g " + headSpeed + "°/sec ";
        }
        else
        {
            testID.GetComponent<Text>().text = testCount + "/" + _amountTestsToPerform; //used to have * _variantsTraining on the end
        }


        Debug.Log("chosenTest: " + chosenTest + ": at speed: " + headSpeed + "g Value: " + gValue + ": at speed: " + headSpeed);// tests[trainingNumber, chosenTest].reflections & 0x1);

        // floorReverb.SetActive((tests[chosenTest].reflections & _RefFloor) == _RefFloor); 
        // LeftWallReverb.SetActive((tests[chosenTest].reflections & _refLWall) == _refLWall);
        // RightWallReverb.SetActive((tests[chosenTest].reflections & _RefRWall) == _RefRWall);

        if ((tests[chosenTest].reflections & _RefFloor) == _RefFloor) // moves the walls away instead of deactivating
        {
            objectFloor.transform.position = origFloorPos;
            if (debugObjects == true)
            {
                Debug.Log("Reverb Floor Active");
            }
        }
        else
        {

            objectFloor.transform.position = farFloorPos;
            if (debugObjects == true)
            {
                Debug.Log("Reverb Right Wall Active");
            }
        }



        if ((tests[chosenTest].reflections & _refLWall) == _refLWall)
        {
            objectLeftWall.transform.position = origLWallPos;
            if (debugObjects == true)
            {
                Debug.Log("Reverb Left Wall Active");
            }
        }
        else
        {
            // Vector3 lWallPos = new Vector3(-10000, 1.1f, 0);
            objectLeftWall.transform.position = farLWallPos;
        }


        if ((tests[chosenTest].reflections & _RefRWall) == _RefRWall)
        {
            // Vector3 rWallPos = new Vector3(10, 1.63f, 0);
            objectRightWall.transform.position = origRWallPos;
        }
        else
        {
            // Vector3 rWallPos = new Vector3(10000, 1.63f, 0);
            objectRightWall.transform.position = farRWallPos;
        }






        Debug.Log("G Value: " + gValue + ":Floor:" + objectFloor.active + ":LeftWall:" + objectLeftWall.active + ":RightWall:" + objectRightWall.active);
        return (false);
    }

    public void acknowledgeTests(bool valjusz)
    {
        if (valjusz)
        {
            LeanTween.scale(yesButton, new Vector3(0, 0, 0), .15f);
            LeanTween.scale(yesButton, new Vector3(0.002f, 0.002f, 0.002f), .15f).setDelay(.15f);

        }
        else
        {
            LeanTween.scale(noButton, new Vector3(0, 0, 0), .15f);
            LeanTween.scale(noButton, new Vector3(0.002f, 0.002f, 0.002f), .15f).setDelay(.15f);

        }
        if (chooseRandomTest()) // Make sure this below can be eliminated as we now go straigt through all the tests
        {

            // trainingNumber++;
        }
        if (testCount == _amountTestsToPerform)
        {
            changeStage(STAGE_FINAL_SCREEN);
        }
        else
        {
            changeStage(STAGE_TEST_SCREEN);
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
                    tests[chosenTest].result = true;
                    tests[chosenTest].finished = true;
                    acknowledgeTests(true);
                    a1 = 0;
                    a2 = 0;
                    break;
                }

            case (BUTTON_TEST_NO):
                {
                    tests[chosenTest].result = false;
                    tests[chosenTest].finished = true;
                    acknowledgeTests(false);
                    a1 = 0;
                    a2 = 0;
                    break;
                }

            case (BUTTON_FINAL_YES):
                {

                    StringBuilder movement = new StringBuilder();
                    StringBuilder results = new StringBuilder();
                    movement.AppendLine("\"id\", \"speed\", \"floor\", \"rwall\", \"lwall\", \"g\", \"result\", \"timestamp\", \"rotation x\", \"rotation y\", \"rotation x\"");
                    results.AppendLine("\"id\", \"speed\", \"floor\", \"rwall\", \"lwall\", \"g\", \"result\"");
                    //  builder.Append("\"id");
                    int i;
                    int j;
                    int k;
                    for (j = 0; j < 3; j++)
                    {
                        for (i = 0; i < _amountTestsToPerform; i++)
                        {
                            results.Append("\"");
                            results.Append(participantId);
                            results.Append("\",\"");
                            results.Append(tests[i].headSpeed);
                            results.Append("\",\"");
                            results.Append(Convert.ToInt32((tests[i].reflections & 0x01) == 1));
                            results.Append("\",\"");
                            results.Append(Convert.ToInt32((tests[i].reflections & 0x02) == 2));
                            results.Append("\",\"");
                            results.Append(Convert.ToInt32((tests[i].reflections & 0x04) == 4));
                            results.Append("\",\"");
                            results.Append(tests[i].gValue);
                            results.Append("\",\"");
                            results.Append(Convert.ToInt32(tests[i].result));
                            results.Append("\"");
                            results.Append("\r\n");


                            for (k = 0; k < tests[i].timestamp.Count; k++)
                            {
                                movement.Append("\"");
                                movement.Append(participantId);
                                movement.Append("\",\"");
                                movement.Append(headRotSpeed[j]);
                                movement.Append("\",\"");
                                movement.Append(Convert.ToInt32((tests[i].reflections & 0x01) == 1));
                                movement.Append("\",\"");
                                movement.Append(Convert.ToInt32((tests[i].reflections & 0x02) == 2));
                                movement.Append("\",\"");
                                movement.Append(Convert.ToInt32((tests[i].reflections & 0x04) == 4));
                                movement.Append("\",\"");
                                movement.Append(tests[i].gValue);
                                movement.Append("\",\"");
                                movement.Append(Convert.ToInt32(tests[i].result));
                                movement.Append("\",\"");
                                movement.Append(tests[i].timestamp[k]);
                                if (recordXandZ)
                                {
                                    movement.Append("\",\"");
                                    movement.Append(tests[i].headRotationx[k]);
                                }
                                movement.Append("\",\"");
                                movement.Append(tests[i].headRotationy[k]);
                                if (recordXandZ)
                                {
                                    movement.Append("\",\"");
                                    movement.Append(tests[i].headRotationz[k]);
                                }
                                movement.Append("\"");
                                movement.Append("\r\n");
                            }
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");


                        }



                    }
                    //   FileUtil.
                    System.IO.File.AppendAllText("movement" + participantId + ".csv", movement.ToString());
                    System.IO.File.AppendAllText("results" + participantId + ".csv", results.ToString());
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
                    mainObject.transform.position = new Vector3(0, ballDistance, 1.4f);
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
                    Debug.Log("BUILDING OPENING SCREEN");
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
                    // limitLeftSide.SetActive(false);
                    // limitRightSide.SetActive(false);
                    soundObject.GetComponent<AudioSource>().mute = false;
                    a2 = 0; // Reset the reference for the rotation
                    break;
                }

            case (STAGE_TEST_SCREEN):
                {
                    appStage = STAGE_TEST_SCREEN;
                    Canvas.SetActive(true);
                    UIpanels[0].SetActive(false);
                    UIpanels[1].SetActive(true);

                    // a2 = 0; // Reset the reference for the rotation   
                    LeanTween.scale(mainObject, new Vector3(0, 0, 0), .5f);
                    LeanTween.alphaCanvas(UIpanels[1].GetComponent<CanvasGroup>(), 1.0f, 0.5f);
                    limitLeftSide.SetActive(true);
                    limitRightSide.SetActive(true);
                    break;
                }


            case (STAGE_TEST):
                {
                    objectFloor.transform.position = origFloorPos;
                    objectLeftWall.transform.position = origLWallPos;
                    objectRightWall.transform.position = origRWallPos;
                    a2 = 0; // Reset the reference for the rotation                    
                    soundObject.GetComponent<AudioSource>().mute = false;
                    AudioSource source = gameObject.GetComponent<AudioSource>(); // Newly added by Ben
                    source.clip = soundSource[tests[chosenTest].soundSource];
                    source.Play();
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
                    }
                    else
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







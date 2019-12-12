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
    public int reflections; // bitwise boolean value = refFloor, refRWall, refLWall
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

    const int _RefFloor = 1;
    const int _RefRWall = 2;
    const int _refLWall = 4;

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

    public AudioClip[] soundSource;
    private int appStage = 0;

    List<bool> testSuccessfull = new List<bool>(); // I don't know what this is
    public testInstance[] tests;
    private int testAmount;

    public int[] headRotSpeed = new int[3];
    public float trainingTime;
    public float trainingGvalue;
    private bool trainingFinished;
    public float arcAngle; // how long is the arc for any scenario

    private float a1, a2, alpha, deltaAngle; // a1=previous angle value, a2 is new angle value, alpha is scene angle for ball location, deltaAngle is the change in angle for the frame

    public bool recordXandZ = false;


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

    private int  reflectionMask;
    private int _reflectionAmount;  // derived from included bools
    private int[] _reflectionArray;

    public GameObject mainObject;
    public GameObject soundObject;
    public GameObject limitRightSide;
    public GameObject limitLeftSide;

    //______________________________________________ IMPORTANT VALUES

    public float[] gValues = new float[6] { -.5f, -.25f, -0f, .25f, .5f, .75f };

    private int participantId;
    private int _amountTestsToPerform;    // during launch should be 0, if you put other value it conduct only _AmountOfTests from each training


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

        float limitAngle = (arcAngle / 2f) + 11.537f; // 11.537f; // what is the angle of the limits with the adjustment 11.537
        rLimitPos.x = Mathf.Sin((limitAngle * Mathf.PI) / 180) * ballDistance;
        rLimitPos.z = Mathf.Cos((limitAngle * Mathf.PI) / 180) * ballDistance;
        lLimitPos.x = Mathf.Sin(((360 - limitAngle) * Mathf.PI) / 180) * ballDistance;
        lLimitPos.z = Mathf.Cos(((360 - limitAngle) * Mathf.PI) / 180) * ballDistance;
        limitRightSide.transform.position = rLimitPos;
        limitLeftSide.transform.position = lLimitPos;

        limitRightSide.transform.Rotate(0, (-(90 - (arcAngle/2))), 0, Space.World); // rotation of ball limits
        limitLeftSide.transform.Rotate(0, (90 - (arcAngle/2)), 0, Space.World);

        _reflectionAmount = 1;
        reflectionMask = 0;
        if(inclFloor)
        {
            _reflectionAmount = _reflectionAmount * 2;
            reflectionMask = reflectionMask | _RefFloor;
        }
        if (inclLeftWall)
        {

            _reflectionAmount = _reflectionAmount * 2;
            reflectionMask = reflectionMask | _refLWall;
        }
        if (inclRightWall)
        {
            _reflectionAmount = _reflectionAmount * 2;
            reflectionMask = reflectionMask | _RefRWall;
        }
 

        _reflectionArray = new int[_reflectionAmount];

        int i;
        int j = 0;
        for (i = 0; i < 8; i++)
        {

            if ((reflectionMask & i) == i) {
                _reflectionArray[j] = i;
                j++;
            }
        }

        _amountTestsToPerform = gValues.Length * headRotSpeed.Length * _reflectionAmount * soundSource.Length;
         tests = new testInstance[_amountTestsToPerform];

        // loading the index file to get the most recent participant ID
        try
        {
            string index = System.IO.File.ReadAllText("index");
            participantId = int.Parse(index);
        }
        catch (System.Exception e)
        {
            participantId = 0;
        }

        // this is the part where you generate all the test variants

        i = 0;
        int speedI;
        int gI;
        int sourceI;
        uint reflectionI;

        for (speedI = 0; speedI < headRotSpeed.Length; speedI++) // speeds??
        {
            for (gI = 0; gI < gValues.Length; gI++)
            {
                for (sourceI = 0; sourceI < soundSource.Length; sourceI++)
                {
                    for (reflectionI = 0; reflectionI < _reflectionAmount; reflectionI++) // reverb variants for each g value
                    {
                        tests[i] = new testInstance();
                        tests[i].gValue = gValues[gI];
                        tests[i].headSpeed = headRotSpeed[speedI];
                        tests[i].soundSource = sourceI;

                        tests[i].reflections = _reflectionArray[reflectionI];

                        tests[i].finished = false;
                        tests[i].result = false;
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

    void set_in_use(int car_num)
    {
        reflectionMask = reflectionMask | (1 << car_num);
    }

    void restartExperience()
    {
        testCount = 0;
     
        int i;
        int j;
        for (j = 0; j < 3; j++)
        {

            // TODO: amount of tests
            for (i = 0; i < _amountTestsToPerform; i++)
            {
                tests[i].finished = false;
                tests[i].result = false;
            }
        }


        trainingFinished = false;
        changeStage(STAGE_TRAINING_SCREEN);
        Debug.Log("BUILDING TRAINING SCREEN");
    }

    // Update is called once per frame
    void FixedUpdate()
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
                 //   Debug.Log("Stage_Training_Screen");
                    break;
                }
            case (STAGE_TRAINING):
                {
                   // Debug.Log("Stage training");
                    Vector3 rotation = OculusCenterEyes.transform.eulerAngles;


                    if (rotation.y > 180)
                    {
                        rotation.y = rotation.y - 360;
                    }

                    Vector3 soundRotation = new Vector3((Mathf.Sin(Mathf.PI / 180f * trainingGvalue * (rotation.y)) * ballDistance), playerOrigin.y, Mathf.Abs(Mathf.Cos(Mathf.PI / 180f * trainingGvalue * (rotation.y))) * ballDistance);
                    soundObject.transform.localPosition = soundRotation;
                    mainObject.transform.localPosition = soundRotation;

                    sourceDirection = rotation.y + 180; // makes the source dirtectional
                    soundObject.transform.eulerAngles = new Vector3(0, sourceDirection, 0);

                    int t = 0;
                    if(deltaTime> trainingTime /2)
                    {
                        if (soundObject.GetComponent<AudioSource>().clip != soundSource[1])
                        {
                            soundObject.GetComponent<AudioSource>().Stop();
                            soundObject.GetComponent<AudioSource>().clip = soundSource[1];
                            soundObject.GetComponent<AudioSource>().Play();
                        }
                    }
                    if (deltaTime > trainingTime)
                    {
                     //   if (deltaTime > trainingTime)
                    //    {
                            trainingFinished = true;
                            changeStage(STAGE_TEST_SCREEN);
                     //   }

       

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

                    if ((alpha > (arcAngle/2)) && (alpha < (180 - (arcAngle / 2))))
                    {
                        alpha += (2 * (90 - (arcAngle / 2)));
                    }
                    else
                    {

                        if ((alpha > (180 + (arcAngle / 2))) && (alpha < (360 - (arcAngle / 2))))
                        {
                            alpha += (2 * (90 - (arcAngle / 2)));
                        }
                    }

                        if (alpha > 360)
                        {
                            alpha -= 360;
                        }
                    

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

    private void playNewSound(int indexFromSoundSource)
    {
      
        soundObject.GetComponent<AudioSource>().Stop();
        soundObject.GetComponent<AudioSource>().clip = soundSource[indexFromSoundSource];
        soundObject.GetComponent<AudioSource>().mute = false;
        soundObject.GetComponent<AudioSource>().Play();
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
      //  playNewSound(tests[chosenTest].soundSource);

        Debug.Log("chosenTest: " + chosenTest + ": at speed: " + headSpeed + "g Value: " + gValue + ": at reflections:"+ tests[chosenTest].reflections);// tests[trainingNumber, chosenTest].reflections & 0x1);

        objectFloor.SetActive((tests[chosenTest].reflections & _RefFloor) == _RefFloor);
        objectLeftWall.SetActive((tests[chosenTest].reflections & _refLWall) == _refLWall);
        objectRightWall.SetActive((tests[chosenTest].reflections & _RefRWall) == _RefRWall);
        /*
        if ((tests[chosenTest].reflections & _RefFloor) == _RefFloor) // moves the walls away instead of deactivating
        {
            objectFloor.transform.position = origFloorPos;
            
                Debug.Log("Reverb Floor Active");
            
        }
        else
        {

            objectFloor.transform.position = farFloorPos;
             Debug.Log("Reverb floor not active");
            
        }



        if ((tests[chosenTest].reflections & _refLWall) == _refLWall)
        {
            objectLeftWall.transform.position = origLWallPos;
            //if (debugObjects == true)
          //  {
                Debug.Log("Reverb Left Wall Active");
          //  }
        }
        else
        {
            // Vector3 lWallPos = new Vector3(-10000, 1.1f, 0);
            Debug.Log("Reverb Left Wall not active");
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
        */
       // Debug.Log("G Value: " + gValue + ":Floor:" + objectFloor.active + ":LeftWall:" + objectLeftWall.active + ":RightWall:" + objectRightWall.active);
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
        Debug.Log("acknowledge");
        if (chooseRandomTest()) // Make sure this below can be eliminated as we now go straigt through all the tests
        {
            Debug.Log("RANDOM");
            changeStage(STAGE_FINAL_SCREEN);
        }
        else
        {
            Debug.Log("Stage");
            changeStage(STAGE_TEST);
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
                    chooseRandomTest();
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
                    movement.AppendLine("\"id\", \"speed\", \"songs\", \"floor\", \"rwall\", \"lwall\", \"g\", \"result\", \"timestamp\", \"rotation x\", \"rotation y\", \"rotation z\"");
                    results.AppendLine("\"id\", \"speed\", \"songs\", \"floor\", \"rwall\", \"lwall\", \"g\", \"result\"");
                    //  builder.Append("\"id");
                    int i;
                    int j;
                    int k;
                  
                        for (i = 0; i < _amountTestsToPerform; i++)
                        {
                            results.Append("\"");
                            results.Append(participantId);
                            results.Append("\",\"");
                            results.Append(tests[i].headSpeed);
                            results.Append("\",\"");
                            results.Append(tests[i].soundSource);
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
                                movement.Append(tests[i].headSpeed);
                                movement.Append("\",\"");
                                movement.Append(tests[i].soundSource);
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
                            movement.AppendLine("\"*,*,*,*,*,*,\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");
                            movement.AppendLine("\"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\", \"*\"");


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
                  //  soundObject.GetComponent<AudioSource>().mute = true;

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
                    soundObject.GetComponent<AudioSource>().clip = soundSource[0];
                    soundObject.GetComponent<AudioSource>().Play();
                    a2 = 0; // Reset the reference for the rotation
                    break;
                }

            case (STAGE_TEST_SCREEN):
                {
                    appStage = STAGE_TEST_SCREEN;
                    Canvas.SetActive(true);
                    UIpanels[0].SetActive(false);
                    UIpanels[1].SetActive(true);
                    soundObject.GetComponent<AudioSource>().Stop();
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
                    playNewSound(tests[chosenTest].soundSource);
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







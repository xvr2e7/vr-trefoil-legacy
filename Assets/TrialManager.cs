using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class TrialManager : MonoBehaviour
{
    public GameObject arrowObject;
    public GameObject dashedTrefoilObject;
    public GameObject previewObject;
    public int numTrials;  // Number of trials
    public TextMeshProUGUI trialText;
    public TextMeshProUGUI trialText2;
    public TextAsset trialFile;  // Reference to the text file containing the trials

    public GameObject shapesObject;
    public GameObject shapesTrefoilObject;


    public enum RotationDirection
    {
        CW,
        CCW
    }

    private readonly int numPracDashedTrials = 21;
    // private readonly int numPracDashedTrials = 1;
    private readonly int numPracSolidTrials = 1;

    [System.Serializable]
    public class Trial
    {
        public int n = 3;
        public float R_1 = 1f, R_2 = 1.5f;
        public float width = 0.02f;
        public int segments = 1000;
        public float rotationSpeed = 60f;
        public RotationDirection rotationDirection;
        public int arrowPointIndex = 0;
        public bool isDashed = false;
        public float dashSpeed = 1f;
        public bool isSelfRotating = true;
        public bool arrowDirection = true;
    }


    [System.Serializable]
    public class TrialRecord
    {
        public int trialNumber;
        public float a;
        public float b;
        public float multiplier;
        public string result; // 'X', 'Y', or empty for non-dashed trials
        public float reactionTime;
    }
    public List<TrialRecord> trialRecords = new();

    public List<Trial> trials = new();
    private int trialNumber = 0;
    private int numDashedTrials = 0;
    private int numSolidTrials = 0;

    private TrefoilRotation trefoil;
    private ArrowGeneratorForDashed arrowGen;
    private ArrowRotationForDashed arrowRot;
    private DashedTrefoilRotation dashedTrefoil;
    private Plot3D preview;

    private readonly string exp_begin_text_0 = "<align=\"center\"><b>Welcome to the experiment!\n\nYou will see three white curves and a rotating black curve.\nFocus on the black curve. Feel free to move around.\nWhich of the three white curves best matches the black curve?\n\nPress 'A' to begin.</b>";
    private readonly string exp_begin_text_1 = "<align=\"left\"><b>During this experiment, you will perform two tasks:\n\n1. A rotating dashed curve. When an arrow appears, determine if the direction of\nthe moving dashes aligns with the arrow’s direction.\n2. A rotating solid curve. There will be a white 3D curve with reference axes. Adjust\nthe white curve using the joysticks to match your perception of the solid black curve.\n\n</b><align=\"center\"><b>'Press 'A' to continue.</b>";
    private readonly string exp_begin_text_2 = "<align=\"left\"><b>Now, you will have some practice runs for each task. Please remember the procedures.\n\n</b><align=\"center\"><b>Press ‘A’ when you’re ready to continue.</b>";
    private readonly string exp_start_text = "<align=\"center\"><b>Now, we can start the experiment.\n\nPress ‘A’ when you’re ready to continue.</b>";
    private readonly string exp_mid_text = "<align=\"center\"><b>Great progress! You have completed half of the experiment.\n\nPress ‘A’ when you’re ready to continue.</b>";
    private readonly string exp_end_text = "<align=\"center\"><b>This concludes the experiment. Thank you for your participation!</b>";
    private readonly string prac_text_solid = "<align=\"left\"><b>[Practice] First, try to perceive the black 3D curve on your right. Then, move each\njoystick up and down to adjust the white 3D curve until it matches your perception of the\nblack curve. Press ‘A’ once you are satisfied with your adjustments.\n\n</b><align=\"center\"><b>Press ‘A’ to begin the task.</b>";
    private readonly string prac_text_dashed = "<align=\"left\"><b>[Practice] Focus on the dashed 3D curve. An arrow will appear. Press ‘Y’ if you believe\nthe direction of the dashes moving along the curve is the same as the arrow's direction.\nOtherwise, press ‘X’.\n\n</b><align=\"center\"><b>Press ‘A’ to begin the task.</b>";
    // private readonly string prac_text_dashed_reminder = "<align=\"left\"><b>Reminder: Press ‘Y’ if dashes move along the arrow’s direction. Otherwise, press ‘X’.</b>";
    private readonly string stim_text_solid = "<align=\"left\"><b>First, try to perceive the black 3D curve on your right. Then, move each joystick up and\ndown to adjust the white 3D curve until it matches your perception of the black curve.\nPress ‘A’ once you are satisfied with your adjustments.\n\n</b><align=\"center\"><b>Press ‘A’ to begin the task.</b>";
    private readonly string stim_text_dashed = "<align=\"left\"><b>Focus on the dashed 3D curve. An arrow will appear. Press ‘Y’ if you believe the\ndirection of the dashes moving along the curve is the same as the arrow's direction.\nOtherwise, press ‘X’.\n\n</b><align=\"center\"><b>Press ‘A’ to begin the task.</b>";
    // private readonly string stim_text_dashed_reminder = "<align=\"left\"><b>Reminder: Press ‘Y’ if dashes move along the arrow’s direction. Otherwise, press ‘X’.</b>";


    private readonly float minArrowDelay = 6f;
    private readonly float maxArrowDelay = 10f;

    void OnValidate()
    {
        // Ensure the list has the correct number of elements
    if (trials.Count != numTrials)
        {
            while (trials.Count < numTrials)
            {
                trials.Add(new Trial());
            }
            while (trials.Count > numTrials)
            {
                trials.RemoveAt(trials.Count - 1);
            }
        }
    }

    void Start()
    {
        // Load all TextAssets from the "Resources/Trials" folder
        TextAsset[] trialFileAssets = Resources.LoadAll<TextAsset>("Trials");

        if (trialFileAssets == null || trialFileAssets.Length == 0)
        {
            Debug.LogError("[TrialLoader] No .csv files (as TextAssets) found in Resources/Trials folder.");
            return;
        }

        TextAsset selectedTrialFileAsset = trialFileAssets
            .OrderByDescending(asset => asset.name) // Sorts alphabetically by name, descending
            .FirstOrDefault();

        if (selectedTrialFileAsset == null)
        {
            Debug.LogError("[TrialLoader] Could not select a trial file from Resources/Trials.");
            return;
        }

        Debug.Log($"[TrialLoader] Loading trials file from Resources: {selectedTrialFileAsset.name}");

        // The TextAsset is already loaded, so you directly use its .text property
        // No need to create a new TextAsset(text)
        trialFile = selectedTrialFileAsset; // Assign to your existing trialFile variable

        if (trialFile == null || string.IsNullOrEmpty(trialFile.text))
        {
            Debug.LogError($"[TrialLoader] Failed to load or read trials file: {selectedTrialFileAsset.name}");
            return;
        }

        LoadTrialsFromCSV(trialFile);
        

        trefoil = GetComponent<TrefoilRotation>();
        arrowGen = arrowObject.GetComponent<ArrowGeneratorForDashed>();
        arrowRot = arrowObject.GetComponent<ArrowRotationForDashed>();
        dashedTrefoil = dashedTrefoilObject.GetComponent<DashedTrefoilRotation>();
        preview = previewObject.GetComponent<Plot3D>();

        if (trefoil == null || arrowGen == null || arrowRot == null || dashedTrefoil == null || preview == null || trialText == null || trialText2 == null)
        {
            Debug.LogError("Some required components are not assigned or found.");
            return;
        }

        // Disable all before trial starts
        trefoil.DisableLine();
        preview.Hide();
        arrowGen.DisableMesh();
        dashedTrefoil.DisableLine();

        if (shapesObject == null || shapesTrefoilObject == null)
        {
            Debug.LogError("Demo shapes components are not assigned or found.");
            return;
        }

        Hide3DShapes();
    }

    private bool hasBegun = false;
    private int currentStage = 1;
    private int executingStage = 0;
    private Coroutine dashedCoroutine;
    private bool startSolidCoroutineEnded = false;
    private bool startDashedCoroutineEnded = false;
    private float reactionStartTime;

    void Update()
    {
        if (!hasBegun)
        {
            hasBegun = true;
            trefoil.DisableLine();
            preview.Hide();
            arrowGen.DisableMesh();
            dashedTrefoil.DisableLine();
        }

        if (trialNumber <= numTrials)
        {
            if (currentStage == 1 && executingStage == currentStage-1)
            {
                StartCoroutine(Introduction());
            }

            else if (currentStage == 2 && executingStage == currentStage-1)
            {
                StartCoroutine(PracticePeriod());
            }

            else if (currentStage == 3 && executingStage == currentStage-1)
            {
                if (trials[trialNumber].isDashed)
                {
                    StartCoroutine(DashedPeriod());
                }
                else
                {
                    StartCoroutine(SolidPeriod());
                }
            }

            else if (currentStage == 4 && executingStage == currentStage-1)
            {
                StartCoroutine(MidExperiment());
            }

            else if (currentStage == 5 && executingStage == currentStage-1)
            {
                if (trials[trialNumber].isDashed)
                {
                    StartCoroutine(DashedPeriod());
                }
                else
                {
                    StartCoroutine(SolidPeriod());
                }
            }
            
            else if (currentStage == 6 && executingStage == currentStage-1)
            {
                EndExperiment();
            }
        }
    }

    IEnumerator Introduction()
    {
        executingStage++;

        DisplayText(exp_begin_text_0);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        Display3DShapes();
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));      
        Hide3DShapes();

        DisplayText(exp_begin_text_1);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();
        
        DisplayText(exp_begin_text_2);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        currentStage++;
    }

    IEnumerator PracticePeriod()
    {
        executingStage++;

        DisplayText(prac_text_solid);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();
        
        for (int i = 0; i < numPracSolidTrials; i++)
        {
            yield return new WaitForSeconds(0.5f);
            StartTrial();
            yield return new WaitUntil(() => startSolidCoroutineEnded);
            yield return new WaitForSeconds(0.3f);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
            var (aValue, bValue, multiplierValue) = preview.GetParams();
            EndSolidTrial(aValue, bValue, multiplierValue);
        }
        
        DisplayText(prac_text_dashed);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();
        
        for (int i = 0; i < numPracDashedTrials; i++)
        {
            yield return new WaitForSeconds(0.3f);
            StartTrial();
            yield return new WaitUntil(() => startDashedCoroutineEnded);
            // DisplayText(prac_text_dashed_reminder);
            yield return new WaitForSeconds(0.3f);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four));
            // ClearText();
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                EndDashedTrial(true);
            }
            else if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                EndDashedTrial(false);
            }
        }

        DisplayText(exp_start_text);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        currentStage++;
    }

    IEnumerator MidExperiment()
    {
        executingStage++;

        DisplayText(exp_mid_text);
        yield return new WaitForSeconds(0.3f);
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        currentStage++;
    }

    void EndExperiment()
    {
        executingStage++;

        DisplayText(exp_end_text);
        SaveTrialDataToCSV(); // Save the recorded data at the end

        currentStage++;
    }

    IEnumerator DashedPeriod()
    {
        executingStage++;

        DisplayText(stim_text_dashed);
        yield return new WaitForSeconds(0.3f);        
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        for (int i = 0; i < numDashedTrials; i++)
        {
            yield return new WaitForSeconds(0.3f);
            StartTrial();
            yield return new WaitUntil(() => startDashedCoroutineEnded);
            // DisplayText(stim_text_dashed_reminder); 
            yield return new WaitForSeconds(0.3f);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.Three) || OVRInput.GetDown(OVRInput.Button.Four));
            // ClearText();       
            if (OVRInput.GetDown(OVRInput.Button.Three))
            {
                EndDashedTrial(true);
            }
            else if (OVRInput.GetDown(OVRInput.Button.Four))
            {
                EndDashedTrial(false);
            } 
        }

        currentStage++;
    }

    IEnumerator SolidPeriod()
    {
        executingStage++;

        DisplayText(stim_text_solid);
        yield return new WaitForSeconds(0.3f);    
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
        ClearText();

        for (int i = 0; i < numSolidTrials; i++)
        {
            yield return new WaitForSeconds(0.5f);
            StartTrial();
            yield return new WaitUntil(() => startSolidCoroutineEnded);
            yield return new WaitForSeconds(0.3f);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One));
            var (aValue, bValue, multiplierValue) = preview.GetParams();
            EndSolidTrial(aValue, bValue, multiplierValue);
        }

        currentStage++;
    }

    IEnumerator StartDashedTrial(Trial trialParams)
    {
        // Debug.Log("Start Dashed Trial " + trialNumber);
        trefoil.DisableLine();
        preview.Hide();

        arrowGen.ResetTo(trialParams);
        arrowRot.ResetRotation(trialParams.arrowDirection);
        
        dashedTrefoil.ResetTo(trialParams);
        dashedTrefoil.EnableLine();
        dashedTrefoil.StopMotion();

        float delay = Random.Range(minArrowDelay, maxArrowDelay);
        yield return new WaitForSeconds(delay);
        arrowGen.EnableMesh();
        yield return new WaitForSeconds(0.5f);
        dashedTrefoil.StartMotion();

        startDashedCoroutineEnded = true;
        reactionStartTime = Time.time;

        yield return new WaitForSeconds(2.0f);
        dashedTrefoil.StopMotion();
        dashedTrefoil.DisableLine();
        // yield return new WaitForSeconds(0.25f);
        arrowGen.DisableMesh();
    }

    void StartSolidTrial(Trial trialParams)
    {
        trefoil.ResetTo(trialParams);
        trefoil.EnableLine();
        preview.ResetTo(trialParams);
        preview.Show();

        arrowGen.DisableMesh();
        dashedTrefoil.DisableLine();

        startSolidCoroutineEnded = true;
        reactionStartTime = Time.time;
    }

    void EndDashedTrial(bool isX)
    {
        // If the dashed coroutine is running, stop it.
        if (dashedCoroutine != null)
        {
            StopCoroutine(dashedCoroutine);
            dashedCoroutine = null;
        }


        Debug.Log("Reaction Time: " + (Time.time - reactionStartTime));
        trialRecords.Add(new TrialRecord
        {
            trialNumber = trialNumber,
            a = 0, // No (a, b, multiplier) for dashed trials
            b = 0,
            multiplier = 0,
            result = isX ? "X" : "Y",
            reactionTime = Time.time - reactionStartTime,
        });

        bool condition = trials[trialNumber].dashSpeed > 0 ^ (trials[trialNumber].arrowDirection == true);
        string correct = isX == condition ? "Correct" : "Incorrect";
        Debug.Log("Trial " + trialNumber + " " + correct);

        startDashedCoroutineEnded = false;

        StopTrial(); 
        trialNumber++;
    }

    void EndSolidTrial(float aValue, float bValue, float multiplierValue)
    {
        Debug.Log("Reaction Time: " + (Time.time - reactionStartTime));
        trialRecords.Add(new TrialRecord
        {
            trialNumber = trialNumber,
            a = aValue,
            b = bValue,
            multiplier = multiplierValue,
            result = "", // No result for solid trials
            reactionTime = Time.time - reactionStartTime,
        });

        startSolidCoroutineEnded = false;

        StopTrial();
        trialNumber++;
    }

    void StartTrial()
    {
        ClearText();
        if (trialNumber < numTrials)
        {
            Debug.Log("Start Trial " + trialNumber);
            Trial currentTrial = trials[trialNumber];
            if (!currentTrial.isDashed)
            {
                StartSolidTrial(currentTrial);
            }
            else
            {
                dashedCoroutine = StartCoroutine(StartDashedTrial(currentTrial));
            }
        }
    }

    void StopTrial()
    {
        trefoil.DisableLine();
        preview.Hide();
        arrowGen.DisableMesh();
        dashedTrefoil.DisableLine();
    }

    void DisplayText(string message)
    {
        trialText.text = message;
        trialText2.text = message;
        trialText.gameObject.SetActive(true);
        trialText2.gameObject.SetActive(true);
    }

    void ClearText()
    {
        trialText.text = "";
        trialText2.text = "";
        trialText.gameObject.SetActive(false);
        trialText2.gameObject.SetActive(false);
    }

    void Display3DShapes()
    {
        shapesObject.SetActive(true);
        shapesTrefoilObject.SetActive(true);
    }

    void Hide3DShapes()
    {
        shapesObject.SetActive(false);
        shapesTrefoilObject.SetActive(false);
    }

    void LoadTrialsFromCSV(TextAsset csv)
    {
        string[] rows = csv.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        bool firstRow = true;
        trials.Clear();

        // Initialize counters for dashed and solid trials.
        int dashedCount = 0;
        int solidCount = 0;

        foreach (string row in rows)
        {
            if (firstRow)
            {
                firstRow = false;
                continue;
            }

            string[] fields = row.Split(',');

            if (fields.Length < 11) // Adjusted for the new field
            {
                Debug.LogWarning($"Invalid row skipped: {row}");
                continue;
            }

            try
            {
                RotationDirection rotationDirection;
                switch (fields[6].Trim())
                {
                    case "CW":
                        rotationDirection = RotationDirection.CW;
                        break;
                    case "CCW":
                        rotationDirection = RotationDirection.CCW;
                        break;
                    default:
                        Debug.LogWarning($"Unknown rotation direction '{fields[6].Trim()}' in row: {row}");
                        continue;
                }

                Trial trial = new()
                {
                    n = int.Parse(fields[0].Trim()),
                    R_1 = float.Parse(fields[1].Trim()),
                    R_2 = float.Parse(fields[2].Trim()),
                    width = float.Parse(fields[3].Trim()),
                    segments = int.Parse(fields[4].Trim()),
                    rotationSpeed = float.Parse(fields[5].Trim()),
                    rotationDirection = rotationDirection,
                    arrowPointIndex = int.Parse(fields[7].Trim()),
                    isDashed = bool.Parse(fields[8].Trim()),
                    dashSpeed = float.Parse(fields[9].Trim()),
                    isSelfRotating = bool.Parse(fields[10].Trim()),
                    arrowDirection = int.Parse(fields[11].Trim()) == 1
                };

                trials.Add(trial);

                // Count dashed or solid trials based on the isDashed property.
                if (trial.isDashed)
                {
                    dashedCount++;
                }
                else
                {
                    solidCount++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing row: {row}. Exception: {ex.Message}");
            }
        }

        numTrials = trials.Count;
        numDashedTrials = dashedCount - numPracDashedTrials;
        numSolidTrials = solidCount - numPracSolidTrials;

        Debug.Log($"Loaded {numTrials} trials from CSV.");
        Debug.Log($"Number of dashed trials: {dashedCount}");
        Debug.Log($"Number of solid trials: {solidCount}");
    }


    void SaveTrialDataToCSV()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Application.persistentDataPath + "/TrialData_" + timestamp + ".csv";
        List<string> csvLines = new()
        {
            "TrialNumber,a,b,Multiplier,Result,ReactionTime" // CSV header
        };

        foreach (TrialRecord record in trialRecords)
        {
            csvLines.Add($"{record.trialNumber},{record.a},{record.b},{record.multiplier},{record.result},{record.reactionTime}");
        }

        System.IO.File.WriteAllLines(filePath, csvLines);
        Debug.Log($"Trial data saved to {filePath}");
    }

}
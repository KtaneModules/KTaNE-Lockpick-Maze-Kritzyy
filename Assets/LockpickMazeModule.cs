using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class LockpickMazeModule : MonoBehaviour
{
    //Unity
    public KMSelectable LockBtn;
    public KMSelectable UpBtn, LeftBtn, RightBtn, DownBtn;

    public KMBombModule Module;

    public KMAudio BombAudio;

    public KMBombInfo BombInfo;

    public GameObject MazeBlock;
    public GameObject Pawn;
    public GameObject MinuteHandCTRL, HourHandCTRL;

    public Renderer LockRender;
    public Renderer[] ArrowRenders; //0 = up, 1 = left, 2 = right, 3 = down.

    public Texture[] LockMaterials;

    //Floats
    float BlockRot = 0;
    float LockRot = 0;

    //Ints
    int TimeLeft;

    int StartingRow = 0;
    int StartingColumn = 0;

    int GoalRow;
    int GoalColumn;

    int CurrentRow;
    int CurrentColumn;

    int MazeNumber;

    //Module ID
    static int moduleIdCounter = 1;
    int ModuleID;

    //Strings
    string DebugStartingColumn, DebugGoalColumn, DebugCurrentColumn;
    public string[] ArrowColors; //0 = up, 1 = left, 2 = right, 3 = down.

    private List<string[,]> Maze;

    public List<string> ModuleNames;

    //Bools
    bool Solved;

    //Misc
    private List<Vector3[,]> PawnCoordinates = new List<Vector3[,]>
    {
        new Vector3[8, 8]
        {
            { new Vector3(-0.367f, 0.373f, -0.396f), new Vector3(-0.2617f, 0.373f, -0.396f), new Vector3(-0.1569f, 0.373f, -0.396f), new Vector3(-0.0571f, 0.373f, -0.396f), new Vector3(0.047f, 0.373f, -0.396f), new Vector3(0.1525f, 0.373f, -0.396f), new Vector3(0.2555f, 0.373f, -0.396f), new Vector3(0.3654f, 0.373f, -0.396f) },
            { new Vector3(-0.367f, 0.373f, -0.2783f), new Vector3(-0.2617f, 0.373f, -0.2783f), new Vector3(-0.1569f, 0.373f, -0.2783f), new Vector3(-0.0571f, 0.373f, -0.2783f), new Vector3(0.047f, 0.373f, -0.2783f), new Vector3(0.1525f, 0.373f, -0.2783f), new Vector3(0.2555f, 0.373f, -0.2783f), new Vector3(0.3654f, 0.373f, -0.2783f) },
            { new Vector3(-0.367f, 0.373f, -0.164f), new Vector3(-0.2617f, 0.373f, -0.164f), new Vector3(-0.1569f, 0.373f, -0.164f), new Vector3(-0.0571f, 0.373f, -0.164f), new Vector3(0.047f, 0.373f, -0.164f), new Vector3(0.1525f, 0.373f, -0.164f), new Vector3(0.2555f, 0.373f, -0.164f), new Vector3(0.3654f, 0.373f, -0.164f) },
            { new Vector3(-0.367f, 0.373f, -0.05f), new Vector3(-0.2617f, 0.373f, -0.05f), new Vector3(-0.1569f, 0.373f, -0.05f), new Vector3(-0.0571f, 0.373f, -0.05f), new Vector3(0.047f, 0.373f, -0.05f), new Vector3(0.1525f, 0.373f, -0.05f), new Vector3(0.2555f, 0.373f, -0.05f), new Vector3(0.3654f, 0.373f, -0.05f) },
            { new Vector3(-0.367f, 0.373f, 0.0626f), new Vector3(-0.2617f, 0.373f, 0.0626f), new Vector3(-0.1569f, 0.373f, 0.0626f), new Vector3(-0.0571f, 0.373f, 0.0626f), new Vector3(0.047f, 0.373f, 0.0626f), new Vector3(0.1525f, 0.373f, 0.0626f), new Vector3(0.2555f, 0.373f, 0.0626f), new Vector3(0.3654f, 0.373f, 0.0626f) },
            { new Vector3(-0.367f, 0.373f, 0.1755f), new Vector3(-0.2617f, 0.373f, 0.1755f), new Vector3(-0.1569f, 0.373f, 0.1755f), new Vector3(-0.0571f, 0.373f, 0.1755f), new Vector3(0.047f, 0.373f, 0.1755f), new Vector3(0.1525f, 0.373f, 0.1755f), new Vector3(0.2555f, 0.373f, 0.1755f), new Vector3(0.3654f, 0.373f, 0.1755f) },
            { new Vector3(-0.367f, 0.373f, 0.2933f), new Vector3(-0.2617f, 0.373f, 0.2933f), new Vector3(-0.1569f, 0.373f, 0.2933f), new Vector3(-0.0571f, 0.373f, 0.2933f), new Vector3(0.047f, 0.373f, 0.2933f), new Vector3(0.1525f, 0.373f, 0.2933f), new Vector3(0.2555f, 0.373f, 0.2933f), new Vector3(0.3654f, 0.373f, 0.2933f) },
            { new Vector3(-0.367f, 0.373f, 0.401f), new Vector3(-0.2617f, 0.373f, 0.401f), new Vector3(-0.1569f, 0.373f, 0.401f), new Vector3(-0.0571f, 0.373f, 0.401f), new Vector3(0.047f, 0.373f, 0.401f), new Vector3(0.1525f, 0.373f, 0.401f), new Vector3(0.2555f, 0.373f, 0.401f), new Vector3(0.3654f, 0.373f, 0.401f) }
        }
    };


    void Awake()
    {
        Module.OnActivate = delegate { ModuleNames = BombInfo.GetSolvableModuleNames(); };
        
        ModuleID = moduleIdCounter++;
    }

    void Start()
    {
        Module.OnActivate = delegate 
        {
            LockGenerator();
            LockBtn.OnInteract = HandleLockPress;
        };
    }

    void LockGenerator()
    {
        int LockMaterialGen = Random.Range(0, 4);
        switch (LockMaterialGen)
        {
            case 0:
                {
                    //Material is bronze
                    Debug.LogFormat("[Lockpick Maze #{0}] The lock material and maze is Bronze.", ModuleID);
                    LockRender.material.mainTexture = LockMaterials[0];
                    MazeGenerator(0);
                    break;
                }
            case 1:
                {
                    //Material is silver
                    Debug.LogFormat("[Lockpick Maze #{0}] The lock material and maze is Silver.", ModuleID);
                    LockRender.material.mainTexture = LockMaterials[1];
                    MazeGenerator(1);
                    break;
                }
            case 2:
                {
                    //Material is gold
                    Debug.LogFormat("[Lockpick Maze #{0}] The lock material and maze is Gold.", ModuleID);
                    LockRender.material.mainTexture = LockMaterials[2];
                    MazeGenerator(2);
                    break;
                }
            case 3:
                {
                    //Material is platinum
                    Debug.LogFormat("[Lockpick Maze #{0}] The lock material and maze is Platinum.", ModuleID);
                    LockRender.material.mainTexture = LockMaterials[3];
                    MazeGenerator(3);
                    break;
                }
        }
    }

    void MazeGenerator(int MazeNumber)
    {
        this.MazeNumber = MazeNumber;
        switch (MazeNumber)
        {
            case 0:
                {
                    Maze = new List<string[,]> //Maze[0][Row, Column]
                    {
                        new string[8, 8] //Bronze
                        {
                            { "D", "D R", "D R L", "L R", "L R", "L R", "L R", "L D" },
                            { "U D", "U R D", "U L D", "R", "L R", "L R", "L R", "U L" },
                            { "U D", "U R D", "L R", "L D", "R D", "L D", "D", "D" },
                            { "U D", "U D", "D", "U D", "U R D", "U L D", "U D", "U D" },
                            { "U R D", "U L D", "U R D", "U L D", "U D", "U D", "U D", "U D" },
                            { "U D", "U D", "U", "U R", "U L", "U R", "U R L", "U L D" },
                            { "U", "U D", "R D", "L R", "L R", "L R", "L R", "U L" },
                            { "R", "U L", "U R", "L R", "L R", "L R", "L R", "L" }
                        }
                    };
                    break;
                }
            case 1:
                {
                    Maze = new List<string[,]> //Maze[0][Row, Column]
                    {
                        new string[8, 8]//Silver
                        {
                            { "D", "D R", "L R", "L R", "L R", "L R", "L R", "L D" },
                            { "U D", "U R", "L R", "L R", "L R", "L D", "U", "D" },
                            { "U R D", "L", "R D", "L D", "D R", "U R D", "L R", "U R D" },
                            { "U R", "L D", "U D", "U D", "U D", "U D", "D", "U D" },
                            { "R D", "U L D", "U D", "U D", "U D", "U D", "U D", "U D" },
                            { "U D", "U R D", "U L", "U R", "U L", "U R", "U L", "U D" },
                            { "U D", "U D", "R D", "L R", "L R", "L R", "L R", "U L D" },
                            { "U", "U", "U R", "L R", "L R", "L R", "L", "U" }
                        }
                    };
                    break;
                }
            case 2:
                {
                    Maze = new List<string[,]> //Maze[0][Row, Column]
                    {
                        new string[8, 8] //Gold
                        {
                            { "R D", "L D", "R D", "L D", "R D", "L R", "L R", "L D" },
                            { "U D", "U D", "U D", "U D", "U R D", "L R D", "L D", "U D" },
                            { "U D", "U D", "U D", "U D", "U R", "U L R", "U L", "U D" },
                            { "U D", "U R", "U L", "U R", "L R", "L R", "L R", "U L" },
                            { "U R", "L R", "L R", "L R", "L R", "L R", "L R", "L D" },
                            { "R D", "L R", "L R", "L R", "L R", "L R", "L R", "U L" },
                            { "U R", "L R", "L R", "L R", "L R", "L R", "L R", "L D" },
                            { "R", "L R", "L R", "L R", "L R", "L R", "L R", "U L" }
                        }
                    };
                    break;
                }
            case 3:
                {
                    Maze = new List<string[,]> //Maze[0][Row, Column]
                    {
                        new string[8, 8] //Platinum
                        {
                            { "D", "R D", "L R", "L R", "L R", "L R", "L R", "L D" },
                            { "U D", "U D", "D", " R D", "L D", "R D", "L D", "U D" },
                            { "U D", "U R D", "U L D", "U D", "U D", "U D", "U D", "U D" },
                            { "U D", "U D", "U D", "U D", "U D", "U D", "U D", "U D" },
                            { "U D", "U D", "U D", "U", "U R", "U L D", "U D", "U D" },
                            { "U D", "U D", "U R", "L R", "L R", "U L", "U D", "U D" },
                            { "U D", "U R", "L R", "L", "R", "L R", "U L", "U D" },
                            { "U R", "L R", "L R", "L R", "L R", "L R", "L R", "U L" }
                        },
                    };
                    break;
                }
        }
        ArrowGenerator();
    }

    void ArrowGenerator()
    {
        int CurrentArrow = 0;
        foreach (Renderer Arrow in ArrowRenders)
        {
            int Color = Random.Range(0, 4);
            switch (Color)
            {
                case 0: //red
                    {
                        ArrowColors[CurrentArrow] = "red";
                        Arrow.material.color = new Color32(255, 0, 0, 255);
                        break;
                    }
                case 1: //blue
                    {
                        ArrowColors[CurrentArrow] = "blue";
                        Arrow.material.color = new Color32(0, 0, 255, 255);
                        break;
                    }
                case 2: //yellow
                    {
                        ArrowColors[CurrentArrow] = "yellow";
                        Arrow.material.color = new Color32(255, 255, 0, 255);
                        break;
                    }
                case 3: //green
                    {
                        ArrowColors[CurrentArrow] = "green";
                        Arrow.material.color = new Color32(0, 255, 0, 255);
                        break;
                    }
            }
            CurrentArrow++;
        }
        Debug.LogFormat("[Lockpick Maze #{0}] The horizontal arrow colors are {1} and {2}; the vertical colors are {3} and {4}.", ModuleID, ArrowColors[1], ArrowColors[2], ArrowColors[0], ArrowColors[3]);
        PlayerLocation();
    }

    void PlayerLocation()
    {
        //Starting column
        switch (ArrowColors[0])
        {
            case "red":
                {
                    StartingColumn += 1;
                    break;
                }
            case "blue":
                {
                    StartingColumn += 2;
                    break;
                }
            case "yellow":
                {
                    StartingColumn += 3;
                    break;
                }
            case "green":
                {
                    StartingColumn += 4;
                    break;
                }
        }
        switch (ArrowColors[3])
        {
            case "red":
                {
                    StartingColumn += 1;
                    break;
                }
            case "blue":
                {
                    StartingColumn += 2;
                    break;
                }
            case "yellow":
                {
                    StartingColumn += 3;
                    break;
                }
            case "green":
                {
                    StartingColumn += 4;
                    break;
                }
        }

        //Starting row
        switch (ArrowColors[1])
        {
            case "red":
                {
                    StartingRow += 4;
                    break;
                }
            case "blue":
                {
                    StartingRow += 3;
                    break;
                }
            case "yellow":
                {
                    StartingRow += 2;
                    break;
                }
            case "green":
                {
                    StartingRow += 1;
                    break;
                }
        }
        switch (ArrowColors[2])
        {
            case "red":
                {
                    StartingRow += 4;
                    break;
                }
            case "blue":
                {
                    StartingRow += 3;
                    break;
                }
            case "yellow":
                {
                    StartingRow += 2;
                    break;
                }
            case "green":
                {
                    StartingRow += 1;
                    break;
                }
        }

        CurrentRow = StartingRow;
        CurrentColumn = StartingColumn;

        StartingRow--;
        StartingColumn--;
        CurrentRow--;
        CurrentColumn--;

        DebugCurrentColumn = Number2String(CurrentColumn + 1, true);
        DebugStartingColumn = Number2String(StartingColumn + 1, true);
        Debug.LogFormat("[Lockpick Maze #{0}] Your starting location is {1}", ModuleID, DebugStartingColumn + (StartingRow + 1));



        PawnPosition(StartingRow, StartingColumn);

        GoalLocation();
    }


    void GoalLocation()
    {
        //Goal column
        int ColumnNumber = BombInfo.GetSolvableModuleNames().Count();
        bool SpecialCase = false;

        foreach (string ModuleName in BombInfo.GetSolvableModuleNames())
        {
            if (!ModuleName.Contains("Lockpick"))
            {
                if (ModuleName.Contains("Color"))
                {
                    ColumnNumber += 5;
                }
                else if (ModuleName.Contains("Maze"))
                {
                    ColumnNumber += 3;
                }
                else if (ModuleName.Contains("Button"))
                {
                    ColumnNumber += 1;
                }
            }
        }
        foreach (string ModuleName in BombInfo.GetSolvableModuleNames())
        {
            if (ModuleName.Contains("Combination Lock") || ModuleName.Contains("Safety Safe"))
            {
                ColumnNumber -= 3;
            }
        }
        if (BombInfo.GetSolvableModuleNames().Contains("Retirement"))
        {
            ColumnNumber -= 4;
        }
        if (BombInfo.IsIndicatorOn(Indicator.BOB) && SpecialCase == false)
        {
            ColumnNumber = BombInfo.GetSerialNumberNumbers().Last();
        }

        //Goal row
        int RowNumber;
        RowNumber = BombInfo.GetSerialNumberNumbers().First() + BombInfo.GetOffIndicators().Count();
        foreach (string Indicator in BombInfo.GetOnIndicators())
        {
            RowNumber += 2;
        }
        if (BombInfo.GetBatteryCount() < 4) RowNumber += BombInfo.GetBatteryCount();

        Debug.LogFormat("<Lockpick Maze #{0}> Batteries: {1} (Less than 4?: {2}), Indicators ON: {3} (Add {4} to RowNumber), Indicators OFF: {5}, Serial 1st digit: {6}. RowNumber before addition/subtration: {7}", ModuleID, BombInfo.GetBatteryCount(), (BombInfo.GetBatteryCount() < 4), BombInfo.GetOnIndicators().Count(), (BombInfo.GetOnIndicators().Count() * 2), BombInfo.GetOffIndicators().Count(), BombInfo.GetSerialNumberNumbers().First(), RowNumber);

        while (ColumnNumber > 8)
        {
            ColumnNumber -= 8;
        }
        while (ColumnNumber <= 0)
        {
            ColumnNumber += 8;
        }

        while (RowNumber > 8)
        {
            RowNumber -= 8;
        }
        while (RowNumber <= 0)
        {
            RowNumber += 8;
        }

        GoalColumn = ColumnNumber - 1;
        GoalRow = RowNumber - 1;

        DebugGoalColumn = Number2String(GoalColumn + 1, true);

        Debug.LogFormat("[Lockpick Maze #{0}] Your goal location is {1}", ModuleID, DebugGoalColumn + (GoalRow + 1));
    }

    private string Number2String(int number, bool isCaps)
    {
        char c = (char)((isCaps ? 65 : 97) + (number - 1));
        return c.ToString();
    }

    void PawnPosition(int Row, int Column)
    {
        Pawn.transform.localPosition = PawnCoordinates[0][Row, Column];
    }

    protected bool HandleLockPress()
    {
        string time = BombInfo.GetFormattedTime().Remove(0, 3);
        if (!DateTime.Now.ToString("mm").Any(time.Contains))
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Actual time: {1} minutes. Bomb time {2}. Conditions are met.", ModuleID, DateTime.Now.ToString("mm"), BombInfo.GetFormattedTime());
            Debug.LogFormat("[Lockpick Maze #{0}] The timer has started, good luck.", ModuleID);
            Initiate();
        }
        else
        {
            if (BombInfo.GetTime() < 60)
            {
                Debug.LogFormat("[Lockpick Maze #{0}] Actual time: {1} minutes. Bomb time {2}. Conditions not met, but bomb time is {3}, which is below 60.", ModuleID, DateTime.Now.ToString("mm"), BombInfo.GetFormattedTime(), BombInfo.GetTime());
                Debug.LogFormat("[Lockpick Maze #{0}] The timer has started, good luck.", ModuleID);
                Initiate();
            }
            else
            {
                Debug.LogFormat("[Lockpick Maze #{0}] Actual time: {1} minutes. Bomb time {2}. Conditions not met.", ModuleID, DateTime.Now.ToString("mm"), BombInfo.GetFormattedTime());
                Debug.LogFormat("[Lockpick Maze #{0}] Initiate conditions not met. Strike handed.", ModuleID);
                BombAudio.PlaySoundAtTransform("IntruderAlert", transform);
                Module.HandleStrike();
            }
        }
        return false;
    }

    void Initiate()
    {
		LockUnlocked = true;
        UpBtn.OnInteract = HandleUp;
        LeftBtn.OnInteract = HandleLeft;
        RightBtn.OnInteract = HandleRight;
        DownBtn.OnInteract = HandleDown;

        LockBtn.OnInteract = HandleBlank;

        StartCoroutine(TurnLock());
        StartCoroutine(HandleTimer());

        BombAudio.PlaySoundAtTransform("MazeTurning", transform);

        StartCoroutine(MinuteHandAnim());
        StartCoroutine(HourHandAnim());
    }

    protected bool HandleUp()
    {
        if (Maze[0][CurrentRow, CurrentColumn].Contains("U"))
        {
            CurrentRow--;
            DebugCurrentColumn = Number2String(CurrentColumn + 1, true);
            PawnPosition(CurrentRow, CurrentColumn);
            CheckFinish();
        }
        else
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Going up at {1}{2} caused a strike", ModuleID, DebugCurrentColumn, CurrentRow + 1);
            Module.HandleStrike();
        }
        return false;
    }
    protected bool HandleLeft()
    {
        if (Maze[0][CurrentRow, CurrentColumn].Contains("L"))
        {
            CurrentColumn--;
            DebugCurrentColumn = Number2String(CurrentColumn + 1, true);
            PawnPosition(CurrentRow, CurrentColumn);
            CheckFinish();
        }
        else
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Going left at {1}{2} caused a strike", ModuleID, DebugCurrentColumn, CurrentRow + 1);
            Module.HandleStrike();
        }
        return false;
    }
    protected bool HandleRight()
    {
        if (Maze[0][CurrentRow, CurrentColumn].Contains("R"))
        {
            CurrentColumn++;
            DebugCurrentColumn = Number2String(CurrentColumn + 1, true);
            PawnPosition(CurrentRow, CurrentColumn);
            CheckFinish();
        }
        else
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Going right at {1}{2} caused a strike", ModuleID, DebugCurrentColumn, CurrentRow + 1);
            Module.HandleStrike();
        }
        return false;
    }
    protected bool HandleDown()
    {
        if (Maze[0][CurrentRow, CurrentColumn].Contains("D"))
        {
            CurrentRow++;
            DebugCurrentColumn = Number2String(CurrentColumn + 1, true);
            PawnPosition(CurrentRow, CurrentColumn);
            CheckFinish();
        }
        else
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Going down at {1}{2} caused a strike", ModuleID, DebugCurrentColumn, CurrentRow + 1);
            Module.HandleStrike();
        }
        return false;
    }

    protected bool HandleBlank()
    {
        return false;
    }

    void CheckFinish()
    {
        if (CurrentRow == GoalRow && CurrentColumn == GoalColumn)
        {
            Debug.LogFormat("[Lockpick Maze #{0}] Reached the end of the maze, module solved.", ModuleID);
            BombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            BombAudio.PlaySoundAtTransform("MazeTurning", transform);
            StartCoroutine(TurnLock());
            Module.HandlePass();

            StopCoroutine(MinuteHandAnim());
            StopCoroutine(HourHandAnim());

            UpBtn.OnInteract = HandleBlank;
            LeftBtn.OnInteract = HandleBlank;
            RightBtn.OnInteract = HandleBlank;
            DownBtn.OnInteract = HandleBlank;

            Solved = true;
        }
    }

    IEnumerator TurnLock()
    {
        for (int T = 0; T < 10; T++)
        {
            BlockRot -= 2.25f;
            MazeBlock.transform.localEulerAngles = new Vector3(0, 180, BlockRot);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        for (int T = 0; T < 5; T++)
        {
            BlockRot -= 1;
            MazeBlock.transform.localEulerAngles = new Vector3(0, 180, BlockRot);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        for (int T = 0; T < 5; T++)
        {
            BlockRot += 1;
            MazeBlock.transform.localEulerAngles = new Vector3(0, 180, BlockRot);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        for (int T = 0; T < 10; T++)
        {
            BlockRot += 2.25f;
            MazeBlock.transform.localEulerAngles = new Vector3(0, 180, BlockRot);
            yield return new WaitForSecondsRealtime(0.025f);
        }
        for (int T = 0; T < 80; T++)
        {
            BlockRot += 2.25f;
            MazeBlock.transform.localEulerAngles = new Vector3(0, 180, BlockRot);
            if (T == 75)
            {
                if (Solved)
                {
                    BombAudio.PlaySoundAtTransform("LockTurn", transform);
                }
            }
            yield return new WaitForSecondsRealtime(0.025f);
        }
    }
    IEnumerator HandleTimer()
    {
        BombAudio.PlaySoundAtTransform("ClockTickingBeat1", transform);
        bool Offbeat = false;
        TimeLeft = 30;
        for (int T = 0; T < 31; T++)
        {
            if (!Solved)
            {	
                if (TimeLeft == 0)
                {
                    Debug.LogFormat("[Lockpick Maze #{0}] Time has ended. Strike handed.", ModuleID);
                    Module.HandleStrike();

                    BombAudio.PlaySoundAtTransform("ClockTickingFinalBeat", transform);
                    BombAudio.PlaySoundAtTransform("TimesUp", transform);

                    StartCoroutine(TurnLock());
                    StopCoroutine(MinuteHandAnim());
                    StopCoroutine(HourHandAnim());

                    UpBtn.OnInteract = HandleBlank;
                    LeftBtn.OnInteract = HandleBlank;
                    RightBtn.OnInteract = HandleBlank;
                    DownBtn.OnInteract = HandleBlank;

                    LockBtn.OnInteract = HandleLockPress;
					LockUnlocked = false;
                }
                else
                {
                    if (!Offbeat)
                    {
                        BombAudio.PlaySoundAtTransform("ClockTickingBeat1", transform);
                        Offbeat = true;
                    }
                    else
                    {
                        BombAudio.PlaySoundAtTransform("ClockTickingBeat2", transform);
                        Offbeat = false;
                    }
                }
            }
            else
            {
                T = 31;
            }
            TimeLeft--;
            yield return new WaitForSecondsRealtime(1);
        }
    }
    IEnumerator MinuteHandAnim()
    {
        float Rot = 0;
        for (int T = 0; T < 61; T++)
        {
            if (!Solved)
            {
                MinuteHandCTRL.transform.localEulerAngles = new Vector3(0, Rot, 0);
                Rot += 12;
            }
            else
            {
                T = 1800;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    IEnumerator HourHandAnim()
    {
        float Rot = 0;
        for (int T = 0; T < 31; T++)
        {
            if (!Solved)
            {

                HourHandCTRL.transform.localEulerAngles = new Vector3(0, Rot, 0);
                Rot += 12;
            }
            else
            {
                T = 1800;
            }
            yield return new WaitForSeconds(1f);
        }
    }
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To determine the current time of the PC being used, use the command !{0} real time | To determine the current time of the bomb, use the command !{0} bomb time | To unlock the lock in the module, use the command !{0} unlock on [SPECIFIC TIME] (You must use the format of Tweaks' timer for inputting the specific time in the command. Example: 09:23, 6:45:32, 1:02:32:11, 00:22. Also, you may have to convert the bomb's time format to Tweaks' time format, and vice versa to get accurate times. Also, the time rule is based on the bomb's timer, not Tweaks' timer.) | To move in the maze, use the command !{0} press n/e/w/s or u/r/d/l (The movement can be performed in a chain)";
    #pragma warning restore 414
	
	bool LockUnlocked = false;
	
	int TimingIsSomething;
	string[] ValidNumbers = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9"};
	string[] SecondsAndMinutes = {"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "50", "51", "52", "53", "54", "55", "56", "57", "58", "59"};
	string[] HoursIfSolo = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"};
	string[] HoursIfMulti = {"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23"};
	string[] DaysUltimate = {"1", "2", "3", "4", "5", "6"};
	string[] ValidMovements = {"u", "d", "r", "l", "n", "e", "s", "w", "U", "D", "R", "L", "N", "E", "S", "W"};

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(command, @"^\s*real time\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            DateTime currenttime = DateTime.Now;
            yield return "sendtochat Current Date/Time: " + currenttime.ToString();
        }

        else if (Regex.IsMatch(command, @"^\s*bomb time\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            string BombTime = BombInfo.GetFormattedTime();
            yield return "sendtochat Current Bomb Time: " + BombTime;
        }

        else if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (LockUnlocked == false)
            {
                yield return "sendtochaterror The lock is still locked. You are unable to interact with the arrows.";
                yield break;
            }

            if (parameters.Length == 2)
            {
                foreach (char c in parameters[1])
                {
                    if (!c.ToString().EqualsAny(ValidMovements))
                    {
                        yield return "sendtochaterror An invalid movement has been detected in the command. The command was not initiated.";
                        yield break;
                    }
                }

                foreach (char d in parameters[1])
                {
                    if (Regex.IsMatch(d.ToString(), @"^\s*u\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(d.ToString(), @"^\s*n\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        UpBtn.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }

                    else if (Regex.IsMatch(d.ToString(), @"^\s*d\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(d.ToString(), @"^\s*s\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        DownBtn.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }

                    else if (Regex.IsMatch(d.ToString(), @"^\s*l\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(d.ToString(), @"^\s*w\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        LeftBtn.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }

                    else if (Regex.IsMatch(d.ToString(), @"^\s*r\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(d.ToString(), @"^\s*e\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                    {
                        RightBtn.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            else
            {
                yield return "sendtochaterror Invalid parameter length.";
                yield break;
            }
        }

        else if (Regex.IsMatch(parameters[0], @"^\s*unlock\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && parameters.Length > 1)
        {
            if (Regex.IsMatch(parameters[1], @"^\s*at\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                if (LockUnlocked == true)
                {
                    yield return "sendtochaterror The lock is unlocked now. You are unable to unlock it again.";
                    yield break;
                }

                if (parameters.Length == 3)
                {
                    string[] timer = parameters[2].Split(':');
                    foreach (string a in timer)
                    {
                        foreach (char b in a)
                        {
                            if (!b.ToString().EqualsAny(ValidNumbers))
                            {
                                yield return "sendtochaterror Time given contains a character which is not a number.";
                                yield break;
                            }
                        }
                    }

                    if (timer.Length != 2)
                    {
                        yield return "sendtochaterror Time length given is not valid.";
                        yield break;
                    }

                    if (!timer[1].EqualsAny(SecondsAndMinutes) || timer[0].Length < 2 || (timer[0].Length > 2 && timer[0][0] == '0'))
                    {
                        yield return "sendtochaterror Time format given is not valid.";
                        yield break;
                    }

                    while (BombInfo.GetFormattedTime() != parameters[2])
                    {
                        yield return "trycancel The unlocking command was cancelled due to a cancel request.";
                        yield return new WaitForSeconds(0.01f);
                    }

                    LockBtn.OnInteract();
                }
                else
                {
                    yield return "sendtochaterror Invalid parameter length.";
                    yield break;
                }
            }
        }
    }
}
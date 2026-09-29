using System;
using UnityEngine;

public class SAFEXFlowController : MonoBehaviour
{
    public static SAFEXFlowController Instance { get; private set; }

    public enum SAFEXState
    {
        Home,
        ModuleSelection,
        Briefing,
        ARPlacement,
        Training,
        TrainingComplete,
        Certificate
    }

    public enum TrainingModule
    {
        None,
        Fire,
        Gas
    }

    [Header("Initial State")]
    [SerializeField] private SAFEXState initialState = SAFEXState.Home;

    [Header("Runtime State")]
    [SerializeField] private SAFEXState currentState = SAFEXState.Home;
    [SerializeField] private TrainingModule selectedModule = TrainingModule.None;

    public SAFEXState CurrentState => currentState;
    public TrainingModule SelectedModule => selectedModule;

    public event Action<SAFEXState> StateChanged;
    public event Action<TrainingModule> ModuleChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentState = initialState;
        selectedModule = TrainingModule.None;
    }

    public void StartTraining()
    {
        SetState(SAFEXState.ModuleSelection);
    }

    // HomeController compatibility
    public void ShowHome()
    {
        selectedModule = TrainingModule.None;
        ModuleChanged?.Invoke(selectedModule);
        SetState(SAFEXState.Home);
    }

    // HomeController compatibility
    public void ShowModuleSelection()
    {
        SetState(SAFEXState.ModuleSelection);
    }

    public void SelectFire()
    {
        selectedModule = TrainingModule.Fire;
        ModuleChanged?.Invoke(selectedModule);
        SetState(SAFEXState.Briefing);
    }

    public void SelectFireModule()
    {
        SelectFire();
    }

    public void SelectGas()
    {
        selectedModule = TrainingModule.Gas;
        ModuleChanged?.Invoke(selectedModule);
        SetState(SAFEXState.Briefing);
    }

    public void SelectGasModule()
    {
        SelectGas();
    }

    public void SelectModule(TrainingModule module)
    {
        selectedModule = module;
        ModuleChanged?.Invoke(selectedModule);
        SetState(SAFEXState.Briefing);
    }

    public void ContinueToAR()
    {
        SetState(SAFEXState.ARPlacement);
    }

    public void ContinueFromBriefing()
    {
        SetState(SAFEXState.ARPlacement);
    }

    public void OnMinePlaced()
    {
        SetState(SAFEXState.Training);
    }

    public void CompleteTraining()
    {
        SetState(SAFEXState.TrainingComplete);
    }

    public void ShowCertificate()
    {
        SetState(SAFEXState.Certificate);
    }

    public void BackToHome()
    {
        ShowHome();
    }

    public void SetState(SAFEXState newState)
    {
        currentState = newState;
        Debug.Log("SAFEX FLOW -> " + newState);
        StateChanged?.Invoke(currentState);
    }

    public bool IsHome() => currentState == SAFEXState.Home;
    public bool IsModuleSelection() => currentState == SAFEXState.ModuleSelection;
    public bool IsBriefing() => currentState == SAFEXState.Briefing;
    public bool IsARPlacement() => currentState == SAFEXState.ARPlacement;
    public bool IsTrainingActive() => currentState == SAFEXState.Training;
    public bool IsTrainingComplete() => currentState == SAFEXState.TrainingComplete;
    public bool IsCertificateScreen() => currentState == SAFEXState.Certificate;

    public bool IsFireModule() => selectedModule == TrainingModule.Fire;
    public bool IsGasModule() => selectedModule == TrainingModule.Gas;

    public string GetSelectedModuleName()
    {
        switch (selectedModule)
        {
            case TrainingModule.Fire:
                return "FIRE";
            case TrainingModule.Gas:
                return "GAS";
            default:
                return "NONE";
        }
    }

    public void ResetFlow()
    {
        ShowHome();
    }
}

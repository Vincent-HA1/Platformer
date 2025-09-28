using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;


public class PlayerInputScript : MonoBehaviour
{
    public PlayerActions actions;
    public InputActionAsset asset;
    private static PlayerInputScript _instance;

    PlayerInput playerInput;

    RebindManager currentRebindManager;
    public void OnDisable()
    {
        SaveRebinds();
    }

    // Start is called before the first frame update

    private void Awake()
    {
        //Handle Dont Destroy On Load
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        //Load the overrides into the asset
        actions = new PlayerActions();
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            asset.LoadBindingOverridesFromJson(rebinds);
            actions.LoadBindingOverridesFromJson(rebinds);
        }
        else
        {
            print("no rebinds");
        }
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions = asset; //set the updated asset
        RebindSetup();
        SceneManager.sceneLoaded += SceneLoaded;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    public void SaveRebinds()
    {
        if (actions == null) return;
        var rebinds = asset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
        actions.LoadBindingOverridesFromJson(rebinds);
    }

    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSetup();
    }

    void RebindSetup()
    {
        //At each scene, set up the event for rebind manager
        currentRebindManager = FindObjectOfType<RebindManager>(true);
        if (currentRebindManager)
        {
            currentRebindManager.inputsRebound += SaveRebinds;
        }
        UpdateUIInputModule();
    }

    void SceneUnloaded(Scene scene)
    {
        if (currentRebindManager) currentRebindManager.inputsRebound -= SaveRebinds;
        currentRebindManager = null;
        SaveRebinds();
    }

    void UpdateUIInputModule()
    {
        playerInput.uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        playerInput.uiInputModule.actionsAsset = asset;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator sceneFadeAnimator;

    [Header("UI References")]
    [SerializeField] Button startButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject optionsScreen;
    [SerializeField] UIBar masterBar;


    // Start is called before the first frame update
    void Start()
    {
        //Hide cursor automatically
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        Time.timeScale = 1;
        startButton.onClick.AddListener(LoadScene);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            SaveSystem.DeleteSave();
        }
        if (Input.GetKeyDown("k"))
        {
            if (optionsScreen.activeInHierarchy)
            {
                //close it
                optionsScreen.SetActive(false);
                titleScreen.SetActive(true);
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
        }
    }
    
    void OpenOptions()
    {
        optionsScreen.SetActive(true);
        titleScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(masterBar.leftArrow.gameObject); //top of options screen
    }

    void LoadScene()
    {
        StartCoroutine(LoadSceneAfterFade());
        EventSystem.current.enabled = false;
    }

    IEnumerator LoadSceneAfterFade()
    {
        sceneFadeAnimator.SetTrigger("FadeOut");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        SceneManager.LoadScene("StageSelect");
    }

    void QuitGame()
    {
        EventSystem.current.enabled = false;
        Application.Quit();
    }
}

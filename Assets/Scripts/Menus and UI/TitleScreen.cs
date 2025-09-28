using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator sceneFadeAnimator;
    [SerializeField] InputHandler inputHandler;

    [Header("UI References")]
    [SerializeField] Button startButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject titleScreen;
    [SerializeField] GameObject optionsScreen;
    EventSystem eventSystem;

    // Start is called before the first frame update
    void Start()
    {
        //Hide cursor automatically
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        eventSystem = EventSystem.current;
        eventSystem.enabled = false;
        Time.timeScale = 1;
        startButton.onClick.AddListener(LoadScene);
        quitButton.onClick.AddListener(QuitGame);
        StartCoroutine(WaitForSceneFade());
    }

    IEnumerator WaitForSceneFade()
    {
        //Wait for screen wipe before allowing input
        yield return new WaitUntil(() => sceneFadeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        eventSystem.SetSelectedGameObject(startButton.gameObject);
        eventSystem.enabled = true;
    }

    private void Update()
    {
        //THIS IS PURELY TEST CODE
        //if (Input.GetKeyDown("f"))
        //{
        //    SaveSystem.DeleteSave();
        //}
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

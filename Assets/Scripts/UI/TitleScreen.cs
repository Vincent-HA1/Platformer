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
    [SerializeField] Button quitButton;


    // Start is called before the first frame update
    void Start()
    {
        //Hide cursor automatically
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        Time.timeScale = 1;
        startButton.onClick.AddListener(LoadScene);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            SaveSystem.DeleteSave();
        }
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

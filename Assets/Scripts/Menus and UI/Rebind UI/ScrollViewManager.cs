using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] List<Button> selectableElements;
    [SerializeField] List<RebindActionUICustom> rebindElements;

    [Header("Attributes")]
    [SerializeField] float scrollValueThreshold = 0.5f;
    [SerializeField] bool animate = false;
    [SerializeField] float smoothTime = 0.12f;

    float current = 1;
    List<Button> rebindButtons = new List<Button>();
    ScrollRect scrollRect;
    EventSystem eventSystem;

    GameObject lastSelectedGameobject;
    Button topButton;
    Button bottomButton;
    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        eventSystem = EventSystem.current;
        rebindButtons = rebindElements.Select(r => r.button).ToList(); //get the buttons of each rebind UI              
        topButton = rebindButtons[0];
        bottomButton = rebindButtons[0];
        //Find top and bottom button in the scroll view
        foreach (Button button in rebindButtons)
        {
            //If above top button
            if (button.transform.position.y < bottomButton.transform.position.y)
            {
                bottomButton = button;
            }
            //If below bottom button
            else if (button.transform.position.y > topButton.transform.position.y)
            {
                topButton = button;
            }
        }
    }

    private void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 1; //start the scroll rect at the top
        current = 1;
    }

    private void OnDisable()
    {
        lastSelectedGameobject = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (eventSystem.currentSelectedGameObject != null)
        {
            //Check if the element is in the list. If so:
            float index = rebindButtons.FindIndex(element => element.gameObject == eventSystem.currentSelectedGameObject);
            if (index != -1)
            {
                if (eventSystem.currentSelectedGameObject != lastSelectedGameobject && lastSelectedGameobject != null)
                {
                    Vector3 currentPos = eventSystem.currentSelectedGameObject.transform.position;
                    Vector3 lastPos = lastSelectedGameobject.transform.position;
                    //Just compare positions
                    if (lastPos.y < currentPos.y)
                    {
                        //Went up. So need to move screen up
                        current = Mathf.Clamp01(current + 1f / (rebindButtons.Count - 1)); //1 - (index - indexOffsetAmount) / (selectableElements.Count - 1);
                                                                                           //scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1 + (current - scrollValueThreshold)); //make sure it stays within 0 and 1
                    }
                    else if (lastPos.y > currentPos.y)
                    {
                        //Go down
                        current = Mathf.Clamp01(current - 1f / (rebindButtons.Count - 1)); //1 - (index - indexOffsetAmount) / (selectableElements.Count - 1);
                                                                                           //scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1 + (current - scrollValueThreshold)); //make sure it stays within 0 and 1
                    }
                    //Check for top and bottom button. Snap to top and bottom if the case
                    if (eventSystem.currentSelectedGameObject.transform.position.y >= topButton.gameObject.transform.position.y)
                    {
                        current = 1;
                    }
                    else if (eventSystem.currentSelectedGameObject.transform.position.y <= bottomButton.gameObject.transform.position.y)
                    {
                        current = 0;
                    }
                    //Smooth scroll or snap to scroll
                    if (!animate || smoothTime <= 0f)
                    {
                        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(1 + (current - scrollValueThreshold));
                    }
                    else
                    {
                        StopAllCoroutines();
                        StartCoroutine(SmoothScrollTo(Mathf.Clamp01(1 + (current - scrollValueThreshold))));
                    }
                }

                lastSelectedGameobject = eventSystem.currentSelectedGameObject;
            }
        }
    }

    IEnumerator SmoothScrollTo(float targetNormalized)
    {
        float start = scrollRect.verticalNormalizedPosition;
        float t = 0f;
        // we will use simple damped lerp
        while (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetNormalized) > 0.001f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, smoothTime);
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(start, targetNormalized, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        scrollRect.verticalNormalizedPosition = targetNormalized;
    }
}

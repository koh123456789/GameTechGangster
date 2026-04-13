using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscene Settings")]
    public PlayableDirector timelineDirector;
    public GameObject objectToActivate;
    public GameObject objectToDeActivate2;
    public GameObject objectToDeActivate;

    void Start()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnCutsceneEnded;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (timelineDirector != null && timelineDirector.state == PlayState.Playing)
            {
                timelineDirector.time = timelineDirector.duration;
                timelineDirector.Evaluate();

                timelineDirector.Stop();
            }
        }
    }

    private void OnCutsceneEnded(PlayableDirector director)
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log("Cutscene ended. Object activated!");
        }
        if (objectToDeActivate2 != null)
            {
                objectToDeActivate2.SetActive(false);
                Debug.Log("Cutscene ended. Object activated!");
            }
        if (objectToDeActivate != null)
        {
            objectToDeActivate.SetActive(false);
            Debug.Log("Cutscene ended. Object deactivated!");
        }
    }

    void OnDestroy()
    {
        if (timelineDirector != null)
        {
            timelineDirector.stopped -= OnCutsceneEnded;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections;
using System.Collections.Generic;

public class GoalManager : MonoBehaviour
{
    public enum OnboardingGoals
    {
        Empty,
        FindSurfaces,
        TapSurface
    }

    [System.Serializable]
    public class Step
    {
        public GameObject stepObject;
        public string buttonText;
        public bool includeSkipButton;
    }

    [SerializeField] List<Step> m_StepList = new List<Step>();
    [SerializeField] ObjectSpawner m_ObjectSpawner;
    [SerializeField] GameObject m_GreetingPrompt;
    [SerializeField] ARTemplateMenuManager m_MenuManager;

    const int k_NumberOfSurfacesTappedToCompleteGoal = 1;

    private Queue<Goal> m_OnboardingGoals;
    private Coroutine m_CurrentCoroutine;
    private Goal m_CurrentGoal;
    private bool m_AllGoalsFinished;
    private int m_SurfacesTapped;
    private int m_CurrentGoalIndex = 0;

    void Update()
    {
        if (Pointer.current != null && 
            Pointer.current.press.wasPressedThisFrame && 
            !m_AllGoalsFinished && 
            m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
        {
            if (m_CurrentCoroutine != null)
            {
                StopCoroutine(m_CurrentCoroutine);
            }
            CompleteGoal();
        }
    }

    void CompleteGoal()
    {
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
            m_ObjectSpawner.objectSpawned -= OnObjectSpawned;

        m_CurrentGoal.Completed = true;
        m_CurrentGoalIndex++;

        if (m_OnboardingGoals.Count > 0)
        {
            m_CurrentGoal = m_OnboardingGoals.Dequeue();

            if (m_CurrentGoalIndex - 1 >= 0 && m_CurrentGoalIndex - 1 < m_StepList.Count)
                m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);

            if (m_CurrentGoalIndex >= 0 && m_CurrentGoalIndex < m_StepList.Count)
                m_StepList[m_CurrentGoalIndex].stepObject.SetActive(true);

            PreprocessGoal();
        }
        else
        {
            if (m_CurrentGoalIndex - 1 >= 0 && m_CurrentGoalIndex - 1 < m_StepList.Count)
                m_StepList[m_CurrentGoalIndex - 1].stepObject.SetActive(false);

            m_AllGoalsFinished = true;
            m_MenuManager.enabled = true;
        }
        
    }

    void PreprocessGoal()
    {
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.FindSurfaces)
        {
            m_CurrentCoroutine = StartCoroutine(WaitUntilNextCard(5f));
        }
        else if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface)
        {
            m_SurfacesTapped = 0;
            m_ObjectSpawner.objectSpawned += OnObjectSpawned;
        }
    }

    public IEnumerator WaitUntilNextCard(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!Pointer.current.press.wasPressedThisFrame)
        {
            m_CurrentCoroutine = null;
            CompleteGoal();
        }
    }

    public void ForceCompleteGoal()
    {
        CompleteGoal();
    }

    void OnObjectSpawned(GameObject spawnedObject)
    {
        m_SurfacesTapped++;
        if (m_CurrentGoal.CurrentGoal == OnboardingGoals.TapSurface && 
            m_SurfacesTapped >= k_NumberOfSurfacesTappedToCompleteGoal)
        {
            CompleteGoal();
        }
    }

    public void StartCoaching()
    {
        if (m_OnboardingGoals != null)
        {
            m_OnboardingGoals.Clear();
        }

        m_OnboardingGoals = new Queue<Goal>();
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.FindSurfaces));
        m_OnboardingGoals.Enqueue(new Goal(OnboardingGoals.TapSurface));

        m_CurrentGoal = m_OnboardingGoals.Dequeue();
        m_AllGoalsFinished = false;
        m_CurrentGoalIndex = 0;

        m_GreetingPrompt.SetActive(false);
        m_MenuManager.enabled = false;

        for (int i = 0; i < m_StepList.Count; i++)
        {
            m_StepList[i].stepObject.SetActive(i == 0);
        }

        PreprocessGoal();
    }

    private struct Goal
    {
        public OnboardingGoals CurrentGoal;
        public bool Completed;

        public Goal(OnboardingGoals goal)
        {
            CurrentGoal = goal;
            Completed = false;
        }
    }
}
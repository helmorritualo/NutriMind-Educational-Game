using UnityEngine;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Single-scene panel switcher for Phase 8B Quiz Portal units.
    /// ponytail: only HomePanel is implemented; other roots are optional serialized refs.
    /// </summary>
    public class QuizPortalNavigationController : MonoBehaviour
    {
        [Header("Panel Roots")]
        [SerializeField] private GameObject _homePanelRoot;
        [SerializeField] private GameObject _availableQuizListRoot;
        [SerializeField] private GameObject _quizResultsRoot;

        public void SetHomePanelRoot(GameObject val) => _homePanelRoot = val;
        public void SetAvailableQuizListRoot(GameObject val) => _availableQuizListRoot = val;
        public void SetQuizResultsRoot(GameObject val) => _quizResultsRoot = val;

        private void Awake()
        {
            ShowHome();
        }

        public void ShowHome()
        {
            SetActivePanel(_homePanelRoot, true);
            SetActivePanel(_availableQuizListRoot, false);
            SetActivePanel(_quizResultsRoot, false);
        }

        public void ShowAvailableQuizList()
        {
            if (_availableQuizListRoot == null)
            {
                Debug.Log("[QuizPortalNavigationController] Available Quiz List panel not built yet (Phase 8B next unit).");
                return;
            }

            SetActivePanel(_homePanelRoot, false);
            SetActivePanel(_availableQuizListRoot, true);
            SetActivePanel(_quizResultsRoot, false);
        }

        public void ShowQuizResults()
        {
            if (_quizResultsRoot == null)
            {
                Debug.Log("[QuizPortalNavigationController] My Quiz Results panel not built yet (deferred).");
                return;
            }

            SetActivePanel(_homePanelRoot, false);
            SetActivePanel(_availableQuizListRoot, false);
            SetActivePanel(_quizResultsRoot, true);
        }

        private static void SetActivePanel(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NutriMind.Runtime.App
{
    /// <summary>
    /// Term 1 mission pick screen (Term_1_SelectMissions).
    /// Mission 1 uses the shared Loading transition; back returns to LiteraQuestTerms.
    /// </summary>
    public class MissionSelectionController : MonoBehaviour
    {
        private const string Mission1SceneKey = "LiteraQuestTerm1Mission1";

        [SerializeField] private Button _mission1PlayButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private GraphicRaycaster _graphicRaycaster;

        private bool _isTransitioning;

        public void SetMission1PlayButton(Button val) => _mission1PlayButton = val;
        public void SetBackButton(Button val) => _backButton = val;
        public void SetGraphicRaycaster(GraphicRaycaster val) => _graphicRaycaster = val;

        private void Awake()
        {
            var missionsRoot = GameObject.Find("Missions")?.transform;
            if (missionsRoot != null)
            {
                if (_graphicRaycaster == null)
                {
                    _graphicRaycaster = missionsRoot.GetComponent<GraphicRaycaster>();
                }

                if (_backButton == null)
                {
                    _backButton = EnsureButton(FindDeepChild(missionsRoot, "back"));
                }

                if (_mission1PlayButton == null)
                {
                    var m1 = FindDeepChild(missionsRoot, "M1");
                    _mission1PlayButton = EnsureButton(
                        FindDeepChild(m1, "PlayButton") ?? FindDeepChild(m1, "pls btn"));
                }
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            if (_mission1PlayButton != null)
            {
                _mission1PlayButton.onClick.AddListener(OnMission1Selected);
            }
        }

        private void OnDestroy()
        {
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(OnBackClicked);
            }

            if (_mission1PlayButton != null)
            {
                _mission1PlayButton.onClick.RemoveListener(OnMission1Selected);
            }
        }

        private void OnBackClicked()
        {
            NavigateToScene("LiteraQuestTerms", AppState.SelectingTerm);
        }

        private void OnMission1Selected()
        {
            if (_isTransitioning) return;

            var root = CompositionRoot.Instance;
            if (root?.Session == null) return;

            if (root.Session.SubjectTermStore == null)
            {
                root.Session.SubjectTermStore = new SubjectTermStore();
            }

            root.Session.SubjectTermStore.PendingSceneKey = Mission1SceneKey;
            NavigateToScene("Loading", AppState.LoadingWorld);
        }

        private void NavigateToScene(string sceneKey, AppState targetState)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_graphicRaycaster != null)
            {
                _graphicRaycaster.enabled = false;
            }

            StartCoroutine(FadeAndLoadRoutine(sceneKey, targetState));
        }

        private IEnumerator FadeAndLoadRoutine(string sceneKey, AppState targetState)
        {
            var root = CompositionRoot.Instance;
            if (root?.StateMachine != null)
            {
                root.StateMachine.TryTransition(targetState);
            }

            System.GC.Collect();

            if (targetState == AppState.LoadingWorld
                && root?.SceneRegistry?.GetScene("Loading") != null)
            {
                AppNavigation.LoadScene("Loading");
            }
            else
            {
                AppNavigation.LoadScene(sceneKey);
            }

            yield break;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            if (parent == null) return null;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == childName) return child;
                var nested = FindDeepChild(child, childName);
                if (nested != null) return nested;
            }

            return null;
        }

        private static Button EnsureButton(Transform target)
        {
            if (target == null) return null;

            var btn = target.GetComponent<Button>();
            if (btn == null)
            {
                btn = target.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
            }

            return btn;
        }
    }
}

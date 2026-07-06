using UnityEngine;

public enum G5Area01InteractionKind
{
    FarmerLira,
    StoryMapFragment
}

[RequireComponent(typeof(Collider))]
public class G5Area01InteractionTarget : MonoBehaviour
{
    [SerializeField] private G5Area01MissionController _mission;
    [SerializeField] private G5Area01InteractionKind _kind = G5Area01InteractionKind.FarmerLira;

    private void Reset()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        NotifyEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        NotifyExit();
    }

    private void NotifyEnter()
    {
        if (_mission == null)
        {
            Debug.LogWarning("[G5Area01InteractionTarget] Mission controller is not assigned.");
            return;
        }

        if (_kind == G5Area01InteractionKind.FarmerLira)
        {
            _mission.EnterFarmerLiraRange();
            return;
        }

        if (_kind == G5Area01InteractionKind.StoryMapFragment)
        {
            _mission.CollectStoryMapFragment();
        }
    }

    private void NotifyExit()
    {
        if (_mission == null)
        {
            return;
        }

        if (_kind == G5Area01InteractionKind.StoryMapFragment)
        {
            return;
        }

        if (_kind == G5Area01InteractionKind.FarmerLira)
        {
            _mission.ExitFarmerLiraRange();
        }
    }

    private static bool IsPlayer(Collider other)
    {
        if (other.GetComponentInParent<PlayerInteractionTrigger>() != null)
        {
            return true;
        }

        PlayerMovementController movement = other.GetComponentInParent<PlayerMovementController>();
        return movement != null && movement.GetComponentInChildren<PlayerInteractionTrigger>(true) != null;
    }
}

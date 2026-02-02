using Archipelago.MultiClient.Net;
using UnityEngine;

namespace CUAP;
public class ElderThornback : MonoBehaviour
{
    ArchipelagoSession Client;
    BuildingEntity build;
    private void Start()
    {
        Client = APClientClass.session;
        build = gameObject.GetComponent<BuildingEntity>();
    }
    private void OnDestroy()
    {
        if (build.health < 0.5 && build.fullName == "Elder thornback" && APClientClass.selectedGoal == 3) // the game checks at 0.5, so I will too
        {
            Client.SetGoalAchieved();
        }
    }
}

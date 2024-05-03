using System;
using System.Collections.Generic;

public class InfoManager : MonoSingleton<InfoManager>
{
    public List<GridInfoAssigner> currentGridInfo;

    public GridInfoAssigner GetCurrentInfo()
    {
        var completedSceneCount = GameManager.instance.GetTotalStagePlayed();
        completedSceneCount %= currentGridInfo.Count + 1;
        completedSceneCount = Math.Max(completedSceneCount, 1);
        return currentGridInfo[completedSceneCount - 1];
    }
}
using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public string userId;
    public string nickname;
    public int coin;
    public List<string> clearedStageIds = new List<string>();
    public List<string> unlockedStageIds = new List<string>();
}

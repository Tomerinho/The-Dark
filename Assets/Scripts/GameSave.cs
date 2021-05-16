// A class describing a game save, initializing essential parameters.
[System.Serializable]
public class GameSave
{
    // Essential positions and rotations are to be given to these fields.
    public (float x, float y, float z) playerPos;
    public (float x, float y, float z, float w) playerRot;
    public (float x, float y, float z) spiderPos;
    public (float x, float y, float z, float w) spiderRot;

    // Player parameters.
    public int playerHealth = 1000;

    // Spider parameters.
    public Spider.State spiderState = Spider.State.IDLE;
    public int spiderHealth = 48;
    public bool isVulnerable = false;
    public bool didReachCenter = false;
    public bool didHitWall = false;
    public bool isExitingIdle = false;
    public bool isDeadAnimTriggered = false;
}

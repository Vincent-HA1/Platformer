using UnityEngine;

/// <summary>
/// Class for all objects that allow a player to move on top of them (and for their movement to move the player with them)
/// </summary>
public class PlatformToFollow : MonoBehaviour
{
    protected PlayerMovement playerMovement;
    protected PlatformEnemy enemyToMove;

    public void SetPlayer(PlayerMovement playerMovement)
    {
        //Get on the platform
        this.playerMovement = playerMovement;
    }

    public void SetEnemy(PlatformEnemy enemyToMove)
    {
        this.enemyToMove = enemyToMove;
    }

    public void Disengage()
    {
        //Make the player get off the platform
        playerMovement = null;
        enemyToMove = null;
    }

    public void SetPlatformDelta(Vector2 difference)
    {
        if(playerMovement) playerMovement.SetPlatformDelta(difference);
        if(enemyToMove) enemyToMove.SetPlatformDelta(difference);
    }
}

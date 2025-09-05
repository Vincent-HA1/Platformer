using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SFXManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] PlayerMovement player;
    [SerializeField] GameObject collectiblesParent;
    [SerializeField] GameObject checkpointsParent;
    [SerializeField] GameObject enemiesParent;
    [SerializeField] GameObject blocksParent;

    [Header("Sound Effects")]
    [SerializeField] GameObject jumpSound;
    [SerializeField] GameObject playerHitSound;
    [SerializeField] GameObject playerKickSound;
    [SerializeField] GameObject enemyHitSound;
    [SerializeField] GameObject enemyDeathSound;
    [SerializeField] GameObject bounceSound;
    [SerializeField] GameObject coinGetSound;
    [SerializeField] GameObject rockHitSound;
    [SerializeField] GameObject rockDestroySound;
    [SerializeField] GameObject eatSound;
    [SerializeField] GameObject flagGetSound;

    // Start is called before the first frame update
    void Start()
    {
        SubscribeToEvents();
    }

    void SubscribeToEvents()
    {
        List<Collectible> allCollectibles = collectiblesParent.GetComponentsInChildren<Collectible>().ToList();
        //Collectible events on pick up
        foreach (Collectible collectible in allCollectibles)
        {
            switch (collectible.GetCollectibleType())
            {
                case Collectible.CollectibleType.Coin:
                    collectible.PickedUp += () => SpawnSoundEffect(coinGetSound);
                    break;
            }
        }

        //Enemies
        List<Enemy> allEnemies = enemiesParent.GetComponentsInChildren<Enemy>().ToList();
        foreach(Enemy enemy in allEnemies)
        {
            enemy.Hit += () => SpawnSoundEffect(enemyHitSound);
            enemy.Death += () => SpawnSoundEffect(enemyDeathSound);
        }

        //Blocks
        List<Block> allBlocks = blocksParent.GetComponentsInChildren<Block>().ToList();
        foreach (Block block in allBlocks)
        {
            block.Hit += () => SpawnSoundEffect(rockHitSound);
            block.Break += () => SpawnSoundEffect(rockDestroySound);
        }
        //Checkpoints
        List<Checkpoint> checkpoints = checkpointsParent.GetComponentsInChildren<Checkpoint>().ToList();
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (!checkpoint.isEndFlag) checkpoint.CheckpointReached += (Checkpoint checkpoint) => SpawnSoundEffect(flagGetSound);
        }
        //Player
        player.Jump += () => SpawnSoundEffect(jumpSound);
        player.Hit += (float damage) => SpawnSoundEffect(playerHitSound);
        player.Healed += (float health) => SpawnSoundEffect(eatSound);
        player.Death += () => SpawnSoundEffect(playerHitSound);
        player.Bounce += () => SpawnSoundEffect(bounceSound);
        player.KickAction += () => SpawnSoundEffect(playerKickSound);
        
    }

    void SpawnSoundEffect(GameObject soundEffect)
    {
        Instantiate(soundEffect);
    }

}

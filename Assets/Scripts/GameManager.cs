﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayStats playerStats;

    public LevelManager LevelManager;

    public playerMovement PlayerMove;

    public playerMeleeAttack PlayerAttack;

    public InventoryManager inventory;
    
    public enum gameStatus
    {
        Pause,
        Run
    }
    public gameStatus currentState;

    public bool LoadingLevel;
    // Start is called before the first frame update
    private void Awake() {
        instance=this;
        playerStats =GameObject.FindObjectOfType<PlayStats>();
        LevelManager = GameObject.FindObjectOfType<LevelManager>();
    }
    void Start()
    {
        inventory=FindObjectOfType<InventoryManager>();
        LoadingLevel=false;
        PlayerMove=FindObjectOfType<playerMovement>();
        PlayerAttack=FindObjectOfType<playerMeleeAttack>();

        currentState=gameStatus.Run;


        //On second Level Double jump upgrade is available
        if(SceneManager.GetActiveScene().name!="1.Level0" && SceneManager.GetActiveScene().name!="0.MainMenu")
        {
            UpgradeDoubleJump();
        }

        if(SceneManager.GetActiveScene().name!="1.Level0" && SceneManager.GetActiveScene().name!="0.MainMenu" && SceneManager.GetActiveScene().name!="1.Level2")
        {
            UpgradeThrowMechanic();
        }
    }

    IEnumerator DestroyWithTimeObject(Transform obj,float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(obj.gameObject);
    }
    public void DestroyGameObjectTime(Transform obj,float time)
    {
        StartCoroutine(DestroyWithTimeObject(obj,time));
    }

    public void ShakeCamera(string EasyMediumHard)
    {
        cameraShake.instance.StartShakeCoroutine(EasyMediumHard);
    }

    public void AddUIItem(Sprite Icon)
    {
        Debug.Log("Add Item");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpgradeDoubleJump()
    {
        PlayerMove.ExtraJumps=1;
    }

    public void UpgradeThrowMechanic()
    {
        PlayerMove.GetComponent<playerMeleeAttack>().checkWrenchStatus.throwMechanicActive=true;
    }
    
    public void EndLevel()
    {
        if(!LoadingLevel)
        {
            LoadingLevel=true;
             Debug.Log("End Level");
            LevelManager.MoveToNextScene();
        }

       

    }

    public void GameOver()
    {
        if(!LoadingLevel)
        {
            LoadingLevel=true;
            Debug.Log("GameOver");

            if(LevelManager!=null)
            {
                LevelManager.RestartLevel();
            }
            else
            {
                RestartLevel(); 
            }

        }

    }

     public void RestartLevel()
    {
        // Get the current active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Reload the current scene
        SceneManager.LoadScene(currentScene.name);
    }

    private void UnloadInactiveScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene != SceneManager.GetActiveScene())
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }



    public void GameOverSequence(bool winscreen)
    {
        Sound_Manager.instance.PlayerSoundHandler_Loop.Stop();
        Sound_Manager.instance.PlayerSoundHandler_Once.Stop();
        if(!winscreen)
        {
            //Restart Level
            var checkpoint=PositionManager.instance.GetLastCheckpoint();
            if(checkpoint==null)
            {
                PlayerMove.GetComponent<Transform>().tag="NoCollision";
                currentState=gameStatus.Pause;
            
                PlayerMove.DeathAnimation();
                Sound_Manager.instance.playerSoundOnce("Die");                        

                UIHandler.instance.StopAllCoroutines();
                UIHandler.instance.DialogueWindow.gameObject.SetActive(false);
                UIHandler.instance.StartCoroutine(UIHandler.instance.GameOver(winscreen));
            }
            else
            {
                //respawnToCheckpoint
                PlayerMove.GetComponent<Transform>().tag="NoCollision";
                currentState=gameStatus.Pause;
            
                PlayerMove.DeathAnimation();
                Sound_Manager.instance.playerSoundOnce("Die"); 

                 UIHandler.instance.StopAllCoroutines();
                UIHandler.instance.DialogueWindow.gameObject.SetActive(false);
                StartCoroutine(respawnToCheckpoint(checkpoint));
            }
            
        }
        else
        {
            //End level go to next one
            PlayerMove.GetComponent<Transform>().tag="NoCollision";
            currentState=gameStatus.Pause;            

            UIHandler.instance.StopAllCoroutines();
            UIHandler.instance.DialogueWindow.gameObject.SetActive(false);
            UIHandler.instance.StartCoroutine(UIHandler.instance.GameOver(winscreen));
        }
        

    }

    public  IEnumerator respawnToCheckpoint(checkPointHandler checkpoint)
    {
        yield return new WaitForSeconds(2f);

        PlayerMove.gameObject.SetActive(false);
        PlayerMove.Graphics.transform.localPosition= PlayerMove.GraphicsOrigPos;
        PlayerMove.transform.position=(Vector2)checkpoint.position;  
        PlayerMove.restorePlayerMovement();
        PlayerMove.gameObject.GetComponent<PlayStats>().CurrentHealth=checkpoint.currentHealth;   
        PlayerMove.GetComponent<Transform>().tag="Player";
        PlayerMove.ResetAllAnimationTrigger();
        PlayerMove.anim.SetTrigger("Idle");


        yield return new WaitForSeconds(1f);

        currentState=gameStatus.Run;

         PlayerMove.gameObject.SetActive(true);

        checkpoint.respawn();

    }



    public void PauseMenu()
    {
        currentState= gameStatus.Pause;
        UIHandler.instance.PauseMenu.SetActive(true);
        Sound_Manager.instance.environmentSoundOnce("Pause");
        Debug.Log("Pause");
    }

    public void BackMenu()
    {
        LevelManager.BackMainMenu();
    }

    public void UnPauseMenu()
    {
        currentState= gameStatus.Run;
        UIHandler.instance.PauseMenu.SetActive(false);
        Sound_Manager.instance.environmentSoundOnce("UnPause");
        Debug.Log("Un Pause");
    }

    public string ExtractNumbers(string input)
    {
        // Find the index of the underscore
        int underscoreIndex = input.IndexOf('_');
        
        // If an underscore is found, extract the substring after it
        if (underscoreIndex != -1 && underscoreIndex + 1 < input.Length)
        {
            return input.Substring(underscoreIndex + 1);
        }

        // Return an empty string if no underscore is found or the underscore is at the end
        return string.Empty;
    }

}

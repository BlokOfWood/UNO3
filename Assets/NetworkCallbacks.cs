using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;
using UnityEngine.SceneManagement;

[BoltGlobalBehaviour]
public class NetworkCallbacks : Bolt.GlobalEventListener
{
    public override void BoltShutdownBegin(AddCallback registerDoneCallback)
    {
        registerDoneCallback(()=>
                {
                    SceneManager.LoadScene(0);
        });
    }
    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "GameScene")
        {
            foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (i.GetComponent<BoltEntity>().Source == null)
                {
                    i.GetComponent<PlayerController>().Load_Refs();
                    return;
                }
            }
        }
    }

    public override void OnEvent(PlusN evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source != null)
            return;

        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        BoltEntity target = gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID + 1];

        int number_of_players = 0;
        foreach (BoltEntity go in gsm.state.ConnectedPlayers)
        {
            if (go == null) break;
            number_of_players++;
        }
        if (gsm.state.CurrentPlayerID + 1 == number_of_players)
        {
            target = gsm.state.ConnectedPlayers[0];
        }

        gsm.Draw_From_Deck_Multiple(target, evnt.NumberOfCards);
    }

    public override void OnEvent(PutCard evnt)
    {
        if (BoltNetwork.FindEntity(evnt.target).Source != null)
            return;
        BoltNetwork.FindEntity(evnt.target).GetComponent<PlayerController>().Place_Card_Into_Hand(evnt.Color, evnt.Type, evnt.Index);
    }

    public override void OnEvent(GameStart evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source == null) GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().Start_Match();
    }

    public override void OnEvent(ChangeCardInDeck evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source == null)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().Place_Card_Into_Deck(evnt.Color, evnt.Type, evnt.Index);
        }
    }
    public override void OnEvent(NextRound evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source == null)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().Next_Round(evnt.Skip, evnt.Reverse);
        }
    }

    public override void OnEvent(NewTopDeck evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source == null)
        {
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().state.CurrentTopCard.Color = evnt.Color;
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().state.CurrentTopCard.Type = evnt.Type;
            GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().state.CurrentTopCard.Origin = evnt.OriginID;
        }
    }

    public override void OnEvent(NextPlayerMustDraw evnt)
    {
        
    }

    public override void OnEvent(SpecificPlusN evnt)
    {
        if (GameObject.FindGameObjectWithTag("GameController").GetComponent<BoltEntity>().Source != null)
            return;
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().Draw_From_Deck_Multiple(evnt.Target, evnt.NumberOfCards);
    }

    public override void Connected(BoltConnection connection)
    {
        if(SceneManager.GetActiveScene().buildIndex == 2 && BoltNetwork.IsServer)
        {
            connection.Disconnect();
        }
    }
}
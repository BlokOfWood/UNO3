using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUI : Bolt.GlobalEventListener
{
    int number_of_players = 1;
    public TMPro.TMP_Text connected_player_text;
    public UnityEngine.UI.Button start_button;
    public override void SceneLoadLocalDone(string scene)
    {
        GameObject player = BoltNetwork.Instantiate(BoltPrefabs.Player);
        GameObject.Find("ConnectedPlayersNumber").GetComponent<TMPro.TMP_Text>().text = number_of_players.ToString();
        player.GetComponent<PlayerController>().state.Name = GameObject.Find("GameManager").GetComponent<GameStateManager>().player_name; 
        if (BoltNetwork.IsClient)
        {
            start_button.enabled = false;
            BoltNetwork.Destroy(GameObject.Find("GameManager"));
            GameObject.Find("GameManager").tag = "Untagged";
            return;
        }
    }

    public override void Connected(BoltConnection connection)
    {
        number_of_players++;
        GameObject.Find("ConnectedPlayersNumber").GetComponent<TMPro.TMP_Text>().text = number_of_players.ToString();
    }
    public override void Disconnected(BoltConnection connection)
    {
        number_of_players--;
        GameObject.Find("ConnectedPlayersNumber").GetComponent<TMPro.TMP_Text>().text = number_of_players.ToString();
    }

    public void Start_Game()
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>().Start_Match();
    }
}
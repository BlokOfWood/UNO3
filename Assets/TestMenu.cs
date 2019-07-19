using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdpKit;
using System;

public class TestMenu : Bolt.GlobalEventListener
{
    public GameStateManager gsm;

    public void Start_Client()
    {
        DontDestroyOnLoad(GameObject.Find("GameManager"));
        GameObject.Find("GameManager").GetComponent<GameStateManager>().player_name = GameObject.Find("Name").GetComponent<TMPro.TMP_InputField>().text;
        if (BoltNetwork.IsRunning) return;
        BoltLauncher.StartClient();
    }

    public void Start_Server()
    {
        DontDestroyOnLoad(GameObject.Find("GameManager"));
        GameObject.Find("GameManager").GetComponent<GameStateManager>().player_name = GameObject.Find("Name").GetComponent<TMPro.TMP_InputField>().text;
        if (BoltNetwork.IsRunning || gsm.max_players == 0) return;
        BoltLauncher.StartServer();
    }

    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            string matchName = Guid.NewGuid().ToString();
            BoltNetwork.SetServerInfo(matchName, null);
            BoltNetwork.LoadScene("LobbyScene");
        }
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        foreach (KeyValuePair<Guid, UdpSession> session in sessionList)
        {
            UdpSession photonSession = session.Value as UdpSession;
            if (photonSession.Source == UdpSessionSource.Photon)
            {
                BoltNetwork.Connect(photonSession);
            }
        }
    }
}
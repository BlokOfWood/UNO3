using System;
using System.Collections;
using System.Collections.Generic;
using UdpKit;
using UnityEngine;

public class GameStateManager : Bolt.EntityBehaviour<IGameState>
{
    public string player_name = "";
    public int max_players = 2;
    public int player_count = 1;

    public void Next_Round(bool skip, bool reverse)
    {
        int number_of_players = 0;
        foreach (BoltEntity i in state.ConnectedPlayers)
        {
            if (i == null) break;
            number_of_players++;
        }
        if (skip)
        {
            if (state.CurrentPlayerID == number_of_players - 1) state.CurrentPlayerID = 0;
            else
                state.CurrentPlayerID++;
        }
        else if(reverse && number_of_players > 1)
        {
            for(int i = 0; i < Mathf.FloorToInt((float)number_of_players/2); i++)
            {
                if (i == state.CurrentPlayerID) state.CurrentPlayerID = number_of_players - 1 - i;
                if (number_of_players - 1 - i == state.CurrentPlayerID) state.CurrentPlayerID = i;
                var first_swapped = state.ConnectedPlayers[i];
                state.ConnectedPlayers[i] = state.ConnectedPlayers[number_of_players - 1 - i];
                state.ConnectedPlayers[number_of_players - 1 - i] = first_swapped;
            }
        }
        if (state.CurrentPlayerID == number_of_players - 1) state.CurrentPlayerID = 0;
        else
            state.CurrentPlayerID++;
    }
    public void Set_Max_Players(string value)
    {
        if (value == "")
        {
            max_players = 0;
            return;
        }
        int max_player_int = int.Parse(value);
        if (max_player_int > 1 && max_player_int < 9) max_players = max_player_int;
        else max_players = 0;
    }

    public void Set_Name(string value)
    {
        player_name = value;
    }

    public void Start_Match()
    {
        if (BoltNetwork.IsServer)
        {
            Fill_Deck();
            foreach (GameObject i in GameObject.FindGameObjectsWithTag("Player"))
            {
                Fill_Player_Hand(i.GetComponent<BoltEntity>());
                if (i.GetComponent<BoltEntity>().Source != null)
                    for(int x = 0; x < i.transform.childCount; x++)
                    {
                        i.transform.GetChild(x).gameObject.SetActive(false);
                    }
            }

            for(int i = 0; i < state.ConnectedPlayers.Length; i++)
            {
                state.ConnectedPlayers[i] = null;
            }
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("Player").Length; i++)
            {
                state.ConnectedPlayers[i] = GameObject.FindGameObjectsWithTag("Player")[i].GetComponent<BoltEntity>();
            }
            for(int i = 0; i < state.ConnectedPlayers.Length; i++)
            {
                if (state.ConnectedPlayers[i] == null) break;

                BoltEntity lowest_value = state.ConnectedPlayers[i];
                int index = i;
                for(int x = i; x < state.ConnectedPlayers.Length; x++)
                {
                    if (state.ConnectedPlayers[x] == null) continue;
                    if(state.ConnectedPlayers[x].NetworkId.PackedValue < lowest_value.NetworkId.PackedValue)
                    {
                        lowest_value = state.ConnectedPlayers[x];
                        index = x;
                    }
                }
                var swapped_value = state.ConnectedPlayers[i];
                state.ConnectedPlayers[i] = lowest_value;
                state.ConnectedPlayers[index] = swapped_value;

            }

            foreach (var i in state.Deck)
            {
                if (i.Type != -4 && i.Color != -1 && i.Type < 10)
                {
                    state.CurrentTopCard.Type = i.Type;
                    state.CurrentTopCard.Color = i.Color;
                    i.Type = -4;
                    i.Color = -4;
                    break;
                }
            }
            BoltNetwork.LoadScene("GameScene");
        }
    }
    public void Draw_From_Deck(IPlayerState player)
    {
        int color = 0;
        int type = -4;

        for(int i = 0; i < state.Deck.Length; i++)
        {
            if(state.Deck[i].Type != -4)
            {
                color = state.Deck[i].Color;
                type = state.Deck[i].Type;
                ChangeCardInDeck evnt = ChangeCardInDeck.Create();
                evnt.Type = -4;
                evnt.Index = i;
                evnt.Send();
                break;
            }
        }

        foreach(var i in player.Hand)
        {
            if(i.Type == -4)
            {
                i.Type = type;
                i.Color = color;
                return;
            }
        }
    }
    public void Draw_From_Deck_Multiple(BoltEntity target, int number_of_cards)
    {
        int last_hand_index = 0;
        int last_deck_index = 0;
        for (int x = 0; x < number_of_cards; x++)
        {
            int color = 0;
            int type = -4;
            for (int i = last_deck_index + 1; i < state.Deck.Length; i++)
            {
                if (state.Deck[i].Type != -4)
                {
                    color = state.Deck[i].Color;
                    type = state.Deck[i].Type;
                    state.Deck[i].Type = -4;
                    last_deck_index = i;
                    break;
                }
            }
            for (int i = last_hand_index + 1; i < target.GetState<IPlayerState>().Hand.Length; i++)
            {
                if (target.GetState<IPlayerState>().Hand[i].Type == -4)
                {
                    PutCard evnt = PutCard.Create();
                    evnt.Type = type;
                    evnt.Color = color;
                    evnt.Index = i;
                    evnt.target = target.NetworkId;
                    evnt.Send();
                    last_hand_index = i;
                    break;
                }
            }
        }
        return;
    }

    public void Place_Card_Into_Deck(int card_color, int card_type)
    {
        int index = UnityEngine.Random.Range(0, state.Deck.Length);
        while (state.Deck[index].Type != -4)
        {
            index = UnityEngine.Random.Range(0, state.Deck.Length);
        }
        state.Deck[index].Type = card_type;
        state.Deck[index].Color = card_color;
    }
    public void Place_Card_Into_Deck(int card_color, int card_type, int index)
    {
        state.Deck[index].Type = card_type;
        state.Deck[index].Color = card_color;
    }

    public void Fill_Deck()
    {
        foreach(var i in state.Deck)
        {
            i.Type = -4;
        }
        for (int card_color = 0; card_color < 4; card_color++)
        {
            for (int card_type = 0; card_type < 13; card_type++)
            {
                Place_Card_Into_Deck(card_color, card_type);
                if (card_type != 0)
                    Place_Card_Into_Deck(card_color, card_type);
            }
        }
        for (int i = 0; i < 4; i++)
        {
            Place_Card_Into_Deck(-1, 1);
            Place_Card_Into_Deck(-1, 2);
        }
    }

    public void Fill_Player_Hand(BoltEntity player)
    {
        IPlayerState player_state = player.GetState<IPlayerState>();
        for(int i = 0; i < player_state.Hand.Length; i++)
        {
            PutCard evnt = PutCard.Create();
            evnt.Index = i;
            evnt.Type = -4;
            evnt.Color = 0;
            evnt.target = player.NetworkId;
            evnt.Send();
        }
        for(int i = 0; i < 7; i++)
        {
            int index = UnityEngine.Random.Range(0, state.Deck.Length);
            while(state.Deck[index].Type == -4)
            {
                index = UnityEngine.Random.Range(0, state.Deck.Length);
            }

            PutCard evnt = PutCard.Create();
            evnt.Color = state.Deck[index].Color;
            evnt.Type = state.Deck[index].Type;
            evnt.Index = i;
            evnt.target = player.NetworkId;
            evnt.Send();
            state.Deck[index].Type = -4;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Bolt.EntityBehaviour<IPlayerState>
{
    /* !!!! Don't forget to open the Bolt menus in Unity. That's where you actually find where I set the networked variables.*/
    public Color on_turn_color;
    public Color off_turn_color;
    int draw_cards = 0;
    GameObject[] card_objects;
    GameObject top_card;
    public int page_number = 0;

    public override void SimulateOwner()
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        if (top_card)
        {
            top_card.GetComponent<Card>().color = (Card.Card_Color)gsm.state.CurrentTopCard.Color;
            top_card.GetComponent<Card>().type = (Card.Card_Type)gsm.state.CurrentTopCard.Type;

            //Is responsible for displaying cards on multiple pages.
            for (int i = 0; i < 10; i++)
            {
                card_objects[i].GetComponent<Card>().type = (Card.Card_Type)state.Hand[page_number * 10 + i].Type;
                card_objects[i].GetComponent<Card>().color = (Card.Card_Color)state.Hand[page_number * 10 + i].Color;
            }
            //This should be weirdly self-explainatory. Checks if the current player is the player who has their turn by comparing NetworkID-s.
            //NetworkID-s are really nice UIDs that identify clients.
            if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId == GetComponent<BoltEntity>().NetworkId)
            {
                Camera.main.backgroundColor = on_turn_color;
            }
            else
            {
                Camera.main.backgroundColor = off_turn_color;
            }
        }
    }

    public void Place_Card_Into_Hand(int card_color, int card_type, int index)
    {
        state.Hand[index].Color = card_color;
        state.Hand[index].Type = card_type;
    }

    //Reading this will definitely give a good idea of how things are done in Bolt town.
    public void Place_Card(int hand_index)
    {
        //This GSM component seems to be a window into the state of the networked variables.
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();

        int player_id = 0;
        //Wow, I wrote a linear search algorithm. So amusing to see a complete replica of how they teach it in school, without having studied programming at this point.
        //This basically finds the index of the player in the list of connected players.
        //You see "entity" is in reference to the player's "BoltEntity", which basically contains their client data or something like that.
        for (int i = 0; i < gsm.state.ConnectedPlayers.Length; i++)
        {
            if (gsm.state.ConnectedPlayers[i].NetworkId == entity.NetworkId)
            {
                player_id = i;
                break;
            }
        }

        //The context for hand_index is that this is called from the UI, with each card from left to right having a manually given hand index.
        var card = state.Hand[page_number * 10 + hand_index];
        //Why exactly was card.Type too hard to type, but card.Color not?
        //Why did I make a variable for this I wonder.
        int card_type = card.Type;

        if (gsm.state.CurrentTopCard.Color == card.Color && gsm.state.CurrentTopCard.Type == card_type && card.Color != -1 && card_type < 9)
        {
            //I do remember this needing an incredible amount of fucking around to make work.
            //I'm not exactly sure what this is. Perhaps it draws a new card?
            NewTopDeck ntd_evnt = NewTopDeck.Create();
            ntd_evnt.Color = card.Color;
            ntd_evnt.Type = card_type;
            ntd_evnt.OriginID = player_id;
            ntd_evnt.Send();

            state.Hand[page_number * 10 + hand_index].Type = -4;
            if (page_number * 10 + hand_index != state.Hand.Length - 1)
                for (int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
                {
                    state.Hand[i - 1].Color = state.Hand[i].Color;
                    state.Hand[i - 1].Type = state.Hand[i].Type;
                }
        }

        //I have no idea why I bothered making go backwards if the maximum amount of connected players are 8.
        int last_connected_player = 0;
        for (int i = gsm.state.ConnectedPlayers.Length - 1; i > -1; i--)
        {
            if (gsm.state.ConnectedPlayers[i] != null)
            {
                last_connected_player = i;
                break;
            }
        }

        //Perhaps related to the denial card?
        bool last_player_denied = false;
        if (player_id > 0 && player_id - 1 == gsm.state.CurrentTopCard.Origin) last_player_denied = true;
        if (player_id == 0 && last_connected_player == gsm.state.CurrentTopCard.Origin) last_player_denied = true;

        if (gsm.state.CurrentTopCard.Type == (int)Card.Card_Type.DENIAL && card_type == (int)Card.Card_Type.DENIAL && last_player_denied)
        {
            NextRound nr_event = NextRound.Create();
            nr_event.Send();
        }

        if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId != GetComponent<BoltEntity>().NetworkId) return;

        //This is cool. This is the pick color thing for the plus four card.
        if (card.Color == -1)
        {
            if (card.Type == (int)Card.Card_Type.PLUS_FOUR)
            {
                draw_cards = 0;
            }
            GameObject.Find("Canvas").GetComponent<GameUI>().BringUpCardChoose((Card.Card_Type)card_type, page_number * 10 + hand_index);
            state.Hand[page_number * 10 + hand_index].Type = -4;
            if (page_number * 10 + hand_index != state.Hand.Length - 1)
                for (int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
                {
                    state.Hand[i - 1].Color = state.Hand[i].Color;
                    state.Hand[i - 1].Type = state.Hand[i].Type;
                }
        }
        else if (gsm.state.CurrentTopCard.Color == card.Color || gsm.state.CurrentTopCard.Type == card_type)
        {
            Debug.Log("lol"); //stylish.
            if (card_type == (int)Card.Card_Type.PLUS_TWO)
            {
                PlusN evnt = PlusN.Create();
                evnt.NumberOfCards = 2;
                evnt.Send();
            }
            NewTopDeck ntd_evnt = NewTopDeck.Create();
            ntd_evnt.Color = card.Color;
            ntd_evnt.Type = card_type;
            ntd_evnt.OriginID = player_id;
            ntd_evnt.Send();
            state.Hand[page_number * 10 + hand_index].Type = -4;
            if (page_number * 10 + hand_index != state.Hand.Length - 1)
                for (int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
                {
                    state.Hand[i - 1].Color = state.Hand[i].Color;
                    state.Hand[i - 1].Type = state.Hand[i].Type;
                }
            Debug.Log(card_type);
            NextRound nr_evnt = NextRound.Create();
            nr_evnt.Skip = (card_type == (int)Card.Card_Type.DENIAL);
            nr_evnt.Reverse = (card_type == (int)Card.Card_Type.REVERSE);
            nr_evnt.Send();
        }
        SpecificPlusN draw_from_deck = SpecificPlusN.Create();
        draw_from_deck.NumberOfCards = draw_cards;
        draw_from_deck.Target = entity;
        draw_from_deck.Send();
    }

    public void Load_Refs()
    {
        GameObject card_parent = GameObject.Find("Cards");
        card_objects = new GameObject[10];
        for (int i = 0; i < 10; i++)
        {
            card_objects[i] = card_parent.transform.GetChild(i).gameObject;
        }

        top_card = GameObject.Find("TopCard");
    }
}
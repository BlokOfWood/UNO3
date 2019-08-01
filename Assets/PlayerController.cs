using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Bolt.EntityBehaviour<IPlayerState>
{
    public Color on_turn_color;
    public Color off_turn_color;
    int draw_cards = 0;
    GameObject[] card_objects;
    GameObject top_card;
    public int page_number = 0;

    public override void SimulateOwner()
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        top_card.GetComponent<Card>().color = (Card.Card_Color) gsm.state.CurrentTopCard.Color;
        top_card.GetComponent<Card>().type = (Card.Card_Type)gsm.state.CurrentTopCard.Type;
        for (int i = 0; i < 10; i++)
        {
            card_objects[i].GetComponent<Card>().type = (Card.Card_Type)state.Hand[page_number * 10 + i].Type;
            card_objects[i].GetComponent<Card>().color = (Card.Card_Color)state.Hand[page_number * 10 + i].Color;
        }
        if(gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId == GetComponent<BoltEntity>().NetworkId)
        {
            Camera.main.backgroundColor = on_turn_color;
        }
        else
        {
            Camera.main.backgroundColor = off_turn_color;
        }
    }

    public void Place_Card_Into_Hand(int card_color, int card_type, int index)
    {
        state.Hand[index].Color = card_color;
        state.Hand[index].Type = card_type;
    }

    public void Place_Card(int hand_index)
    {
        GameStateManager gsm = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameStateManager>();
        var card = state.Hand[page_number * 10 + hand_index];

        if (gsm.state.CurrentTopCard.Color == card.Color && gsm.state.CurrentTopCard.Type == card.Type && card.Color != -1 && card.Type < 9)
        {
            NewTopDeck ntd_evnt = NewTopDeck.Create();
            ntd_evnt.Color = card.Color;
            ntd_evnt.Type = card.Type;
            ntd_evnt.Send();
            state.Hand[page_number * 10 + hand_index].Type = -4;
            if (page_number * 10 + hand_index != state.Hand.Length - 1)
                for (int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
                {
                    state.Hand[i - 1].Color = state.Hand[i].Color;
                    state.Hand[i - 1].Type = state.Hand[i].Type;
                }
        }

        if (gsm.state.ConnectedPlayers[gsm.state.CurrentPlayerID].NetworkId != GetComponent<BoltEntity>().NetworkId) return;

        if (card.Color == -1)
        {
            GameObject.Find("Canvas").GetComponent<GameUI>().BringUpCardChoose((Card.Card_Type)card.Type, page_number*10 + hand_index);
            state.Hand[page_number * 10 + hand_index].Type = -4;
            if (page_number * 10 + hand_index != state.Hand.Length - 1)
                for (int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
                {
                    state.Hand[i - 1].Color = state.Hand[i].Color;
                    state.Hand[i - 1].Type = state.Hand[i].Type;
                }
        }
        else if(gsm.state.CurrentTopCard.Color == card.Color || gsm.state.CurrentTopCard.Type == card.Type)
        {
            if (card.Type == (int)Card.Card_Type.PLUS_TWO)
            {
                PlusN evnt = PlusN.Create();
                evnt.NumberOfCards = 2;
                evnt.Send();
            }
            NewTopDeck ntd_evnt = NewTopDeck.Create();
            ntd_evnt.Color = card.Color;
            ntd_evnt.Type = card.Type;
            ntd_evnt.Send();
            state.Hand[page_number * 10 + hand_index].Type = -4;
            if(page_number * 10 + hand_index != state.Hand.Length-1)
            for(int i = page_number * 10 + hand_index + 1; i < state.Hand.Length; i++)
            {
                state.Hand[i - 1].Color = state.Hand[i].Color;
                state.Hand[i - 1].Type = state.Hand[i].Type;
            }
            NextRound nr_evnt = NextRound.Create();
            nr_evnt.Skip = (card.Type == (int)Card.Card_Type.DENIAL);
            nr_evnt.Reverse = (card.Type == (int)Card.Card_Type.REVERSE);
            nr_evnt.Send();
        }
    }

    public void Load_Refs()
    {
        GameObject card_parent = GameObject.Find("Cards");
        card_objects = new GameObject[10];
        for(int i = 0; i < 10; i++)
        {
            card_objects[i] = card_parent.transform.GetChild(i).gameObject;
        }

        top_card = GameObject.Find("TopCard");
    }
}
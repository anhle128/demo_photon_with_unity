﻿using System;
using System.Collections.Generic;

public class Player : Actor
{

    public float velocityX = 0f; // the X velocity of the player
    public float velocityY = 0f; // the Y velocity of the player
    public int score = 0; // the number of stars this player has collected

    public Player() 
    {
        this.actorType = (byte)ActorType.Player;
        this.radius = 0.5f;
    }

    public void Simulate(float timestep)
    {
        this.posX += this.velocityX * timestep;
        this.posY += this.velocityY * timestep;
    }
}

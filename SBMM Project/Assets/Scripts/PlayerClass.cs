using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class PlayerClass
{
    public string playerName;
    public float playerSkill;
    public float newPlayerSkill = 0.0f;
    public float kad;
    public int daysSinceLastPlay;
    public float RD = 0;
    public int teamScore = 2;

    public float winLossAddtion;
    public bool ratedPlayer = false;
    public int wins;
    public int loses;
    public int gamesPlayed = 0;

    public void CalculateRD_AfterAbsent()
    {
        RD = RD / 2.5f;
        if (RD < 1) { RD = 0; }
    }
}


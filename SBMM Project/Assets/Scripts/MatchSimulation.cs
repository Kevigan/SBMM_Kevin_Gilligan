using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using TMPro;
using System.Reflection;

public class MatchSimulation : MonoBehaviour
{
    public List<PlayerClass> playerList = new List<PlayerClass>();
    public List<PlayerClass> lobbyList = new List<PlayerClass>();

    public GameObject warningPanel;

    private int MaxLowPlayers = 30;
    private int MaxMiddlePlayers = 40;
    private int MaxHighPlayers = 30;

    private int skillDeviation = 5;

    public struct Team
    {
        public List<PlayerClass> playerList;
        public float teamAverageSkill;
        public float winningChance;
    }

    public Team team_A;
    public Team team_B;

    private PlayerClass createdPlayer;
    private List<PlayerClass> createdPremadeTeam = new List<PlayerClass>();

    private bool bSimulateGame = false;

    //Lobby and Teams
    [Header("Lobby and Teams")]
    public TextMeshProUGUI[] ui_team_A_Players;
    public TextMeshProUGUI[] ui_team_B_Players;
    public TextMeshProUGUI ui_lowestSkillInLobby;
    public TextMeshProUGUI ui_highestSkillInLobby;
    public TextMeshProUGUI ui_lobbySize;
    public TextMeshProUGUI ui_lobbyAverageSkill;
    public TextMeshProUGUI ui_team_A_WinningChance;
    public TextMeshProUGUI ui_team_B_WinningChance;
    public TextMeshProUGUI ui_team_A_PremadeAddition;
    public TextMeshProUGUI ui_Team_AaverageTeamSkillWithoutAdditionalPremade;

    //Create a Player
    [Header("Created Player")]
    public TMP_InputField ui_createdPlayerName;
    public TMP_InputField ui_createdPlayerSkill;
    public TextMeshProUGUI uiPanel_createdPlayerName;
    public TextMeshProUGUI uiPanel_createdPlayerSkill;
    public GameObject createPlayerPanel;
    public GameObject createPlayerInputPanel;
    private bool bPlayerCreated = false;

    //Create Premade Team
    public TMP_InputField[] ui_createdPremadePlayerName;
    public TMP_InputField[] ui_createdPremadePlayerSkill;
    public TextMeshProUGUI[] uiPanel_createdPreamdePlayerName;
    public TextMeshProUGUI[] uiPanel_createdPreamdePlayerSkill;
    public GameObject createPreamdePlayerPanel;
    public GameObject createPreamdePlayerInputPanel;
    private int premadeTeamSize;
    private float averagePremadeTeamSkill;
    public TextMeshProUGUI uiPanel_premadeTeamSize;
    public TextMeshProUGUI uiPanel_averagePremadeTeamSkill;
    private bool bPreamdeTeamCreated = false;
    float additionalPremadeSkill;

    //All Players
    [Header("All Players")]
    public TextMeshProUGUI ui_lowestSkillAllPlayers;
    public TextMeshProUGUI ui_highestSkillAllPlayers;
    public TextMeshProUGUI ui_availablePlayers;
    public TextMeshProUGUI ui_averageSkill;
    public TMP_InputField ui_Max_Low_Players;
    public TMP_InputField ui_Max_Middle_Players;
    public TMP_InputField ui_Max_High_Players;

    //Player info text
    public PlayerInfo playerInfo;
    public GameObject playerInfoGO;
    public GameObject matchInfo;
    public TextMeshProUGUI ui_MatchInfo;
    public TextMeshProUGUI ui_NewSkillAddition;
    public TextMeshProUGUI ui_playerKAD;
    public TextMeshProUGUI ui_playerRD;
    public TextMeshProUGUI ui_playerTotal;
    public TextMeshProUGUI ui_playerDaysAbsent;
    public TextMeshProUGUI ui_CurrentRD;

    //Simulate a Player

    //Simulate a Premade Team
    private float simulatedPremadeTeamSKill = 1500.0f;

    //KAD Settings
    public TMP_InputField kadMax_Win;
    public TMP_InputField kadMin_Win;
    public TMP_InputField kadMax_Loss;
    public TMP_InputField kadMin_Loss;

    //write to text
    private string filePath = @"C:\SBMM_Files\PlayerStats.txt";
    private string filePath2 = @"C:\SBMM_Files\Team_A_Stats.txt";
    private string filePath3 = @"C:\SBMM_Files\Team_B_Stats.txt";


    // Start is called before the first frame update
    void Start()
    {
        //GenerateRandomPlayers();
        ui_Max_Low_Players.text = "30";
        ui_Max_Middle_Players.text = "40";
        ui_Max_High_Players.text = "30";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreatePlayerInfoTextTeam_A(int index)
    {
        if (!bSimulateGame) return;
        playerInfoGO.SetActive(true);
        playerInfo = playerInfoGO.GetComponent<PlayerInfo>();
        playerInfo.playerSkill.text = "Skill: " + team_A.playerList[index].playerSkill.ToString();
        playerInfo.newPlayerSkill.text = "New Skill: " + team_A.playerList[index].newPlayerSkill.ToString();
        playerInfo.playerDaysNotPlayed.text = team_A.playerList[index].daysSinceLastPlay.ToString();
        if (team_A.playerList[index].ratedPlayer) playerInfo.playerRatedText.text = "Rated Player";
        else playerInfo.playerRatedText.text = "Unrated Player";

        playerInfo.CurrentRD.text = team_A.playerList[index].RD.ToString();
        if (team_A.playerList[index].teamScore == 0) playerInfo.teamScore.text = "TeamScore: lost";
        else if (team_A.playerList[index].teamScore == 1) playerInfo.teamScore.text = "TeamScore: win";
        else if (team_A.playerList[index].teamScore == 2) playerInfo.teamScore.text = "";

        if (team_A.playerList[index].newPlayerSkill != 0.0f)
        {
            float winLossAddtion = team_A.playerList[index].winLossAddtion;
            float roundedWinLossAddtion;
            roundedWinLossAddtion = (float)UnityEngine.Mathf.Floor(winLossAddtion * 100) / 100;
            if (roundedWinLossAddtion >= 0)
            {
                ui_NewSkillAddition.color = Color.green;
                ui_NewSkillAddition.text = "Win: " + roundedWinLossAddtion.ToString();
            }
            else
            {
                ui_NewSkillAddition.color = Color.red;
                ui_NewSkillAddition.text = "Loss: " + roundedWinLossAddtion.ToString();
            }

            if (team_A.playerList[index].newPlayerSkill > team_A.playerList[index].playerSkill)
            {
                ui_playerRD.color = Color.green;
                float RDRounded = (float)UnityEngine.Mathf.Floor((team_A.playerList[index].RD) * 100) / 100;
                ui_playerRD.text = "RD: +" + RDRounded.ToString();

                ui_playerTotal.color = Color.green;
                float totalRounded = (float)UnityEngine.Mathf.Floor((roundedWinLossAddtion + team_A.playerList[index].kad) * 100 + team_A.playerList[index].RD) / 100;
                ui_playerTotal.text = "Total: +" + (totalRounded).ToString();
            }
            else
            {
                ui_playerRD.color = Color.red;
                float RD = (float)UnityEngine.Mathf.Floor((team_A.playerList[index].RD) * 100) / 100;
                ui_playerRD.text = "RD: -" + RD.ToString();

                ui_playerTotal.color = Color.red;
                float totalRounded = (float)UnityEngine.Mathf.Floor((roundedWinLossAddtion + team_A.playerList[index].kad + (team_A.playerList[index].RD * -1)) * 100) / 100;
                ui_playerTotal.text = "Total: " + (totalRounded).ToString();
            }

            if (team_A.playerList[index].kad >= 0.0f)
            {
                float roundedValueKAD = (float)UnityEngine.Mathf.Floor(team_A.playerList[index].kad * 100) / 100;
                ui_playerKAD.text = "KAD: +" + roundedValueKAD.ToString();
                ui_playerKAD.color = Color.green;
            }
            else
            {
                float roundedValueKAD = (float)UnityEngine.Mathf.Floor(team_A.playerList[index].kad * 100) / 100;
                ui_playerKAD.text = "KAD: " + roundedValueKAD.ToString();
                ui_playerKAD.color = Color.red;
            }
        }
    }

    public void CreatePlayerInfoTextTeam_B(int index)
    {
        if (!bSimulateGame) return;
        playerInfoGO.SetActive(true);
        playerInfo = playerInfoGO.GetComponent<PlayerInfo>();
        playerInfo.playerSkill.text = "Skill: " + team_B.playerList[index].playerSkill.ToString();
        playerInfo.newPlayerSkill.text = "New Skill: " + team_B.playerList[index].newPlayerSkill.ToString();
        playerInfo.playerDaysNotPlayed.text = team_B.playerList[index].daysSinceLastPlay.ToString();
        playerInfo.CurrentRD.text = team_B.playerList[index].RD.ToString();

        if (team_B.playerList[index].ratedPlayer) playerInfo.playerRatedText.text = "Rated Player";
        else playerInfo.playerRatedText.text = "Unrated Player";

        if (team_B.playerList[index].teamScore == 0) playerInfo.teamScore.text = "TeamScore: lost";
        else if (team_B.playerList[index].teamScore == 1) playerInfo.teamScore.text = "TeamScore: win";
        else if (team_B.playerList[index].teamScore == 2) playerInfo.teamScore.text = "";

        if (team_B.playerList[index].newPlayerSkill != 0.0f)
        {
            float winLossAddtion = team_B.playerList[index].winLossAddtion;
            float roundedWinLossAddtion;
            roundedWinLossAddtion = (float)UnityEngine.Mathf.Floor(winLossAddtion * 100) / 100;
            if (roundedWinLossAddtion >= 0)
            {
                ui_NewSkillAddition.color = Color.green;
                ui_NewSkillAddition.text = "Win: " + roundedWinLossAddtion.ToString();
            }
            else
            {
                ui_NewSkillAddition.color = Color.red;
                ui_NewSkillAddition.text = "Loss: " + roundedWinLossAddtion.ToString();
            }
            if (team_B.playerList[index].newPlayerSkill > team_B.playerList[index].playerSkill)
            {

                ui_playerRD.color = Color.green;
                float RD = (float)UnityEngine.Mathf.Floor((team_B.playerList[index].RD) * 100) / 100;
                ui_playerRD.text = "RD: +" + RD.ToString();

                ui_playerTotal.color = Color.green;
                float totalRounded = (float)UnityEngine.Mathf.Floor((roundedWinLossAddtion + team_B.playerList[index].kad + team_B.playerList[index].RD) * 100) / 100;
                ui_playerTotal.text = "Total: +" + (totalRounded).ToString();
            }
            else
            {
                ui_playerRD.color = Color.red;
                float RD = (float)UnityEngine.Mathf.Floor((team_B.playerList[index].RD) * 100) / 100;
                ui_playerRD.text = "RD: -" + RD.ToString();

                ui_playerTotal.color = Color.red;
                float totalRounded = (float)UnityEngine.Mathf.Floor((roundedWinLossAddtion + team_B.playerList[index].kad + (team_B.playerList[index].RD * -1)) * 100) / 100;
                ui_playerTotal.text = "Total: " + (totalRounded).ToString();
            }

            if (team_B.playerList[index].kad >= 0.0f)
            {
                float roundedValue = (float)UnityEngine.Mathf.Floor(team_B.playerList[index].kad * 100) / 100;
                ui_playerKAD.text = "KAD: +" + roundedValue.ToString();
                ui_playerKAD.color = Color.green;
            }
            else
            {
                float roundedValue = (float)UnityEngine.Mathf.Floor(team_B.playerList[index].kad * 100) / 100;
                ui_playerKAD.text = "KAD: " + roundedValue.ToString();
                ui_playerKAD.color = Color.red;
            }
        }
    }

    public void RemovePlayerInfoText()
    {
        if (!bSimulateGame) return;
        playerInfo.playerSkill.text = "";
        playerInfoGO.SetActive(false);
    }

    public void SimulateMatch(int setManually) // 0 = not using, 1 = win Team A, 2 = win Team B
    {
        bool teamAWin = false;

        if (setManually == 0)
        {
            float randomValue = UnityEngine.Random.Range(0.0f, 100.0f) / 100;
            if (randomValue <= team_A.winningChance) teamAWin = true;
        }
        else if (setManually == 1) teamAWin = true;
        else if (setManually == 2) teamAWin = false;

        matchInfo.SetActive(true);

        if (teamAWin)
        {
            CalculateNewPlayerSkill(team_A, team_B, 1);
            CalculateNewPlayerSkill(team_B, team_A, 0);
            ui_MatchInfo.text = "Team A won.\nTeam B lost.\nHover over player for skill info before Continuing. ";
        }
        else
        {
            CalculateNewPlayerSkill(team_A, team_B, 0);
            CalculateNewPlayerSkill(team_B, team_A, 1);
            ui_MatchInfo.text = "Team A lost.\nTeam B won.\nHover over player before Continuing for skill info";
        }
    }

    public float SimulateKAD(int win)
    {
        float kad_max_Win = float.Parse(kadMax_Win.text);
        float kad_min_Win = float.Parse(kadMin_Win.text);

        float kad_max_Loss = float.Parse(kadMax_Loss.text);
        float kad_min_Loss = float.Parse(kadMin_Loss.text);


        float KAD = 0.0f;
        if (win == 1)
        {
            KAD = UnityEngine.Random.Range(kad_min_Win, kad_max_Win); //-0,5 3,0
        }
        else
        {
            KAD = UnityEngine.Random.Range(kad_min_Loss, kad_max_Loss);// -3 0.5
        }
        return KAD;
    }

    public void SimulateDaysNotPlayed()
    {
        foreach (PlayerClass player in team_A.playerList)
        {
            player.daysSinceLastPlay = UnityEngine.Random.Range(0, 100);
            CalculatePlayerRD(player);
        }
        foreach (PlayerClass player in team_B.playerList)
        {
            player.daysSinceLastPlay = UnityEngine.Random.Range(0, 100);
            CalculatePlayerRD(player);
        }
    }

    public void CalculatePlayerRD(PlayerClass player)
    {
        float C = 0.0f;
        if (ReturnSkillGroup(player.playerSkill) == 1) C = 0.6f; //K = 32
        else if (ReturnSkillGroup(player.playerSkill) == 2) C = 0.4f; //K = 24
        else if (ReturnSkillGroup(player.playerSkill) == 3) C = 0.2f; //K = 16

        player.RD = player.daysSinceLastPlay * C;
    }

    public void CalculateNewPlayerSkill(Team one, Team two, int win) //win == 1, lose == 0
    {
        foreach (PlayerClass player in one.playerList)
        {
            float newPlayerSkill = 0.0f;
            int K = 0;
            if (ReturnSkillGroup(player.playerSkill) == 1) K = 32;
            else if (ReturnSkillGroup(player.playerSkill) == 2) K = 24;
            else if (ReturnSkillGroup(player.playerSkill) == 3) K = 16;

            float KAD = SimulateKAD(win);
            player.kad = KAD;

            if (win == 0) player.loses++;
            else if (win == 1) player.wins++;
            player.gamesPlayed++;

            // choose formular depending on rated/ not rated
            if (player.ratedPlayer)
            {
                newPlayerSkill = player.playerSkill + K * (win - CalculateWinningChance(player.playerSkill, two.teamAverageSkill));
            }
            else
            {
                newPlayerSkill = player.playerSkill + ((100) * (player.wins - player.loses) / UnityEngine.Mathf.Clamp(player.gamesPlayed, 1, 10));
                print(player.wins);
                print(player.loses);
                print(player.gamesPlayed);
            }

            player.winLossAddtion = newPlayerSkill - player.playerSkill;

            if (win == 0)
            {
                player.teamScore = 0;
                player.newPlayerSkill = newPlayerSkill + (player.RD * -1) + player.kad;
                player.newPlayerSkill = UnityEngine.Mathf.Clamp(player.newPlayerSkill, 0, 3000);
            }
            else if (win == 1)
            {
                player.teamScore = 1;
                player.newPlayerSkill = newPlayerSkill + player.RD + player.kad;
                player.newPlayerSkill = UnityEngine.Mathf.Clamp(player.newPlayerSkill, 0, 3000);
            }
            if (player.gamesPlayed >= 10) player.ratedPlayer = true;
        }
    }

    public void SetNewToOld()// Set UI Skill text
    {
        if (team_A.playerList.Count > 0)
        {
            foreach (PlayerClass player in team_A.playerList)
            {
                player.playerSkill = player.newPlayerSkill;
                player.newPlayerSkill = 0.0f;
                player.teamScore = 2;
                player.CalculateRD_AfterAbsent();
                player.daysSinceLastPlay = 0;
            }
        }
        if (team_B.playerList.Count > 0)
        {
            foreach (PlayerClass player in team_B.playerList)
            {
                player.playerSkill = player.newPlayerSkill;
                player.newPlayerSkill = 0.0f;
                player.teamScore = 2;
                player.CalculateRD_AfterAbsent();
                player.daysSinceLastPlay = 0;
            }
        }
        ui_NewSkillAddition.text = "";
        ui_playerKAD.text = "";
        ui_playerRD.text = "";
        ui_playerTotal.text = "";
        team_A.teamAverageSkill = CalculateAverageSkill(team_A.playerList);
        team_B.teamAverageSkill = CalculateAverageSkill(team_B.playerList);
        team_A.winningChance = CalculateWinningChance(team_A.teamAverageSkill, team_B.teamAverageSkill);
        ui_team_A_WinningChance.text = team_A.winningChance.ToString() + "%";
        team_B.winningChance = CalculateWinningChance(team_B.teamAverageSkill, team_A.teamAverageSkill);
        ui_team_B_WinningChance.text = team_B.winningChance.ToString() + "%";
        AddPlayersToTeam_A_UI();
        AddPlayersToTeam_B_UI();
    }

    public void SetSimulatedMatch()
    {
        bSimulateGame = !bSimulateGame;
    }

    public float CalculateWinningChance(float averageSkillTeam_A, float averageSkillTeam_B)
    {
        float chance = 1 / (1 + MathF.Pow(10, -(averageSkillTeam_A - averageSkillTeam_B) / 400));
        return chance;
    }

    public void CreateLobby()
    {
        if (bPreamdeTeamCreated)
        {
            CreateLobbyWithPremadeTeam();
        }
        else
        {
            CreateLobbyWithoutPremadeTeam();
        }

        ui_team_A_WinningChance.text = "";
        ui_team_B_WinningChance.text = "";
    }

    public void CreateLobbyWithoutPremadeTeam()
    {
        RemovePlayersFromTeamATeamB_UI();
        UpdateAvailablePlayersUI();
        if (playerList.Count < 10)
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }
            return;
        }
        lobbyList.Clear();
        skillDeviation = 5;
        float averageSkill;
        if (bPlayerCreated && createdPlayer != null)
        {
            lobbyList.Add(createdPlayer);
            averageSkill = createdPlayer.playerSkill;
        }
        else
        {
            averageSkill = playerList[UnityEngine.Random.Range(0, playerList.Count)].playerSkill;
        }
        PlayerClass playerWithLowestSkill = new PlayerClass();

        List<PlayerClass> tempPlayerList = new List<PlayerClass>();

        foreach (PlayerClass pl in playerList)
        {
            tempPlayerList.Add(pl);
        }
        int index = 0;
        while (lobbyList.Count < 10)
        {
            foreach (PlayerClass player in tempPlayerList)
            {
                if (player.playerSkill > averageSkill - skillDeviation && player.playerSkill < averageSkill + skillDeviation)// searching players in certain range
                {
                    playerWithLowestSkill = player;
                }
                else
                {
                    continue;
                }
            }
            if (playerWithLowestSkill != null && playerWithLowestSkill.playerName != null)
            {
                lobbyList.Add(playerWithLowestSkill);
                averageSkill = CalculateAverageSkill(lobbyList);
                tempPlayerList.Remove(playerWithLowestSkill);
                playerList.Remove(playerWithLowestSkill);
                playerWithLowestSkill = null;
            }
            else
            {
                skillDeviation += 15; // No suitable player found, add search range
            }
            index++;
        }
        //update lobby UI
        UpdateLobbyUI();

        //update available players UI
        UpdateAvailablePlayersUI();
    }

    public void CreateLobbyWithPremadeTeam()
    {
        RemovePlayersFromTeamATeamB_UI();
        UpdateAvailablePlayersUI();
        if (playerList.Count < 10)
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(true);
            }
            return;
        }
        lobbyList.Clear();
        skillDeviation = 5;
        float averageSkill;
        if (bPreamdeTeamCreated && createdPremadeTeam.Count > 0)
        {
            foreach (PlayerClass player in createdPremadeTeam)
            {
                lobbyList.Add(player);
            }
            simulatedPremadeTeamSKill = CalculateAverageSkill(createdPremadeTeam);
            averageSkill = simulatedPremadeTeamSKill;

        }
        else
        {
            return;
        }
        PlayerClass playerWithLowestSkill = new PlayerClass();

        List<PlayerClass> tempPlayerList = new List<PlayerClass>();

        foreach (PlayerClass pl in playerList)
        {
            tempPlayerList.Add(pl);
        }

        while (lobbyList.Count < 10)
        {
            foreach (PlayerClass player in tempPlayerList)
            {
                if (player.playerSkill > averageSkill - skillDeviation && player.playerSkill < averageSkill + skillDeviation)// searching players in certain range
                {
                    playerWithLowestSkill = player;
                }
                else
                {
                    continue;
                }
            }
            if (playerWithLowestSkill != null && playerWithLowestSkill.playerName != null)
            {
                lobbyList.Add(playerWithLowestSkill);
                averageSkill = CalculateAverageSkill(lobbyList);
                tempPlayerList.Remove(playerWithLowestSkill);
                playerList.Remove(playerWithLowestSkill);
                playerWithLowestSkill = null;
            }
            else
            {
                skillDeviation += 15;// No suitable player found, add search range
            }
        }
        //update lobby UI
        UpdateLobbyUI();

        //update available players UI
        UpdateAvailablePlayersUI();
    }

    public void FindLobby()
    {

    }

    public void CreatePlayer()
    {
        if (ui_createdPlayerName.text.Length == 0 || ui_createdPlayerSkill.text.Length == 0) return;

        createdPlayer = new PlayerClass();
        createdPlayer.playerName = ui_createdPlayerName.text;
        uiPanel_createdPlayerName.text = ui_createdPlayerName.text;
        createdPlayer.playerSkill = UnityEngine.Mathf.Clamp(int.Parse(ui_createdPlayerSkill.text), 1000, 2000);
        uiPanel_createdPlayerSkill.text = createdPlayer.playerSkill.ToString();
        bPlayerCreated = true;
        createPlayerPanel.SetActive(true);
        createPlayerInputPanel.SetActive(false);
    }

    public void CreatePremadeTeam()
    {
        int index = 0;
        foreach (TMP_InputField inputf in ui_createdPremadePlayerName)
        {
            if (inputf.text.Length == 0 || ui_createdPremadePlayerSkill[index].text.Length == 0)
            {
                index++;
                continue;
            }
            PlayerClass teamPlayer = new PlayerClass();
            teamPlayer.playerName = inputf.text;
            uiPanel_createdPreamdePlayerName[index].text = inputf.text;
            teamPlayer.playerSkill = UnityEngine.Mathf.Clamp(int.Parse(ui_createdPremadePlayerSkill[index].text.ToString()), 1000, 2000);
            uiPanel_createdPreamdePlayerSkill[index].text = teamPlayer.playerSkill.ToString();
            createdPremadeTeam.Add(teamPlayer);
            premadeTeamSize++;
            index++;
        }
        if (createdPremadeTeam.Count > 1) bPreamdeTeamCreated = true;

        createPreamdePlayerPanel.SetActive(true);
        createPreamdePlayerInputPanel.SetActive(false);
        uiPanel_premadeTeamSize.text = "Premade Team Size: " + premadeTeamSize.ToString();
        uiPanel_averagePremadeTeamSkill.text = "Average Skill: " + CalculateAverageSkill(createdPremadeTeam).ToString();
    }

    public void CreateTeams()
    {
        skillDeviation = 5;

        List<PlayerClass> tempLobbyList = new List<PlayerClass>();
        foreach (PlayerClass pl in lobbyList)
        {
            tempLobbyList.Add(pl);
        }
        team_A = new Team();
        team_A.playerList = new List<PlayerClass>();
        team_B = new Team();
        team_B.playerList = new List<PlayerClass>();

        bool isTeamA = true;
        int iterationCount = 0;
        float averageSkill = 0.0f;
        additionalPremadeSkill = 0.0f;

        if (bPreamdeTeamCreated && createdPremadeTeam.Count > 0)
        {
            foreach (PlayerClass pl in createdPremadeTeam)
            {
                team_A.playerList.Add(pl);
                tempLobbyList.Remove(pl);
                iterationCount++;
            }
            averageSkill = CalculateAverageSkill(createdPremadeTeam);

            switch (ReturnSkillGroup(averageSkill))
            {
                case 0:
                    break;
                case 1:
                    additionalPremadeSkill = 20;
                    ui_team_A_PremadeAddition.text = "Additional Premade Skill: " + 20.ToString();
                    break;
                case 2:
                    additionalPremadeSkill = 30;
                    ui_team_A_PremadeAddition.text = "Additional Premade Skill: " + 30.ToString();
                    break;
                case 3:
                    additionalPremadeSkill = 50;
                    ui_team_A_PremadeAddition.text = "Additional Premade Skill: " + 50.ToString();
                    break;
            }
        }

        while (team_A.playerList.Count + team_B.playerList.Count < 10)
        {
            PlayerClass tempPlayer = tempLobbyList[UnityEngine.Random.Range(0, tempLobbyList.Count)];
            if (iterationCount != 0 && (tempPlayer.playerSkill < averageSkill - skillDeviation || tempPlayer.playerSkill > averageSkill + skillDeviation))
            {
                skillDeviation += 15;
                iterationCount++;
                continue;
            }

            if (isTeamA && !bPreamdeTeamCreated)
            {
                team_A.playerList.Add(tempPlayer);
                //team_A.teamAverageSkill = tempPlayer.playerSkill;
                isTeamA = !isTeamA;
                tempLobbyList.Remove(tempPlayer);
                iterationCount++;
            }
            else if (!isTeamA && !bPreamdeTeamCreated)
            {
                team_B.playerList.Add(tempPlayer);
                // team_B.teamAverageSkill = tempPlayer.playerSkill;
                isTeamA = !isTeamA;
                tempLobbyList.Remove(tempPlayer);
                iterationCount++;
            }
            else if (bPreamdeTeamCreated && team_A.playerList.Count < 5)
            {
                team_A.playerList.Add(tempPlayer);
                // team_A.teamAverageSkill = tempPlayer.playerSkill;
                tempLobbyList.Remove(tempPlayer);
                iterationCount++;
            }
            else if (bPreamdeTeamCreated && team_A.playerList.Count == 5)
            {
                team_B.playerList.Add(tempPlayer);
                //team_B.teamAverageSkill = tempPlayer.playerSkill;
                tempLobbyList.Remove(tempPlayer);
                iterationCount++;
            }
            averageSkill = CalculateAverageSkill(lobbyList);
        }

        team_A.teamAverageSkill = CalculateAverageSkill(team_A.playerList) + additionalPremadeSkill;
        team_B.teamAverageSkill = CalculateAverageSkill(team_B.playerList);

        //WriteToTextFile(filePath2, team_A);
        // WriteToTextFile(filePath3, team_B);

        AddPlayersToTeam_A_UI();
        AddPlayersToTeam_B_UI();
        team_A.winningChance = CalculateWinningChance(team_A.teamAverageSkill, team_B.teamAverageSkill);
        ui_team_A_WinningChance.text = team_A.winningChance.ToString() + "%";
        team_B.winningChance = CalculateWinningChance(team_B.teamAverageSkill, team_A.teamAverageSkill);
        ui_team_B_WinningChance.text = team_B.winningChance.ToString() + "%";

        if (bPreamdeTeamCreated)
        {
            ui_Team_AaverageTeamSkillWithoutAdditionalPremade.text = "Average Team Skill Without Additional Premade: " + (team_A.teamAverageSkill - additionalPremadeSkill).ToString();
        }
        //print(tempLobbyList.Count);
    }

    public int ReturnSkillGroup(float averageSkill)
    {
        if (averageSkill < 1300)
        {
            return 1;
        }
        else if (averageSkill >= 1300 && averageSkill < 1700)
        {
            return 2;
        }
        else if (averageSkill >= 1700)
        {
            return 3;
        }
        else return 0;
    }

    public void Reset() // Reset button, resets all
    {
        ui_lobbySize.text = "";
        ui_lowestSkillInLobby.text = "";
        ui_highestSkillInLobby.text = "";
        ui_lobbyAverageSkill.text = "";
        lobbyList.Clear();

        ui_availablePlayers.text = "";
        ui_averageSkill.text = "";
        ui_lowestSkillAllPlayers.text = "";
        ui_highestSkillAllPlayers.text = "";
        playerList.Clear();

        createdPlayer = null;
        bPlayerCreated = false;
        createPlayerPanel.SetActive(false);
        uiPanel_createdPlayerName.text = "";
        uiPanel_createdPlayerSkill.text = "";

        skillDeviation = 5;

        for (int i = 0; i < ui_team_A_Players.Length; i++)
        {
            ui_team_A_Players[i].text = "";
        }

        for (int i = 0; i < ui_team_B_Players.Length; i++)
        {
            ui_team_B_Players[i].text = "";
        }

        //reset premade team
        createdPremadeTeam.Clear();
        bPreamdeTeamCreated = false;
        premadeTeamSize = 0;
        createPreamdePlayerPanel.SetActive(false);
        foreach (TMP_InputField inputf in ui_createdPremadePlayerName)
        {
            inputf.text = "";
        }
        foreach (TMP_InputField inputf in ui_createdPremadePlayerSkill)
        {
            inputf.text = "";
        }
        ;
        foreach (TextMeshProUGUI text in uiPanel_createdPreamdePlayerName)
        {
            text.text = "";
        }
        foreach (TextMeshProUGUI text in uiPanel_createdPreamdePlayerSkill)
        {
            text.text = "";
        }

        ui_team_A_WinningChance.text = "";
        ui_team_B_WinningChance.text = "";
        ui_Team_AaverageTeamSkillWithoutAdditionalPremade.text = "";
        ui_team_A_PremadeAddition.text = "";
    }

    public float CalculateAverageSkill(List<PlayerClass> tempPlayerList)
    {
        float average = 0.0f;
        foreach (PlayerClass playerClass in tempPlayerList)
        {
            average += playerClass.playerSkill;
        }
        average = (average / tempPlayerList.Count);
        return average;
    }

    public void AddPlayersToTeam_A_UI()
    {
        if (team_A.playerList.Count <= 0) return;

        int index = 0;
        foreach (PlayerClass player in team_A.playerList)
        {
            ui_team_A_Players[index].text = "Player " + player.playerName + "/ Skill: " + player.playerSkill;
            index++;
        }

        ui_team_A_Players[5].text = "Average Team Skill: " + team_A.teamAverageSkill;
    }

    public void AddPlayersToTeam_B_UI()
    {
        if (team_B.playerList.Count <= 0) return;

        int index = 0;
        foreach (PlayerClass player in team_B.playerList)
        {
            ui_team_B_Players[index].text = "Player " + player.playerName + "/ Skill: " + player.playerSkill;
            index++;
        }

        ui_team_B_Players[5].text = "Average Team Skill: " + team_B.teamAverageSkill;
    }

    public void RemovePlayersFromTeamATeamB_UI()
    {
        foreach (TextMeshProUGUI player in ui_team_A_Players)
        {
            player.text = "";
        }
        foreach (TextMeshProUGUI player in ui_team_B_Players)
        {
            player.text = "";
        }
    }

    public float FindLowestSkill(List<PlayerClass> playerList)
    {
        float lowestSkill = playerList[0].playerSkill;

        foreach (PlayerClass player in playerList)
        {
            if (player.playerSkill < lowestSkill) lowestSkill = player.playerSkill;
        }

        return lowestSkill;
    }

    public float FindHighestSkill(List<PlayerClass> playerList)
    {
        float highestSkill = playerList[0].playerSkill;

        foreach (PlayerClass player in playerList)
        {
            if (player.playerSkill > highestSkill) highestSkill = player.playerSkill;
        }

        return highestSkill;
    }

    public void UpdateAvailablePlayersUI()
    {
        ui_availablePlayers.text = playerList.Count.ToString();
        if (playerList.Count == 0) return;
        ui_averageSkill.text = CalculateAverageSkill(playerList).ToString();
        ui_lowestSkillAllPlayers.text = FindLowestSkill(playerList).ToString();
        ui_highestSkillAllPlayers.text = FindHighestSkill(playerList).ToString();
    }

    public void UpdateLobbyUI()
    {
        ui_lobbySize.text = lobbyList.Count.ToString();
        if (lobbyList.Count == 0) return;
        ui_lowestSkillInLobby.text = FindLowestSkill(lobbyList).ToString();
        ui_highestSkillInLobby.text = FindHighestSkill(lobbyList).ToString();
        ui_lobbyAverageSkill.text = CalculateAverageSkill(lobbyList).ToString();
    }

    public void OnInputMaxLowPlayersChanged()// UI Input
    {
        if (ui_Max_Low_Players.text.Length == 0) return;
        MaxLowPlayers = int.Parse(ui_Max_Low_Players.text);
    }

    public void OnInputMaxMiddlePlayersChanged()// UI Input
    {
        if (ui_Max_Middle_Players.text.Length == 0) return;
        MaxMiddlePlayers = int.Parse(ui_Max_Middle_Players.text);
    }

    public void OnInputMaxHighPlayersChanged()// UI Input
    {
        if (ui_Max_High_Players.text.Length == 0) return;
        MaxHighPlayers = int.Parse(ui_Max_High_Players.text);
    }

    public void GenerateRandomPlayers()
    {
        playerList.Clear();

        int playerNameRand = 0;
        // Low Skill
        for (int i = 0; i < MaxLowPlayers; i++)
        {
            PlayerClass player = new PlayerClass();
            player.playerName = playerNameRand.ToString();
            player.playerSkill = UnityEngine.Random.Range(1200, 1399);
            player.ratedPlayer = true;
            playerNameRand++;
            playerList.Add(player);
        }
        // Middle Skill
        for (int i = 0; i < MaxMiddlePlayers; i++)
        {
            PlayerClass player = new PlayerClass();
            player.playerName = playerNameRand.ToString();
            player.playerSkill = UnityEngine.Random.Range(1400, 1699);
            player.ratedPlayer = true;
            playerNameRand++;
            playerList.Add(player);
        }
        // High Skill
        for (int i = 0; i < MaxHighPlayers; i++)
        {
            PlayerClass player = new PlayerClass();
            player.playerName = playerNameRand.ToString();
            player.playerSkill = UnityEngine.Random.Range(1700, 1801);
            player.ratedPlayer = true;
            playerNameRand++;
            playerList.Add(player);
        }

        //using (StreamWriter sw = new StreamWriter(filePath, false))
        //    try
        //    {
        //        foreach (PlayerClass pl in playerList)
        //        {
        //            sw.WriteLine("PlayerName: " + pl.playerName);
        //            sw.WriteLine("PlayerSkill: " + pl.playerSkill.ToString());
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("An error occurred: " + e.Message);
        //    }

        UpdateAvailablePlayersUI();

        OnInputMaxLowPlayersChanged();
        OnInputMaxMiddlePlayersChanged();
        OnInputMaxHighPlayersChanged();
    }

    public void WriteToTextFile(string path, Team team)
    {
        using (StreamWriter sw = new StreamWriter(path, false))
            try
            {
                foreach (PlayerClass pl in team.playerList)
                {
                    sw.WriteLine("PlayerName: " + pl.playerName);
                    sw.WriteLine("PlayerSkill: " + pl.playerSkill.ToString());
                }
                sw.WriteLine("");
                sw.WriteLine("TeamAverage: " + team.teamAverageSkill.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

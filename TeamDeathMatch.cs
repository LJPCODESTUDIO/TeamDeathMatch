using AMP;
using AMP.DedicatedServer;
using AMP.DedicatedServer.Plugins;
using AMP.Events;
using AMP.Network.Data;
using AMP.Network.Packets.Implementation;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AMP.Logging;


namespace TeamDeathMatch
{
    public class TeamDeathMatch : AMP_Plugin
    {
        public override string NAME => "TeamDeathMatch";
        public override string AUTHOR => "LJP";
        public override string VERSION => "1.2";


        private bool gameRunning = false;

        private int teamOneScore = 0;
        private int teamTwoScore = 0;

        private List<ClientData> players = new List<ClientData>();
        private List<ClientData> teamOne = new List<ClientData>();
        private List<ClientData> teamTwo = new List<ClientData>();

        private TeamDeathMatchConfig config;

        internal class TeamDeathMatchConfig : PluginConfig
        {
            public int requiredPlayerCount = 2;
            public float matchTime = 300.0f;
            public float intermissionTimer = 10.0f;
        }


        private void MatchTimer()
        {
            float timer = config.matchTime;
            while (timer > 0)
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("matchTimer", $"{timer}", Color.white, new Vector3(-1, 0, 2), true, true, 1)
                );
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("teamOneScore", $"{teamOneScore}", Color.blue, new Vector3(-1, -1, 2), true, true, 1)
                );
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("teamTwoScore", $"{teamTwoScore}", Color.red, new Vector3(-1, 1, 2), true, true, 1)
                );

                List<ClientData> tempTeam = teamOne.ToList();
                Vector3 position = new Vector3();
                foreach (ClientData client in tempTeam)
                {
                    position = client.player.Position;
                    position.y += 2.5f;
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket(client.ClientId.ToString(), $"Team One", Color.blue, position, true, false, 1)
                    );
                }
                tempTeam.Clear();
                tempTeam = teamTwo.ToList();
                foreach (ClientData client in tempTeam)
                {
                    position = client.player.Position;
                    position.y += 2.5f;
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket(client.ClientId.ToString(), $"Team Two", Color.red, position, true, false, 1)
                    );
                }

                Thread.Sleep(1000);
                timer--;
            }

            gameRunning = false;
            if (teamOneScore > teamTwoScore)
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("winner", $"Team One has won!", Color.blue, new Vector3(0, 0, 2), true, true, 5)
                );
            }
            else if (teamTwoScore > teamOneScore)
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("winner", $"Team Two has won!", Color.red, new Vector3(0, 0, 2), true, true, 5)
                );
            }
            else
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("winner", "It's a tie!", Color.white, new Vector3(0, 0, 2), true, true, 5)
                );
            }

            Thread.Sleep(5000);
            if (ModManager.serverInstance.connectedClients >= config.requiredPlayerCount)
            {
                Thread startGame = new Thread(StartGame);
                startGame.Start();
            }
            else
            {
                foreach(ClientData client in players)
                {
                    client.SetInvulnerable(true);
                }
            }
        }

        public override void OnStart()
        {
            ServerEvents.onPlayerJoin += OnPlayerJoin;
            ServerEvents.onPlayerQuit += OnPlayerQuit;
            ServerEvents.onPlayerKilled += OnPlayerKilled;
            config = (TeamDeathMatchConfig)GetConfig();
        }

        public void OnPlayerJoin(ClientData client)
        {
            players.Add(client);
            if (gameRunning)
            {
                if (teamOne.Count > teamTwo.Count)
                {
                    teamTwo.Add(client);
                }
                else
                {
                    teamOne.Add(client);
                }
            }
            else
            {
                client.SetInvulnerable(true);
                string message = $"{client.ClientName} has joined!\nPlayers: {players.Count}/{config.requiredPlayerCount}";
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("playerCount", message, Color.white, Vector3.forward * 2, true, true, 5)
                );

                if (ModManager.serverInstance.connectedClients >= config.requiredPlayerCount)
                {
                    Thread.Sleep(5000);
                    Thread startGame = new Thread(StartGame);
                    startGame.Start();
                }
            }
        }

        public void OnPlayerQuit(ClientData client)
        {
            players.Remove(players.Where(i => i.ClientId == client.ClientId).First());

            foreach (ClientData clientData in teamOne)
            {
                if (clientData.ClientId == client.ClientId)
                {
                    teamOne.Remove(teamOne.Where(i => i.ClientId == client.ClientId).First());
                    break;
                }
            }
            foreach (ClientData clientData in teamTwo)
            {
                if (clientData.ClientId == client.ClientId)
                {
                    teamTwo.Remove(teamTwo.Where(i => i.ClientId == client.ClientId).First());
                    break;
                }
            }
        }

        public void OnPlayerKilled(ClientData killed, ClientData killer)
        {
            if (gameRunning)
            {
                // If on same team, take away one point
                if (teamOne.Contains(killed) && teamOne.Contains(killer))
                {
                    teamOneScore--;
                }
                if (teamTwo.Contains(killed) && teamTwo.Contains(killer))
                {
                    teamTwoScore--;
                }

                // If on different teams, give one point
                if (teamOne.Contains(killed) && teamTwo.Contains(killer))
                {
                    teamTwoScore++;
                }
                if (teamTwo.Contains(killed) && teamOne.Contains(killer))
                {
                    teamOneScore++;
                }
            }
        }

        public void StartGame()
        {
            float timer = config.intermissionTimer;
            while (timer > 0.0f)
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("intermission", $"Match will start in {timer} seconds.", Color.white, new Vector3(-1, 0, 2), true, true, 1)
                );
                Thread.Sleep(1000);
                timer--;
            }

            teamOne.Clear();
            teamTwo.Clear();
            teamOneScore = 0;
            teamTwoScore = 0;
            players.Shuffle();

            int roughlyHalf = players.Count / 2;
            string message = "Team 1:\n";
            for (int i = 0; i < roughlyHalf; i++)
            {
                teamOne.Add(players[i]);
                ModManager.serverInstance.netamiteServer.SendTo(
                    players[i].ClientId,
                    new DisplayTextPacket("teamOneNotify", "You are on Team One.", Color.blue, Vector3.forward * 2, true, true, 5)
                );
                message += "- " + players[i].ClientName + "\n";
            }
            message += "Team 2:\n";
            for (int i = roughlyHalf; i < players.Count; i++)
            {
                teamTwo.Add(players[i]);
                ModManager.serverInstance.netamiteServer.SendTo(
                    players[i].ClientId,
                    new DisplayTextPacket("teamTwoNotify", "You are on Team Two.", Color.red, Vector3.forward * 2, true, true, 5)
                );
                message += "- " + players[i].ClientName + "\n";
            }
            foreach(ClientData client in players)
            {
                client.SetInvulnerable(false);
            }
            Log.Info(message);
            Thread match = new Thread(MatchTimer);
            match.Start();
        }
    }
}

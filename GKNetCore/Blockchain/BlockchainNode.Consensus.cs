/*
 *  "GKCommunicator", the chat and bulletin board of the genealogical network.
 *  Copyright (C) 2018-2024 by Sergey V. Zhdanovskih.
 *
 *  This file is part of "GEDKeeper".
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using GKNet.DHT;

namespace GKNet.Blockchain
{
    public enum ClusterNodeState
    {
        Follower,
        Candidate,
        Leader
    }


    public partial class BlockchainNode
    {
        //public int CommitIndex { get; private set; }
        //public int LastApplied { get; private set; }
        //public List<LogEntry> { get; set; }

        private Timer fHeartbeatTimer;
        private Timer fElectionTimer;
        private bool fIsHeartbeatActive;
        private bool fIsElectionActive;
        private int fTerm;
        private int fVoteFor;
        private int fVotesCounter;


        public ClusterNodeState State { get; private set; }


        private void InitConsensus()
        {
            // Инициализация таймеров
            fHeartbeatTimer = new Timer(Heartbeat, null, Timeout.Infinite, Timeout.Infinite);
            fElectionTimer = new Timer(StartElection, null, Timeout.Infinite, Timeout.Infinite);
            fIsHeartbeatActive = false;
            fIsElectionActive = false;

            Reset();
        }

        private void Reset()
        {
            State = ClusterNodeState.Follower;
            fTerm = 0;
            fVoteFor = -1;
            /*if (Log == null) {
                Log = new List<LogEntry>();
            } else {
                Log.Clear();
            }
            CommitIndex = 0;
            LastApplied = 0;*/
        }

        #region Heartbeat

        private void Heartbeat(object state)
        {
            if (State == ClusterNodeState.Leader) {
                // Отправляем heartbeat сообщения другим узлам
                AppendEntries();
            } else if (State == ClusterNodeState.Follower) {
                // Если не получаем heartbeat от лидера в течение определенного времени, запускаем выборы
                if (!fIsHeartbeatActive) {
                    StartElection();
                }
            }
        }

        private void StartHeartbeat()
        {
            if (!fIsHeartbeatActive) {
                // Рандомизированный интервал для heartbeat
                int heartbeatInterval = new Random().Next(1000, 3000); // Пример интервала от 1 до 3 секунд
                fHeartbeatTimer.Change(heartbeatInterval, heartbeatInterval);
                fIsHeartbeatActive = true;
            }
        }

        private void StopHeartbeat()
        {
            if (fIsHeartbeatActive) {
                fHeartbeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                fIsHeartbeatActive = false;
            }
        }

        #endregion

        #region Election

        private void StartElection(object state)
        {
            if (State == ClusterNodeState.Follower) {
                State = ClusterNodeState.Candidate;
                fTerm++;
                fVoteFor = -1; // Самого себя
                RequestVote(); // Запрашиваем голоса у других узлов
                StopHeartbeat(); // Останавливаем таймер heartbeat, так как мы переходим в состояние Candidate
                StartElectionTimer(); // Запускаем таймер выборов
            }
        }

        private void StartElectionTimer()
        {
            if (!fIsElectionActive) {
                // Рандомизированный интервал для выборов
                int electionInterval = new Random().Next(1000, 3000); // Пример интервала от 1 до 3 секунд
                fElectionTimer.Change(electionInterval, electionInterval);
                fIsElectionActive = true;
            }
        }

        private void StopElectionTimer()
        {
            if (fIsElectionActive) {
                fElectionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                fIsElectionActive = false;
            }
        }

        #endregion

        public void Start()
        {
            Reset();
            StartHeartbeat();
        }

        public void Stop()
        {
            StopHeartbeat();
            StopElectionTimer();
            Reset();
        }

        public void StartElection()
        {
            StartElection(null);
        }

        public void RequestVote()
        {
            fVotesCounter = 0;
            SendRequestVoteToAllNodes(fTerm, fCommunicatorCore.DHTClient.LocalID);
        }

        private void SendRequestVoteToAllNodes(int term, DHTId id)
        {
            throw new NotImplementedException();
        }

        public void RequestVote(int candidateTerm, int candidateId, int lastLogIndex, int lastLogTerm)
        {
            if (fVoteFor == -1 || fVoteFor == candidateId) {
                if (fTerm < candidateTerm) {
                    fTerm = candidateTerm;
                    fVoteFor = candidateId;
                    State = ClusterNodeState.Follower;
                    // Отправляем ответ на запрос голоса
                } else if (fTerm == candidateTerm) {
                    // Если текущий узел уже голосовал за кандидата, отправляем ответ на запрос голоса
                }
            }
        }

        public void AppendEntries()
        {
            // Предположим, что у нас есть метод SendAppendEntriesToAllNodes, который отправляет запрос на добавление записей всем узлам
            //SendAppendEntriesToAllNodes(Term, Id, Log.Count - 1, Log.Last().Term, new List<LogEntry>(), CommitIndex);
            StartHeartbeat(); // Запускаем таймер heartbeat, так как мы активно взаимодействуем с другими узлами
        }

        public void AppendEntries(int leaderTerm, int leaderId, int prevLogIndex, int prevLogTerm, List<Transaction> transactions, int leaderCommit)
        {
            if (fTerm < leaderTerm) {
                fTerm = leaderTerm;
                fVoteFor = -1;
                State = ClusterNodeState.Follower;
                // Обработка транзакций и добавление блока в блокчейн
                //var newBlock = new Block(Blockchain.Count, transactions, Blockchain.Last().Hash, DateTime.Now.Ticks);
                //Blockchain.Add(newBlock);
                // Отправляем ответ на запрос добавления записей
            } else if (fTerm == leaderTerm) {
                if (State == ClusterNodeState.Follower) {
                    // Обработка записей и отправка ответа
                } else if (State == ClusterNodeState.Candidate) {
                    // Обработка записей и отправка ответа
                }
                // Обработка транзакций и добавление блока в блокчейн
                //var newBlock = new Block(Blockchain.Count, transactions, Blockchain.Last().Hash, DateTime.Now.Ticks);
                //Blockchain.Add(newBlock);
                // Отправляем ответ на запрос добавления записей
            }
        }

        public void HandleVoteResponse(int term, bool voteGranted)
        {
            if (term > fTerm) {
                fTerm = term;
                State = ClusterNodeState.Follower;
                fVoteFor = -1;
                StopElectionTimer(); // Останавливаем таймер выборов, так как мы перешли в состояние Follower
            } else if (term == fTerm) {
                if (State == ClusterNodeState.Candidate) {
                    if (voteGranted) {
                        // Увеличиваем счетчик голосов
                        fVotesCounter++;
                    } else {
                        // Уменьшаем счетчик голосов
                        fVotesCounter--;
                    }

                    // Проверяем, достигли ли мы кворума голосов
                    if (HaveMajorityVotes()) {
                        State = ClusterNodeState.Leader;
                        StopElectionTimer(); // Останавливаем таймер выборов, так как мы стали лидером
                        StartHeartbeat(); // Запускаем таймер heartbeat для поддержания связи с другими узлами
                        AppendEntries(); // Начинаем отправлять запросы на добавление записей
                    }
                } else if (State == ClusterNodeState.Follower) {
                    // Если узел в состоянии Follower и получает ответ на запрос голоса,
                    // он может перейти в состояние Candidate, если не получает heartbeat от лидера
                    if (!fIsHeartbeatActive) {
                        StartElection();
                    }
                }
            }
        }

        private bool HaveMajorityVotes()
        {
            var peers = fCommunicatorCore.Peers;
            int quorumSize = (peers.Count / 2) + 1;

            // Реализация проверки, достигли ли мы кворума голосов
            return fVotesCounter >= quorumSize;
        }
    }
}

using System;

namespace TowerDefense.Core
{
    public class BattleStatisticsObserver
    {
        public int TotalEnemiesKilled{get; private set;}
        public int TotalGoldEarned{get; private set;}
        public int HighestWaveReached{get; private set;}

        public void OnEnemyKilledHandler(int goldReward)
        {
            TotalEnemiesKilled++;
            if (goldReward > 0) TotalGoldEarned += goldReward;
        }

        public void OnWaveChangedHandler(int currentWave, int totalWaves)
        {
            if (currentWave > HighestWaveReached)
            {
                HighestWaveReached = currentWave;
            }
        }

        public void Reset()
        {
            TotalEnemiesKilled = 0;
            TotalGoldEarned = 0;
            HighestWaveReached = 0;
        }

        public string GetSummaryText()
        {
            return $"Đợt cao nhất: {HighestWaveReached} | Quái đã diệt: {TotalEnemiesKilled} | Tổng vàng đạt được: {TotalGoldEarned}G";
        }
    }
}
namespace TowerDefense.Model
{
    public class Enums
    {
        public enum EnemyType
        {
            Vogel,
            Hund,
            Katze
        }

        public enum GameState
        {
            Start = 0, // before you hit Start
            Preparing = 1, // grid exists; user picks start & end
            Editing = 2,
            Playing = 3, // path is marked, game is “in play”
            GameOver = 4, // Gameover
            EndGame = 5 // GameFinished
        }

        public enum NotificationType
        {
            Nothing,
            NotEnoughGold,
            TowerPositionIsNotValid
        }

        public enum PlayerActions
        {
            GetDamage,
            SpendGold,
            EnemyKilled,
            NewLevel,
            GameOver
        }

        public enum TowerOptions
        {
            Nothing = 0,
            NewTower = 1,
            OldTower = 2
        }
    }
}
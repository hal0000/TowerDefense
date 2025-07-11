namespace TowerDefense.Model
{
    public class Enums
    {
        public enum GameState
        {
            Nothing = 0, // before you hit Start
            Preparing = 1,// grid exists; user picks start & end
            Playing = 2,// path is marked, game is “in play”
            Endgame = 3// reached destination or aborted
        }
        public enum EnemyType
        {
            Vogel,
            Hund,
            Katze
        }
        public enum PlayerActions
        {
            GetDamage,
            SpendGold,
            GameOver
        }
    }
}
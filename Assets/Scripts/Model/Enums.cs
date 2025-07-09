namespace TowerDefense.Model
{
    public class Enums
    {
        public enum GameState
        {
            Nothing, // before you hit Start
            Preparing, // grid exists; user picks start & end
            Playing, // path is marked, game is “in play”
            Endgame // reached destination or aborted
        }
    }
}
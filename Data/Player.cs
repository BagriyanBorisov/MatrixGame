namespace MatrixGame.Data
{
    public class Player
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Strength { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

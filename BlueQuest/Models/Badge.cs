namespace BlueQuest.Models
{
    public class Badge
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public BadgeName BadgeName { get; set; }
    }
    public enum BadgeName
    {
        Novice, Proficient, Master
    }
}

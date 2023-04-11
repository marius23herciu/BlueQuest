using System.ComponentModel.DataAnnotations.Schema;

namespace BlueQuest.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Rank Rank { get; set; } = Rank.Light_Blue;
        public List<PointsByCategory> Points { get; set; } = new List<PointsByCategory>();
        public List<Badge> Badges { get; set; } = new List<Badge>();
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
        public int TotalQuestsAttempts { get; set; } = 0;
    }

    public enum Rank
    {
        Light_Blue, Blue, Heaven_Blue, Allmighty_Blue
    }
}

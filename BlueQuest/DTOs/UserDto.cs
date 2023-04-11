using BlueQuest.Models;
using System.ComponentModel.DataAnnotations;

namespace BlueQuest.DTOs
{
    public class UserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Points { get; set; }
        public Rank Rank { get; set; }
        public int TotalQuestsAttempts { get; set; }
        public List<string> Badges { get; set; }
    }
}

using BlueQuest.Models;

namespace BlueQuest.DTOs
{
    public class QuestBasicDetailsDto
    {
        public Category Category { get; set; }
        public Difficulty Difficulty { get; set; }
        public string Title { get; set; }
        public int TotalAttempts { get; set; }
        public decimal RateOfSuccess { get; set; }
        public decimal UsersRating { get; set; }
    }
}

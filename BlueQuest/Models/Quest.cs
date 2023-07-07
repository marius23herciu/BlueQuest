using System.ComponentModel.DataAnnotations.Schema;

namespace BlueQuest.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public Category Category { get; set; }
        public Difficulty Difficulty { get; set; }
        public List<UserId> UsersWhoSolvedQuest { get; set; } = new List<UserId> ();
        public List<UserId> UsersWhoRatedQuest { get; set; } = new List<UserId>();
        public User CreatedBy { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string Image { get; set; } = string.Empty;
        public string Video { get; set; } = string.Empty;
        public string Audio { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string TipsAndLinks { get; set; } = string.Empty;
        public DateTime PostingTime { get; set; } = DateTime.Now;
        public int AvailabilityInDays { get; set; }
        public DateTime EndAvailabilityDate { get; set; }  = DateTime.Now.AddDays(1);
        public int TotalAttempts { get; set; }
        public int SuccessfulAttempts { get; set; }
        public decimal RateOfSuccess { get; set; }
        public int NoOfRatings { get; set; }
        public int SumOfRatings { get; set; }
        public decimal UsersRating { get; set; }
    }

    public enum Category
    {
        Movies, Music, News, Celebrity, Games, General_Knowledge, Sports, Geography,
        History, Literature, Science, Technology, Languages
    }
    public enum Difficulty
    {
        Easy, Medium, Hard
    }
}

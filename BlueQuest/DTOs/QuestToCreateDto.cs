using BlueQuest.Models;

namespace BlueQuest.DTOs
{
    public class QuestToCreateDto
    {
        public Difficulty Difficulty { get; set; }
        public string Title { get; set; }
        public string Question { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public string? Audio { get; set; }
        public string Answer { get; set; }
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? TipsAndLinks { get; set; }
        public int AvailabilityInDays { get; set; }
    }
}

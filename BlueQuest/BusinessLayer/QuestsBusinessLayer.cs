using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueQuest.BusinessLayer
{
    public class QuestsBusinessLayer
    {
        private readonly BlueQuestDbContext _context;
        private readonly ToDTOs _toDtos;

        public QuestsBusinessLayer(BlueQuestDbContext context, ToDTOs toDtos)
        {
            this._context = context;
            this._toDtos = toDtos;
        }
        /// <summary>
        /// Creates new quest in the selected category.
        /// </summary>
        /// <param name="userId">Id of the user who creates the quest.</param>
        /// <param name="quest">DTO of the quest's details.</param>
        /// <param name="category">Category for the quest.</param>
        /// <returns></returns>
        public async Task<QuestDto> CreateQuest(int userId, QuestToCreateDto quest)
        {
            var user = await _context.Users.Include(p => p.Points).FirstOrDefaultAsync(u => u.Id == userId);

            var usersPoints = user.Points;
            var questCategoryPoints = 0;
            foreach (var categ in usersPoints)
            {
                if (categ.Category == quest.Category)
                {
                    questCategoryPoints = categ.Points;
                }
            }

            if (user.Rank < Rank.Intermediate && questCategoryPoints < 100)
            {
                return null;
            }

            foreach (var categ in usersPoints)
            {
                if (categ.Category == quest.Category)
                {
                    categ.Points -= 100;
                }
            }

            await UpdateRank(user);

            var newQuest = new Quest
            {
                Category = quest.Category,
                Difficulty = quest.Difficulty,
                Title = quest.Title,
                Question = quest.Question,
                Image = quest.Image,
                Video = quest.Video,
                Audio = quest.Audio,
                Answer = quest.Answer,
                Option1 = quest.Option1,
                Option2 = quest.Option2,
                Option3 = quest.Option3,
                TipsAndLinks = quest.TipsAndLinks,
                AvailabilityInDays = quest.AvailabilityInDays,
                CreatedBy = user,
            };

            newQuest.EndAvailabilityDate = newQuest.PostingTime.AddDays(newQuest.AvailabilityInDays);

            var questDto = _toDtos.QuestToDto(newQuest, userId).Result;

            await _context.AddAsync(newQuest);
            await _context.SaveChangesAsync();

            return questDto;
        }

        ///doar pentru demonstrarea functionalitatii
        
        /// <summary>
        /// Adds point in a category for a user.
        /// </summary>
        /// <param name="userId">User to receive points.</param>
        /// <param name="category"></param>
        /// <param name="noOfPoints">Number of points.</param>
        /// <returns></returns>
        public async Task<bool?> AddPointsToUser(int userId, Category category, int noOfPoints)
        {
            var user = await _context.Users.Include(p => p.Points).Include(b=>b.Badges).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            await CreateCategoryForUserIfItDoesntExist(user, category);

            var usersPoints = user.Points;
            foreach (var categ in usersPoints)
            {
                if (categ.Category == category)
                {
                    categ.Points += noOfPoints;
                }
            }

            await UpdateRank(user);
            await AwardBadgeOrRemoveBadge(user, category);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool?> AddPointsToUserForEachCategory(int userId, int points)
        {
            var user = await _context.Users.Include(p => p.Points).Include(b => b.Badges).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }

            foreach (var categ in (Category[])Enum.GetValues(typeof(Category)))
            {
                await CreateCategoryForUserIfItDoesntExist(user, categ);
            }

            var usersPoints = user.Points;
            foreach (var categ in usersPoints)
            {
                categ.Points += points;
            }

            await UpdateRank(user);

            var categoryEnumerable = Enum.GetValues(typeof(Category)).Cast<Category>();
            foreach (var cat in categoryEnumerable)
            {
                await AwardBadgeOrRemoveBadge(user, cat);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Checks if the category exists in users's points list.
        /// If not, the category is added.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool> CreateCategoryForUserIfItDoesntExist(User user, Category category)
        {
            bool categoryExists = false;
            foreach (var cat in user.Points)
            {
                if (cat.Category == category)
                {
                    categoryExists = true;
                    break;
                }
            }

            if (!categoryExists)
            {
                user.Points.Add(new PointsByCategory
                {
                    Category = category,
                    Points = 0
                });
            }
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Retunrs a DTO of the quest with te inserted Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<QuestDto> GetQuest(int questId, int userId)
        {
            var questToSolve = await _context.Quests.Include(u=>u.CreatedBy).Include(u=>u.UsersWhoRatedQuest).
                Include(u=>u.UsersWhoSolvedQuest).Where(q => q.Id == questId).FirstOrDefaultAsync();

            if (questToSolve == null)
            {
                return null;
            }

            var questDto = _toDtos.QuestToDto(questToSolve, userId).Result;

            return questDto;
        }
        /// <summary>
        /// Gives points to the specific category for user, according to quest's difficulty.
        /// Easy=25; Medium=50; Hard=100;
        /// </summary>
        /// <param name="user"></param>
        /// <param name="difficulty"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool> AwardPointsToUserAccordintToQuestDifficulty(User user, Difficulty difficulty, Category category)
        {

            if (difficulty == Difficulty.Easy)
            {
                var usersPoints = user.Points;
                foreach (var pointsByCategory in usersPoints)
                {
                    if (pointsByCategory.Category == category)
                    {
                        pointsByCategory.Points += 25;
                    }
                }
            }
            else if (difficulty == Difficulty.Medium)
            {
                var usersPoints = user.Points;
                foreach (var pointsByCategory in usersPoints)
                {
                    if (pointsByCategory.Category == category)
                    {
                        pointsByCategory.Points += 50;
                    }
                }
            }
            else if (difficulty == Difficulty.Hard)
            {
                var usersPoints = user.Points;
                foreach (var pointsByCategory in usersPoints)
                {
                    if (pointsByCategory.Category == category)
                    {
                        pointsByCategory.Points += 100;
                    }
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Resolve the quest for one user.
        /// If the answer is correct, the user will recieve points, and if it has enough points after,
        /// he will get badges ar a rank upgrade.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="userId"></param>
        /// <param name="answer">User's answer.</param>
        /// <returns></returns>
        public async Task<bool?> ResolveQuest(int questId, int userId, string answer)
        {
            var user = await _context.Users.Include(p => p.Points).FirstOrDefaultAsync(u => u.Id == userId);
            var questToSolve = await _context.Quests.Include(c => c.CreatedBy).Include(u=>u.UsersWhoSolvedQuest)
                                     .Where(q => q.Id == questId).FirstOrDefaultAsync();

            var checkIfAllreadySolved = questToSolve.UsersWhoSolvedQuest;
            foreach (var id in checkIfAllreadySolved)
            {
                if (id.User==userId)
                {
                    return null;
                }
            }

            if (questToSolve == null || user == null || questToSolve.EndAvailabilityDate < DateTime.Now
                || questToSolve.CreatedBy.Id == userId)
            {
                return null;
            }

            questToSolve.TotalAttempts += 1;
            user.TotalQuestsAttempts += 1;

            if (questToSolve.TotalAttempts % 5 == 0)
            {
                var questCreatorUser = await _context.Users.Include(p => p.Points).FirstOrDefaultAsync(u => u.Id == questToSolve.CreatedBy.Id);
                var usersPoints = questCreatorUser.Points;
                foreach (var categ in usersPoints)
                {
                    if (categ.Category == questToSolve.Category)
                    {
                        categ.Points += 10;
                    }
                }
            }

            if (answer != questToSolve.Answer)
            {
                questToSolve.RateOfSuccess = (decimal)questToSolve.SuccessfulAttempts / (decimal)questToSolve.TotalAttempts * 100;
                await _context.SaveChangesAsync();
                return false;
            }

            questToSolve.SuccessfulAttempts += 1;
            questToSolve.RateOfSuccess = (decimal)questToSolve.SuccessfulAttempts / (decimal)questToSolve.TotalAttempts * 100;

            questToSolve.UsersWhoSolvedQuest.Add(new UserId
            {
                User = userId
            });

            await CreateCategoryForUserIfItDoesntExist(user, questToSolve.Category);

            await AwardPointsToUserAccordintToQuestDifficulty(user, questToSolve.Difficulty, questToSolve.Category);

            await UpdateRank(user);

            await AwardBadgeOrRemoveBadge(user, questToSolve.Category);

            await _context.SaveChangesAsync();


            return true;
        }

        public async Task<List<string>> GetAvailableCategToCreate(int userId)
        {
            var categs = new List<string>();
            var user = await _context.Users.Include(p=>p.Points).Where(i=>i.Id==userId).FirstOrDefaultAsync();
            if (user==null)
            {
                return categs;
            }

            var points = user.Points;

            foreach (var categ in points)
            {
                if (categ.Points>=100)
                {
                    categs.Add(categ.Category.ToString());
                }
            }

            return categs;
            //return Enum.GetNames(typeof(Category)).ToList();
        }
        public async Task<List<string>> GetAllCategories()
        {
            return Enum.GetNames(typeof(Category)).ToList();
        }

        public async Task<List<string>> GetDifficultyLevels()
        {
            return Enum.GetNames(typeof(Difficulty)).ToList();
        }
        


        /// <summary>
        /// Checks users points for a Rank Update.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool?> UpdateRank(User user)
        {
            var points = user.Points;
            int totalPoints = 0;

            foreach (var category in points)
            {
                totalPoints += category.Points;
            }

            if (totalPoints >= 25000 )
            {
                user.Rank = Rank.Expert;
            }
            else if (totalPoints >= 10000 && totalPoints < 25000)
            {
                user.Rank = Rank.Advanced;
            }
            else if (totalPoints >= 1000 && totalPoints < 10000)
            {
                user.Rank = Rank.Intermediate;
            }
            else if (totalPoints < 1000 )
            {
                user.Rank = Rank.Beginner;
            }


            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Checks if user has enough points in the selected category to receive a badge.
        /// If it does, the user will get the appropriate badge.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<bool?> AwardBadgeOrRemoveBadge(User user, Category category)
        {
            var points = user.Points;
            int pointsForQuestsCategory = 0;

            foreach (var categ in points)
            {
                if (categ.Category == category)
                {
                    pointsForQuestsCategory += categ.Points;
                }
            }

            if (pointsForQuestsCategory < 500)
            {
                var badges = user.Badges;
                foreach (var badge in user.Badges)
                {
                    if (badge.Category == category)
                    {
                        user.Badges.Remove(badge);
                    }
                }
            }

            if (pointsForQuestsCategory >= 500 && pointsForQuestsCategory < 1000)
            {
                var badges = user.Badges;
                bool badgeNotFound = true;
                foreach (var badge in user.Badges)
                {
                    if (badge.Category == category)
                    {
                        badgeNotFound = false;
                        badge.Category = category;
                        badge.BadgeName = BadgeName.Novice;
                    }
                }
                if (badgeNotFound)
                {
                    user.Badges.Add(new Badge
                    {
                        Category = category,
                        BadgeName = BadgeName.Novice,
                    });
                }
            }

            if (pointsForQuestsCategory >= 1000 && pointsForQuestsCategory < 2000)
            {
                var badges = user.Badges;
                bool badgeNotFound = true;
                foreach (var badge in user.Badges)
                {
                    if (badge.Category == category)
                    {
                        badgeNotFound = false;
                        badge.Category = category;
                        badge.BadgeName = BadgeName.Proficient;
                    }
                }
                if (badgeNotFound)
                {
                    user.Badges.Add(new Badge
                    {
                        Category = category,
                        BadgeName = BadgeName.Proficient,
                    });
                }
            }

            if (pointsForQuestsCategory >= 2000)
            {
                var badges = user.Badges;
                bool badgeNotFound = true;
                foreach (var badge in user.Badges)
                {
                    if (badge.Category == category)
                    {
                        badgeNotFound = false;
                        badge.Category = category;
                        badge.BadgeName = BadgeName.Master;
                    }
                }
                if (badgeNotFound)
                {
                    user.Badges.Add(new Badge
                    {
                        Category = category,
                        BadgeName = BadgeName.Master,
                    });
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Returns  a list with all the quests from the selected category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestByCategory(Category category)
        {
            var quests = await _context.Quests.Where(q => q.Category == category).ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderBy(d => d.Difficulty).ThenBy(t => t.Title).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// Returns  a list with all the quests for the selected difficulty.
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestByDifficulty(Difficulty difficulty)
        {
            var quests = await _context.Quests.Where(q => q.Difficulty == difficulty).ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderBy(t => t.Title).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// Returns  a list with all the quests from the selected category and difficulty.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestByCategoryAndDifficulty(Category category, Difficulty difficulty)
        {
            var quests = await _context.Quests.Where(q => q.Difficulty == difficulty).Where(q => q.Category == category).ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderBy(c => c.Category).ThenBy(d => d.Difficulty).ThenBy(t => t.Title).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// Returns  a list with all the quests in the descending order of popularity.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestsByPopularity()
        {
            var quests = await _context.Quests.ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderByDescending(c => c.TotalAttempts).ThenBy(t => t.Title).ThenBy(d => d.Difficulty).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// Returns  a list with all the quests in descending order of their success rate.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestsBySuccessfulRate()
        {
            var quests = await _context.Quests.ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderByDescending(c => c.RateOfSuccess).ThenBy(t => t.Title).ThenBy(d => d.Difficulty).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// Returns  a list with all the quests in descending order of ratings given by users.
        /// </summary>
        /// <returns></returns>
        public async Task<List<QuestBasicDetailsDto>> GetQuestByUsersRating()
        {
            var quests = await _context.Quests.ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderByDescending(c => c.UsersRating).ThenBy(t => t.Title).ThenBy(d => d.Difficulty).ToList();

            return orderdDtos;
        }
        /// <summary>
        /// User gives a rating to a quest.
        /// Value of rating must be between 1 and 5.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="userId"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        public async Task<bool?> RateQuest(int questId, int userId, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                return null;
            }

            var user = await _context.Users.Include(p => p.Points).FirstOrDefaultAsync(u => u.Id == userId);
            var questToRate = await _context.Quests.Include(u => u.UsersWhoSolvedQuest).Include(c => c.CreatedBy)
                                     .Include(u=>u.UsersWhoRatedQuest).Where(q => q.Id == questId).FirstOrDefaultAsync();

            if (questToRate.CreatedBy.Id == userId)
            {
                return false;
            }

            foreach (var q in questToRate.UsersWhoRatedQuest)
            {
                if (q.User==userId)
                {
                    return false;
                }
            }

            questToRate.NoOfRatings += 1;
            questToRate.SumOfRatings += rating;
            questToRate.UsersRating = (decimal)questToRate.SumOfRatings / (decimal)questToRate.NoOfRatings;
            questToRate.UsersWhoRatedQuest.Add(new UserId { User = userId });

            await _context.SaveChangesAsync();

            return true;
        }
        /// <summary>
        /// Deletes a quest only if the user who wants to delete it is also the cretor of the quest.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool?> DeleteQuest(int questId, int userId)
        {
            var questToDelete = await _context.Quests.Include(u => u.UsersWhoSolvedQuest).Include(c => c.CreatedBy)
                                     .Include(c => c.UsersWhoRatedQuest)
                                     .Where(q => q.Id == questId).FirstOrDefaultAsync();

            if (questToDelete == null)
            {
                return null;
            }
            if (questToDelete.CreatedBy.Id != userId)
            {
                return false;
            }

            _context.Quests.Remove(questToDelete);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<List<int>> GetRandomQuests(int number, int userId)
        {
            var finalQuests = new List<int>();
            Random r = new Random();
            var random = new List<int>();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var quests = await _context.Quests.Include(u => u.CreatedBy).Include(u => u.UsersWhoSolvedQuest).Where(i => i.CreatedBy.Id != userId)
                .ToListAsync();
            var questsFiltered = new List<Quest>();
            foreach (var q in quests)
            {
                if (q.UsersWhoSolvedQuest.Count == 0)
                {
                    questsFiltered.Add(q);
                }
                int counter = 0;
                foreach (var u in q.UsersWhoSolvedQuest)
                {
                    if (u.User != userId)
                    {
                        counter++;
                    }
                    if (counter==q.UsersWhoSolvedQuest.Count)
                    {
                        questsFiltered.Add(q);
                    }
                }
            }
            foreach (var q in questsFiltered)
            {
                finalQuests.Add(q.Id);
            }

            if (quests.Count >= number)
            {
                random = finalQuests.OrderBy(x => r.Next()).Take(number).ToList();
                return random;
            }

            return random;
        }
        public async Task<List<int>> GetRandomQuestsByCategory(string category, int noOfQuests, int userId)
        {
            Random r = new Random();
            var random = new List<int>();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            var quests = await _context.Quests.Include(u => u.CreatedBy).Include(u => u.UsersWhoSolvedQuest).Where(i => i.CreatedBy.Id != userId)
                 .ToListAsync();
            var questsFiltered = new List<Quest>();
            foreach (var q in quests)
            {
                if (q.UsersWhoSolvedQuest.Count == 0)
                {
                    questsFiltered.Add(q);
                }
                int counter = 0;
                foreach (var u in q.UsersWhoSolvedQuest)
                {
                    if (u.User != userId)
                    {
                        counter++;
                    }
                    if (counter == q.UsersWhoSolvedQuest.Count)
                    {
                        questsFiltered.Add(q);
                    }
                }
            }
            var questsId = questsFiltered.Where(q => Enum.GetName(q.Category) == category).Select(i => i.Id).ToList();

            if (questsId.Count >= noOfQuests)
            {
                random = questsId.OrderBy(x => r.Next()).Take(noOfQuests).ToList();
                return random;
            }
            return random;
        }
        public async Task<List<int>> GetRandomQuestsByDifficulty(string difficulty, int noOfQuests, int userId)
        {
            Random r = new Random();
            var random = new List<int>();
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);


            var quests = await _context.Quests.Include(u => u.CreatedBy).Include(u => u.UsersWhoSolvedQuest).Where(i => i.CreatedBy.Id != userId)
                .ToListAsync();
            var questsFiltered = new List<Quest>();
            foreach (var q  in quests)
            {
                if (q.UsersWhoSolvedQuest.Count == 0)
                {
                    questsFiltered.Add(q);
                }
                int counter = 0;
                foreach (var u in q.UsersWhoSolvedQuest)
                {
                    if (u.User != userId)
                    {
                        counter++;
                    }
                    if (counter == q.UsersWhoSolvedQuest.Count)
                    {
                        questsFiltered.Add(q);
                    }
                }
            }
            var questsId = questsFiltered.Where(q => Enum.GetName(q.Difficulty) == difficulty).Select(i => i.Id).ToList();

            if (questsId.Count >= noOfQuests)
            {
                random = questsId.OrderBy(x => r.Next()).Take(noOfQuests).ToList();
                return random;
            }
            return random;
        }
        public async Task<List<QuestBasicDetailsDto>> GetQuestByCreator(int userId)
        {
            var quests = await _context.Quests.Include(u=>u.CreatedBy).Where(q => q.CreatedBy.Id == userId).ToListAsync();

            List<QuestBasicDetailsDto> questsDto = new List<QuestBasicDetailsDto>();
            foreach (var quest in quests)
            {
                questsDto.Add(_toDtos.QuestBasicDetailsToDto(quest).Result);
            }

            var orderdDtos = questsDto.OrderBy(d => d.Difficulty).ThenBy(t => t.Title).ToList();

            return orderdDtos;
        }
    }
}

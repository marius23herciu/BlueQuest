﻿using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace BlueQuest.BusinessLayer
{
    public class ToDTOs
    {
        private readonly BlueQuestDbContext _context;
        public ToDTOs(BlueQuestDbContext context)
        {
            this._context = context;
        }

        public async Task<UserToEditDto> UserToEditToDto(User user)
        {
            var userDto = new UserToEditDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = GetDepartmentsName(user.Id).Result,
                Email = user.Email,
            };
            return userDto;
        }
        public async Task<UserDto> UserToDto(User user)
        {
            var badges = user.Badges.ToList();
            List<string> stringOfBadges = new List<string>();

            foreach (var badge in badges)
            {
                stringOfBadges.Add($"{badge.Category} {badge.BadgeName}");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = GetDepartmentsName(user.Id).Result,
                Rank = user.Rank,
                Points = GetTotalPointsForUser(user).Result,
                Badges = stringOfBadges,
                TotalQuestsAttempts = user.TotalQuestsAttempts
            };
            return userDto;
        }

        public async Task<UserDto> UserByCategoryToDto(User user, Category category)
        {
            var badges = user.Badges.ToList();
            List<string> stringOfBadges = new List<string>();

            foreach (var badge in badges)
            {
                stringOfBadges.Add($"{badge.Category} {badge.BadgeName}");
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Department = GetDepartmentsName(user.Id).Result,
                Rank = user.Rank,
                Points = GetTotalPointsForUserByCategory(user, category).Result,
                Badges = stringOfBadges,
                TotalQuestsAttempts = user.TotalQuestsAttempts,
            };
            return userDto;
        }

        public async Task<string> GetDepartmentsName(int id)
        {
            var departments = await _context.Departments.Include(u => u.Users).ToListAsync();

            foreach (var dep in departments)
            {
                var userOfDepartment = dep.Users.FirstOrDefault(d => d.Id == id);
                if (userOfDepartment != null)
                {
                    return dep.Name;
                }
            }

            return null;
        }

        public async Task<QuestDto> QuestToDto(Quest newQuest, int userId)
        {
            var createdByUser = false;
            var allowedToSolve = true;
            if (newQuest.CreatedBy.Id==userId) 
            { 
                createdByUser= true;
            }
            var user = await _context.Users.FirstOrDefaultAsync(i => i.Id == userId);
            var allreadySolvedByUser = false;
            foreach (var id in newQuest.UsersWhoSolvedQuest)
            {
                if (id.User==userId)
                {
                    allreadySolvedByUser = true;
                    break;
                }
            }
            if (createdByUser==true || allreadySolvedByUser)
            {
                allowedToSolve= false;
            }

            var allreadyRatedByUser = false;
            foreach (var id in newQuest.UsersWhoRatedQuest)
            {
                if (id.User == userId)
                {
                    allreadyRatedByUser = true;
                    break;
                }
            }

            var questDto = new QuestDto
            {
                Category = newQuest.Category,
                Difficulty = newQuest.Difficulty,
                Title = newQuest.Title,
                Question = newQuest.Question,
                Image = newQuest.Image,
                Video = newQuest.Video,
                Audio = newQuest.Audio,
                Option1 = newQuest.Option1,
                Option2 = newQuest.Option2,
                Option3 = newQuest.Option3,
                TipsAndLinks = newQuest.TipsAndLinks,
                EndAvailabilityDate = newQuest.EndAvailabilityDate,
                TotalAttempts = newQuest.TotalAttempts,
                RateOfSuccess = newQuest.RateOfSuccess,
                UsersRating = newQuest.UsersRating,
                CreatedByActiveUser= createdByUser,
                AllowedToSolve = allowedToSolve,
                AllreadyRated = allreadyRatedByUser
            };

            return questDto;
        }

        public async Task<QuestBasicDetailsDto> QuestBasicDetailsToDto(Quest quest)
        {
            var questDto = new QuestBasicDetailsDto
            {
                Id = quest.Id,
                Category = quest.Category,
                Difficulty = quest.Difficulty,
                Title = quest.Title,
                TotalAttempts = quest.TotalAttempts,
                RateOfSuccess = quest.RateOfSuccess,
                UsersRating = quest.UsersRating,
            };

            return questDto;
        }

        public async Task<int> GetTotalPointsForUser(User user)
        {
            var points = user.Points;
            int totalPoints = 0;

            foreach (var category in points)
            {
                totalPoints += category.Points;
            }

            return totalPoints;
        }
        public async Task<int> GetTotalPointsForUserByCategory(User user, Category category)
        {
            var points = user.Points;
            int totalPoints = 0;

            foreach (var usersCategory in points)
            {
                if (usersCategory.Category == category)
                {
                    totalPoints += usersCategory.Points;
                }
            }

            return totalPoints;
        }

    }
}

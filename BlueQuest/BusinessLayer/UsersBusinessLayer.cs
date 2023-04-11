using Azure.Core;
using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BlueQuest.BusinessLayer
{
    public class UsersBusinessLayer
    {
        private readonly BlueQuestDbContext _context;
        private readonly ToDTOs _toDtos;

        public UsersBusinessLayer(BlueQuestDbContext context, ToDTOs toDtos)
        {
            this._context = context;
            this._toDtos = toDtos;
        }
        public async Task<UserDto> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Points).Include(b => b.Badges).Where(u=>u.Id==id).FirstOrDefaultAsync();
            if (user==null)
            {
                return null;
            }
            var userDto = _toDtos.UserToDto(user).Result;
            
            return userDto;
        }
        /// <summary>
        /// Returns a list of all users in alphabetical order.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersAlphabetically()
        {
            var users = await _context.Users.Include(p => p.Points).Include(b=>b.Badges).OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

            var usersDto = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = _toDtos.UserToDto(user).Result;
                usersDto.Add(userDto);
            }

            return usersDto;
        }
        /// <summary>
        /// Returns a list of all users in descdending order according to their rate of activity.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserDto>> GetMostActiveUsers()
        {
            var users = await _context.Users.Include(p => p.Points).Include(b => b.Badges).OrderByDescending(a=>a.TotalQuestsAttempts).ThenBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

            var usersDto = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = _toDtos.UserToDto(user).Result;
                usersDto.Add(userDto);
            }

            return usersDto;
        }
        /// <summary>
        /// Returns a list of all users in descdending order according to their ranking and points.
        /// </summary>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersRanking()
        {
            var users = await _context.Users.Include(p => p.Points).Include(b => b.Badges).OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

            var usersDto = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = _toDtos.UserToDto(user).Result;
                usersDto.Add(userDto);
            }

            var resultUsersDto = usersDto.OrderByDescending(p=>p.Points).ThenBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();

            return resultUsersDto;
        }
        /// <summary>
        /// Returns a list of all users from the selected category in descdending order according to their ranking and points.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersRankingByCategory(Category category)
        {
            var users = await _context.Users.Include(p => p.Points).Include(b => b.Badges).OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();

            var usersDto = new List<UserDto>();
            foreach (var user in users)
            {
                var userDto = _toDtos.UserByCategoryToDto(user, category).Result;
                usersDto.Add(userDto);
            }

            var resultUsersDto = usersDto.OrderByDescending(p => p.Points).ThenBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();

            return resultUsersDto;
        }
        /// <summary>
        ///  Returns a list of all users from the selected department in descdending order according to their ranking and points.
        /// </summary>
        /// <param name="department"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersRankingByDepartment(string department)
        {
            var selectedDepartment = await _context.Departments.Include(u=>u.Users).Where(d=>d.Name==department).FirstOrDefaultAsync();
            if (selectedDepartment==null)
            {
                return null;
            }

            var users = selectedDepartment.Users.ToList();

            var usersWithPoints = new List<User>();
            foreach (var user in users)
            {
                usersWithPoints.Add(await _context.Users.Include(p=>p.Points).Include(b => b.Badges).Where(i=>i.Id==user.Id).FirstOrDefaultAsync());
            }

            var usersDto = new List<UserDto>();
            foreach (var user in usersWithPoints)
            {
                var userDto = _toDtos.UserToDto(user).Result;
                usersDto.Add(userDto);
            }

            var resultUsersDto = usersDto.OrderByDescending(p => p.Points).ThenBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();

            return resultUsersDto;
        }
        /// <summary>
        ///  Returns a list of all users from the selected category and department in descdending order according to their ranking and points.
        /// </summary>
        /// <param name="department"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<UserDto>> GetUsersRankingByDepartmentAndCategory(string department, Category category)
        {
            var selectedDepartment = await _context.Departments.Include(u => u.Users).Where(d => d.Name == department).FirstOrDefaultAsync();
            if (selectedDepartment == null)
            {
                return null;
            }

            var users = selectedDepartment.Users.ToList();

            var usersWithPoints = new List<User>();
            foreach (var user in users)
            {
                usersWithPoints.Add(await _context.Users.Include(p => p.Points).Include(b => b.Badges).Where(i => i.Id == user.Id).FirstOrDefaultAsync());
            }

            var usersDto = new List<UserDto>();
            foreach (var user in usersWithPoints)
            {
                var userDto = _toDtos.UserByCategoryToDto(user, category).Result;
                usersDto.Add(userDto);
            }

            var resultUsersDto = usersDto.OrderByDescending(p => p.Points).ThenBy(e => e.LastName).ThenBy(e => e.FirstName).ToList();

            return resultUsersDto;
        }
        /// <summary>
        /// Changes the values inserted for the user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="editUser"></param>
        /// <returns></returns>
        public async Task<UserToEditDto> UpdateUser(int id, UserToEditDto editUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == id);

            if (user == null)
            {
                return null;
            }

            user.FirstName = editUser.FirstName;
            user.LastName = editUser.LastName;
            user.Email = editUser.Email;

            var alldDepartments = await _context.Departments.Include(u=>u.Users).ToListAsync();
            foreach (var dep in alldDepartments)
            {
                if (dep.Users.Contains(user))
                {
                    dep.Users.Remove(user);
                }
            }


            bool userIsAddedToDepartment = false;
            foreach (var dep in alldDepartments)
            {
                if (dep.Name == editUser.Department)
                {
                    dep.Users.Add(user);
                    userIsAddedToDepartment = true;
                    break;
                }
            }

            if (!userIsAddedToDepartment)
            {
                var employeesDepartment = new Department
                {
                    Name = editUser.Department
                };
                employeesDepartment.Users.Add(user);
                await _context.Departments.AddAsync(employeesDepartment);
            }

            await _context.SaveChangesAsync();

            return _toDtos.UserToEditToDto(user).Result;
        }

        /// <summary>
        /// Deletes a user by its Id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> DeleteUser(int id)
        {

            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == id);

            if (user == null)
            {
                return false;
            }

            var allDepartments = await _context.Departments.ToListAsync();
            foreach (var dep in allDepartments)
            {
                if (dep.Users.Contains(user))
                {
                    dep.Users.Remove(user);
                }
            }

            _context.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}

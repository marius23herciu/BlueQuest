using BlueQuest.BusinessLayer;
using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlueQuest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersBusinessLayer _bussinesLayer;
        private readonly ToDTOs _toDtos;
        public UsersController(UsersBusinessLayer bussinesLayer, ToDTOs toDtos)
        {
            this._bussinesLayer = bussinesLayer;
            this._toDtos = toDtos;
        }

        [HttpGet]
        [Route ("{id}")]
        public async Task<IActionResult> GetAllUsers([FromRoute] int id)
        {
            var user = await _bussinesLayer.GetUser(id);
            if (user==null)
            {
                return NotFound("User Not Found.");
            }
            return Ok(user);
        }

        //[HttpGet, Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _bussinesLayer.GetUsersAlphabetically());
        }

        [HttpGet]
        [Route("most-active-users")]
        public async Task<IActionResult> GetMostActiveUsers()
        {
            return Ok(await _bussinesLayer.GetMostActiveUsers());
        }


        [HttpGet]
        [Route ("users-ranking")]
        public async Task<IActionResult> GetUsersRanking()
        {
            return Ok(await _bussinesLayer.GetUsersRanking());
        }

        [HttpGet]
        [Route("users-ranking-by-category")]
        public async Task<IActionResult> GetUsersRankingByCategory(Category category)
        {
            return Ok(await _bussinesLayer.GetUsersRankingByCategory(category));
        }


        [HttpGet]
        [Route("users-ranking-by-{department}")]
        public async Task<IActionResult> GetUsersRankingByDepartment([FromRoute] string department)
        {
            var usersRankedByDepartment = await _bussinesLayer.GetUsersRankingByDepartment(department);
            if (usersRankedByDepartment==null)
            {
                return NotFound($"Department {department} doesn't exist.");
            }
            return Ok(usersRankedByDepartment);
        }

        [HttpGet]
        [Route("users-ranking-by-department-and-category")]
        public async Task<IActionResult> GetUsersRankingByDepartmentAndCategory(string department, Category category)
        {
            var usersRankedByDepartment = await _bussinesLayer.GetUsersRankingByDepartmentAndCategory(department, category);
            if (usersRankedByDepartment == null)
            {
                return NotFound($"Department {department} doesn't exist.");
            }
            return Ok(usersRankedByDepartment);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UserToEditDto editEmployee)
        {
            var user = await _bussinesLayer.UpdateUser(id, editEmployee);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            var isUserDeleted = await _bussinesLayer.DeleteUser(id);

            if (!isUserDeleted)
            {
                return NotFound("User not found.");
            }

            return Ok();
        }


    }
}

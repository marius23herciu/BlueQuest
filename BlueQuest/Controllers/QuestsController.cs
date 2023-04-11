using BlueQuest.BusinessLayer;
using BlueQuest.Data;
using BlueQuest.DTOs;
using BlueQuest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlueQuest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly QuestsBusinessLayer _businessLayer;
        private readonly ToDTOs _toDtos;
        public QuestsController(QuestsBusinessLayer bussinesLayer, ToDTOs toDtos)
        {
            this._businessLayer = bussinesLayer;
            this._toDtos = toDtos;
        }

        [HttpPost("quest")]
        public async Task<ActionResult<QuestDto>> CreateQuest(int userId, QuestToCreateDto quest, Category category)
        {
            if (quest is null)
            {
                return BadRequest("Complete all the required fields to create a quest.");
            }

            var newQuest = await _businessLayer.CreateQuest(userId, quest, category);
            if (newQuest == null)
            {
                return BadRequest("User has to be at leat Rank Blue to create quests and has to have at least 100 point in the quest category.");
            }
            return Ok(newQuest);
        }

        ///////sterge - l la final //////////////////////////////////////////////////////
        [HttpPost("add-points-to-user")]
        public async Task<ActionResult<QuestDto>> AddPointsToUser(int userId, Category category, int noOfPoints)
        {
            var pointsAdded = await _businessLayer.AddPointsToUser(userId, category, noOfPoints);
            if (pointsAdded == null)
            {
                return BadRequest("User not found.");
            }
            return Ok(pointsAdded);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<QuestDto>> GetQuest([FromRoute] int id)
        {
            var quest = await _businessLayer.GetQuest(id);
            if (quest == null)
            {
                return NotFound("QuestNotFound");
            }
            return Ok(quest);
        }


        [HttpGet]
        [Route("quests-by-category")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsByCategory(Category category)
        {
            return Ok(await _businessLayer.GetQuestByCategory(category));
        }

        [HttpGet]
        [Route("test-rating")] 
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> getttTestrating()
        {
            return Ok(await _businessLayer.getttTestrating());
        }

        [HttpGet]
        [Route("quests-by-difficulty")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByDifficulty(Difficulty difficulty)
        {
            return Ok(await _businessLayer.GetQuestByDifficulty(difficulty));
        }

        [HttpGet]
        [Route("quests-by-category-and-difficulty")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByCategoryAndDifficulty(Category category, Difficulty difficulty)
        {
            return Ok(await _businessLayer.GetQuestByCategoryAndDifficulty(category, difficulty));
        }

        [HttpGet]
        [Route("quests-by-popularity")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsByPopularity()
        {
            return Ok(await _businessLayer.GetQuestsByPopularity());
        }

        [HttpGet]
        [Route("quests-by-successful-rate")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsBySuccessfulRate()
        {
            return Ok(await _businessLayer.GetQuestsBySuccessfulRate());
        }

        [HttpGet]
        [Route("quests-by-users-rating")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByUsersRating()
        {
            return Ok(await _businessLayer.GetQuestByUsersRating());
        }

        [HttpPut]
        [Route("resolve-quest-for-user")]
        public async Task<ActionResult<bool>> ResolveQuest(int questId, int userId, string answer)
        {
            var questSolved = await _businessLayer.ResolveQuest(questId, userId, answer);
            if (questSolved == null)
            {
                return BadRequest("Quest date is expired or the user is not allowed to solve it.");
            }

            return Ok(questSolved);
        }

        [HttpPut]
        [Route("rate-quest")]
        public async Task<ActionResult<bool>> RateQuest(int questId, int userId, int rating)
        {
            var questRated = await _businessLayer.RateQuest(questId, userId, rating);

            if (questRated == null)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }
            if (questRated == false)
            {
                return BadRequest("You can't rate the quest you created.");
            }

            return Ok(questRated);
        }

        [HttpDelete]
        [Route("{questId}")]
        public async Task<ActionResult<QuestDto>> DeleteQuest([FromRoute] int questId, int userId)
        {
            var questDeleted = await _businessLayer.DeleteQuest(questId, userId);
            if (questDeleted == null)
            {
                return NotFound("Quest not found");
            }
            if (questDeleted == false)
            {
                return BadRequest("Only the creator of the quest is allowed to delete it.");
            }
            return Ok();
        }
    }
}

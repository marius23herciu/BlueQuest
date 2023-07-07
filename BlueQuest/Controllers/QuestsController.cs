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

        [HttpPost("quest-{userId}")]
        public async Task<ActionResult<QuestDto>> CreateQuest([FromRoute] int userId, [FromBody] QuestToCreateDto quest)
        {
            if (quest is null)
            {
                return BadRequest("Complete all the required fields to create a quest.");
            }

            var newQuest = await _businessLayer.CreateQuest(userId, quest);
            if (newQuest == null)
            {
                return BadRequest("User has to be at leat Rank Blue to create quests and has to have at least 100 point in the quest category.");
            }
            return Ok(newQuest);
        }

        [HttpPost("add-{noOfPoints}/for-{category}")]
        public async Task<ActionResult<QuestDto>> AddPointsToUser([FromBody] int userId, [FromRoute] Category category, [FromRoute] int noOfPoints)
        {
            var pointsAdded = await _businessLayer.AddPointsToUser(userId, category, noOfPoints);
            if (pointsAdded == null)
            {
                return BadRequest("User not found.");
            }
            return Ok(pointsAdded);
        }
        [HttpPost("bonus-{points}")]
        public async Task<ActionResult<bool>> AddPointsToUserForEachCategory([FromBody]int userId, [FromRoute] int points)
        {
            var pointsAdded = await _businessLayer.AddPointsToUserForEachCategory(userId, points);
            if (pointsAdded == null)
            {
                return BadRequest("User not found.");
            }
            return Ok(pointsAdded);
        }
        
        [HttpGet]
        [Route("{questId}/{userId}")]
        public async Task<ActionResult<QuestDto>> GetQuest([FromRoute] int questId, [FromRoute] int userId)
        {
            var quest = await _businessLayer.GetQuest(questId, userId);
            if (quest == null)
            {
                return NotFound("QuestNotFound");
            }
            return Ok(quest);
        }
        [HttpGet]
        [Route("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            return Ok(await _businessLayer.GetAllCategories());
        }
        [HttpGet]
        [Route("{number}-random-quests/for-{userId}")]
        public async Task<ActionResult<List<int>>> GetRandomQuests([FromRoute]int number, [FromRoute] int userId)
        {
            return Ok(await _businessLayer.GetRandomQuests(number, userId));
        }

        [HttpGet]
        [Route("random-{number}/{category}/{userId}")]
        public async Task<ActionResult<List<int>>> GetRandomQuestsByCategory([FromRoute] int number, [FromRoute] string category, [FromRoute] int userId)
        {
            return Ok(await _businessLayer.GetRandomQuestsByCategory(category, number, userId));
        }
        [HttpGet]
        [Route("{number}-random/{difficulty}/{userId}")]
        public async Task<ActionResult<List<int>>> GetRandomQuestsByDifficulty([FromRoute] int number, [FromRoute] string difficulty, [FromRoute] int userId)
        {
            return Ok(await _businessLayer.GetRandomQuestsByDifficulty(difficulty, number, userId));
        }

        [HttpGet]
        [Route("difficulty")]
        public async Task<IActionResult> GetDifficultyLevels()
        {
            return Ok(await _businessLayer.GetDifficultyLevels());
        }

        [HttpGet]
        [Route("quests-by-{category}")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsByCategory([FromRoute]Category category)
        {
            return Ok(await _businessLayer.GetQuestByCategory(category));
        }
        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsByCreator([FromRoute] int userId)
        {
            return Ok(await _businessLayer.GetQuestByCreator(userId));
        }

        [HttpGet]
        [Route("{difficulty}/quests")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByDifficulty([FromRoute] Difficulty difficulty)
        {
            return Ok(await _businessLayer.GetQuestByDifficulty(difficulty));
        }

        [HttpGet]
        [Route("{category}-and-{difficulty}")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByCategoryAndDifficulty(Category category, Difficulty difficulty)
        {
            return Ok(await _businessLayer.GetQuestByCategoryAndDifficulty(category, difficulty));
        }

        [HttpGet]
        [Route("popularity")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsByPopularity()
        {
            return Ok(await _businessLayer.GetQuestsByPopularity());
        }

        [HttpGet]
        [Route("quests-by/successful-rate")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestsBySuccessfulRate()
        {
            return Ok(await _businessLayer.GetQuestsBySuccessfulRate());
        }

        [HttpGet]
        [Route("quests-by/users-rating")]
        public async Task<ActionResult<List<QuestBasicDetailsDto>>> GetQuestByUsersRating()
        {
            return Ok(await _businessLayer.GetQuestByUsersRating());
        }
        [HttpGet]
        [Route ("get-categ-to-create-for-{userId}")]
        public async Task<ActionResult<List<string>>> GetAvailableCategToCreate(int userId)
        {
            return Ok(await _businessLayer.GetAvailableCategToCreate(userId));
        }
        [HttpPut]
        [Route("solve-{questId}")]
        public async Task<ActionResult<bool>> ResolveQuest([FromRoute] int questId, SolveQuestDto quest)
        {
            var questSolved = await _businessLayer.ResolveQuest(questId, quest.UserId, quest.Answer);
            if (questSolved == null)
            {
                return BadRequest("Quest date is expired or the user is not allowed to solve it.");
            }

            return Ok(questSolved);
        }

        [HttpPut]
        [Route("rate-quest")]
        public async Task<ActionResult<bool>> RateQuest(RateQuestDto rateQuestDto)
        {
            var questRated = await _businessLayer.RateQuest(rateQuestDto.QuestId, rateQuestDto.UserId, rateQuestDto.Rating);

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

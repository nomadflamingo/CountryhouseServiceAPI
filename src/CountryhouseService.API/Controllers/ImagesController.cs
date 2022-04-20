using CountryhouseService.API.Dtos;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace CountryhouseService.API.Controllers
{
    [Route("api/v1/images")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImagesController> _logger;
        
        public ImagesController(
            IUnitOfWork unitOfWork, 
            ILogger<ImagesController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        // POST /images/ad-images
        [HttpPost("ad-images")]
        public async Task<ActionResult> CreateAdImageAsync([FromBody] CreateImageDto imageDto)
        {
            return await CreateImageAsync(imageDto, _unitOfWork.AdImagesRepository);
        }


        // POST /images/avatar
        [HttpPost("avatar")]
        public async Task<ActionResult> CreateAvatarAsync([FromBody] CreateImageDto imageDto)
        {
            return await CreateImageAsync(imageDto, _unitOfWork.AvatarsRepository);
        }


        private async Task<ActionResult> CreateImageAsync<T>(CreateImageDto createImageDto, IImagesRepository<T> imagesRepository) where T : Image, new()
        {
            // Try to add image to server
            try
            {
                Uri imgSource = await imagesRepository.UploadToServerAsync(createImageDto.Base64, createImageDto.Name);
                
                // Construct new image
                T imageToAdd = new()
                {
                    Source = imgSource
                };

                // Add image to db
                int imageId = await imagesRepository.AddToDbAsync(imageToAdd);

                // Save changes in db
                await _unitOfWork.SaveChangesAsync();

                // Return result
                ImageDto imageDto = new(imageId, imageToAdd.Source);
                _logger.LogControllerAction(LogLevel.Information, $"Created {nameof(T)} {imageDto}");
                return Created(imageToAdd.Source, null);
            }
            catch (HttpRequestException e)
            {
                _logger.LogControllerAction(LogLevel.Error, e.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch (NullReferenceException e)
            {
                _logger.LogControllerAction(LogLevel.Error, e.Message);
                return BadRequest(e.Message);
            }
        }


        private async Task<ActionResult> RemoveImageAsync<T>(int imageId, IImagesRepository<T> imagesRepository) where T : Image
        {
            // Get image by id
            T? img = await imagesRepository.GetAsync(imageId);
            if (img is null)
            {
                string notFoundMessage = $"{nameof(T)} with id {imageId} not found";

                _logger.LogControllerAction(LogLevel.Error, notFoundMessage);
                ModelState.AddModelError("", notFoundMessage);
                return BadRequest(imageId);
            }

            // Remove image from db
            imagesRepository.Remove(img);

            // Remove image from server
            try
            {
                await imagesRepository.DeleteFromServerAsync(img.Source);
            }
            catch (HttpRequestException e)
            {
                _logger.LogControllerAction(LogLevel.Error, e.Message);
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            // Save changes in db
            await _unitOfWork.SaveChangesAsync();

            // Return result
            _logger.LogControllerAction(LogLevel.Information, $"Removed {nameof(T)} {new ImageDto(imageId, img.Source)}");
            return NoContent();
        }
    }
}

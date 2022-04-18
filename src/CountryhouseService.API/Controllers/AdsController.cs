using CountryhouseService.API.Defaults;
using CountryhouseService.API.Dtos;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Helpers;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CountryhouseService.API.Controllers
{
    [Route("api/v1/ads")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdsController> _logger;
        private readonly UserManager<User> _userManager;

        public AdsController(
            IUnitOfWork unitOfWork, 
            ILogger<AdsController> logger, 
            UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }


        // GET /ads
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdDto>>> GetAsync(int skip = 0, int limit = 5, string? search = null)
        {
            if (search is null) search = "";
            IEnumerable<Ad> ads = await _unitOfWork.AdsRepository.GetFixedAmountAsync(
                searchBy: ad => ad.Title.Contains(search),
                trackChanges: false,
                skip: skip,
                limit: limit,
                ad => ad.Author, ad => ad.Status);
            
            AdDto[] adDtos = ads.Select(ad => ad.AsDto()).ToArray();

            _logger.LogControllerAction(LogLevel.Information, $"Retreived {adDtos.Length} ad entries");  
            return Ok(adDtos);
        }


        // GET /ads/{id}
        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<ActionResult<AdDto>> GetByIdAsync(int id)
        {
            Ad? ad = await _unitOfWork.AdsRepository.GetByIdOrDefaultAsync(id,
                false,
                ad => ad.Author, ad => ad.Status);

            if (ad is null)
            {
                string notFoundMessage = $"Ad with id {id} not found";
                _logger.LogControllerAction(LogLevel.Error, notFoundMessage);
                return NotFound(id);
            }

            if (ad.PreviewImageSource is not null)
            {
                await _unitOfWork.AdsRepository.LoadOrderedImagesAsync(ad);
            }

            _logger.LogControllerAction(LogLevel.Information, $"Retrieved ad with id {id}");  
            return Ok(ad.AsDto());
        }


        // POST /ads
        [HttpPost]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<ActionResult<AdDto>> CreateAsync([FromBody] CreateAdDto createAdDto)
        {
            if (createAdDto.AccomplishUntilDate?.Date < createAdDto.AccomplishFromDate?.Date)
            {
                ModelState.AddModelError(
                    nameof(createAdDto.AccomplishUntilDate),
                    "Accomplish until date cannot be earlier than accomplish from date");
            
                _logger.LogControllerAction(LogLevel.Error, $"Received invalid dto for creating ad");
                return BadRequest(ModelState);
            }

            // Get the published ad status
            AdStatus publishedStatus = await _unitOfWork.AdStatusesRepository.GetAsync(AdStatusNames.PUBLISHED);

            // Get current user
            User user = await _userManager.GetUserAsync(User);

            // Construct ad object from dto
            Ad ad = new()
            {
                Title = createAdDto.Title,
                Description = createAdDto.Description,
                Address = createAdDto.Address,
                Budget = createAdDto.Budget,
                ContactNumber = createAdDto.ContactNumber,
                PreviewImageSource = null,
                AccomplishFromDate = createAdDto.AccomplishFromDate,
                AccomplishUntilDate = createAdDto.AccomplishUntilDate,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                StatusId = publishedStatus.Id,
                Status = publishedStatus,
                AuthorId = user.Id,
                Author = user
            };

            // Validate ad model
            ICollection<ValidationResult> results = new List<ValidationResult>();
            ValidationContext vc = new(ad);
            if (!Validator.TryValidateObject(ad, vc, results, true))
            {
                _logger.LogControllerAction(LogLevel.Error, "Received invalid dto for creating ad");
                return BadRequest(ModelState);
            }

            // Add preview image source if there are images
            if (createAdDto.ImagesIds != null && createAdDto.ImagesIds.Any())
            {
                try
                {
                    await AssignPreviewImageAsync(ad, createAdDto.ImagesIds.First());
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogControllerAction(LogLevel.Error, e.Message);
                    ModelState.AddModelError(nameof(createAdDto.ImagesIds), e.Message);
                    return NotFound(ModelState);
                }
            }

            // Add constructed object to db
            int adId = await _unitOfWork.AdsRepository.CreateAsync(ad);

            // Assign ad images to the appropriate ad (skip the preview image)
            if (createAdDto.ImagesIds != null && createAdDto.ImagesIds.Skip(1).Any())
            {
                try
                {
                    await AssignNonPreviewImagesAsync(ad, createAdDto.ImagesIds.Skip(1));
                }
                catch (InvalidOperationException e)
                {
                    _logger.LogControllerAction(LogLevel.Error, e.Message);
                    ModelState.AddModelError(nameof(createAdDto.ImagesIds), e.Message);
                    return NotFound(ModelState);
                }
                catch (ArgumentException e)
                {
                    _logger.LogControllerAction(LogLevel.Error, e.Message);
                    ModelState.AddModelError(nameof(createAdDto.ImagesIds), e.Message);
                    return BadRequest(ModelState);
                }
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync();
            
            // Log info
            _logger.LogControllerAction(LogLevel.Information, $"Created ad with id {adId}");
            
            // Return result
            return CreatedAtAction(nameof(GetByIdAsync), new { id = adId }, ad.AsDto());
        }


        // DELETE ads/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<IActionResult> CancelAsync(int id)
        {
            Ad? ad = await _unitOfWork.AdsRepository.GetByIdOrDefaultAsync(id);
            if (ad is null)
            {
                string notFoundMessage = $"Ad with id {id} not found";

                ModelState.AddModelError("", notFoundMessage);
                _logger.LogControllerAction(LogLevel.Error, notFoundMessage);
                return NotFound(ModelState);
            }
            else
            {
                // Check that the action was performed by an ad author or an admin
                bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
                bool IsAuthor = ControllerHelpers.IsAdAuthor(ad, User);
                bool isAllowedToCancel = IsAdmin || IsAuthor;
                if (!isAllowedToCancel)
                {
                    _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to cancel ad with id {id}");
                    return Forbid();
                }

                // Set new status
                AdStatus cancelledStatus = await _unitOfWork.AdStatusesRepository.GetAsync(AdStatusNames.CANCELLED);
                ad.Status = cancelledStatus;
                ad.StatusId = cancelledStatus.Id;

                // Update in db and log to controller
                _unitOfWork.AdsRepository.Update(ad);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogControllerAction(LogLevel.Information, $"Cancelled ad with id {id}");
                return NoContent();
            }
        }


        // PUT ads/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<ActionResult<AdDto>> EditAsync(int id, [FromBody] UpdateAdDto updateAdDto)
        {
            // Check that dates are valid
            if (updateAdDto.AccomplishUntilDate?.Date < updateAdDto.AccomplishFromDate?.Date)
            {
                ModelState.AddModelError(
                    nameof(updateAdDto.AccomplishUntilDate),
                    "Accomplish until date should be greater than accomplish from date");

                _logger.LogControllerAction(LogLevel.Error, "Received invalid model for updating ad");
                return BadRequest(ModelState);
            }

            // Get ad from db and check if it is null
            Ad? ad = await _unitOfWork.AdsRepository.GetByIdOrDefaultAsync(id, 
                false,
                ad => ad.Author, ad => ad.Status);

            if (ad is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Ad with id {id} not found");
                return NotFound(id);
            }

            // Check that current user is ad owner or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAuthor = ControllerHelpers.IsAdAuthor(ad, User);
            bool isAllowedToEdit = IsAdmin || IsAuthor;
            if (!isAllowedToEdit)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Access denied for ad with id {id}");
                return Forbid();
            }

            // Construct an updated ad object
            Ad updatedAd = new()
            {
                Id = ad.Id,
                Title = updateAdDto.Title,
                Description = updateAdDto.Description,
                Address = updateAdDto.Address,
                Budget = updateAdDto.Budget,
                ContactNumber = updateAdDto.ContactNumber,
                PreviewImageSource = null,
                CreatedOn = ad.CreatedOn,
                UpdatedOn = DateTime.UtcNow,
                StatusId = ad.StatusId,
                Status = ad.Status,
                AuthorId = ad.AuthorId,
                Author = ad.Author,
                AccomplishFromDate = updateAdDto.AccomplishFromDate,
                AccomplishUntilDate = updateAdDto.AccomplishUntilDate,
            };

            // Validate new ad model
            ICollection<ValidationResult> results = new List<ValidationResult>();
            ValidationContext vc = new(updatedAd);
            if (!Validator.TryValidateObject(updatedAd, vc, results, true))
            {
                foreach (var result in results)
                    ModelState.AddModelError(result.MemberNames.FirstOrDefault() ?? "", result.ErrorMessage ?? "Invalid field");

                _logger.LogControllerAction(LogLevel.Error, $"Received invalid dto for creating ad");
                return BadRequest(ModelState);
            }
            
            try
            {
                // Set new preview image
                if (updateAdDto.ImagesIds != null && updateAdDto.ImagesIds.Any())
                    await AssignPreviewImageAsync(updatedAd, updateAdDto.ImagesIds.First());

                // Assign new ad images to ad if there were any in the model
                if (updateAdDto.ImagesIds != null && updateAdDto.ImagesIds.Skip(1).Any())
                    await AssignNonPreviewImagesAsync(updatedAd, updateAdDto.ImagesIds.Skip(1));
            }
            catch (InvalidOperationException e)
            {
                _logger.LogControllerAction(LogLevel.Error, e.Message);
                ModelState.AddModelError(nameof(updateAdDto.ImagesIds), e.Message);
                return NotFound(ModelState);
            }
            catch (ArgumentException e)
            {
                _logger.LogControllerAction(LogLevel.Error, e.Message);
                ModelState.AddModelError(nameof(updateAdDto.ImagesIds), e.Message);
                return BadRequest(ModelState);
            }

            // Update ad object in a database
            _unitOfWork.AdsRepository.Update(updatedAd);

            // Check if there were any other images in the model and deassociate them
            // (They will be deleted automatically from db and from server after some period of time)
            await _unitOfWork.AdsRepository.LoadOrderedImagesAsync(ad);
            if (ad.NonPreviewImages != null && ad.NonPreviewImages.Any())
            {
                DissociateNonPreviewImages(ad.NonPreviewImages);
            }

            // Save changes
            await _unitOfWork.SaveChangesAsync();

            // Try delete preview image from server and if failed, revert changes
            if (ad.PreviewImageSource != null)
            {
                try
                {
                    await _unitOfWork.AdImagesRepository.DeleteFromServerAsync(ad.PreviewImageSource);
                }
                catch (HttpRequestException e)
                {
                    await RevertToPreviousAdAsync(ad);
                    _logger.LogControllerAction(LogLevel.Error, e.Message);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                    // TODO: check logged message
                }
            }

            // Return result
            _logger.LogControllerAction(LogLevel.Information, $"Updated ad {id} with dto {updateAdDto}");
            return Ok(updatedAd.AsDto());
        }


        // GET ads/{id}/requests
        [HttpGet("{id}/requests")]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<ActionResult<IEnumerable<RequestDto>>> GetRequestsForAdAsync(int id, int skip = 0, int limit = 5)
        {
            // Find ad
            Ad? ad = await _unitOfWork.AdsRepository.GetByIdOrDefaultAsync(
                id,
                true);

            // Check if it is null
            if (ad is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Ad with id {id} not found");
                return NotFound(id);
            }
            
            // Check if user is allowed
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAdAuthor = ControllerHelpers.IsAdAuthor(ad, User);
            bool isAllowedToSeeRequests = IsAdmin || IsAdAuthor;
            if (!isAllowedToSeeRequests)
            {
                _logger.LogControllerAction(LogLevel.Error, 
                    $"User is not allowed to accept request with id {id}");
                return Forbid();
            }

            // Load requests and contractors
            await _unitOfWork.AdsRepository.LoadRequestsAsync(
                ad: ad,
                orderBy: null,
                orderByDescending: false,
                skip: skip,
                limit: limit,
                rq => rq.Contractor, rq => rq.Status);

            _logger.LogControllerAction(LogLevel.Information, $"Loaded {ad.Requests.Count}");
            return Ok(ad.Requests.Select(r => r.AsDto()));
        }


        // POST ads/{id}/request
        [HttpPost("{id}/request")]
        [Authorize(Roles = UserRoleNames.CONTRACTOR)]
        public async Task<ActionResult> CreateRequestAsync(int id, [FromBody] CreateRequestDto createRequestDto)
        {
            // Find ad
            Ad? ad = await _unitOfWork.AdsRepository.GetByIdOrDefaultAsync(
                id,
                true,
                ad => ad.Status);

            // Check if it is null
            if (ad is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Ad with id {id} not found");
                return NotFound(id);
            }

            // Check ad status
            if (ad.Status.Name != AdStatusNames.PUBLISHED)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Cannot add request to ad with status {ad.Status.Name}");
                return BadRequest();
            }

            // Get current user and status
            User currentUser = await _userManager.GetUserAsync(User);
            RequestStatus pendingStatus = await _unitOfWork.RequestStatusesRepository.GetAsync(RequestStatusNames.PENDING);
            Request request = new()
            {
                Comment = createRequestDto.Comment,
                Contractor = currentUser,
                ContractorId = currentUser.Id,
                Ad = ad,
                AdId = id,
                Status = pendingStatus,
                StatusId = pendingStatus.Id,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow
            };

            // Insert request to db
            int requestId = await _unitOfWork.RequestsRepository.CreateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogControllerAction(LogLevel.Information, $"Created request with id {requestId}");
            return Created(requestId.ToString(), request.AsDto());
        }


        private async Task RevertToPreviousAdAsync(Ad ad)
        {
            _unitOfWork.AdsRepository.Update(ad);
            if (ad.NonPreviewImages != null)
            {
                foreach (AdImage image in ad.NonPreviewImages)
                {
                    image.AdId = ad.Id;
                    _unitOfWork.AdImagesRepository.Update(image);
                }
                await _unitOfWork.SaveChangesAsync();
            }
        }


        private void DissociateNonPreviewImages(ICollection<AdImage> nonPreviewImages)
        {
            foreach (AdImage image in nonPreviewImages)
            {
                // Deassociate image with ad
                image.AdId = null;

                // Update image in db
                _unitOfWork.AdImagesRepository.Update(image);
            }
        }


        /// <summary>
        /// Assigns the preview image field to an already defined ad, provided an image id stored in db.
        /// Throws an InvalidOperationException if image is not found by id.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">This exception is thrown when image is not found</exception>
        private async Task AssignPreviewImageAsync(Ad ad, int previewImageId)
        {
            AdImage? previewImage = await _unitOfWork.AdImagesRepository.GetAsync(previewImageId);
            if (previewImage is null)
                throw new InvalidOperationException($"Image with id {previewImageId} not found");

            ad.PreviewImageSource = previewImage.Source;

            // Delete preview image from adImage table
            _unitOfWork.AdImagesRepository.Remove(previewImage);
        }


        /// <summary>
        /// Assigns non-preview images to an alreadly defined ad provided images' ids stored in db.
        /// Throws an InvalidOperationException if image is not found by id.
        /// Throws an ArgumentException if loaded too many images.
        /// </summary>
        /// <param name="imgsIds">An IEnumerable of non-preview images</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">When image is not found or image is in use</exception>
        /// <exception cref="ArgumentException">When trying to load too many images</exception>
        private async Task AssignNonPreviewImagesAsync(Ad ad, IEnumerable<int> imgsIds)
        {
            ad.NonPreviewImages = new List<AdImage>();
            using var enumerator = imgsIds.GetEnumerator();
            for (byte i = 2; enumerator.MoveNext(); i++)
            {
                int imgId = enumerator.Current;
                AdImage? img = await _unitOfWork.AdImagesRepository.GetAsync(imgId);

                if (img is null)
                    throw new InvalidOperationException($"Image with id {imgId} not found");

                if (img.AdId != null)
                    throw new InvalidOperationException($"Image with id {imgId} is already in use");

                // Assign order and ad id
                img.AdId = ad.Id;
                img.Order = i;

                // Validate that order is valid
                ICollection<ValidationResult> results = new List<ValidationResult>();
                ValidationContext vc = new(img) { MemberName = nameof(img.Order) };
                if (!Validator.TryValidateProperty(img.Order, vc, results))
                    throw new ArgumentException("Attempted to load too many images");

                // Add image in the ad.NonPreviewImages collection (for later display in dto)
                ad.NonPreviewImages.Add(img);

                _unitOfWork.AdImagesRepository.Update(img);
            }
        }
    }
}

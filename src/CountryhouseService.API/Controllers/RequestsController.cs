using CountryhouseService.API.Defaults;
using CountryhouseService.API.Dtos;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Helpers;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CountryhouseService.API.Controllers
{
    [Route("api/v1/requests")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            ILogger<RequestsController> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }
        

        // DELETE requests/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = UserRoleNames.CONTRACTOR)]
        public async Task<ActionResult> DeleteRequestAsync(int id)
        {
            // Get request by id
            Request? request = await _unitOfWork.RequestsRepository.GetByIdOrDefaultAsync(id, true);
            if (request is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Request with id {id} not found");
                return NotFound(id);
            }

            // Check that current user is either request author or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAuthor = ControllerHelpers.IsRequestAuthor(request, User);
            bool isAllowedToDelete = IsAdmin || IsAuthor;
            if (!isAllowedToDelete)
            {
                _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to delete request with id {id}");
                return Forbid();
            }

            _unitOfWork.RequestsRepository.Remove(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogControllerAction(LogLevel.Information, $"Deleted request {id}");
            return NoContent();
        }


        // PUT /requests/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = UserRoleNames.CONTRACTOR)]
        public async Task<ActionResult> EditRequestAsync(int id, [FromBody] UpdateRequestDto updateRequestDto)
        {
            // Get request by id
            Request? request = await _unitOfWork.RequestsRepository.GetByIdOrDefaultAsync(
                id, 
                true,
                rq => rq.Ad, rq => rq.Contractor, rq => rq.Status);

            if (request is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Request with id {id} not found");
                return NotFound(id);
            }

            // Check that current user is request author or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAuthor = ControllerHelpers.IsRequestAuthor(request, User);
            bool isAllowedToEdit = IsAdmin || IsAuthor;
            if (!isAllowedToEdit)
            {
                _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to delete request with id {id}");
                return Forbid();
            }

            request.Comment = updateRequestDto.Comment;
            request.UpdatedOn = DateTime.UtcNow;

            // Validate request model
            ICollection<ValidationResult> results = new List<ValidationResult>();
            ValidationContext vc = new(request);
            if (!Validator.TryValidateObject(request, vc, results, true))
            {
                _logger.LogControllerAction(LogLevel.Error, "Received invalid dto for creating request");
                return BadRequest(ModelState);
            }

            // Update, save, log
            _unitOfWork.RequestsRepository.Update(request);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogControllerAction(LogLevel.Information, $"Updated request {id} with dto: {updateRequestDto}");
            return Ok(request.AsDto());
        }


        // PUT /requests/{id}/accept
        [HttpPut("{id}/accept")]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<ActionResult> AcceptRequestAsync(int id)
        {
            // Get request by id
            Request? currentRequest = await _unitOfWork.RequestsRepository.GetByIdOrDefaultAsync(
                id, 
                true,
                rq => rq.Ad, rq => rq.Status);

            if (currentRequest is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Request with id {id} not found");
                return NotFound(id);
            }

            // Cannot change request status if it has alreadly been changed
            if (currentRequest.Status.Name != RequestStatusNames.PENDING)
            {
                _logger.LogControllerAction(LogLevel.Error, 
                    $"Cannot accept request with id {id} because it alreadly has status {currentRequest.Status.Name}");
                return Forbid();
            }

            // Check that user is ad owner or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAdAuthor = ControllerHelpers.IsAdAuthor(currentRequest.Ad, User);
            bool isAllowedToAccept = IsAdmin || IsAdAuthor;
            if (!isAllowedToAccept)
            {
                _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to accept request with id {id}");
                return Forbid();
            }

            // Get request statuses
            var requestStatuses = await _unitOfWork.RequestStatusesRepository.GetAllAsync();

            // Change all requests status to rejected
            RequestStatus requestRejectedStatus = requestStatuses.First(rq => rq.Name == RequestStatusNames.REJECTED);
            await _unitOfWork.AdsRepository.LoadRequestsAsync(currentRequest.Ad); 
            foreach (var request in currentRequest.Ad.Requests)
            {
                request.Status = requestRejectedStatus;
                request.StatusId = requestRejectedStatus.Id;
            }

            // Change current request status to accepted
            RequestStatus requestAcceptedStatus = requestStatuses.First(rq => rq.Name == RequestStatusNames.ACCEPTED);
            currentRequest.Status = requestAcceptedStatus;
            currentRequest.StatusId = requestAcceptedStatus.Id;

            // Change ad status to accepted
            AdStatus adAcceptedStatus = await _unitOfWork.AdStatusesRepository.GetAsync(AdStatusNames.ACCEPTED);
            currentRequest.Ad.Status = adAcceptedStatus;
            currentRequest.Ad.StatusId = adAcceptedStatus.Id;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogControllerAction(LogLevel.Information, $"Request with id {id} for ad {currentRequest.AdId} accepted");
            return NoContent();
        }


        // PUT /requests/{id}/reject
        [HttpPut("{id}/reject")]
        [Authorize(Roles = UserRoleNames.OWNER)]
        public async Task<ActionResult> RejectRequestAsync(int id)
        {
            // Get request by id
            Request? request = await _unitOfWork.RequestsRepository.GetByIdOrDefaultAsync(
                id,
                true,
                rq => rq.Ad, rq => rq.Status);

            if (request is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Request with id {id} not found");
                return NotFound(id);
            }

            // Cannot change request status if it has alreadly been changed
            if (request.Status.Name != RequestStatusNames.PENDING)
            {
                _logger.LogControllerAction(LogLevel.Error,
                    $"Cannot accept request with id {id} because it alreadly has status {request.Status.Name}");
                return Forbid();
            }

            // Check that user is ad author or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsAdAuthor = ControllerHelpers.IsAdAuthor(request.Ad, User);
            bool isAllowedToReject = IsAdmin || IsAdAuthor;
            if (!isAllowedToReject)
            {
                _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to reject request with id {id}");
                return Forbid();
            }

            // Get request statuses
            var requestStatuses = await _unitOfWork.RequestStatusesRepository.GetAllAsync();

            // Change current request status to rejected
            RequestStatus requestRejectedStatus = requestStatuses.First(rq => rq.Name == RequestStatusNames.REJECTED);
            request.Status = requestRejectedStatus;
            request.StatusId = requestRejectedStatus.Id;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogControllerAction(LogLevel.Information, $"Request with id {id} for ad {request.AdId} rejected");
            return NoContent();
        }


        // PUT /requests/{id}/accomplish
        [HttpPut("{id}/accomplish")]
        [Authorize(Roles = UserRoleNames.CONTRACTOR)]
        public async Task<ActionResult> AccomplishRequestAsync(int id)
        {
            // Make sure that this is an action perfrmed by the request author
            // Make sure that the request status is accepted
            // Set ad status to accomplished

            // Get request by id
            Request? request = await _unitOfWork.RequestsRepository.GetByIdOrDefaultAsync(
                id,
                true,
                rq => rq.Ad, rq => rq.Status);

            if (request is null)
            {
                _logger.LogControllerAction(LogLevel.Error, $"Request with id {id} not found");
                return NotFound(id);
            }

            // Cannot accomplish ad if it wasn't accepted
            if (request.Status.Name != RequestStatusNames.ACCEPTED)
            {
                _logger.LogControllerAction(LogLevel.Error,
                    $"Cannot accept request with id {id}, because it has status {request.Status.Name}");
                return Forbid();
            }

            // Check that user is request author or admin
            bool IsAdmin = await ControllerHelpers.IsAdminAsync(_userManager, User);
            bool IsRequestAuthor = ControllerHelpers.IsRequestAuthor(request, User);
            bool isAllowed = IsAdmin || IsRequestAuthor;
            if (!isAllowed)
            {
                _logger.LogControllerAction(LogLevel.Error, $"User is not allowed to accomplish request with id {id}");
                return Forbid();
            }

            // Set ad status to accomplished
            AdStatus adAccomplishedStatus = await _unitOfWork.AdStatusesRepository.GetAsync(AdStatusNames.ACCOMPLISHED);
            request.Ad.Status = adAccomplishedStatus;
            request.Ad.StatusId = adAccomplishedStatus.Id;

            // Save, log, return
            await _unitOfWork.SaveChangesAsync();
            _logger.LogControllerAction(LogLevel.Information, $"Request with id {id} for ad {request.AdId} accomplished");
            return NoContent();
        }
    }
}

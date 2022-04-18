using CountryhouseService.API.Controllers;
using CountryhouseService.API.Defaults;
using CountryhouseService.API.Dtos;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using CountryhouseService.Tests.Data;
using CountryhouseService.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CountryhouseService.Tests
{
    public class AdsControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkStub = new();
        private readonly Mock<IAdsRepository> _adsRepository = new();
        private readonly Mock<ILogger<AdsController>> _loggerStub = new();
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Random _rand = new();
        
        
        public AdsControllerTests()
        {
            _unitOfWorkStub.SetupGet(u => u.AdsRepository)
                .Returns(_adsRepository.Object);

            // Mock user manager
            Mock<IUserStore<User>> store = new();
            _userManager = new(store.Object, null, null, null, null, null, null, null, null);
        }


        [Fact]
        public async Task GetByIdAsync_WithUnexistingAd_ReturnsNotFound()
        {
            // Arrange
            AdsController controller = new(
                _unitOfWorkStub.Object, 
                _loggerStub.Object, 
                _userManager.Object);
            
            _adsRepository.Setup(  // When requeting ad, returns null
                r => r.GetByIdOrDefaultAsync(
                    It.IsAny<int>(), 
                    It.IsAny<bool>(), 
                    It.IsAny<Expression<Func<Ad, object>>[]>()))
                .ReturnsAsync((Ad?)null);

            int id = _rand.Next();

            // Act
            var result = await controller.GetByIdAsync(id);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
            (result.Result as NotFoundObjectResult).Value.Should().BeEquivalentTo(id);
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidAds), MemberType = typeof(AdsControllerTestsData))]
        public async Task GetByIdAsync_WithExistingAd_ReturnsAdDto(Ad ad)
        {
            // Arrange
            AdsController controller = new(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);
            
            _adsRepository.Setup(  // When requesting ad, returns valid ad
                r => r.GetByIdOrDefaultAsync(
                    It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
                .ReturnsAsync(ad);

            // Act
            var result = await controller.GetByIdAsync(_rand.Next());

            // Assert
            // Check that return type is correct
            result.Result.Should().BeOfType<OkObjectResult>();
            
            // Check that fields didn't change
            (result.Result as OkObjectResult).Value
                .Should().BeEquivalentTo(ad.AsDto());

            // Check that images were loaded
            if (ad.PreviewImageSource != null)
                _adsRepository.Verify(
                    r => r.LoadOrderedImagesAsync(It.IsAny<Ad>()), Times.Once);

        }

        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidAdLists), MemberType = typeof(AdsControllerTestsData))]
        public async Task GetAsync_WithExistingAds_ReturnsAdDtos(IEnumerable<Ad> ads)
        {
            // Arrange
            IEnumerable<AdDto> adDtos = ads.Select(a => a.AsDto());

            /*_adsRepository.Setup(  // When requesting ads, returns valid ad list
                r => r.GetAllAsync(
                    It.IsAny<Expression<Func<Ad, bool>>>(),
                    It.IsAny<bool>(), It.IsAny<int>(),
                    It.IsAny<Expression<Func<Ad, object>>[]>()))
                .ReturnsAsync(ads);*/

            AdsController controller = new(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            // Act
            var result = await controller.GetAsync();

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            var resultAdDtos = (result.Result as OkObjectResult).Value as IEnumerable<AdDto>;
            resultAdDtos.Should().BeEquivalentTo(adDtos);
        }

        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidCreateAdDtos), MemberType = typeof(AdsControllerTestsData))]
        public async Task CreateAsync_WithValidDtos_ReturnsCreatedAdAsDto(CreateAdDto createAdDto)
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            User user = new()  // Setup user to display in dto
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };

            int statusId = _rand.Next();
            _unitOfWorkStub.Setup(  // When requesting published ad status, returns published status with random id
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.PUBLISHED))
                .ReturnsAsync(new AdStatus { Id = statusId, Name = AdStatusNames.PUBLISHED });

            int adId = _rand.Next();
            _adsRepository.Setup(  // When creating ad with model, returns random id and assigns it to model
                r => r.CreateAsync(It.IsAny<Ad>()))
                .ReturnsAsync(adId)
                .Callback<Ad>(ad => ad.Id = adId);

            _userManager.Setup(  // When requesting current user, returns set up user
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            // Setup image uris that increment each time a method is called
            // This is used to later check that images were added in order
            string uriPrefix = @"http://www.example.com/";
            int order = 1;
            _unitOfWorkStub.Setup(
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(() => new AdImage { Source = new Uri(uriPrefix + order) })
                .Callback<int>(id =>
                {
                    /*
                     * Check that the repository was called with an id that really exists 
                     * In createDto images array and that the order which is assigned to this uri 
                     * Corresponds to this id's position in the createDto images array
                     */
                    createAdDto.ImagesIds.ElementAt(order - 1).Should().Be(id);
                    order++;
                });


            // Act
            var result = await controller.CreateAsync(createAdDto);

            // Assert
            // Check the type of the result
            result.Result.Should().BeOfType<CreatedAtActionResult>();
            
            var createdAtActionResult = (result.Result as CreatedAtActionResult);

            // Check that the action name is correct
            createdAtActionResult.ActionName.Should().BeEquivalentTo(nameof(AdsController.GetByIdAsync));

            // Check that route values contain specified id
            createdAtActionResult.RouteValues.Should()
                .ContainKey("Id").WhoseValue.Should().BeEquivalentTo(adId);

            var resultDto = createdAtActionResult.Value as AdDto;

            // Check that author is the same
            resultDto.AuthorId.Should().BeEquivalentTo(user.Id);

            // Check that status is set correctly
            resultDto.Status.Should().BeEquivalentTo(AdStatusNames.PUBLISHED);

            // Check id
            resultDto.Id.Should().Be(adId);

            // Check that created date and updated date are close to current date
            resultDto.CreatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            resultDto.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            // Check other fields
            resultDto.Should().BeEquivalentTo(createAdDto, options =>
                options.ExcludingMissingMembers());

            // Check that image path was not modified by the controller and that they were added in order
            if (createAdDto.ImagesIds is not null)
            {
                // Check that images count in dto is correct
                resultDto.NonPreviewImages.Should().HaveCount(createAdDto.ImagesIds.Count-1);
                
                // Check that preview image starts with a valid prefix
                resultDto.PreviewImage.ToString().Should().StartWith(uriPrefix);

                // Check that preview image is the first image added
                resultDto.PreviewImage.ToString().Should().EndWith("1");

                if (createAdDto.ImagesIds.Skip(1).Any())
                {
                    using var enumerator = resultDto.NonPreviewImages.GetEnumerator();
                    for (int i = 2; enumerator.MoveNext(); i++)
                    {
                        Uri imageUri = enumerator.Current;

                        // Check prefix
                        imageUri.ToString().Should().StartWith(uriPrefix);

                        // Check order
                        imageUri.ToString().Should().EndWith(i.ToString());
                    }
                }

                // Check that preview image is deleted from db as it's source should only be stored in ad.PreviewImage property
                _unitOfWorkStub.Verify(u => u.AdImagesRepository.Remove(
                    It.Is<AdImage>(i => i.Source.ToString().EndsWith("1"))), Times.Once);
            }

            // Check that the image was never assigned an invalid ad id or an invalid uri prefix
            _unitOfWorkStub.Verify(u => u.AdImagesRepository.Update(
                It.Is<AdImage>(i => i.AdId != adId || !i.Source.ToString().StartsWith(uriPrefix))), Times.Never);


            // Check that changes were saved once
            _unitOfWorkStub.Verify(u => u.SaveChangesAsync(), Times.Once());
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.InvalidCreateAdDtos), MemberType = typeof(AdsControllerTestsData))]
        public async Task CreateAsync_WithInvalidDtos_ReturnsBadRequest(CreateAdDto createAdDto)
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            User user = new()  // Setup user to display in dto
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };

            int statusId = _rand.Next();
            _unitOfWorkStub.Setup(  // When requresting published status, returns new status with a random id
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.PUBLISHED))
                .ReturnsAsync(new AdStatus { Id = statusId, Name = AdStatusNames.PUBLISHED });

            int adId = _rand.Next();
            _adsRepository.Setup(  // When creating ad with model, returns random id and assigns it to model
                r => r.CreateAsync(It.IsAny<Ad>()))
                .ReturnsAsync(adId)
                .Callback<Ad>(ad => ad.Id = adId);

            _userManager.Setup(  // When requesting current user, returns set up user
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            _unitOfWorkStub.Setup(  // When requesting images, returns dummy image with source
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdImage { Source = new Uri(@"http://www.example.com/abc") });

            // Act
            var result = await controller.CreateAsync(createAdDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_WithUnexistingImages_ReturnsNotFound()
        {
            // Arrange
            CreateAdDto createAdDto = AdsControllerTestsData.CreateRandomCreateAdDto(2);

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            User user = new()  // Setup user to display in dto
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };

            int statusId = _rand.Next();
            _unitOfWorkStub.Setup(  // When requresting published status, returns new status with a random id
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.PUBLISHED))
                .ReturnsAsync(new AdStatus { Id = statusId, Name = AdStatusNames.PUBLISHED });

            int adId = _rand.Next();
            _adsRepository.Setup(  // When creating ad with model, returns random id and assigns it to model
                r => r.CreateAsync(It.IsAny<Ad>()))
                .ReturnsAsync(adId)
                .Callback<Ad>(ad => ad.Id = adId);

            _userManager.Setup(  // When requesting current user, returns set up user
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            _unitOfWorkStub.Setup(  // When requesting images, returns null
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync((AdImage?)null);


            // Act
            var result = await controller.CreateAsync(createAdDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidCreateAdDtosWithImagesIds), MemberType = typeof(AdsControllerTestsData))]
        public async Task CreateAsync_WithUnexistingNonPreviewImages_ReturnsNotFound(CreateAdDto createAdDto)
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            User user = new()  // Setup user to display in dto
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };

            int statusId = _rand.Next();
            _unitOfWorkStub.Setup(  // When requresting published status, returns new status with a random id
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.PUBLISHED))
                .ReturnsAsync(new AdStatus { Id = statusId, Name = AdStatusNames.PUBLISHED });

            int adId = _rand.Next();
            _adsRepository.Setup(  // When creating ad with model, returns random id and assigns it to model
                r => r.CreateAsync(It.IsAny<Ad>()))
                .ReturnsAsync(adId)
                .Callback<Ad>(ad => ad.Id = adId);

            _userManager.Setup(  // When requesting current user, returns set up user
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            _unitOfWorkStub.SetupSequence(  // When requesting images, first returns dummy image, then null
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdImage { Source = new Uri(@"http://www.example.com/abc") })
                .ReturnsAsync((AdImage?)null);

            // Act
            var result = await controller.CreateAsync(createAdDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidAds), MemberType = typeof(AdsControllerTestsData))]
        public async Task CancelAsync_WithExistingAdAndAdCreator_ReturnsNoContent(Ad existingAd)
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);
            
            User user = new()  // Setup user as ad author
            {
                Id = existingAd.AuthorId
            };

            _userManager.Setup(  // When requesing current user, returns ad author
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            _unitOfWorkStub.Setup(  // When requesting cancelled ad status, returns it
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.CANCELLED))
                .ReturnsAsync(new AdStatus { Name = AdStatusNames.CANCELLED });

            _adsRepository.Setup(
               r => r.GetByIdOrDefaultAsync(  // When requesting ad, returns existing ad
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(existingAd);

            // Act
            var result = await controller.CancelAsync(existingAd.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidAds), MemberType = typeof(AdsControllerTestsData))]
        public async Task CancelAsync_WithExistingAdAndAdminUser_ReturnsNoContent(Ad existingAd)
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            User user = new();
            _userManager.Setup(  // When requesting current user, returns dummy user
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(user);

            _userManager.Setup(  // When checking current user roles, returns that user is admin
                um => um.IsInRoleAsync(user, UserRoleNames.ADMIN))
                .ReturnsAsync(true);

            _unitOfWorkStub.Setup(  // When requesting cancelled ad status, returns new ad status
                u => u.AdStatusesRepository.GetAsync(AdStatusNames.CANCELLED))
                .ReturnsAsync(new AdStatus { Name = AdStatusNames.CANCELLED });

            _adsRepository.Setup(  // When requesting ad, returns existing ad
               r => r.GetByIdOrDefaultAsync(
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(existingAd);

            // Act
            var result = await controller.CancelAsync(existingAd.Id);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }
        

        [Fact]
        public async Task CancelAsync_WithUnexistingAd_ReturnsNotFound()
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            _adsRepository.Setup(
               r => r.GetByIdOrDefaultAsync(
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync((Ad?)null);

            // Act
            var result = await controller.CancelAsync(_rand.Next());

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task CancelAsync_WithInvalidUser_ReturnsForbidden()
        {
            // Arrange
            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            _userManager.Setup(
                um => um.GetUserAsync(controller.User))
                .ReturnsAsync(new User());

            _adsRepository.Setup(
               r => r.GetByIdOrDefaultAsync(
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(new Ad());

            // Act
            var result = await controller.CancelAsync(_rand.Next());

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.ValidUpdateAdDtos), MemberType = typeof(AdsControllerTestsData))]
        public async Task EditAsync_WithValidDtoAndAdCreator_ReturnsUpdatedDto(Ad ad, UpdateAdDto updateAdDto, string changedPropertyName, object changedPropertyValue)
        {
            // Arrange

            // Setup user claims
            string userNameIdentifier = ad.AuthorId;
            string userName = Guid.NewGuid().ToString();

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(userNameIdentifier, userName);

            User user = new()  // Setup user as ad author and to display in dto
            {
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };

            _adsRepository.Setup(  // When requesting ad, returns ad from test data
               r => r.GetByIdOrDefaultAsync(
                   ad.Id, 
                   It.IsAny<bool>(), 
                   It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(ad);

            _unitOfWorkStub.Setup(  // When deleting preview image from server, returns deleted image uri (success)
                u => u.AdImagesRepository.DeleteFromServerAsync(ad.PreviewImageSource))
                .ReturnsAsync(ad.PreviewImageSource);

            // Setup image uris that increment each time a method is called
            // This is used to later check that images were added in order
            string uriPrefix = @"http://www.example.com/";
            int order = 1;
            _unitOfWorkStub.Setup(
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(() => new AdImage { Source = new Uri(uriPrefix + order) })
                .Callback<int>(id =>
                {
                    /*
                     * Check that the repository was called with an id that really exists 
                     * In createDto images array and that the order which is assigned to this uri 
                     * Corresponds to this id's position in the createDto images array
                     */
                    updateAdDto.ImagesIds.ElementAt(order - 1).Should().Be(id);
                    order++;
                });


            // Act
            var result = await controller.EditAsync(ad.Id, updateAdDto);


            // Assert
            // Check the type of the result
            result.Result.Should().BeOfType<OkObjectResult>();

            var resultDto = (result.Result as OkObjectResult).Value as AdDto;

            // Check that author is the same
            resultDto.AuthorId.Should().BeEquivalentTo(ad.AuthorId);

            // Check that status is set correctly
            resultDto.Status.Should().BeEquivalentTo(ad.Status.Name);

            // Check id
            resultDto.Id.Should().Be(ad.Id);

            // Check that created date is not close to current date and updated date is close to current date
            resultDto.CreatedOn.Should().NotBeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
            resultDto.UpdatedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

            // Check changed field
            resultDto.GetType().GetProperty(changedPropertyName).GetValue(resultDto)
                .Should().BeEquivalentTo(changedPropertyValue);


            // Check that previous images were dissociated
            if (ad.PreviewImageSource is not null)
            {
                // Check that the old preview image was deleted from server
                _unitOfWorkStub.Verify(u => u.AdImagesRepository.DeleteFromServerAsync(
                    ad.PreviewImageSource), Times.Once);
                
                // Check that non-preview images were disasociated
                if (ad.NonPreviewImages != null && ad.NonPreviewImages.Any())
                {
                    foreach (AdImage image in ad.NonPreviewImages)
                    {
                        image.AdId.Should().BeNull();
                        _unitOfWorkStub.Verify(
                            u => u.AdImagesRepository.Update(image));
                    }
                }
            }

            // Check that image path was not modified by the controller and that they were added in order
            if (updateAdDto.ImagesIds != null && updateAdDto.ImagesIds.Any())
            {
                // Check that images count in dto is correct
                resultDto.NonPreviewImages.Should().HaveCount(updateAdDto.ImagesIds.Count - 1);

                // Check that preview image starts with a valid prefix
                resultDto.PreviewImage.ToString().Should().StartWith(uriPrefix);

                // Check that preview image is the first image added
                resultDto.PreviewImage.ToString().Should().EndWith("1");

                if (updateAdDto.ImagesIds.Skip(1).Any())
                {
                    using var enumerator = resultDto.NonPreviewImages.GetEnumerator();
                    for (int i = 2; enumerator.MoveNext(); i++)
                    {
                        Uri imageUri = enumerator.Current;

                        // Check prefix
                        imageUri.ToString().Should().StartWith(uriPrefix);

                        // Check order
                        imageUri.ToString().Should().EndWith(i.ToString());
                    }
                }

                // Check that preview image is deleted from db as it's source should only be stored in ad.PreviewImage property
                _unitOfWorkStub.Verify(u => u.AdImagesRepository.Remove(
                    It.Is<AdImage>(i => i.Source.ToString().EndsWith("1"))), Times.Once);
            }

            // Check that the image was never assigned an invalid ad id or an invalid uri prefix
            _unitOfWorkStub.Verify(u => u.AdImagesRepository.Update(
                It.Is<AdImage>(i => (i.AdId != ad.Id && i.AdId != null) || !i.Source.ToString().StartsWith(uriPrefix))), Times.Never);

            // Check that changes were saved once
            _unitOfWorkStub.Verify(u => u.SaveChangesAsync(), Times.Once());
        }


        [Theory]
        [MemberData(nameof(AdsControllerTestsData.InvalidUpdateAdDtos), MemberType = typeof(AdsControllerTestsData))]
        public async Task EditAsync_WithAdAuthorAndInvalidDtos_ReturnsBadRequest(Ad ad, UpdateAdDto updateAdDto)
        {
            // Arrange

            // Setup user claims
            string userNameIdentifier = ad.AuthorId;
            string userName = Guid.NewGuid().ToString();

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(userNameIdentifier, userName);

            _adsRepository.Setup(  // When requesting ad, returns ad from test data
               r => r.GetByIdOrDefaultAsync(
                   ad.Id,
                   It.IsAny<bool>(),
                   It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(ad);

            _unitOfWorkStub.Setup(  // When requesting ad image, returns dummy image with source
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdImage { Source = new Uri(@"http://www.example.com/abc") });

            // Act
            var result = await controller.EditAsync(ad.Id, updateAdDto);

            // Assert
            // Check result type
            result.Result.Should().BeOfType<BadRequestObjectResult>();

            // Check that no images were permanently deleted from server
            _unitOfWorkStub.Verify(
                u => u.AdImagesRepository.DeleteFromServerAsync(It.IsAny<Uri>()), Times.Never);
        }


        [Fact]
        public async Task EditAsync_WithInvalidUser_ReturnsForbidden()
        {
            // Arrange
            UpdateAdDto randomUpdateAdDto = AdsControllerTestsData.CreateRandomUpdateAdDto(0);

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            _adsRepository.Setup(  // When requesting ad, returns empty ad
               r => r.GetByIdOrDefaultAsync(
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(new Ad());

            // Act
            var result = await controller.EditAsync(_rand.Next(), randomUpdateAdDto);

            // Assert
            result.Result.Should().BeOfType<ForbidResult>();
        }
           

        [Fact]
        public async Task EditAsync_WithUnexistingAd_ReturnsNotFound()
        {
            // Arrange
            UpdateAdDto randomUpdateAdDto = AdsControllerTestsData.CreateRandomUpdateAdDto(0);

            _adsRepository.Setup(  // When requesting ad, returns null
               r => r.GetByIdOrDefaultAsync(
                   It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync((Ad?)null);

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object);

            // Act
            var result = await controller.EditAsync(_rand.Next(), randomUpdateAdDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task EditAsync_WithUnexistingImages_ReturnsNotFound()
        {
            // Arrange
            UpdateAdDto updateAdDto = AdsControllerTestsData.CreateRandomUpdateAdDto(2);
            Ad ad = AdsControllerTestsData.CreateRandomAd(0);

            // Setup user claims
            string userNameIdentifier = ad.AuthorId;
            string userName = Guid.NewGuid().ToString();

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(userNameIdentifier, userName);
            
            _adsRepository.Setup(  // When requesting ad, returns random ad
               r => r.GetByIdOrDefaultAsync(
                   ad.Id,
                   It.IsAny<bool>(),
                   It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(ad);
            
            _unitOfWorkStub.Setup(  // When requesting images, returns null
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync((AdImage?)null);

            // Act
            var result = await controller.EditAsync(ad.Id, updateAdDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task EditAsync_WithUnexistingNonPreviewImages_ReturnsNotFound()
        {
            // Arrange
            // Setup random ad and random updateAdDto
            UpdateAdDto updateAdDto = AdsControllerTestsData.CreateRandomUpdateAdDto(2);
            Ad ad = AdsControllerTestsData.CreateRandomAd(0);

            string userNameIdentifier = ad.AuthorId;
            string userName = Guid.NewGuid().ToString();

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(userNameIdentifier, userName);

            _adsRepository.Setup(  // When requesting ad, returns ad from test data
               r => r.GetByIdOrDefaultAsync(
                   ad.Id,
                   It.IsAny<bool>(),
                   It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(ad);

            _unitOfWorkStub.SetupSequence(  // When requesting images, returns dummy image for first request, for next returns null
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdImage { Source = new Uri(@"http://www.example.com/abc") })
                .ReturnsAsync((AdImage?)null);

            // Act
            var result = await controller.EditAsync(ad.Id, updateAdDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }


        [Fact]
        public async Task EditAsync_WithImageStoringServiceNotWorking_ReturnsServiceUnavailable()
        {
            // Arrange
            // Setup random ad and random updateAdDto
            UpdateAdDto updateAdDto = AdsControllerTestsData.CreateRandomUpdateAdDto(2);
            Ad ad = AdsControllerTestsData.CreateRandomAd(4);

            // Setup claimsPrincipal that will be passed to controller
            string userNameIdentifier = ad.AuthorId;
            string userName = Guid.NewGuid().ToString();

            var controller = new AdsController(
                _unitOfWorkStub.Object,
                _loggerStub.Object,
                _userManager.Object)
                .WithIdentity(userNameIdentifier, userName);

            _adsRepository.Setup(  // When requesting ad, returns random ad
               r => r.GetByIdOrDefaultAsync(
                   ad.Id,
                   It.IsAny<bool>(),
                   It.IsAny<Expression<Func<Ad, object>>[]>()))
               .ReturnsAsync(ad);

            _unitOfWorkStub.Setup(  // When requesting ad image, returns dummy image with source
                u => u.AdImagesRepository.GetAsync(It.IsAny<int>()))
                .ReturnsAsync(new AdImage { Source = new Uri(@"http://www.example.com/abc") });

            _unitOfWorkStub.Setup(  // When deleting preview image from server, returns deleted image uri (success)
                u => u.AdImagesRepository.DeleteFromServerAsync(ad.PreviewImageSource))
                .ThrowsAsync(new HttpRequestException());

            // Act
            var result = await controller.EditAsync(ad.Id, updateAdDto);

            // Assert
            result.Result.Should().BeOfType<StatusCodeResult>();

            var statusCodeResult = result.Result as StatusCodeResult;
            statusCodeResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
using CountryhouseService.API.Controllers;
using CountryhouseService.API.Dtos;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using CountryhouseService.Tests.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CountryhouseService.Tests
{
    public class ImagesControllerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<ILogger<ImagesController>> _logger = new();
        private readonly Random _rand = new();
        private readonly ImagesController _controller;

        public ImagesControllerTests()
        {
            _controller = new(_unitOfWork.Object, _logger.Object);
        }


        [Theory]
        [MemberData(nameof(ImagesControllerTestsData.ValidCreateImageDtos), MemberType = typeof(ImagesControllerTestsData))]
        public async Task CreateAdImageAsync_WithValidDto_ReturnsCreatedResult(CreateImageDto createImageDto)
        {
            // Arrange
            Uri randomImageUri = ImagesControllerTestsData.CreateRandomImageUri();
            int randomImageId = _rand.Next();
            ImageDto randomImageDto = new(randomImageId, randomImageUri);

            _unitOfWork.Setup(  // When adding image to db, returns new new random id
                u => u.AdImagesRepository.AddToDbAsync(It.Is<AdImage>(img => img.Source == randomImageUri)))
                .ReturnsAsync(randomImageId);

            _unitOfWork.Setup(  // When uploading image to server, returns new random uri
                u => u.AdImagesRepository.UploadToServerAsync(It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync(randomImageUri);

            // Act
            var result = await _controller.CreateAdImageAsync(createImageDto);

            // Assert
            result.Should().BeOfType<CreatedResult>();

            var createdResult = result as CreatedResult;
            createdResult.Location.Should().BeEquivalentTo(randomImageUri.ToString());
            createdResult.Value.Should().BeEquivalentTo(randomImageDto);
        }


        [Theory]
        [MemberData(nameof(ImagesControllerTestsData.ValidCreateImageDtos), MemberType = typeof(ImagesControllerTestsData))]
        public async Task CreateAvatarAsync_WithValidDto_ReturnsCreatedResult(CreateImageDto createImageDto)
        {
            // Arrange
            Uri randomImageUri = ImagesControllerTestsData.CreateRandomImageUri();
            int randomImageId = _rand.Next();
            ImageDto randomImageDto = new(randomImageId, randomImageUri);

            _unitOfWork.Setup(  // When adding image to db, returns new new random id
                u => u.AvatarsRepository.AddToDbAsync(It.Is<Avatar>(img => img.Source == randomImageUri)))
                .ReturnsAsync(randomImageId);

            _unitOfWork.Setup(  // When uploading image to server, returns new random uri
                u => u.AvatarsRepository.UploadToServerAsync(It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync(randomImageUri);

            // Act
            var result = await _controller.CreateAvatarAsync(createImageDto);

            // Assert
            result.Should().BeOfType<CreatedResult>();

            var createdResult = result as CreatedResult;
            createdResult.Location.Should().BeEquivalentTo(randomImageUri.ToString());
            createdResult.Value.Should().BeEquivalentTo(randomImageDto);
        }
    } 
}

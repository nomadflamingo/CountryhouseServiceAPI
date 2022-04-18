using CountryhouseService.API.Dtos;
using CountryhouseService.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CountryhouseService.Tests.Data
{
    public static class AdsControllerTestsData
    {
        private static readonly Random _rand = new();

        
        public static IEnumerable<object[]> ValidAds =>
            new object[][]
            {
                new object[] { CreateRandomAd(0) },
                new object[] { CreateRandomAd(1) },
                new object[] { CreateRandomAd(2) },
                new object[] { CreateRandomAd(3) },
            };

        
        public static IEnumerable<object[]> ValidAdLists =>
            new object[][]
            {
                new object[]
                {
                    new Ad[]
                    {
                        CreateRandomAd(0),
                        CreateRandomAd(1),
                        CreateRandomAd(1),
                        CreateRandomAd(0),
                        CreateRandomAd(1),
                        CreateRandomAd(1)
                    }
                },
                new object[]
                {
                    new Ad[]
                    {
                        CreateRandomAd(0),
                        CreateRandomAd(1),
                        CreateRandomAd(1),
                    }
                },
                new object[]
                {
                    Array.Empty<Ad>()
                }
            };

        
        public static IEnumerable<object[]> ValidCreateAdDtos =>
            new object[][]
            {
                new object[] { CreateRandomCreateAdDto(0) },
                new object[] { CreateRandomCreateAdDto(1) },
                new object[] { CreateRandomCreateAdDto(2) },
                new object[] { CreateRandomCreateAdDto(5)
                    .WithDates(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2)) }
            };

        public static IEnumerable<object[]> InvalidCreateAdDtos =>
           new object[][]
           {
               // 1. Too many images
                new object[] { CreateRandomCreateAdDto(9) },

                // 2. Too long description
                new object[] { CreateRandomCreateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Description), new string('*', 2000)) },
               
                // 3. Negative budget
                new object[] { CreateRandomCreateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Budget), -1) },

                // 4. Missing required fields
                new object[] { CreateRandomCreateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Address), null) },

                // 5. Empty string
                new object[] { CreateRandomCreateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Title), "") },

                // 6. Space only string
                new object[] { CreateRandomCreateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.ContactNumber), "  ") },

                // 7. Accomplish from date is less than today's date
               new object[] { CreateRandomCreateAdDto(4)
                    .WithDates(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(2)) },

                // 8. Accomplish until date is is less than accomplish from date
                new object[] { CreateRandomCreateAdDto(3)
                    .WithDates(DateTime.Today.AddDays(2), DateTime.Today.AddDays(1)) },

                // 9. Accomplish until date is less than today's date
                new object[] { CreateRandomCreateAdDto(2)
                    .WithDates(null, DateTime.Today.AddDays(-2)) },
           };

        public static IEnumerable<object[]> ValidCreateAdDtosWithImagesIds =>
            new object[][]
            {
                new object[] { CreateRandomCreateAdDto(2) }
            };

        public static IEnumerable<object[]> ValidUpdateAdDtos =>
            new object[][]
            {
                // 1
                new object[] { CreateRandomAd(1), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Title), "Cheese"),
                    nameof(CreateAdDto.Title), "Cheese" },

                // 2
                new object[] { CreateRandomAd(2), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Description), "Cheese"), 
                    nameof(CreateAdDto.Description), "Cheese" },

                // 3
                new object[] { CreateRandomAd(0), CreateRandomUpdateAdDto(1)
                    .WithProperty(nameof(CreateAdDto.Address), "Cheese"),
                    nameof(CreateAdDto.Address), "Cheese" },

                // 4
                new object[] { CreateRandomAd(0), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Budget), 123),
                    nameof(CreateAdDto.Budget), 123 },

                // 5
                new object[] { CreateRandomAd(0), CreateRandomUpdateAdDto(2)
                    .WithProperty(nameof(CreateAdDto.ContactNumber), "123"),
                    nameof(CreateAdDto.ContactNumber), "123" },

                // 6
                new object[] { CreateRandomAd(1), CreateRandomUpdateAdDto(1)
                    .WithProperty(nameof(CreateAdDto.Title), "Cheese"),
                    nameof(CreateAdDto.Title), "Cheese" },

                // 7
                new object[] { CreateRandomAd(2), CreateRandomUpdateAdDto(2)
                    .WithProperty(nameof(CreateAdDto.Description), "Cheese"),
                    nameof(CreateAdDto.Description), "Cheese" },

                // 8
                new object[] { CreateRandomAd(2), CreateRandomUpdateAdDto(5)
                    .WithProperty(nameof(CreateAdDto.Address), "Cheese"),
                    nameof(CreateAdDto.Address), "Cheese" },

                // 9
                new object[] { CreateRandomAd(5), CreateRandomUpdateAdDto(2)
                    .WithProperty(nameof(CreateAdDto.Budget), 123),
                    nameof(CreateAdDto.Budget), 123 }
            };

        public static IEnumerable<object[]> InvalidUpdateAdDtos =>
           new object[][]
           {
               // 1. Too many images
                new object[] { CreateRandomAd(0), CreateRandomUpdateAdDto(9) },

                // 2. Too long description
                new object[] { CreateRandomAd(1), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Description), new string('*', 2000)) },
               
                // 3. Negative budget
                new object[] { CreateRandomAd(0), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Budget), -1) },

                // 4. Missing required fields
                new object[] { CreateRandomAd(2), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Address), null) },

                // 5. Empty string
                new object[] { CreateRandomAd(5), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.Title), "") },

                // 6. Space only string
                new object[] { CreateRandomAd(3), CreateRandomUpdateAdDto(0)
                    .WithProperty(nameof(CreateAdDto.ContactNumber), "  ") },

                // 7. Accomplish from date is less than today's date
               new object[] { CreateRandomAd(3), CreateRandomUpdateAdDto(4)
                    .WithDates(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(2)) },

                // 8. Accomplish until date is is less than accomplish from date
                new object[] { CreateRandomAd(1), CreateRandomUpdateAdDto(1)
                    .WithDates(DateTime.Today.AddDays(2), DateTime.Today.AddDays(1)) },

                // 9. Accomplish until date is less than today's date
                new object[] { CreateRandomAd(2), CreateRandomUpdateAdDto(2)
                    .WithDates(null, DateTime.Today.AddDays(-2)) },
           };


        public static Ad CreateRandomAd(int imagesCount = 0)
        {
            Ad ad = new()
            {
                Id = _rand.Next(1, int.MaxValue),
                Title = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Address = Guid.NewGuid().ToString(),
                Budget = _rand.Next(),
                ContactNumber = _rand.Next().ToString(),
                CreatedOn = DateTime.UtcNow.AddDays(-2),
                UpdatedOn = DateTime.UtcNow.AddDays(-1),
            };

            ad.Author = new User()
            {
                Email = Guid.NewGuid().ToString(),
                FirstName = Guid.NewGuid().ToString(),
                LastName = Guid.NewGuid().ToString()
            };
            ad.AuthorId = Guid.NewGuid().ToString();

            ad.Status = new AdStatus()
            {
                Id = _rand.Next(),
                Name = Guid.NewGuid().ToString()
            };
            ad.StatusId = _rand.Next();

            if (imagesCount > 0)
            {
                ad.PreviewImageSource = ImagesControllerTestsData.CreateRandomImageUri();
                if (imagesCount > 1)
                {
                    AdImage[] adImages = new AdImage[imagesCount - 1];
                    for (int i = 1; i < imagesCount; i++)
                    {
                        adImages[i - 1] = new AdImage() { Source = ImagesControllerTestsData.CreateRandomImageUri() };
                    }
                    ad.NonPreviewImages = adImages;
                }
            }
            return ad;
        }


        /// <summary>
        /// Creates a random CreateAdDto provided a number of random images ids that should be in dto
        /// </summary>
        /// <param name="imagesCount">Number of random images ids to be passed in dto</param>
        public static CreateAdDto CreateRandomCreateAdDto(int imagesCount = 0)
        {
            int[]? imagesIds = null;
            if (imagesCount > 0)
            {
                imagesIds = new int[imagesCount];
                for (int i = 0; i < imagesCount; i++)
                    imagesIds[i] = _rand.Next();
            }

            CreateAdDto dto = new(
                Title: Guid.NewGuid().ToString(),
                Description: Guid.NewGuid().ToString(),
                Budget: _rand.Next(),
                Address: Guid.NewGuid().ToString(),
                ContactNumber: _rand.Next().ToString(),
                ImagesIds: imagesIds);

            return dto;
        }

        /// <summary>
        /// Creates a random UpdateAdDto provided a number of random images ids that should be in dto
        /// </summary>
        /// <param name="imagesCount">Number of random images ids to be passed in dto</param>
        public static UpdateAdDto CreateRandomUpdateAdDto(int imagesCount = 0)
        {
            int[]? imagesIds = null;
            if (imagesCount > 0)
            {
                imagesIds = new int[imagesCount];
                for (int i = 0; i < imagesCount; i++)
                    imagesIds[i] = _rand.Next();
            }

            UpdateAdDto dto = new(
                Title: Guid.NewGuid().ToString(),
                Description: Guid.NewGuid().ToString(),
                Budget: _rand.Next(),
                Address: Guid.NewGuid().ToString(),
                ContactNumber: _rand.Next().ToString(),
                ImagesIds: imagesIds);

            return dto;
        }


        /// <summary>
        /// Adds custom accomplishFromDate and accomplishUntilDate fields to a createAdDto
        /// </summary>
        public static CreateAdDto WithDates(
            this CreateAdDto createAdDto,
            DateTime? accomplishFromDate = null, 
            DateTime? accomplishUntilDate = null)
        {
            return createAdDto with
            {
                AccomplishFromDate = accomplishFromDate,
                AccomplishUntilDate = accomplishUntilDate
            };
        }

        /// <summary>
        /// Adds custom accomplishFromDate and accomplishUntilDate fields to an updateAdDto
        /// </summary>
        public static UpdateAdDto WithDates(
            this UpdateAdDto updateAdDto,
            DateTime? accomplishFromDate = null,
            DateTime? accomplishUntilDate = null)
        {
            return updateAdDto with
            {
                AccomplishFromDate = accomplishFromDate,
                AccomplishUntilDate = accomplishUntilDate
            };
        }


        /// <summary>
        /// Allows you to add custom fields to a createAdDto
        /// </summary>
        public static CreateAdDto WithProperty(
            this CreateAdDto createAdDto,
            string propertyName,
            object? propertyValue)
        {
            createAdDto.GetType().GetProperty(propertyName).SetValue(createAdDto, propertyValue);
            return createAdDto;
        }


        /// <summary>
        /// Allows you to add custom fields to an updateAdDto
        /// </summary>
        public static UpdateAdDto WithProperty(
            this UpdateAdDto updateAdDto,
            string propertyName,
            object? propertyValue)
        {
            updateAdDto.GetType().GetProperty(propertyName).SetValue(updateAdDto, propertyValue);
            return updateAdDto;
        }

        
    }
}

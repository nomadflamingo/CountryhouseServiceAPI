using CountryhouseService.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryhouseService.Tests.Data
{
    public static class ImagesControllerTestsData
    {
        public static IEnumerable<object[]> ValidCreateImageDtos =>
            new object[][]
            {
                new object[] { new CreateImageDto(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()) },
                new object[] { new CreateImageDto(Guid.NewGuid().ToString(), null) }
            };


        public static IEnumerable<object[]> InvalidCreateImageDtos =>
            new object[][]
            {
                new object[] { new CreateImageDto(null, null) }
            };


        public static Uri CreateRandomImageUri()
        {
            string uriPrefix = @"http://www.example.com/";
            string uriPostfix = @".jpg";
            return new Uri($"{uriPrefix}{Guid.NewGuid()}{uriPostfix}");
        }
    }
}

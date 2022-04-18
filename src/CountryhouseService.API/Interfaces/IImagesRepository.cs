using CountryhouseService.API.Models;

namespace CountryhouseService.API.Interfaces
{
    public interface IImagesRepository<TImage> where TImage : Image
    {
        Task<TImage?> GetAsync(int id);

        /// <summary>
        /// Uploads image to a remote server. Throws HttpRequestException when unable to upload
        /// (Note that this happens instantly and doesn't wait until unit of work saves changes)
        /// </summary>
        /// <param name="base64">A base64 image representation</param>
        /// <param name="name">An image name. Is null if not specified</param>
        /// <returns>The url of deleted resource</returns>
        /// <exception cref="HttpRequestException"></exception>
        Task<Uri> UploadToServerAsync(string base64, string? name);

        /// <summary>
        /// Deletes image from a remote server.
        /// Throws HttpRequestException when unable to upload
        /// (Note that this happens instantly and doesn't wait until unit of work saves changes)
        /// (There is no need to explicitly call this method for 
        /// images stored in database, since unassigned images
        /// get automatically deleted after a certain
        /// period of time.)
        /// </summary>
        /// <param name="url">A URI that identifies an image on a server</param>
        /// <returns>The url of deleted resource or null if the server responded with an error status code</returns>
        /// <exception cref="HttpRequestException"></exception>
        Task<Uri> DeleteFromServerAsync(Uri url);

        /// <summary>
        /// Adds an image object to a database
        /// </summary>
        /// <param name="image">An image object</param>
        /// <returns>Id of a newly added image or null if server responded with an error status code</returns>
        Task<int> AddToDbAsync(TImage image);

        void Update(TImage image);
        
        void Remove(TImage image);
    }
}

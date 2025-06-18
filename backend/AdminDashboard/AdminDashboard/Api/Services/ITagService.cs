using Api.Models;

namespace Api.Services
{
    public interface ITagService
    {
        Task<List<Tag>> GetTags();
        Task<Tag> GetTag(int id);
        Task<Tag> CreateTag(Tag tag);
        Task<Tag> UpdateTag(int id, Tag tag);
        Task DeleteTag(int id);
        Task UpdateClientTags(int clientId, int[] tagIds);
    }
}

using BackendSite.Service.BLL.Interface;
using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class TagBLL : ITagBLL
    {
        private readonly ITagService _tagService;

        public TagBLL(ITagService tagService)
        {
            _tagService = tagService;
        }

        public async Task<IEnumerable<Tag>> GetAllTag()
        {
            var data = await _tagService.GetAllTag();
            var tags = data.Select(a => new Tag()
            {
                Id = a.Id,
                TagName = a.TagName,
                TagColour = a.TagColour,
                Description = a.Description
            });
            return tags;
        }
    }
}
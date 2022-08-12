using BackendSite.Service.DAL.Interface;
using BackendSite.Service.Model;

namespace BackendSite.Service.BLL
{
    public class SocialNetworkBLL
    {
        private readonly ILogger<SocialNetworkBLL> _logger;
        private readonly ISocialNetworkService _socialNetworkService;

        public SocialNetworkBLL(ISocialNetworkService socialNetworkService, ILogger<SocialNetworkBLL> logger)
        {
            this._socialNetworkService = socialNetworkService;
            this._logger = logger;
        }

        public async Task<Dictionary<string, List<SocialDataListResponse>>> GetSocialDataList(SocialDataListRequest request)
        {
            var dbData = await _socialNetworkService.GetSocialDataList(request);
            var responseData = dbData.Select(r => new SocialDataListResponse()
            {
                LangId = r.LangId,
                Sort = r.Sort,
                Id = r.Id,
                ChannelName = r.ChannelName,
                Title = r.Title,
                MainPageStatus = r.MainPageStatus,
                ContactPageStatus = r.ContactPageStatus,
                FooterStatus = r.FooterStatus,
                AppPageStatus = r.AppPageStatus,
                Link = r.Link,
                AndroidLink = r.AndroidLink,
                IosLink = r.IosLink,
                LinkType = LinkTypeConvertToBoolean(r.LinkType),
                LastUpdateUser = r.LastUpdateUser,
                LastUpdateTime = ParseDateTime(r.LastUpdateTime)
            });

            return responseData.GroupBy(x => x.LangId).ToDictionary(item => item.Key, item => item.OrderBy(r => r.Sort).ToList());
        }

        public async Task SetSocialSort(SetSocialSortRequest request, string lastUpdateUser)
        {
            var updateDataList = request.SortIds.Select((id, index) => new SetSocialSortDto
            {
                SiteId = request.SiteId,
                CurrencyId = request.CurrencyId,
                LangId = request.LangId,
                Id = id,
                Sort = index + 1,
                LastUpdateUser = lastUpdateUser
            }).ToList();

            var tasks = new List<Task<int>>();
            try
            {
                tasks.AddRange(updateDataList.Select(updateItem => Task.Run(() => _socialNetworkService.SetSocialSort(updateItem))));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw ex;
            }
        }

        public async Task SetSocialData(SetSocialDataRequest request, string lastUpdateUser)
        {
            request.LinkType = (byte)LinkTypeConvertToByte(request.FrontEndLikeType);
            await _socialNetworkService.SetSocialData(request, lastUpdateUser);
        }

        private string ParseDateTime(DateTime dateTime)
        {
            return $"{dateTime:yyyy-MM-ddTHH:mm:ss}+00:00";
        }

        /// <summary>
        /// PM希望 1:Url連結, 2:QRCode 轉換成 Url:false ，QRCode:true
        /// </summary>
        /// <param name="linkType"></param>
        /// <returns></returns>
        private bool LinkTypeConvertToBoolean(byte linkType)
        {
            return linkType == 1 ? false : true;
        }

        /// <summary>
        /// true：QRCode，false:Url連結
        /// </summary>
        /// <param name="linkType"></param>
        /// <returns></returns>
        private int LinkTypeConvertToByte(bool linkType)
        {
            return linkType ? 2 : 1;
        }
    }
}

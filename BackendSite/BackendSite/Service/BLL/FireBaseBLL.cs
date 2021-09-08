using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class FireBaseBLL
    {
        private readonly FireBaseService fireBaseService;

        public FireBaseBLL(FireBaseService fireBaseService)
        {
            this.fireBaseService = fireBaseService;
        }
        public string GetContentByLang(string Lang, Dictionary<string, string> multiLangContent)
        {
            if (!multiLangContent.TryGetValue(Lang, out var Content))
            {
                if (!multiLangContent.TryGetValue("en", out Content))
                {
                    Content = multiLangContent.Values.First();
                }
            }
            return Content;

        }
        public async Task<ApiResult<IEnumerable<FireBaseReturn>>> SendFireBaseMessageAsync(int fireBaseId)
        {
            var fireBaseMessageList = fireBaseService.GetFireBaseMessageList(fireBaseId);
            var fireBaseMessageCustList = new List<FireBaseMessage>();
            var result = new ApiResult<IEnumerable<FireBaseReturn>>();
            foreach (var fireBaseItem in fireBaseMessageList)
            {
                if(fireBaseItem.SendType == 0) //site群發
                {
                    var custList = fireBaseService.GetFireBaseCustList(fireBaseItem.SiteId,0);
                    foreach (var custInfo in custList)
                    {
                        fireBaseMessageCustList.Add(
                            new FireBaseMessage {
                                FireBaseId = fireBaseItem.FirebaseId,
                                CustId = custInfo.CustId,
                                SiteName = custInfo.SiteName,
                                Token = custInfo.FirebaseToken,
                                Title = GetContentByLang(custInfo.UserLang, fireBaseItem.IFirebaseTitle),
                                Body = GetContentByLang(custInfo.UserLang, fireBaseItem.IFirebaseBody),
                                Status = fireBaseItem.SendStatus

                            }
                        );

                    }
                }
                else
                {
                    foreach (var custId in fireBaseItem.ICustList)
                    {
                        var custInfo = fireBaseService.GetFireBaseCustList(fireBaseItem.SiteId, custId).First();
                        fireBaseMessageCustList.Add(
                            new FireBaseMessage
                            {
                                FireBaseId = fireBaseItem.FirebaseId,
                                CustId = custInfo.CustId,
                                SiteName = custInfo.SiteName,
                                Token = custInfo.FirebaseToken,
                                Title = GetContentByLang(custInfo.UserLang, fireBaseItem.IFirebaseTitle),
                                Body = GetContentByLang(custInfo.UserLang, fireBaseItem.IFirebaseBody),
                                Status = fireBaseItem.SendStatus
                            }
                        );
                    }
                }
            }
            if (fireBaseMessageCustList.Any())
            {
                result = await fireBaseService.SendFireBaseNotificationAsync(fireBaseMessageCustList);
            }

            if (result.Data.Any())
            {
                foreach (var item in result.Data)
                {
                    fireBaseService.UpdateFireBaseSendStatus(item.FireBaseId, item.Status);
                }
            }
            return result;
        }

    }
}

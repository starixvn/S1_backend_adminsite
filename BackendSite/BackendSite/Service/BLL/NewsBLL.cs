using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.BLL
{
    public class NewsBLL
    {
        private readonly NewsServices newsServices;

        public NewsBLL(NewsServices newsServices)
        {
            this.newsServices = newsServices;
        }

        public IEnumerable<News> GetNewsList()
        {
            return newsServices.GetNewsList();
        }

        public News GetNewsById(int NewsId)
        {
            return newsServices.GetNewsById(NewsId);
        }

        public int? SetNews(bool IsAdd, News newsDatas)
        {
            return newsServices.SetNews(IsAdd, newsDatas);
        }

        public void SetStatus(int NewsId, int Status, string LastUpdateUser)
        {
            newsServices.SetStatus(NewsId, Status, LastUpdateUser);
        }
    }
}

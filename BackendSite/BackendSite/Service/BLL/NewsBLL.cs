using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;

namespace BackendSite.Service.BLL
{
    public class NewsBLL
    {
        private readonly NewsServices newsServices;

        public NewsBLL(NewsServices newsServices)
        {
            this.newsServices = newsServices;
        }

        public NewsData GetNewsList(IEnumerable<int> selectSite, DateTime startDate, DateTime endDate, int pageSize, int pageNumber, int status)
        {
            return newsServices.GetNewsList(selectSite, startDate, endDate, pageSize, pageNumber, status);
        }

        public News GetNewsById(int newsId)
        {
            return newsServices.GetNewsById(newsId);
        }

        public int? SetNews(bool isAdd, News newsDatas)
        {
            return newsServices.SetNews(isAdd, newsDatas);
        }

        public void SetStatus(int newsId, int status, string lastUpdateUser)
        {
            newsServices.SetStatus(newsId, status, lastUpdateUser);
        }
    }
}

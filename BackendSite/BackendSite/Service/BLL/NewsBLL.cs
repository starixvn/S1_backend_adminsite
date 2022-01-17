using System;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BackendSite.Service.BLL
{
    public class NewsBLL
    {
        private readonly NewsServices newsServices;

        public NewsBLL(NewsServices newsServices)
        {
            this.newsServices = newsServices;
        }

        public NewsData GetNewsList(IEnumerable<int> selectSite, DateTime startDate, DateTime endDate, int pageSize, int pageNumber, IEnumerable<int> status)
        {
            return newsServices.GetNewsList(JsonConvert.SerializeObject(selectSite), startDate,
                endDate, JsonConvert.SerializeObject(status),
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize), Math.Abs(pageSize));
        }

        public News GetNewsById(int newsId)
        {
            return newsServices.GetNewsById(newsId);
        }

        public int? SetNews(bool isAdd, News newsDatas)
        {
            if (newsDatas.PostTime != null)
            {
                newsDatas.UnixPostTime = DateTimeOffset.Parse(newsDatas.PostTime.ToString()).ToUnixTimeSeconds();
            }

            return newsServices.SetNews(isAdd, newsDatas);
        }

        public void SetStatus(int newsId, int status, string lastUpdateUser)
        {
            newsServices.SetStatus(newsId, status, lastUpdateUser);
        }
    }
}

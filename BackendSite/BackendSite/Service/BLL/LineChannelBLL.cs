using BackendSite.ExceptionHandling;
using BackendSite.Service.DAL;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BackendSite.Service.BLL
{
    public class LineChannelBLL
    {
        private readonly LineChannelService _lineChannelService;

        public LineChannelBLL(IServiceProvider provider)
        {
            _lineChannelService = provider.GetRequiredService<LineChannelService>();
        }

        public IEnumerable<LineChannelModel> GetList(int siteId, int pageNumber, int pageSize,
            out int totalPages, out int rowCount)
        {
            var datas = _lineChannelService.GetList(true, 0, siteId,
                (Math.Abs(pageNumber) - 1) * Math.Abs(pageSize),
                Math.Abs(pageSize), out totalPages, out rowCount);

            return datas.Select(a => new LineChannelModel(a));
        }

        public LineChannelModel GetById(int id)
        {
            var result = _lineChannelService.GetList(false, id, 0,
                0, 10, out _, out _);

            if (result.Count() == 0)
                throw new AdminException(ErrorCodeEnum.NoData);

            if (result.Count() != 1)
                throw new AdminException(ErrorCodeEnum.DuplicateData);

            var data = result.FirstOrDefault();

            return new LineChannelModel(data);
        }

        public bool Set(bool isAdd, LineChannelDto newsDatas)
        {
            var result = _lineChannelService.Set(newsDatas);

            if (isAdd && result != 1)
                //Insert Affect Row Should be 1;
                throw new AdminException(ErrorCodeEnum.UpdateFailed);

            if (!isAdd && result != 2)
                //Update Affect Row Should be 2;
                throw new AdminException(ErrorCodeEnum.UpdateFailed);

            return true;
        }

    }
}

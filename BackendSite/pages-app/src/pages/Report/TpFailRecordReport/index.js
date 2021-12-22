import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const pageSize = 10;
  const timeZone = 8;
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    FromTransTime: moment().format("l"),
    ToTransTime: moment().format("l"),
    SiteId: 0,
    TpId: 0,
    UserName: '',
    TransId: ''
  });

  const [stateDatas, setDatas] = useState({
    Datas: null,
    FilterInfo: null,
    SiteList: null
  });

  const [siteList, setSiteList] = useState();
  const [SubmitStatus, setSubmitStatus] = useState(false);
  const [ExportStatus, setExportStatus] = useState(false);

  useEffect(() => {
    let param = {
      Url: "api/Common/AdminUserSiteList",
    };

    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      }
      else {
        alert(res.data.message ?? "failed to get site list.");
      }
    });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Filter_Submit = () => {
    if (!CheckTransTime())
      return false;

    let param = {
      Url: 'apiV2/TpFailRecordReport/GetList',
      Data: {
        SiteId:stateFilter.SiteId,
        TpId:stateFilter.TpId,
        FromTransTime: moment(stateFilter.FromTransTime).unix(),
        ToTransTime: moment(stateFilter.ToTransTime).add(1, 'days').unix() - 1,
        UserName: stateFilter.UserName,
        TransId: stateFilter.TransId,
        PageSize: pageSize,
        PageNumber: 1
      }
    };

    setSubmitStatus(true);
    setExportStatus(true);

    WebAPI.PostData(param).then((res) => {
      setSubmitStatus(false);
      setExportStatus(false);
      if (res.data.errorCode === 0) {
        setDatas({
          PageInfo: {
            totalPages: res.data.totalPages,
            pageNumber: 1,
            rowCount: res.data.rowCount,
          },
          Datas: [...res.data.data],
          FilterInfo: {
            ...stateFilter,
          },
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const ChangePage = (PageNumber) => {
    let param = {
      Url: 'apiV2/TpFailRecordReport/GetList',
      Data: {
        SiteId:stateFilter.SiteId,
        TpId:stateFilter.TpId,
        FromTransTime: moment(stateFilter.FromTransTime).unix(),
        ToTransTime: moment(stateFilter.ToTransTime).add(1, 'days').unix() - 1,
        UserName: stateFilter.UserName,
        TransId: stateFilter.TransId,
        PageSize: pageSize,
        PageNumber: PageNumber
      }
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setDatas({
          PageInfo: {
            totalPages: res.data.totalPages,
            pageNumber: PageNumber,
            rowCount: res.data.rowCount,
          },
          Datas: [...res.data.data],
          FilterInfo: {
            ...stateFilter,
          },
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const ExportToExcel = () => {
    if (!CheckTransTime())
      return false;

    setSubmitStatus(true);
    setExportStatus(true);

    let param = {
      Url: 'apiV2/TpFailRecordReport/Export',
      Data: {
        SiteId: stateFilter.SiteId,
        TpId: stateFilter.TpId,
        FromTransTime: moment(stateFilter.FromTransTime).unix(),
        ToTransTime: moment(stateFilter.ToTransTime).add(1, 'days').unix() - 1,
        UserName: stateFilter.UserName,
        TransId: stateFilter.TransId,
        TimeZone: timeZone
      }
    };

    WebAPI.DownloadData(param);

    setTimeout(() => {
      setSubmitStatus(false);
      setExportStatus(false);
    }, 5000);
  };

  
  const CheckTransTime = () => {
    let momFromTransTime = moment(stateFilter.FromTransTime);
    let momToTransTime = moment(stateFilter.FromTransTime)

    if (!momFromTransTime.unix() || !momToTransTime.unix()) {
      alert("Please choose TransTime of report.");
        return false;
    }

    if (Math.abs(momFromTransTime.diff(moment(), "days")) > 30) {
      alert("Only the data of the past 30 days can be queried.");
        return false;
    }

    return true;
  };

  return (
    <div className="container-fluid" style={{ width: "auto" }}>
      <h4 className="page-name">Bet Failure Record</h4>
      <Filter
        filterInfo={stateFilter}
        onChange={Filter_OnChange}

        ClickSubmit={Filter_Submit}
        SiteList={siteList}
        SubmitBtnDisabled={SubmitStatus}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          ExportToExcel={ExportToExcel}
          ExportBtnDisabled={ExportStatus}
          ChangePage={ChangePage}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

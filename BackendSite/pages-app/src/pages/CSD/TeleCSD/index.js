import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";

const App = () => {
  moment.tz.setDefault("Asia/Taipei");
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SiteIds: [],
    Currency: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Deposit: 0,
    CallOut: 0,
    DormantUser: 3,
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [Data, setData] = useState(null);
  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const Submit = () => {
    if (stateFilter.SiteIds.length === 0 || stateFilter.Currency.length === 0) {
      alert("Please Choose at least one Site and Currency");
      return false;
    }

    const siteIds = [];
    stateFilter.SiteIds.map((o) => {
      siteIds.push(o.value);
    });

    const currencyIds = [];
    stateFilter.Currency.map((o) => {
      currencyIds.push(Number(o.value));
    });

    let param = {
      Url: "CustManage/GetTeleCSD",
      Data: {
        SiteIds: siteIds,
        Currency: currencyIds,
        Deposit: Number(stateFilter.Deposit),
        CallOut: Number(stateFilter.CallOut),
        DormantUser: Number(stateFilter.DormantUser),
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).unix(),
        PageNumber: 1,
      },
    };

    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };
  const Complete_once = (prop) => {
    let Data = {
      CustId: prop.custId,
      SiteId: prop.siteId,
    };
    api
      .post("/api/CustManage/UpdateCustomerTele", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          UpdateReset();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const UpdateReset = () => {
    const siteIds = [];
    stateDatas.FilterInfo.SiteIds.map((o) => {
      siteIds.push(o.value);
    });

    const currencyIds = [];
    stateDatas.FilterInfo.Currency.map((o) => {
      currencyIds.push(Number(o.value));
    });

    let param = {
      Url: "CustManage/GetTeleCSD",
      Data: {
        SiteIds: siteIds,
        Currency: currencyIds,
        Deposit: Number(stateDatas.FilterInfo.Deposit),
        CallOut: Number(stateDatas.FilterInfo.CallOut),
        DormantUser: Number(stateDatas.FilterInfo.DormantUser),
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
        PageNumber: stateDatas.PageInfo.pageNumber,
      },
    };

    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: stateDatas.PageInfo.pageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Result_ChangePage = (PageNumber) => {
    if (
      stateDatas.FilterInfo.SiteIds.length === 0 ||
      stateDatas.FilterInfo.Currency.length === 0
    ) {
      alert("Please Choose at least one Site and Currency");
      return false;
    }

    const siteIds = [];
    stateFilter.SiteIds.map((o) => {
      siteIds.push(o.value);
    });

    const currencyIds = [];
    stateFilter.Currency.map((o) => {
      currencyIds.push(Number(o.value));
    });

    let param = {
      Url: "CustManage/GetTeleCSD",
      Data: {
        SiteIds: siteIds,
        Currency: currencyIds,
        Deposit: Number(stateFilter.Deposit),
        CallOut: Number(stateFilter.CallOut),
        DormantUser: Number(stateFilter.DormantUser),
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).unix(),
        PageNumber: PageNumber,
      },
    };

    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Result_ExportToExcel = () => {
    const siteIds = [];
    stateFilter.SiteIds.map((o) => {
      siteIds.push(o.value);
    });

    const currencyIds = [];
    stateFilter.Currency.map((o) => {
      currencyIds.push(Number(o.value));
    });

    let param = {
      Url: "CustManage/ExportTeleCSDData",
      Data: {
        SiteIds: JSON.stringify(siteIds),
        Currency: JSON.stringify(currencyIds),
        Deposit: Number(stateDatas.FilterInfo.Deposit),
        CallOut: Number(stateDatas.FilterInfo.CallOut),
        DormantUser: Number(stateDatas.FilterInfo.DormantUser),
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
      },
    };

    WebAPI.DownloadData(param);
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">TeleCSD</h4>
      <Filter
        SiteList={siteList}
        StateFilter={stateFilter}
        SetFilter={setFilter}
        Submit={Submit}
        onChange={Filter_OnChange}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          Complete={Complete_once}
          ChangePage={Result_ChangePage}
          ExportToExcel={Result_ExportToExcel}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";

const App = () => {
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SiteIds: [],
    Currency: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Deposit: "0",
    CallOut: "0",
    DormantUser: "3",
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
    let Data = {
      SiteIds: JSON.stringify(stateFilter.SiteIds),
      Currency: JSON.stringify(stateFilter.Currency),
      Deposit: stateFilter.Deposit,
      CallOut: stateFilter.CallOut,
      DormantUser: stateFilter.DormantUser,
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).unix(),
      PageNumber: 1,
    };

    api
      .post("/api/CustManage/GetTeleCSD", Data)
      .then(function (res) {
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
      SiteId: prop.siteId
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
  const UpdateReset = ()=>{
    let Data = {
      SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
      Currency: JSON.stringify(stateDatas.FilterInfo.Currency),
      Deposit: stateDatas.FilterInfo.Deposit,
      CallOut: stateDatas.FilterInfo.CallOut,
      DormantUser: stateDatas.FilterInfo.DormantUser,
      StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
      EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
      PageNumber: stateDatas.PageInfo.pageNumber,
    };
    api
      .post("/api/CustManage/GetTeleCSD", Data)
      .then(function (res) {
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
  }
  const Result_ChangePage = (PageNumber) => {
    if (
      stateDatas.FilterInfo.SiteIds.length === 0 ||
      stateDatas.FilterInfo.Currency.length === 0
    ) {
      alert("Please Choose at least one Site and Currency");
      return false;
    }
    let Data = {
      SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
      Currency: JSON.stringify(stateDatas.FilterInfo.Currency),
      Deposit: stateDatas.FilterInfo.Deposit,
      CallOut: stateDatas.FilterInfo.CallOut,
      DormantUser: stateDatas.FilterInfo.DormantUser,
      StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
      EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
      PageNumber: PageNumber,
    };
    api
      .post("/api/CustManage/GetTeleCSD", Data)
      .then(function (res) {
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
    let param = {
      Url: "CustManage/ExportTeleCSDData",
      Data: {
        SiteIds: JSON.stringify(stateDatas.FilterInfo.SiteIds),
        Currency: JSON.stringify(stateDatas.FilterInfo.Currency),
        Deposit: stateDatas.FilterInfo.Deposit,
        CallOut: stateDatas.FilterInfo.CallOut,
        DormantUser: stateDatas.FilterInfo.DormantUser,
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

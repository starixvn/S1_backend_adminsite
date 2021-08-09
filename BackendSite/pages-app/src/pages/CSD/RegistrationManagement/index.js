import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Note from "./Note";

const App = () => {
  const [siteList, setSiteList] = useState();
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    Currency: 0,
    DepositStatus: 0,
    Status: 0,
    User: "",
    RegTimeType: 1,
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
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
  const Filter_CheckBox_OnChange = (v, dataColumn) => {
    let isRemove = stateFilter[dataColumn].includes(v);
    let newChecked = isRemove
      ? stateFilter[dataColumn].filter((value) => value !== v)
      : stateFilter[dataColumn].push(v);
    let currentTab = stateFilter[dataColumn];
    if (newChecked.length === 0) {
      currentTab = [];
    } else if (currentTab === null) {
      currentTab = newChecked;
    }

    if (isRemove && !newChecked.includes(currentTab)) {
      currentTab = newChecked.length === 0 ? [] : newChecked;
    }
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
    if (v !== "All") return false;
    let currentTab =
      stateFilter[dataColumn].length === options.length
        ? []
        : options.map((o) => {
            return o.value;
          });
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: stateFilter.SelectSite,
        Currency: Number(stateFilter.Currency),
        DepositStatus: Number(stateFilter.DepositStatus),
        Status: Number(stateFilter.Status),
        User: stateFilter.User,
        Sort: Number(stateFilter.RegTimeType),
        PageSize: 30,
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

  const Result_ChangePage = (PageNumber) => {
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: stateDatas.FilterInfo.SelectSite,
        Currency: Number(stateDatas.FilterInfo.Currency),
        DepositStatus: Number(stateDatas.FilterInfo.DepositStatus),
        Status: Number(stateDatas.FilterInfo.Status),
        User: stateDatas.FilterInfo.User,
        Sort: Number(stateDatas.FilterInfo.RegTimeType),
        PageSize: 30,
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
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const UpdateStatus = (status, custid, siteid) => {
    let Data = {
      CustId: Number(custid),
      SiteId: Number(siteid),
      Status: Number(status),
    };
    api
      .post("/api/CustManage/SetRegLiveStatus", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          UpdateReset();
        } else {
          alert(res.data.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const UpdateReset = () => {
    let param = {
      Url: "CustManage/GetRegLiveStatusData",
      Data: {
        SiteIds: stateDatas.FilterInfo.SelectSite,
        Currency: Number(stateDatas.FilterInfo.Currency),
        DepositStatus: Number(stateDatas.FilterInfo.DepositStatus),
        Status: Number(stateDatas.FilterInfo.Status),
        User: stateDatas.FilterInfo.User,
        Sort: Number(stateDatas.FilterInfo.RegTimeType),
        PageSize: 30,
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
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const EditNote = (custid) => {
    let param = {
      Url: "CustManage/GetRegLiveStatusNotes",
      Data: {
        CustId: Number(custid),
      },
    };
    WebAPI.PostData(param)
      .then((res) => {
        if (res.data.errorCode === 0) {
          console.log('ok'); 
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Registration Management</h4>
      <Filter
        SiteList={siteList}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        ClickSubmit={Submit}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          UpdateStatus={UpdateStatus}
          ChangePage={Result_ChangePage}
          EditNote={EditNote}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

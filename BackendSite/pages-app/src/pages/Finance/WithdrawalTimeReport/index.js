import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import qs from "qs";
import Filter from "./Filter";
import { api } from "../../../module/WebAPI";
import Result from "./Result";

const App = () => {
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Currency: "0",
    WithdrawalID: "",
    Status: "0",
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
    console.log(v, dataColumn);
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }

    let Data = {
        SelectSite: stateFilter.SelectSite,
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
        WithdrawalID: stateFilter.WithdrawalID,
        CurrencyId: parseInt(stateFilter.Currency, 10),
        Status: parseInt(stateFilter.Status, 10),
        PageNumber: 1,
        PageSize: 10,
      };
    api
      .post("/api/Withdrawal/WithdrawalTimeReportGet", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
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
    let Data = {
      SelectSite: stateDatas.FilterInfo.SelectSite,
      StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
      EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
      WithdrawalID: stateDatas.FilterInfo.WithdrawalID,
      CurrencyId: parseInt(stateDatas.FilterInfo.Currency, 10),
      Status: parseInt(stateDatas.FilterInfo.Status, 10),
      PageNumber: PageNumber,
      PageSize: 10,
    };
    api
      .post("/api/Withdrawal/WithdrawalTimeReportGet", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
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

  return (
    <div className="container-fluid">
      <h4 className="page-name">Withdrawal Time Report</h4>
      <Filter
        SiteList={siteList}
        filterInfo={stateFilter}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        onChange={Filter_OnChange}
        ClickSubmit={Filter_Submit}
      />
      {stateDatas.Datas === null ? null : (
        <Result {...stateDatas} ChangePage={Result_ChangePage} />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

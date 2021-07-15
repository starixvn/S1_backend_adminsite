import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import { api } from "../../../module/WebAPI";

const App = () => {
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Currency: "0",
    User: "",
    Category: "0",
    BaseCurrency: 0,
    DateQuickPicker:"0"
  });

  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

  const [siteList, setSiteList] = useState();
  const [CategoryList, setCategoryList] = useState(null);

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

  useEffect(() => {
    api.post("/api/Payment/GetTPCategoryListByVipRebate").then((res) => {
      if (res.data.errorCode === 0) {
        setCategoryList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const DatePicker_OnChange = (props) => {
    let _startDate = "";
    let _endDate = "";

    switch (props) {
      case "1":
        _startDate = moment().startOf("day");
        _endDate = moment().endOf("day");

        break;
      case "2":
        _startDate = moment().subtract(1, "days");
        _endDate = moment().subtract(1, "days");

        break;
      case "3":
        _startDate = moment().startOf("week");
        _endDate = moment().endOf("week");

        break;
      case "4":
        _startDate = moment().subtract(1, "week").startOf("week");
        _endDate = moment().subtract(1, "week").endOf("week");

        break;
      case "5":
        _startDate = moment().startOf("month");
        _endDate = moment().endOf("month");

        break;
      case "6":
        _startDate = moment().subtract(1, "month").startOf("month");
        _endDate = moment().subtract(1, "month").endOf("month");

        break;
      default:
        break;
    }
    if (props !== "0") {
      setFilter({
        ...stateFilter,
        ["StartDate"]: _startDate,
        ["EndDate"]: _endDate,
        ["DateQuickPicker"]:props
      });
    }else{
      setFilter({
        ...stateFilter,
        ["DateQuickPicker"]:props
      });
    }
  };

  const Filter_OnChange = (v, dataColumn) => {
    if (dataColumn === "StartDate" || dataColumn === "EndDate") {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
        ["DateQuickPicker"]:"0"
      });
    } else {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
      });
    }
  };

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
  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      Currency: parseInt(stateFilter.Currency),
      CategoryId: parseInt(stateFilter.Category),
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).unix(),
      User: stateFilter.User,
      PageNumber: 1,
      PageSize: 10,
      BaseCurrencyId: parseInt(stateFilter.BaseCurrency, 10),
    };
    api
      .post("/api/Payment/GetVipRebateInfoList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
              currencyAmount: res.data.currencyAmount
            },
            Datas: [...res.data.data],
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
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateDatas.FilterInfo.SelectSite,
      Currency: parseInt(stateDatas.FilterInfo.Currency),
      CategoryId: parseInt(stateDatas.FilterInfo.Category),
      StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
      EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
      User: stateDatas.FilterInfo.User,
      PageNumber: PageNumber,
      PageSize: 10,
      BaseCurrencyId: parseInt(stateDatas.FilterInfo.BaseCurrency, 10),
    };

    api
      .post("/api/Payment/GetVipRebateInfoList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            PageInfo: {
              ...stateDatas.PageInfo,
              pageNumber: PageNumber,
            },
            Datas: [...res.data.data],
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
      Url: "Payment/GetVipRebateInfoListExport",
      Data: {
        SelectSite: JSON.stringify(stateDatas.FilterInfo.SelectSite),
        Currency: parseInt(stateDatas.FilterInfo.Currency),
        CategoryId: parseInt(stateDatas.FilterInfo.Category),
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate: moment(stateDatas.FilterInfo.EndDate).unix(),
        User: stateDatas.FilterInfo.User,
        BaseCurrencyId: parseInt(stateDatas.FilterInfo.BaseCurrency, 10),
      },
    };

    WebAPI.DownloadData(param);
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Vip Rebate Report</h4>
      <Filter
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        ClickSubmit={Filter_Submit}
        SiteList={siteList}
        CategoryList={CategoryList}
        datePickeronChange={DatePicker_OnChange}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
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

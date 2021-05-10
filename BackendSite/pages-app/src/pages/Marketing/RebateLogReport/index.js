import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";

const App = () => {
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    Type: 0,
    User: "",
    Currency: 0,
    BaseCurrency: 2,
    TotalAmountShow: 0.0,
    RebateCategories: 0
  });

  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

  const [siteList, setSiteList] = useState();

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

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
        ...stateFilter,
        [dataColumn]: v,
      });
  };
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
    let param = {
      Url: "Payment/GetRebateLog",
      Data: qs.stringify({
        SelectSite: stateFilter.SelectSite,
        RebateType: stateFilter.Type,
        StartDate: moment(stateFilter.StartDate).unix(),
        EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
        UserName: stateFilter.User,
        PageNumber: 1,
        CurrencyId: parseInt(stateFilter.Currency, 10),
        BaseCurrencyId: parseInt(stateFilter.BaseCurrency, 10),
        TotalAmountShow: parseFloat(stateFilter.TotalAmountShow),
        RebateCategoriesId: parseInt(stateFilter.RebateCategories, 10),
      }),
    };

    WebAPI.PostData(param).then((res) => {
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

  const Result_ChangePage = (PageNumber) => {
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let param = {
      Url: "Payment/GetRebateLog",
      Data: qs.stringify({
        SelectSite: stateDatas.FilterInfo.SelectSite,
        RebateType: stateDatas.FilterInfo.Type,
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate:
          moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
        UserName: stateDatas.FilterInfo.User,
        PageNumber: PageNumber,
        CurrencyId: parseInt(stateDatas.FilterInfo.Currency, 10),
        BaseCurrencyId: parseInt(stateDatas.FilterInfo.BaseCurrency, 10),
        TotalAmountShow: parseFloat(stateDatas.FilterInfo.TotalAmountShow),
        RebateCategoriesId: parseInt(
          stateDatas.FilterInfo.RebateCategories,
          10
        ),
      }),
    };

    WebAPI.PostData(param).then((res) => {
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
    });
  };

  const Result_ExportToExcel = () => {
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let param = {
      Url: "Payment/ExportRebateLog",
      Data: {
        SelectSite: JSON.stringify(stateDatas.FilterInfo.SelectSite),
        RebateType: stateDatas.FilterInfo.Type,
        StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
        EndDate:
          moment(stateDatas.FilterInfo.EndDate).add(1, "days").unix() - 1,
        UserName: stateDatas.FilterInfo.User,
      },
    };

    WebAPI.DownloadData(param);
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Rebate Log Report</h4>
      <Filter
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        ClickSubmit={Filter_Submit}
        SiteList={siteList}
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

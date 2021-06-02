import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";

const App = () => {
  const [stateFilter, setFilter] = useState({
    SiteIds: [],
    CurrencyIds: [],
    StartDate: moment().add(-14, "days").format("l"),
    EndDate: moment().format("l"),
  });

  const [stateDatas, setDatas] = useState({
    Datas: null,
    FilterInfo: null,
    PlusNum: 0,
    MinusNum: 0,
    FeeNum: 0,
  });

  const [siteList, setSiteList] = useState([]);
  const [SubmitStatus, setSubmitStatus] = useState(false);
  const [ExportStatus, setExportStatus] = useState(false);

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
    if (stateFilter.SiteIds.length === 0) {
      alert("Please Choose Site.");
      return false;
    }
    if (stateFilter.CurrencyIds.length === 0) {
      alert("Please Choose Currency.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose Start Date.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose End Date.");
      return false;
    }

    const currencyIds = [];
    stateFilter.CurrencyIds.map((o) => {
      currencyIds.push(o.value);
    });

    let param = {
      Url: "BankFinancialReport/GetBankFinancialReport",
      Data: {
        SiteIds: stateFilter.SiteIds,
        CurrencyIds: currencyIds,
        StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
        EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      },
    };

    setSubmitStatus(true);
    setExportStatus(true);

    WebAPI.PostData(param).then((res) => {
      setSubmitStatus(false);
      setExportStatus(false);

      if (res.data.errorCode === 0) {
        console.log("result", res.data.data);
        setDatas({
          Datas: res.data.data,
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
    if (stateFilter.SiteIds.length === 0) {
      alert("Please Choose Site.");
      return false;
    }
    if (stateFilter.CurrencyIds.length === 0) {
      alert("Please Choose Currency.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose Start Date.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose End Date.");
      return false;
    }

    setSubmitStatus(true);
    setExportStatus(true);

    const currencyIds = [];
    stateFilter.CurrencyIds.map((o) => {
      currencyIds.push(o.value);
    });

    let param = {
      Url: "BankFinancialReport/ExportBankFinancialReport",
      Data: {
        SiteIds: JSON.stringify(stateFilter.SiteIds),
        CurrencyIds: JSON.stringify(currencyIds),
        StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
        EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      },
    };

    WebAPI.DownloadData(param);

    setTimeout(() => {
      setSubmitStatus(false);
      setExportStatus(false);
    }, 5000);
  };

  return (
    <div className="container-fluid" style={{ width: "auto" }}>
      <h4 className="page-name">Bank Financial Report</h4>
      <Filter
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        ClickSubmit={Filter_Submit}
        SiteList={siteList}
        SubmitBtnDisabled={SubmitStatus}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          ExportToExcel={ExportToExcel}
          ExportBtnDisabled={ExportStatus}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

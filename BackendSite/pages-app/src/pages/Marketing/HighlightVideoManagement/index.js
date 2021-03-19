import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";
import Filter from "./Filter";

const App = () => {
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
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

  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      PageNumber: 1,
      StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
    };
    // api
    //   .post("/api/DrawPrizeReport/PrizeSearch", Data)
    //   .then(function (res) {
    //     if (res.data.errorCode === 0) {
    //       setDatas({
    //         PageInfo: {
    //           totalPages: res.data.totalPages,
    //           pageNumber: 1,
    //           rowCount: res.data.rowCount,
    //         },
    //         Datas: [...res.data.data],
    //         FilterInfo: {
    //           ...stateFilter,
    //         },
    //       });
    //     } else {
    //       alert(res.data.message ?? "failed to get data.");
    //     }
    //   })
    //   .catch(function (error) {
    //     alert(error);
    //   });
  };
  const CreateHighlightVideo =()=>{

  }
  return (
    <div className="container-fluid">
      <h4 className="page-name">Highlight Video Management</h4>
      <Filter
        filterInfo={stateFilter}
        SiteList={siteList}
        onChange={Filter_OnChange}
        onChangeCheckBox={Filter_CheckBox_OnChange}
        onChangeAllCheckBox={Filter_CheckBox_CheckAll}
        ClickSubmit={Filter_Submit}
      />
      <button
        className="btn btn-add"
        style={{ marginBottom: "20px" }}
        onClick={CreateHighlightVideo}
      >
        Create Highlight Video
      </button>
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

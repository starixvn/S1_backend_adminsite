import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import qs from "qs";
import Result from "./Result";
import moment from "moment";
import Detail from "./Detail";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState();
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    Type: 0,
    User: "",
    Event: "",
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

  const [detailDatas, setdetailDatas] = useState({
    PrizeId: 0,
    EventId: 0,
    StartDate: "",
    EndDate: "",
    EventTime: ""
  });

  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    api.post("/api/Common/AdminUserSiteList").then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    stateFilter[dataColumn] = v;
    setFilter({
      ...stateFilter,
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
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      CurrencyId: parseInt(stateFilter.Type, 10),
      PageNumber: 1,
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
    };
    api
      .post("/api/BonusManage/PrizeSearch", Data)
      .then(function (res) {
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
      SelectSite: stateFilter.SelectSite,
      CurrencyId: parseInt(stateFilter.Type, 10),
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      PageNumber: PageNumber,
    };
    api
      .post("/api/BonusManage/PrizeSearch", Data)
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
  const Click_Detail = (prop) => {
    setdetailDatas({
      ...detailDatas,
      PrizeId: prop.prizeId,
      EventId: prop.eventId,
      StartDate: prop.startTime,
      EndDate: prop.endTime,
      EventTime: prop.eventTime
    });
    setstate(1);
  };

  const Click_Back = () => {
    setstate(0);
  };

  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Prize Search</h4>
        <Filter
          filterInfo={stateFilter}
          onChange={Filter_OnChange}
          onChangeCheckBox={Filter_CheckBox_OnChange}
          onChangeAllCheckBox={Filter_CheckBox_CheckAll}
          ClickSubmit={Filter_Submit}
          SiteList={siteList}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            ChangePage={Result_ChangePage}
            Detail={Click_Detail}
          />
        )}
      </div>
    );
  } else {
    return (
      <div>
        <h4 className="detail">List Of Winner</h4>
        <Detail 
        PrizeId={detailDatas.PrizeId}
        EventId={detailDatas.EventId}
        StartDate={detailDatas.StartDate}
        EndDate={detailDatas.EndDate}
        EventTime={detailDatas.EventTime}/>
        <div className="button-group">
          <button
            className="btn btn-search"
            type="back"
            onClick={Click_Back}
            style={{
              marginTop: "20px",
              marginBottom: "40px",
              marginRight: "10px",
            }}
          >
            Back
          </button>
        </div>
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI, { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import moment from "moment";
import Detail from "./Detail";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    Type: 0,
    User: "",
    Event: "",
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    EventName: "0",
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
    EventTime: "",
    PrizeName: "",
    EventDate: "",
  });

  const [eventName, seteventName] = useState(null);

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
    api.post("/api/DrawPrizeReport/GetEventName").then((res) => {
      if (res.data.errorCode === 0) {
        seteventName(res.data.data);
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
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      CurrencyId: parseInt(stateFilter.Type, 10),
      PageNumber: 1,
      StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      EventName: stateFilter.EventName === "0" ? "" : stateFilter.EventName,
    };
    api
      .post("/api/DrawPrizeReport/PrizeSearch", Data)
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
      SelectSite: stateDatas.FilterInfo.SelectSite,
      CurrencyId: parseInt(stateDatas.FilterInfo.Type, 10),
      StartDate: moment(stateDatas.FilterInfo.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateDatas.FilterInfo.EndDate).format("YYYY-MM-DD"),
      PageNumber: PageNumber,
      EventName:
        stateDatas.FilterInfo.EventName === "0"
          ? ""
          : stateDatas.FilterInfo.EventName,
    };
    api
      .post("/api/DrawPrizeReport/PrizeSearch", Data)
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
      EventTime: prop.eventTime,
      PrizeName: prop.prizeName,
      EventDate: prop.eventDate,
    });
    setstate(1);
  };

  const Click_Back = () => {
    setstate(0);
  };

  const Result_ExportToExcel = () => {
    let param = {
      Url: "DrawPrizeReport/ExportPrizeData",
      Data: {
        SelectSite: JSON.stringify(stateDatas.FilterInfo.SelectSite),
        StartDate: moment(stateDatas.FilterInfo.StartDate).format("YYYY-MM-DD"),
        EndDate: moment(stateDatas.FilterInfo.EndDate).format("YYYY-MM-DD"),
        CurrencyId: parseInt(stateDatas.FilterInfo.Type, 10),
        EventName:
          stateDatas.FilterInfo.EventName === "0"
            ? ""
            : stateDatas.FilterInfo.EventName,
      },
    };

    WebAPI.DownloadData(param);
  };

  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Draw Prize Report</h4>
        <Filter
          filterInfo={stateFilter}
          onChange={Filter_OnChange}
          onChangeCheckBox={Filter_CheckBox_OnChange}
          onChangeAllCheckBox={Filter_CheckBox_CheckAll}
          ClickSubmit={Filter_Submit}
          SiteList={siteList}
          EventName={eventName}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            ChangePage={Result_ChangePage}
            Detail={Click_Detail}
            ExportToExcel={Result_ExportToExcel}
          />
        )}
      </div>
    );
  } else {
    return (
      <div>
        <h5 className="detail">EventDate:{detailDatas.EventDate}</h5>
        <h5 className="detail">PrizeName:{detailDatas.PrizeName}</h5>
        <Detail
          PrizeId={detailDatas.PrizeId}
          EventId={detailDatas.EventId}
          StartDate={detailDatas.StartDate}
          EndDate={detailDatas.EndDate}
          EventTime={detailDatas.EventTime}
        />
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

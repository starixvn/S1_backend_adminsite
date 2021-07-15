import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";

const App = () => {
  const [state, setstate] = useState(0);
  const [Menu, setMenu] = useState(null);
  const [stateFilter, setFilter] = useState({
    Menu: "0",
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    User: "",
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [detailData, setdetailData] = useState({
    Id: null,
    Menu: null,
    ActionType: null,
    DetailInfo: null,
    CreateTime: null,
    Operator: null,
  });

  useEffect(() => {
    api
      .post("/api/AdjustmentLogReport/GetAdjustmentInfoLogMenu")
      .then((res) => {
        if (res.data.errorCode === 0) {
          setMenu(res.data.data);
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
  const SetDetailData = (props) => {
    setdetailData({
      Id: props.id,
      Menu: props.menu,
      ActionType: props.actionType,
      DetailInfo: props.detailInfo,
      CreateTime: props.createTime,
      Operator: props.operator,
    });
    setstate(1);
  };
  const Back=()=>{
    setstate(0);
  }

  const Submit = () => {
    let Data = {
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      User: stateFilter.User,
      Menu: stateFilter.Menu,
      PageNumber: 1,
      PageSize: parseInt(10),
    };
    api
      .post("/api/AdjustmentLogReport/GetAdjustmentInfoLog", Data)
      .then((res) => {
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

  const ChangePage = (PageNumber) => {
    let Data = {
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      User: stateFilter.User,
      Menu: stateFilter.Menu,
      PageNumber: PageNumber,
      PageSize: parseInt(10),
    };
    api
      .post("/api/AdjustmentLogReport/GetAdjustmentInfoLog", Data)
      .then((res) => {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
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

  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Adjustment Log Report</h4>
        <Filter
          Menu={Menu}
          stateFilter={stateFilter}
          onChange={Filter_OnChange}
          ClickSubmit={Submit}
        />
        {stateDatas.Datas === null ? null : (
          <Result {...stateDatas} ChangePage={ChangePage} SetDetailData={SetDetailData} />
        )}
      </div>
    );
  } else {
    return (
      <div>
        <Detail
          DetailData={detailData}
          Back={Back}
        />
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";

const App = () => {
  const [Menu, setMenu] = useState(null);
  const [stateFilter, setFilter] = useState({
    Menu: "0",
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    User: "",
  });
  useEffect(() => {
    api
      .post("/api/AdjustmentLogReport/GetAdjustmentInfoLogMenu")
      .then((res) => {
        if (res.data.errorCode === 0) {
          console.log(res.data.data);
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

  const Submit = () => {
    let Data = {
      StartDate: moment(stateFilter.StartDate).unix(),
      EndDate: moment(stateFilter.EndDate).add(1, "days").unix() - 1,
      User: stateFilter.User,
      Menu: stateFilter.Menu,
      PageNumber: 1,
      PageSize: parseInt(10),
    };
    api.post("/api/AdjustmentLogReport/GetAdjustmentInfoLog",Data).then((res) => {
      if (res.data.errorCode === 0) {
        console.log(res.data.data);
        // setMenu(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Adjustment Log Report</h4>
      <Filter
        Menu={Menu}
        stateFilter={stateFilter}
        onChange={Filter_OnChange}
        ClickSubmit={Submit}
      />
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

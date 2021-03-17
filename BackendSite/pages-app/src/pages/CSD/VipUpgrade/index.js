import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";

const App = () => {
  const [siteList, setSiteList] = useState([]);
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
  const [stateFilter, setFilter] = useState({
    optSite: 0,
    month: 1,
    year: 2021
  });
  const Submit = ()=>{
    console.log(stateFilter)
    let Data = {
      Year: stateFilter.year,
      Month: stateFilter.month,
      SiteId: stateFilter.optSite,
    };
    api
      .post("/api/CustManage/GetVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }
  return (
    <div className="container-fluid">
      <h4 className="page-name">VipUpgrade</h4>
      <Filter
        SiteList={siteList}
        StateFilter={stateFilter}
        SetFilter={setFilter}
        Submit={Submit}
      />
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

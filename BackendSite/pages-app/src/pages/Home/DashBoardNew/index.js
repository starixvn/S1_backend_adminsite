import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";

const App = () => {
  const [siteList, setsiteList] = useState([]);
  const [stateFilter, setFilter] = useState({
    Site: "0",
    Currency: "0",
    BaseCurrency: "2",
  });
  const [stateDatas, setDatas] = useState(null);
  useEffect(() => {

    api.post("/api/Common/AdminUserSiteList").then((res) => {
      if (res.data.errorCode === 0) {
        let result = [];
        res.data.data.map((e, index) => {
          result.push({ value: e.siteId, displayText: e.siteName });
        });
        setsiteList(result);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  useEffect(() => {
    let param = {
        SiteId: parseInt(stateFilter.Site),
        CurrencyId:parseInt(stateFilter.Currency),
        BaseCurrencyId:parseInt(stateFilter.BaseCurrency)
    };
    api.post("/api/Home/GetDashBoardData",param).then((res) => {
      if (res.data.errorCode === 0) {
        console.log(res.data.data);
        setDatas(res.data.data)
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, [stateFilter]);

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
    console.log(stateFilter);
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Dash Board</h4>
      <Filter filterInfo={stateFilter} SiteList={siteList} onChange={Filter_OnChange}/>
      {stateDatas === null ? null : (
          <Result DashBoardData = {stateDatas}/>
        )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

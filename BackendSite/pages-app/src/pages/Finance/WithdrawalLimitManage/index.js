import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import { api } from "../../../module/WebAPI";

const App = () => {
  const [siteList, setsiteList] = useState(null);
  const [site, setsite] = useState(0);
  useEffect(() => {
    api
      .post("/api/Common/AdminUserSiteList")
      .then(function (response) {
        if (response.data.errorCode === 0) {
          let SiteList = response.data.data.map((s) => {
            return {
              value: s.siteId,
              displayText: s.siteName,
            };
          });
          setsiteList(SiteList);
        } else {
          alert(response.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);
  const FilterChange = (v, dataColumn) => {
    setsite(v);
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">WithdrawalLimitManage</h4>
      {siteList === null ? null : <Filter SiteList={siteList} FilterChange={FilterChange} Site={site}/>}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

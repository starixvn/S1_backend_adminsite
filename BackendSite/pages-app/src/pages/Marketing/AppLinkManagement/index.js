import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
const App = () => {
  const [site, setSite] = useState([]);
  const [searchData, setsearchData] = useState([]);
  useEffect(() => {
    api
      .post("/api/Common/AdminUserSiteList")
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setSite(response.data === null ? [] : response.data);
        } else {
          // alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);

  const optSite = (siteId) => {
    setsearchData([]);
    api
      .post("/api/AppLinkTable/GetAppLinkTable", {
        SiteId: siteId,
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setsearchData(response.data.data);
        } else {
          // alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">App Link Management</h4>
      <Filter site={site} optSite={optSite} />
      {searchData.length === 0 ? null : 
      <Result 
        Data={searchData}
      />}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";

const App = () => {
  const [site, setSite] = useState([]);
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
    // setsearchData([]);
    // api
    //   .post("/api/AppLinkTable/GetAppLinkTable", {
    //     SiteId: siteId,
    //   })
    //   .then(function (response) {
    //     if (response.data.errorCode === 0) {
    //       setsearchData(response.data.data);
    //     } else {
    //     }
    //   })
    //   .catch(function (error) {
    //     alert(error);
    //   });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Deep Link Management</h4>
      <Filter site={site} optSite={optSite} />
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

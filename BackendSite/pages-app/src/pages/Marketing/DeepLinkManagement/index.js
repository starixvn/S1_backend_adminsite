import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
const App = () => {
  const [site, setSite] = useState([]);
  const [deepLinkList, setdeepLinkList] = useState([]);
  const [filterInfo, setfilterInfo] = useState({
    DeepLinkId: 0,
    SiteId: 0,
  });
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
  useEffect(() => {
    api
      .post("/api/DeepLinkManagement/GetDeepLinkList")
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setdeepLinkList(response.data.data);
        } else {
          // alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);
  useEffect(() => {
    if (
      Number(filterInfo.SiteId) !== 0 &&
      Number(filterInfo.DeepLinkId) !== 0
    ) {
      api
        .post("/api/DeepLinkManagement/GetDeepLinkTable", {
          SiteId: Number(filterInfo.SiteId),
          DeepLinkId: Number(filterInfo.DeepLinkId),
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
    }else{
        setsearchData([]);
    }
  }, [filterInfo]);

  const filterChange = (value, column) => {
    setfilterInfo({
      ...filterInfo,
      [column]: value,
    });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">Deep Link Management</h4>
      <Filter
        site={site}
        deepLinkList={deepLinkList}
        filterChange={filterChange}
        filterInfo={filterInfo}
      />
      {searchData.length === 0 ? null : <Result Data={searchData} />}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

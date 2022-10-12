import React, { useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import WebAPI from "../../../module/WebAPI";
import moment from "moment-timezone";
import Filter from "./Filter";
import CategoryFilter from "./CategoryFilter";
import Result from "./Result";

const App = () => {
  const [siteList, setSiteList] = useState([]);
  const [stateTPList, setTPList] = useState(null);
  const [stateFilter, setFilter] = useState({
    TPid: 0,
    Name: "",
    Site: 0,
    Category: 0,
  });
  const [backupFilter, setbackupFilter] = useState({
    TPid: null,
    Name: null,
    Site: 0,
    Category: 0,
  });
  const [datas, setdatas] = useState(null);
  const [pageInfo, setpageInfo] = useState(null);
  useEffect(() => {
    api.post("/api/Common/AdminUserSiteList").then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);
  useEffect(() => {
    api
      .post("/api/GameCasinoManagement/GetTpList")
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setTPList(res.data.data);
          if (res.data.data.length > 0) {
            setFilter({
              ...stateFilter,
              TPid: res.data.data[0].tpId,
            });
          }
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);
  const Submit = () => {
    if (stateFilter.Site === 0) {
      alert("Please Choose at Site");
      return false;
    }
    let Data = {
      TpId: parseInt(stateFilter.TPid),
      Name: stateFilter.Name,
      Category: 0,
      SiteId: parseInt(stateFilter.Site),
      PageNumber: 1,
    };
    api
      .post("/api/SlotGamesManagement/GetSlotGameCategoryList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setFilter({
            ...stateFilter,
            Category: 0,
          });
          setpageInfo({
            totalPages: res.data.totalPages,
            pageNumber: 1,
            rowCount: res.data.rowCount,
          });
          setdatas(res.data.data);
          setbackupFilter({
            TPid: parseInt(stateFilter.TPid),
            Site: stateFilter.Site,
            Name: stateFilter.Name,
            Category: parseInt(stateFilter.Category),
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
    let Data = {
      TpId: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      Category: parseInt(backupFilter.Category),
      SiteId: parseInt(backupFilter.Site),
      PageNumber: PageNumber,
    };
    api
      .post("/api/SlotGamesManagement/GetSlotGameCategoryList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setpageInfo({
            totalPages: res.data.totalPages,
            pageNumber: res.data.pageNumber,
            rowCount: res.data.rowCount,
          });
          setdatas(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const FilterOnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };
  const SetSite = (siteId) => {
    setFilter({
      ...stateFilter,
      Site: siteId,
    });
  };
  const SetCategory = (category) => {
    setFilter({
      ...stateFilter,
      Category: category,
    });
    setbackupFilter({
      ...backupFilter,
      Category: category,
    });
    let Data = {
      TpId: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      Category: category,
      SiteId: parseInt(backupFilter.Site),
      PageNumber: pageInfo.pageNumber,
    };
    api
      .post("/api/SlotGamesManagement/GetSlotGameCategoryList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setdatas(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const UpdateResultStatus = () => {
    let Data = {
      TpId: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      Category: parseInt(backupFilter.Category),
      SiteId: parseInt(backupFilter.Site),
      PageNumber: pageInfo.pageNumber,
    };
    api
      .post("/api/SlotGamesManagement/GetSlotGameCategoryList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setdatas(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">CategoryManagement</h4>
      {stateTPList === null ? null : (
        <Filter
          StateFilter={stateFilter}
          FiterOnChange={FilterOnChange}
          StateTPList={stateTPList}
          Submit={Submit}
          siteList={siteList}
          SetSite={SetSite}
        />
      )}
      {datas === null ? null : (
        <>
          <CategoryFilter StateFilter={stateFilter} SetCategory={SetCategory} />
          <Result
            StateFilter={stateFilter}
            BackupFilter={backupFilter}
            Datas={datas}
            UpdateResultStatus={UpdateResultStatus}
            PageInfo={pageInfo}
            ChangePage={Result_ChangePage}
            SiteList={siteList}
          />
        </>
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

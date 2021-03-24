import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";

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
    YearMonth: moment(),
    Status:0,
    UserName:""
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
    CanUpgrade: false
  });
  const Submit = () => {
    if (stateFilter.optSite === 0) {
      alert("Please Choose at one Site");
      return false;
    }
    let Data = {
      SiteId: stateFilter.optSite,
      YearMonth: moment(stateFilter.YearMonth).format("YYYYMM"),
      PageNumber: 1,
      Status: parseInt(stateFilter.Status, 10),
      UserName: stateFilter.UserName
    };
    api
      .post("/api/VipLevel/GetVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
            CanUpgrade: res.data.errorCode === 0
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
    if (stateDatas.FilterInfo.optSite === 0) {
      alert("Please Choose at one Site");
      return false;
    }
    let Data = {
      SiteId: stateDatas.FilterInfo.optSite,
      YearMonth: moment(stateDatas.FilterInfo.YearMonth).format("YYYYMM"),
      PageNumber: PageNumber,
      Status: parseInt(stateDatas.FilterInfo.Status, 10),
      UserName: stateFilter.UserName
    };
    api
      .post("/api/VipLevel/GetVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
            CanUpgrade: res.data.errorCode === 0
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const UpdateReset = () => {
    let Data = {
      SiteId: stateDatas.FilterInfo.optSite,
      YearMonth: moment(stateDatas.FilterInfo.YearMonth).format("YYYYMM"),
      PageNumber: stateDatas.PageInfo.pageNumber,
      Status: parseInt(stateDatas.FilterInfo.Status, 10),
      UserName: stateFilter.UserName
    };
    api
      .post("/api/VipLevel/GetVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: stateDatas.PageInfo.pageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
            CanUpgrade: res.data.errorCode === 0
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Upgrade = (props) => {
    let CustIds = [];
    CustIds.push(props.custId);
    let Data = {
      CustIds: JSON.stringify(CustIds),
      YearMonth: moment(stateDatas.FilterInfo.YearMonth).format("YYYYMM"),
    };
    api
      .post("/api/VipLevel/UpdateVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          UpdateReset();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const AllUpgrade = () => {
    let CustIds = [];
    stateDatas.Datas.forEach((x) => {
      if (x["status"] === 1) {
        CustIds.push(x["custId"]);
      }
    });
    if(CustIds.length === 0){
      return false;
    }
    let Data = {
      CustIds: JSON.stringify(CustIds),
      YearMonth: moment(stateDatas.FilterInfo.YearMonth).format("YYYYMM"),
    };
    api
      .post("/api/VipLevel/UpdateVipUpgradeReview", Data)
      .then(function (res) {
        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          UpdateReset();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Result_ExportToExcel =()=>{
    let param = {
      Url: "VipLevel/ExportVipUpgradedata",
      Data: {
        SiteId: stateDatas.FilterInfo.optSite,
        YearMonth: moment(stateDatas.FilterInfo.YearMonth).format("YYYYMM"),
        Status: parseInt(stateDatas.FilterInfo.Status, 10),
        UserName: stateFilter.UserName
      },
    };

    WebAPI.DownloadData(param);
  }
  const StatusChange=(props)=>{
    setFilter({
      ...stateFilter,
      Status:props
    })
  }
  const UserNameChange=(props)=>{
    setFilter({
      ...stateFilter,
      UserName:props
    })
  }
  return (
    <div className="container-fluid">
      <h4 className="page-name">VipUpgrade</h4>
      <Filter
        SiteList={siteList}
        StateFilter={stateFilter}
        SetFilter={setFilter}
        Submit={Submit}
        StatusChange={StatusChange}
        UserNameChange={UserNameChange}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          AllUpgrade={AllUpgrade}
          Upgrade={Upgrade}
          ChangePage={Result_ChangePage}
          ExportToExcel={Result_ExportToExcel}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import Filter from "./Filter";
import Result from "./Result";
import DetailFilterPage from "./DetailFilter";

const App = () => {
  const [mainPage, setmainPage] = useState(0);
  const [stateFilter, setFilter] = useState({
    TPid: 0,
    Name: ""
  });
  const [backupFilter, setbackupFilter] = useState({
    TPid: null,
    Name: null
  });
  const [detailData,setdetailData] = useState({
    
  });
  const [stateTPList, setTPList] = useState(null);
  const [pageInfo, setpageInfo] = useState(null);
  const [datas, setdatas] = useState(null);
  const [siteList, setsiteList] = useState(null);
  const [DetailFilter, setDetailFilter] = useState(0);
  useEffect(() => {
    api
      .post("/api/GameCasinoManagement/GetTpList")
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setTPList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);

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

  const FilterOnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };
  const DetailFilterChange = (v, dataColumn)=>{
    setDetailFilter(v)
  }
  const DetailSubmit =()=>{
      console.log(DetailFilter)

  }
  const Submit = () => {
    if (stateFilter.TPid === 0) {
      alert("Please Choose at TP");
      return false;
    }
    let Data = {
      TPid: parseInt(stateFilter.TPid),
      Name: stateFilter.Name,
      PageNumber: 1,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPIdV2", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setpageInfo({
            totalPages: res.data.totalPages,
            pageNumber: 1,
            rowCount: res.data.rowCount,
          });
          setdatas(res.data.data);
          setbackupFilter({
            TPid: parseInt(stateFilter.TPid),
            Currency: stateFilter.Currency,
            Site: stateFilter.Site,
            Name: stateFilter.Name,
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
      TPid: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      PageNumber: PageNumber,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPIdV2", Data)
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
  const Click_Detail = (GameData) => {
    console.log(GameData)
    setmainPage(1);
  };
  const UpdateResultStatus = () => {
    let Data = {
      TPid: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      PageNumber: pageInfo.pageNumber,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPIdV2", Data)
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
  if(mainPage === 0){
    return (
        <div className="container-fluid">
          <h4 className="page-name">GameCasinoManagement</h4>
          {stateTPList === null ? null : (
            <Filter
              StateFilter={stateFilter}
              FiterOnChange={FilterOnChange}
              StateTPList={stateTPList}
              Submit={Submit}
            />
          )}
          {datas === null ? null : (
            <Result
              StateFilter={stateFilter}
              Datas={datas}
              PageInfo={pageInfo}
              ChangePage={Result_ChangePage}
              Detail={Click_Detail}
              Setdatas={setdatas}
              UpdateResultStatus={UpdateResultStatus}
            />
          )}
        </div>
      );
  }
  else{
    return(<div className="container-fluid">
        <DetailFilterPage
            SiteList={siteList}
            DetailFilterChange={DetailFilterChange}
            DetailFilter={DetailFilter}
            Submit={DetailSubmit}
        />        
    </div>)
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

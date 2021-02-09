import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";

const App = () => {
  const [stateFilter, setFilter] = useState({
    Site: null,
    Currency: null,
    TPid: null,
  });
  const [stateTPList, setTPList] = useState(null);
  const [datas, setdatas] = useState(null);
  const [pageInfo, setpageInfo] = useState(null);
  const [mainPage, setmainPage] = useState(0);
  const [detailInfo, setdetailInfo] = useState({
    Ename: "",
    Cname: "",
    site: null,
    currency: null,
    tpId: null,
    gameCasinoId: null,
  });
  const [detailDatas, setdetailDatas] = useState({
    SiteId: 0,
    CurrencyId: 0,
    GameCasinoId: 0,
    Hot: false,
    Featured: false,
    Status: false,
    UpdateAt: "",
  });

  const FilterOnChange = (v, dataColumn) => {
    console.log(v, dataColumn);
    if ((dataColumn === "Site") & (stateFilter.Currency !== null)) {
      GetTpBySiteAndCurrency(v, stateFilter.Currency);
    }
    if ((dataColumn === "Currency") & (stateFilter.Site !== null)) {
      GetTpBySiteAndCurrency(stateFilter.Site, v);
    }
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
    if (stateFilter.Site !== null) {
    }
  };

  const GetTpBySiteAndCurrency = (site, currency) => {
    let Data = {
      Site: site,
      Currency: currency,
    };
    api
      .post("/api/Tp/GetTpBySiteAndCurrency", Data)
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
  };

  const Submit = () => {
    if (stateFilter.TPid === null || stateFilter.TPid === "0") {
      alert("Please Choose at TP");
      return false;
    }
    let Data = {
      Site: stateFilter.Site,
      Currency: stateFilter.Currency,
      TPid: parseInt(stateFilter.TPid),
      PageNumber: 1,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPId", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setpageInfo({
            totalPages: res.data.totalPages,
            pageNumber: 1,
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

  const Result_ChangePage = (PageNumber) => {
    let Data = {
      Site: stateFilter.Site,
      Currency: stateFilter.Currency,
      TPid: parseInt(stateFilter.TPid),
      PageNumber: PageNumber,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPId", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          console.log(888);
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
    let Data = {
      SiteId: GameData.site,
      CurrencyId: GameData.currency,
      GameCasinoId: GameData.gameCasinoId,
      TpId: GameData.tpId,
    };
    setdetailInfo({
      ...detailInfo,
      Ename: GameData.ename,
      Cname: GameData.cname,
      site: GameData.site,
      currency: GameData.currency,
      tpId: GameData.tpId,
      gameCasinoId: GameData.gameCasinoId,
    });

    api
      .post("/api/GameCasinoManagement/GetGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          console.log(123);
          console.log(res.data);
          if (res.data.data !== null) {
            console.log(456);
          }
          setdetailDatas({
            ...detailDatas,
            SiteId: res.data.data.siteId,
            CurrencyId: res.data.data.currencyId,
            GameCasinoId: res.data.data.gameCasinoId,
            Hot: res.data.data.hot === 1,
            Featured: res.data.data.featured === 1,
            Status: res.data.data.status === 1,
            UpdateAt: "",
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
    setmainPage(1);
  };

  const Goback = () => {
    let Data = {
      Site: stateFilter.Site,
      Currency: stateFilter.Currency,
      TPid: parseInt(stateFilter.TPid),
      PageNumber: pageInfo.pageNumber,
    };
    api
      .post("/api/GameCasinoManagement/GameCasinoByTPId", Data)
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
    setmainPage(0);
  };
  const Create = () => {
    let Data = {
      SiteId: detailInfo.site,
      CurrencyId: detailInfo.currency,
      GameCasinoId: detailInfo.gameCasinoId,
    };
    api
      .post("/api/GameCasinoManagement/InsertGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setdetailDatas({
            ...detailDatas,
            SiteId: detailInfo.site,
            CurrencyId: detailInfo.currency,
            GameCasinoId: detailInfo.gameCasinoId,
            Hot: false,
            Featured: false,
            Status: false,
            UpdateAt: "",
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Delete = () => {
    let Data = {
      SiteId: detailInfo.site,
      CurrencyId: detailInfo.currency,
      GameCasinoId: detailInfo.gameCasinoId,
    };
    api
      .post("/api/GameCasinoManagement/DeleteGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setdetailDatas({
            SiteId: 0,
            CurrencyId: 0,
            GameCasinoId: 0,
            Hot: false,
            Featured: false,
            Status: false,
            UpdateAt: "",
          });
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  if (mainPage === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">GameCasinoManagement</h4>
        <Filter
          StateFilter={stateFilter}
          FiterOnChange={FilterOnChange}
          StateTPList={stateTPList}
          Submit={Submit}
        />
        {datas === null ? null : (
          <Result
            StateFilter={stateFilter}
            Datas={datas}
            PageInfo={pageInfo}
            ChangePage={Result_ChangePage}
            Detail={Click_Detail}
          />
        )}
      </div>
    );
  } else {
    return (
      <div>
        <h5 className="detail">SiteId:{detailInfo.site}</h5>
        <h5 className="detail">CurrencyId:{detailInfo.currency}</h5>
        <h5 className="detail">TpId:{detailInfo.tpId}</h5>
        <h5 className="detail">GameCasinoId:{detailInfo.gameCasinoId}</h5>
        <h5 className="detail">Ename:{detailInfo.Ename}</h5>
        <h5 className="detail">Cname:{detailInfo.Cname}</h5>
        <Detail
          Datas={detailDatas}
          SetdetailDatas={setdetailDatas}
          detailDatas={detailDatas}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="back"
            onClick={Goback}
            style={{
              marginTop: "20px",
              marginBottom: "40px",
              marginRight: "10px",
            }}
          >
            Back
          </button>
          {detailDatas.SiteId === 0 ? (
            <button
              className="btn btn-search"
              type="back"
              onClick={Create}
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
            >
              Create
            </button>
          ) : null}
          {detailDatas.SiteId !== 0 ? (
            <button
              className="btn btn-search"
              type="back"
              onClick={Delete}
              style={{
                marginTop: "20px",
                marginBottom: "40px",
                marginRight: "10px",
              }}
            >
              Delete
            </button>
          ) : null}
        </div>
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

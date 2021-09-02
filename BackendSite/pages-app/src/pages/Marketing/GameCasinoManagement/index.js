import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";
import Filter from "./Filter";
import Result from "./Result";

const App = () => {
  const [stateFilter, setFilter] = useState({
    TPid: 0,
    Name: "",
    Categories: 0,
    Currency: 0,
    Site:0
  });
  const [backupFilter, setbackupFilter] = useState({
    TPid: null,
    Name: null,
    Categories: 0,
    Currency: 0,
    Site:0
  });
  const [detailData, setdetailData] = useState({});
  const [stateTPList, setTPList] = useState(null);
  const [pageInfo, setpageInfo] = useState(null);
  const [datas, setdatas] = useState(null);
  const [siteList, setsiteList] = useState(null);
  const [detailSearchData, setdetailSearchData] = useState(null);
  const [detailgameCasinoId,setdetailgameCasinoId] = useState(0);

  useEffect(() => {
    if (detailgameCasinoId != 0) {
      let Data = {
        GameCasinoId: parseInt(detailData.gameCasinoId),
      };

      api
        .post("/api/GameCasinoManagement/GetGameCasinoDetailV3", Data)
        .then(function (res) {
          if (res.data.errorCode === 0) {
            setdetailSearchData(res.data.data);
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  }, [detailgameCasinoId]);

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

  const DetailSubmit = () => {
    let Data = {
      GameCasinoId: parseInt(detailData.gameCasinoId),
    };

    api
      .post("/api/GameCasinoManagement/GetGameCasinoDetailV3", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setdetailSearchData(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const Submit = () => {
    if (stateFilter.TPid === 0) {
      alert("Please Choose at TP");
      return false;
    }
    let Data = {
      TPid: parseInt(stateFilter.TPid),
      Name: stateFilter.Name,
      Currency: parseInt(stateFilter.Currency),
      Categories: parseInt(stateFilter.Categories),
      Site: parseInt(stateFilter.Site),
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
            Categories: stateFilter.Categories,
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
      Currency: parseInt(backupFilter.Currency),
      Categories: parseInt(backupFilter.Categories),
      Site: parseInt(backupFilter.Site),
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
    setdetailSearchData(null);
    setdetailData(GameData);
    setdetailgameCasinoId(GameData.gameCasinoId);
  };
  const UpdateResultStatus = () => {
    let Data = {
      TPid: parseInt(backupFilter.TPid),
      Name: backupFilter.Name,
      Currency: parseInt(backupFilter.Currency),
      Categories: parseInt(backupFilter.Categories),
      Site: parseInt(backupFilter.Site),
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
  const Create_Detail = (props) => {
    let Data = {
      SiteId: props.siteId,
      CurrencyId: props.currencyId,
      GameCasinoId: props.gameCasinoId,
    };
    api
      .post("/api/GameCasinoManagement/InsertGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          DetailSubmit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Delete_Detail = (props) => {
    let Data = {
      SiteId: props.siteId,
      CurrencyId: props.currencyId,
      GameCasinoId: props.gameCasinoId,
      Hot: props.hot,
      Featured: props.featured,
      Status: props.status,
    };
    api
      .post("/api/GameCasinoManagement/DeleteGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          DetailSubmit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Update_Detail_Status = (type, props) => {
    let Data = {
      SiteId: props.siteId,
      CurrencyId: props.currencyId,
      GameCasinoId: props.gameCasinoId,
      Type: type,
      Hot: props.hot,
      Featured: props.featured,
      Status: props.status,
      Promotion: props.promotion,
    };
    api
      .post("/api/GameCasinoManagement/SetGameCasinoDetail", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          DetailSubmit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Goback = () => {
    setdetailSearchData(null);
    setdetailgameCasinoId(0);
  };
  return (
    <div className="container-fluid">
      <h4 className="page-name">GameCasinoManagement</h4>
      {stateTPList === null ? null : (
        <Filter
          StateFilter={stateFilter}
          FiterOnChange={FilterOnChange}
          StateTPList={stateTPList}
          Submit={Submit}
          SiteList={siteList}
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
          DetailGameCasinoId={detailgameCasinoId}
          DetailSearchData={detailSearchData}
          Create={Create_Detail}
          Delete={Delete_Detail}
          Update={Update_Detail_Status}
          Goback={Goback}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

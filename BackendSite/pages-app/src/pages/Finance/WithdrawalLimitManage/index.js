import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import { api } from "../../../module/WebAPI";
import { PopBoxContentEdit } from "./PopBoxContentEdit";

const App = () => {
  //const [mainPage, setmainPage] = useState(0);
  const [siteList, setsiteList] = useState(null);
  const [site, setsite] = useState(0);
  const [SearchData, setSearchData] = useState(null);
  const [EditData, setEditData] = useState(null);
  const [isOpen, setOpen] = useState(false);
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
  useEffect(() => {
    if (site !== 0) {
      let Data = {
        SiteId: parseInt(site),
      };
      api
        .post("/api/Withdrawal/GetWithdrawalLimitData", Data)
        .then(function (response) {
          if (response.data.errorCode === 0) {
            setSearchData(response.data.data);
          } else {
            alert(response.data.message ?? "failed to get data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  }, [site]);
  const ReSearch = () => {
    let Data = {
      SiteId: parseInt(site),
    };
    api
      .post("/api/Withdrawal/GetWithdrawalLimitData", Data)
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setSearchData(response.data.data);
        } else {
          alert(response.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const FilterChange = (v, dataColumn) => {
    setsite(v);
  };
  const Create = (props) => {
    let Data = {
      SiteId: parseInt(props.siteId),
      CurrencyId: parseInt(props.currencyId),
    };
    api
      .post("/api/Withdrawal/InsertWithdrawalLimit", Data)
      .then(function (response) {
        if (response.data.errorCode === 0) {
          ReSearch();
        } else {
          alert(response.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };
  const Edit = (props) => {
    setEditData(props);
    setOpen(true);
  };
  const EditSet = (props)=>{
    setEditData({
      ...EditData,
      withdrawalMin:props
    })
  }
  const Save = ()=>{
    let Data = {
      SiteId: parseInt(EditData.siteId),
      CurrencyId: parseInt(EditData.currencyId),
      WithdrawalMin:parseInt(EditData.withdrawalMin)
    };

  }
  const Cancel = ()=>{
    console.log('Cancel')
  }
  return (
    <div className="container-fluid">
      <h4 className="page-name">WithdrawalLimitManage</h4>
      {siteList === null ? null : (
        <Filter SiteList={siteList} FilterChange={FilterChange} Site={site} />
      )}
      {SearchData === null ? null : (
        <Result Data={SearchData} Create={Create} Edit={Edit} />
      )}
      {isOpen ? (
        <PopBoxContentEdit
          EditData={EditData}
          EditSet={EditSet}
          Save={Save}
          Cancel={Cancel}
        />
      ) : null}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

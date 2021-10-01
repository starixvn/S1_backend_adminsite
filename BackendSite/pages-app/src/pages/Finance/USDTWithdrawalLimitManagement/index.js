import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import { CurrencyInfo } from "./CurrencyInfo";

const NetworkType = {
    1: "ERC20",
    2: "TRC20"
  };

const App = () => {
  return (
    <div className="container-fluid">
      <h4 className="page-name">Withdrawal Limit Management</h4>
      <div>
        <USDTWithdrawalLimitInfo />
      </div>
    </div>
  );
};

const USDTWithdrawalLimitInfo = () => {
  const [opt, setOpt] = useState({
    siteId: 0,
    siteCurrencyList: [],
    Network: 0
  });
  const [site, setSite] = useState([]);
  const optSite = (siteId, siteCurrencyList) => {
    setOpt({ ...opt,siteId:siteId, siteCurrencyList:siteCurrencyList });
  };
  const optNetwork = (Network) =>{
    setOpt({
        ...opt,
        Network:Network
    })
  }
  let radio = "";
  let Networkradio = "";
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

  Networkradio = Object.keys(NetworkType).map((key) => {
    // return {
    //   displayText: StatusType[key],
    //   value: key,
    // };
    return (
        <div
          key={`NetworkOpt-${key}`}
          className="custom-control custom-radio custom-control-inline"
        >
          <input
            type="radio"
            id={`Network_${key}`}
            name="NetworkList"
            className="custom-control-input"
            value={key}
            onChange={optNetwork.bind(null, key)}
          />
          <label
            className="custom-control-label"
            htmlFor={`Network_${key}`}
          >
            {NetworkType[key]}
          </label>
        </div>
      );
  });

  if (site.length === 0) {
    return <></>;
  } else {
    radio = site.data.map((value) => {
      return (
        <div
          key={`SiteOpt-${value.siteId}`}
          className="custom-control custom-radio custom-control-inline"
        >
          <input
            type="radio"
            id={`SiteId_${value.siteId}`}
            name="SiteList"
            className="custom-control-input"
            value={value.siteId}
            onChange={optSite.bind(null, value.siteId, value.iCurrencyList)}
          />
          <label
            className="custom-control-label"
            htmlFor={`SiteId_${value.siteId}`}
          >
            {value.siteName}
          </label>
        </div>
      );
    });
  }
  return (
    <>
      <div className="form-group">
        <label className="col-form-label d-block">Site Name</label>
        {radio}
        <label className="col-form-label d-block">Network</label>
        {Networkradio}
      </div>
      {opt.siteCurrencyList.length === 0 || opt.Network === 0 ? null : (
        <CurrencyInfo optSite={opt} />
      )}
    </>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useState, useEffect } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import CurrencyLimitSetting from "./CurrencyLimitSetting";

const App = () => {
  //window.RootUrl = "http://localhost:9158";
  return (
    <div className="container-fluid">
      <h4 className="page-name">Crypto Withdrawal Limit Management</h4>
      <div>
        <CryptoWithdrawalLimitInfo />
      </div>
    </div>
  );
};

const CryptoWithdrawalLimitInfo = () => {
  const [opt, setOpt] = useState({
    siteId: 0,
    siteCurrencyList: [],
    networkType: 0,
    cryptoCurrencyId: 0
  });

  const [siteList, setSiteList] = useState([]);
  const [networkList, setNetwork] = useState([]);
  const [cryptoCurrencyList, setCryptoCurrency] = useState([]);

  const optSite = (siteId, siteCurrencyList) => {
    setOpt({
       ...opt,
       siteId: siteId,
       siteCurrencyList: siteCurrencyList
    });
  };

  const optNetwork = (networkType) => {
    setOpt({
      ...opt,
      networkType: networkType
    })
  };

  const optCryptoCurrency = (cryptoCurrencyId) => {
    setOpt({
      ...opt,
      cryptoCurrencyId: cryptoCurrencyId,
      networkType: networkList.find(x => x.currencyId === cryptoCurrencyId).networkType
    })
  }

  useEffect(() => {
    let param = {
      Url: "api/Common/AdminUserSiteList",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message || "failed to get site list data.");
      }
    });
  }, []);

  useEffect(() => {
    let param = {
      Url: "api/GetAllData/GetCryptoInfoData",
    };
    WebAPI.PostData(param).then((res) => {
      if (res.data.errorCode === 0) {
        setCryptoCurrency(res.data.data.cryptoCurrency);
        setNetwork(res.data.data.cryptoNetwork);
      } else {
        alert(res.data.message || "failed to get crypto info data.");
      }
    });
  }, []);

  let cryptoCurrencyRadio = cryptoCurrencyList.map((item) => {
    return (
      <div
        key={`CryptoCurrencyOpt-${item.currencyId}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`CryptoCurrency_${item.currencyId}`}
          name="CryptoCurrencyList"
          className="custom-control-input"
          value={item.currencyId}
          onChange={optCryptoCurrency.bind(null, item.currencyId)}
        />
        <label
          className="custom-control-label"
          htmlFor={`CryptoCurrency_${item.currencyId}`}
        >
          {item.currencyName}
        </label>
      </div>
    );
  });

  let networkRadio = networkList
    .filter(x => x.currencyId === opt.cryptoCurrencyId)
    .map((item) => {
    return (
      <div
        key={`NetworkOpt-${item.networkType}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`Network_${item.networkType}`}
          name="NetworkList"
          className="custom-control-input"
          value={item.networkType}
          onChange={optNetwork.bind(null, item.networkType)}
          checked={opt.networkType === item.networkType}
        />
        <label
          className="custom-control-label"
          htmlFor={`Network_${item.networkType}`}
        >
          {item.network}
        </label>
      </div>
    );
  });

  let siteRadio = siteList.length > 0
    ? (siteList.map((value) => {
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
    }))
    : "";

    

  return (
    <>
      <div className="form-group">
        <label className="col-form-label d-block">Site Name</label>
        {siteRadio}
        <label className="col-form-label d-block">Crypto Currency</label>
        {cryptoCurrencyRadio}
        {opt.cryptoCurrencyId === 0 ? null:
          <>
            <label className="col-form-label d-block">Network</label>
            {networkRadio}
          </>}
        
      </div>
      {opt.siteCurrencyList.length === 0
        || opt.cryptoCurrencyId === 0
        || opt.network === 0 ? null : (
          <CurrencyLimitSetting
            siteCurrencyList={opt.siteCurrencyList}
            siteId={opt.siteId}
            networkType={opt.networkType}
            cryptoCurrencyId={opt.cryptoCurrencyId} />
      )}
    </>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}
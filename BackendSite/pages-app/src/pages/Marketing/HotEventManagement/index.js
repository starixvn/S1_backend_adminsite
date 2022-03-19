import React, { useEffect, useRef, useReducer, useState } from "react";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";

const App = () => {
  const [siteList, setSiteList] = useState();
  const [filter, setfilter] = useState({
    SiteId: 0,
    CurrencyId: 0,
    Status: 0,
  });
  const [bannerList, setbannerList] = useState(null);
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
  const changeFilter = (value, type) => {
    setfilter({
      ...filter,
      [type]: value,
    });
    if (filter.SiteId != 0) {
      let param = {
        Url: "BannerManage/GetHotEventList",
        Data: {
          SiteId: filter.SiteId,
          CurrencyId: type == "CurrencyId" ? value : filter.CurrencyId,
          Status: type == "Status" ? value : filter.Status,
        },
      };
      WebAPI.PostData(param).then(
        (res) => {
          if (res.data.errorCode === 0) {
            setbannerList(res.data.data);
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        },
        (err) => {
          alert("API Server Error.");
        }
      );
    }
  };
  const setSiteId = (siteId, currencyId) => {
    setfilter({
      ...filter,
      SiteId: siteId,
      CurrencyId: currencyId,
    });
    let param = {
      Url: "BannerManage/GetHotEventList",
      Data: {
        SiteId: siteId,
        CurrencyId: currencyId,
        Status: filter.Status,
      },
    };
    WebAPI.PostData(param).then(
      (res) => {
        if (res.data.errorCode === 0) {
          setbannerList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      },
      (err) => {
        alert("API Server Error.");
      }
    );
  };
  const UpdateBannerStatus = (idx) => {
    let _banner = bannerList[idx];
    let msg =
      _banner.status === 1
        ? "Disable this HotEnent all currency?"
        : "Enable this HotEnent all currency?";
    if (_banner && window.confirm(msg)) {
      let newStatus = _banner.status === 0 ? 1 : 0;
      let param = {
        Url: "BannerManage/UpdateBannerStatus",
        Data: qs.stringify({
          SiteId: _banner.siteId,
          BannerName: _banner.bannerName,
          Status: newStatus,
        }),
      };
      WebAPI.PostData(param).then((res) => {
        if (res.data.errorCode === 0) {
          GetSiteBannerList(_banner.siteId, CurrencyId);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      });
    }
  };
  const ClickCreateBanner = () => {};
  const ClickEdit = (idx) => {};
  return (
    <div>
      <h4 className="detail">Hot Event Management</h4>
      {siteList == null ? null : (
        <Filter
          Site={siteList}
          ChangeFilter={changeFilter}
          Filter={filter}
          SetSiteId={setSiteId}
        />
      )}
      {bannerList == null ? null : (
        <Result
          bannerList={bannerList}
          UpdateBannerStatus={UpdateBannerStatus}
          ClickCreateBanner={ClickCreateBanner}
          ClickEdit={ClickEdit}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

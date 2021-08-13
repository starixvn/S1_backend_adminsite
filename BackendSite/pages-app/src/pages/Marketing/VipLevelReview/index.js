import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import UpgradeResult from "./UpgradeResult";
import DowngradeResult from "./DowngradeResult";
import * as Loading from "./Loading";

const App = () => {
  const [siteList, setSiteList] = useState([]);

  const [stateFilter, setFilter] = useState({
    VipType: 1,
    SiteId: 1,
    Upgrade_YearMonth: moment(),
    Downgrade_FromYearMonth: moment().add(-3, "M"),
    Downgrade_ToYearMonth: moment().add(-1, "M"),
    Status: 0,
    UserName: "",
  });

  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

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

  const Filter_Downgrade_YearMonthChange = (fromYM, toYM) => {
    setFilter({
      ...stateFilter,
      Downgrade_FromYearMonth: fromYM,
      Downgrade_ToYearMonth: toYM,
    });
  };

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Submit = () => {
    Result_ChangePage(1);
  };

  const Result_ChangePage = (pageNumber) => {
    Loading.OnLoading();
    let apiUrl = "";
    let Data = {};
    if (stateFilter.VipType === 1) {
      apiUrl = "/api/VipLevel/GetVipUpgradeReviewList";
      Data = {
        SiteId: stateFilter.SiteId,
        CalculateYearMonth: moment(stateFilter.Upgrade_YearMonth).format(
          "YYYYMM"
        ),
        Status: parseInt(stateFilter.Status, 10),
        UserName: stateFilter.UserName,
        PageNumber: pageNumber,
        PageSize: 20,
      };
    } else {
      apiUrl = "/api/VipLevel/GetVipDowngradeReviewList";
      Data = {
        SiteId: stateFilter.SiteId,
        CalculateYearMonth:
          moment(stateFilter.Downgrade_FromYearMonth).format("YYYYMM") +
          moment(stateFilter.Downgrade_ToYearMonth).format("YYYYMM"),
        Status: parseInt(stateFilter.Status),
        UserName: stateFilter.UserName,
        PageNumber: pageNumber,
        PageSize: 20,
      };
    }
    api
      .post(apiUrl, Data)
      .then(function (res) {
        Loading.UnLoading();
        if (res.data.errorCode === 0) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: pageNumber,
              rowCount: res.data.rowCount,
            },
            FilterInfo: {
              ...stateFilter,
            },
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
    Result_ChangePage(stateDatas.PageInfo.pageNumber);
  };

  const Result_ExportToExcel = () => {
    let param = {};
    if (stateDatas.FilterInfo.VipType === 1) {
      param = {
        Url: "VipLevel/ExportVipUpgradeReviewList",
        Data: {
          SiteId: stateDatas.FilterInfo.SiteId,
          YearMonth: moment(stateDatas.FilterInfo.Upgrade_YearMonth).format(
            "YYYYMM"
          ),
          Status: parseInt(stateDatas.FilterInfo.Status, 10),
          UserName: stateDatas.FilterInfo.UserName,
        },
      };
    } else {
      param = {
        Url: "VipLevel/ExportVipDowngradeReviewList",
        Data: {
          SiteId: stateDatas.FilterInfo.SiteId,
          CalculateYearMonth:
            moment(stateDatas.FilterInfo.Downgrade_FromYearMonth).format(
              "YYYYMM"
            ) +
            moment(stateDatas.FilterInfo.Downgrade_ToYearMonth).format(
              "YYYYMM"
            ),
          Status: parseInt(stateDatas.FilterInfo.Status, 10),
          UserName: stateDatas.FilterInfo.UserName,
        },
      };
    }

    WebAPI.DownloadData(param);
  };

  const Reject = (props) => {
    let CustIds = [];
    CustIds.push(props.custId);
    UpdateStatus(CustIds, 3);
  };

  const Approved = (props) => {
    let CustIds = [];
    CustIds.push(props.custId);
    UpdateStatus(CustIds, 2);
  };

  const AllApproved = () => {
    let CustIds = [];
    stateDatas.Datas.forEach((x) => {
      if (x["status"] === 1 || x["status"] === 3) {
        CustIds.push(x["custId"]);
      }
    });
    if (CustIds.length === 0) {
      alert("No data can Approved.");
      return false;
    }
    if (window.confirm("Are you sure you want to All Approved?")) {
      UpdateStatus(CustIds, 2);
    }
  };

  const UpdateStatus = (custIds, status) => {
    Loading.OnLoading();
    let apiUrl = "";
    let Data = {};
    if (stateFilter.VipType === 1) {
      apiUrl = "/api/VipLevel/UpdateVipUpgradeReview";
      Data = {
        CustIdList: custIds,
        CalculateYearMonth: moment(
          stateDatas.FilterInfo.Upgrade_YearMonth
        ).format("YYYYMM"),
        SiteId: stateDatas.FilterInfo.SiteId,
        NewStatus: parseInt(status),
      };
    } else {
      apiUrl = "/api/VipLevel/UpdateVipDowngradeReview";
      Data = {
        CustIdList: custIds,
        CalculateYearMonth:
          moment(stateDatas.FilterInfo.Downgrade_FromYearMonth).format(
            "YYYYMM"
          ) +
          moment(stateDatas.FilterInfo.Downgrade_ToYearMonth).format("YYYYMM"),
        SiteId: stateDatas.FilterInfo.SiteId,
        NewStatus: parseInt(status),
      };
    }
    api
      .post(apiUrl, Data)
      .then(function (res) {
        Loading.UnLoading();
        if (res.data.errorCode === 0) {
          alert("Update successful.");
          UpdateReset();
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
      <h4 className="page-name">Vip Level Review</h4>
      <Filter
        SiteList={siteList}
        StateFilter={stateFilter}
        FilterOnChange={Filter_OnChange}
        FilterDowngradeYearMonthChange={Filter_Downgrade_YearMonthChange}
        Submit={Submit}
      />
      {stateDatas.Datas === null ? null : stateDatas.FilterInfo.VipType ===
        1 ? (
        <UpgradeResult
          {...stateDatas}
          AllApproved={AllApproved}
          Approved={Approved}
          ChangePage={Result_ChangePage}
          ExportToExcel={Result_ExportToExcel}
          Reject={Reject}
        />
      ) : (
        <DowngradeResult
          {...stateDatas}
          AllApproved={AllApproved}
          Approved={Approved}
          ChangePage={Result_ChangePage}
          ExportToExcel={Result_ExportToExcel}
          Reject={Reject}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

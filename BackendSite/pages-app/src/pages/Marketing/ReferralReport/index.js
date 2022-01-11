import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import { api } from "../../../module/WebAPI";
import WebAPI from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import Detail from "./Detail";
import moment from "moment-timezone";

const App = () => {
  const [state, setstate] = useState(0);
  const [siteList, setSiteList] = useState();
  const [stateFilter, setFilter] = useState({
    SelectSite: [],
    Type: 0,
    User: "",
    StartDate: "",
    EndDate: "",
  });
  const [custId, setCustId] = useState();

  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });

  useEffect(() => {
    let param = {
      Url: "Common/AdminUserSiteList",
    };
    api.post("/api/Common/AdminUserSiteList").then((res) => {
      if (res.data.errorCode === 0) {
        setSiteList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    stateFilter[dataColumn] = v;
    setFilter({
      ...stateFilter,
    });
  };

  const Filter_CheckBox_OnChange = (v, dataColumn) => {
    let isRemove = stateFilter[dataColumn].includes(v);
    let newChecked = isRemove
      ? stateFilter[dataColumn].filter((value) => value !== v)
      : stateFilter[dataColumn].push(v);
    let currentTab = stateFilter[dataColumn];
    if (newChecked.length === 0) {
      currentTab = [];
    } else if (currentTab === null) {
      currentTab = newChecked;
    }

    if (isRemove && !newChecked.includes(currentTab)) {
      currentTab = newChecked.length === 0 ? [] : newChecked;
    }
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
    if (v !== "All") return false;
    let currentTab =
      stateFilter[dataColumn].length === options.length
        ? []
        : options.map((o) => {
            return o.value;
          });
    setFilter({
      ...stateFilter,
      [dataColumn]: currentTab,
    });
  };

  const Filter_Submit = () => {
    if (stateFilter.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }else if(stateFilter.StartDate && !stateFilter.EndDate ||  !stateFilter.StartDate && stateFilter.EndDate){
      alert("Please Choose both FromDate and ToDate or none");
      return false;
    }
    let Data = {
      SelectSite: stateFilter.SelectSite,
      CurrencyId: parseInt(stateFilter.Type, 10),
      StartDate: stateFilter.StartDate === "" ? null : moment(stateFilter.StartDate).format("YYYY-MM-DD") ,
      EndDate: stateFilter.EndDate === "" ? null : moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      UserName: stateFilter.User,
      PageNumber: 1,
    };
    api
      .post("/api/ReferralManagement/GetReferralCustList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setDatas({
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: 1,
              rowCount: res.data.rowCount,
            },
            Datas: [...res.data.data],
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

  const Result_ChangePage = (PageNumber) => {
    if (stateDatas.FilterInfo.SelectSite.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }else if(stateFilter.StartDate && !stateFilter.EndDate ||  !stateFilter.StartDate && stateFilter.EndDate){
      alert("Please Choose both FromDate and ToDate or none");
      return false;
    }
    let Data = {
        SelectSite: stateFilter.SelectSite,
        CurrencyId: parseInt(stateFilter.Type, 10),
        StartDate: stateFilter.StartDate === "" ? null : moment(stateFilter.StartDate).format("YYYY-MM-DD") ,
        EndDate: stateFilter.EndDate === "" ? null : moment(stateFilter.EndDate).format("YYYY-MM-DD"),
        UserName: stateFilter.User,
        PageNumber: PageNumber,
      };
      api
        .post("/api/ReferralManagement/GetReferralCustList", Data)
        .then(function (res) {
          if (res.data.errorCode === 0) {
            setDatas({
                ...stateDatas,
                PageInfo: {
                  ...stateDatas.PageInfo,
                  pageNumber: PageNumber,
                },
                Datas: [...res.data.data],
              });
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
  };

  const Result_ExportToExcel = () => {
    let param = {};
      param = {
        Url: "ReferralManagement/GetReferralCustListExport",
        Data: {
          SelectSite: JSON.stringify(stateFilter.SelectSite) ,
          CurrencyId: parseInt(stateFilter.Type, 10),
          StartDate: stateFilter.StartDate === "" ? null : moment(stateFilter.StartDate).format("YYYY-MM-DD") ,
          EndDate: stateFilter.EndDate === "" ? null : moment(stateFilter.EndDate).format("YYYY-MM-DD"),
          UserName: stateFilter.User,
        },
      };
      
    WebAPI.DownloadData(param);
  };
  
  const Click_Detail = (prop) => {
    setCustId(prop.custId);
    setstate(1);
  };

  const Click_Back = () => {
    setstate(0);
  };

  if (state === 0) {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Referral Report</h4>
        <Filter
          filterInfo={stateFilter}
          onChange={Filter_OnChange}
          onChangeCheckBox={Filter_CheckBox_OnChange}
          onChangeAllCheckBox={Filter_CheckBox_CheckAll}
          ClickSubmit={Filter_Submit}
          SiteList={siteList}
        />
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            Result_ExportToExcel={Result_ExportToExcel}
            ChangePage={Result_ChangePage}
            Detail={Click_Detail}
          />
        )}
      </div>
    );
  } else {
    return (
      <div>
        <h4 className="detail">Referral Report History</h4>
        <Detail CustId={custId} Filter={stateFilter} />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="back"
            onClick={Click_Back}
            style={{
              marginTop: "20px",
              marginBottom: "40px",
              marginRight: "10px",
            }}
          >
            Back
          </button>
        </div>
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

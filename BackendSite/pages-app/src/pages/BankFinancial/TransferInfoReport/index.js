import React, { useState, useRef, useEffect } from "react";
import ReactDOM from "react-dom";
import Filter from "./Filter";
import Result from "./Result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import moment from "moment";

const App = () => {
  const [stateFilter, setFilter] = useState({
    SiteId: 0,
    AccountNo: null,
    StartDate: moment().add(-14, "days").format("l"),
    EndDate: moment().format("l"),
  });

  const [stateDatas, setDatas] = useState({
    Datas: null,
    FilterInfo: null,
    PlusNum: 0,
    MinusNum: 0,
    FeeNum: 0,
  });

  const [siteList, setSiteList] = useState([]);
  const [accountList, setAccountList] = useState([]);
  const [newAccountList, setNewAccountList] = useState([]);
  const [SubmitStatus, setSubmitStatus] = useState(false);
  const [ExportStatus, setExportStatus] = useState(false);

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

  useEffect(() => {
    let GetPayListParam = {
      Url: "TransferManage/GetTransferAccountList",
    };
    WebAPI.PostData(GetPayListParam).then((res) => {
      if (res.data.errorCode === 0) {
        setAccountList(res.data.data);
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  const Filter_SiteId_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
      AccountNo: null,
    });
    let newAccountList = [];
    accountList.map((s) => {
      if (s.siteId == v) {
        newAccountList.push({
          displayName: s.displayName,
          accountNo: s.accountNo,
          siteId: s.siteId,
          siteName: s.siteName,
        });
      }
    });
    setNewAccountList(newAccountList);
  };

  const Filter_Submit = () => {
    if (stateFilter.SiteId === 0) {
      alert("Please Choose Site.");
      return false;
    }
    if (stateFilter.AccountNo === null) {
      alert("Please Choose Account.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose Start Date.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose End Date.");
      return false;
    }

    let param = {
      Url: "BankFinancialReport/GetTransferInfoReport",
      Data: {
        SiteId: Number(stateFilter.SiteId),
        AccountNo: stateFilter.AccountNo,
        StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
        EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      },
    };

    setSubmitStatus(true);
    setExportStatus(true);

    WebAPI.PostData(param).then((res) => {
      setSubmitStatus(false);
      setExportStatus(false);

      if (res.data.errorCode === 0) {
        let plusNum = 0;
        let minusNum = 0;
        let feeNum = 0;

        res.data.data.forEach((element) => {
          if (element.amount > 0) {
            plusNum = plusNum + element.amount;
          } else {
            minusNum = minusNum + element.amount;
          }
          feeNum = feeNum + element.fee;
        });
        console.log(plusNum);
        console.log(minusNum);
        console.log(feeNum);
        setDatas({
          Datas: res.data.data,
          FilterInfo: {
            ...stateFilter,
          },
          PlusNum: plusNum,
          MinusNum: minusNum,
          FeeNum: feeNum,
        });
      } else {
        alert(res.data.message ?? "failed to get data.");
      }
    });
  };

  const ExportToExcel = () => {
    if (stateFilter.SiteId === 0) {
      alert("Please Choose Site.");
      return false;
    }
    if (stateFilter.AccountNo === null) {
      alert("Please Choose Account.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose Start Date.");
      return false;
    }
    if (!moment(stateFilter.SelectDate).unix()) {
      alert("Please Choose End Date.");
      return false;
    }

    setSubmitStatus(true);
    setExportStatus(true);

    let param = {
      Url: "BankFinancialReport/ExportTransferInfoReport",
      Data: {
        SiteId: Number(stateFilter.SiteId),
        AccountNo: stateFilter.AccountNo,
        StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
        EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      },
    };

    WebAPI.DownloadData(param);

    setTimeout(() => {
      setSubmitStatus(false);
      setExportStatus(false);
    }, 5000);
  };

  return (
    <div className="container-fluid" style={{ width: "auto" }}>
      <h4 className="page-name">Transfer Info Report</h4>
      <Filter
        filterInfo={stateFilter}
        onChange={Filter_OnChange}
        siteIdOnChange={Filter_SiteId_OnChange}
        ClickSubmit={Filter_Submit}
        SiteList={siteList}
        AccountList={newAccountList}
        SubmitBtnDisabled={SubmitStatus}
      />
      {stateDatas.Datas === null ? null : (
        <Result
          {...stateDatas}
          ExportToExcel={ExportToExcel}
          ExportBtnDisabled={ExportStatus}
        />
      )}
    </div>
  );
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";
import Filter from "./Filter";
import Result from "./Result";
import DetailFilterPage from "./DetailFilter";
import Detail from "./Detail";
import Accounting from "./Accounting";

const App = () => {
  const [siteList, setSiteList] = useState([]);
  const [SubmitStatus, setSubmitStatus] = useState(false);
  const [DetailStatus, setDetailStatus] = useState(false);
  const [PopoutStatus, setPopoutStatus] = useState(false);
  const [stateFilter, setFilter] = useState({
    SiteIds: [],
    CurrencyIds: [],
    StartDate: moment().add(-10, "days").format("l"),
    EndDate: moment().format("l"),
    TransId: "",
    UserName: "",
  });
  const [stateDatas, setDatas] = useState({
    PageInfo: null,
    Datas: null,
    FilterInfo: null,
  });
  const [Page, setPage] = useState("main");
  const [transIdData, settransIdData] = useState({
    Withdrawal: null,
    AccountingDatas: null,
  });
  const [accountingData, setAccountingData] = useState(null);
  const [bankList, setBankList] = useState([]);

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

  const Filter_OnChange = (v, dataColumn) => {
    setFilter({
      ...stateFilter,
      [dataColumn]: v,
    });
  };

  //[Main] > [Submit]
  //Get Withdrawal Summary Data For Show in Main Page.
  const Filter_Submit = () => {
    Result_ChangePage(1);
  };

  //[Main] > [PageChange]
  //Get Withdrawal Summary Data For Show in Main Page.
  const Result_ChangePage = (PageNumber) => {
    if (stateFilter.SiteIds.length === 0) {
      alert("Please Choose at least one Site");
      return false;
    }
    if (stateFilter.CurrencyIds.length === 0) {
      alert("Please Choose at least one Currency");
      return false;
    }
    if (!moment(stateFilter.StartDate).unix()) {
      alert("Please Choose Start Date");
      return false;
    }
    if (!moment(stateFilter.EndDate).unix()) {
      alert("Please Choose End Date");
      return false;
    }

    setSubmitStatus(true);
    setDetailStatus(true);

    const siteIds = [];
    stateFilter.SiteIds.map((o) => {
      siteIds.push(o.value);
    });

    const currencyIds = [];
    stateFilter.CurrencyIds.map((o) => {
      currencyIds.push(Number(o.value));
    });

    let Data = {
      SiteIds: siteIds,
      CurrencyIds: currencyIds,
      StartDate: moment(stateFilter.StartDate).format("YYYY-MM-DD"),
      EndDate: moment(stateFilter.EndDate).format("YYYY-MM-DD"),
      TransId: stateFilter.TransId,
      UserName: stateFilter.UserName,
      PageNumber: PageNumber,
      PageSize: 10,
    };
    api
      .post(
        "/api/WithdrawalBankAccounting/GetWithdrawalInfoAndAccountingList",
        Data
      )
      .then(function (res) {
        setSubmitStatus(false);
        setDetailStatus(false);

        if (res.data.errorCode === 0 || res.data.errorCode === 1) {
          setDatas({
            ...stateDatas,
            Datas: res.data.data,
            PageInfo: {
              totalPages: res.data.totalPages,
              pageNumber: PageNumber,
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

  //[Main] > [Detail]
  //Get Withdrawal Data For Show in Withdrawal Page.
  const Click_Detail = (transId) => {
    let Data = {
      TransId: transId,
    };
    api
      .post(
        "/api/WithdrawalBankAccounting/GetWithdrawalInfoAndAccountingByTransId",
        Data
      )
      .then(function (res) {
        if (res.data.errorCode === 0) {
          settransIdData({
            ...transIdData,
            Withdrawal: res.data.data.withdrawal,
            AccountingDatas: res.data.data.accountingList,
          });
          setPage("withdrawal");
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  //[Withdrawal] > [Create]
  //Get Default Value Before Create.
  const Create_Accounting = (props) => {
    let maxSeq = 0;
    if (props.AccountingDatas.length > 0) {
      maxSeq = Math.max.apply(
        Math,
        props.AccountingDatas.map(function (o) {
          return o.seq;
        })
      );
    }
    console.log(maxSeq);
    let Data = {
      TransId: props.Withdrawal.transId,
      MaxSeq: maxSeq,
    };
    api
      .post(
        "/api/WithdrawalBankAccounting/CreateWithdrawalBankAccounting",
        Data
      )
      .then(function (res) {
        if (res.data.errorCode === 0) {
          //res.data.data.hasPicture = false;
          setAccountingData(res.data.data);
          Accounting_BankList_Get(transIdData.Withdrawal);
          setPage("accounting");
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  //[Withdrawal] > [GoBack]
  //Go Back From Withdrawal To Main.
  const GoBackToMain = () => {
    Result_ChangePage(1);
    setPage("main");
  };

  //[Withdrawal] > [Edit]
  //Get Previous Value Before Edit.
  const Click_Accounting_Edit = (id) => {
    let Data = {
      Id: id,
    };
    api
      .post("/api/WithdrawalBankAccounting/GetWithdrawalBankAccounting", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          //res.data.data.hasPicture = false;
          setAccountingData(res.data.data);
          Accounting_BankList_Get(transIdData.Withdrawal);
          setPage("accounting");
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  //[Withdrawal] > [Delete]
  //Submit Delete Accounting Data.
  const Delete_Accounting = (data) => {
    console.log(data);
    let Data = {
      Id: data.id,
    };
    if (window.confirm(`Are you sure to delete data ?`)) {
      api
        .post("/api/WithdrawalBankAccounting/DeleteWithdrawalAccounting", Data)
        .then(function (res) {
          if (res.data.errorCode === 0) {
            alert("Delete Successful!");
            GoBackToWithdrawal(data.transId);
          } else {
            alert(res.data.message ?? "failed to get data.");
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  //[Accounting] > [BankAccountNo]
  //Get Bank Data For DropDownList
  const Accounting_BankList_Get = (data) => {
    let Data = {
      CurrencyId: data.currencyId,
      SysId: 0,
      SiteId: data.siteId,
    };
    api
      .post("/api/Withdrawal/GetBankInfoOffList", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          setBankList(res.data.data);
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  //[Accounting] > [Submit]
  //Submit Data to server when Create or Update.
  const Set_Accounting = (props) => {
    let Data = {
      Id: props.id,
      TransId: props.transId,
      Seq: props.seq,
      BankAccountNo: props.bankAccountNo,
      Amount: Number(props.amount),
      ChargeFee: Number(props.chargeFee),
      Memo: props.memo,
      Creator: props.creator,
      CreateTime: props.createTime,
      Operator: props.operator,
      OperateTime: props.operateTime,
    };
    if (props.id == 0) {
      //Insert
      if (window.confirm(`Are you sure to Create data ?`)) {
        api
          .post(
            "/api/WithdrawalBankAccounting/InsertWithdrawalAccounting",
            Data
          )
          .then(function (res) {
            if (res.data.errorCode === 0) {
              alert("Create Successful!");
              GoBackToWithdrawal(props.transId);
              //DetailSubmit();
            } else {
              alert(res.data.message ?? "failed to get data.");
            }
          })
          .catch(function (error) {
            alert(error);
          });
      }
    } else {
      //Update
      if (window.confirm(`Are you sure to update data ?`)) {
        api
          .post(
            "/api/WithdrawalBankAccounting/UpdateWithdrawalAccounting",
            Data
          )
          .then(function (res) {
            if (res.data.errorCode === 0) {
              alert("Update Successful!");
              GoBackToWithdrawal(props.transId);
              //DetailSubmit();
            } else {
              alert(res.data.message ?? "failed to get data.");
            }
          })
          .catch(function (error) {
            alert(error);
          });
      }
    }
  };

  //[Accounting] > [GoBack]
  //Go Back From Accounting To Withdrawal.
  const GoBackToWithdrawal = (transId) => {
    Click_Detail(transId);
    setPage("withdrawal");
  };

  //[Withdrawal] > [PicturePopout] > [Close]
  //Go Back From PicturePopout To Withdrawal.
  const PicturePopout_Close = () => {
    setAccountingData(null);
    setPopoutStatus(false);
  };

  //[Withdrawal] > [PicturePopout] > [Open]
  //Open Accounting Picture From Withdrawal.
  const PicturePopout_Open = (data) => {
    console.log("PicturePopout", data);

    let Data = {
      TransId: data.transId,
      Seq: data.seq,
      UploadPicTimes: data.uploadPicTimes,
    };
    api
      .post("/api/WithdrawalBankAccounting/CheckAccountingPicture", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          data.hasPicture = res.data.data;
          setAccountingData(data);
          setPopoutStatus(true);
        } else {
          alert(res.data.message ?? "check picture error.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  if (Page === "main") {
    return (
      <div className="container-fluid">
        <h4 className="page-name">Withdrawal Accounting</h4>
        {siteList === null ? null : (
          <Filter
            SiteList={siteList}
            filterInfo={stateFilter}
            SetFilter={setFilter}
            onChange={Filter_OnChange}
            ClickSubmit={Filter_Submit}
            SubmitBtnDisabled={SubmitStatus}
          />
        )}
        {stateDatas.Datas === null ? null : (
          <Result
            {...stateDatas}
            DetailBtnDisabled={DetailStatus}
            ChangePage={Result_ChangePage}
            Detail={Click_Detail}
          />
        )}
      </div>
    );
  } else if (Page == "withdrawal") {
    return (
      <div className="container-fluid">
        <DetailFilterPage
          TransIdData={transIdData}
          Create={Create_Accounting}
          GoBack={GoBackToMain}
        />
        <Detail
          AccountingData={transIdData.AccountingDatas}
          PopOutData={accountingData}
          PopOutStatus={PopoutStatus}
          GetById={Click_Accounting_Edit}
          Delete={Delete_Accounting}
          PopOut_Open={PicturePopout_Open}
          PopOut_Close={PicturePopout_Close}
          test={setAccountingData}
        />
      </div>
    );
  } else if (Page == "accounting") {
    return (
      <div className="container-fluid">
        <Accounting
          BankList={bankList}
          AccountingData={accountingData}
          ForCaculateDatas={transIdData}
          GoBack={GoBackToWithdrawal}
          Submit={Set_Accounting} //Insert And Update Submit
        />
      </div>
    );
  }
};

export default App;

if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

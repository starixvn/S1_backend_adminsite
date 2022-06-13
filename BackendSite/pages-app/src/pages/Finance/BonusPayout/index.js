import React, { useEffect, useRef, useReducer, useState } from "react";
import ReactDOM from "react-dom";
import WebAPI from "../../../module/WebAPI";
import moment from "moment";
import { api } from "../../../module/WebAPI";
import Filter from "./Filter";
import Result from "./Result";
import { PopBoxContent } from "./PopBoxContent";

const App = () => {
  const [siteList, setSiteList] = useState([]);

  const [stateFilter, setFilter] = useState({
    SiteId: 1,
    SabaId: "",
    UserName: "",
    BonusType: 1,
    BonusCode: "",
  });
  const [custData, setcustData] = useState(null);
  const [bonusData, setbonusData] = useState({
    CheckedBonusCode: "",
    Amount: 1,
    RollOver: 1,
    ProductCategory: "",
    WalletType: 0,
  });
  const [adjustmentLimitMAX, setadjustmentLimitMAX] = useState(0);
  const [needCheckPassword, setneedCheckPassword] = useState(false);
  const [hasCheckPassword, sethasCheckPassword] = useState(false);

  useEffect(() => {
    api
      .post("/api/Common/AdminUserSiteList")
      .then(function (response) {
        if (response.data.errorCode === 0) {
          setSiteList(response.data.data);
        } else {
          alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }, []);

  const Filter_OnChange = (v, dataColumn) => {
    if (dataColumn === "BonusType") {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
        BonusCode: "",
      });
      setbonusData({
        CheckedBonusCode: "",
        Amount: 1,
        RollOver: 1,
        ProductCategory: "",
        WalletType: 0,
      });
    } else {
      setFilter({
        ...stateFilter,
        [dataColumn]: v,
      });
    }
  };

  const Submit = () => {
    let msg = "";
    let check = true;

    if (!stateFilter.SabaId && !stateFilter.UserName) {
      check = false;
      msg = "Please input SabaId or UserName.";
    }

    if (!stateFilter.SiteId) {
      check = false;
      msg = "Please select Site.";
    }

    if (check) {
      axios
        .post("/api/CustManage/GetCustInfoDetails", {
          SabaId: stateFilter.SabaId,
          UserName: stateFilter.UserName,
          SiteId: stateFilter.SiteId,
        })
        .then(
          function (response) {
            //Loading.UnLoading();
            let result = response.data;
            if (result.errorCode == 0) {
              setcustData(result.data);
              GetAdjustmentLimitMAX(result.data.siteId, result.data.currencyId);
            } else {
              alert(result.message);
              setcustData(null);
            }
          }.bind(this)
        )
        .catch(
          function (error) {
            //Loading.UnLoading();
            alert(error);
          }.bind(this)
        );
    } else {
      alert(msg);
    }
  };

  const GetAdjustmentLimitMAX = (siteId, currencyId) => {
    axios
      .post("/api/Payment/GetAdjustmentData", {
        siteId: siteId,
        currencyId: currencyId,
      })
      .then(
        function (response) {
          //Loading.UnLoading();
          let result = response.data;
          if (result.errorCode == 0) {
            if (result.data) {
              setadjustmentLimitMAX(result.data.adjustmentMax);
            } else {
              setadjustmentLimitMAX(0);
            }
          } else {
            alert(result.message);
          }
        }.bind(this)
      )
      .catch(
        function (error) {
          //Loading.UnLoading();
          alert(error);
        }.bind(this)
      );
  };

  const Check = () => {
    let msg = "";
    let check = true;

    if (!stateFilter.BonusCode || stateFilter.BonusCode === "") {
      check = false;
      msg = "Please input BonusCode.";
    }
    if (check) {
      axios
        .post("/api/Payment/CheckBalanceAdjustBonusCode", {
          BonusCode: stateFilter.BonusCode,
          PromotionType: stateFilter.BonusType === 1 ? 9 : 8,
          SiteId: stateFilter.SiteId,
          CurrencyId: custData.currencyId,
        })
        .then(
          function (response) {
            //Loading.UnLoading();
            let result = response.data;
            if (result.errorCode == 0) {
              if (!result.data.hasBonusCode) {
                setbonusData({
                  CheckedBonusCode: "",
                  RollOver: 1,
                  Amount: 1,
                  ProductCategory: "",
                  WalletType: 0,
                });
                alert("BonusCode Error! Please Check Again");
              } else {
                if (stateFilter.BonusType === 1) {
                  setbonusData({
                    CheckedBonusCode: result.data.bonusCode,
                    RollOver: result.data.rollOver,
                    Amount: result.data.bonus,
                    ProductCategory: result.data.name,
                    WalletType: result.data.walletType,
                  });
                } else {
                  setbonusData({
                    CheckedBonusCode: result.data.bonusCode,
                    RollOver: result.data.rollOver,
                    ProductCategory: result.data.name,
                    WalletType: result.data.walletType,
                    Amount: 1,
                  });
                }
                alert("BonusCode " + result.data.bonusCode + " exist");
              }
            } else {
              alert(result.message);
            }
          }.bind(this)
        )
        .catch(
          function (error) {
            //Loading.UnLoading();
            alert(error);
          }.bind(this)
        );
    } else {
      alert(msg);
    }
  };

  const BonusInfo_Onchange = (v, dataColumn) => {
    setbonusData({
      ...bonusData,
      [dataColumn]: v,
    });
  };

  const SubmitBonusPayout = () => {
    if (!bonusData.Amount || parseInt(bonusData.Amount) === 0) {
      alert("Please Enter Amount(cannot be 0)");
      return;
    }
    if (
      bonusData.Amount > adjustmentLimitMAX &&
      adjustmentLimitMAX !== 0 &&
      !hasCheckPassword
    ) {
      setneedCheckPassword(true);
      return;
    }
    axios
      .post("/apiv2/BonusPayout/SetBonusPayout", {
        CustId: custData.custId,
        Amount: bonusData.Amount,
        BounsCode: bonusData.CheckedBonusCode,
        RollOver: bonusData.RollOver,
        PromotionType: stateFilter.BonusType == 1 ? 9 : 8,
      })
      .then(
        function (response) {
          //Loading.UnLoading();
          let result = response.data;
          if (result.errorCode == 0) {
            alert(result.message);
          } else {
            alert(result.message);
          }
        }.bind(this)
      )
      .catch(
        function (error) {
          //Loading.UnLoading();
          alert(error);
        }.bind(this)
      );
  };

  const CheckPassWordSuccess = () => {
    setneedCheckPassword(false);
    sethasCheckPassword(true);

    axios
      .post("/apiv2/BonusPayout/SetBonusPayout", {
        CustId: custData.custId,
        Amount: bonusData.Amount,
        BounsCode: bonusData.CheckedBonusCode,
        RollOver: bonusData.RollOver,
        PromotionType: stateFilter.BonusType == 1 ? 9 : 8,
      })
      .then(
        function (response) {
          //Loading.UnLoading();
          let result = response.data;
          if (result.errorCode == 0) {
            alert(result.message);
          } else {
            alert(result.message);
          }
        }.bind(this)
      )
      .catch(
        function (error) {
          //Loading.UnLoading();
          alert(error);
        }.bind(this)
      );
  };
  const Cancel = () => {
    setneedCheckPassword(false);
  };

  return (
    <div className="container-fluid">
      <h4 className="page-name">Bonus Payout</h4>
      <Filter
        SiteList={siteList}
        StateFilter={stateFilter}
        FilterOnChange={Filter_OnChange}
        Submit={Submit}
      />
      {custData === null ? null : (
        <Result
          CustData={custData}
          StateFilter={stateFilter}
          FilterOnChange={Filter_OnChange}
          Check={Check}
          BonusData={bonusData}
          BonusInfoOnchange={BonusInfo_Onchange}
          Submit={SubmitBonusPayout}
        />
      )}
      {needCheckPassword ? (
        <PopBoxContent
          ClickCancel={Cancel}
          SiteId={custData.siteId}
          CheckPassWordSuccess={CheckPassWordSuccess}
          Amount={bonusData.Amount}
          Currency={custData.currencyId}
        />
      ) : null}
    </div>
  );
};

export default App;
if (process.env.NODE_ENV === "production") {
  ReactDOM.render(<App />, document.getElementById("content"));
}

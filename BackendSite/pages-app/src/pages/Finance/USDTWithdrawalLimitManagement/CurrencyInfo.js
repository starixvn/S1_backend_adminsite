import React, { useState, useEffect } from "react";
import { LevelInfo } from "./LevelInfo";
import { PopBoxContentAddLevel } from "./PopBoxContentAddLevel";
import { api } from "../../../module/WebAPI";

if (process.env.NODE_ENV !== "production") {
  window.i18n = {
    lbl_Currency1: "SGD",
    lbl_Currency2: "MYR",
    lbl_Currency3: "USD",
    lbl_Currency4: "THB",
    lbl_Currency6: "EURO",
    lbl_Currency9: "AUD",
    lbl_Currency12: "GBP",
    lbl_Currency13: "CNY",
    lbl_Currency15: "IDR",
    lbl_Currency20: "UUS",
    lbl_Currency32: "JPY",
    lbl_Currency40: "WON",
    lbl_Currency41: "CHF",
    lbl_Currency45: "WON",
    lbl_Currency51: "VND",
    lbl_Currency61: "INR",
    lbl_Save: "Save",
    lbl_Success: "Success",
    lbl_Action: "Action",
    lbl_SelectSite: "Select site",
    lbl_Point: "Point",
    lbl_DepositLeast: "Deposit least",
    lbl_BetLeast: "Bet least",
    lbl_AddPointLevel: "Add point level",
    lbl_AddNewPointLevel: "Add new point level",
    lbl_Delete: "Delete",
    lbl_AreYouSure: "Are you sure?",
    lbl_OK: "OK",
    lbl_Cancel: "Cancel",
    lbl_Error: "Error",
    lbl_Currency61: "INR",
  };
}

export const CurrencyInfo = (props) => {
    console.log(props,'CurrencyInfo');
  let CurrencyList = props.optSite.siteCurrencyList.map((value) => {
    return (
      <div
        key={`Site-${props.optSite.siteId}-CurrencyInfo-${value}-Network-${props.optSite.Network}`}
        className="panel mt-20"
      >
        <a
          className="panel-title"
          data-toggle="collapse"
          href={`#collapseExample-${value}`}
          role="button"
          aria-expanded="true"
          aria-controls="collapseExample"
        >
          {window.i18n["lbl_Currency" + value]}
        </a>
        <Result siteId={props.optSite.siteId} currId={value} networkType={props.optSite.Network}/>
      </div>
    );
  });
  return CurrencyList;
};

const Result = (props) => {
  const [stateEdit, setEdit] = useState({});
  const [editValue, setEditValue] = useState(null);
  const [isOpen, setOpen] = useState(false);
  const [levelList, setLevelList] = useState(null);
  const handleOpen = (_isOpen) => {
    setOpen(_isOpen);
  };

  const handleAddLevelList = (_newList) => {
    setLevelList([...levelList, _newList]);
    setOpen(false);
  };

  const handleEdit = (idx) => {
    let isEdit = stateEdit[idx] === true ? false : true;
    if (editValue !== null) {
      setEditValue(null);
    }
    setEdit({
      [idx]: isEdit,
    });
  };

  const handleSetLevelList = (idx, status, withdrawalMin, withdrawalMax) => {
    let tmp = [...levelList];
    if (status === "Delete") {
      tmp.splice(idx, 1);
      setLevelList(tmp);
    } else if (status === "Update") {
      tmp[idx].withdrawalMin = withdrawalMin;
      tmp[idx].withdrawalMax = withdrawalMax;
      setLevelList(tmp);
    }
  };

  const handleDelete = (idx) => {
    if (window.confirm("您真的確定要刪除嗎？\n\n請確認！") === true) {
      api
        .post("/api/Withdrawal/DeleteUSDTWithdrawalLimitInfo", {
          siteId: levelList[idx].siteId,
          currencyId: levelList[idx].currencyId,
          vipLevel: levelList[idx].vipLevel,
          networkType: levelList[idx].networkType
        })
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLevelList(idx, "Delete");
            alert(response.data.message);
          } else {
            alert(response.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const handleUpdate = (idx) => {
    if (editValue !== null) {
      let withdrawalMin =
        typeof editValue.withdrawalMin === "undefined"
          ? levelList[idx].withdrawalMin
          : editValue.withdrawalMin;
      let withdrawalMax =
        typeof editValue.withdrawalMax === "undefined"
          ? levelList[idx].withdrawalMax
          : editValue.withdrawalMax;
      api
        .post("/api/Withdrawal/EditUSDTWithdrawalLimitInfo", {
          siteId: levelList[idx].siteId,
          currencyId: levelList[idx].currencyId,
          vipLevel: levelList[idx].vipLevel,
          networkType: levelList[idx].networkType,
          withdrawalMin: parseInt(withdrawalMin),
          withdrawalMax: parseInt(withdrawalMax),
        })
        .then(function (response) {
          if (response.data.errorCode === 0) {
            handleSetLevelList(idx, "Update", withdrawalMin, withdrawalMax);
            alert(response.data.message);
            handleEdit(idx);
          } else {
            alert(response.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const ChangeData = (idx, _locate, _newData) => {
    setEditValue({
      ...editValue,
      idx: idx,
      [_locate]: _newData,
    });
  };

  useEffect(() => {
    let mounted = true;
    api
      .post("/api/Withdrawal/GetUSDTWithdrawalLimitInfos", {
        siteId: props.siteId,
        currencyId: props.currId,
        networkType: parseInt(props.networkType,10)
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          if (mounted) {
            setLevelList(response.data === null ? [] : response.data.data);
          }
        } else {
          // alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
    return () => (mounted = false);
  }, []);

  if (levelList === null) {
    return <></>;
  } else {
    return (
      <div className="panel-body">
        <div className="collapse show" id={`collapseExample-${props.currId}`}>
          <div className="table">
            <div className="table-options">
              <button
                className="btn btn-add"
                onClick={handleOpen.bind(null, true)}
              >
                Add point level
              </button>
            </div>
            <LevelInfo
              LevelList={levelList}
              handleEdit={handleEdit}
              handleUpdate={handleUpdate}
              handleDelete={handleDelete}
              EditValue={stateEdit}
              ChangeData={ChangeData}
            />
          </div>
          {isOpen ? (
            <PopBoxContentAddLevel
              ClickCancel={handleOpen.bind(null, false)}
              LevelList={levelList}
              SiteId={props.siteId}
              CurrencyId={props.currId}
              ClickSave={handleAddLevelList}
              NetworkType={props.networkType}
            />
          ) : null}
        </div>
      </div>
    );
  }
};

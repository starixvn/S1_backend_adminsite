import React, { useState, useEffect } from "react";
import { api } from "../../../module/WebAPI";
import { PopBoxContentAdd } from "./PopBoxContentAdd";

if (process.env.NODE_ENV !== "production") {
  window.i18n = {
    lbl_Currency1: "SGD",
    lbl_Currency2: "MYR",
    lbl_Currency3: "USD",
    lbl_Currency4: "THB",
    lbl_Currency6: "EURO",
    lbl_Currency9: "AUD",
    lbl_Currency12: "GBP",
    lbl_Currency13: "RMB",
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
  let CurrencyList = props.optSite.siteCurrencyList.map((value) => {
    return (
      <div key={`CurrencyInfo-${value}`} className="panel mt-20">
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
        <Result siteId={props.optSite.siteId} currId={value} />
      </div>
    );
  });
  return CurrencyList;
};
const Result = (props) => {
  const [currnecyData, setcurrnecyData] = useState(null);
  const [isOpen, setOpen] = useState(false);
  let row = "";

  const handleOpen = (_isOpen) => {
    setOpen(_isOpen);
  };

  useEffect(() => {
    let mounted = true;
    api
      .post("/api/Payment/GetAdjustmentData", {
        siteId: props.siteId,
        currencyId: props.currId,
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          if (mounted) {
            setcurrnecyData(response.data);
            // setLevelList(response.data === null ? [] : response.data.data);
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

  if (currnecyData === null) {
    row = (
      <div className="table-options">
        <button className="btn btn-add" onClick={handleOpen.bind(null, true)}>
          Add point level
        </button>
      </div>
    );
  } else {
    row = <></>;
  }
  return (
    <div className="panel-body">
      <div className="collapse show" id={`collapseExample-${props.currId}`}>
        <div className="table">
          <div className="table-options">
            <button
              className="btn btn-add"
              onClick={handleOpen.bind(null, true)}
            >
              Add
            </button>
          </div>
        </div>
        {isOpen ? <PopBoxContentAdd ClickCancel={handleOpen.bind(null, false)}  SiteId={props.siteId} CurrencyId={props.currId} ClickSave={handleAddLevelList} /> : null}
      </div>
    </div>
  );
};

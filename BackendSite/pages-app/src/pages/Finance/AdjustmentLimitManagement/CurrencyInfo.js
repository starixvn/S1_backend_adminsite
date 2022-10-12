import React, { useState, useEffect } from "react";
import { api } from "../../../module/WebAPI";
import { PopBoxContentAdd } from "./PopBoxContentAdd";
import { number_format } from "../../../module/Formatting/NumberFormat";

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
    lbl_Currency74: "BDT",
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
      <div key={`Site-${props.optSite.siteId}-CurrencyInfo-${value}`} className="panel mt-20">
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

const InputBox = (props) => {
  return props.isEdit ? (
    <input
      type="number"
      defaultValue={props.defaultValue}
      onChange={(e) => {
        let value = e.target.value;
        if (!isNaN(value)) {
          props.onChange(e.target.value);
        }
      }}
    />
  ) : (
    number_format(props.defaultValue)
  );
};

const Result = (props) => {
  const [currnecyData, setcurrnecyData] = useState(null);
  const [isOpen, setOpen] = useState(false);
  const [editValue, setEditValue] = useState(null);
  const [origValue, setOrigValue] = useState(null);
  const [isEdit, setEdit] = useState(false);
  let row = "";

  const handleOpen = (_isOpen) => {
    setOpen(_isOpen);
  };

  const handleAdd = (_newList) => {
    setOpen(false);
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
            if (response.data.data) {
              setcurrnecyData(response.data.data);
              setOrigValue(response.data.data.adjustmentMax);
            }
          }
        } else {
          alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
    return () => (mounted = false);
  }, []);

  const ReGetData = () => {
    api
      .post("/api/Payment/GetAdjustmentData", {
        siteId: props.siteId,
        currencyId: props.currId,
      })
      .then(function (response) {
        if (response.data.errorCode === 0) {
          if (response.data.data) {
            setcurrnecyData(response.data.data);
          }
        } else {
          alert(response.message);
        }
      })
      .catch(function (error) {
        alert(error);
      });
  };

  const EditMax = (e) => {
    setEditValue(e);
  };

  const EditClick = () => {
    setEdit(true);
  };

  const EditSubmit = () => {
    if (editValue !== null) {
      api
        .post("/api/Payment/AddAdjustmentLimitInfo", {
          siteId: props.siteId,
          currencyId: props.currId,
          before:{
            adjustmentMax: origValue
          },
          after:{
            adjustmentMax: parseFloat(editValue)
          }
        })
        .then(function (response) {
          if (response.data.errorCode === 0) {
            setEditValue(null);
            setEdit(false);
            ReGetData();
          } else {
            alert(response.data.message);
          }
        })
        .catch(function (error) {
          alert(error);
        });
    }
  };

  const EditCancel = () => {
    setEdit(false);
    setEditValue(null);
  };
  const CreateCancel = () => {
    handleOpen(false);
  };
  if (currnecyData === null) {
    row = (
      <div className="table-options">
        <button className="btn btn-add" onClick={handleOpen.bind(null, true)}>
          Add
        </button>
      </div>
    );
  } else {
    let tempAdjustmentMax =
      editValue === null ? currnecyData.adjustmentMax : editValue;
    let List = (
      <tr key={`${currnecyData.currencyId}`}>
        <td>
          <InputBox
            isEdit={isEdit}
            defaultValue={tempAdjustmentMax}
            onChange={EditMax}
          />
        </td>
        <td>
          {isEdit ? (
            <div className="button-action">
              <button className="btn btn-save" onClick={EditSubmit}>
                <i className="icon-save"></i>
              </button>
              <button className="btn btn-icon-cancel" onClick={EditCancel}>
                <i className="icon-cancel"></i>
              </button>
            </div>
          ) : (
            <div className="button-action">
              <button className="btn btn-edit" onClick={EditClick}>
                <i className="icon-edit"></i>
              </button>
            </div>
          )}
        </td>
      </tr>
    );
    row = (
      <table className="table table-striped">
        <thead>
          <tr>
            <th scope="col">Adjust Max (0 = Unlimited)</th>
            <th scope="col">Action</th>
          </tr>
        </thead>
        <tbody>{List}</tbody>
      </table>
    );
  }
  return (
    <div className="panel-body">
      <div className="collapse show" id={`collapseExample-${props.currId}`}>
        <div className="table">{row}</div>
        {isOpen ? (
          <PopBoxContentAdd
            ClickCancel={CreateCancel}
            SiteId={props.siteId}
            CurrencyId={props.currId}
            ClickSave={handleAdd}
            ReGetData={ReGetData}
          />
        ) : null}
      </div>
    </div>
  );
};

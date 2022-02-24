import React, { useState } from "react";

const BonusType = {
  1: "Retention Bonus",
  2: "Manual Bonus",
};

const Panel = (props) => {
  let BonusRadio = Object.keys(BonusType).map((key) => {
    return (
      <div
        key={`BonusOpt-${key}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`BonusType_${key}`}
          name="BonusTypeList"
          className="custom-control-input"
          value={key}
          onChange={(e) => {
            props.FilterOnChange(parseInt(key), "BonusType");
          }}
          disabled={false}
          checked={props.StateFilter.BonusType === parseInt(key)}
        />
        <label className="custom-control-label" htmlFor={`BonusType_${key}`}>
          {BonusType[key]}
        </label>
      </div>
    );
  });
  return (
    <>
      <div>
        <h4 className="page-name">Cust Info</h4>
        <table className="table table-detail">
          <tbody>
            <tr>
              <th scope="row">SiteName</th>
              <td>{props.CustData.siteName}</td>
            </tr>
            <tr>
              <th scope="row">User Name</th>
              <td>{props.CustData.userName}</td>
            </tr>
            <tr>
              <th scope="row">Currency</th>
              <td>{i18n["lbl_Currency" + props.CustData.currencyId]}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div className="row-flex">
        <div className="form-group">
          <label className="col-form-label d-block">Bonus Type</label>
          {BonusRadio}
        </div>
      </div>
      <div className="row-flex">
        <div className="form-group">
          <InputField
            columnTitle="BonusCode"
            Value={props.StateFilter.BonusCode}
            dataColumn="BonusCode"
            onChange={props.FilterOnChange}
          />
        </div>
      </div>
      <div className="row-flex">
        <div className="field">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.Check}
          >
            CheckBonusCode
          </button>
        </div>
      </div>
      {props.BonusData.CheckedBonusCode != "" ? (
        <>
          <div>
            <h4 className="page-name">
              {props.StateFilter.BonusType == 1
                ? "Retention Bonus Info"
                : "Manual Bonus Info"}
            </h4>
            <table className="table table-detail">
              <tbody>
                <tr>
                  <th scope="row">BonusCode</th>
                  <td>{props.BonusData.CheckedBonusCode}</td>
                </tr>
                <tr>
                  <th scope="row">WalletType</th>
                  <td>
                    {props.BonusData.WalletType == 0 ? "Transfer" : "Seamless"}
                  </td>
                </tr>
                <tr>
                  <th scope="row">ProductCategory</th>
                  <td>{props.BonusData.ProductCategory}</td>
                </tr>
              </tbody>
            </table>
          </div>
          <div className="row-flex">
            <div className="form-group">
              <InputField2
                columnTitle="Amount"
                Value={props.BonusData.Amount}
                dataColumn="Amount"
                onChange={props.BonusInfoOnchange}
                Disabled={props.StateFilter.BonusType == 1 ? true : false}
              />
            </div>
          </div>
          <div className="row-flex">
            <div className="form-group">
              <InputField2
                columnTitle="RollOver"
                Value={props.BonusData.RollOver}
                dataColumn="RollOver"
                onChange={props.BonusInfoOnchange}
                Disabled={true}
              />
            </div>
          </div>
          <div className="row-flex">
            <div className="field">
              <button
                className="btn btn-search"
                type="submit"
                onClick={props.Submit}
              >
                Submit
              </button>
            </div>
          </div>
        </>
      ) : null}
    </>
  );
};

const InputField = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <input
        className="form-control"
        value={props.Value}
        onChange={EditField}
        onBlur={EditField}
        style={{ width: `300px` }}
      />
    </div>
  );
};

const InputField2 = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <input
        className="form-control"
        value={props.Value}
        onChange={EditField}
        onBlur={EditField}
        disabled={props.Disabled}
        style={{ width: `200px` }}
      />
    </div>
  );
};

export default Panel;

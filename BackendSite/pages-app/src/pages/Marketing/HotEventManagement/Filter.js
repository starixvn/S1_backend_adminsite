import React, { useState } from "react";
import ReactDOM from "react-dom";

const defStatusType = {
  1: "Active",
  2: "Inactive",
};

const Panel = (props) => {
  let siteRows = props.Site.map((value) => {
    return (
      <div
        key={`SiteOpt-${value.siteId}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`SiteId_${value.siteId}`}
          name="SiteList"
          className="custom-control-input"
          value={value.siteId}
          onChange={props.ChangeFilter.bind(null, value.siteId, "SiteId")}
          checked={value.siteId === props.Filter.SiteId}
        />
        <label
          className="custom-control-label"
          htmlFor={`SiteId_${value.siteId}`}
        >
          {value.siteName}
        </label>
      </div>
    );
  });
  let currencyRows =
    props.Site.length === 0 || props.Filter.SiteId === 0
      ? null
      : props.Site.find(
          (x) => x.siteId === props.Filter.SiteId
        ).iCurrencyList.map((currency) => {
          return (
            <div
              key={`CurrencyOpt-${currency}`}
              className="custom-control custom-radio custom-control-inline"
            >
              <input
                type="radio"
                id={`Currency_${currency}`}
                name="CurrencyList"
                onChange={props.ChangeFilter.bind(null, currency, "CurrencyId")}
                className="custom-control-input"
                value={currency}
                checked={currency === props.Filter.CurrencyId}
              />
              <label
                className="custom-control-label"
                htmlFor={`Currency_${currency}`}
              >
                {i18n[`lbl_Currency${currency}`]}
              </label>
            </div>
          );
        });
  return (
    <>
      <div className="filter">
        <label className="col-form-label d-block">Available Sites</label>
        {siteRows}
        {props.Site.length === 0 || props.Filter.SiteId === 0 ? null : (
          <div className="table">
            <label className="col-form-label d-block">Available Currency</label>
            <div className="row-flex">{currencyRows}</div>
          </div>
        )}
        <div className="row-flex">
          <DropDownBox
            columnTitle="Status"
            options={Object.keys(defStatusType).map((key) => {
              return { value: key, displayText: defStatusType[key] };
            })}
            hasAll={true}
            selectedValue={props.Filter.Status}
            dataColumn="Status"
            onChange={props.ChangeFilter}
          />
        </div>
      </div>
    </>
  );
};
const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <div className="field">
        <select defaultValue={props.selectedValue} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};

export default Panel;

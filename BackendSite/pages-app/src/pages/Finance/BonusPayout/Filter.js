import React, { useState } from "react";

const Panel = (props) => {
  let radio = "";
  let _site = [];
  if (props.SiteList.length === 0) {
    return <></>;
  } else {
    radio = props.SiteList.map((value) => {
      _site[value.siteId] = value.siteName;
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
            onChange={(e) => {
              props.FilterOnChange(value.siteId, "SiteId");
            }}
            disabled={false}
            checked={props.StateFilter.SiteId === value.siteId}
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
  }
  return (
    <div className="filter">
      <div className="row-flex">
        <div className="form-group" style={{ marginTop: "20px" }}>
          <label className="col-form-label d-block">Site Name</label>
          {radio}
        </div>
      </div>
      <div className="row-flex">
        <div className="form-group">
          <InputField
            columnTitle="UserName"
            Value={props.StateFilter.UserName}
            dataColumn="UserName"
            onChange={props.FilterOnChange}
          />
        </div>
      </div>
      <div className="row-flex">
        <div className="form-group">
          <InputField
            columnTitle="SabaId"
            Value={props.StateFilter.SabaId}
            dataColumn="SabaId"
            onChange={props.FilterOnChange}
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
    </div>
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
      />
    </div>
  );
};

export default Panel;

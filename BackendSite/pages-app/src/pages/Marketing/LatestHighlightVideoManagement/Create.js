import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  let radio = "";
  let body = "";
  radio = props.SiteList.map((value) => {
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
          onChange={(e) => props.SetcreateSite(value.siteId)}
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
  if (props.LangDatas !== null) {
    body = props.LangDatas.map((data, idx) => {
      return (
        <div key={`LangInfo-${data.langId}`} className="panel mt-20">
          <a
            className="panel-title"
            data-toggle="collapse"
            style={{ color: "white" }}
          >
            {data.langId}
          </a>
          <div className="panel-body">
            <div className="row-flex">
              <InputField
                columnTitle="Title"
                Value={data.title}
                dataColumn="Title"
                defaultValue=""
                onChange={(e) => props.SetCreateData(e, "title", idx)}
                Disabled={false}
                Size={1000}
              />
            </div>
            <div className="row-flex">
              <InputField
                columnTitle="Link"
                Value={data.link}
                dataColumn="Link"
                defaultValue=""
                onChange={(e) => props.SetCreateData(e, "link", idx)}
                Disabled={false}
                Size={1000}
              />
            </div>
          </div>
        </div>
      );
    });
  }

  return (
    <div>
      <div className="filter">
        <label className="col-form-label d-block">Site Name</label>
        {radio}
        <div className="row-flex">
          <div>
            <DateTimePickerElem
              columnTitle="Date"
              dateTimePickerProps={{
                defaultValue: moment(props.CreateStatus.createDate),
                timeFormat: false,
                onBlur: (e) => {
                  try {
                    props.ChangeCreateDate(moment(e).format("l"), "Date");
                  } catch (ex) {}
                },
              }}
            />
          </div>
        </div>
      </div>
      <>{body}</>
      <div className="button-group">
        {props.CreateStatus.createSite !== 0 ? (
          <button
            className="btn btn-search"
            type="submit"
            style={{ marginTop: "20px", marginBottom: "40px" }}
            onClick={props.CreateSubmit}
          >
            Create
          </button>
        ) : null}

        <button
          className="btn btn-search"
          type="submit"
          style={{ marginTop: "20px", marginBottom: "40px" }}
          onClick={props.Back}
        >
          Back
        </button>
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
        disabled={props.Disabled}
        style={{ width: `${props.Size}px` }}
      />
    </div>
  );
};
const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};
export default Panel;

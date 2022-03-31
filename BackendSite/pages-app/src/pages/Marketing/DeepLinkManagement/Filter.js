import React, { useState } from "react";

const Panel = (props) => {
  let radio = "";
  let deepLinkListOptions = [];
  deepLinkListOptions.push({ value: 0, displayText: "" });
  if (props.site.length === 0) {
    return <></>;
  } else {
    radio = props.site.data.map((value) => {
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
            onChange={props.filterChange.bind(null, value.siteId,'SiteId')}
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
  if (props.deepLinkList.length !== 0) {
    // deepLinkListOptions = props.deepLinkList.map((i) => {
    //   return {
    //     value: Number(i.id),
    //     displayText: i.name,
    //   };
    // });
    props.deepLinkList.forEach((i) => {
      deepLinkListOptions.push({ value: Number(i.id), displayText: i.name });
    });
  }
  return (
    <>
      <div className="container-fluid">
        <div className="filter">
          <label className="col-form-label d-block">Site Name</label>
          <div className="row-flex">
            {radio}
          </div>
          <div className="row-flex">
            <DropDownBox
              columnTitle="DeepLinkType"
              options={deepLinkListOptions}
              hasAll={false}
              selectedValue={props.filterInfo.DeepLinkId}
              dataColumn="DeepLinkId"
              onChange={(e) => props.filterChange(e, "DeepLinkId")}
            />
          </div>
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

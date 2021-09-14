import React, { useState } from "react";

const Panel = (props) => {
  let radio = "";
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
            onChange={props.optSite.bind(null, value.siteId)}
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
    <>
      <div className="form-group">
        <label className="col-form-label d-block">Site Name</label>
        {radio}
      </div>
    </>
  );
};

export default Panel;

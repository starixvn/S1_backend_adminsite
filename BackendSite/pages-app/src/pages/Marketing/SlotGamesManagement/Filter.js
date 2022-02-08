import React from "react";

const defStatusType = {
    0: "All",
    1: "Open",
    2: "Closed"
  };

const Panel = (props) => {
  let siteRows = props.siteList.map((_data) => {
    return (
      <div
        key={`SiteOpt-${_data.siteId}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`SiteId_${_data.siteId}`}
          name="SiteList"
          className="custom-control-input"
          value={_data.siteId}
          onChange={props.SetSite.bind(null, _data.siteId)}
          checked={_data.siteId === props.Site}
        />
        <label
          className="custom-control-label"
          htmlFor={`SiteId_${_data.siteId}`}
        >
          {_data.siteName}
        </label>
      </div>
    );
  });
  return (
    <div className="filter">
      <label className="col-form-label d-block">Sites</label>
      {siteRows}
      <div className="row-flex">
        <DropDownBox
          columnTitle="Tp"
          options={GetTPOptions(props.StateTPList)}
          hasAll={true}
          dataColumn="TPid"
          onChange={props.FiterOnChange}
          value={props.StateFilter.TPid}
        />
        <DropDownBox
          columnTitle="Status"
          options={Object.keys(defStatusType).map((key) => {
            return { value: key, displayText: defStatusType[key] };
          })}
          hasAll={false}
          selectedValue={0}
          dataColumn="Status"
          onChange={props.FiterOnChange}
          value={props.StateFilter.Status}
        />
        <InputField
          columnTitle="Name"
          Value={props.StateFilter.Name}
          dataColumn="Name"
          onChange={props.FiterOnChange}
          defaultValue=""
        />
        <div className="row-flex">
          {props.StateTPList === null ? null : (
            <button
              className="btn btn-add"
              style={{ marginTop: "20px" }}
              onClick={props.Submit}
            >
              Submit
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

const GetTPOptions = (StateTPList) => {
  let TPOptions = [];
  // TPOptions.push({ value: 0, displayText: "----------------------------------------" });
  if (StateTPList != null) {
    StateTPList.forEach((x) => {
      TPOptions.push({ value: x.tpId, displayText: x.tpName });
    });
  }
  return TPOptions;
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
        <select onChange={onChange} value={props.value}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
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

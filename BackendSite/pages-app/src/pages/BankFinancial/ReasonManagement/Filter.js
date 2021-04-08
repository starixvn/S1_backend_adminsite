import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  let siteListOptions = [];
  if (props.SiteList) {
    siteListOptions = props.SiteList.map((i) => {
      return {
        value: Number(i.siteId),
        displayText: i.siteName,
      };
    });
  }
  return (
    <div className="filter">
      <div className="row-flex">
        {props.Funckey !== null ? (
          <DropDownBox
            columnTitle="Funckey"
            options={props.Funckey.map((x, index) => {
              return { value: x.funcKey, displayText: x.funcKey };
            })}
            hasAll={true}
            selectedValue={0}
            dataColumn="Funckey"
            onChange={props.onChange}
          />
        ) : null}
      </div>
      <div className="row-flex">
        <div className="button-group">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.ClickSubmit}
          >
            Submit
          </button>
        </div>
      </div>
    </div>
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

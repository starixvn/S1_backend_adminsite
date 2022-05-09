import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const defCurrencyType = {
  0: "",
  2: "MYR",
  4: "THB",
  13: "CNY",
  15: "IDR",
  51: "VND",
  61: "INR",
};
const defCategoryType = {
  0: "",
  1: "Hot",
  2: "Featured",
  3: "Status",
  4: "Jackpot",
};
const Panel = (props) => {
  let siteListOptions = [];
  if (props.SiteList) {
    siteListOptions.push({value:0,displayText:""})
    props.SiteList.forEach((i) => {
      siteListOptions.push({
        value: i.value,
        displayText: i.displayText,
      });
    });
  }
  return (
    <div className="filter">
      <div className="row-flex">
        <DropDownBox
          columnTitle="TP"
          options={GetTPOptions(props.StateTPList)}
          hasAll={false}
          selectedValue={0}
          dataColumn="TPid"
          onChange={props.FiterOnChange}
          value={props.StateFilter.TPid}
        />
        <DropDownBox
          columnTitle="Categories"
          options={Object.keys(defCategoryType).map((key) => {
            return { value: key, displayText: defCategoryType[key] };
          })}
          hasAll={false}
          selectedValue={0}
          dataColumn="Categories"
          onChange={props.FiterOnChange}
          value={props.StateFilter.Categories}
        />
        <DropDownBox
          columnTitle="Site"
          options={siteListOptions}
          hasAll={false}
          selectedValue={0}
          dataColumn="Site"
          onChange={props.FiterOnChange}
          value={props.StateFilter.Site}
        />
        <DropDownBox
          columnTitle="Currency"
          options={Object.keys(defCurrencyType).map((key) => {
            return { value: key, displayText: defCurrencyType[key] };
          })}
          hasAll={false}
          selectedValue={0}
          dataColumn="Currency"
          onChange={props.FiterOnChange}
          value={props.StateFilter.Currency}
        />
        <InputField
          columnTitle="Name"
          Value={props.StateFilter.Name}
          dataColumn="Name"
          onChange={props.FiterOnChange}
          defaultValue=""
        />
      </div>
      <div className="row-flex">
        {props.StateTPList === null ? null : (
          <button
            className="btn btn-add"
            style={{ marginTop: "20px"}}
            onClick={props.Submit}
          >
            Submit
          </button>
        )}
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

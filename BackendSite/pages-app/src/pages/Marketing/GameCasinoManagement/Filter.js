import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const defSite = {
  0: {
    value: 1,
    displayText: "Nova88",
  },
  1: {
    value: 2,
    displayText: "Goal123",
  },
  2: {
    value: 3,
    displayText: "Bwing",
  },
};
const defCurrency = {
  0: {
    value: 2,
    displayText: "MYR",
  },
  1: {
    value: 4,
    displayText: "THB",
  },
  2: {
    value: 13,
    displayText: "RMB",
  },
  3: {
    value: 15,
    displayText: "IDR",
  },
  4: {
    value: 51,
    displayText: "VND",
  },
  5: {
    value: 96,
    displayText: "USDT",
  },
};
const initSite = () => Object.values(defSite);
const initCurrency = () => Object.values(defCurrency);

const Panel = (props) => {
  return (
    <div className="filter">
      <div className="row-flex">
        <div>
          <RadioBox
            Title="Site"
            Options={initSite()}
            OnChange={props.FiterOnChange}
            dataColumn="Site"
            Selected={props.StateFilter.Site}
          />
        </div>
      </div>
      <div>
        <div>
          <RadioBox
            Title="Currency"
            Options={initCurrency()}
            OnChange={props.FiterOnChange}
            dataColumn="Currency"
            Selected={props.StateFilter.Currency}
          />
        </div>
      </div>
      <div>
        {props.StateTPList === null ? null : (
          <DropDownBox
            columnTitle="TP"
            options = {GetTPOptions(props.StateTPList)}
            hasAll={false}
            selectedValue={0}
            dataColumn="TPid"
            onChange={props.FiterOnChange}
            value={props.StateFilter.TPid}
          />
        )}
      </div>
      <div>
        {props.StateTPList === null ? null : (
          <button
            className="btn btn-add"
            style={{ marginTop: "20px", marginBottom: "40px" }}
            onClick={props.Submit}
          >
            Submit
          </button>
        )}
      </div>
    </div>
  );
};
const GetTPOptions=(StateTPList)=>{
  let TPOptions = [];
  TPOptions.push({ value: 0, displayText: "Option" });
  StateTPList.forEach(x=>{
    TPOptions.push({ value: x.tpId, displayText: x.tpName });
  });
  return TPOptions;
}

const GetPromoCategories = () => {};
const RadioBox = ({
  Title,
  Options,
  Selected,
  OnChange,
  dataColumn,
  mainKey = null,
}) => {
  let opt = Options.map((o, oIdx) => {
    let key = `${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`;
    return (
      <RadioBtn
        key={key}
        Id={key}
        DisplayText={o.displayText}
        Value={`${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        dataColumn={dataColumn}
        Name={`${Title}${mainKey === null ? "" : `-${mainKey}`}`}
        OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
        Checked={Selected === o.value}
      />
    );
  });
  return (
    <>
      <div className="row-flex">
        <div className="input-item">
          <label className="label">{Title}</label>
        </div>
      </div>
      <div className="row-flex">{opt}</div>
    </>
  );
};

const RadioBtn = ({
  Id,
  DisplayText,
  Value,
  Name,
  OnChange,
  Checked,
  Disabled,
}) => {
  return (
    <div className="custom-control custom-radio custom-control-inline">
      <input
        type="radio"
        id={Id}
        name={Name}
        className="custom-control-input revert-style"
        value={Value}
        onChange={OnChange}
        checked={Checked}
        disabled={Disabled}
      />
      <label className="custom-control-label" htmlFor={Id}>
        {DisplayText}
      </label>
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
        <select onChange={onChange} value={props.value}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};

export default Panel;

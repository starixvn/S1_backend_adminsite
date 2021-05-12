import React from "react";

const defCurrencyType = {
  2: "MYR",
  4: "THB",
  13: "RMB",
  15: "IDR",
  51: "VND",
  96: "USDT",
};

const Panel = (props) => {
  let CurrencyList = Object.keys(defCurrencyType).map((key) => {
    return {
      displayText: defCurrencyType[key],
      value: key,
    };
  });
  return (
    <div className="filter">
      <div className="row-flex">
        <DropDownBox
          columnTitle="Site"
          options={props.SiteList}
          hasAll={true}
          selectedValue={props.filterInfo.Site}
          dataColumn="Site"
          onChange={(e) => props.onChange(e, "Site")}
        />
        <DropDownBox
          columnTitle="Currency"
          options={CurrencyList}
          hasAll={true}
          selectedValue={props.filterInfo.Currency}
          dataColumn="Currency"
          onChange={(e) => props.onChange(e, "Currency")}
        />
        <DropDownBox
          columnTitle="BaseCurrency"
          options={CurrencyList}
          hasAll={false}
          selectedValue={props.filterInfo.BaseCurrency}
          dataColumn="BaseCurrency"
          onChange={(e) => props.onChange(e, "BaseCurrency")}
        />
        {props.Isloading === true ? (
          <div>
            <button
              type="button"
              className="btn btn-refresh"
              disabled={true}
              onClick={null}
            >
              Loading
            </button>
          </div>
        ) : (
          <div>
            <button
              type="button"
              className="btn btn-refresh"
              onClick={props.Refresh}
            >
              Refresh <span>{props.Count}</span>
            </button>
          </div>
        )}
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

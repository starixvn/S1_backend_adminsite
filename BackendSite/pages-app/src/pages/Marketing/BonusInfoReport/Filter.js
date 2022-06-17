import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

if (process.env.NODE_ENV !== "production") {
  window.gCurrencyList = [2, 3, 4, 13, 15, 51, 61, 74];
  window.i18n = {
    lbl_Currency2: "MYR",
    lbl_Currency3: "USD",
    lbl_Currency4: "THB",
    lbl_Currency13: "CNY",
    lbl_Currency15: "IDR",
    lbl_Currency51: "VND",
    lbl_Currency61: "INR",
    lbl_Currency74: "BDT",
  };
}

const defPromotionType = {
  1: "First Deposit",
  2: "Re-Deposit",
  3: "FreeBet",
  //4: "Special",
  5: "Wallet Bonus",
  6: "Rebate",
  7: "CashBack",
  8: "Manual Bonus(Static Page)",
  9: "Retention Bonus",
};
const defDateType = {
  0: "",
  1: "Today",
  2: "Yesterday",
  3: "This Week",
  4: "Last Week",
  5: "This Month",
  6: "Last Month",
};
const defCurrencyType = {
  0: "Original Currency",
  2: "MYR",
  3: "USD",
  4: "THB",
  13: "CNY",
  15: "IDR",
  51: "VND",
  61: "INR",
  74: "BDT",
};
const defCategoryType = {
  1: "Slots Game",
  2: "Live Casino",
  3: "Fish",
  4: "Saba Sports",
  5: "Lottery",
  6: "Sports",
};

const Panel = (props) => {
  let siteListOptions = "";
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
        {siteListOptions ? (
          <CheckBoxElem
            Title="Site"
            Options={siteListOptions}
            Selected={props.filterInfo.SelectSite}
            OnChange={props.onChangeCheckBox}
            OnChangeAll={props.onChangeAllCheckBox}
            dataColumn="SelectSite"
          />
        ) : null}
        <DropDownBox
          columnTitle="Promotion Type"
          options={Object.keys(defPromotionType).map((key) => {
            return { value: key, displayText: defPromotionType[key] };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="Type"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Currency"
          options={window.gCurrencyList.map((curr) => {
            return {
              displayText: window.i18n["lbl_Currency" + curr],
              value: curr,
            };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="Currency"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Base Currency Show"
          options={Object.keys(defCurrencyType).map((key) => {
            return { value: key, displayText: defCurrencyType[key] };
          })}
          hasAll={false}
          selectedValue={0}
          dataColumn="BaseCurrency"
          onChange={props.onChange}
        />
        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
            value: props.filterInfo.StartDate,
            defaultValue: props.filterInfo.StartDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.onChange(e, "StartDate");
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="To Date"
          dateTimePickerProps={{
            value: props.filterInfo.EndDate,
            defaultValue: props.filterInfo.EndDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.onChange(e, "EndDate");
              } catch (ex) {}
            },
          }}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="User"
          defaultValue=""
          dataColumn="User"
          onChange={props.onChange}
        />
        <DropDownBox2
          columnTitle="Date Quick Picker"
          options={Object.keys(defDateType).map((key) => {
            return { value: key, displayText: defDateType[key] };
          })}
          hasAll={false}
          value={props.filterInfo.DateQuickPicker}
          dataColumn="DateQuickPicker"
          onChange={props.datePickeronChange}
        />
        <InputField
          columnTitle="TransID"
          defaultValue=""
          dataColumn="TransId"
          onChange={props.onChange}
        />
        <InputField
          columnTitle="BonusCode"
          defaultValue=""
          dataColumn="BonusCode"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Categories"
          options={Object.keys(defCategoryType).map((key) => {
            return { value: key, displayText: defCategoryType[key] };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="RebateCategories"
          onChange={props.onChange}
        />
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

const InputField = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <div className="field">
        <input
          type="text"
          defaultValue={props.defaultValue}
          onChange={onChange}
        />
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

const DropDownBox2 = (props) => {
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
        <select value={props.value} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};

const DateTimePickerElem = (props) => {
  console.log(props);
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};

const CheckBoxElem = ({
  Title,
  Options,
  Selected = [],
  OnChange,
  OnChangeAll,
  dataColumn,
  mainKey,
}) => {
  let opt = Options.map((o) => {
    return (
      <CheckBox
        key={`${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        Options={null}
        onChange={OnChange}
        Title={o.displayText}
        checked={Selected.includes(o.value)}
        Value={o.value}
        dataColumn={dataColumn}
      />
    );
  });
  return (
    <div className="input-item" style={{ width: "auto" }}>
      <label className="label">{Title}</label>
      <CheckBox
        onChange={OnChangeAll}
        Title="All"
        Options={Options}
        checked={Selected.length === Options.length && Selected.length > 0}
        Value="All"
        dataColumn={dataColumn}
      />
      {opt}
    </div>
  );
};

const CheckBox = (props) => {
  const OnChange = (e) => {
    props.onChange(props.Value, props.dataColumn, props.Options);
  };
  return (
    <label className="checkbox checkbox-checked">
      <input
        type="checkbox"
        style={{ width: "0" }}
        value={props.Value}
        checked={props.checked}
        onChange={OnChange}
      />
      <i className="icon icon-checkbox"></i>
      <span className="text">{props.Title}</span>
    </label>
  );
};

export default Panel;

import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";
import MultiSelect from "react-multi-select-component";

const defCurrencyType = {
  2: "MYR",
  3: "USD",
  4: "THB",
  13: "CNY",
  15: "IDR",
  51: "VND",
  61: "INR",
  74: "BDT",
};
const defDepositStatusType = {
  1: "Non deposit",
  2: "Deposited",
};

const defCallOutStatusType = {
  1: "Pending1st Call",
  2: "Pending2nd Call",
  3: "Pending3rd Call",
  4: "Done",
};

const defDormantUserType = {
  3: "3 day inactive",
  7: "1 week inactive",
  14: "2 week inactive",
};

const Panel = (props) => {
  let siteList = props.SiteList.map((x) => {
    return {
      label: x.siteName,
      value: x.siteId,
    };
  });
  let currencyList = Object.keys(defCurrencyType).map((key) => {
    return {
      label: defCurrencyType[key],
      value: key,
    };
  });
  let depositStatusList = Object.keys(defDepositStatusType).map((key) => {
    return {
      label: defDepositStatusType[key],
      value: key,
    };
  });
  let depositStatusList2 = Object.keys(defDepositStatusType).map((key) => {
    return {
      displayText: defDepositStatusType[key],
      value: key,
    };
  });
  let callOutStatusList = Object.keys(defCallOutStatusType).map((key) => {
    return {
      label: defCallOutStatusType[key],
      value: key,
    };
  });
  let callOutStatusList2 = Object.keys(defCallOutStatusType).map((key) => {
    return {
      displayText: defCallOutStatusType[key],
      value: key,
    };
  });
  let dormantUserList = Object.keys(defDormantUserType).map((key) => {
    return {
      label: defDormantUserType[key],
      value: key,
    };
  });
  let dormantUserList2 = Object.keys(defDormantUserType).map((key) => {
    return {
      displayText: defDormantUserType[key],
      value: key,
    };
  });
  const SiteIdsChange = (x) => {
    props.SetFilter({
      ...props.StateFilter,
      SiteIds: x,
    });
  };
  const CurrencyChange = (x) => {
    props.SetFilter({
      ...props.StateFilter,
      Currency: x,
    });
  };
  const DepositChange = (x) => {
    props.SetFilter({
      ...props.StateFilter,
      Deposit: x,
    });
    console.log(props.StateFilter);
  };
  const CallOutChange = (x) => {
    props.SetFilter({
      ...props.StateFilter,
      CallOut: x,
    });
  };
  const DormantUserChange = (x) => {
    props.SetFilter({
      ...props.StateFilter,
      DormantUser: x,
    });
  };

  return (
    <div className="filter">
      <div className="row-flex">
        <div className="input-item" style={{ width: "200px", zIndex: "999" }}>
          <span className="label">Site</span>
          <div className="field">
            <MultiSelect
              disableSearch={"true"}
              options={siteList}
              value={props.StateFilter.SiteIds}
              onChange={SiteIdsChange}
              labelledBy={"Select"}
              ItemRenderer={itemRenderer}
              //   overrideStrings={{"selectSomeItems":"aaaaaa","allItemsAreSelected":"bbbbbbb"}}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "999" }}>
          <div className="field">
            <DateTimePickerElem
              columnTitle="From Date"
              dateTimePickerProps={{
                defaultValue: moment(props.StateFilter.StartDate).format("l"),
                timeFormat: false,
                onBlur: (e) => {
                  try {
                    props.onChange(moment(e).format("l"), "StartDate");
                  } catch (ex) {}
                },
              }}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "999" }}>
          <div className="field">
            <DateTimePickerElem
              columnTitle="To Date"
              dateTimePickerProps={{
                defaultValue: moment(props.StateFilter.EndDate).format("l"),
                timeFormat: false,
                onBlur: (e) => {
                  try {
                    props.onChange(moment(e).format("l"), "EndDate");
                  } catch (ex) {}
                },
              }}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "999" }}>
          <span className="label">Currency</span>
          <div className="field">
            <MultiSelect
              disableSearch={"true"}
              options={currencyList}
              value={props.StateFilter.Currency}
              onChange={CurrencyChange}
              labelledBy={"Select"}
              ItemRenderer={itemRenderer}
            />
          </div>
        </div>
      </div>
      <div className="row-flex">
        <div className="input-item" style={{ width: "200px", zIndex: "998" }}>
          <div className="field">
            <DropDownBox
              columnTitle="Deposit Status"
              options={depositStatusList2}
              hasAll={true}
              selectedValue={0}
              dataColumn="Deposit Status"
              onChange={DepositChange}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "998" }}>
          <div className="field">
            <DropDownBox
              columnTitle="CallOut Status"
              options={callOutStatusList2}
              hasAll={true}
              selectedValue={0}
              dataColumn="CallOut Status"
              onChange={CallOutChange}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "998" }}>
          <div className="field">
            <DropDownBox
              columnTitle="Dormant User"
              options={dormantUserList2}
              hasAll={false}
              selectedValue={0}
              dataColumn="Dormant User"
              onChange={DormantUserChange}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "998" }}>
          <div className="button-group">
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Submit}
            >
              Submit
            </button>
          </div>
          (Adjusted to GMT+8)
        </div>
      </div>
    </div>
  );
};
const itemRenderer = ({ checked, option, onClick }) => {
  return (
    <div>
      <input
        style={{ height: "15px", width: "20px" }}
        id={option.label}
        type="checkbox"
        onChange={onClick}
        checked={checked}
      />
      <label htmlFor={option.label}>{option.label}</label>
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

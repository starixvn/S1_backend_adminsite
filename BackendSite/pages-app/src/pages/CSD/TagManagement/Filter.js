import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";
import CheckBox from "./Component/Checkbox";

const ShowStatusType = {
  1: "Show ALL",
};

const Panel = (props) => {
  let ShowStatusOptions = Object.keys(ShowStatusType).map((key) => {
    return {
      displayText: ShowStatusType[key],
      value: key,
    };
  });
  return (
    <div className="filter">
      <div className="row-flex">
        <CheckBox
          Options={ShowStatusOptions}
          Title=""
          OnChange={props.ShowAllCheckBox}
          dataColumn="showAll"
          Selected={props.ShowAll}
          mainKey={null}
        />
        <div className="row-flex">
          <div className="button-group">
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.ClickSubmit}
              style={{
                // marginTop: "20px",
                marginBottom: "5px",
                marginRight: "10px",
              }}
            >
              Submit
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Panel;

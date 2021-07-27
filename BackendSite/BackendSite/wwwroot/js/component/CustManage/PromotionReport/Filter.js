import DateTimePicker from "react-datetime";
import moment from "moment-timezone";
const Filter = (props) => {
  return (
    <form className="filter" style={{ display: "block" }}>
      <div className="row-flex">
        <DateTimePickerElem
          columnTitle="StartTime"
          dateTimePickerProps={{
            defaultValue: moment(props.DateRangeDatas.StartTime).format("l"),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.EditDateRangeFiled(
                  moment(e).format("l"),
                  "StartTime"
                );
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="EndTime"
          dateTimePickerProps={{
            defaultValue: moment(props.DateRangeDatas.EndTime).format("l"),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.EditDateRangeFiled(moment(e).format("l"), "EndTime");
              } catch (ex) {}
            },
          }}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="button"
            onClick={props.ClickSubmit}
          >
            Submit
          </button>
        </div>
        <div>(Adjusted to GMT+8)</div>
      </div>
    </form>
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
export default Filter;
